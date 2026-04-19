import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
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

  // add currentBid input so template bindings work
  @Input() currentBid: number = 0;

  @Output() bidPlaced = new EventEmitter<{ productId: string, amount: number }>();

  form: FormGroup;
  // don't reference this.currentBid at declaration time
  amountControl!: FormControl;
  errorMsg: string | null = null;

  constructor(private fb: FormBuilder) {
    // create control without min validator (will be set/updated in ngOnChanges)
    this.amountControl = new FormControl(null, [Validators.required]);
    // link the control into the form so this.form.invalid works
    this.form = this.fb.group({
      amount: this.amountControl
    });
  }

  private lastMin: number = 0;

  ngOnChanges(changes: SimpleChanges) {
    // compute the minimum allowed value:
    // ensure it's at least currentBid and at least highestBid + minIncrement
    const minFromHighest = (this.highestBid || 0) + (this.minIncrement || 1);
    const min = Math.max(this.currentBid || 0, minFromHighest);

    // only update validators if the min actually changed
    if (min !== this.lastMin) {
      this.lastMin = min;
      this.amountControl.setValidators([Validators.required, Validators.min(min)]);
      this.amountControl.updateValueAndValidity();
    }
  }

  get endsInPast(): boolean {
    if (!this.endsAt) return false;
    const t = new Date(this.endsAt).getTime();
    return Date.now() > t;
  }

  placeBid() {
    this.errorMsg = null;
    
    if (this.endsInPast) {
      this.errorMsg = 'The auction has ended.';
      return;
    }

    // validate the amount control (form is tied to the control)
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