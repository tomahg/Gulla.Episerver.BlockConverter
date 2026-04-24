using EPiServer.DataAbstraction;

namespace Gulla.Episerver.BlockConverter.Models;

public class ConvertBlocksViewModel
{
    public List<ContentType> BlockTypes { get; set; } = new();
    public string ConversionMode { get; set; } = "single";

    // Single-block mode
    public int? SingleBlockId { get; set; }
    public string? SingleBlockName { get; set; }
    public int? SingleBlockTypeId { get; set; }

    // All-blocks-of-type mode
    public int FromBlockTypeId { get; set; }

    public int ToBlockTypeId { get; set; }
}

public class PropertyMappingsViewModel
{
    public int FromBlockTypeId { get; set; }
    public int ToBlockTypeId { get; set; }
    public List<PropertyMappingRow> Rows { get; set; } = new();
}

public class PropertyMappingRow
{
    public int FromPropertyId { get; set; }
    public string FromPropertyName { get; set; } = string.Empty;
    public List<PropertyMappingOption> Options { get; set; } = new();
    public int SelectedToPropertyId { get; set; }
}

public class PropertyMappingOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ConvertRequest
{
    public string ConversionMode { get; set; } = "single";
    public int? SingleBlockId { get; set; }
    public int FromBlockTypeId { get; set; }
    public int ToBlockTypeId { get; set; }
    public bool IsTest { get; set; }
    public List<PropertyMapping> PropertyMappings { get; set; } = new();
}

public class PropertyMapping
{
    public int FromId { get; set; }
    public int ToId { get; set; }
}
