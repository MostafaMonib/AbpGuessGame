import React from 'react';
import { Bot, UserCheck, Zap, Award } from 'lucide-react';

export const BotRaceComparison: React.FC<{
  playerGuesses: number;
  botGuesses: number;
  beatTheBot: boolean;
}> = ({ playerGuesses, botGuesses, beatTheBot }) => {
  return (
    <div className="bg-slate-900/90 rounded-xl border border-slate-700/80 p-5 mt-4">
      <div className="flex items-center justify-between border-b border-slate-800 pb-3 mb-4">
        <h4 className="font-semibold text-sm text-slate-200 flex items-center space-x-2">
          <Zap className="w-4 h-4 text-amber-400" />
          <span>Player vs. Binary-Search Bot Race</span>
        </h4>
        {beatTheBot ? (
          <span className="flex items-center space-x-1 text-xs font-bold text-emerald-400 bg-emerald-950/60 px-2 py-0.5 rounded border border-emerald-700">
            <Award className="w-3.5 h-3.5" />
            <span>You Beat the Bot!</span>
          </span>
        ) : (
          <span className="text-xs text-slate-400 bg-slate-800 px-2 py-0.5 rounded">
            Bot Was Faster
          </span>
        )}
      </div>

      <div className="grid grid-cols-2 gap-4 text-center">
        <div className="p-3 bg-slate-800/60 rounded-lg border border-slate-700/50">
          <div className="flex items-center justify-center space-x-1 text-slate-400 text-xs mb-1">
            <UserCheck className="w-3.5 h-3.5 text-emerald-400" />
            <span>Your Guesses</span>
          </div>
          <div className="text-2xl font-black text-white">{playerGuesses}</div>
        </div>

        <div className="p-3 bg-slate-800/60 rounded-lg border border-slate-700/50">
          <div className="flex items-center justify-center space-x-1 text-slate-400 text-xs mb-1">
            <Bot className="w-3.5 h-3.5 text-cyan-400" />
            <span>Binary-Search Bot</span>
          </div>
          <div className="text-2xl font-black text-cyan-400">{botGuesses}</div>
        </div>
      </div>

      <p className="text-[11px] text-slate-400 mt-3 text-center">
        The bot solved the same secret using an optimal binary search strategy.
      </p>
    </div>
  );
};

