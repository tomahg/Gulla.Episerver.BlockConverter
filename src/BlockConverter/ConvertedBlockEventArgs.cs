using EPiServer.Core;
using EPiServer.DataAbstraction;

namespace Gulla.Episerver.BlockConverter;

public class ConvertedBlockEventArgs : EventArgs
{
    public ConvertedBlockEventArgs(
        ContentReference contentLink,
        BlockType fromBlockType,
        BlockType toBlockType,
        bool recursive)
    {
        ContentLink = contentLink;
        FromBlockType = fromBlockType;
        ToBlockType = toBlockType;
        Recursive = recursive;
    }

    public ContentReference ContentLink { get; }
    public BlockType FromBlockType { get; }
    public BlockType ToBlockType { get; }
    public bool Recursive { get; }
}
