import React from 'react';
import { GuessHistoryItemDto } from '../types/game';
import { ArrowUpCircle, ArrowDownCircle, CheckCircle2, History } from 'lucide-react';

export const GuessHistory: React.FC<{ items: GuessHistoryItemDto[] }> = ({ items }) => {
  if (items.length === 0) {
    return (
      <div className="bg-slate-800/40 rounded-xl border border-slate-700/60 p-6 text-center text-slate-500">
        <History className="w-8 h-8 mx-auto mb-2 opacity-50" />
        <p className="text-sm">No guesses yet. Enter a number between 1 and 43 to start!</p>
      </div>
    );
  }

  return (
    <div className="bg-slate-800/60 rounded-xl border border-slate-700/80 p-4">
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-sm font-semibold text-slate-300 flex items-center space-x-2">
          <History className="w-4 h-4 text-emerald-400" />
          <span>Guess History ({items.length})</span>
        </h3>
        <span className="text-xs text-slate-400">Order: oldest to newest</span>
      </div>

      <div className="space-y-2 max-h-64 overflow-y-auto pr-1">
        {items.map((item) => (
          <div
            key={item.id}
            className="flex items-center justify-between px-3 py-2 bg-slate-900/60 rounded-lg border border-slate-700/50"
          >
            <div className="flex items-center space-x-3">
              <span className="text-xs font-mono text-slate-500">#{item.guessNumber}</span>
              <span className="font-bold text-base text-white">{item.value}</span>
            </div>

            <div className="flex items-center space-x-1.5">
              {item.hint === 'Higher' && (
                <span className="flex items-center space-x-1 text-xs font-medium text-amber-400 bg-amber-950/40 px-2 py-0.5 rounded border border-amber-800/60">
                  <ArrowUpCircle className="w-3.5 h-3.5" />
                  <span>Aim Higher</span>
                </span>
              )}
              {item.hint === 'Lower' && (
                <span className="flex items-center space-x-1 text-xs font-medium text-cyan-400 bg-cyan-950/40 px-2 py-0.5 rounded border border-cyan-800/60">
                  <ArrowDownCircle className="w-3.5 h-3.5" />
                  <span>Aim Lower</span>
                </span>
              )}
              {item.hint === 'Correct' && (
                <span className="flex items-center space-x-1 text-xs font-medium text-emerald-400 bg-emerald-950/40 px-2 py-0.5 rounded border border-emerald-800/60">
                  <CheckCircle2 className="w-3.5 h-3.5" />
                  <span>Correct!</span>
                </span>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

