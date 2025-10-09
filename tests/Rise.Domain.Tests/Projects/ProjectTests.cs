using Rise.Domain.Projects;

namespace Rise.Domain.Tests.Projects;

public class ProjectTests
{
    [Fact]
    public void Constructor_AssignsTechnicianAndLocation()
    {
        var technician = new Technician("Jane", "Doe", "account-1");
        var address = new Address("Main street", "Suite 1", "Townsville", "1234");

        var project = new Project("Migration", technician, address);

        project.Technician.ShouldBe(technician);
        technician.Projects.ShouldContain(project);
        project.Location.ShouldBe(address);
    }

    [Fact]
    public void ReassignTechnician_UpdatesBothSides()
    {
        var originalTechnician = new Technician("Jane", "Doe", "account-1");
        var replacementTechnician = new Technician("John", "Smith", "account-2");
        var address = new Address("Main street", "Suite 1", "Townsville", "1234");
        var project = new Project("Migration", originalTechnician, address);

        project.ReassignTechnician(replacementTechnician);

        project.Technician.ShouldBe(replacementTechnician);
        replacementTechnician.Projects.ShouldContain(project);
        originalTechnician.Projects.ShouldNotContain(project);
    }

    [Fact]
    public void UpdateLocation_ReplacesAddress()
    {
        var technician = new Technician("Jane", "Doe", "account-1");
        var project = new Project("Migration", technician, new Address("Main street", "Suite 1", "Townsville", "1234"));
        var newLocation = new Address("Second street", "Floor 2", "Townsville", "5678");

        project.UpdateLocation(newLocation);

        project.Location.ShouldBe(newLocation);
    }
}
