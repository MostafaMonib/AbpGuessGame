import React from 'react';
import { GuessResultDto } from '../types/game';
import { BotRaceComparison } from './BotRaceComparison';
import { Trophy, Sparkles, RefreshCw } from 'lucide-react';

export const WinModal: React.FC<{
  result: GuessResultDto;
  onPlayAgain: () => void;
}> = ({ result, onPlayAgain }) => {
  return (
    <div className="fixed inset-0 bg-slate-950/80 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div className="bg-slate-900 border border-slate-700/80 rounded-2xl p-6 max-w-md w-full shadow-2xl animate-in fade-in zoom-in-95 duration-200">
        <div className="text-center">
          <div className="w-14 h-14 mx-auto rounded-2xl bg-gradient-to-tr from-amber-500 to-yellow-300 flex items-center justify-center shadow-lg shadow-amber-500/20 mb-3">
            <Trophy className="w-8 h-8 text-slate-950" />
          </div>

          <h2 className="text-2xl font-black text-white">Victory! You Guessed It!</h2>
          <p className="text-sm text-slate-400 mt-1">
            The secret number was <span className="font-bold text-amber-400 text-lg">{result.secretNumber}</span>
          </p>

          {result.isNewBest && (
            <div className="mt-3 inline-flex items-center space-x-1.5 px-3 py-1 bg-amber-500/20 border border-amber-500/40 rounded-full text-amber-300 text-xs font-semibold">
              <Sparkles className="w-3.5 h-3.5" />
              <span>New Personal Best: {result.bestGuessCount} Guesses!</span>
            </div>
          )}

          <BotRaceComparison
            playerGuesses={result.guessCount}
            botGuesses={result.botGuessCount ?? 0}
            beatTheBot={result.beatTheBot ?? false}
          />

          <button
            onClick={onPlayAgain}
            className="w-full mt-6 py-3 px-4 rounded-xl bg-gradient-to-r from-emerald-500 to-teal-500 hover:from-emerald-600 hover:to-teal-600 font-bold text-slate-950 flex items-center justify-center space-x-2 shadow-lg shadow-emerald-500/20 transition cursor-pointer"
          >
            <RefreshCw className="w-4 h-4" />
            <span>Play Another Round</span>
          </button>
        </div>
      </div>
    </div>
  );
};

