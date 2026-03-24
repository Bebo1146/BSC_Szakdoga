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
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, Navbar, ProductTableComponent, ProductFormModalComponent, ProductToolbarComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent {
  private readonly productService = inject(ProductService);

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
      };
      rows = rows.filter((r) => r.status === statusMap[sf]);
    }
    return rows;
  });

  constructor() {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading.set(true);
    this.error.set(null);

    this.productService.getMyProducts().subscribe({
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

  saveProduct(): void {
    const product = this.newProduct();

    console.log('Saving product:', product);

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

  onSelectionChange(newSet: Set<string>): void {
    this.selectedIds.set(newSet);
  }

  onAddProduct(): void {
    this.showAddModal.set(true);
  }

  closeModal(): void {
    this.showAddModal.set(false);
  }

  onProductChange(updated: NewProduct): void {
    this.newProduct.set(updated);
  }
}