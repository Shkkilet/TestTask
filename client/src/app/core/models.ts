export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  name: string;
  role: 'Admin' | 'User';
  expiresAt: string;
}

export interface ShortUrlDto {
  id: number;
  originalUrl: string;
  shortCode: string;
  shortUrl: string;
  createdDate: string;
  createdByUserName: string;
  canDelete: boolean;
}

export interface ShortUrlDetails {
  id: number;
  originalUrl: string;
  shortCode: string;
  shortUrlFull: string;
  createdDate: string;
  createdByUserName: string;
  visitCount: number;
}

export interface JwtPayload {
  sub: string;
  email: string;

  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': string;

  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role':
    ('Admin' | 'User')[];

  exp: number;
}
export interface CurrentUser {
  id: string;
  email: string;
  name: string;
  roles: ('Admin' | 'User')[];
  expiresAt: string;
}