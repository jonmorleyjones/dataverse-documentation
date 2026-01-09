using System.Text;

namespace DataverseDoc.Dataverse;

/// <summary>
/// Fluent builder for constructing OData queries for the Dataverse Web API.
/// </summary>
public class ODataQueryBuilder
{
    private readonly string _entity;
    private readonly List<string> _selectColumns = new();
    private readonly List<string> _filters = new();
    private readonly List<string> _expands = new();
    private readonly List<string> _orderBy = new();
    private int? _top;

    /// <summary>
    /// Creates a new ODataQueryBuilder for the specified entity.
    /// </summary>
    /// <param name="entity">The entity logical name or path.</param>
    public ODataQueryBuilder(string entity)
    {
        if (string.IsNullOrWhiteSpace(entity))
        {
            throw new ArgumentException("Entity name is required.", nameof(entity));
        }
        _entity = entity;
    }

    /// <summary>
    /// Adds columns to the $select clause.
    /// </summary>
    public ODataQueryBuilder Select(params string[] columns)
    {
        _selectColumns.AddRange(columns);
        return this;
    }

    /// <summary>
    /// Adds a raw filter condition to the $filter clause.
    /// Multiple filters are combined with 'and'.
    /// </summary>
    public ODataQueryBuilder Filter(string condition)
    {
        if (!string.IsNullOrWhiteSpace(condition))
        {
            _filters.Add(condition);
        }
        return this;
    }

    /// <summary>
    /// Adds an equality filter condition.
    /// </summary>
    public ODataQueryBuilder FilterEqual(string column, object value)
    {
        var formattedValue = FormatValue(value);
        return Filter($"{column} eq {formattedValue}");
    }

    /// <summary>
    /// Adds a navigation property expansion with optional nested clauses.
    /// </summary>
    public ODataQueryBuilder Expand(
        string navigationProperty,
        string[]? select = null,
        string? filter = null)
    {
        var expandClause = new StringBuilder(navigationProperty);

        var nestedClauses = new List<string>();
        if (select != null && select.Length > 0)
        {
            nestedClauses.Add($"$select={string.Join(",", select)}");
        }
        if (!string.IsNullOrWhiteSpace(filter))
        {
            nestedClauses.Add($"$filter={filter}");
        }

        if (nestedClauses.Count > 0)
        {
            expandClause.Append($"({string.Join(";", nestedClauses)})");
        }

        _expands.Add(expandClause.ToString());
        return this;
    }

    /// <summary>
    /// Adds a column to the $orderby clause.
    /// </summary>
    public ODataQueryBuilder OrderBy(string column, bool descending = false)
    {
        var orderClause = descending ? $"{column} desc" : column;
        _orderBy.Add(orderClause);
        return this;
    }

    /// <summary>
    /// Sets the $top limit.
    /// </summary>
    public ODataQueryBuilder Top(int count)
    {
        _top = count;
        return this;
    }

    /// <summary>
    /// Builds the final OData query string.
    /// </summary>
    public string Build()
    {
        var clauses = new List<string>();

        if (_selectColumns.Count > 0)
        {
            clauses.Add($"$select={string.Join(",", _selectColumns)}");
        }

        if (_filters.Count > 0)
        {
            clauses.Add($"$filter={string.Join(" and ", _filters)}");
        }

        if (_expands.Count > 0)
        {
            clauses.Add($"$expand={string.Join(",", _expands)}");
        }

        if (_orderBy.Count > 0)
        {
            clauses.Add($"$orderby={string.Join(",", _orderBy)}");
        }

        if (_top.HasValue)
        {
            clauses.Add($"$top={_top}");
        }

        if (clauses.Count == 0)
        {
            return _entity;
        }

        return $"{_entity}?{string.Join("&", clauses)}";
    }

