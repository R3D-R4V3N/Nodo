using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;
using Shouldly;
using System;
using Xunit;

namespace Rise.Domain.Tests.Common.ValueObjects;

public class LastNameTests
{
    [Theory]
    [InlineData("Snow", "Snow")]
    [InlineData("snow", "Snow")]
    [InlineData("snOw", "Snow")]
    [InlineData(" Snow", "Snow")]
    [InlineData("Snow ", "Snow")]
    [InlineData(" Snow ", "Snow")]
    [InlineData("Snow\n", "Snow")]
    [InlineData("Snow\t", "Snow")]
    [InlineData("Snow e", "Snow E")]
    [InlineData("Snow eR", "Snow Er")]
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
        var result = LastName.Create(invalidValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Achternaam is leeg."]);
    }

    [Fact]
    public void Create_ShouldReturnConflict_WhenValueIsTooLong()
    {
        var longValue = new string('a', LastName.MAX_LENGTH + 1);

        var result = LastName.Create(longValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Achternaam is te lang."]);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnStringValue()
    {
        var url = "Snow";
        var bio = LastName.Create(url).Value;

        string result = bio;

        result.ShouldBe(url);
    }

    [Fact]
    public void ExplicitOperator_ShouldReturnLastName_WhenValueIsValid()
    {
        var text = "Snow";

        var bio = (LastName)text;

        bio.Value.ShouldBe(text);
    }

    [Fact]
    public void ExplicitOperator_ShouldThrow_WhenValueIsInvalid()
    {
        string invalid = "";

        Action act = () => { var _ = (LastName)invalid; };

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("Achternaam is leeg.");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForSameValue()
    {
        var text = "Snow";
        var a = LastName.Create(text).Value;
        var b = LastName.Create(text).Value;

        var areEqual = a.Equals(b);

        areEqual.ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var text = "Snow";
        var bio = LastName.Create(text).Value;

        var result = bio.ToString();

        result.ShouldBe(text);
    }
}
