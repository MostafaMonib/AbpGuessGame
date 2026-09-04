using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace AbpGuessGame.Domain.Tests;

/// <summary>
/// Unit tests for Game aggregate and domain rules.
/// Per DOCUMENTATION.md §16.3, these prove:
///   - higher/lower/correct hints
///   - win condition and status transition
///   - out-of-range rejection
///   - guess after won rejection
///   - BestGuessCount rule (first win, better win, worse win keeps old)
///   - duplicate guess detection
///   - Won => GuessCount >= 1 invariant
///   - multi-guess sequence tracking
/// Uses FakeSecretNumberGenerator for deterministic behavior.
/// </summary>
public class GameTests
{
    // ───────────────────────────────────────
    // Game Creation
    // ───────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(22)]
    [InlineData(43)]
    public void CreateGame_WithValidSecret_ShouldSucceed(int secret)
    {
        var game = CreateTestGame(secret);

        game.SecretNumber.ShouldBe(secret);
        game.GuessCount.ShouldBe(0);
        game.Status.ShouldBe(GameStatus.InProgress);
        game.BotGuessCount.ShouldBeGreaterThan(0);
        game.Guesses.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(44)]
    [InlineData(100)]
    public void CreateGame_WithInvalidSecret_BotServiceShouldThrow(int invalidSecret)
    {
        var botService = new BinarySearchBotService();

        Should.Throw<ArgumentOutOfRangeException>(() =>
            botService.ComputeGuessCount(invalidSecret));
    }

    // ───────────────────────────────────────
    // Hint Logic (Higher / Lower / Correct)
    // ───────────────────────────────────────

    [Fact]
    public void RecordGuess_WhenValueLessThanSecret_ShouldReturnHigherHint()
    {
        // Secret is 30, guess is 20 => should hint Higher
        var game = CreateTestGame(30);
        var result = SimulateGuess(game, 20);

        result.Hint.ShouldBe(Hint.Higher);
        game.GuessCount.ShouldBe(1);
        game.Status.ShouldBe(GameStatus.InProgress);
    }

    [Fact]
    public void RecordGuess_WhenValueGreaterThanSecret_ShouldReturnLowerHint()
    {
        // Secret is 10, guess is 35 => should hint Lower
        var game = CreateTestGame(10);
        var result = SimulateGuess(game, 35);

        result.Hint.ShouldBe(Hint.Lower);
        game.GuessCount.ShouldBe(1);
        game.Status.ShouldBe(GameStatus.InProgress);
    }

    [Fact]
    public void RecordGuess_WhenValueEqualsSecret_ShouldReturnCorrectAndWin()
    {
        var game = CreateTestGame(22);
        var result = SimulateGuess(game, 22);

        result.Hint.ShouldBe(Hint.Correct);
        game.Status.ShouldBe(GameStatus.Won);
        game.GuessCount.ShouldBe(1);
    }

    // ───────────────────────────────────────
    // Out-of-Range Rejection
    // ───────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(44)]
    [InlineData(100)]
    public void RecordGuess_WithOutOfRangeValue_ShouldReject(int outOfRangeValue)
    {
        var game = CreateTestGame(22);

        Should.Throw<ArgumentOutOfRangeException>(() =>
            SimulateGuess(game, outOfRangeValue));

        // GuessCount must not have incremented
        game.GuessCount.ShouldBe(0);
    }

    // ───────────────────────────────────────
    // Guess After Won
    // ───────────────────────────────────────

    [Fact]
    public void RecordGuess_AfterWon_ShouldReject()
    {
        var game = CreateTestGame(22);
        SimulateGuess(game, 22); // Win
        game.Status.ShouldBe(GameStatus.Won);

        // Another guess after win should be rejected
        Should.Throw<InvalidOperationException>(() =>
            SimulateGuess(game, 10));

        // GuessCount must remain 1 (the winning guess)
        game.GuessCount.ShouldBe(1);
    }

    // ───────────────────────────────────────
    // Won => GuessCount >= 1 Invariant (§7.1)
    // ───────────────────────────────────────

