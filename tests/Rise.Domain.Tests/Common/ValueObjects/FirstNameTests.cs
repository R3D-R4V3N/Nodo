using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;
using Shouldly;
using System;
using Xunit;

namespace Rise.Domain.Tests.Common.ValueObjects;

public class FirstNameTests
{
    [Theory]
    [InlineData("John", "John")]
    [InlineData("john", "John")]
    [InlineData("jOhn", "John")]
    [InlineData(" John", "John")]
    [InlineData("John ", "John")]
    [InlineData(" John ", "John")]
    [InlineData("John\n", "John")]
    [InlineData("John\t", "John")]
    [InlineData("John eres", "John Eres")]
    [InlineData("John eRes", "John Eres")]
    public void Create_ShouldReturnSuccess_WhenValueIsValid(string raw, string cleanedUp)
    {
        var result = FirstName.Create(raw);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(cleanedUp);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_ShouldReturnConflict_WhenValueIsEmptyOrNull(string invalidValue)
    {
        var result = FirstName.Create(invalidValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Voornaam is leeg."]);
    }

    [Fact]
    public void Create_ShouldReturnConflict_WhenValueIsTooLong()
    {
        var longValue = new string('a', FirstName.MAX_LENGTH + 1);

        var result = FirstName.Create(longValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Voornaam is te lang."]);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnStringValue()
    {
        var url = "John";
        var bio = FirstName.Create(url).Value;

        string result = bio;

        result.ShouldBe(url);
    }

    [Fact]
    public void ExplicitOperator_ShouldReturnFirstName_WhenValueIsValid()
    {
        var text = "John";

        var bio = (FirstName)text;

        bio.Value.ShouldBe(text);
    }

    [Fact]
    public void ExplicitOperator_ShouldThrow_WhenValueIsInvalid()
    {
        string invalid = "";

        Action act = () => { var _ = (FirstName)invalid; };

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("Voornaam is leeg.");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForSameValue()
    {
        var text = "John";
        var a = FirstName.Create(text).Value;
        var b = FirstName.Create(text).Value;

        var areEqual = a.Equals(b);

        areEqual.ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var text = "John";
        var bio = FirstName.Create(text).Value;

        var result = bio.ToString();

        result.ShouldBe(text);
    }
}
