using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace Gulla.Episerver.BlockConverter
{
    /// <summary>Converts page type for pages</summary>
    public static class BlockTypeConverter
    {
        /// <summary>Convert a block to a new block type</summary>
        /// <param name="contentLink">The link to the block which will be conversion will start</param>
        /// <param name="fromBlockType">The block type to convert from</param>
        /// <param name="toBlockType">Type block type to convert to</param>
        /// <param name="propertyTypeMap">"from"-"to" mappings of properties
        /// , the mapped properties has
        /// to be on the same base form</param>
        /// <param name="recursive">if set to <c>true</c> the conversion will be performed for all child blocks as well</param>
        /// <param name="isTest">if set to <c>true</c> the conversion will not actually be performed bur rather a test run to see effect will be performed</param>
        /// <returns>
        /// </returns>
        public static string Convert(
          ContentReference contentLink,
          BlockType fromBlockType,
          BlockType toBlockType,
          List<KeyValuePair<int, int>> propertyTypeMap,
          bool recursive,
          bool isTest)
        {
            DefaultBlockTypeConverter instance = ServiceLocator.Current.GetInstance<DefaultBlockTypeConverter>();
            instance.BlocksConverted += (o, e) =>
            {
                var blocksConverted = BlocksConverted;
                if (blocksConverted == null)
                    return;
                blocksConverted(o, e);
            };
            return instance.Convert(contentLink, fromBlockType, toBlockType, propertyTypeMap, recursive, isTest);
        }

        /// <summary>
        /// Raised when pages are converted from one block type to another.
        /// </summary>
        public static event EventHandler<ConvertedBlockEventArgs> BlocksConverted;
    }
}
