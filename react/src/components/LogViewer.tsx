import React, { useState, useEffect } from 'react';
import { subscribeToClientLogs } from '../services/api';
import { ClientLogEntry } from '../types/game';
import { Terminal, Copy, Check, Trash2, X } from 'lucide-react';

export const LogViewer: React.FC<{ isOpen: boolean; onClose: () => void }> = ({ isOpen, onClose }) => {
  const [logs, setLogs] = useState<ClientLogEntry[]>([]);
  const [copiedId, setCopiedId] = useState<string | null>(null);

  useEffect(() => {
    return subscribeToClientLogs((entry) => {
      setLogs((prev) => [entry, ...prev].slice(0, 50));
    });
  }, []);

  if (!isOpen) return null;

  const copyToClipboard = (text: string, id: string) => {
    navigator.clipboard.writeText(text);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 1500);
  };

  return (
    <div className="fixed bottom-4 right-4 w-96 md:w-[480px] bg-slate-950 border border-slate-700 rounded-xl shadow-2xl z-50 flex flex-col max-h-[500px] overflow-hidden">
      <div className="flex items-center justify-between px-4 py-2.5 bg-slate-900 border-b border-slate-800">
        <div className="flex items-center space-x-2 text-xs font-mono text-emerald-400">
          <Terminal className="w-4 h-4" />
          <span className="font-semibold">Live Trace Logs (X-Correlation-Id)</span>
        </div>
        <div className="flex items-center space-x-1">
          <button
            onClick={() => setLogs([])}
            className="p-1 text-slate-400 hover:text-slate-200"
            title="Clear logs"
          >
            <Trash2 className="w-3.5 h-3.5" />
          </button>
          <button
            onClick={onClose}
            className="p-1 text-slate-400 hover:text-slate-200"
            title="Close"
          >
            <X className="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      <div className="p-3 overflow-y-auto space-y-2 font-mono text-xs flex-1">
        {logs.length === 0 ? (
          <p className="text-slate-500 text-center py-6">No API operations logged yet.</p>
        ) : (
          logs.map((log) => (
            <div
              key={log.id}
              className={`p-2 rounded border ${
                log.level === 'error'
                  ? 'bg-rose-950/40 border-rose-800 text-rose-300'
                  : log.level === 'warn'
                  ? 'bg-amber-950/40 border-amber-800 text-amber-300'
                  : 'bg-slate-900 border-slate-800 text-slate-300'
              }`}
            >
              <div className="flex items-center justify-between text-[10px] text-slate-500 mb-1">
                <span>{new Date(log.timestamp).toLocaleTimeString()}</span>
                {log.correlationId && (
                  <button
                    onClick={() => copyToClipboard(log.correlationId!, log.id)}
                    className="flex items-center space-x-1 text-emerald-400 hover:underline"
                    title="Copy correlation id to grep in backend Serilog logs"
                  >
                    <span>corr: {log.correlationId.substring(0, 8)}...</span>
                    {copiedId === log.id ? <Check className="w-2.5 h-2.5" /> : <Copy className="w-2.5 h-2.5" />}
                  </button>
                )}
              </div>
              <div className="font-medium break-all">{log.message}</div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

