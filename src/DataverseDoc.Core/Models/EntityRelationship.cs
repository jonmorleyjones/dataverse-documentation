namespace DataverseDoc.Core.Models;

/// <summary>
/// Represents a relationship between two Dataverse entities.
/// </summary>
public record EntityRelationship(
    string ParentEntity,
    string ChildEntity,
    string RelationshipType,
    string? RelationshipName);

/// <summary>
/// Represents entity metadata with its relationships.
/// </summary>
public record EntityMetadata(
    string LogicalName,
    string? DisplayName,
    List<EntityRelationship> Relationships);
