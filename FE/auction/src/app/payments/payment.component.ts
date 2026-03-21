import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { switchMap, finalize } from 'rxjs';
import { PaymentService, PaymentResponse } from '../services/payment.service';
import { BidService } from '../services/bid.service';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  styleUrls: ['./payment.component.scss'],
  templateUrl: './payment.component.html',
})
export class PaymentComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private paymentService = inject(PaymentService);
  private bidService = inject(BidService);
  private cdr = inject(ChangeDetectorRef);

  loading = false;
  error = '';
  success = '';
  bid: any = null;

  form = this.fb.group({
    amount: [0],
    cardHolder: ['', Validators.required],
    cardNumber: ['', [Validators.required, Validators.pattern(/^\d{4} \d{4} \d{4} \d{4}$/)]],
    expiry: ['', [Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/)]],
    cvc: ['', [Validators.required, Validators.pattern(/^\d{3,4}$/)]],
  });

  // format card number as user types: 1234 5678 9012 3456
  formatCardNumber(event: Event) {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 16);
    const formatted = digits.match(/.{1,4}/g)?.join(' ') ?? digits;
    this.form.patchValue({ cardNumber: formatted }, { emitEvent: false });
    input.value = formatted;
  }

  // format expiry as MM/YY
  formatExpiry(event: Event) {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 4);
    const formatted = digits.length > 2 ? digits.slice(0, 2) + '/' + digits.slice(2) : digits;
    this.form.patchValue({ expiry: formatted }, { emitEvent: false });
    input.value = formatted;
  }

  ngOnInit() {
    const bidId = this.route.snapshot.queryParamMap.get('bidId');
    if (!bidId) return;

    this.loading = true;

    this.bidService.getBidById(bidId)
      .subscribe({
        next: (b: any) => {
          setTimeout(() => {
            this.bid = b;
            this.form.patchValue({ amount: b?.amount ?? 0 });
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
        error: () => {
          setTimeout(() => {
            this.error = 'Could not load product details.';
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  pay() {
    this.error = '';
    this.success = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const bidId = this.bid?.id ?? this.route.snapshot.queryParamMap.get('bidId');
    if (!bidId) {
      this.error = 'No bid selected to pay for.';
      return;
    }

    const { amount } = this.form.value;
    this.loading = true;

    this.paymentService
      .createPayment({ bidId, amount: amount ?? 0, method: 'card' })
      .pipe(
        switchMap((res: PaymentResponse) => this.paymentService.finalizePayment(res)),
        finalize(() => {
          setTimeout(() => {
            this.loading = false;
            this.cdr.detectChanges();
          });
        })
      )
      .subscribe({
        next: (res: any) => {
          if (res?.status === 'succeeded') {
            this.success = 'Payment succeeded!';
            setTimeout(() => this.router.navigate(['/my-bids']), 1200);
            return;
          }
          this.success = `Payment status: ${res?.status}`;
        },
        error: (err: any) => {
          this.error = err?.error?.error ?? err?.message ?? 'Payment failed.';
        },
      });
  }
}