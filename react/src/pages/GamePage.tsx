import React, { useState, useEffect } from 'react';
import { GameDto, GuessResultDto, GuessHistoryItemDto } from '../types/game';
import { GameService } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { GameBoard } from '../components/GameBoard';
import { GuessHistory } from '../components/GuessHistory';
import { WinModal } from '../components/WinModal';
import { Play, Loader2 } from 'lucide-react';

export const GamePage: React.FC = () => {
  const { refreshProfile } = useAuth();
  const [game, setGame] = useState<GameDto | null>(null);
  const [history, setHistory] = useState<GuessHistoryItemDto[]>([]);
  const [lastResult, setLastResult] = useState<GuessResultDto | null>(null);
  const [winResult, setWinResult] = useState<GuessResultDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  const loadActiveGame = async () => {
    setIsLoading(true);
    try {
      const active = await GameService.getActiveGame();
      if (active && (active.status === 'InProgress' || (active.status as unknown) === 0)) {
        setGame(active);
        const hist = await GameService.getGuessHistory(active.id);
        setHistory(hist);
      } else {
        setGame(null);
      }
    } catch {
      setGame(null);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadActiveGame();
  }, []);

  const handleStartGame = async () => {
    setIsLoading(true);
    setLastResult(null);
    setWinResult(null);
    try {
      const newGame = await GameService.startGame();
      setGame(newGame);
      const hist = await GameService.getGuessHistory(newGame.id);
      setHistory(hist);
    } finally {
      setIsLoading(false);
    }
  };

  const handleGuess = async (val: number) => {
    if (!game) return;
    setIsSubmitting(true);
    try {
      const result = await GameService.submitGuess(game.id, val);
      setLastResult(result);

      if (result.isCorrect) {
        setWinResult(result);
        setGame(prev => prev ? { ...prev, status: 'Won' as const, guessCount: result.guessCount } : prev);
        await refreshProfile();
      } else {
        // Update guess count on non-win as well
        setGame(prev => prev ? { ...prev, guessCount: result.guessCount } : prev);
      }

      const updatedHistory = await GameService.getGuessHistory(game.id);
      setHistory(updatedHistory);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-[70vh] flex items-center justify-center">
        <Loader2 className="w-8 h-8 text-emerald-400 animate-spin" />
      </div>
    );
  }

  if (!game) {
    return (
      <div className="max-w-2xl mx-auto mt-12 px-4">
        <div className="bg-slate-800/80 rounded-2xl border border-slate-700/80 p-8 text-center shadow-xl">
          <div className="w-16 h-16 rounded-2xl bg-gradient-to-tr from-emerald-500 to-teal-400 mx-auto flex items-center justify-center shadow-lg shadow-emerald-500/20 mb-4">
            <Play className="w-8 h-8 text-slate-950 fill-current ml-1" />
          </div>
          <h2 className="text-2xl font-black text-white">Ready for a New Round?</h2>
          <p className="text-sm text-slate-400 mt-2 max-w-md mx-auto">
            The server will choose a secret number between 1 and 43. Challenge yourself and see if you can solve it in fewer attempts than the optimal binary search bot!
          </p>
          <button
            onClick={handleStartGame}
            className="mt-6 py-3.5 px-8 bg-gradient-to-r from-emerald-500 to-teal-500 hover:from-emerald-600 hover:to-teal-600 font-bold text-slate-950 text-base rounded-xl transition shadow-lg shadow-emerald-500/25 cursor-pointer inline-flex items-center space-x-2"
          >
            <Play className="w-5 h-5 fill-current" />
            <span>Start Game</span>
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8 space-y-6">
      <GameBoard
        game={game}
        lastResult={lastResult}
        onGuess={handleGuess}
        isSubmitting={isSubmitting}
      />

      <GuessHistory items={history} />

      {winResult && (
        <WinModal
          result={winResult}
          onPlayAgain={handleStartGame}
        />
      )}
    </div>
  );
};

