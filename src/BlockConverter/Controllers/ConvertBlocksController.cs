using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using Gulla.Episerver.BlockConverter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulla.Episerver.BlockConverter.Controllers;

[Route("convertblocks")]
[Authorize(Roles = "Administrators, WebAdmins")]
public class ConvertBlocksController : Controller
{
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly IContentRepository _contentRepository;
    private readonly DefaultBlockTypeConverter _converter;

    public ConvertBlocksController(
        IContentTypeRepository contentTypeRepository,
        IContentRepository contentRepository,
        DefaultBlockTypeConverter converter)
    {
        _contentTypeRepository = contentTypeRepository;
        _contentRepository = contentRepository;
        _converter = converter;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var model = new ConvertBlocksViewModel
        {
            BlockTypes = GetBlockTypes()
        };
        return View(model);
    }

    [HttpGet("blockinfo")]
    public IActionResult BlockInfo(int id)
    {
        try
        {
            var content = _contentRepository.Get<IContent>(new ContentReference(id));
            var blockType = _contentTypeRepository.Load(content.ContentTypeID) as BlockType;
            if (blockType == null)
                return BadRequest(new { error = "Selected content is not a block." });

            return Json(new { name = content.Name, typeId = blockType.ID, typeName = blockType.Name });
        }
        catch
        {
            return BadRequest(new { error = $"Could not find block with id {id}." });
        }
    }

    [HttpGet("properties")]
    public IActionResult Properties(int fromTypeId, int toTypeId)
    {
        var fromType = _contentTypeRepository.Load(fromTypeId) as BlockType;
        var toType = _contentTypeRepository.Load(toTypeId) as BlockType;

        if (fromType == null || toType == null)
            return BadRequest();

        var model = BuildPropertyMappingsViewModel(fromType, toType);
        return PartialView("_PropertyMappings", model);
    }

    [HttpPost("convert")]
    [IgnoreAntiforgeryToken]
    public IActionResult Convert([FromBody] ConvertRequest request)
    {
        try
        {
            ContentReference startRef;
            BlockType fromType;

            if (request.ConversionMode == "single")
            {
                if (request.SingleBlockId == null)
                    return BadRequest(new { error = "No block selected." });

                var content = _contentRepository.Get<IContent>(new ContentReference(request.SingleBlockId.Value));
                fromType = (_contentTypeRepository.Load(content.ContentTypeID) as BlockType)!;
                if (fromType == null)
                    return BadRequest(new { error = "Selected content is not a block." });
                startRef = content.ContentLink;
            }
            else
            {
                fromType = (_contentTypeRepository.Load(request.FromBlockTypeId) as BlockType)!;
                if (fromType == null)
                    return BadRequest(new { error = "Invalid from block type." });
                startRef = ContentReference.RootPage;
            }

            var toType = _contentTypeRepository.Load(request.ToBlockTypeId) as BlockType;
            if (toType == null)
                return BadRequest(new { error = "Invalid to block type." });

            var mappings = request.PropertyMappings
                .Select(m => new KeyValuePair<int, int>(m.FromId, m.ToId))
                .ToList();

            bool recursive = request.ConversionMode != "single";
            string log = _converter.Convert(startRef, fromType, toType, mappings, recursive, request.IsTest);

            return Json(new { success = true, log });
        }
        catch (EPiServerException ex)
        {
            return Json(new { success = false, log = ex.Message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, log = ex.Message + "\n" + ex.StackTrace });
        }
    }

    private List<BlockType> GetBlockTypes()
        => _contentTypeRepository.List().OfType<BlockType>().OrderBy(t => t.Name).ToList();

    private PropertyMappingsViewModel BuildPropertyMappingsViewModel(BlockType fromType, BlockType toType)
    {
        var model = new PropertyMappingsViewModel
        {
            FromBlockTypeId = fromType.ID,
            ToBlockTypeId = toType.ID
        };

        foreach (var fromProp in fromType.PropertyDefinitions)
        {
            var row = new PropertyMappingRow
            {
                FromPropertyId = fromProp.ID,
                FromPropertyName = fromProp.Name
            };

            foreach (var toProp in toType.PropertyDefinitions)
            {
                if (toProp.Type.DataType != fromProp.Type.DataType)
                    continue;

                // For block-typed properties, source and target block type GUIDs must match
                if (fromProp.Type is BlockPropertyDefinitionType fromBlockPropType &&
                    toProp.Type is BlockPropertyDefinitionType toBlockPropType &&
                    fromBlockPropType.BlockType.GUID != toBlockPropType.BlockType.GUID)
                    continue;

                row.Options.Add(new PropertyMappingOption { Id = toProp.ID, Name = toProp.Name });

                if (row.SelectedToPropertyId == 0 &&
                    string.Equals(toProp.Name, fromProp.Name, StringComparison.OrdinalIgnoreCase))
                    row.SelectedToPropertyId = toProp.ID;
            }

            model.Rows.Add(row);
        }

        return model;
    }
}
