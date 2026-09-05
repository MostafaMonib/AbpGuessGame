using System.Security.Cryptography;
using Volo.Abp.DependencyInjection;

namespace AbpGuessGame;

/// <summary>
/// Production implementation: generates a cryptographically strong random number in [1, 43].
/// </summary>
public class RandomSecretNumberGenerator : ISecretNumberGenerator, ISingletonDependency
{
    public int Generate()
    {
        return RandomNumberGenerator.GetInt32(1, 44); // GetInt32 upper bound is exclusive; 44 gives [1, 43]
    }
}
