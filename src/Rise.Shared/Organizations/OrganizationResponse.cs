namespace Rise.Shared.Organizations;

/// <summary>
/// Responses for organization related endpoints.
/// </summary>
public static class OrganizationResponse
{
    /// <summary>
    /// Represents a selectable organization in dropdowns or lists.
    /// </summary>
    /// <param name="Id">Unique identifier of the organization.</param>
    /// <param name="Name">Display name of the organization.</param>
    /// <param name="Address">Formatted address of the organization.</param>
    public record ListItem(int Id, string Name, string Address);
}
