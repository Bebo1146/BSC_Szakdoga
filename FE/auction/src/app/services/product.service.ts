import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Product, NewProduct } from '../models/product.model';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private readonly http = inject(HttpClient);
  // use relative path so the Angular dev proxy will forward to the backend
  // private readonly apiUrl = 'http://localhost:5215/api/BffProxy';
  private readonly apiUrl = '/api/BffProxy';

  // Cookie-based flow: let interceptor handle cookies/auth.
  private readonly defaultOptions = { withCredentials: true };

  getAllProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/products/getall`, {
      ...this.defaultOptions,
    });
  }

  getMyProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/products/my-products`, {
      ...this.defaultOptions,
    });
  }

  // new: GET /api/BffProxy/products/my-bids
  getMyBids(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/products/my-bids`, {
      ...this.defaultOptions,
    });
  }

  addMultipleProducts(products: NewProduct[]): Observable<Product[]> {
    return this.http.post<Product[]>(`${this.apiUrl}/products/addMultiple`, products, {
      ...this.defaultOptions,
    });
  }

  updateProduct(id: string, product: Partial<NewProduct>): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/products/${id}`, product, {
      ...this.defaultOptions,
    });
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/products/${id}`, {
      ...this.defaultOptions,
    });
  }

  // POST /api/BffProxy/products/mark-sold — accepts a list of product ids
  markAsSold(productIds: string[]): Observable<any> {
    return this.http.post<any>(
      `${this.apiUrl}/products/mark-sold`,
      productIds,
      { ...this.defaultOptions }
    );
  }

  markAsRejected(requests: { id: string; reason?: string }[]): Observable<any> {
    return this.http.post<any>(
      `${this.apiUrl}/products/mark-rejected`,
      requests,
      { ...this.defaultOptions }
    ).pipe(
      tap(res => console.log('markAsRejected response:', res))
    );
  }

  markAsAccepted(productIds: string[]): Observable<any> {
    return this.http.post<any>(
      `${this.apiUrl}/products/mark-accepted`,
      productIds,
      { ...this.defaultOptions }
    ).pipe(
      tap(res => console.log('markAsAccepted response:', res))
    );
  }
}