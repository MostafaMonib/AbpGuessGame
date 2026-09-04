using System;

namespace AbpGuessGame.Dtos;

/// <summary>
/// Game DTO returned to client. SecretNumber is null while Status == InProgress.
/// </summary>
public class GameDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// Secret number. Only revealed when Status == Won.
    /// </summary>
    public int? SecretNumber { get; set; }

    public int GuessCount { get; set; }

    public GameStatus Status { get; set; }

    /// <summary>
    /// Binary-search bot guess count for the same secret.
    /// Only revealed when game is won.
    /// </summary>
    public int? BotGuessCount { get; set; }

    public DateTime CreationTime { get; set; }
}