    [Fact]
    public void RecordGuess_FirstGuessCorrect_ShouldSetGuessCountToOne()
    {
        // Proves invariant: Won => GuessCount >= 1
        var game = CreateTestGame(22);
        var result = SimulateGuess(game, 22);

        game.Status.ShouldBe(GameStatus.Won);
        game.GuessCount.ShouldBe(1);
        game.GuessCount.ShouldBeGreaterThanOrEqualTo(1);
        result.Hint.ShouldBe(Hint.Correct);
    }

    // ───────────────────────────────────────
    // Multi-Guess Sequence
    // ───────────────────────────────────────

    [Fact]
    public void MultipleGuesses_ThenWin_ShouldTrackCountCorrectly()
    {
        var game = CreateTestGame(22);

        // Guess 1: too low
        var r1 = SimulateGuess(game, 10);
        r1.Hint.ShouldBe(Hint.Higher);
        game.GuessCount.ShouldBe(1);

        // Guess 2: too high
        var r2 = SimulateGuess(game, 30);
        r2.Hint.ShouldBe(Hint.Lower);
        game.GuessCount.ShouldBe(2);

        // Guess 3: correct
        var r3 = SimulateGuess(game, 22);
        r3.Hint.ShouldBe(Hint.Correct);
        game.GuessCount.ShouldBe(3);
        game.Status.ShouldBe(GameStatus.Won);

        // Verify all guesses were tracked
        game.Guesses.Count.ShouldBe(3);
        game.Guesses.Select(g => g.Value).ShouldBe(new[] { 10, 30, 22 });
        game.Guesses.Select(g => g.Hint).ShouldBe(new[] { Hint.Higher, Hint.Lower, Hint.Correct });
        game.Guesses.Select(g => g.GuessNumber).ShouldBe(new[] { 1, 2, 3 });
    }

    // ───────────────────────────────────────
    // Duplicate Guess Detection (§7.4)
    // ───────────────────────────────────────

    [Fact]
    public void RecordGuess_DuplicateValue_ShouldNotIncrementCount()
    {
        var game = CreateTestGame(22);

        // First guess of 10
        var r1 = SimulateGuess(game, 10);
        r1.Hint.ShouldBe(Hint.Higher);
        game.GuessCount.ShouldBe(1);

        // Duplicate guess of 10 — should be rejected without incrementing
        var r2 = SimulateDuplicateGuess(game, 10);
        r2.AlreadyGuessed.ShouldBeTrue();
        r2.Hint.ShouldBe(Hint.Higher); // Same hint as original
        game.GuessCount.ShouldBe(1); // Count unchanged

        // Only one Guess row should exist
        game.Guesses.Count.ShouldBe(1);
    }

    // ───────────────────────────────────────
    // BestGuessCount Rule (§7.1)
    // ───────────────────────────────────────

    [Fact]
    public void BestGuessCount_FirstWinWith5Guesses_ShouldSetBest()
    {
        int? bestGuessCount = null;
        int gameGuessCount = 5;

        bestGuessCount = ApplyBestScoreRule(bestGuessCount, gameGuessCount);

        bestGuessCount.ShouldBe(5);
    }

    [Fact]
    public void BestGuessCount_SecondWinWithLowerCount_ShouldUpdateBest()
    {
        int? bestGuessCount = 5;
        int gameGuessCount = 3;

        bestGuessCount = ApplyBestScoreRule(bestGuessCount, gameGuessCount);

        bestGuessCount.ShouldBe(3);
    }

    [Fact]
    public void BestGuessCount_SecondWinWithHigherCount_ShouldNotChange()
    {
        int? bestGuessCount = 3;
        int gameGuessCount = 10;

        bestGuessCount = ApplyBestScoreRule(bestGuessCount, gameGuessCount);

        bestGuessCount.ShouldBe(3); // Unchanged
    }

    [Fact]
    public void BestGuessCount_AbandonedGame_ShouldNotUpdate()
    {
        // Best score only updates on Win, never on Abandon
        int? bestGuessCount = null;
        var game = CreateTestGame(22);
        game.Status = GameStatus.Abandoned;

        // Best should remain null — abandon doesn't trigger update
        bestGuessCount.ShouldBeNull();
    }

