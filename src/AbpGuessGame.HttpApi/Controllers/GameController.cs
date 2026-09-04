using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using AbpGuessGame.Application;
using AbpGuessGame.Dtos;

namespace AbpGuessGame.Controllers;

/// <summary>
/// Game REST API endpoints.
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/app/games")]
[Authorize]
public class GameController : AbpControllerBase
{
    private readonly GameAppService _gameAppService;

    public GameController(GameAppService gameAppService)
    {
        _gameAppService = gameAppService;
    }

    /// <summary>
    /// Start a new game or resume the existing in-progress game.
    /// </summary>
    /// <returns>Game DTO (secret number not included while in progress)</returns>
    [HttpPost("start")]
    [ProducesResponseType(typeof(GameDto), 200)]
    [ProducesResponseType(401)]
    public async Task<GameDto> StartAsync()
    {
        var input = new CreateGameDto();
        return await _gameAppService.StartAsync(input);
    }

    /// <summary>
    /// Get the current in-progress game for the authenticated user.
    /// </summary>
    /// <returns>Game DTO or 204 No Content if no in-progress game</returns>
    [HttpGet("current")]
    [ProducesResponseType(typeof(GameDto), 200)]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<GameDto>> GetCurrentAsync()
    {
        var game = await _gameAppService.GetCurrentGameAsync();
        if (game == null)
            return NoContent();
        return Ok(game);
    }

    /// <summary>
    /// Get a specific game by ID (owner only).
    /// </summary>
    /// <param name="gameId">Game ID</param>
    /// <returns>Game DTO (secret and bot count included if won)</returns>
    [HttpGet("{gameId:guid}")]
    [ProducesResponseType(typeof(GameDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<GameDto> GetAsync([FromRoute] Guid gameId)
    {
        return await _gameAppService.GetGameAsync(gameId);
    }

    /// <summary>
    /// Record a guess for a game.
    /// Implements idempotency, duplicate detection, and win logic.
    /// </summary>
    /// <param name="gameId">Game ID</param>
    /// <param name="input">Guess input with value (1-43) and optional idempotency key</param>
    /// <returns>Guess result DTO with hint, status, and optional secret/bot count</returns>
    [HttpPost("{gameId:guid}/guess")]
    [ProducesResponseType(typeof(GuessResultDto), 200)]
    [ProducesResponseType(400)] // Validation error (value out of range, etc.)
    [ProducesResponseType(403)] // Not the owner
    [ProducesResponseType(404)] // Game not found
    [ProducesResponseType(409)] // Game not in progress
    [ProducesResponseType(429)] // Rate limited
    [ProducesResponseType(401)]
    public async Task<GuessResultDto> RecordGuessAsync(
        [FromRoute] Guid gameId,
        [FromBody] RecordGuessDto input)
    {
        // Optional: extract Idempotency-Key from header if not in body
        if (string.IsNullOrEmpty(input.IdempotencyKey))
        {
            if (HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var headerValue))
                input.IdempotencyKey = headerValue.ToString();
            else if (HttpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var corrId))
                input.IdempotencyKey = corrId.ToString();
        }

        return await _gameAppService.RecordGuessAsync(gameId, input);
    }

    /// <summary>
    /// Get the guess history (log) for a specific game.
    /// Owner only; returns guesses in order.
    /// </summary>
    /// <param name="gameId">Game ID</param>
    /// <returns>Ordered list of guess history items</returns>
    [HttpGet("{gameId:guid}/guesses")]
    [ProducesResponseType(typeof(List<GuessHistoryItemDto>), 200)]
    [ProducesResponseType(403)] // Not the owner
    [ProducesResponseType(404)] // Game not found
    [ProducesResponseType(401)]
    public async Task<List<GuessHistoryItemDto>> GetGuessHistoryAsync([FromRoute] Guid gameId)
    {
        return await _gameAppService.GetGameHistoryAsync(gameId);
    }
}
