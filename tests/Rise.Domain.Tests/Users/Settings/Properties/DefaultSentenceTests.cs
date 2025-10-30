using Ardalis.Result;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Domain.Tests.Users.Settings.Properties;

public class DefaultSentenceTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValid()
    {
        var validSentence = "This is a sentence.   ";

        var result = DefaultSentence.Create(validSentence);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validSentence);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_ShouldReturnConflict_WhenValueIsEmptyOrNull(string invalidValue)
    {
        var result = DefaultSentence.Create(invalidValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Standaardzin is leeg."]);
    }

    [Fact]
    public void Create_ShouldReturnConflict_WhenValueIsTooLong()
    {
        var longValue = new string('a', DefaultSentence.MAX_LENGTH + 1);

        var result = DefaultSentence.Create(longValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Standaardzin is te lang."]);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnStringValue()
    {
        var sentence = "This is a sentence.";
        var defaultSentence = DefaultSentence.Create(sentence).Value;

        string result = defaultSentence;

        result.ShouldBe(sentence);
    }

    [Fact]
    public void ExplicitOperator_ShouldReturnDefaultSentence_WhenValueIsValid()
    {
        var sentence = "This is a sentence.";

        var avatarUrl = (DefaultSentence)sentence;

        avatarUrl.Value.ShouldBe(sentence);
    }

    [Fact]
    public void ExplicitOperator_ShouldThrow_WhenValueIsInvalid()
    {
        string invalid = "";

        Action act = () => { var _ = (DefaultSentence)invalid; };

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("Standaardzin is leeg.");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForSameValue()
    {
        var sentence = "zin";
        var a = DefaultSentence.Create(sentence).Value;
        var b = DefaultSentence.Create(sentence).Value;

        var areEqual = a.Equals(b);

        areEqual.ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var sentence = "zin";
        var defaultSentence = DefaultSentence.Create(sentence).Value;

        var result = defaultSentence.ToString();

        result.ShouldBe(sentence);
    }
}
