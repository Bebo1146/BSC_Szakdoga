import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Product, ProductStatus } from '../models/product.model';
import { BidService } from '../services/bid.service';
import { AuthService } from '../services/auth.service';
import { ProductBidComponent } from '../product-bid/product-bid.component';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-product-table',
  standalone: true,
  imports: [CommonModule, FormsModule, ProductBidComponent],
  templateUrl: './product-table.component.html',
  styleUrls: ['./product-table.component.scss'],
})
export class ProductTableComponent {
  @Input() products: Product[] = [];
  @Input() selectedIds = new Set<string>();
  @Input() loading = false;
  @Input() error: string | null = null;
  
  @Output() selectionChange = new EventEmitter<Set<string>>();
  @Output() productClick = new EventEmitter<Product>();

  readonly ProductStatus = ProductStatus;

  private readonly bidService = inject(BidService);
  private readonly cdr = inject(ChangeDetectorRef);
  public authService = inject(AuthService);

  isAllSelected(): boolean {
    return this.products.length > 0 && this.products.every(p => this.selectedIds.has(p.id));
  }

  toggleAll(checked: boolean): void {
    const newSet = new Set(this.selectedIds);
    if (checked) {
      this.products.forEach(p => newSet.add(p.id));
    } else {
      this.products.forEach(p => newSet.delete(p.id));
    }
    this.selectionChange.emit(newSet);
  }

  toggleOne(id: string, checked: boolean): void {
    const newSet = new Set(this.selectedIds);
    if (checked) {
      newSet.add(id);
    } else {
      newSet.delete(id);
    }
    this.selectionChange.emit(newSet);
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
    return `$${value.toFixed(2)}`;
  }

  async onBidPlaced(event: { productId: string; amount: number }, row: Product) {
    try {
      await firstValueFrom(this.bidService.placeBid(event.productId, event.amount));

      const updated = await firstValueFrom(this.bidService.getProductById(event.productId));

      this.products = this.products.map(p => (p.id === updated.id ? updated : p));

      this.cdr.detectChanges();
    } catch (err) {
      // handle/log error as appropriate
      console.error('Failed to place bid / refresh product:', err);
    }
  }
}