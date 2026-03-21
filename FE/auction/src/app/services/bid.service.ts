import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Product } from '../models/product.model';
import { map } from 'rxjs/operators';

interface PlaceBidResponse {
  product: any;
  bid: any;
}

@Injectable({ providedIn: 'root' })
export class BidService {
  // use inject() and a relative apiUrl so the dev proxy forwards requests
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/BffProxy';

  // POST /api/products/{id}/bid with { Amount: number } body
  placeBid(productId: string, amount: number): Observable<PlaceBidResponse> {
    const url = `${this.apiUrl}/products/${encodeURIComponent(productId)}/bid`;
    const body = { Amount: amount };
    return this.http.post<PlaceBidResponse>(url, body);
  }

  // GET /api/products/{id} - returns updated product
  getProductById(productId: string): Observable<Product> {
    const url = `${this.apiUrl}/products/${encodeURIComponent(productId)}`;
    return this.http.get<Product>(url);
  }

  // bidId is the product id — fetch product and map to bid shape
  getBidById(bidId: string): Observable<any> {
    return this.getProductById(bidId).pipe(
      map((p: any) => ({
        id: bidId,
        productId: p?.id,
        amount: p?.currentBid ?? p?.startingPrice ?? 0,
        product: p,
      }))
    );
  }
}