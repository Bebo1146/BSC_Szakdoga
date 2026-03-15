import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { Navbar } from '../navbar/navbar';
import { ProductTableComponent } from '../product-table/product-table.component';
import { ProductToolbarComponent } from '../product-toolbar/product-toolbar.component';
import { ProductService } from '../services/product.service';
import { Product, ProductStatus, TransactionStatus } from '../models/product.model';

@Component({
  selector: 'app-my-bids',
  standalone: true,
  imports: [CommonModule, FormsModule, Navbar, ProductTableComponent, ProductToolbarComponent],
  templateUrl: './my-bids.component.html',
  styleUrls: ['./my-bids.component.scss'],
})
export class MyBidsComponent implements OnInit {
  private readonly productService = inject(ProductService);

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

    // basic sort implementations matching home.component behavior
    const s = this.sort();
    switch (s) {
      case 'Newest':
        rows = rows.slice().sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        break;
      case 'Name A-Z':
        rows = rows.slice().sort((a, b) => a.name.localeCompare(b.name));
        break;
      case 'Ending Soon':
        rows = rows.slice().sort((a, b) => (a.auctionEndTime > b.auctionEndTime ? 1 : -1));
        break;
      case 'Most Bids':
        rows = rows.slice().sort((a, b) => (b.totalBids || 0) - (a.totalBids || 0));
        break;
    }

    return rows;
  });

  ngOnInit(): void {
    void this.loadProducts();
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
}