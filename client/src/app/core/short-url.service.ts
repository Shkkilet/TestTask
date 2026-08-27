import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ShortUrlDetails, ShortUrlDto } from './models';

@Injectable({ providedIn: 'root' })
export class ShortUrlService {
  private readonly baseUrl = '/api/shorturl';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ShortUrlDto[]> {
    return this.http.get<ShortUrlDto[]>(this.baseUrl);
  }

  getById(id: string): Observable<ShortUrlDto> {
    return this.http.get<ShortUrlDto>(`${this.baseUrl}/${id}`);
  }

  create(originalUrl: string): Observable<ShortUrlDto> {
    return this.http.post<ShortUrlDto>(this.baseUrl, { originalUrl });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
