import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormControl } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-product-bid',
  templateUrl: './product-bid.component.html',
  styleUrls: ['./product-bid.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class ProductBidComponent implements OnChanges {
  @Input() productId!: string;
  @Input() highestBid: number = 0;
  @Input() minIncrement: number = 1;
  @Input() endsAt?: string | Date;
  @Input() isLoggedIn: boolean = false;

  @Output() bidPlaced = new EventEmitter<{ productId: string, amount: number }>();

  form: FormGroup;
  amountControl: FormControl;
  errorMsg: string | null = null;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      amount: [null, [Validators.required]]
    });
    this.amountControl = this.form.get('amount') as FormControl;
  }

  ngOnChanges(changes: SimpleChanges) {
    const min = (this.highestBid || 0) + (this.minIncrement || 1);
    this.amountControl.setValidators([Validators.required, Validators.min(min)]);
    this.amountControl.updateValueAndValidity();
  }

  get endsInPast(): boolean {
    if (!this.endsAt) return false;
    const t = new Date(this.endsAt).getTime();
    return Date.now() > t;
  }

  placeBid() {
    this.errorMsg = null;

    if (!this.isLoggedIn) {
      this.errorMsg = 'You must be logged in to bid.';
      return;
    }

    if (this.endsInPast) {
      this.errorMsg = 'The auction has ended.';
      return;
    }

    if (this.form.invalid) {
      this.errorMsg = 'Invalid amount.';
      this.amountControl.markAsTouched();
      return;
    }

    const amount = Number(this.amountControl.value);
    this.bidPlaced.emit({ productId: this.productId, amount });
    this.form.reset();
  }
}