// Decompiled with JetBrains decompiler
// Type: EPiServer.Core.ConvertedPageEventArgs
// Assembly: EPiServer, Version=11.12.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7
// MVID: DD7755C1-5804-4516-BC55-0FAD4D404A5A
// Assembly location: EPiServer.dll

using EPiServer.DataAbstraction;
using System;

namespace EPiServer.Core
{
    /// <summary>
    /// Event argument used in <see cref="T:EPiServer.Core.PageTypeConverter" /></summary>
    public class ConvertedPageEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="T:EPiServer.Core.ConvertedPageEventArgs" /></summary>
        public ConvertedPageEventArgs(
            PageReference pageLink,
            PageType fromPageType,
            PageType toPageType,
            bool recursive)
        {
            this.PageLink = pageLink;
            this.FromPageType = fromPageType;
            this.ToPageType = toPageType;
            this.Recursive = recursive;
        }

        /// <summary>The page which is converted</summary>
        public PageReference PageLink { get; }

        /// <summary>Specifies from which pagetype it is converted</summary>
        public PageType FromPageType { get; }

        /// <summary>Specifies to which pagetype it is converted</summary>
        public PageType ToPageType { get; }

        /// <summary>
        /// Specifies if the convert operation is recursive for descendents of same pagetype
        /// </summary>
        public bool Recursive { get; }
    }
}