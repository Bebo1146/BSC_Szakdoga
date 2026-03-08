import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Navbar } from '../navbar/navbar';
import { ProductTableComponent } from '../product-table/product-table.component';
import { ProductFormModalComponent } from '../product-form-modal/product-form-modal.component';
import { ProductToolbarComponent } from '../product-toolbar/product-toolbar.component';

export enum ProductStatus {
  Draft = 0,
  Active = 1,
  Sold = 2,
  Expired = 3,
  Cancelled = 4,
  UnderReview = 5,
}

export enum TransactionStatus {
  Pending = 0,
  PaymentReceived = 1,
  Shipped = 2,
  Delivered = 3,
  Completed = 4,
  Disputed = 5,
  Cancelled = 6,
}

export interface FeedbackDto {
  reviewerId: string;
  reviewerUsername: string;
  rating: number;
  comment: string | null;
  createdAt: string;
}

export interface Product {
  id: string;
  name: string;
  description: string;
  category: string;
  status: ProductStatus;
  imageUrl: string;
  startingPrice: number;
  currentBid: number | null;
  reservePrice: number | null;
  auctionStartTime: string;
  auctionEndTime: string;
  totalBids: number;
  highestBidderId: string | null;
  highestBidderUsername: string | null;
  sellerId: string;
  sellerUsername: string;
  createdAt: string;
  updatedAt: string | null;
  isCompleted: boolean;
  transactionStatus: TransactionStatus | null;
  feedback: FeedbackDto | null;
  isActive: boolean;
  hasEnded: boolean;
  timeRemaining: string | null;
}

export interface NewProduct {
  name: string;
  description: string;
  category: string;
  status: ProductStatus;
  imageUrl: string;
  startingPrice: number;
  reservePrice: number | null;
  auctionStartTime: string;
  auctionEndTime: string;
}

@Component({
  selector: 'app-product',
  standalone: true,
  imports: [CommonModule, FormsModule, Navbar, ProductTableComponent, ProductFormModalComponent, ProductToolbarComponent],
  templateUrl: './product.component.html',
  styleUrls: ['./product.component.scss'],
})
export class ProductComponent {
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
      .get<Product[]>(this.apiUrl + '/getAll', { headers: this.getAuthHeaders() })
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

  statusLabel(status: ProductStatus): string {
    const labels: Record<ProductStatus, string> = {
      [ProductStatus.Draft]: 'Draft',
      [ProductStatus.Active]: 'Active',
      [ProductStatus.Sold]: 'Sold',
      [ProductStatus.Expired]: 'Expired',
      [ProductStatus.Cancelled]: 'Cancelled',
      [ProductStatus.UnderReview]: 'Under Review',
    };
    return labels[status] ?? 'Unknown';
  }

  transactionLabel(status: TransactionStatus | null): string {
    if (status === null) return '—';
    const labels: Record<TransactionStatus, string> = {
      [TransactionStatus.Pending]: 'Pending',
      [TransactionStatus.PaymentReceived]: 'Payment Received',
      [TransactionStatus.Shipped]: 'Shipped',
      [TransactionStatus.Delivered]: 'Delivered',
      [TransactionStatus.Completed]: 'Completed',
      [TransactionStatus.Disputed]: 'Disputed',
      [TransactionStatus.Cancelled]: 'Cancelled',
    };
    return labels[status] ?? 'Unknown';
  }

  formatTimeRemaining(timeRemaining: string | null): string {
    if (!timeRemaining) return 'Ended';
    // Parse .NET TimeSpan format: "d.hh:mm:ss.fffffff"
    const match = timeRemaining.match(/^(\d+)\.(\d{2}):(\d{2}):(\d{2})/);
    if (match) {
      const [, days, hours, minutes] = match;
      const d = parseInt(days, 10);
      const h = parseInt(hours, 10);
      const m = parseInt(minutes, 10);
      if (d > 0) return `${d}d ${h}h`;
      if (h > 0) return `${h}h ${m}m`;
      return `${m}m`;
    }
    return timeRemaining;
  }

  formatCurrency(value: number | null): string {
    if (value === null) return '—';
    return '$' + value.toFixed(2);
  }

  filtered = computed(() => {
    const q = this.query().trim().toLowerCase();
    const status = this.statusFilter();
    let rows = this.products();

    if (status !== 'All') {
      const statusEnum = ProductStatus[status as keyof typeof ProductStatus];
      rows = rows.filter((r) => r.status === statusEnum);
    }
    if (q) {
      rows = rows.filter(
        (r) =>
          r.name.toLowerCase().includes(q) ||
          r.category.toLowerCase().includes(q) ||
          r.description.toLowerCase().includes(q) ||
          r.sellerUsername.toLowerCase().includes(q)
      );
    }

    switch (this.sort()) {
      case 'Name A-Z':
        rows = [...rows].sort((a, b) => a.name.localeCompare(b.name));
        break;
      case 'Ending Soon':
        rows = [...rows].sort(
          (a, b) =>
            new Date(a.auctionEndTime).getTime() - new Date(b.auctionEndTime).getTime()
        );
        break;
      case 'Most Bids':
        rows = [...rows].sort((a, b) => b.totalBids - a.totalBids);
        break;
      default:
        rows = [...rows].sort(
          (a, b) =>
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        break;
    }

    return rows;
  });

  onSelectionChange(newSet: Set<string>): void {
    this.selectedIds.set(newSet);
  }

  onAddProduct(): void {
    const now = new Date();
    const weekLater = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
    this.newProduct.set({
      name: '',
      description: '',
      category: '',
      status: ProductStatus.Draft,
      imageUrl: '',
      startingPrice: 0,
      reservePrice: null,
      auctionStartTime: now.toISOString().slice(0, 16),
      auctionEndTime: weekLater.toISOString().slice(0, 16),
    });
    this.showAddModal.set(true);
  }

  closeModal(): void {
    this.showAddModal.set(false);
  }

  updateNewProduct<K extends keyof NewProduct>(field: K, value: NewProduct[K]): void {
    this.newProduct.set({ ...this.newProduct(), [field]: value });
  }

  saveProduct(): void {
    const product = this.newProduct();

    if (!product.name.trim() || !product.category.trim()) {
      alert('Name and Category are required.');
      return;
    }
    if (product.startingPrice <= 0) {
      alert('Starting price must be greater than 0.');
      return;
    }

    this.saving.set(true);

    this.http
      .post<Product[]>(this.apiUrl + '/addMultiple', [product], {
        headers: this.getAuthHeaders(),
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.showAddModal.set(false);
          this.loadProducts();
        },
        error: (err) => {
          this.saving.set(false);
          alert('Failed to add product.');
          console.error(err);
        },
      });
  }

  onProductChange(updated: NewProduct): void {
    this.newProduct.set(updated);
  }
}