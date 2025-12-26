import { Component, computed, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Navbar } from './navbar/navbar';

type ProductStatus = 'Active' | 'Draft' | 'Archived';

interface ProductRow {
  id: string;
  name: string;
  status: ProductStatus;
  category: string;
  inventoryText: string;
  vendor: string;
  imageUrl?: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, Navbar],
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
})
export class App {
  private readonly http = inject(HttpClient);

  storeName = 'S.laz Store';

  query = signal('');
  statusFilter = signal<'All' | ProductStatus>('Active');
  sort = signal<'Newest' | 'Name A-Z'>('Newest');

  selectedIds = signal<Set<string>>(new Set());

  // NEW: loading + error state
  loading = signal(false);
  error = signal<string | null>(null);

  // NEW: start empty; fill from HTTP
  products = signal<ProductRow[]>([]);

  constructor() {
    this.loadProducts();
  }

  loadProducts() {
    this.loading.set(true);
    this.error.set(null);

    this.http.get<ProductRow[]>('http://localhost:5184/api/products').subscribe({
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
      rows = rows.filter((r) => r.status === status);
    }
    if (q) {
      rows = rows.filter(
        (r) =>
          r.name.toLowerCase().includes(q) ||
          r.category.toLowerCase().includes(q) ||
          r.vendor.toLowerCase().includes(q)
      );
    }

    if (this.sort() === 'Name A-Z') {
      rows = [...rows].sort((a, b) => a.name.localeCompare(b.name));
    } else {
      rows = [...rows];
    }

    return rows;
  });

  isAllSelected = computed(() => {
    const rows = this.filtered();
    if (rows.length === 0) return false;
    const set = this.selectedIds();
    return rows.every((r) => set.has(r.id));
  });

  toggleAll(checked: boolean) {
    const next = new Set(this.selectedIds());
    const rows = this.filtered();
    if (checked) rows.forEach((r) => next.add(r.id));
    else rows.forEach((r) => next.delete(r.id));
    this.selectedIds.set(next);
  }

  toggleOne(id: string, checked: boolean) {
    const next = new Set(this.selectedIds());
    if (checked) next.add(id);
    else next.delete(id);
    this.selectedIds.set(next);
  }

  onAddProduct() {
    alert('Add product (later: modal / route)');
  }
}
