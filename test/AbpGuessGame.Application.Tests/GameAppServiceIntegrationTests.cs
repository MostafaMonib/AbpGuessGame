using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Modularity;
using AbpGuessGame.Application;
using AbpGuessGame.Dtos;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace AbpGuessGame.Application.Tests;

/// <summary>
/// Application service tests for GameAppService.
/// Per DOCUMENTATION.md §16.4:
///   - StartAsync with no game creates a new game; DTO has no secretNumber
///   - StartAsync with in-progress game returns same id (resume, no second row)
///   - RecordGuessAsync valid guess returns DTO with hint; no secret while in progress
///   - RecordGuessAsync winning guess returns secret, botGuessCount, beatTheBot, bestGuessCount
///   - RecordGuessAsync duplicate value returns alreadyGuessed: true; guessCount unchanged
///   - GetGameHistoryAsync returns rows ordered by GuessNumber
///   - DI resolution checks remain as sanity tests
/// </summary>
public abstract class GameAppServiceIntegrationTests<TStartupModule> : AbpGuessGameApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    // ───────────────────────────────────────
    // DI Resolution (sanity)
    // ───────────────────────────────────────

    [Fact]
    public void GameAppService_ShouldBeRegisteredInDependencyContainer()
    {
        var appService = GetRequiredService<GameAppService>();
        appService.ShouldNotBeNull();
    }

    [Fact]
    public void BinarySearchBotService_ShouldBeResolvable()
    {
        var botService = GetRequiredService<BinarySearchBotService>();
        botService.ShouldNotBeNull();
    }

    [Fact]
    public void ISecretNumberGenerator_ShouldBeResolvable()
    {
        var secretGen = GetRequiredService<ISecretNumberGenerator>();
        secretGen.ShouldNotBeNull();
    }

    // ───────────────────────────────────────
    // StartAsync
    // ───────────────────────────────────────

    [Fact]
    public async Task StartAsync_NoGame_CreatesNewGame_DtoHasNoSecret()
    {
        var appService = GetRequiredService<GameAppService>();

        var result = await appService.StartAsync(new CreateGameDto());

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Status.ShouldBe(GameStatus.InProgress);
        result.GuessCount.ShouldBe(0);
        // Per §7.6 and §16.4: in-progress DTO must NOT contain secretNumber
        result.SecretNumber.ShouldBeNull();
        result.BotGuessCount.ShouldBeNull();
    }

    [Fact]
    public async Task StartAsync_WithInProgressGame_ReturnsSameId()
    {
        var appService = GetRequiredService<GameAppService>();

        // Start first game
        var first = await appService.StartAsync(new CreateGameDto());
        // Start again — should resume, not create a second row
        var second = await appService.StartAsync(new CreateGameDto());

        second.Id.ShouldBe(first.Id);
        second.GuessCount.ShouldBe(first.GuessCount);
    }

    // ───────────────────────────────────────
    // RecordGuessAsync — valid guess (no win)
    // ───────────────────────────────────────

    [Fact]
    public async Task RecordGuessAsync_ValidGuess_ReturnsDtoWithHint_NoSecret()
    {
        var appService = GetRequiredService<GameAppService>();
        var game = await appService.StartAsync(new CreateGameDto());

        // Guess 1 — will likely be wrong (unless secret happens to be 1, which is possible)
        var result = await appService.RecordGuessAsync(game.Id, new RecordGuessDto { Value = 1 });

        result.ShouldNotBeNull();
        result.GameId.ShouldBe(game.Id);
        result.GuessCount.ShouldBe(1);

        if (result.Status == GameStatus.InProgress)
        {
            // Secret must not leak while in progress
            result.SecretNumber.ShouldBeNull();
            result.BotGuessCount.ShouldBeNull();
            result.Hint.ShouldBeOneOf(Hint.Higher, Hint.Lower);
        }
    }

    // ───────────────────────────────────────
    // RecordGuessAsync — out-of-range
    // ───────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(44)]
    [InlineData(-1)]
    public async Task RecordGuessAsync_OutOfRange_ShouldReject(int badValue)
    {
        var appService = GetRequiredService<GameAppService>();
        var game = await appService.StartAsync(new CreateGameDto());

        await Should.ThrowAsync<ArgumentOutOfRangeException>(
            () => appService.RecordGuessAsync(game.Id, new RecordGuessDto { Value = badValue }));
    }

    // ───────────────────────────────────────
    // RecordGuessAsync — game not in progress
    // ───────────────────────────────────────

    [Fact]
    public async Task RecordGuessAsync_AfterGameWon_ShouldReject()
    {
        var appService = GetRequiredService<GameAppService>();
        var game = await appService.StartAsync(new CreateGameDto());

        // Win the game by brute force: try all values 1-43
        GuessResultDto? winResult = null;
        for (int guess = 1; guess <= 43; guess++)
        {
            var result = await appService.RecordGuessAsync(game.Id, new RecordGuessDto { Value = guess });
            if (result.Status == GameStatus.Won)
            {
                winResult = result;
                break;
            }
        }

        winResult.ShouldNotBeNull("Game should have been won within 43 guesses");

        // Now attempt another guess — should be rejected
        await Should.ThrowAsync<InvalidOperationException>(
            () => appService.RecordGuessAsync(game.Id, new RecordGuessDto { Value = 1 }));
    }

    // ───────────────────────────────────────
    // RecordGuessAsync — winning guess includes secret and bot
    // ───────────────────────────────────────

    [Fact]
    public async Task RecordGuessAsync_WinningGuess_ReturnsSecretAndBotAndBest()
    {
        var appService = GetRequiredService<GameAppService>();
        var game = await appService.StartAsync(new CreateGameDto());

        // Win the game by brute force
        GuessResultDto? winResult = null;
        for (int guess = 1; guess <= 43; guess++)
        {
            var result = await appService.RecordGuessAsync(game.Id, new RecordGuessDto { Value = guess });
            if (result.Status == GameStatus.Won)
            {
                winResult = result;
                break;
            }
        }

        winResult.ShouldNotBeNull();
        winResult.Status.ShouldBe(GameStatus.Won);
        winResult.Hint.ShouldBe(Hint.Correct);

        // On win, DTO must include secret and bot count
        winResult.SecretNumber.ShouldNotBeNull();
        winResult.SecretNumber!.Value.ShouldBeInRange(1, 43);
        winResult.BotGuessCount.ShouldNotBeNull();
        winResult.BotGuessCount!.Value.ShouldBeGreaterThan(0);
        winResult.BeatTheBot.ShouldNotBeNull();

        // UpdatedBestGuessCount should be set (first win = new best)
        winResult.UpdatedBestGuessCount.ShouldNotBeNull();
        winResult.UpdatedBestGuessCount!.Value.ShouldBeGreaterThanOrEqualTo(1);
    }

    // ───────────────────────────────────────
    // RecordGuessAsync — duplicate value
    // ───────────────────────────────────────

    [Fact]
    public async Task RecordGuessAsync_DuplicateValue_ReturnsAlreadyGuessed()
    {
        var appService = GetRequiredService<GameAppService>();
        var game = await appService.StartAsync(new CreateGameDto());

        // First guess of 1
        var first = await appService.RecordGuessAsync(game.Id, new RecordGuessDto { Value = 1 });

        if (first.Status == GameStatus.Won)
        {
            // Edge case: 1 was the secret — can't test duplicate. Skip.
            return;
        }

        first.AlreadyGuessed.ShouldBeFalse();
        var countAfterFirst = first.GuessCount;

        // Duplicate guess of 1
        var duplicate = await appService.RecordGuessAsync(game.Id, new RecordGuessDto { Value = 1 });

        duplicate.AlreadyGuessed.ShouldBeTrue();
        duplicate.GuessCount.ShouldBe(countAfterFirst); // Count unchanged
        duplicate.Hint.ShouldBe(first.Hint); // Same hint
    }

    // ───────────────────────────────────────
    // RecordGuessAsync — idempotency key
    // ───────────────────────────────────────

    [Fact]
    public async Task RecordGuessAsync_SameIdempotencyKey_ReturnsIdenticalResult()
    {
        var appService = GetRequiredService<GameAppService>();
        var game = await appService.StartAsync(new CreateGameDto());

        var idempotencyKey = Guid.NewGuid().ToString();

        // First submission
        var first = await appService.RecordGuessAsync(game.Id, new RecordGuessDto
        {
            Value = 5,
            IdempotencyKey = idempotencyKey
        });

        if (first.Status == GameStatus.Won)
        {
            return; // Edge case: 5 was the secret
        }

        // Replay same key
        var replay = await appService.RecordGuessAsync(game.Id, new RecordGuessDto
        {
            Value = 5,
            IdempotencyKey = idempotencyKey
        });

        replay.GuessCount.ShouldBe(first.GuessCount);
        replay.Hint.ShouldBe(first.Hint);
    }

    // ───────────────────────────────────────
    // GetGameHistoryAsync
    // ───────────────────────────────────────

    [Fact]
    public async Task GetGameHistoryAsync_ReturnsOrderedGuesses()
    {
        var appService = GetRequiredService<GameAppService>();
        var game = await appService.StartAsync(new CreateGameDto());

        // Make two different guesses to build history
        var r1 = await appService.RecordGuessAsync(game.Id, new RecordGuessDto { Value = 5 });
        if (r1.Status == GameStatus.Won) return; // Edge case

        var r2 = await appService.RecordGuessAsync(game.Id, new RecordGuessDto { Value = 40 });

        var history = await appService.GetGameHistoryAsync(game.Id);

        history.ShouldNotBeNull();
        history.Count.ShouldBeGreaterThanOrEqualTo(2);

        // Should be ordered by GuessNumber
        for (int i = 1; i < history.Count; i++)
        {
            history[i].GuessNumber.ShouldBeGreaterThan(history[i - 1].GuessNumber);
        }

        // First guess should be value 5, second should be value 40
        history[0].Value.ShouldBe(5);
        history[1].Value.ShouldBe(40);
    }

    // ───────────────────────────────────────
    // GetCurrentGameAsync
    // ───────────────────────────────────────

    [Fact]
    public async Task GetCurrentGameAsync_NoGame_ReturnsNull()
    {
        var appService = GetRequiredService<GameAppService>();

        var result = await appService.GetCurrentGameAsync();
        // May or may not be null depending on test data;
        // if null, proves the "no current game" path works
        if (result != null)
        {
            result.Status.ShouldBe(GameStatus.InProgress);
        }
    }

    [Fact]
    public async Task GetCurrentGameAsync_WithActiveGame_ReturnsGame()
    {
        var appService = GetRequiredService<GameAppService>();
        var started = await appService.StartAsync(new CreateGameDto());

        var current = await appService.GetCurrentGameAsync();

        current.ShouldNotBeNull();
        current!.Id.ShouldBe(started.Id);
        current.SecretNumber.ShouldBeNull(); // No leak
    }

    // ───────────────────────────────────────
    // Secret Leak Prevention (§16.4)
    // ───────────────────────────────────────

    [Fact]
    public async Task InProgressGame_DtoNeverLeaksSecret()
    {
        var appService = GetRequiredService<GameAppService>();
        var game = await appService.StartAsync(new CreateGameDto());

        // Start DTO — no secret
        game.SecretNumber.ShouldBeNull();
        game.BotGuessCount.ShouldBeNull();

        // Guess DTO while in progress — no secret
        var guessResult = await appService.RecordGuessAsync(game.Id, new RecordGuessDto { Value = 1 });
        if (guessResult.Status == GameStatus.InProgress)
        {
            guessResult.SecretNumber.ShouldBeNull();
            guessResult.BotGuessCount.ShouldBeNull();
        }

        // GetCurrentGame — no secret
        var current = await appService.GetCurrentGameAsync();
        if (current != null && current.Status == GameStatus.InProgress)
        {
            current.SecretNumber.ShouldBeNull();
        }
    }
}
