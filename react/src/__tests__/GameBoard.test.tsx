import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { GameBoard } from '../components/GameBoard';
import { BotRaceComparison } from '../components/BotRaceComparison';
import { GameDto, GuessResultDto } from '../types/game';

describe('GameBoard Component', () => {
  const mockGame: GameDto = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    userId: '123e4567-e89b-12d3-a456-426614174001',
    status: 'InProgress',
    guessCount: 2,
    botGuessCount: 6,
    creationTime: new Date().toISOString()
  };

  it('renders game title and attempt count correctly', () => {
    render(
      <GameBoard
        game={mockGame}
        lastResult={null}
        onGuess={vi.fn()}
        isSubmitting={false}
      />
    );

    expect(screen.getByText(/Guess The Number \(1–43\)/i)).toBeInTheDocument();
    expect(screen.getByText('Attempts')).toBeInTheDocument();
    expect(screen.getByText('Attempts').nextElementSibling).toHaveTextContent('2');
  });

  it('displays Aim HIGHER hint when server returns Higher', () => {
    const higherResult: GuessResultDto = {
      gameId: mockGame.id,
      guessNumber: 1,
      value: 10,
      hint: 'Higher',
      status: 'InProgress',
      guessCount: 1,
      isCorrect: false,
      alreadyGuessed: false,
      botGuessCount: 6,
      beatTheBot: false,
      isNewBest: false
    };

    render(
      <GameBoard
        game={mockGame}
        lastResult={higherResult}
        onGuess={vi.fn()}
        isSubmitting={false}
      />
    );

    expect(screen.getByText(/Aim HIGHER than 10/i)).toBeInTheDocument();
  });

  it('displays Aim LOWER hint when server returns Lower', () => {
    const lowerResult: GuessResultDto = {
      gameId: mockGame.id,
      guessNumber: 2,
      value: 35,
      hint: 'Lower',
      status: 'InProgress',
      guessCount: 2,
      isCorrect: false,
      alreadyGuessed: false,
      botGuessCount: 6,
      beatTheBot: false,
      isNewBest: false
    };

    render(
      <GameBoard
        game={mockGame}
        lastResult={lowerResult}
        onGuess={vi.fn()}
        isSubmitting={false}
      />
    );

    expect(screen.getByText(/Aim LOWER than 35/i)).toBeInTheDocument();
  });

  it('validates bounds: rejects numbers < 1 or > 43', () => {
    const onGuess = vi.fn();
    render(
      <GameBoard
        game={mockGame}
        lastResult={null}
        onGuess={onGuess}
        isSubmitting={false}
      />
    );

    const input = screen.getByLabelText(/Enter your guess/i);
    const form = screen.getByTestId('guess-form');

    fireEvent.change(input, { target: { value: '50' } });
    fireEvent.submit(form);

    expect(screen.getByText(/Please enter a valid number between 1 and 43/i)).toBeInTheDocument();
    expect(onGuess).not.toHaveBeenCalled();
  });
});

describe('BotRaceComparison Component', () => {
  it('renders victory badge when player beats the bot', () => {
    render(
      <BotRaceComparison
        playerGuesses={4}
        botGuesses={6}
        beatTheBot={true}
      />
    );

    expect(screen.getByText(/You Beat the Bot!/i)).toBeInTheDocument();
    expect(screen.getByText('4')).toBeInTheDocument();
    expect(screen.getByText('6')).toBeInTheDocument();
  });

  it('renders faster bot notice when player takes more attempts', () => {
    render(
      <BotRaceComparison
        playerGuesses={8}
        botGuesses={5}
        beatTheBot={false}
      />
    );

    expect(screen.getByText(/Bot Was Faster/i)).toBeInTheDocument();
    expect(screen.getByText('8')).toBeInTheDocument();
    expect(screen.getByText('5')).toBeInTheDocument();
  });
});

