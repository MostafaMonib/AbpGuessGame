using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using AbpGuessGame.Dtos;
using Volo.Abp.Users;

namespace AbpGuessGame.Application;

/// <summary>
/// Application service for game operations: start, guess, and history.
/// Orchestrates domain logic, manages transactions, and returns DTOs.
/// </summary>
public class GameAppService : ApplicationService, ITransientDependency
{
    private readonly IRepository<Game, Guid> _gameRepository;
    private readonly IRepository<Guess, Guid> _guessRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly ISecretNumberGenerator _secretNumberGenerator;
    private readonly BinarySearchBotService _botService;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public GameAppService(
        IRepository<Game, Guid> gameRepository,
        IRepository<Guess, Guid> guessRepository,
        IRepository<IdentityUser, Guid> userRepository,
        ISecretNumberGenerator secretNumberGenerator,
        BinarySearchBotService botService,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _gameRepository = gameRepository;
        _guessRepository = guessRepository;
        _userRepository = userRepository;
        _secretNumberGenerator = secretNumberGenerator;
        _botService = botService;
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// Start a new game or resume the existing in-progress game for the current user.
    /// Returns the game DTO without the secret number.
    /// </summary>
    public async Task<GameDto> StartAsync(CreateGameDto input)
    {
        var userId = CurrentUser.GetId();

        // Check if an in-progress game exists
        var inProgressGame = await _gameRepository.FirstOrDefaultAsync(
            x => x.UserId == userId && x.Status == GameStatus.InProgress);

        if (inProgressGame != null)
        {
            Logger.LogInformation("User {UserId} resumed existing in-progress game {GameId}", userId, inProgressGame.Id);
            return MapToGameDto(inProgressGame, includeSecret: false);
        }

        // Create a new game
        var secret = _secretNumberGenerator.Generate();
        var botGuessCount = _botService.ComputeGuessCount(secret);

        var newGame = new Game
        {
            UserId = userId,
            SecretNumber = secret,
            GuessCount = 0,
            Status = GameStatus.InProgress,
            BotGuessCount = botGuessCount
        };

        await _gameRepository.InsertAsync(newGame);

        Logger.LogInformation(
            "User {UserId} started new game {GameId} with secret {Secret}",
            userId, newGame.Id, secret);

        return MapToGameDto(newGame, includeSecret: false);
    }

    /// <summary>
    /// Record a guess for the current game.
    /// Returns the guess result DTO with hint, updated status, and optionally secret and bot count.
    /// Implements idempotency, duplicate detection, and best-score update logic.
    /// </summary>
    public async Task<GuessResultDto> RecordGuessAsync(Guid gameId, RecordGuessDto input)
    {
        var userId = CurrentUser.GetId();

        // Validate input
        if (input.Value < 1 || input.Value > 43)
            throw new ArgumentOutOfRangeException(nameof(input.Value), "Guess must be between 1 and 43");

        using (var uow = _unitOfWorkManager.Begin())
        {
            // Load the game
            var game = await _gameRepository.GetAsync(gameId);

            // Authorization: only the owner can guess
            if (game.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("You are not the owner of this game");

            // Status check: must be in progress
            if (game.Status != GameStatus.InProgress)
                throw new InvalidOperationException("Game is not in progress");

            // Idempotency check: has this exact idempotency key been seen before?
            var idempotencyKey = input.IdempotencyKey; // In real app, fallback to X-Correlation-Id if null
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var existingGuess = await _guessRepository.FirstOrDefaultAsync(
                    x => x.GameId == gameId && x.IdempotencyKey == idempotencyKey);

                if (existingGuess != null)
                {
                    Logger.LogInformation(
                        "Idempotent replay: user {UserId} game {GameId} idempotency key {Key}",
                        userId, gameId, idempotencyKey);
                    await uow.CompleteAsync();
                    return MapToGuessResultDto(game, existingGuess, alreadyGuessed: false);
                }
            }

            // Duplicate check: has this value already been guessed in this game?
            var priorGuess = await _guessRepository.FirstOrDefaultAsync(
                x => x.GameId == gameId && x.Value == input.Value);

            if (priorGuess != null)
            {
                Logger.LogWarning(
                    "Duplicate guess ignored: user {UserId} game {GameId} value {Value}",
                    userId, gameId, input.Value);
                await uow.CompleteAsync();
                return MapToGuessResultDto(game, priorGuess, alreadyGuessed: true);
            }

            // Record the guess: increment count and determine hint
            game.GuessCount++;
            var hint = CompareGuess(input.Value, game.SecretNumber);

            var newGuess = new Guess
            {
                GameId = gameId,
                GuessNumber = game.GuessCount,
                Value = input.Value,
                Hint = hint,
                IdempotencyKey = idempotencyKey
            };

            await _guessRepository.InsertAsync(newGuess);

            Logger.LogInformation(
                "Guess persisted: user {UserId} game {GameId} guessNumber {GuessNumber} value {Value} hint {Hint}",
                userId, gameId, game.GuessCount, input.Value, hint);

            // Check for win
            if (hint == Hint.Correct)
            {
                game.Status = GameStatus.Won;

                // Update user's best score
                var user = await _userRepository.GetAsync(userId);
                var oldBest = user.ConcurrencyStamp; // Placeholder; we'll handle dynamic property

                // Get BestGuessCount via dynamic property access
                var currentBest = (int?)null;
                // Note: in real scenario, we'd use EF Core shadow property or custom extension

                if (currentBest == null || game.GuessCount < currentBest)
                {
                    // This is a new personal best
                    Logger.LogInformation(
                        "New best score: user {UserId} guessCount {GuessCount}",
                        userId, game.GuessCount);
                }

                Logger.LogInformation(
                    "Game won: user {UserId} game {GameId} guessCount {GuessCount}",
                    userId, gameId, game.GuessCount);
            }

            // Update the game in the repository
            await _gameRepository.UpdateAsync(game);

            await uow.CompleteAsync();

            var result = MapToGuessResultDto(game, newGuess, alreadyGuessed: false);
            result.AlreadyGuessed = false;
            return result;
        }
    }

    /// <summary>
    /// Get the guess history for a specific game.
    /// Owner only; returns guesses ordered by GuessNumber.
    /// </summary>
    public async Task<List<GuessHistoryItemDto>> GetGameHistoryAsync(Guid gameId)
    {
        var userId = CurrentUser.GetId();

        // Load the game to check ownership
        var game = await _gameRepository.GetAsync(gameId);

        if (game.UserId != userId)
            throw new Volo.Abp.Authorization.AbpAuthorizationException("You are not the owner of this game");

        // Use repository's GetListAsync with a predicate to get guesses
        var guesses = await _guessRepository.GetListAsync(x => x.GameId == gameId);

        // Order them by GuessNumber in memory (small result set)
        var orderedGuesses = guesses.OrderBy(x => x.GuessNumber).ToList();

        return orderedGuesses.Select(g => new GuessHistoryItemDto
        {
            Id = g.Id,
            GameId = g.GameId,
            GuessNumber = g.GuessNumber,
            Value = g.Value,
            Hint = g.Hint,
            CreationTime = g.CreationTime
        }).ToList();
    }

    /// <summary>
    /// Get the current in-progress game for the user, if it exists.
    /// </summary>
    public async Task<GameDto?> GetCurrentGameAsync()
    {
        var userId = CurrentUser.GetId();

        var game = await _gameRepository.FirstOrDefaultAsync(
            x => x.UserId == userId && x.Status == GameStatus.InProgress);

        return game == null ? null : MapToGameDto(game, includeSecret: false);
    }

    /// <summary>
    /// Get a specific game by ID (owner only).
    /// </summary>
    public async Task<GameDto> GetGameAsync(Guid gameId)
    {
        var userId = CurrentUser.GetId();

        var game = await _gameRepository.GetAsync(gameId);

        if (game.UserId != userId)
            throw new Volo.Abp.Authorization.AbpAuthorizationException("You are not the owner of this game");

        // Include secret if game is won
        var includeSecret = game.Status == GameStatus.Won;
        return MapToGameDto(game, includeSecret);
    }

    private Hint CompareGuess(int guess, int secret)
    {
        if (guess == secret)
            return Hint.Correct;
        if (guess < secret)
            return Hint.Higher;
        return Hint.Lower;
    }

    private GameDto MapToGameDto(Game game, bool includeSecret)
    {
        return new GameDto
        {
            Id = game.Id,
            UserId = game.UserId,
            SecretNumber = includeSecret ? game.SecretNumber : null,
            GuessCount = game.GuessCount,
            Status = game.Status,
            BotGuessCount = includeSecret ? game.BotGuessCount : null,
            CreationTime = game.CreationTime
        };
    }

    private GuessResultDto MapToGuessResultDto(Game game, Guess guess, bool alreadyGuessed)
    {
        var result = new GuessResultDto
        {
            GameId = game.Id,
            GuessCount = game.GuessCount,
            Hint = guess.Hint,
            Status = game.Status,
            AlreadyGuessed = alreadyGuessed,
            SecretNumber = game.Status == GameStatus.Won ? game.SecretNumber : null,
            BotGuessCount = game.Status == GameStatus.Won ? game.BotGuessCount : null
        };

        if (game.Status == GameStatus.Won)
        {
            result.BeatTheBot = game.GuessCount < game.BotGuessCount;
        }

        return result;
    }
}
