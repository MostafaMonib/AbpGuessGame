using System;

namespace AbpGuessGame.Dtos;

/// <summary>
/// DTO for a single guess in the game history.
/// This is the immutable guess log entry.
/// SecretNumber is never included (even for won games, to respect the history API contract).
/// </summary>
public class GuessHistoryItemDto
{
    public Guid Id { get; set; }

    public Guid GameId { get; set; }

    /// <summary>
    /// 1-based ordinal within the game.
    /// </summary>
    public int GuessNumber { get; set; }

    public int Value { get; set; }

    public Hint Hint { get; set; }

    public DateTime CreationTime { get; set; }
}
