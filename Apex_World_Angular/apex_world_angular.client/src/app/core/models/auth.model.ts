export interface LoginRequestDto {
  username?: string;
  password?: string;
}

export interface RegisterBuyerDto {
  username?: string;
  password?: string;
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  city?: string;
}

export interface RegisterAdminDto {
  username?: string;
  password?: string;
  department?: string;
  role?: string;
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  city?: string;
}

export interface RefreshTokenRequestDto {
  accessToken?: string;
  refreshToken?: string;
}

export interface LogoutRequestDto {
  refreshToken?: string;
}

// What the backend actually returns: ApiResponse<TokenResponseDto>
export interface ApiTokenResponse {
  success: boolean;
  data: {
    accessToken: string;
    refreshToken: string;
  };
  message?: string;
}

// Flattened structure used internally by AuthService after unwrapping
export interface AuthResponse {
  accessToken?: string;
  refreshToken?: string;
  role?: string;
  username?: string;
}
