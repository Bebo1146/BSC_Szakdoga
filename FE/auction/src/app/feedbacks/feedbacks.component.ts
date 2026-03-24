import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
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
export class FeedbacksComponent implements OnInit {
  private themeService = inject(ThemeService);
  private feedbackService = inject(FeedbackService);

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
      },
      error: (err) => {
        this.error.set(err?.error?.Message ?? 'Failed to load feedbacks.');
        this.loading.set(false);
      },
    });
  }
}