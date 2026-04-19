import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ProductService } from './product.service';
import { NewProduct, Product, ProductStatus, TransactionStatus } from '../models/product.model';

function createMockProduct(overrides: Partial<Product> = {}): Product {
  return {
    id: 'p-1',
    name: 'Test Product',
    description: 'A test product',
    category: 'Electronics',
    status: ProductStatus.Active,
    startingPrice: 100,
    currentBid: 150,
    auctionStartTime: '2026-01-01T00:00:00Z',
    auctionEndTime: '2026-12-31T23:59:59Z',
    totalBids: 3,
    highestBidderId: 'user-2',
    highestBidderUsername: 'bidder1',
    sellerId: 'user-1',
    sellerUsername: 'seller1',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    isCompleted: false,
    transactionStatus: null,
    feedback: null,
    isActive: true,
    hasEnded: false,
    timeRemaining: '1.00:00:00',
    ...overrides,
  };
}

describe('ProductService', () => {
  let service: ProductService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(ProductService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAllProducts', () => {
    it('should fetch all products via GET', () => {
      const mockProducts = [createMockProduct(), createMockProduct({ id: 'p-2', name: 'Product 2' })];

      service.getAllProducts().subscribe((products) => {
        expect(products).toHaveLength(2);
        expect(products[0].name).toBe('Test Product');
        expect(products[1].name).toBe('Product 2');
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/getall');
      expect(req.request.method).toBe('GET');
      expect(req.request.withCredentials).toBe(true);
      req.flush(mockProducts);
    });

    it('should return empty array when no products exist', () => {
      service.getAllProducts().subscribe((products) => {
        expect(products).toHaveLength(0);
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/getall');
      req.flush([]);
    });
  });

  describe('getMyProducts', () => {
    it('should fetch user products via GET', () => {
      const mockProducts = [createMockProduct({ sellerId: 'current-user' })];

      service.getMyProducts().subscribe((products) => {
        expect(products).toHaveLength(1);
        expect(products[0].sellerId).toBe('current-user');
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/my-products');
      expect(req.request.method).toBe('GET');
      expect(req.request.withCredentials).toBe(true);
      req.flush(mockProducts);
    });
  });

  describe('getMyBids', () => {
    it('should fetch bidded products via GET', () => {
      const mockProducts = [createMockProduct()];

      service.getMyBids().subscribe((products) => {
        expect(products).toHaveLength(1);
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/my-bids');
      expect(req.request.method).toBe('GET');
      expect(req.request.withCredentials).toBe(true);
      req.flush(mockProducts);
    });
  });

  describe('addMultipleProducts', () => {
    it('should POST new products', () => {
      const newProducts: NewProduct[] = [
        {
          name: 'New Item',
          description: 'Desc',
          category: 'Art',
          status: ProductStatus.Draft,
          startingPrice: 50,
          auctionStartTime: '2026-06-01T00:00:00Z',
          auctionEndTime: '2026-06-08T00:00:00Z',
        },
      ];

      service.addMultipleProducts(newProducts).subscribe((result) => {
        expect(result).toHaveLength(1);
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/addMultiple');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newProducts);
      expect(req.request.withCredentials).toBe(true);
      req.flush([createMockProduct({ name: 'New Item' })]);
    });
  });

  describe('updateProduct', () => {
    it('should PUT product updates', () => {
      const update = { name: 'Updated Name' };

      service.updateProduct('p-1', update).subscribe((result) => {
        expect(result.name).toBe('Updated Name');
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/p-1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(update);
      req.flush(createMockProduct({ name: 'Updated Name' }));
    });
  });

  describe('deleteProduct', () => {
    it('should DELETE a product by id', () => {
      service.deleteProduct('p-1').subscribe();

      const req = httpTesting.expectOne('/api/BffProxy/products/p-1');
      expect(req.request.method).toBe('DELETE');
      expect(req.request.withCredentials).toBe(true);
      req.flush(null);
    });
  });

  describe('markAsSold', () => {
    it('should POST product ids to mark-sold', () => {
      const ids = ['p-1', 'p-2'];

      service.markAsSold(ids).subscribe();

      const req = httpTesting.expectOne('/api/BffProxy/products/mark-sold');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(ids);
      req.flush({ success: ['p-1', 'p-2'], failed: [] });
    });
  });

  describe('markAsRejected', () => {
    it('should POST rejection requests with reasons', () => {
      const requests = [
        { id: 'p-1', reason: 'Inappropriate' },
        { id: 'p-2', reason: 'Duplicate' },
      ];

      service.markAsRejected(requests).subscribe();

      const req = httpTesting.expectOne('/api/BffProxy/products/mark-rejected');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(requests);
      req.flush({ success: ['p-1', 'p-2'], failed: [] });
    });
  });

  describe('markAsAccepted', () => {
    it('should POST product ids to mark-accepted', () => {
      const ids = ['p-1'];

      service.markAsAccepted(ids).subscribe();

      const req = httpTesting.expectOne('/api/BffProxy/products/mark-accepted');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(ids);
      req.flush({ success: ['p-1'], failed: [] });
    });
  });

  describe('error handling', () => {
    it('should propagate HTTP errors for getAllProducts', () => {
      service.getAllProducts().subscribe({
        error: (err) => {
          expect(err.status).toBe(401);
        },
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/getall');
      req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
    });

    it('should propagate HTTP errors for addMultipleProducts', () => {
      service.addMultipleProducts([]).subscribe({
        error: (err) => {
          expect(err.status).toBe(500);
        },
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/addMultiple');
      req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
    });
  });
});
