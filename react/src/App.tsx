import React, { useEffect, useState } from 'react';
import { useAuth } from './context/AuthContext';
import { Navbar } from './components/Navbar';
import { LogViewer } from './components/LogViewer';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { GamePage } from './pages/GamePage';
import { Loader2 } from 'lucide-react';
import { GuessResultDto } from './types/game';

const SUCCESSFUL_GAME_COUNT_KEY = 'successful_game_count';

const getSuccessfulGameCountKey = (userId: string) =>
  `${SUCCESSFUL_GAME_COUNT_KEY}:${userId}`;

export const AppContent: React.FC = () => {
  const { isAuthenticated, isLoading, user } = useAuth();
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
  const [isLogsOpen, setIsLogsOpen] = useState(false);
  const [successfulGameCount, setSuccessfulGameCount] = useState(0);

  useEffect(() => {
    if (!user?.id) {
      setSuccessfulGameCount(0);
      return;
    }

    const storedCount = Number(
      localStorage.getItem(getSuccessfulGameCountKey(user.id))
    );
    setSuccessfulGameCount(
      Number.isFinite(storedCount) && storedCount >= 0 ? storedCount : 0
    );
  }, [user?.id]);

  const handleGameResult = (result: GuessResultDto) => {
    const isWon = result.isCorrect ||
      result.status === 'Won' ||
      result.status === 1 ||
      result.hint === 'Correct' ||
      result.hint === 2;
    if (!isWon || !user?.id) return;

    const userCountKey = getSuccessfulGameCountKey(user.id);

    setSuccessfulGameCount(currentCount => {
      const nextCount = currentCount + 1;
      localStorage.setItem(userCountKey, String(nextCount));
      return nextCount;
    });
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-slate-900 flex items-center justify-center">
        <Loader2 className="w-8 h-8 text-emerald-400 animate-spin" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-900 text-slate-100 flex flex-col selection:bg-emerald-500 selection:text-slate-950">
      <Navbar
        onToggleLogs={() => setIsLogsOpen(!isLogsOpen)}
        isLogsOpen={isLogsOpen}
        successfulGameCount={successfulGameCount}
      />

      <main className="flex-1">
        {!isAuthenticated ? (
          authMode === 'login' ? (
            <LoginPage onSwitchToRegister={() => setAuthMode('register')} />
          ) : (
            <RegisterPage onSwitchToLogin={() => setAuthMode('login')} />
          )
        ) : (
          <GamePage onGameResult={handleGameResult} />
        )}
      </main>

      <LogViewer isOpen={isLogsOpen} onClose={() => setIsLogsOpen(false)} />
    </div>
  );
};

export const App: React.FC = () => {
  return <AppContent />;
};

export default App;

