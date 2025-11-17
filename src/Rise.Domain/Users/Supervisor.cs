using Rise.Domain.Organizations;
using Rise.Domain.Users.Settings;

namespace Rise.Domain.Users;
public class Supervisor : BaseUser
{
    private Organization _organization;
    public required Organization Organization
    {
        get => _organization;
        set
        {
            if (_organization == value) return;

            _organization = Guard.Against.Null(value);
            if (!_organization.Workers.Contains(this))
            {
                _organization.AddWorker(this);
            }
        }
    }
}
