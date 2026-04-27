import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Navbar } from '../navbar/navbar';
import { ThemeService } from '../services/theme.service';
import { FeedbackService } from '../services/feedback.service';

@Component({
  selector: 'app-feedback',
  standalone: true,
  imports: [CommonModule, FormsModule, Navbar],
  templateUrl: './feedback.component.html',
  styleUrls: ['./feedback.component.scss'],
})
export class FeedbackComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private themeService = inject(ThemeService);
  private feedbackService = inject(FeedbackService);

  productId: string | null = null;
  rating = 3;
  hoveredRating = 0;
  comment = '';
  submitted = false;

  loading = false;
  success = '';
  error = '';

  readonly stars = [1, 2, 3, 4, 5];

  ngOnInit(): void {
    this.themeService.applyTheme();
    this.productId = this.route.snapshot.queryParamMap.get('productId');
    if (!this.productId) {
      this.error = 'No product selected.';
    }
  }

  setRating(star: number): void {
    this.rating = star;
  }

  submit(): void {
    this.submitted = true;

    if (!this.productId) return;
    if (this.rating === 0) {
      this.error = 'Please select at least 1 star.';
      return;
    }
    if (!this.comment.trim()) {
      this.error = 'Please write a comment.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = '';

    this.feedbackService
      .submitFeedback(this.productId, {
        rating: this.rating,
        comment: this.comment.trim(),
      })
      .subscribe({
        next: () => {
          this.success = 'Thank you for your feedback!';
          this.loading = false;
          setTimeout(() => this.router.navigate(['/my-bids']), 1500);
        },
        error: (err) => {
          this.error = err?.error?.Message ?? err?.error?.message ?? 'Failed to submit feedback.';
          this.loading = false;
        },
      });
  }
}