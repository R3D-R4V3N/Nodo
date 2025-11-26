using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;
using Shouldly;
using System;
using Xunit;

namespace Rise.Domain.Tests.Common.ValueObjects;

public class FontSizeTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(17)]
    [InlineData(30)]
    public void Create_ShouldReturnSuccess_WhenValueIsValid(int validSize)
    {
        var result = FontSize.Create(validSize);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validSize);
    }

    [Theory]
    [InlineData(-17)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(31)]
    [InlineData(1_000)]
    public void Create_ShouldReturnConflict_WhenValueIsTooLowOrHigh(int invalidSize)
    {
        var result = FontSize.Create(invalidSize);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Lettergrootte moet tussen {FontSize.MIN_FONT_SIZE} en {FontSize.MAX_FONT_SIZE} zijn"]);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnValue()
    {
        var size = 10;
        var avatarUrl = FontSize.Create(size).Value;

        int result = avatarUrl;

        result.ShouldBe(size);
    }

    [Fact]
    public void ExplicitOperator_ShouldReturnFontSize_WhenValueIsValid()
    {
        var size = 10;

        var avatarUrl = (FontSize)size;

        avatarUrl.Value.ShouldBe(size);
    }

    [Fact]
    public void ExplicitOperator_ShouldThrow_WhenValueIsInvalid()
    {
        var invalid = -1;

        Action act = () => { var _ = (FontSize)invalid; };

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain($"Lettergrootte moet tussen {FontSize.MIN_FONT_SIZE} en {FontSize.MAX_FONT_SIZE} zijn");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForSameValue()
    {
        var size = 12;
        var a = FontSize.Create(size).Value;
        var b = FontSize.Create(size).Value;

        var areEqual = a.Equals(b);

        areEqual.ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var size = 12;
        var avatarUrl = FontSize.Create(size).Value;

        var result = avatarUrl.ToString();

        result.ShouldBe(size.ToString());
    }
}
