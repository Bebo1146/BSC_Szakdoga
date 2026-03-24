import { Component, Input, Output, EventEmitter } from '@angular/core';
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
  @Input() statusFilter: 'All' | 'Active' | 'Draft' | 'Sold' | 'Expired' = 'All';
  @Input() sort: 'Newest' | 'Name A-Z' | 'Ending Soon' | 'Most Bids' = 'Newest';
  @Input() searchPlaceholder = 'Search products...';
  @Input() showExport = true;
  @Input() showImport = true;
  @Input() showAddButton = true;
  @Input() addButtonText = '+ Add product';
  @Input() showStatusSelect = true; // consumers can hide the select
  @Input() statusOptions: string[] = ['All', 'Active', 'Draft', 'Sold', 'Expired'];

  @Output() queryChange = new EventEmitter<string>();
  @Output() statusFilterChange = new EventEmitter<string>();
  @Output() sortChange = new EventEmitter<'Newest' | 'Name A-Z' | 'Ending Soon' | 'Most Bids'>();
  @Output() addClick = new EventEmitter<void>();

  onQueryChange(value: string): void {
    this.queryChange.emit(value);
  }

  onStatusFilterChange(v: string): void {
    this.statusFilterChange.emit(v);
  }

  onAddProduct(): void {
    this.addClick.emit();
  }
}