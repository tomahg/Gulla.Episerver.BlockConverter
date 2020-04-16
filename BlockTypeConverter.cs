// Decompiled with JetBrains decompiler
// Type: EPiServer.Core.PageTypeConverter
// Assembly: EPiServer, Version=11.12.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7
// MVID: DD7755C1-5804-4516-BC55-0FAD4D404A5A
// Assembly location: EPiServer.dll

using EPiServer.Core.Internal;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;

namespace EPiServer.Core
{
    /// <summary>Converts page type for pages</summary>
    public static class PageTypeConverter
    {
        /// <summary>Convert a page to a new page type</summary>
        /// <param name="pageLink">The link to the page which will be conversion will start</param>
        /// <param name="fromPageType">The page type to convert from</param>
        /// <param name="toPageType">Type page type to convert to</param>
        /// <param name="propertyTypeMap">"from"-"to" mappings of properties
        /// , the mapped properties has
        /// to be on the same base form</param>
        /// <param name="recursive">if set to <c>true</c> the conversion will be performed for all subpages as well</param>
        /// <param name="isTest">if set to <c>true</c> the conversion will not actually be performed bur rather a test run to see effect will be performed</param>
        /// <returns>
        /// </returns>
        public static string Convert(
          PageReference pageLink,
          PageType fromPageType,
          PageType toPageType,
          List<KeyValuePair<int, int>> propertyTypeMap,
          bool recursive,
          bool isTest)
        {
            DefaultPageTypeConverter instance = ServiceLocator.Current.GetInstance<DefaultPageTypeConverter>();
            instance.PagesConverted += (EventHandler<ConvertedPageEventArgs>)((o, e) =>
            {
                EventHandler<ConvertedPageEventArgs> pagesConverted = PageTypeConverter.PagesConverted;
                if (pagesConverted == null)
                    return;
                pagesConverted(o, e);
            });
            return instance.Convert(pageLink, fromPageType, toPageType, propertyTypeMap, recursive, isTest);
        }

        /// <summary>
        /// Raised when pages are converted from one pagetype to another.
        /// </summary>
        public static event EventHandler<ConvertedPageEventArgs> PagesConverted;

        /// <summary>Convert a page to a new page type</summary>
        /// <param name="pageLink">The link to the page which will be conversion will start</param>
        /// <param name="fromPageType">The page type to convert from</param>
        /// <param name="toPageType">Type page type to convert to</param>
        /// <param name="propertyTypeMap">"from"-"to" mappings of properties
        /// , the mapped properties has
        /// to be on the same base form</param>
        /// <param name="recursive">if set to <c>true</c> the conversion will be performed for all subpages as well</param>
        /// <param name="isTest">if set to <c>true</c> the conversion will not actually be performed bur rather a test run to see effect will be performed</param>
        /// <param name="contentRepository">The <see cref="T:EPiServer.IContentRepository" /> instance to work with</param>
        /// <returns>
        /// </returns>
        [Obsolete("Use the overload public static string Convert(PageReference pageLink, PageType fromPageType, PageType toPageType, List<KeyValuePair<int, int>> propertyTypeMap, bool recursive, bool isTest) instead.", false)]
        public static string Convert(
          PageReference pageLink,
          PageType fromPageType,
          PageType toPageType,
          List<KeyValuePair<int, int>> propertyTypeMap,
          bool recursive,
          bool isTest,
          IContentRepository contentRepository)
        {
            return PageTypeConverter.Convert(pageLink, fromPageType, toPageType, propertyTypeMap, recursive, isTest);
        }
    }
}
