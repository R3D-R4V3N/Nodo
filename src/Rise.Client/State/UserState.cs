using Rise.Shared.Users;

namespace Rise.Client.State;
public partial class UserState 
{
    private UserDto.CurrentUser? _user;
    public UserDto.CurrentUser? User
    {
        get => _user;
        set
        {
            _user = value;
            NotifyUserChanged();
        }
    }

    public event Action? OnChange;
    private void NotifyUserChanged() => OnChange?.Invoke();
}