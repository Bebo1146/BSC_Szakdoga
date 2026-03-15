import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class BidService {
  private base = '/api'; // állítsd a backend URL-t

  constructor(private http: HttpClient) {}

  placeBid(productId: string, amount: number): Observable<any> {
    return this.http.post(`${this.base}/bids`, { productId, amount });
  }
}