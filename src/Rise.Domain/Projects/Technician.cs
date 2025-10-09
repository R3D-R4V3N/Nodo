namespace Rise.Domain.Projects;

/// <summary>
/// A technician is a person who is owner to multiple projects.
/// This is a domain entity and seperated from the <see cref="IdentityUser"/> the link is made via the <see cref="AccountId"/>.
/// So we can swap out the identity provider without changing the domain.
/// </summary>
public class Technician : Entity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Link to the <see cref="IdentityUser"/> account, so a Technician HAS AN account and not IS A <see cref="IdentityUser"/>.
    /// </summary>
    public string AccountId { get; private set; } = string.Empty;

    private readonly List<Project> _projects = [];
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

    private Technician()
    {
    }

    public Technician(string firstName, string lastName, string accountId)
    {
        UpdateName(firstName, lastName);
        UpdateAccount(accountId);
    }

    public void UpdateName(string firstName, string lastName)
    {
        FirstName = Guard.Against.NullOrEmpty(firstName);
        LastName = Guard.Against.NullOrEmpty(lastName);
    }

    public void UpdateAccount(string accountId)
    {
        AccountId = Guard.Against.NullOrEmpty(accountId);
    }

    internal void AttachProject(Project project)
    {
        Guard.Against.Null(project);

        if (_projects.Contains(project))
        {
            return;
        }

        _projects.Add(project);
    }

    internal void DetachProject(Project project)
    {
        if (project is null)
        {
            return;
        }

        _projects.Remove(project);
    }
}
