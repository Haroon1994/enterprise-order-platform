using EnterpriseOrderPlatform.BuildingBlocks;
using Xunit;

namespace EnterpriseOrderPlatform.UnitTests;

public class ResultTests
{
    private static readonly ResultError SampleError = new("sample.error", "Something went wrong.");

    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(ResultError.None, result.Error);
    }

    [Fact]
    public void Failure_CarriesTheError()
    {
        var result = Result.Failure(SampleError);

        Assert.True(result.IsFailure);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void Failure_WithoutError_IsRejected()
    {
        Assert.Throws<InvalidOperationException>(() => Result.Failure(ResultError.None));
    }

    [Fact]
    public void GenericSuccess_ExposesValue()
    {
        var result = Result.Success(42);

        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GenericFailure_ThrowsWhenReadingValue()
    {
        var result = Result.Failure<int>(SampleError);

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
