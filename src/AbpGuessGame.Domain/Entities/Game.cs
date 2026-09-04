using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace AbpGuessGame;

public class Game : AuditedAggregateRoot<Guid>
{
    public Guid UserId { get; set; }

    public int SecretNumber { get; set; }

    public int GuessCount { get; set; }

    public GameStatus Status { get; set; }

    public int BotGuessCount { get; set; }

    public string? ConcurrencyStamp { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public ICollection<Guess> Guesses { get; set; } = new List<Guess>();
}
