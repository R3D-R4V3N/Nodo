namespace Rise.Domain.Projects;

/// <summary>
/// A project is something that a technician is working on.
/// </summary>
public class Project : Entity
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => _name = Guard.Against.NullOrEmpty(value);
    }

    public Technician Technician { get; private set; } = default!;

    /// <summary>
    /// The address is immutable and owned by the <see cref="Project"/>. If you want to change the address, create a new Address and link it to the project.
    /// <see cref="ProjectConfiguration"/>
    /// </summary>
    public Address Location { get; private set; } = default!;

    /// <summary>
    /// Entity Framework Core Constructor
    /// </summary>
    private Project()
    {
    }

    public Project(string name, Technician technician, Address location)
    {
        Name = name;
        SetTechnician(technician);
        UpdateLocation(location);
    }

    public bool CanBeEditedBy(Technician technician)
    {
        return Technician == technician; // due to Entity (comparision via ID)
    }

    public void Edit(string name)
    {
        Name = Guard.Against.NullOrEmpty(name);
    }

    public void ReassignTechnician(Technician technician)
    {
        Guard.Against.Null(technician);

        if (Technician == technician)
        {
            return;
        }

        Technician?.DetachProject(this);
        SetTechnician(technician);
    }

    public void UpdateLocation(Address location)
    {
        Location = Guard.Against.Null(location);
    }

    private void SetTechnician(Technician technician)
    {
        Technician = Guard.Against.Null(technician);
        Technician.AttachProject(this);
    }
}
