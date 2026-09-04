using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace AbpGuessGame.Domain.Tests;

/// <summary>
/// Unit tests for BinarySearchBotService.
/// Tests the binary search algorithm across all secrets in [1, 43].
/// Verifies that the algorithm finds the secret and returns a valid guess count.
/// </summary>
public class BinarySearchBotServiceTests
{
    private readonly BinarySearchBotService _botService = new();

    [Theory]
    [InlineData(1)]
    [InlineData(22)]
    [InlineData(43)]
    public void ComputeGuessCount_WithValidSecret_ShouldReturnValidCount(int secret)
    {
        // Act
        var count = _botService.ComputeGuessCount(secret);

        // Assert
        count.ShouldBeGreaterThan(0);
        count.ShouldBeLessThanOrEqualTo(7); // Max guesses for binary search on [1, 43] is ~6
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(44)]
    public void ComputeGuessCount_WithInvalidSecret_ShouldThrow(int invalidSecret)
    {
        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() =>
            _botService.ComputeGuessCount(invalidSecret));
    }

    [Fact]
    public void ComputeGuessCount_AllSecrets1To43_ShouldTerminate()
    {
        // Act & Assert: verify bot always terminates and finds secret for all values in range
        for (int secret = 1; secret <= 43; secret++)
        {
            var count = _botService.ComputeGuessCount(secret);
            count.ShouldBeGreaterThan(0);
            count.ShouldBeLessThanOrEqualTo(7);
        }
    }

    [Fact]
    public void ComputeWithPath_Secret22_ShouldReturnPath()
    {
        // Act
        var result = _botService.ComputeWithPath(22);

        // Assert
        result.GuessCount.ShouldBeGreaterThan(0);
        result.Guesspath.ShouldNotBeEmpty();
        result.Guesspath.Count.ShouldBe(result.GuessCount);
        result.Guesspath[result.Guesspath.Count - 1].ShouldBe(22); // Last guess is the secret
    }

    [Fact]
    public void ComputeWithPath_Secret1_ShouldFindSecret()
    {
        // Act
        var result = _botService.ComputeWithPath(1);

        // Assert
        result.GuessCount.ShouldBeGreaterThan(0);
        result.Guesspath[result.Guesspath.Count - 1].ShouldBe(1);
    }

    [Fact]
    public void ComputeWithPath_Secret43_ShouldFindSecret()
    {
        // Act
        var result = _botService.ComputeWithPath(43);

        // Assert
        result.GuessCount.ShouldBeGreaterThan(0);
        result.Guesspath[result.Guesspath.Count - 1].ShouldBe(43);
    }

    [Fact]
    public void ComputeGuessCount_Secret22_ShouldMatchKnownValue()
    {
        // Binary search for 22 in [1, 43]: mid=22 at first guess, so count=1
        // Act
        var count = _botService.ComputeGuessCount(22);

        // Assert
        count.ShouldBe(1); // First guess hits 22
    }
}
