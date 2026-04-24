using EPiServer;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Gulla.Episerver.BlockConverter;

// Mirrors EPiServer.DataAccess.Internal.ConvertPageTypeDB (CMS 13), reusing the same
// embedded SQL resources from EPiServer.dll so block conversion stays in sync with
// future Optimizely updates to the SQL.
[ServiceConfiguration]
public class ConvertBlockTypeDb
{
    private readonly IDatabaseExecutor _executor;
    private readonly IContentRepository _contentRepository;
    private readonly ILanguageBranchRepository _languageBranchRepository;
    private readonly IPropertyDefinitionRepository _propertyDefinitionRepository;

    private static readonly Lazy<IReadOnlyDictionary<string, string>> _sqlCache =
        new(LoadEpiServerSql, LazyThreadSafetyMode.PublicationOnly);

    public ConvertBlockTypeDb(
        IDatabaseExecutor executor,
        IContentRepository contentRepository,
        ILanguageBranchRepository languageBranchRepository,
        IPropertyDefinitionRepository propertyDefinitionRepository)
    {
        _executor = executor;
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
        int masterLanguageId = _languageBranchRepository
            .Load(((ILocalizable)_contentRepository.Get<IContent>(new ContentReference(blockLinkId))).MasterLanguage)
            .ID;

        return _executor.ExecuteTransaction(() => new DataSet
        {
            Locale = CultureInfo.InvariantCulture,
            Tables =
            {
                ConvertProperties(blockLinkId, fromBlockTypeId, masterLanguageId, propertyTypeMap, recursive, isTest),
                ConvertContentType(blockLinkId, fromBlockTypeId, toBlockTypeId, recursive, isTest)
            }
        });
    }

    private DataTable ConvertProperties(
        int blockLinkId,
        int fromBlockTypeId,
        int masterLanguageId,
        List<KeyValuePair<int, int>> propertyTypeMap,
        bool recursive,
        bool isTest)
    {
        var dataTable = new DataTable("Properties") { Locale = CultureInfo.InvariantCulture };
        dataTable.Columns.Add("FromPropertyID");
        dataTable.Columns.Add("ToPropertyID");
        dataTable.Columns.Add("Count");

        foreach (var propertyType in propertyTypeMap)
        {
            using var cmd = CreateCommand("netConvertPropertyForPageType");
            AddInput(cmd, "PageID", blockLinkId, DbType.Int32);
            AddInput(cmd, "FromPageType", fromBlockTypeId, DbType.Int32);
            AddInput(cmd, "FromPropertyID", propertyType.Key, DbType.Int32);
            AddInput(cmd, "ToPropertyID", propertyType.Value, DbType.Int32);
            AddInput(cmd, "Recursive", recursive, DbType.Boolean);
            AddInput(cmd, "MasterLanguageID", masterLanguageId, DbType.Int32);
            AddInput(cmd, "IsTest", isTest, DbType.Boolean);
            var countParam = AddOutput(cmd, "COUNT", DbType.Int32);
            cmd.ExecuteNonQuery();

            var row = dataTable.NewRow();
            row[0] = propertyType.Key;
            row[1] = propertyType.Value;
            row[2] = countParam.Value;
            dataTable.Rows.Add(row);

            if (_propertyDefinitionRepository.Load(propertyType.Key).Type.DataType == PropertyDataType.Category)
            {
                using var catCmd = CreateCommand("netConvertCategoryPropertyForPageType");
                AddInput(catCmd, "PageID", blockLinkId, DbType.Int32);
                AddInput(catCmd, "FromPageType", fromBlockTypeId, DbType.Int32);
                AddInput(catCmd, "FromPropertyID", propertyType.Key, DbType.Int32);
                AddInput(catCmd, "ToPropertyID", propertyType.Value, DbType.Int32);
                AddInput(catCmd, "Recursive", recursive, DbType.Boolean);
                AddInput(catCmd, "MasterLanguageID", masterLanguageId, DbType.Int32);
                catCmd.ExecuteNonQuery();
            }
        }

        return dataTable;
    }

    private DataTable ConvertContentType(
        int blockLinkId,
        int fromBlockTypeId,
        int toBlockTypeId,
        bool recursive,
        bool isTest)
    {
        var dataTable = new DataTable("Pages") { Locale = CultureInfo.InvariantCulture };
        dataTable.Columns.Add("Count");

        using var cmd = CreateCommand("netConvertPageType");
        AddInput(cmd, "PageID", blockLinkId, DbType.Int32);
        AddInput(cmd, "FromPageType", fromBlockTypeId, DbType.Int32);
        AddInput(cmd, "ToPageType", toBlockTypeId, DbType.Int32);
        AddInput(cmd, "Recursive", recursive, DbType.Boolean);
        AddInput(cmd, "IsTest", isTest, DbType.Boolean);
        var countParam = AddOutput(cmd, "COUNT", DbType.Int32);
        cmd.ExecuteNonQuery();

        var row = dataTable.NewRow();
        row["Count"] = countParam.Value;
        dataTable.Rows.Add(row);

        return dataTable;
    }

    private DbCommand CreateCommand(string sqlName)
    {
        if (!_sqlCache.Value.TryGetValue(sqlName, out var sql))
            throw new InvalidOperationException(
                $"Embedded SQL resource '{sqlName}' not found in EPiServer assembly. " +
                "Ensure EPiServer.CMS.Core 13.x is referenced.");

        var cmd = _executor.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = sql;
        return cmd;
    }

    private static void AddInput(DbCommand cmd, string name, object value, DbType dbType)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        p.DbType = dbType;
        cmd.Parameters.Add(p);
    }

    private static DbParameter AddOutput(DbCommand cmd, string name, DbType dbType)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Direction = ParameterDirection.Output;
        p.DbType = dbType;
        cmd.Parameters.Add(p);
        return p;
    }

    // Loads all *.sql embedded resources from the EPiServer assembly, keyed by the
    // bare filename (e.g. "netConvertPropertyForPageType") so CreateCommand can look
    // them up the same way EmbeddedSqlReader does internally.
    private static IReadOnlyDictionary<string, string> LoadEpiServerSql()
    {
        const string prefix = "EPiServer.DataAccess.EmbeddedSql.";
        const string suffix = ".sql";

        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "EPiServer");

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (assembly == null) return result;

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.StartsWith(prefix, StringComparison.Ordinal) ||
                !resourceName.EndsWith(suffix, StringComparison.Ordinal))
                continue;

            var key = resourceName.Substring(prefix.Length, resourceName.Length - prefix.Length - suffix.Length);
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            result[key] = reader.ReadToEnd();
        }

        return result;
    }
}