    /// <summary>
    /// Builds a query for solution components.
    /// </summary>
    /// <param name="solutionId">The solution ID.</param>
    /// <param name="componentType">The component type code.</param>
    /// <returns>The OData query string.</returns>
    public static string ForSolutionComponents(Guid solutionId, int componentType)
    {
        return new ODataQueryBuilder("solutioncomponents")
            .Select("objectid", "componenttype")
            .Filter($"_solutionid_value eq {solutionId}")
            .Filter($"componenttype eq {componentType}")
            .Build();
    }

    /// <summary>
    /// Builds a query for entity metadata.
    /// </summary>
    /// <param name="entityLogicalName">The entity logical name.</param>
    /// <param name="includeRelationships">Whether to include relationships.</param>
    /// <returns>The OData query string.</returns>
    public static string ForEntityMetadata(string entityLogicalName, bool includeRelationships = false)
    {
        var builder = new StringBuilder($"EntityDefinitions(LogicalName='{entityLogicalName}')");
        builder.Append("?$select=LogicalName,DisplayName,SchemaName,PrimaryIdAttribute,PrimaryNameAttribute");

        if (includeRelationships)
        {
            builder.Append("&$expand=");
            builder.Append("OneToManyRelationships($select=SchemaName,ReferencingEntity,ReferencingAttribute,ReferencedEntity,ReferencedAttribute),");
            builder.Append("ManyToOneRelationships($select=SchemaName,ReferencingEntity,ReferencingAttribute,ReferencedEntity,ReferencedAttribute),");
            builder.Append("ManyToManyRelationships($select=SchemaName,Entity1LogicalName,Entity2LogicalName,IntersectEntityName)");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds a query for global option sets.
    /// </summary>
    /// <returns>The OData query string.</returns>
    public static string ForGlobalOptionSets()
    {
        return "GlobalOptionSetDefinitions?$select=Name,OptionSetType,DisplayName";
    }

    /// <summary>
    /// Builds a query for environment variable definitions in a solution.
    /// </summary>
    /// <param name="solutionUniqueName">The solution unique name.</param>
    /// <returns>The OData query string.</returns>
    public static string ForEnvironmentVariables(string solutionUniqueName)
    {
        return new ODataQueryBuilder("environmentvariabledefinitions")
            .Select("displayname", "schemaname", "type", "defaultvalue", "environmentvariabledefinitionid")
            .Expand("environmentvariabledefinition_environmentvariablevalue",
                select: new[] { "value" })
            .Build();
    }

    /// <summary>
    /// Builds a query for workflows (processes) in a solution.
    /// </summary>
    /// <param name="category">The workflow category (0=Workflow, 4=BPF, 5=Modern Flow).</param>
    /// <returns>The OData query string.</returns>
    public static string ForWorkflows(int? category = null)
    {
        var builder = new ODataQueryBuilder("workflows")
            .Select("name", "category", "statecode", "statuscode", "_ownerid_value")
            .Expand("ownerid", select: new[] { "fullname" });

        if (category.HasValue)
        {
            builder.FilterEqual("category", category.Value);
        }

        return builder.Build();
    }

    /// <summary>
    /// Builds a query for security roles.
    /// </summary>
    /// <returns>The OData query string.</returns>
    public static string ForSecurityRoles()
    {
        return new ODataQueryBuilder("roles")
            .Select("name", "roleid", "_businessunitid_value")
            .Expand("businessunitid", select: new[] { "name" })
            .Build();
    }

    /// <summary>
    /// Builds a query for queues.
    /// </summary>
    /// <returns>The OData query string.</returns>
    public static string ForQueues()
    {
        return new ODataQueryBuilder("queues")
            .Select("name", "queueid", "queuetypecode", "emailaddress")
            .Build();
    }

    private static string FormatValue(object value)
    {
        return value switch
        {
            string s => $"'{s}'",
            Guid g => g.ToString(),
            bool b => b.ToString().ToLowerInvariant(),
            DateTime dt => dt.ToString("o"),
            DateTimeOffset dto => dto.ToString("o"),
            _ => value.ToString() ?? "null"
        };
    }
}
