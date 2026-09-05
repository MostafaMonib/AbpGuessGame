export type GameStatus = 'InProgress' | 'Won' | 'Abandoned' | 0 | 1 | 2;
export type Hint = 'Higher' | 'Lower' | 'Correct' | 0 | 1 | 2;

export interface GameDto {
  id: string;
  userId: string;
  status: GameStatus;
  guessCount: number;
  botGuessCount: number;
  secretNumber?: number | null;
  creationTime: string;
}

export interface GuessResultDto {
  gameId: string;
  guessNumber?: number;
  value?: number;
  hint: Hint;
  status: GameStatus;
  guessCount: number;
  isCorrect?: boolean;
  alreadyGuessed: boolean;
  secretNumber?: number | null;
  botGuessCount?: number | null;
  beatTheBot?: boolean | null;
  bestGuessCount?: number | null;
  updatedBestGuessCount?: number | null;
  isNewBest?: boolean;
}

export interface GuessHistoryItemDto {
  id: string;
  guessNumber: number;
  value: number;
  hint: Hint;
  creationTime: string;
}

export interface ClientLogEntry {
  id: string;
  timestamp: string;
  level: 'info' | 'warn' | 'error';
  message: string;
  correlationId?: string;
  data?: unknown;
}

