import React, { useState } from 'react';
import { useAuth } from './context/AuthContext';
import { Navbar } from './components/Navbar';
import { LogViewer } from './components/LogViewer';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { GamePage } from './pages/GamePage';
import { Loader2 } from 'lucide-react';

export const AppContent: React.FC = () => {
  const { isAuthenticated, isLoading } = useAuth();
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
  const [isLogsOpen, setIsLogsOpen] = useState(false);

  if (isLoading) {
    return (
      <div className="min-h-screen bg-slate-900 flex items-center justify-center">
        <Loader2 className="w-8 h-8 text-emerald-400 animate-spin" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-900 text-slate-100 flex flex-col selection:bg-emerald-500 selection:text-slate-950">
      <Navbar onToggleLogs={() => setIsLogsOpen(!isLogsOpen)} isLogsOpen={isLogsOpen} />

      <main className="flex-1">
        {!isAuthenticated ? (
          authMode === 'login' ? (
            <LoginPage onSwitchToRegister={() => setAuthMode('register')} />
          ) : (
            <RegisterPage onSwitchToLogin={() => setAuthMode('login')} />
          )
        ) : (
          <GamePage />
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

