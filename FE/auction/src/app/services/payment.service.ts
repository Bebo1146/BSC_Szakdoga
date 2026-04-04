import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PaymentRequest {
  bidId?: string;
  amount?: number;
  method?: string;
}

export interface PaymentResponse {
  id: string;
  status: string;
  clientSecret?: string;
  paymentUrl?: string;
}

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  constructor(private http: HttpClient) {}

  // Routed through the BFF which handles session/token forwarding to Payments service
  private apiUrl = '/api/BffProxy/payments';

  createPayment(body: PaymentRequest): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>(this.apiUrl, body, { withCredentials: true });
  }

  confirmPayment(id: string): Observable<{ id: string; status: string }> {
    return this.http.post<{ id: string; status: string }>(
      `${this.apiUrl}/${encodeURIComponent(id)}/confirm`,
      {},
      { withCredentials: true }
    );
  }

  getPayment(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${encodeURIComponent(id)}`, { withCredentials: true });
  }

  finalizePayment(payment: PaymentResponse): Observable<any> {
    if (environment.fakeServer) {
      return this.confirmPayment(payment.id);
    }
    return of(payment);
  }
}