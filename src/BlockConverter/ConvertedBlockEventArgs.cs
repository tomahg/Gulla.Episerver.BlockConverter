using EPiServer.Core;
using EPiServer.DataAbstraction;

namespace Gulla.Episerver.BlockConverter;

public class ConvertedBlockEventArgs : EventArgs
{
    public ConvertedBlockEventArgs(
        ContentReference contentLink,
        ContentType fromBlockType,
        ContentType toBlockType,
        bool recursive)
    {
        ContentLink = contentLink;
        FromBlockType = fromBlockType;
        ToBlockType = toBlockType;
        Recursive = recursive;
    }

    public ContentReference ContentLink { get; }
    public ContentType FromBlockType { get; }
    public ContentType ToBlockType { get; }
    public bool Recursive { get; }
}
