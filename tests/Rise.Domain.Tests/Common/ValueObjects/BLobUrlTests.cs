using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;
using Shouldly;
using System;
using Xunit;

namespace Rise.Domain.Tests.Common.ValueObjects;

public class BLobUrlTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValid()
    {
        var validUrl = "https://example.com/avatar.png";

        var result = BlobUrl.Create(validUrl);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_ShouldReturnConflict_WhenValueIsEmptyOrNull(string invalidValue)
    {
        var result = BlobUrl.Create(invalidValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Blob url is leeg."]);
    }

    [Fact]
    public void Create_ShouldReturnConflict_WhenValueIsTooLong()
    {
        var longValue = new string('a', BlobUrl.MAX_LENGTH + 1);

        var result = BlobUrl.Create(longValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Blob url is te lang. Maximum {BlobUrl.MAX_LENGTH} tekens."]);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnStringValue()
    {
        var url = "https://avatar.com/test.png";
        var avatarUrl = BlobUrl.Create(url).Value;

        string result = avatarUrl;

        result.ShouldBe(url);
    }

    [Fact]
    public void ExplicitOperator_ShouldReturnAvatarUrl_WhenValueIsValid()
    {
        var url = "https://valid.com/avatar.jpg";

        var avatarUrl = (BlobUrl)url;

        avatarUrl.Value.ShouldBe(url);
    }

    [Fact]
    public void ExplicitOperator_ShouldThrow_WhenValueIsInvalid()
    {
        string invalid = "";

        Action act = () => { var _ = (BlobUrl)invalid; };

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("Blob url is leeg.");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForSameValue()
    {
        var url = "https://same.com/avatar.png";
        var a = BlobUrl.Create(url).Value;
        var b = BlobUrl.Create(url).Value;

        var areEqual = a.Equals(b);

        areEqual.ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var url = "https://test.com/avatar.jpg";
        var avatarUrl = BlobUrl.Create(url).Value;

        var result = avatarUrl.ToString();

        result.ShouldBe(url);
    }
}
