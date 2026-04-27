import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { switchMap, finalize, catchError, of, map, filter, take, timeout } from 'rxjs';
import { PaymentService, PaymentResponse } from '../services/payment.service';
import { BidService } from '../services/bid.service';
import { ProductService } from '../services/product.service';
import { AuthService } from '../services/auth.service';
import { ThemeService } from '../services/theme.service';

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
  private productService = inject(ProductService);
  private bidService = inject(BidService);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);
  private themeService = inject(ThemeService);

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

  formatCardNumber(event: Event) {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 16);
    const formatted = digits.match(/.{1,4}/g)?.join(' ') ?? digits;
    this.form.patchValue({ cardNumber: formatted }, { emitEvent: false });
    input.value = formatted;
  }

  formatExpiry(event: Event) {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 4);
    const formatted = digits.length > 2 ? digits.slice(0, 2) + '/' + digits.slice(2) : digits;
    this.form.patchValue({ expiry: formatted }, { emitEvent: false });
    input.value = formatted;
  }

  ngOnInit() {
    this.themeService.applyTheme();

    const bidId = this.route.snapshot.queryParamMap.get('bidId');
    if (!bidId) {
      this.error = 'No product selected.';
      return;
    }

    this.loading = true;

    this.authService.userId$.pipe(
      filter(id => id !== null),
      take(1),
      timeout(10_000),
      switchMap(() => this.bidService.getBidById(bidId))
    ).subscribe({
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
          this.error = 'Could not load product details. Please log in and try again.';
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
        switchMap((res: any) => {
          if (res?.status === 'succeeded') {
            return this.productService.markAsSold([bidId]).pipe(
              catchError(() => of(res)),
              map(() => res)
            );
          }
          return of(res);
        }),
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
          this.error = `Unexpected payment status: ${res?.status}`;
        },
        error: (err: any) => {
          this.error = err?.error?.error ?? err?.message ?? 'Payment failed.';
        },
      });
  }
}