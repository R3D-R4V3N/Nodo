using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Rise.Client.Identity.Components;
using Rise.Shared.Identity.Accounts;
using Xunit;

public class RegisterFormShould : TestContext
{
    [Fact]
    public void RenderAllInputFieldsAndButton()
    {
        // Arrange
        var model = new AccountRequest.Register();
        var cut = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        // Controleer dat alle velden aanwezig zijn
        cut.Find("#register-name");
        cut.Find("#register-email");
        cut.Find("#register-password");
        cut.Find("#register-confirm-password");
        cut.Find("#register-organization");

        // Controleer dat de submit button aanwezig is
        var submitButton = cut.Find("button[type='submit']");
        Assert.NotNull(submitButton);

        // Controleer dat de link terug naar login aanwezig is
        var loginLink = cut.Find("a[href='/login']");
        Assert.NotNull(loginLink);
    }

    [Fact]
    public void TriggerOnSubmit_WhenFormIsSubmitted()
    {
        // Arrange
        var model = new AccountRequest.Register();
        bool submitted = false;

        var cut = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { submitted = true; }))
        );

        // Vul de velden
        cut.Find("#register-name").Change("Kyandro Voet");
        cut.Find("#register-email").Change("kyandro@example.com");
        cut.Find("#register-password").Change("Secret123!");
        cut.Find("#register-confirm-password").Change("Secret123!");
        cut.Find("#register-organization").Change("org1");

        // Act
        cut.Find("form").Submit();

        // Assert
        Assert.True(submitted, "OnSubmit should be triggered when the form is submitted");
        Assert.Equal("org1", model.Organization);
    }

    //[Fact]
    public void ShowValidationMessages_WhenFieldsAreEmpty()
    {
        // Arrange
        var model = new AccountRequest.Register();
        var cut = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act
        cut.Find("form").Submit();

        // Assert: Controleer dat ValidationMessages zichtbaar zijn
        var messages = cut.FindAll("div span.text-red-600");
        Assert.NotEmpty(messages);
    }
}