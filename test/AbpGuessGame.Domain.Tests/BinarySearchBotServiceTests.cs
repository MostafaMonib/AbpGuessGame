using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace AbpGuessGame.Domain.Tests;

/// <summary>
/// Unit tests for BinarySearchBotService.
/// Per DOCUMENTATION.md §16.3:
///   - Table-driven tests for boundaries (1, 22, 43)
///   - Theory/InlineData for all 43 secrets: bot always terminates, last guess equals secret
///   - Path consistency: ComputeGuessCount and ComputeWithPath return same count
///   - Invalid secrets throw ArgumentOutOfRangeException
/// </summary>
public class BinarySearchBotServiceTests
{
    private readonly BinarySearchBotService _botService = new();

    // ───────────────────────────────────────
    // Boundary tests
    // ───────────────────────────────────────

    [Fact]
    public void ComputeGuessCount_Secret22_ShouldBe1()
    {
        // Binary search for 22 in [1, 43]: mid = (1+43)/2 = 22 on first guess
        var count = _botService.ComputeGuessCount(22);
        count.ShouldBe(1);
    }

    [Fact]
    public void ComputeWithPath_Secret1_ShouldFindSecret()
    {
        var result = _botService.ComputeWithPath(1);

        result.GuessCount.ShouldBeGreaterThan(0);
        result.Guesspath.ShouldNotBeEmpty();
        result.Guesspath.Last().ShouldBe(1);
    }

    [Fact]
    public void ComputeWithPath_Secret43_ShouldFindSecret()
    {
        var result = _botService.ComputeWithPath(43);

        result.GuessCount.ShouldBeGreaterThan(0);
        result.Guesspath.Last().ShouldBe(43);
    }

    [Fact]
    public void ComputeWithPath_Secret22_ShouldReturnPathWithSingleElement()
    {
        var result = _botService.ComputeWithPath(22);

        result.GuessCount.ShouldBe(1);
        result.Guesspath.Count.ShouldBe(1);
        result.Guesspath[0].ShouldBe(22);
    }

    // ───────────────────────────────────────
    // All 43 secrets: bot must always terminate and find the secret
    // ───────────────────────────────────────

    [Theory]
    [InlineData(1)]  [InlineData(2)]  [InlineData(3)]  [InlineData(4)]  [InlineData(5)]
    [InlineData(6)]  [InlineData(7)]  [InlineData(8)]  [InlineData(9)]  [InlineData(10)]
    [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
    [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
    [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)]
    [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
    [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)] [InlineData(35)]
    [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)] [InlineData(40)]
    [InlineData(41)] [InlineData(42)] [InlineData(43)]
    public void ComputeWithPath_AllSecrets_ShouldTerminateAndFindSecret(int secret)
    {
        var result = _botService.ComputeWithPath(secret);

        // Bot always terminates with count >= 1
        result.GuessCount.ShouldBeGreaterThanOrEqualTo(1);

        // Path length must match count
        result.Guesspath.Count.ShouldBe(result.GuessCount);

        // Last guess in path must be the secret
        result.Guesspath.Last().ShouldBe(secret);

        // For [1, 43], binary search takes at most 6 guesses (ceil(log2(43)) = 6)
        result.GuessCount.ShouldBeLessThanOrEqualTo(6);
    }

    // ───────────────────────────────────────
    // Path consistency: ComputeGuessCount and ComputeWithPath agree
    // ───────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(22)]
    [InlineData(33)]
    [InlineData(43)]
    public void ComputeGuessCount_And_ComputeWithPath_ShouldReturnSameCount(int secret)
    {
        var count = _botService.ComputeGuessCount(secret);
        var result = _botService.ComputeWithPath(secret);

        result.GuessCount.ShouldBe(count);
    }

    // ───────────────────────────────────────
    // Invalid secrets
    // ───────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(44)]
    [InlineData(100)]
    public void ComputeGuessCount_WithInvalidSecret_ShouldThrow(int invalidSecret)
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            _botService.ComputeGuessCount(invalidSecret));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(44)]
    public void ComputeWithPath_WithInvalidSecret_ShouldThrow(int invalidSecret)
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            _botService.ComputeWithPath(invalidSecret));
    }

    // ───────────────────────────────────────
    // All path elements must be within [1, 43]
    // ───────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(22)]
    [InlineData(43)]
    public void ComputeWithPath_AllGuessesInPath_ShouldBeInRange(int secret)
    {
        var result = _botService.ComputeWithPath(secret);

        foreach (var guess in result.Guesspath)
        {
            guess.ShouldBeGreaterThanOrEqualTo(1);
            guess.ShouldBeLessThanOrEqualTo(43);
        }
    }
}
