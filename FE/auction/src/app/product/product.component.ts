import { Component, computed, inject, signal, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Navbar } from '../navbar/navbar';
import { ProductTableComponent } from '../product-table/product-table.component';
import { ProductFormModalComponent } from '../product-form-modal/product-form-modal.component';
import { ProductToolbarComponent } from '../product-toolbar/product-toolbar.component';
import { ProductService } from '../services/product.service';
import { AuthService } from '../services/auth.service';
import { AuctionSignalService, AuctionTimeUpdate } from '../services/auction-hub.service';
import { Subscription, interval } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import {
  Product,
  NewProduct,
  ProductStatus,
  TransactionStatus,
} from '../models/product.model';

@Component({
  selector: 'app-product',
  standalone: true,
  imports: [CommonModule, FormsModule, Navbar, ProductTableComponent, ProductFormModalComponent, ProductToolbarComponent],
  templateUrl: './product.component.html',
  styleUrls: ['./product.component.scss'],
})
export class ProductComponent implements OnDestroy {
  private readonly productService = inject(ProductService);
  private readonly authService = inject(AuthService);
  private readonly auctionHub = inject(AuctionSignalService);
  private readonly router = inject(Router);
  private hubSubscription?: Subscription;
  private pollingSubscription?: Subscription;

  readonly ProductStatus = ProductStatus;
  readonly TransactionStatus = TransactionStatus;

  query = signal('');
  sort = signal<'Newest' | 'Name A-Z' | 'Ending Soon' | 'Most Bids'>('Newest');
  selectedIds = signal<Set<string>>(new Set());

  loading = signal(false);
  error = signal<string | null>(null);

  products = signal<Product[]>([]);

  showAddModal = signal(false);
  saving = signal(false);
  newProduct = signal<NewProduct>({
    name: '',
    description: '',
    category: '',
    status: ProductStatus.Draft,
    startingPrice: 0,
    auctionStartTime: '',
    auctionEndTime: '',
  });

  constructor() {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading.set(true);
    this.error.set(null);

    this.productService.getAllProducts().subscribe({
      next: (rows) => {
        this.products.set((rows ?? []).map(p => ({
          ...p,
          timeRemainingSeconds: p.timeRemainingSeconds ?? this.parseTimeRemaining(p.timeRemaining),
        })));
        this.loading.set(false);
        this.connectToHub();
        this.startPolling();
      },
      error: (err) => {
        this.loading.set(false);  
        this.error.set('Failed to load products.');
        console.error(err);
        if (err.status === 401 || err.status === 403) {
          this.router.navigate(['/login']);
        }
      },
    });
  }

  private connectToHub(): void {
    this.hubSubscription?.unsubscribe();
    
    this.auctionHub.start();
    this.hubSubscription = this.auctionHub.updates$.subscribe((updates: AuctionTimeUpdate[]) => {
      this.products.update(current => {
        let hasChanges = false;

        const next = current.map(p => {
          const update = updates.find(u => u.productId === p.id);
          if (!update) return p;

          const statusAsEnum = ProductStatus[update.status as keyof typeof ProductStatus];
          if (
            p.isActive === update.isActive &&
            p.hasEnded === update.hasEnded &&
            p.status === statusAsEnum &&
            p.timeRemainingSeconds === update.timeRemainingSeconds
          ) {
            return p;
          }

          hasChanges = true;
          return {
            ...p,
            isActive: update.isActive,
            hasEnded: update.hasEnded,
            status: statusAsEnum,
            timeRemainingSeconds: update.timeRemainingSeconds,
          };
        });

        return hasChanges ? next : current;
      });
    });
  }

  private startPolling(): void {
    this.pollingSubscription?.unsubscribe();

    this.pollingSubscription = interval(3000).pipe(
      switchMap(() => this.productService.getAllProducts())
    ).subscribe({
      next: (rows) => {
        const incoming = (rows ?? []).map(p => ({
          ...p,
          timeRemainingSeconds: p.timeRemainingSeconds ?? this.parseTimeRemaining(p.timeRemaining),
        }));

        this.products.update(current => {
          const currentMap = new Map(current.map(p => [p.id, p]));
          let hasChanges = false;
          const next: Product[] = [];

          for (const item of incoming) {
            const existing = currentMap.get(item.id);
            if (!existing) {
              hasChanges = true;
              next.push(item);
            } else {
              if (
                existing.currentBid !== item.currentBid ||
                existing.totalBids !== item.totalBids ||
                existing.highestBidderId !== item.highestBidderId ||
                existing.highestBidderUsername !== item.highestBidderUsername ||
                existing.status !== item.status ||
                existing.transactionStatus !== item.transactionStatus ||
                existing.isCompleted !== item.isCompleted ||
                existing.name !== item.name ||
                existing.description !== item.description
              ) {
                hasChanges = true;
                next.push({ ...item, timeRemainingSeconds: existing.timeRemainingSeconds ?? item.timeRemainingSeconds });
              } else {
                next.push(existing);
              }
            }
          }

          if (next.length !== current.length) {
            hasChanges = true;
          }

          return hasChanges ? next : current;
        });
      },
    });
  }

  ngOnDestroy(): void {
    this.hubSubscription?.unsubscribe();
    this.pollingSubscription?.unsubscribe();
    this.auctionHub.stop();
  }

  filtered = computed(() => {
    const q = this.query().trim().toLowerCase();
    const currentUserId = this.authService.getUserIdSync();
    let rows = this.products().filter(
      (r) => r.status === ProductStatus.Active && r.sellerId !== currentUserId
    );

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
          (a, b) => new Date(a.auctionEndTime).getTime() - new Date(b.auctionEndTime).getTime()
        );
        break;
      case 'Most Bids':
        rows = [...rows].sort((a, b) => b.totalBids - a.totalBids);
        break;
      default:
        rows = [...rows].sort(
          (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        break;
    }

    return rows;
  });

  onSelectionChange(newSet: Set<string>): void {
    this.selectedIds.set(newSet);
  }

  onBidUpdated(updated: Product): void {
    this.products.update(current =>
      current.map(p => p.id === updated.id ? { ...updated, timeRemainingSeconds: p.timeRemainingSeconds } : p)
    );
  }

  onAddProduct(): void {
    const now = new Date();
    const weekLater = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
    this.newProduct.set({
      name: '',
      description: '',
      category: '',
      status: ProductStatus.Draft,
      startingPrice: 0,
      auctionStartTime: now.toISOString().slice(0, 16),
      auctionEndTime: weekLater.toISOString().slice(0, 16),
    });
    this.showAddModal.set(true);
  }

  closeModal(): void {
    this.showAddModal.set(false);
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

    this.productService.addMultipleProducts([product]).subscribe({
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

  private parseTimeRemaining(timeRemaining: string | null): number {
    if (!timeRemaining) return 0;
    const parts = timeRemaining.split(':');
    if (parts.length === 3) {
      const dayHour = parts[0].split('.');
      const days = dayHour.length > 1 ? parseInt(dayHour[0]) : 0;
      const hours = parseInt(dayHour[dayHour.length - 1]);
      const minutes = parseInt(parts[1]);
      const seconds = parseInt(parts[2]);
      return days * 86400 + hours * 3600 + minutes * 60 + seconds;
    }
    return 0;
  }
}