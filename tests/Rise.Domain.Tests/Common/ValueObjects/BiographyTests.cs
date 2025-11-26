using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;
using Shouldly;
using System;
using Xunit;

namespace Rise.Domain.Tests.Common.ValueObjects;

public class BiographyTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValid()
    {
        var validBio = "Hey how are you.";

        var result = Biography.Create(validBio);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validBio);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_ShouldReturnConflict_WhenValueIsEmptyOrNull(string invalidValue)
    {
        var result = Biography.Create(invalidValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Biografie is leeg."]);
    }

    [Fact]
    public void Create_ShouldReturnConflict_WhenValueIsTooLong()
    {
        var longValue = new string('a', Biography.MAX_LENGTH + 1);

        var result = Biography.Create(longValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Biografie is te lang."]);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnStringValue()
    {
        var url = "Hey, I like turtles.";
        var bio = Biography.Create(url).Value;

        string result = bio;

        result.ShouldBe(url);
    }

    [Fact]
    public void ExplicitOperator_ShouldReturnBiography_WhenValueIsValid()
    {
        var text = "Hey, I like turtles.";

        var bio = (Biography)text;

        bio.Value.ShouldBe(text);
    }

    [Fact]
    public void ExplicitOperator_ShouldThrow_WhenValueIsInvalid()
    {
        string invalid = "";

        Action act = () => { var _ = (Biography)invalid; };

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("Biografie is leeg.");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForSameValue()
    {
        var text = "Hey, kachow";
        var a = Biography.Create(text).Value;
        var b = Biography.Create(text).Value;

        var areEqual = a.Equals(b);

        areEqual.ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var text = "Hey, zim";
        var bio = Biography.Create(text).Value;

        var result = bio.ToString();

        result.ShouldBe(text);
    }
}
