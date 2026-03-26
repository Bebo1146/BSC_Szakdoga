import { Component, computed, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Navbar } from '../navbar/navbar';
import { ProductTableComponent } from '../product-table/product-table.component';
import { ProductToolbarComponent } from '../product-toolbar/product-toolbar.component';
import { ProductService } from '../services/product.service';
import { Product, ProductStatus, TransactionStatus } from '../models/product.model';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { FeedbackService } from '../services/feedback.service';
import { AuctionSignalService, AuctionTimeUpdate } from '../services/auction-hub.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-my-bids',
  standalone: true,
  imports: [CommonModule, FormsModule, Navbar, ProductTableComponent, ProductToolbarComponent],
  templateUrl: './my-bids.component.html',
  styleUrls: ['./my-bids.component.scss'],
})
export class MyBidsComponent implements OnInit, OnDestroy {
  private readonly productService = inject(ProductService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly feedbackService = inject(FeedbackService);
  private readonly auctionHub = inject(AuctionSignalService);
  private hubSubscription?: Subscription;

  readonly ProductStatus = ProductStatus;
  readonly TransactionStatus = TransactionStatus;

  query = signal('');
  // restrict statusFilter to the desired set
  statusFilter = signal<'All' | 'Active' | 'Draft' | 'Sold' | 'Expired'>('All');
  sort = signal<'Newest' | 'Name A-Z' | 'Ending Soon' | 'Most Bids'>('Newest');
  selectedIds = signal<Set<string>>(new Set());

  loading = signal(false);
  error = signal<string | null>(null);
  products = signal<Product[]>([]);

  filtered = computed(() => {
    const currentUserId = this.authService.getUserIdSync();
    const currentUsername = this.authService.getPreferredNameSync();
    const q = this.query().trim().toLowerCase();
    const status = this.statusFilter();
    let rows = this.products();

    // hide sold items where feedback has already been given
    rows = rows.filter((p: any) => {
      if (p.status === ProductStatus.Sold && this.feedbackService.hasFeedbackBeenGiven(p.id)) {
        return false;
      }

      // only show expired items if current user is the highest bidder
      const isExpired = p.status === 'expired' || p.status === ProductStatus.Expired;
      if (isExpired) {
        return p.highestBidderId === currentUserId || p.highestBidderUsername === currentUsername;
      }

      return true;
    });

    console.log('After status filter, rows:', rows);

    // apply status filter from toolbar (if not "All")
    if (status && status !== 'All') {
      rows = rows.filter((r: any) => {
        const statusEnum = (ProductStatus as any)[status];
        // support both enum value and lowercase string representations
        return r.status === statusEnum || String(r.status).toLowerCase() === status.toLowerCase();
      });
    }

    console.log('After status filter, rows:', rows);

    // apply search query
    if (q) {
      rows = rows.filter(
        (r: any) =>
          r.name.toLowerCase().includes(q) ||
          r.description?.toLowerCase().includes(q) ||
          r.category?.toLowerCase().includes(q)
      );
    }

    // apply sorting
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
        rows = [...rows].sort((a, b) => (b.totalBids ?? 0) - (a.totalBids ?? 0));
        break;
      default:
        rows = [...rows].sort(
          (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        break;
    }

    return rows;
  });

  ngOnInit(): void {
    void this.loadProducts();
    console.log('userId:', this.authService.getUserIdSync());
    console.log('preferredName:', this.authService.getPreferredNameSync());
  }

  loadProducts(): void {
    this.loading.set(true);
    this.error.set(null);

    this.productService.getMyBids().subscribe({
      next: (rows) => {
        this.products.set(
          (rows ?? []).map((p) => ({
            ...p,
            timeRemainingSeconds: p.timeRemainingSeconds ?? this.parseTimeRemaining(p.timeRemaining),
          }))
        );
        this.loading.set(false);
        this.connectToHub();
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set('Failed to load products.');
        console.error(err);
      },
    });
  }

  private connectToHub(): void {
    this.hubSubscription?.unsubscribe();
    this.auctionHub.start();
    this.hubSubscription = this.auctionHub.updates$.subscribe((updates: AuctionTimeUpdate[]) => {
      this.products.update((current) => {
        let hasChanges = false;
        const next = current.map((p) => {
          const update = updates.find((u) => u.productId === p.id);
          if (!update) return p;
          const statusAsEnum = ProductStatus[update.status as keyof typeof ProductStatus];
          if (
            p.isActive === update.isActive &&
            p.hasEnded === update.hasEnded &&
            p.status === statusAsEnum &&
            p.timeRemainingSeconds === update.timeRemainingSeconds
          )
            return p;
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

  ngOnDestroy(): void {
    this.hubSubscription?.unsubscribe();
    this.auctionHub.stop();
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

  onSelectionChange(newSet: Set<string>): void {
    this.selectedIds.set(newSet);
  }

  onProductClick(p: Product): void {
    console.log('product clicked', p);
  }

  onFeedbackClick(p: Product): void {
    this.router.navigate(['/feedback'], { queryParams: { productId: p.id } });
  }
}