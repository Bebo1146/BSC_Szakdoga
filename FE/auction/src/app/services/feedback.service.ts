import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { FeedbackItem } from '../feedbacks/feedbacks.component';

export interface FeedbackDto {
  rating: number;
  comment?: string | null;
}

const STORAGE_KEY = 'feedback.submitted';

@Injectable({
  providedIn: 'root',
})
export class FeedbackService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/BffProxy';

  submitFeedback(productId: string, feedback: FeedbackDto): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/products/${productId}/feedback`,
      feedback,
      { withCredentials: true }
    ).pipe(
      tap(() => this.markFeedbackGiven(productId))
    );
  }

  hasFeedbackBeenGiven(productId: string): boolean {
    return this.getSubmittedIds().includes(productId);
  }

  private markFeedbackGiven(productId: string): void {
    const ids = this.getSubmittedIds();
    if (!ids.includes(productId)) {
      ids.push(productId);
      localStorage.setItem(STORAGE_KEY, JSON.stringify(ids));
    }
  }

  private getSubmittedIds(): string[] {
    try {
      return JSON.parse(localStorage.getItem(STORAGE_KEY) ?? '[]');
    } catch {
      return [];
    }
  }

  getMyFeedbacks(): Observable<FeedbackItem[]> {
    return this.http.get<FeedbackItem[]>(
      `${this.apiUrl}/products/my-received-feedback`,
      { withCredentials: true }
    ).pipe(
      tap((data) => console.log('[FeedbackService] getMyFeedbacks response:', data))
    );
  }
}