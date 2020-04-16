// Decompiled with JetBrains decompiler
// Type: EPiServer.Core.Internal.DefaultPageTypeConverter
// Assembly: EPiServer, Version=11.12.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7
// MVID: DD7755C1-5804-4516-BC55-0FAD4D404A5A
// Assembly location: EPiServer.dll

using EPiServer.DataAbstraction;
using EPiServer.DataAccess.Internal;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace EPiServer.Core.Internal
{
    [ServiceConfiguration]
    internal class DefaultPageTypeConverter
    {
        private readonly ServiceAccessor<ConvertPageTypeDB> _dbAccessor;
        private readonly IContentRepository _contentRepository;
        private readonly IContentCacheRemover _contentCacheRemover;
        private readonly IPermanentLinkMapper _permanentLinkMapper;
        private readonly LocalizationService _localizationService;
        private readonly IPropertyDefinitionRepository _propertyDefinitionRepository;
        private readonly IContentTypeRepository _contentTypeRepository;

        public event EventHandler<ConvertedPageEventArgs> PagesConverted;

        public DefaultPageTypeConverter(
          ServiceAccessor<ConvertPageTypeDB> dbAccessor,
          IContentRepository contentRepository,
          IContentCacheRemover contentCacheRemover,
          IPermanentLinkMapper permanentLinkMapper,
          IPropertyDefinitionRepository propertyDefinitionRepository,
          IContentTypeRepository contentTypeRepository,
          LocalizationService localizationService)
        {
            this._dbAccessor = dbAccessor;
            this._contentRepository = contentRepository;
            this._contentCacheRemover = contentCacheRemover;
            this._permanentLinkMapper = permanentLinkMapper;
            this._localizationService = localizationService;
            this._propertyDefinitionRepository = propertyDefinitionRepository;
            this._contentTypeRepository = contentTypeRepository;
        }

        public string Convert(
          PageReference pageLink,
          PageType fromPageType,
          PageType toPageType,
          List<KeyValuePair<int, int>> propertyTypeMap,
          bool recursive,
          bool isTest)
        {
            if (PageReference.IsNullOrEmpty(pageLink))
                throw new EPiServerException("Must specify a valid page, cannot be empty");
            if ((ContentType)fromPageType == (ContentType)null)
                throw new ArgumentNullException(nameof(fromPageType));
            if ((ContentType)toPageType == (ContentType)null)
                throw new ArgumentNullException(nameof(toPageType));
            if (fromPageType.Equals((object)toPageType))
                throw new EPiServerException("Can not convert to same page type");
            if (propertyTypeMap == null)
                throw new ArgumentNullException(nameof(propertyTypeMap));
            StringBuilder stringBuilder = new StringBuilder();
            DataSet ds = this._dbAccessor().Convert(pageLink.ID, fromPageType.ID, toPageType.ID, propertyTypeMap, recursive, isTest);
            stringBuilder.Append((object)this.GenerateLogMessage(ds, pageLink.ID, fromPageType.ID, toPageType.ID, recursive, isTest));
            if (!isTest)
            {
                stringBuilder.Append((object)this.RefreshPages(pageLink, recursive));
                EventHandler<ConvertedPageEventArgs> pagesConverted = this.PagesConverted;
                if (pagesConverted != null)
                    pagesConverted((object)this, new ConvertedPageEventArgs(pageLink, fromPageType, toPageType, recursive));
            }
            return stringBuilder.ToString();
        }

        private StringBuilder RefreshPages(PageReference root, bool recursive)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (recursive)
            {
                this._contentCacheRemover.Clear();
                this._permanentLinkMapper.Clear();
                stringBuilder.Append(this._localizationService.GetString("/admin/convertpagetype/log/clearcacheall")).Append("\n");
            }
            else
            {
                this._contentCacheRemover.Remove((ContentReference)root);
                this._permanentLinkMapper.Clear();
                stringBuilder.Append(string.Format((IFormatProvider)CultureInfo.InvariantCulture, this._localizationService.GetString("/admin/convertpagetype/log/clearcache"), (object)root)).Append("\n");
            }
            return stringBuilder;
        }

        private string GeneratePageLog(int pageLinkID)
        {
            return new StringBuilder(" '").Append(this._contentRepository.Get<IContent>(new ContentReference(pageLinkID)).Name).Append("[").Append(pageLinkID).Append("]'  ").ToString();
        }

        private StringBuilder GeneratePropertyLog(int propertyID)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (propertyID.Equals((object)"-1"))
                stringBuilder.Append("'PageName'");
            else
                stringBuilder.Append("'").Append(this._propertyDefinitionRepository.Load(propertyID).Name).Append("'");
            return stringBuilder;
        }

        private StringBuilder GeneratePageTypeLog(int pageTypeId)
        {
            PageType pageType = this._contentTypeRepository.Load(pageTypeId) as PageType;
            return new StringBuilder("'").Append(pageType.Name).Append("'(").Append(pageType.ID).Append(")' ");
        }

        private static StringBuilder GenerateLanguageLog(string languageID)
        {
            return new StringBuilder(" '").Append(languageID).Append("' ");
        }

        private StringBuilder GenerateConvertedPropertyLog(
          int fromPropertyId,
          int toPropertyId,
          int cntUpdated,
          bool isTest)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (cntUpdated < 0)
            {
                stringBuilder.Append(string.Format((IFormatProvider)CultureInfo.InvariantCulture, this._localizationService.GetString(isTest ? "/admin/convertpagetype/log/removepropertytest" : "/admin/convertpagetype/log/removeproperty") ?? string.Empty, (object)-cntUpdated, (object)this.GeneratePropertyLog(fromPropertyId)));
                stringBuilder.Append("\n");
            }
            else if (cntUpdated > 0)
            {
                stringBuilder.Append(string.Format((IFormatProvider)CultureInfo.InvariantCulture, this._localizationService.GetString(isTest ? "/admin/convertpagetype/log/updatepropertytest" : "/admin/convertpagetype/log/updateproperty") ?? string.Empty, (object)cntUpdated, (object)this.GeneratePropertyLog(fromPropertyId), (object)this.GeneratePropertyLog(toPropertyId)));
                stringBuilder.Append("\n");
            }
            else
                stringBuilder.Append(string.Format((IFormatProvider)CultureInfo.InvariantCulture, this._localizationService.GetString("/admin/convertpagetype/log/noproperty") ?? string.Empty, (object)this.GeneratePropertyLog(fromPropertyId))).Append("\n");
            return stringBuilder;
        }

        private StringBuilder GenerateConvertedPageLog(
          int toPageTypeID,
          int fromPageTypeID,
          int cntUpdate,
          bool isTest)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format((IFormatProvider)CultureInfo.InvariantCulture, this._localizationService.GetString(isTest ? "/admin/convertpagetype/log/convertedpagetypetest" : "/admin/convertpagetype/log/convertedpagetype") ?? string.Empty, (object)cntUpdate, (object)this.GeneratePageTypeLog(fromPageTypeID), (object)this.GeneratePageTypeLog(toPageTypeID)));
            stringBuilder.Append("\n");
            stringBuilder.Append(string.Format((IFormatProvider)CultureInfo.InvariantCulture, this._localizationService.GetString(isTest ? "/admin/convertpagetype/log/cntpagestest" : "/admin/convertpagetype/log/cntpages") ?? string.Empty, (object)cntUpdate)).Append("\n");
            return stringBuilder;
        }

        private StringBuilder GenerateLogMessage(
          DataSet ds,
          int pageLinkId,
          int fromPageTypeId,
          int toPageTypeId,
          bool recursive,
          bool isTest)
        {
            StringBuilder ret = new StringBuilder();
            if (isTest)
            {
                if (recursive)
                    this.AddLogMessageIfNotNull("/admin/convertpagetype/log/headingrecursivetest", ret, pageLinkId);
                else
                    this.AddLogMessageIfNotNull("/admin/convertpagetype/log/headingtest", ret, pageLinkId);
            }
            else if (recursive)
                this.AddLogMessageIfNotNull("/admin/convertpagetype/log/headingrecursive", ret, pageLinkId);
            else
                this.AddLogMessageIfNotNull("/admin/convertpagetype/log/heading", ret, pageLinkId);
            ret.Append("\n\n");
            foreach (DataRow row in (InternalDataCollectionBase)ds.Tables["Properties"].Rows)
                ret.Append((object)this.GenerateConvertedPropertyLog(int.Parse((string)row["FromPropertyID"]), int.Parse((string)row["ToPropertyID"]), int.Parse((string)row["Count"], (IFormatProvider)CultureInfo.InvariantCulture), isTest));
            ret.Append("\n");
            ret.Append((object)this.GenerateConvertedPageLog(toPageTypeId, fromPageTypeId, int.Parse((string)ds.Tables["Pages"].Rows[0][0], (IFormatProvider)CultureInfo.InvariantCulture), isTest));
            if (!isTest)
                ret.Append(this._localizationService.GetString("/admin/convertpagetype/log/commit")).Append("\n");
            return ret;
        }

        private void AddLogMessageIfNotNull(string languageKey, StringBuilder ret, int pageLinkId)
        {
            string format = this._localizationService.GetString(languageKey);
            if (format == null)
                return;
            ret.Append(string.Format(format, (object)this.GeneratePageLog(pageLinkId)));
        }
    }
}
