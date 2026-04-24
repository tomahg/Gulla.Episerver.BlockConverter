using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace Gulla.Episerver.BlockConverter;

public static class BlockTypeConverter
{
    public static string Convert(
        ContentReference contentLink,
        ContentType fromBlockType,
        ContentType toBlockType,
        List<KeyValuePair<int, int>> propertyTypeMap,
        bool recursive,
        bool isTest)
    {
        var instance = ServiceLocator.Current.GetInstance<DefaultBlockTypeConverter>();
        instance.BlocksConverted += (o, e) => BlocksConverted?.Invoke(o, e);
        return instance.Convert(contentLink, fromBlockType, toBlockType, propertyTypeMap, recursive, isTest);
    }

    public static event EventHandler<ConvertedBlockEventArgs>? BlocksConverted;
}
