import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Navbar } from '../navbar/navbar';
import { ProductTableComponent } from '../product-table/product-table.component';
import { ProductToolbarComponent } from '../product-toolbar/product-toolbar.component';
import { ProductService } from '../services/product.service';
import { Product, ProductStatus } from '../models/product.model';
import { RejectModalComponent } from '../reject-modal/reject-modal.component';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, FormsModule, Navbar, ProductTableComponent, ProductToolbarComponent, RejectModalComponent],
  templateUrl: './admin-products.component.html',
  styleUrls: ['./admin-products.component.scss'],
})
export class AdminProductsComponent implements OnInit {
  private readonly productService = inject(ProductService);

  readonly ProductStatus = ProductStatus;

  query = signal('');
  statusFilter = signal<'All' | 'Active' | 'Draft' | 'Sold' | 'Expired'>('All');
  sort = signal<'Newest' | 'Name A-Z' | 'Ending Soon' | 'Most Bids'>('Newest');
  selectedIds = signal<Set<string>>(new Set());

  loading = signal(false);
  error = signal<string | null>(null);
  products = signal<Product[]>([]);

  showRejectModal = signal(false);
  pendingDeleteProduct = signal<Product | null>(null);

  filtered = computed(() => {
    const q = this.query().trim().toLowerCase();
    const status = this.statusFilter();
    let rows = this.products();

    // only show Draft and Active products on the admin page
    rows = rows.filter(
      (p) => p.status === ProductStatus.Draft || p.status === ProductStatus.Active
    );

    if (status && status !== 'All') {
      rows = rows.filter((r) =>
        String(r.status).toLowerCase() === status.toLowerCase() ||
        ProductStatus[r.status]?.toLowerCase() === status.toLowerCase()
      );
    }

    if (q) {
      rows = rows.filter(
        (r) =>
          r.name.toLowerCase().includes(q) ||
          r.description?.toLowerCase().includes(q) ||
          r.category?.toLowerCase().includes(q)
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
  }

  loadProducts(): void {
    this.loading.set(true);
    this.error.set(null);

    this.productService.getAllProducts().subscribe({
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

  onDeleteSelected(): void {
    const ids = Array.from(this.selectedIds());
    if (!ids.length) return;

    const confirmed = confirm(`Are you sure you want to delete ${ids.length} product(s)?`);
    if (!confirmed) return;

    const deletes = ids.map((id) =>
      this.productService.deleteProduct(id).subscribe({
        next: () => {
          this.products.update((prev) => prev.filter((p) => p.id !== id));
        },
        error: (err) => {
          console.error(`Failed to delete product ${id}`, err);
        },
      })
    );

    this.selectedIds.set(new Set());
  }

  onDeleteProduct(p: Product): void {
    if (!p) return;
    this.pendingDeleteProduct.set(p);
    this.showRejectModal.set(true);
  }

  onRejectConfirmed(reason: string): void {
    const p = this.pendingDeleteProduct();

    if (!p) return;

    this.productService.markAsRejected([{ id: p.id, reason }]).subscribe({
      next: () => {
        this.products.update((prev) => prev.filter((x) => x.id !== p.id));
        this.showRejectModal.set(false);
        this.pendingDeleteProduct.set(null);
      },
      error: (err) => {
        console.error('Failed to reject product', err);
        this.error.set('Failed to reject product.');
      },
    });
  }

  onRejectCancelled(): void {
    this.showRejectModal.set(false);
    this.pendingDeleteProduct.set(null);
  }

  isDraft(p: Product): boolean {
    return p.status === ProductStatus.Draft;
  }

  onAcceptProduct(p: Product): void {
    this.productService.markAsAccepted([p.id]).subscribe({
      next: () => {
        this.products.update((prev) => prev.filter((x) => x.id !== p.id));
      },
      error: (err) => {
        console.error('Failed to accept product', err);
        this.error.set('Failed to accept product.');
      },
    });
  }
}