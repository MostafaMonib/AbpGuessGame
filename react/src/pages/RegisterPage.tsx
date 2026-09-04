import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { UserPlus, AlertCircle, Sparkles } from 'lucide-react';

export const RegisterPage: React.FC<{ onSwitchToLogin: () => void }> = ({ onSwitchToLogin }) => {
  const { register } = useAuth();
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await register(username, email, password);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: { message?: string } } } })?.response?.data?.error?.message
        || 'Registration failed. Password requires uppercase, lowercase, digit, and special character.';
      setError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center p-4">
      <div className="bg-slate-800/90 border border-slate-700/80 rounded-2xl p-8 max-w-md w-full shadow-2xl">
        <div className="text-center mb-6">
          <div className="w-12 h-12 rounded-xl bg-gradient-to-tr from-emerald-500 to-teal-400 mx-auto flex items-center justify-center shadow-lg shadow-emerald-500/20 mb-3">
            <Sparkles className="w-6 h-6 text-slate-950" />
          </div>
          <h2 className="text-2xl font-bold text-white">Create Account</h2>
          <p className="text-xs text-slate-400 mt-1">Register to start your game and track your high score</p>
        </div>

        {error && (
          <div className="p-3 mb-4 rounded-xl bg-rose-950/40 border border-rose-800 text-rose-300 text-xs flex items-center space-x-2">
            <AlertCircle className="w-4 h-4 flex-shrink-0" />
            <span>{error}</span>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-xs font-medium text-slate-300 mb-1">Username</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              className="w-full px-4 py-2.5 bg-slate-900 border border-slate-700 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition text-sm"
              placeholder="e.g. gameplayer"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-300 mb-1">Email Address</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-4 py-2.5 bg-slate-900 border border-slate-700 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition text-sm"
              placeholder="player@example.com"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-300 mb-1">Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full px-4 py-2.5 bg-slate-900 border border-slate-700 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition text-sm"
              placeholder="••••••••"
            />
            <p className="text-[10px] text-slate-500 mt-1">Must contain min 6 chars with digits and symbols.</p>
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full py-3 px-4 rounded-xl bg-emerald-500 hover:bg-emerald-600 disabled:bg-slate-700 font-bold text-slate-950 flex items-center justify-center space-x-2 transition shadow-lg shadow-emerald-500/20 cursor-pointer text-sm"
          >
            <UserPlus className="w-4 h-4" />
            <span>{isSubmitting ? 'Creating account...' : 'Register'}</span>
          </button>
        </form>

        <div className="mt-6 pt-6 border-t border-slate-700/60 text-center text-xs text-slate-400">
          Already have an account?{' '}
          <button
            onClick={onSwitchToLogin}
            className="text-emerald-400 hover:underline font-semibold cursor-pointer"
          >
            Sign In
          </button>
        </div>
      </div>
    </div>
  );
};

