using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using NSubstitute;
using Rise.Shared.Identity.Accounts;
using Rise.Client.Pages;
using Xunit.Abstractions;
using Rise.Client.Identity.Components;

namespace Rise.Client.Pages;

public class LoginFormShould : TestContext
{
    public LoginFormShould(ITestOutputHelper outputHelper)
    {
        Services.AddXunitLogger(outputHelper);
    }

    [Fact]
    public void RenderAllElements()
    {
        // Arrange
        var model = new AccountRequest.Login();

        // Act
        var cut = RenderComponent<LoginForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Assert
        cut.Find("#login-email");
        cut.Find("#login-password");
        cut.Find("button[type=submit]");
        cut.Find("a[href='/register']");
        cut.Find("a[href='#']");
    }

    [Fact]
    public void BindEmailAndPasswordToModel()
    {
        // Arrange
        var model = new AccountRequest.Login();
        var cut = RenderComponent<LoginForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act
        cut.Find("#login-email").Change("kyandro@example.com");
        cut.Find("#login-password").Change("SuperSecret123");

        // Assert
        Assert.Equal("kyandro@example.com", model.Email);
        Assert.Equal("SuperSecret123", model.Password);
    }

    [Fact]
    public async Task TriggerOnSubmitWhenFormSubmitted()
    {
        // Arrange
        var model = new AccountRequest.Login
        {
            Email = "kyandro@example.com",
            Password = "password123"
        };
        var submitted = false;

        var cut = RenderComponent<LoginForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => submitted = true))
        );

        // Act
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        // Assert
        Assert.True(submitted);
    }
}