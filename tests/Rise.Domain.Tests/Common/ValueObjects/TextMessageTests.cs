using Ardalis.Result;
using Rise.Domain.Common;
using Rise.Domain.Common.ValueObjects;
using Rise.Tests.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Domain.Tests.Common.ValueObjects;
public class TextMessageTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValid()
    {
        var valid = "Hey how are you.";

        var result = TextMessage.Create(valid);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(valid);
        result.Value.CleanedUpValue.ShouldBe(valid);
        result.Value.IsSuspicious.ShouldBeFalse();
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValidAndSuspicious()
    {
        var valid = $"{DomainData.GetRandomBlacklistedWord()} how are you.";
        var cleanedUp = WordFilter.Censor(valid);

        var result = TextMessage.Create(valid);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(valid);
        result.Value.CleanedUpValue.ShouldBe(cleanedUp);
        result.Value.IsSuspicious.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_ShouldReturnConflict_WhenValueIsEmptyOrNull(string invalidValue)
    {
        var result = TextMessage.Create(invalidValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Bericht is leeg."]);
    }

    [Fact]
    public void Create_ShouldReturnConflict_WhenValueIsTooLong()
    {
        var longValue = new string('a', TextMessage.MAX_LENGTH + 1);

        var result = TextMessage.Create(longValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Bericht is te lang."]);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnStringValue()
    {
        var text = "Hey, I like turtles.";
        var note = TextMessage.Create(text).Value;

        string result = note;

        result.ShouldBe(text);
    }

    [Fact]
    public void ExplicitOperator_ShouldReturnTextMessage_WhenValueIsValid()
    {
        var text = "Hey, I like turtles.";

        var note = (TextMessage)text;

        note.Value.ShouldBe(text);
    }

    [Fact]
    public void ExplicitOperator_ShouldThrow_WhenValueIsInvalid()
    {
        string invalid = "";

        Action act = () => { var _ = (TextMessage)invalid; };

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("Bericht is leeg.");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForSameValue()
    {
        var text = "Hey, kachow";
        var a = TextMessage.Create(text).Value;
        var b = TextMessage.Create(text).Value;

        var areEqual = a.Equals(b);

        areEqual.ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var text = "Hey, zim";
        var message = TextMessage.Create(text).Value;

        var result = message.ToString();

        result.ShouldBe(text);
    }
}
