using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace AbpGuessGame;

public class Guess : CreationAuditedEntity<Guid>
{
    public Guid GameId { get; set; }

    public int GuessNumber { get; set; }

    public int Value { get; set; }

    public Hint Hint { get; set; }

    public string? IdempotencyKey { get; set; }

    public Game? Game { get; set; }
}
