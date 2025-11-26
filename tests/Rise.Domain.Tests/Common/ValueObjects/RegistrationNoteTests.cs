using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;
using Shouldly;
using System;
using Xunit;

namespace Rise.Domain.Tests.Common.ValueObjects;

public class registrationNoteTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValid()
    {
        var validNote = "Hey how are you.";

        var result = RegistrationNote.Create(validNote);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validNote);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_ShouldReturnConflict_WhenValueIsEmptyOrNull(string invalidValue)
    {
        var result = RegistrationNote.Create(invalidValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Registratie notitie is leeg."]);
    }

    [Fact]
    public void Create_ShouldReturnConflict_WhenValueIsTooLong()
    {
        var longValue = new string('a', RegistrationNote.MAX_LENGTH + 1);

        var result = RegistrationNote.Create(longValue);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Registratie notitie is te lang."]);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnStringValue()
    {
        var text = "Hey, I like turtles.";
        var note = RegistrationNote.Create(text).Value;

        string result = note;

        result.ShouldBe(text);
    }

    [Fact]
    public void ExplicitOperator_ShouldReturnRegistrationNote_WhenValueIsValid()
    {
        var text = "Hey, I like turtles.";

        var note = (RegistrationNote)text;

        note.Value.ShouldBe(text);
    }

    [Fact]
    public void ExplicitOperator_ShouldThrow_WhenValueIsInvalid()
    {
        string invalid = "";

        Action act = () => { var _ = (RegistrationNote)invalid; };

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("Registratie notitie is leeg.");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForSameValue()
    {
        var text = "Hey, kachow";
        var a = RegistrationNote.Create(text).Value;
        var b = RegistrationNote.Create(text).Value;

        var areEqual = a.Equals(b);

        areEqual.ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var text = "Hey, zim";
        var note = RegistrationNote.Create(text).Value;

        var result = note.ToString();

        result.ShouldBe(text);
    }
}
