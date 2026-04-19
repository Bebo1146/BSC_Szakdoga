import { TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { ProductBidComponent } from './product-bid.component';

describe('ProductBidComponent', () => {
  let component: ProductBidComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductBidComponent],
    }).compileComponents();

    const fixture = TestBed.createComponent(ProductBidComponent);
    component = fixture.componentInstance;

    // Set default inputs
    component.productId = 'p-test';
    component.highestBid = 100;
    component.currentBid = 100;
    component.minIncrement = 1;
    component.endsAt = new Date(Date.now() + 3600000).toISOString(); // 1 hour from now

    // Trigger ngOnChanges manually with mock SimpleChanges
    component.ngOnChanges({});
  });

  it('should be created', () => {
    expect(component).toBeTruthy();
  });

  describe('form validation', () => {
    it('should have a form with amount control', () => {
      expect(component.form).toBeDefined();
      expect(component.amountControl).toBeDefined();
    });

    it('should be invalid when amount is empty', () => {
      component.amountControl.setValue(null);
      expect(component.form.invalid).toBe(true);
    });

    it('should be invalid when amount is below minimum', () => {
      // min is max(currentBid, highestBid + minIncrement) = max(100, 101) = 101
      component.amountControl.setValue(50);
      expect(component.amountControl.valid).toBe(false);
    });

    it('should be valid when amount meets minimum', () => {
      component.amountControl.setValue(101);
      expect(component.amountControl.valid).toBe(true);
    });

    it('should be valid when amount exceeds minimum', () => {
      component.amountControl.setValue(500);
      expect(component.amountControl.valid).toBe(true);
    });
  });

  describe('minimum calculation', () => {
    it('should use highestBid + minIncrement as minimum when it exceeds currentBid', () => {
      component.highestBid = 200;
      component.currentBid = 100;
      component.minIncrement = 10;
      component.ngOnChanges({});

      component.amountControl.setValue(209);
      expect(component.amountControl.valid).toBe(false);

      component.amountControl.setValue(210);
      expect(component.amountControl.valid).toBe(true);
    });

    it('should use currentBid as minimum when it exceeds highestBid + minIncrement', () => {
      component.highestBid = 50;
      component.currentBid = 200;
      component.minIncrement = 1;
      component.ngOnChanges({});

      // min = max(200, 51) = 200
      component.amountControl.setValue(199);
      expect(component.amountControl.valid).toBe(false);

      component.amountControl.setValue(200);
      expect(component.amountControl.valid).toBe(true);
    });

    it('should handle zero highestBid (no bids yet)', () => {
      component.highestBid = 0;
      component.currentBid = 0;
      component.minIncrement = 1;
      component.ngOnChanges({});

      // min = max(0, 0 + 1) = 1
      component.amountControl.setValue(0);
      expect(component.amountControl.valid).toBe(false);

      component.amountControl.setValue(1);
      expect(component.amountControl.valid).toBe(true);
    });
  });

  describe('endsInPast', () => {
    it('should return false when auction has not ended', () => {
      component.endsAt = new Date(Date.now() + 3600000).toISOString();
      expect(component.endsInPast).toBe(false);
    });

    it('should return true when auction has ended', () => {
      component.endsAt = new Date(Date.now() - 3600000).toISOString();
      expect(component.endsInPast).toBe(true);
    });

    it('should return false when endsAt is undefined', () => {
      component.endsAt = undefined;
      expect(component.endsInPast).toBe(false);
    });
  });

  describe('placeBid', () => {
    let emittedBid: { productId: string; amount: number } | null;

    beforeEach(() => {
      emittedBid = null;
      component.bidPlaced.subscribe((val: { productId: string; amount: number }) => {
        emittedBid = val;
      });
    });

    it('should emit bidPlaced event with correct data', () => {
      component.amountControl.setValue(150);
      component.placeBid();

      expect(emittedBid).toEqual({ productId: 'p-test', amount: 150 });
    });

    it('should reset form after successful bid', () => {
      component.amountControl.setValue(150);
      component.placeBid();

      expect(component.amountControl.value).toBeNull();
    });

    it('should set error message when auction has ended', () => {
      component.endsAt = new Date(Date.now() - 1000).toISOString();
      component.amountControl.setValue(150);
      component.placeBid();

      expect(component.errorMsg).toBe('The auction has ended.');
      expect(emittedBid).toBeNull();
    });

    it('should set error message when form is invalid', () => {
      component.amountControl.setValue(null);
      component.placeBid();

      expect(component.errorMsg).toBe('Invalid amount.');
      expect(emittedBid).toBeNull();
    });

    it('should not emit when amount is below minimum', () => {
      component.amountControl.setValue(1);
      component.placeBid();

      expect(component.errorMsg).toBe('Invalid amount.');
      expect(emittedBid).toBeNull();
    });

    it('should clear error message on new valid bid attempt', () => {
      // First: invalid
      component.amountControl.setValue(null);
      component.placeBid();
      expect(component.errorMsg).toBe('Invalid amount.');

      // Second: valid
      component.amountControl.setValue(200);
      component.placeBid();
      expect(component.errorMsg).toBeNull();
    });
  });
});
