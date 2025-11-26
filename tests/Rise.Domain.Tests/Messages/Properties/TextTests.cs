using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;
using Rise.Tests.Shared;

namespace Rise.Domain.Tests.Messages.Properties;

public class TextTests
{
    [Theory]
    [InlineData("this is a valid sentence.", "this is a valid sentence.")]
    [InlineData("this is a valid sentence. ", "this is a valid sentence.")]
    [InlineData(" this is a valid sentence.", "this is a valid sentence.")]
    [InlineData(" this is a valid sentence. ", "this is a valid sentence.")]
    public void Create_ShouldReturnSuccess(string sentence, string actual)
    {
        var result = TextMessage.Create(sentence);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsSuspicious.ShouldBeFalse();
        result.Value.Value.ShouldBe(actual);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_ContainsBadWord()
    {
        var validSentence = $"This is a {DomainData.ValidNaughtyWord()} sentence.";

        var result = TextMessage.Create(validSentence);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsSuspicious.ShouldBeTrue();
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
        var sentence = "This is a sentence.";
        var defaultSentence = TextMessage.Create(sentence).Value;

        string result = defaultSentence;

        result.ShouldBe(sentence);
    }

    [Fact]
    public void ExplicitOperator_ShouldReturnDefaultSentence_WhenValueIsValid()
    {
        var sentence = "This is a sentence.";

        var avatarUrl = (TextMessage)sentence;

        avatarUrl.Value.ShouldBe(sentence);
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
        var sentence = "zin";
        var a = TextMessage.Create(sentence).Value;
        var b = TextMessage.Create(sentence).Value;

        var areEqual = a.Equals(b);

        areEqual.ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var sentence = "zin";
        var defaultSentence = TextMessage.Create(sentence).Value;

        var result = defaultSentence.ToString();

        result.ShouldBe(sentence);
    }
}
