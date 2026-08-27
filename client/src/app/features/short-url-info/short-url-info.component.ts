import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ShortUrlService } from '../../core/short-url.service';
import { ShortUrlDto } from '../../core/models';

@Component({
  selector: 'app-short-url-info',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './short-url-info.component.html',
})
export class ShortUrlInfoComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private shortUrlService = inject(ShortUrlService);

  details = signal<ShortUrlDto | null>(null);
  isLoading = signal(true);
  loadError = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loadError.set('Invalid URL id.');
      this.isLoading.set(false);
      return;
    }

    this.shortUrlService.getById(id).subscribe({
      next: (details) => {
        this.details.set(details);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.loadError.set(
          err.status === 404 ? 'This short URL does not exist.' : 'Could not load the details for this URL.'
        );
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/table']);
  }
}
