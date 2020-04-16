// Decompiled with JetBrains decompiler
// Type: EPiServer.DataAccess.Internal.ConvertPageTypeDB
// Assembly: EPiServer, Version=11.12.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7
// MVID: DD7755C1-5804-4516-BC55-0FAD4D404A5A
// Assembly location: EPiServer.dll

using EPiServer.Core;
using EPiServer.Data;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace EPiServer.DataAccess.Internal
{
    /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice.</summary>
    /// <internal-api />
    /// <exclude />
    [ServiceConfiguration]
    public class ConvertPageTypeDB : DataAccessBase
    {
        private readonly IContentRepository _contentRepository;
        private readonly ILanguageBranchRepository _languageBranchRepository;
        private readonly IPropertyDefinitionRepository _propertyDefinitionRepository;

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice. Initializes a new instance of the <see cref="T:EPiServer.DataAccess.Internal.ConvertPageTypeDB" /> class.
        /// </summary>
        /// <exclude />
        public ConvertPageTypeDB(
          IDatabaseExecutor databaseHandler,
          IContentRepository contentRepository,
          ILanguageBranchRepository languageBranchRepository,
          IPropertyDefinitionRepository propertyDefinitionRepository)
          : base(databaseHandler)
        {
            this._contentRepository = contentRepository;
            this._languageBranchRepository = languageBranchRepository;
            this._propertyDefinitionRepository = propertyDefinitionRepository;
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice. Convert a page to a new page type
        /// This member supports the EPiServer infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="pageLinkId">The id to the page which will be converted</param>
        /// <param name="fromPageTypeId">The id of the page type to convert from</param>
        /// <param name="toPageTypeId">To id of the page type to convert to</param>
        /// <param name="propertyTypeMap">"from"-"to" mappings of properties
        /// , the mapped properties has to be on the same base form</param>
        /// <param name="recursive">if set to <c>true</c> the conversion will be performed for all subpages also</param>
        /// <param name="isTest">if set to <c>true</c> no actual conversion is made but a test to see the effect of the conversion</param>
        /// <returns>A dataset with information of changes</returns>
        /// <exclude />
        public virtual DataSet Convert(
          int pageLinkId,
          int fromPageTypeId,
          int toPageTypeId,
          List<KeyValuePair<int, int>> propertyTypeMap,
          bool recursive,
          bool isTest)
        {
            return this.Executor.ExecuteTransaction<DataSet>((Func<DataSet>)(() => new DataSet()
            {
                Locale = CultureInfo.InvariantCulture,
                Tables = {
          this.ConvertPageTypeProperties(pageLinkId, fromPageTypeId, propertyTypeMap, recursive, isTest),
          this.ConvertPageType(pageLinkId, fromPageTypeId, toPageTypeId, recursive, isTest)
        }
            }));
        }

        private DataTable ConvertPageTypeProperties(
          int pageLinkId,
          int fromPageTypeId,
          List<KeyValuePair<int, int>> propertyTypeMap,
          bool recursive,
          bool isTest)
        {
            DataTable dataTable = new DataTable("Properties");
            dataTable.Locale = CultureInfo.InvariantCulture;
            dataTable.Columns.Add("FromPropertyID");
            dataTable.Columns.Add("ToPropertyID");
            dataTable.Columns.Add("Count");
            int id = this._languageBranchRepository.Load(this._contentRepository.Get<PageData>(new ContentReference(pageLinkId)).MasterLanguage).ID;
            foreach (KeyValuePair<int, int> propertyType in propertyTypeMap)
            {
                DbCommand command = this.CreateCommand("netConvertPropertyForPageType");
                command.Parameters.Add((object)this.CreateReturnParameter());
                command.Parameters.Add((object)this.CreateParameter("PageID", (object)pageLinkId));
                command.Parameters.Add((object)this.CreateParameter("FromPageType", (object)fromPageTypeId));
                command.Parameters.Add((object)this.CreateParameter("FromPropertyID", (object)propertyType.Key));
                command.Parameters.Add((object)this.CreateParameter("ToPropertyID", (object)propertyType.Value));
                command.Parameters.Add((object)this.CreateParameter("Recursive", (object)recursive));
                command.Parameters.Add((object)this.CreateParameter("MasterLanguageID", (object)id));
                command.Parameters.Add((object)this.CreateParameter("IsTest", (object)isTest));
                command.ExecuteNonQuery();
                DataRow row = dataTable.NewRow();
                row[0] = (object)propertyType.Key;
                row[1] = (object)propertyType.Value;
                row[2] = (object)this.GetReturnValue(command);
                dataTable.Rows.Add(row);
                if (this._propertyDefinitionRepository.Load(propertyType.Key).Type.DataType == PropertyDataType.Category)
                {
                    command.CommandText = "netConvertCategoryPropertyForPageType";
                    command.ExecuteNonQuery();
                }
            }
            return dataTable;
        }

        private DataTable ConvertPageType(
          int pageLinkId,
          int fromPageTypeId,
          int toPageTypeId,
          bool recursive,
          bool isTest)
        {
            DataTable dataTable = new DataTable("Pages");
            dataTable.Locale = CultureInfo.InvariantCulture;
            dataTable.Columns.Add("Count");
            DbCommand command = this.CreateCommand("netConvertPageType");
            command.Parameters.Add((object)this.CreateReturnParameter());
            command.Parameters.Add((object)this.CreateParameter("PageID", (object)pageLinkId));
            command.Parameters.Add((object)this.CreateParameter("FromPageType", (object)fromPageTypeId));
            command.Parameters.Add((object)this.CreateParameter("ToPageType", (object)toPageTypeId));
            command.Parameters.Add((object)this.CreateParameter("Recursive", (object)recursive));
            command.Parameters.Add((object)this.CreateParameter("IsTest", (object)isTest));
            command.ExecuteNonQuery();
            DataRow row = dataTable.NewRow();
            row["Count"] = (object)this.GetReturnValue(command);
            dataTable.Rows.Add(row);
            return dataTable;
        }
    }
}
