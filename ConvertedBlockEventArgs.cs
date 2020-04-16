using System;
using EPiServer.Core;
using EPiServer.DataAbstraction;

namespace Alloy.Business.ConvertBlocks
{
    /// <summary>
    /// Event argument used in <see cref="T:EPiServer.Core.PageTypeConverter" /></summary>
    public class ConvertedBlockEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="T:TinyMCE.Business.ConvertBlocks.ConvertedPageEventArgs" /></summary>
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

        /// <summary>The page which is converted</summary>
        public ContentReference ContentLink { get; }

        /// <summary>Specifies from which block type it is converted</summary>
        public BlockType FromBlockType { get; }

        /// <summary>Specifies to which block type it is converted</summary>
        public BlockType ToBlockType { get; }

        /// <summary>
        /// Specifies if the convert operation is recursive for descendents of same block type
        /// </summary>
        public bool Recursive { get; }
    }
}