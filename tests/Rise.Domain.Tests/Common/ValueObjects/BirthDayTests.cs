using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;
using Shouldly;
using System;
using Xunit;

namespace Rise.Domain.Tests.Common.ValueObjects;

public class BirthDayTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValid()
    {
        var validBday = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-10);

        var result = BirthDay.Create(validBday);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validBday);
    }

    [Fact]
    public void Create_ShouldReturnConflict_WhenValueIsEmptyOrNull()
    {
        var result = BirthDay.Create(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1));

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Verjaardag ligt in de toekomst."]);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnDateOnlyValue()
    {
        var bday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var birthday = BirthDay.Create(bday).Value;

        DateOnly result = birthday;

        result.ShouldBe(bday);
    }

    [Fact]
    public void ExplicitOperator_ShouldReturnBirthDay_WhenValueIsValid()
    {
        var bday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);

        var birthday = (BirthDay)bday;

        birthday.Value.ShouldBe(bday);
    }

    [Fact]
    public void ExplicitOperator_ShouldThrow_WhenValueIsInvalid()
    {
        DateOnly invalid = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        Action act = () => { var _ = (BirthDay)invalid; };

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("Verjaardag ligt in de toekomst.");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForSameValue()
    {
        DateOnly bday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var a = BirthDay.Create(bday).Value;
        var b = BirthDay.Create(bday).Value;

        var areEqual = a.Equals(b);

        areEqual.ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var bday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var bio = BirthDay.Create(bday).Value;

        var result = bio.ToString();

        result.ShouldBe(bday.ToShortDateString());
    }
}
