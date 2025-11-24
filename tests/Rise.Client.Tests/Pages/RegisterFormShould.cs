using System;
using System.Collections.Generic;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Rise.Client.Identity.Components;
using Rise.Client.UserConnections.Components.FriendProfile;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.Organizations;
using Rise.Shared.Users;
using Xunit;

namespace Rise.Client.Tests.Pages;
public class RegisterFormShould : TestContext
{
    [Fact]
    public void RenderAllInputFieldsAndButton()
    {
        // Arrange
        var model = new AccountRequest.Register
        {
            BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            AvatarDataUrl = "data:image/png;base64,AAA",
        };
        var organizations = new List<OrganizationDto.Summary>
        {
            new() { Id = 1, Name = "Nodo" }
        };
        var cut = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.Organizations, organizations)
        );

        // Act & Assert
        cut.Find("#register-first-name");
        cut.Find("#register-last-name");
        cut.Find("#register-email");
        cut.Find("#register-password");
        cut.Find("#register-confirm-password");
        cut.Find("#register-organization");
        cut.Find("#register-birth-date");
        cut.Find("#register-gender");
        cut.Find("#register-avatar");

        cut.Find("#next-slide").Click();

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
        var model = new AccountRequest.Register
        {
            BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            AvatarDataUrl = "data:image/png;base64,AAA",
        };
        var organizations = new List<OrganizationDto.Summary>
        {
            new() { Id = 1, Name = "Nodo" }
        };
        bool submitted = false;

        var cut = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { submitted = true; }))
            .Add(p => p.Organizations, organizations)
        );

        // Vul de velden
        cut.Find("#register-first-name").Change("Kyandro");
        cut.Find("#register-last-name").Change("Voet");
        cut.Find("#register-email").Change("kyandro@example.com");
        cut.Find("#register-password").Change("Secret123!");
        cut.Find("#register-confirm-password").Change("Secret123!");
        cut.Find("#register-organization").Change("1");
        cut.Find("#register-gender").Change(GenderTypeDto.Man.ToString());
        cut.Find("#register-birth-date").Change(model.BirthDate?.ToString("yyyy-MM-dd"));

        // Act
        cut.Find("form").Submit();

        // Assert
        Assert.True(submitted, "OnSubmit should be triggered when the form is submitted");
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