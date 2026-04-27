import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription, interval } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { Navbar } from '../navbar/navbar';
import { ThemeService } from '../services/theme.service';
import { FeedbackService } from '../services/feedback.service';

export interface FeedbackItem {
  id: string;
  productId: string;
  productName: string;
  rating: number;
  comment?: string | null;
  createdAt: string;
  buyerUsername: string;
}

@Component({
  selector: 'app-feedbacks',
  standalone: true,
  imports: [CommonModule, Navbar],
  templateUrl: './feedbacks.component.html',
  styleUrls: ['./feedbacks.component.scss'],
})
export class FeedbacksComponent implements OnInit, OnDestroy {
  private themeService = inject(ThemeService);
  private feedbackService = inject(FeedbackService);
  private pollingSubscription?: Subscription;

  feedbacks = signal<FeedbackItem[]>([]);
  loading = signal(true);
  error = signal('');

  readonly stars = [1, 2, 3, 4, 5];

  averageRating = computed(() => {
    const f = this.feedbacks();
    if (!f.length) return 0;
    return f.reduce((sum, item) => sum + item.rating, 0) / f.length;
  });

  ngOnInit(): void {
    this.themeService.applyTheme();
    this.feedbackService.getMyFeedbacks().subscribe({
      next: (data) => {
        this.feedbacks.set(data);
        this.loading.set(false);
        this.startPolling();
      },
      error: (err) => {
        this.error.set(err?.error?.Message ?? 'Failed to load feedbacks.');
        this.loading.set(false);
      },
    });
  }

  private startPolling(): void {
    this.pollingSubscription = interval(3000)
      .pipe(switchMap(() => this.feedbackService.getMyFeedbacks()))
      .subscribe({
        next: (incoming) => {
          this.feedbacks.update((current) => {
            const currentMap = new Map(current.map((f) => [f.id, f]));
            let hasChanges = false;
            const next: FeedbackItem[] = [];

            for (const item of incoming) {
              const existing = currentMap.get(item.id);
              if (!existing) {
                hasChanges = true;
                next.push(item);
              } else {
                if (
                  existing.rating !== item.rating ||
                  existing.comment !== item.comment ||
                  existing.productName !== item.productName
                ) {
                  hasChanges = true;
                  next.push(item);
                } else {
                  next.push(existing);
                }
              }
            }

            if (next.length !== current.length) {
              hasChanges = true;
            }

            return hasChanges ? next : current;
          });
        },
      });
  }

  ngOnDestroy(): void {
    this.pollingSubscription?.unsubscribe();
  }
}