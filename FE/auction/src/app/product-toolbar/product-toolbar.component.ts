import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-product-toolbar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-toolbar.component.html',
  styleUrls: ['./product-toolbar.component.scss'],
})
export class ProductToolbarComponent {
  @Input() query = '';
  @Input() statusFilter: 'All' | 'Active' | 'Draft' | 'Sold' | 'Expired' | 'Cancelled' | 'UnderReview' = 'All';
  @Input() sort: 'Newest' | 'Name A-Z' | 'Ending Soon' | 'Most Bids' = 'Newest';
  @Input() searchPlaceholder = 'Search products...';
  @Input() showExport = true;
  @Input() showImport = true;
  @Input() showAddButton = true;
  @Input() addButtonText = '+ Add product';

  @Output() queryChange = new EventEmitter<string>();
  @Output() statusFilterChange = new EventEmitter<'All' | 'Active' | 'Draft' | 'Sold' | 'Expired' | 'Cancelled' | 'UnderReview'>();
  @Output() sortChange = new EventEmitter<'Newest' | 'Name A-Z' | 'Ending Soon' | 'Most Bids'>();
  @Output() addProduct = new EventEmitter<void>();
  @Output() export = new EventEmitter<void>();
  @Output() import = new EventEmitter<void>();

  onQueryChange(value: string): void {
    this.queryChange.emit(value);
  }

  onStatusFilterChange(value: string): void {
    this.statusFilterChange.emit(value as any);
  }

  onSortChange(value: string): void {
    this.sortChange.emit(value as any);
  }

  onAddProduct(): void {
    this.addProduct.emit();
  }

  onExport(): void {
    this.export.emit();
  }

  onImport(): void {
    this.import.emit();
  }
}