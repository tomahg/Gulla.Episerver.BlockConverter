using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace Alloy.Business.ConvertBlocks
{
    /// <summary>Converts page type for pages</summary>
    public static class BlockTypeConverter
    {
        /// <summary>Convert a page to a new page type</summary>
        /// <param name="contentLink">The link to the page which will be conversion will start</param>
        /// <param name="fromBlockType">The page type to convert from</param>
        /// <param name="toBlockType">Type page type to convert to</param>
        /// <param name="propertyTypeMap">"from"-"to" mappings of properties
        /// , the mapped properties has
        /// to be on the same base form</param>
        /// <param name="recursive">if set to <c>true</c> the conversion will be performed for all subpages as well</param>
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
            instance.PagesConverted += (EventHandler<ConvertedBlockEventArgs>)((o, e) =>
            {
                EventHandler<ConvertedBlockEventArgs> pagesConverted = BlockTypeConverter.BlocksConverted;
                if (pagesConverted == null)
                    return;
                pagesConverted(o, e);
            });
            return instance.Convert(contentLink, fromBlockType, toBlockType, propertyTypeMap, recursive, isTest);
        }

        /// <summary>
        /// Raised when pages are converted from one block type to another.
        /// </summary>
        public static event EventHandler<ConvertedBlockEventArgs> BlocksConverted;
    }
}
