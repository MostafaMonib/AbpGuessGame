namespace AbpGuessGame.Dtos;

/// <summary>
/// Input DTO for recording a guess in a game.
/// </summary>
public class RecordGuessDto
{
    /// <summary>
    /// Guess value must be between 1 and 43 inclusive.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Optional idempotency key to prevent duplicate processing.
    /// If not provided, X-Correlation-Id header is used as fallback.
    /// </summary>
    public string? IdempotencyKey { get; set; }
}
