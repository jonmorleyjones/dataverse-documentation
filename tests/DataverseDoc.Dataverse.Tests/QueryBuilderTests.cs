using FluentAssertions;
using Xunit;

namespace DataverseDoc.Dataverse.Tests;

public class ODataQueryBuilderTests
{
    [Fact]
    public void Build_WithEntityOnly_ReturnsEntityName()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts");

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts");
    }

    [Fact]
    public void Select_SingleColumn_AddsSelectClause()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .Select("name");

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$select=name");
    }

    [Fact]
    public void Select_MultipleColumns_AddsCommaSeparatedSelectClause()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .Select("name", "accountnumber", "revenue");

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$select=name,accountnumber,revenue");
    }

    [Fact]
    public void Filter_SingleCondition_AddsFilterClause()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .Filter("statecode eq 0");

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$filter=statecode eq 0");
    }

    [Fact]
    public void Filter_MultipleConditions_CombinesWithAnd()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .Filter("statecode eq 0")
            .Filter("revenue gt 1000000");

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$filter=statecode eq 0 and revenue gt 1000000");
    }

    [Fact]
    public void Expand_SingleNavigation_AddsExpandClause()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .Expand("primarycontactid");

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$expand=primarycontactid");
    }

    [Fact]
    public void Expand_WithNestedSelect_AddsNestedSelectClause()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .Expand("primarycontactid", select: new[] { "fullname", "emailaddress1" });

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$expand=primarycontactid($select=fullname,emailaddress1)");
    }

    [Fact]
    public void Expand_WithNestedFilter_AddsNestedFilterClause()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .Expand("contact_customer_accounts", filter: "statecode eq 0");

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$expand=contact_customer_accounts($filter=statecode eq 0)");
    }

    [Fact]
    public void Expand_WithNestedSelectAndFilter_AddsBothClauses()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .Expand("contact_customer_accounts",
                select: new[] { "fullname" },
                filter: "statecode eq 0");

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$expand=contact_customer_accounts($select=fullname;$filter=statecode eq 0)");
    }

    [Fact]
    public void Top_AddsTopClause()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .Top(10);

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$top=10");
    }

    [Fact]
    public void OrderBy_SingleColumn_AddsOrderByClause()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .OrderBy("name");

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$orderby=name");
    }

    [Fact]
    public void OrderBy_Descending_AddsDescSuffix()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .OrderBy("createdon", descending: true);

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$orderby=createdon desc");
    }

    [Fact]
    public void ComplexQuery_CombinesAllClauses()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .Select("name", "accountnumber")
            .Filter("statecode eq 0")
            .Expand("primarycontactid", select: new[] { "fullname" })
            .OrderBy("name")
            .Top(50);

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Contain("$select=name,accountnumber");
        query.Should().Contain("$filter=statecode eq 0");
        query.Should().Contain("$expand=primarycontactid($select=fullname)");
        query.Should().Contain("$orderby=name");
        query.Should().Contain("$top=50");
    }

    [Fact]
    public void ForSolutionComponents_BuildsCorrectQuery()
    {
        // Arrange
        var solutionId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var query = ODataQueryBuilder.ForSolutionComponents(solutionId, componentType: 380);

        // Assert
        query.Should().Contain("solutioncomponents");
        query.Should().Contain("_solutionid_value eq 12345678-1234-1234-1234-123456789012");
        query.Should().Contain("componenttype eq 380");
    }

    [Fact]
    public void ForEntityMetadata_BuildsCorrectQuery()
    {
        // Act
        var query = ODataQueryBuilder.ForEntityMetadata("account");

        // Assert
        query.Should().StartWith("EntityDefinitions(LogicalName='account')");
    }

    [Fact]
    public void ForEntityMetadata_WithRelationships_IncludesExpand()
    {
        // Act
        var query = ODataQueryBuilder.ForEntityMetadata("account", includeRelationships: true);

        // Assert
        query.Should().Contain("$expand=");
        query.Should().Contain("OneToManyRelationships");
        query.Should().Contain("ManyToOneRelationships");
    }

    [Fact]
    public void FilterEqual_CreatesCorrectFilter()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .FilterEqual("statecode", 0);

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$filter=statecode eq 0");
    }

    [Fact]
    public void FilterEqual_WithGuid_FormatsCorrectly()
    {
        // Arrange
        var id = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var builder = new ODataQueryBuilder("accounts")
            .FilterEqual("accountid", id);

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$filter=accountid eq 12345678-1234-1234-1234-123456789012");
    }

    [Fact]
    public void FilterEqual_WithString_AddsQuotes()
    {
        // Arrange
        var builder = new ODataQueryBuilder("accounts")
            .FilterEqual("name", "Contoso");

        // Act
        var query = builder.Build();

        // Assert
        query.Should().Be("accounts?$filter=name eq 'Contoso'");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidEntity_ThrowsArgumentException(string? entity)
    {
        // Act
        var act = () => new ODataQueryBuilder(entity!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
