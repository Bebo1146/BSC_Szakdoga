import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { BidService } from './bid.service';

describe('BidService', () => {
  let service: BidService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(BidService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('placeBid', () => {
    it('should POST bid with correct URL and body', () => {
      service.placeBid('p-123', 200).subscribe((result) => {
        expect(result.bid).toBeDefined();
        expect(result.product).toBeDefined();
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/p-123/bid');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ Amount: 200 });
      req.flush({
        product: { id: 'p-123', currentBid: 200 },
        bid: { id: 'b-1', amount: 200 },
      });
    });

    it('should URL-encode the product id', () => {
      service.placeBid('p-special/id', 100).subscribe();

      const req = httpTesting.expectOne('/api/BffProxy/products/p-special%2Fid/bid');
      expect(req.request.method).toBe('POST');
      req.flush({ product: {}, bid: {} });
    });

    it('should propagate errors when bid fails', () => {
      service.placeBid('p-1', 50).subscribe({
        error: (err) => {
          expect(err.status).toBe(400);
        },
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/p-1/bid');
      req.flush('Bad Request', { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('getProductById', () => {
    it('should GET product by id', () => {
      service.getProductById('p-42').subscribe((product) => {
        expect(product.id).toBe('p-42');
        expect(product.name).toBe('Test');
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/p-42');
      expect(req.request.method).toBe('GET');
      req.flush({ id: 'p-42', name: 'Test', currentBid: 100 });
    });
  });

  describe('getBidById', () => {
    it('should map product to bid shape', () => {
      service.getBidById('p-42').subscribe((bid) => {
        expect(bid.id).toBe('p-42');
        expect(bid.productId).toBe('p-42');
        expect(bid.amount).toBe(250);
        expect(bid.product).toBeDefined();
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/p-42');
      req.flush({ id: 'p-42', name: 'Test', currentBid: 250, startingPrice: 100 });
    });

    it('should use startingPrice when currentBid is null', () => {
      service.getBidById('p-10').subscribe((bid) => {
        expect(bid.amount).toBe(100);
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/p-10');
      req.flush({ id: 'p-10', currentBid: null, startingPrice: 100 });
    });

    it('should default to 0 when both currentBid and startingPrice are missing', () => {
      service.getBidById('p-empty').subscribe((bid) => {
        expect(bid.amount).toBe(0);
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/p-empty');
      req.flush({ id: 'p-empty' });
    });
  });
});