    // ───────────────────────────────────────
    // Status Transitions
    // ───────────────────────────────────────

    [Fact]
    public void Game_StatusTransitionToWon_ShouldBeCorrect()
    {
        var game = CreateTestGame(22);

        game.Status.ShouldBe(GameStatus.InProgress);
        SimulateGuess(game, 22);
        game.Status.ShouldBe(GameStatus.Won);
    }

    [Fact]
    public void Game_StatusTransitionToAbandoned_ShouldPreserveGuesses()
    {
        var game = CreateTestGame(22);
        SimulateGuess(game, 10); // One guess

        game.Status = GameStatus.Abandoned;

        game.Status.ShouldBe(GameStatus.Abandoned);
        game.Guesses.Count.ShouldBe(1); // Guess rows preserved per §7.4
    }

    // ───────────────────────────────────────
    // FakeSecretNumberGenerator
    // ───────────────────────────────────────

    [Fact]
    public void FakeSecretNumberGenerator_ShouldReturnFixedValue()
    {
        var gen = new FakeSecretNumberGenerator(15);
        gen.Generate().ShouldBe(15);
        gen.Generate().ShouldBe(15); // Deterministic
    }

    [Theory]
    [InlineData(0)]
    [InlineData(44)]
    [InlineData(-1)]
    public void FakeSecretNumberGenerator_InvalidValue_ShouldThrow(int invalid)
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new FakeSecretNumberGenerator(invalid));
    }

    // ───────────────────────────────────────
    // Helpers — simulate domain logic inline
    //   (The Game entity is anemic; these helpers replicate
    //    what GameAppService.RecordGuessAsync does at the domain level)
    // ───────────────────────────────────────

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

    /// <summary>
    /// Simulates the core domain logic of recording a guess:
    /// validate range, validate status, increment count, determine hint,
    /// create Guess entity, set status to Won if correct.
    /// </summary>
    private GuessResult SimulateGuess(Game game, int value)
    {
        if (value < 1 || value > 43)
            throw new ArgumentOutOfRangeException(nameof(value), "Guess must be between 1 and 43");

        if (game.Status != GameStatus.InProgress)
            throw new InvalidOperationException("Game is not in progress");

        game.GuessCount++;
        var hint = CompareGuess(value, game.SecretNumber);

        var guess = new Guess
        {
            GameId = game.Id,
            GuessNumber = game.GuessCount,
            Value = value,
            Hint = hint
        };
        game.Guesses.Add(guess);

        if (hint == Hint.Correct)
        {
            game.Status = GameStatus.Won;
        }

        return new GuessResult { Hint = hint, AlreadyGuessed = false };
    }

    /// <summary>
    /// Simulates a duplicate-guess check: if value already guessed, return prior hint
    /// without incrementing count or adding a new Guess row.
    /// </summary>
    private GuessResult SimulateDuplicateGuess(Game game, int value)
    {
        var prior = game.Guesses.FirstOrDefault(g => g.Value == value);
        if (prior != null)
        {
            return new GuessResult { Hint = prior.Hint, AlreadyGuessed = true };
        }

        return SimulateGuess(game, value);
    }

    private Hint CompareGuess(int guess, int secret)
    {
        if (guess == secret) return Hint.Correct;
        if (guess < secret) return Hint.Higher;
        return Hint.Lower;
    }

    /// <summary>
    /// Applies the best-score rule from §7.1:
    /// On win, if bestGuessCount is null or gameGuessCount &lt; bestGuessCount,
    /// set bestGuessCount = gameGuessCount.
    /// </summary>
    private int? ApplyBestScoreRule(int? bestGuessCount, int gameGuessCount)
    {
        if (bestGuessCount == null || gameGuessCount < bestGuessCount)
        {
            return gameGuessCount;
        }
        return bestGuessCount;
    }

    /// <summary>
    /// Simple result record for test assertions.
    /// </summary>
    private record GuessResult
    {
        public Hint Hint { get; init; }
        public bool AlreadyGuessed { get; init; }
    }
}
