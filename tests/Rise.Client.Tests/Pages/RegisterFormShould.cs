using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Rise.Client.Identity.Components;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.Organizations;
using Xunit;

public class RegisterFormShould : TestContext
{
    [Fact]
    public void RenderAllInputFieldsAndButton()
    {
        // Arrange
        var model = new AccountRequest.Register();
        var organizations = new List<OrganizationDto.Summary>
        {
            new() { Id = 1, Name = "Nodo vzw" },
            new() { Id = 2, Name = "HoGent" }
        };
        var cut = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.Organizations, organizations)
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
        var organizations = new List<OrganizationDto.Summary>
        {
            new() { Id = 1, Name = "Nodo vzw" },
            new() { Id = 2, Name = "HoGent" }
        };
        bool submitted = false;

        var cut = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.Organizations, organizations)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { submitted = true; }))
        );

        // Vul de velden
        cut.Find("#register-name").Change("Kyandro Voet");
        cut.Find("#register-email").Change("kyandro@example.com");
        cut.Find("#register-password").Change("Secret123!");
        cut.Find("#register-confirm-password").Change("Secret123!");
        cut.Find("#register-organization").Change("2");

        // Act
        cut.Find("form").Submit();

        // Assert
        Assert.True(submitted, "OnSubmit should be triggered when the form is submitted");
        Assert.Equal(2, model.OrganizationId);
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