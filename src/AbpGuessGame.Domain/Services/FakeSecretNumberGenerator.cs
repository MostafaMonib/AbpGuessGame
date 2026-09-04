using System;

namespace AbpGuessGame;

/// <summary>
/// Test fake: always returns a fixed secret number.
/// Use this in unit tests to make game behavior deterministic.
/// </summary>
public class FakeSecretNumberGenerator : ISecretNumberGenerator
{
    private readonly int _fixedSecret;

    public FakeSecretNumberGenerator(int fixedSecret = 22)
    {
        if (fixedSecret < 1 || fixedSecret > 43)
            throw new ArgumentOutOfRangeException(nameof(fixedSecret), "Secret must be in [1, 43]");

        _fixedSecret = fixedSecret;
    }

    public int Generate()
    {
        return _fixedSecret;
    }
}
