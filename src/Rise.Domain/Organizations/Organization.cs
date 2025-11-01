using Rise.Domain.Common;

namespace Rise.Domain.Organizations;

/// <summary>
/// Represents an organisation that users can belong to.
/// </summary>
public class Organization : Entity
{
    /// <summary>
    /// The display name of the organisation.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description shown in tooling.
    /// </summary>
    public string? Description { get; set; }
}
