import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Navbar } from '../navbar/navbar';
import { ProductTableComponent } from '../product-table/product-table.component';
import { ProductFormModalComponent } from '../product-form-modal/product-form-modal.component';
import { ProductToolbarComponent } from '../product-toolbar/product-toolbar.component';
import { ProductService } from '../services/product.service';
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
export class ProductComponent {
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
}