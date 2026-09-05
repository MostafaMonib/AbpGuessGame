import React, { useState } from 'react';
import { GameDto, GuessResultDto } from '../types/game';
import { ArrowUpCircle, ArrowDownCircle, AlertCircle, Send, Dices } from 'lucide-react';

interface GameBoardProps {
  game: GameDto;
  lastResult: GuessResultDto | null;
  onGuess: (value: number) => Promise<void>;
  isSubmitting: boolean;
}

export const GameBoard: React.FC<GameBoardProps> = ({
  game,
  lastResult,
  onGuess,
  isSubmitting
}) => {
  const [inputValue, setInputValue] = useState<string>('');
  const [validationError, setValidationError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setValidationError(null);

    const num = parseInt(inputValue, 10);
    if (isNaN(num) || num < 1 || num > 43) {
      setValidationError('Please enter a valid number between 1 and 43.');
      return;
    }

    await onGuess(num);
    setInputValue('');
  };

  const handleQuickPick = (val: number) => {
    setInputValue(val.toString());
    setValidationError(null);
  };

  return (
    <div className="bg-slate-800/80 rounded-2xl border border-slate-700/80 p-6 shadow-xl">
      <div className="flex items-center justify-between border-b border-slate-700/60 pb-4 mb-6">
        <div>
          <h2 className="text-xl font-bold text-white flex items-center space-x-2">
            <Dices className="w-5 h-5 text-emerald-400" />
            <span>Guess The Number (1–43)</span>
          </h2>
          <p className="text-xs text-slate-400 mt-0.5">
            The server generated a secret integer. Try to guess it with the fewest attempts!
          </p>
        </div>

        <div className="bg-slate-900/80 px-4 py-2 rounded-xl border border-slate-700/80 text-center">
          <div className="text-xs text-slate-400 uppercase tracking-wider font-semibold">Attempts</div>
          <div className="text-2xl font-black text-emerald-400 font-mono">
            {lastResult ? lastResult.guessCount : game.guessCount}
          </div>
        </div>
      </div>

      {/* Live Hint Feedback */}
      {lastResult && (
        <div className="mb-6 animate-in fade-in duration-200">
          {lastResult.alreadyGuessed && (
            <div className="p-4 rounded-xl bg-amber-950/40 border border-amber-800 text-amber-300 flex items-center space-x-3">
              <AlertCircle className="w-5 h-5 flex-shrink-0" />
              <div>
                <p className="font-semibold text-sm">Already Guessed {lastResult.value}!</p>
                <p className="text-xs opacity-80">This number was already tried and does not count as a new attempt.</p>
              </div>
            </div>
          )}

          {!lastResult.alreadyGuessed && lastResult.hint === 'Higher' && (
            <div className="p-4 rounded-xl bg-amber-950/40 border border-amber-700/70 text-amber-300 flex items-center justify-between">
              <div className="flex items-center space-x-3">
                <ArrowUpCircle className="w-6 h-6 text-amber-400 flex-shrink-0" />
                <div>
                  <div className="font-bold text-base">Aim HIGHER than {lastResult.value}</div>
                  <div className="text-xs text-amber-300/80">The secret number is greater than your guess.</div>
                </div>
              </div>
              <span className="text-xs font-mono bg-amber-900/60 px-2.5 py-1 rounded-md border border-amber-600/40">
                &gt; {lastResult.value}
              </span>
            </div>
          )}

          {!lastResult.alreadyGuessed && lastResult.hint === 'Lower' && (
            <div className="p-4 rounded-xl bg-cyan-950/40 border border-cyan-700/70 text-cyan-300 flex items-center justify-between">
              <div className="flex items-center space-x-3">
                <ArrowDownCircle className="w-6 h-6 text-cyan-400 flex-shrink-0" />
                <div>
                  <div className="font-bold text-base">Aim LOWER than {lastResult.value}</div>
                  <div className="text-xs text-cyan-300/80">The secret number is less than your guess.</div>
                </div>
              </div>
              <span className="text-xs font-mono bg-cyan-900/60 px-2.5 py-1 rounded-md border border-cyan-600/40">
                &lt; {lastResult.value}
              </span>
            </div>
          )}
        </div>
      )}

      {/* Input Form */}
      <form onSubmit={handleSubmit} data-testid="guess-form" className="space-y-4">
        <div>
          <label htmlFor="guess-input" className="block text-xs font-medium text-slate-300 mb-1.5">
            Enter your guess (1 – 43)
          </label>
          <div className="flex space-x-3">
            <input
              id="guess-input"
              type="number"
              min="1"
              max="43"
              value={inputValue}
              onChange={(e) => {
                setInputValue(e.target.value);
                setValidationError(null);
              }}
              placeholder="e.g. 22"
              disabled={isSubmitting}
              className="flex-1 px-4 py-3 bg-slate-900 border border-slate-700 rounded-xl font-mono text-lg text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-transparent transition"
              autoFocus
            />
            <button
              type="submit"
              disabled={isSubmitting || !inputValue}
              className="px-6 py-3 bg-emerald-500 hover:bg-emerald-600 disabled:bg-slate-700 disabled:text-slate-500 font-bold text-slate-950 rounded-xl flex items-center space-x-2 transition shadow-lg shadow-emerald-500/20 disabled:shadow-none cursor-pointer"
            >
              <Send className="w-4 h-4" />
              <span>{isSubmitting ? 'Checking...' : 'Submit Guess'}</span>
            </button>
          </div>
          {validationError && (
            <p className="text-xs text-rose-400 mt-1.5 flex items-center space-x-1">
              <AlertCircle className="w-3.5 h-3.5" />
              <span>{validationError}</span>
            </p>
          )}
        </div>

        {/* Quick Suggestion Buttons */}
        <div>
          <div className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider mb-2">
            Quick Binary Search Milestones
          </div>
          <div className="flex flex-wrap gap-1.5">
            {[1, 11, 22, 33, 43].map((num) => (
              <button
                key={num}
                type="button"
                onClick={() => handleQuickPick(num)}
                className="px-2.5 py-1 text-xs font-mono bg-slate-900/60 hover:bg-slate-700 text-slate-300 rounded-lg border border-slate-700/60 transition cursor-pointer"
              >
                {num}
              </button>
            ))}
          </div>
        </div>
      </form>
    </div>
  );
};

