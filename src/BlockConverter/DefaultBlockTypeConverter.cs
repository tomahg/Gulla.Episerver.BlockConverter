using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using System.Data;
using System.Globalization;
using System.Text;

namespace Gulla.Episerver.BlockConverter;

[ServiceConfiguration]
public class DefaultBlockTypeConverter
{
    private readonly ConvertBlockTypeDb _db;
    private readonly IContentRepository _contentRepository;
    private readonly IContentCacheRemover _contentCacheRemover;
    private readonly LocalizationService _localizationService;
    private readonly IPropertyDefinitionRepository _propertyDefinitionRepository;
    private readonly IContentTypeRepository _contentTypeRepository;

    public event EventHandler<ConvertedBlockEventArgs>? BlocksConverted;

    public DefaultBlockTypeConverter(
        ConvertBlockTypeDb db,
        IContentRepository contentRepository,
        IContentCacheRemover contentCacheRemover,
        IPropertyDefinitionRepository propertyDefinitionRepository,
        IContentTypeRepository contentTypeRepository,
        LocalizationService localizationService)
    {
        _db = db;
        _contentRepository = contentRepository;
        _contentCacheRemover = contentCacheRemover;
        _localizationService = localizationService;
        _propertyDefinitionRepository = propertyDefinitionRepository;
        _contentTypeRepository = contentTypeRepository;
    }

    public string Convert(
        ContentReference contentLink,
        ContentType fromBlockType,
        ContentType toBlockType,
        List<KeyValuePair<int, int>> propertyTypeMap,
        bool recursive,
        bool isTest)
    {
        if (ContentReference.IsNullOrEmpty(contentLink))
            throw new EPiServerException("Must specify a valid block, cannot be empty");
        if (fromBlockType == null)
            throw new ArgumentNullException(nameof(fromBlockType));
        if (toBlockType == null)
            throw new ArgumentNullException(nameof(toBlockType));
        if (fromBlockType.Equals(toBlockType))
            throw new EPiServerException("Can not convert to same block type");
        if (propertyTypeMap == null)
            throw new ArgumentNullException(nameof(propertyTypeMap));

        var sb = new StringBuilder();
        DataSet ds = _db.Convert(contentLink.ID, fromBlockType.ID, toBlockType.ID, propertyTypeMap, recursive, isTest);
        sb.Append(GenerateLogMessage(ds, contentLink.ID, fromBlockType.ID, toBlockType.ID, recursive, isTest));
        if (!isTest)
        {
            _contentCacheRemover.Clear();
            sb.Append(_localizationService.GetString("/admin/convertblocktype/log/clearcacheall")).Append("\n");
            BlocksConverted?.Invoke(this, new ConvertedBlockEventArgs(contentLink, fromBlockType, toBlockType, recursive));
        }
        return sb.ToString();
    }

    private string GenerateBlockLog(int pageLinkId)
        => $"'{_contentRepository.Get<IContent>(new ContentReference(pageLinkId)).Name}' ({pageLinkId})'";

    private string GeneratePropertyLog(int propertyId)
        => $"'{_propertyDefinitionRepository.Load(propertyId).Name}'";

    private string GenerateBlockTypeLog(int blockTypeId)
    {
        var type = _contentTypeRepository.Load(blockTypeId)!;
        return $"'{type.Name}' ({type.ID})";
    }

    private StringBuilder GenerateConvertedPropertyLog(int fromPropertyId, int toPropertyId, int cntUpdated, bool isTest)
    {
        var sb = new StringBuilder();
        if (cntUpdated < 0)
            sb.Append(string.Format(CultureInfo.InvariantCulture,
                _localizationService.GetString(isTest ? "/admin/convertblocktype/log/removepropertytest" : "/admin/convertblocktype/log/removeproperty") ?? string.Empty,
                -cntUpdated, GeneratePropertyLog(fromPropertyId)));
        else if (cntUpdated > 0)
            sb.Append(string.Format(CultureInfo.InvariantCulture,
                _localizationService.GetString(isTest ? "/admin/convertblocktype/log/updatepropertytest" : "/admin/convertblocktype/log/updateproperty") ?? string.Empty,
                cntUpdated, GeneratePropertyLog(fromPropertyId), GeneratePropertyLog(toPropertyId)));
        else
            sb.Append(string.Format(CultureInfo.InvariantCulture,
                _localizationService.GetString("/admin/convertblocktype/log/noproperty") ?? string.Empty,
                GeneratePropertyLog(fromPropertyId)));
        sb.Append("\n");
        return sb;
    }

    private StringBuilder GenerateConvertedBlockLog(int toBlockTypeId, int fromBlockTypeId, int cntUpdate, bool isTest)
    {
        var sb = new StringBuilder();
        sb.Append(string.Format(CultureInfo.InvariantCulture,
            _localizationService.GetString(isTest ? "/admin/convertblocktype/log/convertedpagetypetest" : "/admin/convertblocktype/log/convertedpagetype") ?? string.Empty,
            cntUpdate, GenerateBlockTypeLog(fromBlockTypeId), GenerateBlockTypeLog(toBlockTypeId)));
        sb.Append("\n");
        sb.Append(string.Format(CultureInfo.InvariantCulture,
            _localizationService.GetString(isTest ? "/admin/convertblocktype/log/cntpagestest" : "/admin/convertblocktype/log/cntpages") ?? string.Empty,
            cntUpdate));
        sb.Append("\n");
        return sb;
    }

    private StringBuilder GenerateLogMessage(DataSet ds, int blockId, int fromBlockTypeId, int toBlockTypeId, bool recursive, bool isTest)
    {
        var ret = new StringBuilder();

        string headingKey = (isTest, recursive) switch
        {
            (true, true)  => "/admin/convertblocktype/log/headingrecursivetest",
            (true, false) => "/admin/convertblocktype/log/headingtest",
            (false, true) => "/admin/convertblocktype/log/headingrecursive",
            _             => "/admin/convertblocktype/log/heading"
        };

        var headingFormat = _localizationService.GetString(headingKey);
        if (headingFormat != null)
        {
            if (recursive)
            {
                ret.Append(string.Format(headingFormat, GenerateBlockTypeLog(fromBlockTypeId)));
            }
            else
            {
                ret.Append(string.Format(headingFormat, GenerateBlockLog(blockId)));
            }
        }

        ret.Append("\n\n");

        foreach (DataRow row in ds.Tables["Properties"]!.Rows)
            ret.Append(GenerateConvertedPropertyLog(
                int.Parse((string)row["FromPropertyID"]),
                int.Parse((string)row["ToPropertyID"]),
                int.Parse((string)row["Count"], CultureInfo.InvariantCulture),
                isTest));

        ret.Append("\n");
        ret.Append(GenerateConvertedBlockLog(toBlockTypeId, fromBlockTypeId,
            int.Parse((string)ds.Tables["Pages"]!.Rows[0][0], CultureInfo.InvariantCulture), isTest));

        if (!isTest)
            ret.Append(_localizationService.GetString("/admin/convertblocktype/log/commit")).Append("\n");

        return ret;
    }
}
