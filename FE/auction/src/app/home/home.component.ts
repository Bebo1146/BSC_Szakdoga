import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Navbar } from '../navbar/navbar';
import { ProductTableComponent } from '../product-table/product-table.component';
import { ProductFormModalComponent } from '../product-form-modal/product-form-modal.component';
import { ProductToolbarComponent } from '../product-toolbar/product-toolbar.component';
import {
  Product,
  NewProduct,
  ProductStatus,
  TransactionStatus,
} from '../product/product.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, Navbar, ProductTableComponent, ProductFormModalComponent, ProductToolbarComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5124/api/Products';

  readonly ProductStatus = ProductStatus;
  readonly TransactionStatus = TransactionStatus;

  query = signal('');
  statusFilter = signal<'All' | 'Active' | 'Draft' | 'Sold' | 'Expired' | 'Cancelled' | 'UnderReview'>('All');
  sort = signal<'Newest' | 'Name A-Z' | 'Ending Soon' | 'Most Bids'>('Newest');
  selectedIds = signal<Set<string>>(new Set());

  loading = signal(false);
  error = signal<string | null>(null);

  products = signal<Product[]>([]);

  // Add product modal state
  showAddModal = signal(false);
  saving = signal(false);
  newProduct = signal<NewProduct>({
    name: '',
    description: '',
    category: '',
    status: ProductStatus.Draft,
    imageUrl: '',
    startingPrice: 0,
    reservePrice: null,
    auctionStartTime: '',
    auctionEndTime: '',
  });

  filtered = computed(() => {
    let rows = this.products();
    const q = this.query().toLowerCase();
    if (q) {
      rows = rows.filter(
        (r) =>
          r.name.toLowerCase().includes(q) ||
          r.description?.toLowerCase().includes(q) ||
          r.category?.toLowerCase().includes(q)
      );
    }
    const sf = this.statusFilter();
    if (sf !== 'All') {
      const statusMap: Record<string, ProductStatus> = {
        Active: ProductStatus.Active,
        Draft: ProductStatus.Draft,
        Sold: ProductStatus.Sold,
        Expired: ProductStatus.Expired,
        Cancelled: ProductStatus.Cancelled,
        UnderReview: ProductStatus.UnderReview,
      };
      rows = rows.filter((r) => r.status === statusMap[sf]);
    }
    return rows;
  });

  constructor() {
    this.loadProducts();
  }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('access_token') ?? '';
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    });
  }

  loadProducts(): void {
    this.loading.set(true);
    this.error.set(null);

    this.http
      .get<Product[]>(this.apiUrl + '/my-products', { headers: this.getAuthHeaders() })
      .subscribe({
        next: (rows) => {
          this.products.set(rows ?? []);
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set('Failed to load products.');
          console.error(err);
        },
      });
  }

  onSelectionChange(newSet: Set<string>): void {
    this.selectedIds.set(newSet);
  }

  onAddProduct(): void {
    this.showAddModal.set(true);
  }

  closeModal(): void {
    this.showAddModal.set(false);
  }

  updateNewProduct(field: keyof NewProduct, value: any): void {
    this.newProduct.update(p => ({ ...p, [field]: value }));
  }

  saveProduct(): void {
    this.saving.set(true);
    this.http.post<Product>(this.apiUrl, this.newProduct(), { headers: this.getAuthHeaders() })
      .subscribe({
        next: (product) => {
          this.products.update(list => [...list, product]);
          this.closeModal();
          this.saving.set(false);
          this.newProduct.set({
            name: '',
            description: '',
            category: '',
            status: ProductStatus.Draft,
            imageUrl: '',
            startingPrice: 0,
            reservePrice: null,
            auctionStartTime: '',
            auctionEndTime: '',
          });
        },
        error: (err) => {
          this.saving.set(false);
          console.error('Failed to save product', err);
        },
      });
  }

  onProductChange(updated: NewProduct): void {
    this.newProduct.set(updated);
  }
}