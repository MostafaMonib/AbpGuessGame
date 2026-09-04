using System;
using System.Collections.Generic;
using Volo.Abp.DependencyInjection;

namespace AbpGuessGame;

/// <summary>
/// Pure domain service that simulates a binary-search bot guessing the same secret.
/// This is deterministic and has no I/O or side effects.
/// </summary>
public class BinarySearchBotService : ITransientDependency
{
    private const int MinSecret = 1;
    private const int MaxSecret = 43;

    /// <summary>
    /// Simulate binary search for the given secret and return the number of guesses needed.
    /// </summary>
    public int ComputeGuessCount(int secret)
    {
        if (secret < MinSecret || secret > MaxSecret)
            throw new ArgumentOutOfRangeException(nameof(secret), $"Secret must be in [{MinSecret}, {MaxSecret}]");

        return BinarySearch(secret);
    }

    /// <summary>
    /// Simulate binary search and return the full path (sequence of guesses).
    /// Used primarily for testing and visualization.
    /// </summary>
    public BinarySearchResult ComputeWithPath(int secret)
    {
        if (secret < MinSecret || secret > MaxSecret)
            throw new ArgumentOutOfRangeException(nameof(secret), $"Secret must be in [{MinSecret}, {MaxSecret}]");

        var path = new List<int>();
        int count = BinarySearchWithPath(secret, path);

        return new BinarySearchResult
        {
            GuessCount = count,
            Guesspath = path
        };
    }

    private int BinarySearch(int secret)
    {
        int low = MinSecret;
        int high = MaxSecret;
        int count = 0;

        while (low <= high)
        {
            int mid = (low + high) / 2;
            count++;

            if (mid == secret)
                return count;

            if (mid < secret)
                low = mid + 1;
            else
                high = mid - 1;
        }

        return count;
    }

    private int BinarySearchWithPath(int secret, List<int> path)
    {
        int low = MinSecret;
        int high = MaxSecret;
        int count = 0;

        while (low <= high)
        {
            int mid = (low + high) / 2;
            count++;
            path.Add(mid);

            if (mid == secret)
                return count;

            if (mid < secret)
                low = mid + 1;
            else
                high = mid - 1;
        }

        return count;
    }
}

/// <summary>
/// Result of binary search simulation.
/// </summary>
public class BinarySearchResult
{
    public int GuessCount { get; set; }

    public List<int> Guesspath { get; set; } = new();
}
