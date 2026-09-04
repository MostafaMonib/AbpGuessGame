import React from 'react';
import { useAuth } from '../context/AuthContext';
import { Trophy, LogOut, User, Sparkles } from 'lucide-react';

export const Navbar: React.FC<{ onToggleLogs: () => void; isLogsOpen: boolean }> = ({ onToggleLogs, isLogsOpen }) => {
  const { user, isAuthenticated, logout } = useAuth();

  return (
    <header className="bg-slate-800/80 backdrop-blur border-b border-slate-700/80 sticky top-0 z-40">
      <div className="max-w-6xl mx-auto px-4 py-3 flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-emerald-500 to-teal-400 flex items-center justify-center shadow-lg shadow-emerald-500/20">
            <Sparkles className="w-6 h-6 text-slate-950" />
          </div>
          <div>
            <h1 className="font-bold text-lg text-white leading-none">AbpGuessGame</h1>
            <p className="text-xs text-slate-400">Guess the Number 1–43 vs Bot</p>
          </div>
        </div>

        {isAuthenticated && user && (
          <div className="flex items-center space-x-4">
            <div className="flex items-center space-x-2 bg-slate-900/60 px-3 py-1.5 rounded-lg border border-slate-700">
              <Trophy className="w-4 h-4 text-amber-400" />
              <span className="text-xs text-slate-300">Best Score:</span>
              <span className="text-sm font-semibold text-amber-400">
                {user.bestGuessCount != null ? `${user.bestGuessCount} guesses` : 'No win yet'}
              </span>
            </div>

            <div className="flex items-center space-x-2 text-slate-300 text-sm">
              <User className="w-4 h-4 text-emerald-400" />
              <span>{user.userName}</span>
            </div>

            <button
              onClick={onToggleLogs}
              className={`px-2.5 py-1 text-xs rounded-md border font-mono transition ${
                isLogsOpen
                  ? 'bg-emerald-500/20 border-emerald-500 text-emerald-300'
                  : 'bg-slate-700/50 border-slate-600 text-slate-400 hover:text-white'
              }`}
              title="Inspect X-Correlation-Id and API trace logs"
            >
              Trace Logs
            </button>

            <button
              onClick={logout}
              className="p-1.5 rounded-lg bg-slate-700/40 text-slate-400 hover:text-rose-400 hover:bg-slate-700 transition"
              title="Logout"
            >
              <LogOut className="w-4 h-4" />
            </button>
          </div>
        )}
      </div>
    </header>
  );
};

