import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NewProduct, ProductStatus } from '../product/product.component';

@Component({
  selector: 'app-product-form-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-form-modal.component.html',
  styleUrls: ['./product-form-modal.component.scss'],
})
export class ProductFormModalComponent {
  @Input() show = false;
  @Input() saving = false;
  @Input() product: NewProduct = {
    name: '',
    description: '',
    category: '',
    status: ProductStatus.Draft,
    imageUrl: '',
    startingPrice: 0,
    reservePrice: null,
    auctionStartTime: '',
    auctionEndTime: '',
  };
  @Input() title = 'Add Product';
  @Input() submitButtonText = 'Add Product';

  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<void>();
  @Output() productChange = new EventEmitter<NewProduct>();

  readonly ProductStatus = ProductStatus;

  updateField<K extends keyof NewProduct>(field: K, value: NewProduct[K]): void {
    const updated = { ...this.product, [field]: value };
    this.productChange.emit(updated);
  }

  onClose(): void {
    this.close.emit();
  }

  onSave(): void {
    this.save.emit();
  }
}