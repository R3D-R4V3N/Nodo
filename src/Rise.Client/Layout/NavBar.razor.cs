using Microsoft.AspNetCore.Components;
using Rise.Shared.Users;

namespace Rise.Client.Layout;
public partial class NavBar
{
    [CascadingParameter] public UserDto.CurrentUser? CurrentUser { get; set; }
}