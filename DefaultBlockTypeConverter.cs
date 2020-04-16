using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;

namespace Alloy.Business.ConvertBlocks
{
    [ServiceConfiguration]
    internal class DefaultBlockTypeConverter
    {
        private readonly ServiceAccessor<ConvertBlockTypeDb> _dbAccessor;
        private readonly IContentRepository _contentRepository;
        private readonly IContentCacheRemover _contentCacheRemover;
        private readonly LocalizationService _localizationService;
        private readonly IPropertyDefinitionRepository _propertyDefinitionRepository;
        private readonly IContentTypeRepository _contentTypeRepository;

        public event EventHandler<ConvertedBlockEventArgs> BlocksConverted;

        public DefaultBlockTypeConverter(
          ServiceAccessor<ConvertBlockTypeDb> dbAccessor,
          IContentRepository contentRepository,
          IContentCacheRemover contentCacheRemover,
          IPropertyDefinitionRepository propertyDefinitionRepository,
          IContentTypeRepository contentTypeRepository,
          LocalizationService localizationService)
        {
            _dbAccessor = dbAccessor;
            _contentRepository = contentRepository;
            _contentCacheRemover = contentCacheRemover;
            _localizationService = localizationService;
            _propertyDefinitionRepository = propertyDefinitionRepository;
            _contentTypeRepository = contentTypeRepository;
        }

        public string Convert(
          ContentReference contentLink,
          BlockType fromBlockType,
          BlockType toBlockType,
          List<KeyValuePair<int, int>> propertyTypeMap,
          bool recursive,
          bool isTest)
        {
            if (ContentReference.IsNullOrEmpty(contentLink))
                throw new EPiServerException("Must specify a valid block, cannot be empty");
            if (fromBlockType == null)
                throw new ArgumentNullException(nameof(fromBlockType));
            if (toBlockType == null)
                throw new ArgumentNullException(nameof(toBlockType));
            if (fromBlockType.Equals(toBlockType))
                throw new EPiServerException("Can not convert to same block type");
            if (propertyTypeMap == null)
                throw new ArgumentNullException(nameof(propertyTypeMap));
            StringBuilder stringBuilder = new StringBuilder();
            DataSet ds = _dbAccessor().Convert(contentLink.ID, fromBlockType.ID, toBlockType.ID, propertyTypeMap, recursive, isTest);
            stringBuilder.Append(GenerateLogMessage(ds, contentLink.ID, fromBlockType.ID, toBlockType.ID, recursive, isTest));
            if (!isTest)
            {
                stringBuilder.Append(RefreshPages(contentLink, recursive));
                BlocksConverted?.Invoke(this, new ConvertedBlockEventArgs(contentLink, fromBlockType, toBlockType, recursive));
            }
            return stringBuilder.ToString();
        }

        private StringBuilder RefreshPages(ContentReference root, bool recursive)
        {
            StringBuilder stringBuilder = new StringBuilder();
            _contentCacheRemover.Clear();
            stringBuilder.Append(_localizationService.GetString("/admin/convertblocktype/log/clearcacheall")).Append("\n");
            return stringBuilder;
        }

        private string GeneratePageLog(int pageLinkID)
        {
            return new StringBuilder(" '").Append(_contentRepository.Get<IContent>(new ContentReference(pageLinkID)).Name).Append("[").Append(pageLinkID).Append("]'  ").ToString();
        }

        private StringBuilder GeneratePropertyLog(int propertyID)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (propertyID.Equals("-1"))
                stringBuilder.Append("'PageName'");
            else
                stringBuilder.Append("'").Append(_propertyDefinitionRepository.Load(propertyID).Name).Append("'");
            return stringBuilder;
        }

        private StringBuilder GeneratePageTypeLog(int pageTypeId)
        {
            BlockType pageType = _contentTypeRepository.Load(pageTypeId) as BlockType;
            return new StringBuilder("'").Append(pageType.Name).Append("'(").Append(pageType.ID).Append(")' ");
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
                stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, _localizationService.GetString(isTest ? "/admin/convertblocktype/log/removepropertytest" : "/admin/convertblocktype/log/removeproperty") ?? string.Empty, -cntUpdated, GeneratePropertyLog(fromPropertyId)));
                stringBuilder.Append("\n");
            }
            else if (cntUpdated > 0)
            {
                stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, _localizationService.GetString(isTest ? "/admin/convertblocktype/log/updatepropertytest" : "/admin/convertblocktype/log/updateproperty") ?? string.Empty, cntUpdated, GeneratePropertyLog(fromPropertyId), GeneratePropertyLog(toPropertyId)));
                stringBuilder.Append("\n");
            }
            else
                stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, _localizationService.GetString("/admin/convertblocktype/log/noproperty") ?? string.Empty, GeneratePropertyLog(fromPropertyId))).Append("\n");
            return stringBuilder;
        }

        private StringBuilder GenerateConvertedPageLog(
          int toBlockTypeId,
          int fromBlockTypeId,
          int cntUpdate,
          bool isTest)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, _localizationService.GetString(isTest ? "/admin/convertblocktype/log/convertedpagetypetest" : "/admin/convertblocktype/log/convertedpagetype") ?? string.Empty, cntUpdate, GeneratePageTypeLog(fromBlockTypeId), GeneratePageTypeLog(toBlockTypeId)));
            stringBuilder.Append("\n");
            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, _localizationService.GetString(isTest ? "/admin/convertblocktype/log/cntpagestest" : "/admin/convertblocktype/log/cntpages") ?? string.Empty, cntUpdate)).Append("\n");
            return stringBuilder;
        }

        private StringBuilder GenerateLogMessage(
          DataSet ds,
          int pageLinkId,
          int fromBlockTypeId,
          int toBlockTypeId,
          bool recursive,
          bool isTest)
        {
            StringBuilder ret = new StringBuilder();
            if (isTest)
            {
                if (recursive)
                    AddLogMessageIfNotNull("/admin/convertblocktype/log/headingrecursivetest", ret, pageLinkId);
                else
                    AddLogMessageIfNotNull("/admin/convertblocktype/log/headingtest", ret, pageLinkId);
            }
            else if (recursive)
                AddLogMessageIfNotNull("/admin/convertblocktype/log/headingrecursive", ret, pageLinkId);
            else
                AddLogMessageIfNotNull("/admin/convertblocktype/log/heading", ret, pageLinkId);
            ret.Append("\n\n");
            foreach (DataRow row in (InternalDataCollectionBase)ds.Tables["Properties"].Rows)
                ret.Append(GenerateConvertedPropertyLog(int.Parse((string)row["FromPropertyID"]), int.Parse((string)row["ToPropertyID"]), int.Parse((string)row["Count"], CultureInfo.InvariantCulture), isTest));
            ret.Append("\n");
            ret.Append(GenerateConvertedPageLog(toBlockTypeId, fromBlockTypeId, int.Parse((string)ds.Tables["Pages"].Rows[0][0], CultureInfo.InvariantCulture), isTest));
            if (!isTest)
                ret.Append(_localizationService.GetString("/admin/convertblocktype/log/commit")).Append("\n");
            return ret;
        }

        private void AddLogMessageIfNotNull(string languageKey, StringBuilder ret, int pageLinkId)
        {
            string format = _localizationService.GetString(languageKey);
            if (format == null)
                return;
            ret.Append(string.Format(format, GeneratePageLog(pageLinkId)));
        }
    }
}
