import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { GameDto, GuessResultDto, GuessHistoryItemDto, ClientLogEntry } from '../types/game';
import { AuthResponse, UserProfile } from '../types/auth';

const logListeners: ((log: ClientLogEntry) => void)[] = [];

export function subscribeToClientLogs(listener: (log: ClientLogEntry) => void) {
    logListeners.push(listener);
    return () => {
        const index = logListeners.indexOf(listener);
        if (index > -1) logListeners.splice(index, 1);
    };
}

export function logClientEvent(level: 'info' | 'warn' | 'error', message: string, correlationId?: string, data?: unknown) {
    const entry: ClientLogEntry = {
        id: Math.random().toString(36).substring(2, 9),
        timestamp: new Date().toISOString(),
        level,
        message,
        correlationId,
        data
    };
    if (level === 'error') {
        console.error(`[API Error] ${message}`, { correlationId, data });
    } else if (level === 'warn') {
        console.warn(`[API Warning] ${message}`, { correlationId, data });
    } else {
        console.log(`[API Info] ${message}`, { correlationId, data });
    }
    logListeners.forEach(fn => fn(entry));
}

const BASE_URL = import.meta.env.VITE_API_BASE_URL || '';
console.log(`%c[AbpGuessGame API]%c Initialized with BASE_URL: "${BASE_URL || '(relative / same-origin)'}"`, 'color: #10b981; font-weight: bold;', 'color: inherit;');

const api = axios.create({
    baseURL: BASE_URL,
    withCredentials: true,
    headers: {
        'Content-Type': 'application/json',
    },
});

function generateUUID(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

function getXsrfToken(): string | null {
  const match = document.cookie.match(new RegExp('(^|;\\s*)(?:XSRF-TOKEN|RequestVerificationToken)=([^;]*)'));
  return match ? decodeURIComponent(match[2]) : null;
}

function clearXsrfCookie(name: string) {
    document.cookie = `${name}=; Max-Age=0; path=/`;
}

async function refreshAntiForgeryToken() {
    clearXsrfCookie('XSRF-TOKEN');
    clearXsrfCookie('RequestVerificationToken');
    await api.get('/api/abp/application-configuration');
}

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const correlationId = generateUUID();
  config.headers.set('X-Correlation-Id', correlationId);
  config.headers.set('X-Requested-With', 'XMLHttpRequest');

  const token = localStorage.getItem('access_token');
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`);
  }

  const xsrfToken = getXsrfToken();
  if (xsrfToken) {
    config.headers.set('RequestVerificationToken', xsrfToken);
    config.headers.set('X-XSRF-TOKEN', xsrfToken);
  }

  logClientEvent('info', `HTTP ${config.method?.toUpperCase()} ${config.url}`, correlationId, config.data);
  return config;
});

api.interceptors.response.use(
    (response) => {
        const correlationId = (response.config.headers['X-Correlation-Id'] as string) || '';
        logClientEvent('info', `Response ${response.status} from ${response.config.url}`, correlationId, response.data);
        return response;
    },
    (error: AxiosError) => {
        const correlationId = (error.config?.headers?.['X-Correlation-Id'] as string) || '';
        logClientEvent('error', `HTTP Error ${error.response?.status || 'Network'}: ${error.message}`, correlationId, error.response?.data);
        if (error.response?.status === 401) {
            localStorage.removeItem('access_token');
        }
        return Promise.reject(error);
    }
);

export const GameService = {
    async startGame(): Promise<GameDto> {
        const res = await api.post<GameDto>('/api/app/games/start', {});
        return res.data;
    },

    async submitGuess(gameId: string, value: number, idempotencyKey?: string): Promise<GuessResultDto> {
        const res = await api.post<GuessResultDto>(`/api/app/games/${gameId}/guess`, {
            value,
            idempotencyKey: idempotencyKey || generateUUID()
        });
        const data = res.data;
        const isWon = data.status === 'Won' || (data.status as unknown) === 1;
        const isCorrectHint = data.hint === 'Correct' || (data.hint as unknown) === 2;
        const isCorrect = isWon || isCorrectHint;

        return {
            ...data,
            isCorrect,
            isNewBest: !!data.updatedBestGuessCount,
            bestGuessCount: data.updatedBestGuessCount,
            value
        };
    },

    async getGuessHistory(gameId: string): Promise<GuessHistoryItemDto[]> {
        const res = await api.get<GuessHistoryItemDto[]>(`/api/app/games/${gameId}/guesses`);
        return res.data;
    },

    async getActiveGame(): Promise<GameDto | null> {
        try {
            const res = await api.get<GameDto>('/api/app/games/current');
            if (res.status === 204 || !res.data) {
                return null;
            }
            return res.data;
        } catch {
            return null;
        }
    },

    async login(userNameOrEmailAddress: string, password: string): Promise<AuthResponse> {
        const url = `${BASE_URL}/connect/token`;
        const correlationId = generateUUID();
        console.log(`%c[Auth Service]%c Sending POST request to: ${url}`, 'color: #3b82f6; font-weight: bold;', 'color: inherit;', {
            username: userNameOrEmailAddress,
            correlationId
        });

        const params = new URLSearchParams();
        params.append('grant_type', 'password');
        params.append('client_id', 'AbpGuessGame_App');
        params.append('username', userNameOrEmailAddress);
        params.append('password', password);
        params.append('scope', 'openid profile email AbpGuessGame');

        try {
            const res = await axios.post<AuthResponse>(url, params, {
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Correlation-Id': correlationId
                }
            });
            console.log(`%c[Auth Service]%c Login SUCCESS (${res.status})`, 'color: #10b981; font-weight: bold;', 'color: inherit;', res.data);
            return res.data;
        } catch (error) {
            const axiosErr = error as AxiosError;
            console.error(`%c[Auth Service]%c Login FAILED for URL: ${url}`, 'color: #ef4444; font-weight: bold;', 'color: inherit;', {
                status: axiosErr.response?.status,
                statusText: axiosErr.response?.statusText,
                data: axiosErr.response?.data,
                message: axiosErr.message
            });
            throw error;
        }
    },

    async register(userName: string, emailAddress: string, password: string): Promise<void> {
        await refreshAntiForgeryToken();
        await api.post('/api/account/register', {
            userName,
            emailAddress,
            password,
            appName: 'AbpGuessGame'
        });
    },

    async getCurrentUser(): Promise<UserProfile> {
        const res = await api.get<UserProfile>('/api/account/my-profile');
        return res.data;
    },

    async getApplicationConfiguration(): Promise<unknown> {
        try {
            const res = await api.get('/api/abp/application-configuration');
            return res.data;
        } catch {
            return null;
        }
    }
};

export default api;

