using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;
using Shouldly;
using System;
using Xunit;

namespace Rise.Domain.Tests.Common.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValid()
    {
        var validEmail = "test@exmaple.com";

        var result = Email.Create(validEmail);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_ShouldReturnConflict_WhenValueIsEmptyOrNull(string invalidValue)
    {
        var result = Email.Create(invalidValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Email is leeg."]);
    }

    [Fact]
    public void Create_ShouldReturnConflict_WhenValueIsTooLong()
    {
        var longValue = new string('a', Email.MAX_LENGTH + 1);

        var result = Email.Create(longValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Email is te lang. Maximum {Email.MAX_LENGTH} tekens."]);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnStringValue()
    {
        var mail = "example@email.com";
        var email = Email.Create(mail).Value;

        string result = email;

        result.ShouldBe(mail);
    }

    [Fact]
    public void ExplicitOperator_ShouldReturnEmail_WhenValueIsValid()
    {
        var mail = "example@email.com";

        var email = (Email)mail;

        email.Value.ShouldBe(mail);
    }

    [Fact]
    public void ExplicitOperator_ShouldThrow_WhenValueIsInvalid()
    {
        string invalid = "";

        Action act = () => { var _ = (Email)invalid; };

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("Email is leeg.");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForSameValue()
    {
        var mail = "example@email.com";
        var a = Email.Create(mail).Value;
        var b = Email.Create(mail).Value;

        var areEqual = a.Equals(b);

        areEqual.ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var mail = "example@email.com";
        var email = Email.Create(mail).Value;

        var result = email.ToString();

        result.ShouldBe(mail);
    }
}
