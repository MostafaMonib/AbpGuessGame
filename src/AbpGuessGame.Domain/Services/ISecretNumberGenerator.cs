namespace AbpGuessGame;

/// <summary>
/// Service for generating random secret numbers for games.
/// This is injected into the domain so that tests can provide deterministic implementations.
/// </summary>
public interface ISecretNumberGenerator
{
    /// <summary>
    /// Generate a random secret number in range [1, 43] inclusive.
    /// </summary>
    int Generate();
}
