import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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

  addMultipleProducts(products: NewProduct[]): Observable<Product[]> {
    console.log('Adding multiple products:', products);

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
}