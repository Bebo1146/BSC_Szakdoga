import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Navbar } from '../navbar/navbar';
import {
  Product,
  NewProduct,
  ProductStatus,
  TransactionStatus,
} from '../product/product.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, Navbar],
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
          (r.name ?? '').toLowerCase().includes(q) ||
          (r.category ?? '').toLowerCase().includes(q) ||
          (r.description ?? '').toLowerCase().includes(q) ||
          (r.sellerUsername ?? '').toLowerCase().includes(q)
      );
    }

    switch (this.sort()) {
      case 'Name A-Z':
        rows = [...rows].sort((a, b) => (a.name ?? '').localeCompare(b.name ?? ''));
        break;
      case 'Ending Soon':
        rows = [...rows].sort(
          (a, b) =>
            new Date(a.auctionEndTime).getTime() - new Date(b.auctionEndTime).getTime()
        );
        break;
      case 'Most Bids':
        rows = [...rows].sort((a, b) => (b.totalBids ?? 0) - (a.totalBids ?? 0));
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

  isAllSelected = computed(() => {
    const rows = this.filtered();
    if (rows.length === 0) return false;
    const set = this.selectedIds();
    return rows.every((r) => r.id != null && set.has(r.id));
  });

  toggleAll(checked: boolean): void {
    const next = new Set(this.selectedIds());
    const rows = this.filtered();
    if (checked) rows.forEach((r) => { if (r.id) next.add(r.id); });
    else rows.forEach((r) => { if (r.id) next.delete(r.id); });
    this.selectedIds.set(next);
  }

  toggleOne(id: string, checked: boolean): void {
    if (!id) return;
    const next = new Set(this.selectedIds());
    if (checked) next.add(id);
    else next.delete(id);
    this.selectedIds.set(next);
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
      auctionStartTime: now.toISOString(),
      auctionEndTime: weekLater.toISOString(),
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

    const payload = {
      Id: crypto.randomUUID(),
      Name: product.name,
      Description: product.description,
      Category: product.category,
      Status: product.status,
      ImageUrl: product.imageUrl,
      StartingPrice: product.startingPrice,
      ReservePrice: product.reservePrice,
      AuctionStartTime: product.auctionStartTime,
      AuctionEndTime: product.auctionEndTime,
      SellerId: 'vmi1',
      SellerUsername: 'vmi2',
    };

    this.http
      .post<Product[]>(this.apiUrl + '/addMultiple', [payload], {
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
}