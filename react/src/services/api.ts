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
    logListeners.forEach(fn => fn(entry));
}

const api = axios.create({
    baseURL: '',
    headers: {
        'Content-Type': 'application/json',
    },
});

function getXsrfToken(): string | null {
  const match = document.cookie.match(new RegExp('(^|;\\s*)(?:XSRF-TOKEN|RequestVerificationToken)=([^;]*)'));
  return match ? decodeURIComponent(match[2]) : null;
}

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const correlationId = crypto.randomUUID();
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
            idempotencyKey: idempotencyKey || crypto.randomUUID()
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
        const params = new URLSearchParams();
        params.append('grant_type', 'password');
        params.append('client_id', 'AbpGuessGame_App');
        params.append('username', userNameOrEmailAddress);
        params.append('password', password);
        params.append('scope', 'openid profile email AbpGuessGame');

        const res = await axios.post<AuthResponse>('/connect/token', params, {
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Correlation-Id': crypto.randomUUID()
            }
        });
        return res.data;
    },

    async register(userName: string, emailAddress: string, password: string): Promise<void> {
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

