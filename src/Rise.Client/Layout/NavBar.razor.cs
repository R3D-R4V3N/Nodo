using Microsoft.AspNetCore.Components;
using Rise.Client.State;

namespace Rise.Client.Layout;
public partial class NavBar
{
    [Inject] public UserState UserState { get; set; }
}