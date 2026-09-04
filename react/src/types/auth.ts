export interface UserProfile {
  id: string;
  userName: string;
  email: string;
  bestGuessCount: number | null;
}

export interface AuthResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
}

