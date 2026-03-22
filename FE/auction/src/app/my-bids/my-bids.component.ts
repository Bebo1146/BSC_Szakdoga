import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { Navbar } from '../navbar/navbar';
import { ProductTableComponent } from '../product-table/product-table.component';
import { ProductToolbarComponent } from '../product-toolbar/product-toolbar.component';
import { ProductService } from '../services/product.service';
import { Product, ProductStatus, TransactionStatus } from '../models/product.model';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-my-bids',
  standalone: true,
  imports: [CommonModule, FormsModule, Navbar, ProductTableComponent, ProductToolbarComponent],
  templateUrl: './my-bids.component.html',
  styleUrls: ['./my-bids.component.scss'],
})
export class MyBidsComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly ProductStatus = ProductStatus;
  readonly TransactionStatus = TransactionStatus;

  query = signal('');
  statusFilter = signal<'All' | 'Active' | 'Draft' | 'Sold' | 'Expired' | 'Cancelled' | 'UnderReview'>('All');
  sort = signal<'Newest' | 'Name A-Z' | 'Ending Soon' | 'Most Bids'>('Newest');
  selectedIds = signal<Set<string>>(new Set());

  loading = signal(false);
  error = signal<string | null>(null);
  products = signal<Product[]>([]);

  filtered = computed(() => {
    const currentUserId = this.authService.getUserIdSync();
    const currentUsername = this.authService.getPreferredNameSync();

    return this.products()
      .filter((p: any) => {
        // only show expired items if current user is the highest bidder
        const isExpired = p.status === 'expired' || p.status === ProductStatus.Expired;
        if (isExpired) {
          return p.highestBidderId === currentUserId
            || p.highestBidderUsername === currentUsername;
        }

        return true;
      })
      .filter(
        (r) =>
          r.name.toLowerCase().includes(this.query().toLowerCase()) ||
          r.description?.toLowerCase().includes(this.query().toLowerCase()) ||
          r.category?.toLowerCase().includes(this.query().toLowerCase())
      );
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

  onProductClick(p: Product): void {
    console.log('product clicked', p);
  }

  onFeedbackClick(p: Product): void {
    this.router.navigate(['/feedback'], { queryParams: { productId: p.id } });
  }
}