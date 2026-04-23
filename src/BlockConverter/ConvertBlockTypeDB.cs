using EPiServer;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.ServiceLocation;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace Gulla.Episerver.BlockConverter;

[ServiceConfiguration]
public class ConvertBlockTypeDb : DataAccessBase
{
    private readonly IContentRepository _contentRepository;
    private readonly ILanguageBranchRepository _languageBranchRepository;
    private readonly IPropertyDefinitionRepository _propertyDefinitionRepository;

    public ConvertBlockTypeDb(
        IDatabaseExecutor databaseHandler,
        IContentRepository contentRepository,
        ILanguageBranchRepository languageBranchRepository,
        IPropertyDefinitionRepository propertyDefinitionRepository)
        : base(databaseHandler)
    {
        _contentRepository = contentRepository;
        _languageBranchRepository = languageBranchRepository;
        _propertyDefinitionRepository = propertyDefinitionRepository;
    }

    public virtual DataSet Convert(
        int blockLinkId,
        int fromBlockTypeId,
        int toBlockTypeId,
        List<KeyValuePair<int, int>> propertyTypeMap,
        bool recursive,
        bool isTest)
    {
        return Executor.ExecuteTransaction(() => new DataSet
        {
            Locale = CultureInfo.InvariantCulture,
            Tables =
            {
                ConvertPageTypeProperties(blockLinkId, fromBlockTypeId, propertyTypeMap, recursive, isTest),
                ConvertPageType(blockLinkId, fromBlockTypeId, toBlockTypeId, recursive, isTest)
            }
        });
    }

    private DataTable ConvertPageTypeProperties(
        int blockLinkId,
        int fromBlockTypeId,
        List<KeyValuePair<int, int>> propertyTypeMap,
        bool recursive,
        bool isTest)
    {
        var dataTable = new DataTable("Properties") { Locale = CultureInfo.InvariantCulture };
        dataTable.Columns.Add("FromPropertyID");
        dataTable.Columns.Add("ToPropertyID");
        dataTable.Columns.Add("Count");

        var content = (ILocalizable)_contentRepository.Get<IContent>(new ContentReference(blockLinkId));
        int languageBranchId = _languageBranchRepository.Load(content.MasterLanguage).ID;

        foreach (var propertyType in propertyTypeMap)
        {
            DbCommand command = CreateCommand("netConvertPropertyForPageType");
            command.Parameters.Add(CreateReturnParameter());
            command.Parameters.Add(CreateParameter("PageID", blockLinkId));
            command.Parameters.Add(CreateParameter("FromPageType", fromBlockTypeId));
            command.Parameters.Add(CreateParameter("FromPropertyID", propertyType.Key));
            command.Parameters.Add(CreateParameter("ToPropertyID", propertyType.Value));
            command.Parameters.Add(CreateParameter("Recursive", recursive));
            command.Parameters.Add(CreateParameter("MasterLanguageID", languageBranchId));
            command.Parameters.Add(CreateParameter("IsTest", isTest));
            command.ExecuteNonQuery();

            var row = dataTable.NewRow();
            row[0] = propertyType.Key;
            row[1] = propertyType.Value;
            row[2] = GetReturnValue(command);
            dataTable.Rows.Add(row);

            if (_propertyDefinitionRepository.Load(propertyType.Key).Type.DataType == PropertyDataType.Category)
            {
                command.CommandText = "netConvertCategoryPropertyForPageType";
                command.ExecuteNonQuery();
            }
        }

        return dataTable;
    }

    private DataTable ConvertPageType(
        int blockLinkId,
        int fromBlockTypeId,
        int toBlockTypeId,
        bool recursive,
        bool isTest)
    {
        var dataTable = new DataTable("Pages") { Locale = CultureInfo.InvariantCulture };
        dataTable.Columns.Add("Count");

        DbCommand command = CreateCommand("netConvertPageType");
        command.Parameters.Add(CreateReturnParameter());
        command.Parameters.Add(CreateParameter("PageID", blockLinkId));
        command.Parameters.Add(CreateParameter("FromPageType", fromBlockTypeId));
        command.Parameters.Add(CreateParameter("ToPageType", toBlockTypeId));
        command.Parameters.Add(CreateParameter("Recursive", recursive));
        command.Parameters.Add(CreateParameter("IsTest", isTest));
        command.ExecuteNonQuery();

        var row = dataTable.NewRow();
        row["Count"] = GetReturnValue(command);
        dataTable.Rows.Add(row);

        return dataTable;
    }
}
