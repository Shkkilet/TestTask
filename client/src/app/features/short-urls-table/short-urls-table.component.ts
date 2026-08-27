import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/auth.service';
import { ShortUrlService } from '../../core/short-url.service';
import { ShortUrlDto } from '../../core/models';

@Component({
  selector: 'app-short-urls-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './short-urls-table.component.html',
})
export class ShortUrlsTableComponent implements OnInit {
  protected auth = inject(AuthService);
  private shortUrlService = inject(ShortUrlService);
  private router = inject(Router);

  urls = signal<ShortUrlDto[]>([]);
  isLoading = signal(true);
  loadError = signal<string | null>(null);

  newUrl = '';
  isAdding = signal(false);
  addError = signal<string | null>(null);

  deletingId = signal<number | null>(null);

  ngOnInit(): void {
    this.refresh();
  }

refresh(): void {

  this.isLoading.set(true);

  this.shortUrlService.getAll().subscribe({
    next: (urls) => {

      this.urls.set(urls);
      this.isLoading.set(false);
    },
    error: (err) => {

      this.loadError.set('Could not load short URLs.');
      this.isLoading.set(false);
    },
  });
}
  addUrl(): void {
    const value = this.newUrl.trim();
    if (!value) {
      return;
    }

    this.isAdding.set(true);
    this.addError.set(null);

    this.shortUrlService.create(value).subscribe({
      next: (created) => {

        this.urls.update((current) => {
          return [created, ...current];
        });

        this.newUrl = '';
        this.isAdding.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.isAdding.set(false);
        this.addError.set(
          err.status === 409 ? 'This URL has already been shortened.' : 'Could not create the short URL.'
        );
      },
    });
  }

  deleteUrl(item: ShortUrlDto): void {
    this.deletingId.set(item.id);
    this.shortUrlService.delete(item.id).subscribe({
      next: () => {
        this.urls.update((current) => current.filter((u) => u.id !== item.id));
        this.deletingId.set(null);
      },
      error: () => {
        this.deletingId.set(null);
        this.loadError.set('Could not delete that record.');
      },
    });
  }

  viewDetails(item: ShortUrlDto): void {
    this.router.navigate(['/urls', item.id]);
  }
}
