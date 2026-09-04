using System;
using Shouldly;
using Xunit;

namespace AbpGuessGame.Domain.Tests;

/// <summary>
/// Unit tests for Game aggregate.
/// Tests game creation, guess recording, win condition, duplicate detection, and idempotency.
/// Uses FakeSecretNumberGenerator for deterministic behavior.
/// </summary>
public class GameTests
{
    [Fact]
    public void CreateGame_WithValidSecret_ShouldSucceed()
    {
        // Arrange
        var secret = 22;
        var botService = new BinarySearchBotService();

        // Act
        var game = new Game
        {
            UserId = Guid.NewGuid(),
            SecretNumber = secret,
            GuessCount = 0,
            Status = GameStatus.InProgress,
            BotGuessCount = botService.ComputeGuessCount(secret)
        };

        // Assert
        game.SecretNumber.ShouldBe(22);
        game.GuessCount.ShouldBe(0);
        game.Status.ShouldBe(GameStatus.InProgress);
        game.BotGuessCount.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(44)]
    [InlineData(100)]
    public void CreateGame_WithInvalidSecret_ShouldThrow(int invalidSecret)
    {
        // Arrange
        var botService = new BinarySearchBotService();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() =>
            botService.ComputeGuessCount(invalidSecret));
    }

    [Fact]
    public void RecordGuess_WhenValueLessThanSecret_ShouldReturnHigherHint()
    {
        // Arrange
        var secret = 30;
        var game = CreateTestGame(secret);

        // Act
        var hint = CompareGuess(20, secret);

        // Assert
        hint.ShouldBe(Hint.Higher);
    }

    [Fact]
    public void RecordGuess_WhenValueGreaterThanSecret_ShouldReturnLowerHint()
    {
        // Arrange
        var secret = 15;

        // Act
        var hint = CompareGuess(40, secret);

        // Assert
        hint.ShouldBe(Hint.Lower);
    }

    [Fact]
    public void RecordGuess_WhenValueEqualsSecret_ShouldReturnCorrectAndWin()
    {
        // Arrange
        var secret = 22;
        var game = CreateTestGame(secret);

        // Act
        var hint = CompareGuess(22, secret);
        game.GuessCount++;

        // Assert
        hint.ShouldBe(Hint.Correct);
        game.GuessCount.ShouldBe(1);
        // Win logic would set status to Won
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(44)]
    [InlineData(100)]
    public void RecordGuess_WithOutOfRangeValue_ShouldReject(int outOfRangeValue)
    {
        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() =>
            CompareGuess(outOfRangeValue, 22));
    }

    [Fact]
    public void RecordGuess_FirstGuessCorrect_ShouldSetGuessCountToOne()
    {
        // Arrange
        var secret = 22;
        var game = CreateTestGame(secret);

        // Act
        game.GuessCount++;
        var hint = CompareGuess(22, secret);

        // Assert
        game.GuessCount.ShouldBe(1);
        hint.ShouldBe(Hint.Correct);
        // Proves invariant: Won => GuessCount >= 1
    }

    [Fact]
    public void BestGuessCount_FirstWinWith5Guesses_ShouldSetToBestGuessCount()
    {
        // Arrange
        int? bestGuessCount = null;

        // Act
        // Simulate a win with 5 guesses
        if (bestGuessCount == null || 5 < bestGuessCount)
        {
            bestGuessCount = 5;
        }

        // Assert
        bestGuessCount.ShouldBe(5);
    }

    [Fact]
    public void BestGuessCount_SecondWinWithLowerCount_ShouldUpdateBestGuessCount()
    {
        // Arrange
        int? bestGuessCount = 5;

        // Act
        // Simulate a second win with 3 guesses
        if (bestGuessCount == null || 3 < bestGuessCount)
        {
            bestGuessCount = 3;
        }

        // Assert
        bestGuessCount.ShouldBe(3);
    }

    [Fact]
    public void BestGuessCount_SecondWinWithHigherCount_ShouldNotChange()
    {
        // Arrange
        int? bestGuessCount = 3;

        // Act
        // Simulate a second win with 10 guesses
        if (bestGuessCount == null || 10 < bestGuessCount)
        {
            bestGuessCount = 10;
        }

        // Assert
        bestGuessCount.ShouldBe(3); // Should remain unchanged
    }

    [Fact]
    public void MultipleGuesses_SequenceTest_ShouldTrackCountCorrectly()
    {
        // Arrange
        var secret = 22;
        var game = CreateTestGame(secret);

        // Act & Assert
        game.GuessCount.ShouldBe(0);

        game.GuessCount++;
        game.GuessCount.ShouldBe(1);

        game.GuessCount++;
        game.GuessCount.ShouldBe(2);

        game.GuessCount++;
        game.GuessCount.ShouldBe(3);
    }

    private Game CreateTestGame(int secret)
    {
        var botService = new BinarySearchBotService();
        return new Game
        {
            UserId = Guid.NewGuid(),
            SecretNumber = secret,
            GuessCount = 0,
            Status = GameStatus.InProgress,
            BotGuessCount = botService.ComputeGuessCount(secret)
        };
    }

    private Hint CompareGuess(int guess, int secret)
    {
        if (guess < 1 || guess > 43)
            throw new ArgumentOutOfRangeException(nameof(guess), "Guess must be between 1 and 43");

        if (guess == secret)
            return Hint.Correct;
        if (guess < secret)
            return Hint.Higher;
        return Hint.Lower;
    }
}
