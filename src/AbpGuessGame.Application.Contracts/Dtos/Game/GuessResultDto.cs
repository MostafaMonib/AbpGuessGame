using System;

namespace AbpGuessGame.Dtos;

/// <summary>
/// Result DTO after recording a guess.
/// SecretNumber and BotGuessCount are only included when game is won.
/// </summary>
public class GuessResultDto
{
    public Guid GameId { get; set; }

    public int GuessCount { get; set; }

    public Hint Hint { get; set; }

    /// <summary>
    /// Secret number. Only included when Status == Won.
    /// </summary>
    public int? SecretNumber { get; set; }

    public GameStatus Status { get; set; }

    /// <summary>
    /// Binary-search bot guess count. Only included when Status == Won.
    /// </summary>
    public int? BotGuessCount { get; set; }

    /// <summary>
    /// Whether the player beat the bot (player guess count less than bot guess count).
    /// Only included when Status == Won.
    /// </summary>
    public bool? BeatTheBot { get; set; }

    /// <summary>
    /// If true, the value was already guessed in this game.
    /// GuessCount was not incremented, and no new Guess row was inserted.
    /// The hint returned is from the original guess.
    /// </summary>
    public bool AlreadyGuessed { get; set; }

    /// <summary>
    /// Updated BestGuessCount after this guess (if it was a win and a new best).
    /// Null if no best score yet or if this guess didn't set a new best.
    /// </summary>
    public int? UpdatedBestGuessCount { get; set; }
}
