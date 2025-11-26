using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Users;
using Rise.Domain.Users.Settings;

namespace Rise.Domain.Registrations;

public class RegistrationStatus : ValueObject
{
    private RegistrationRequest _request;
    public RegistrationRequest Request
    {
        get => _request;
        set
        {
            if (_request == value) return;

            _request = Guard.Against.Null(value);
            if (_request.Status != this)
            {
                _request.Status = this;
            }
        }
    }
    public required RegistrationStatusType StatusType { get; set; }
    public Supervisor? HandledBy { get; set; }
    public DateTime? HandledDate { get; set; }
    public RegistrationNote? Note { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Request;
    }
}