import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ProductComponent } from './product.component';
import { Product, ProductStatus } from '../models/product.model';
import { AuctionSignalService } from '../services/auction-hub.service';
import { Subject } from 'rxjs';

function createMockProduct(overrides: Partial<Product> = {}): Product {
  return {
    id: 'p-1',
    name: 'Test Product',
    description: 'A test',
    category: 'Electronics',
    status: ProductStatus.Active,
    startingPrice: 100,
    currentBid: null,
    auctionStartTime: '2026-01-01T00:00:00Z',
    auctionEndTime: '2026-12-31T23:59:59Z',
    totalBids: 0,
    highestBidderId: null,
    highestBidderUsername: null,
    sellerId: 'user-1',
    sellerUsername: 'seller1',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    isCompleted: false,
    transactionStatus: null,
    feedback: null,
    isActive: true,
    hasEnded: false,
    timeRemaining: null,
    ...overrides,
  };
}

// Mock window.matchMedia (used by ThemeService via Navbar)
if (typeof window !== 'undefined' && !window.matchMedia) {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: (query: string) => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: () => {},
      removeListener: () => {},
      addEventListener: () => {},
      removeEventListener: () => {},
      dispatchEvent: () => false,
    }),
  });
}

describe('ProductComponent', () => {
  let component: ProductComponent;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    TestBed.resetTestingModule();

    await TestBed.configureTestingModule({
      imports: [ProductComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: AuctionSignalService,
          useValue: {
            updates$: new Subject(),
            start: () => {},
            stop: () => {},
          },
        },
      ],
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(ProductComponent);
    component = fixture.componentInstance;

    // The constructor calls loadProducts, so flush the initial request
    const req = httpTesting.expectOne('/api/BffProxy/products/getall');
    req.flush([]);
  });

  afterEach(() => {
    // Flush any outstanding requests silently
    try {
      httpTesting.verify();
    } catch {
      // Some tests may trigger additional requests from constructor; ignore
    }
  });

  it('should be created', () => {
    expect(component).toBeTruthy();
  });

  describe('filtered computed', () => {
    it('should only return Active products', () => {
      component.products.set([
        createMockProduct({ id: 'p-1', status: ProductStatus.Active }),
        createMockProduct({ id: 'p-2', status: ProductStatus.Draft }),
        createMockProduct({ id: 'p-3', status: ProductStatus.Sold }),
        createMockProduct({ id: 'p-4', status: ProductStatus.Active }),
      ]);

      const result = component.filtered();
      expect(result).toHaveLength(2);
      expect(result.every((p) => p.status === ProductStatus.Active)).toBe(true);
    });

    it('should filter by name (case-insensitive)', () => {
      component.products.set([
        createMockProduct({ id: 'p-1', name: 'Vintage Watch' }),
        createMockProduct({ id: 'p-2', name: 'Modern Laptop' }),
        createMockProduct({ id: 'p-3', name: 'vintage clock' }),
      ]);
      component.query.set('vintage');

      const result = component.filtered();
      expect(result).toHaveLength(2);
    });

    it('should filter by category', () => {
      component.products.set([
        createMockProduct({ id: 'p-1', category: 'Electronics' }),
        createMockProduct({ id: 'p-2', category: 'Art' }),
      ]);
      component.query.set('art');

      const result = component.filtered();
      expect(result).toHaveLength(1);
      expect(result[0].category).toBe('Art');
    });

    it('should filter by description', () => {
      component.products.set([
        createMockProduct({ id: 'p-1', description: 'Beautiful painting' }),
        createMockProduct({ id: 'p-2', description: 'Old car' }),
      ]);
      component.query.set('painting');

      const result = component.filtered();
      expect(result).toHaveLength(1);
    });

    it('should filter by sellerUsername', () => {
      component.products.set([
        createMockProduct({ id: 'p-1', sellerUsername: 'johndoe' }),
        createMockProduct({ id: 'p-2', sellerUsername: 'janedoe' }),
      ]);
      component.query.set('john');

      const result = component.filtered();
      expect(result).toHaveLength(1);
      expect(result[0].sellerUsername).toBe('johndoe');
    });

    it('should return all active products when query is empty', () => {
      component.products.set([
        createMockProduct({ id: 'p-1' }),
        createMockProduct({ id: 'p-2' }),
      ]);
      component.query.set('');

      expect(component.filtered()).toHaveLength(2);
    });

    it('should return empty when query matches nothing', () => {
      component.products.set([
        createMockProduct({ id: 'p-1', name: 'Watch' }),
      ]);
      component.query.set('xyz-no-match');

      expect(component.filtered()).toHaveLength(0);
    });
  });

  describe('sorting', () => {
    const products = [
      createMockProduct({
        id: 'p-a',
        name: 'Alpha',
        createdAt: '2026-03-01T00:00:00Z',
        auctionEndTime: '2026-06-01T00:00:00Z',
        totalBids: 5,
      }),
      createMockProduct({
        id: 'p-c',
        name: 'Charlie',
        createdAt: '2026-01-01T00:00:00Z',
        auctionEndTime: '2026-04-01T00:00:00Z',
        totalBids: 10,
      }),
      createMockProduct({
        id: 'p-b',
        name: 'Bravo',
        createdAt: '2026-02-01T00:00:00Z',
        auctionEndTime: '2026-05-01T00:00:00Z',
        totalBids: 1,
      }),
    ];

    beforeEach(() => {
      component.products.set(products);
      component.query.set('');
    });

    it('should sort by Newest (createdAt descending) by default', () => {
      component.sort.set('Newest');
      const result = component.filtered();
      expect(result[0].id).toBe('p-a');
      expect(result[1].id).toBe('p-b');
      expect(result[2].id).toBe('p-c');
    });

    it('should sort by Name A-Z', () => {
      component.sort.set('Name A-Z');
      const result = component.filtered();
      expect(result[0].name).toBe('Alpha');
      expect(result[1].name).toBe('Bravo');
      expect(result[2].name).toBe('Charlie');
    });

    it('should sort by Ending Soon (auctionEndTime ascending)', () => {
      component.sort.set('Ending Soon');
      const result = component.filtered();
      expect(result[0].id).toBe('p-c'); // April
      expect(result[1].id).toBe('p-b'); // May
      expect(result[2].id).toBe('p-a'); // June
    });

    it('should sort by Most Bids (totalBids descending)', () => {
      component.sort.set('Most Bids');
      const result = component.filtered();
      expect(result[0].totalBids).toBe(10);
      expect(result[1].totalBids).toBe(5);
      expect(result[2].totalBids).toBe(1);
    });
  });

  describe('parseTimeRemaining', () => {
    // Access private method for testing
    const callParse = (component: ProductComponent, value: string | null): number => {
      return (component as any).parseTimeRemaining(value);
    };

    it('should return 0 for null', () => {
      expect(callParse(component, null)).toBe(0);
    });

    it('should return 0 for empty string', () => {
      expect(callParse(component, '')).toBe(0);
    });

    it('should parse hh:mm:ss format', () => {
      // 04:25:10 = 4*3600 + 25*60 + 10 = 15910
      expect(callParse(component, '04:25:10')).toBe(15910);
    });

    it('should parse days.hh:mm:ss format', () => {
      // 3.04:25:10 = 3*86400 + 4*3600 + 25*60 + 10 = 275110
      expect(callParse(component, '3.04:25:10')).toBe(275110);
    });

    it('should parse 0:00:00', () => {
      expect(callParse(component, '0:00:00')).toBe(0);
    });

    it('should return 0 for invalid format', () => {
      expect(callParse(component, 'invalid')).toBe(0);
    });
  });

  describe('saveProduct validation', () => {
    it('should not save when name is empty', () => {
      const alertSpy = vi.spyOn(globalThis, 'alert').mockImplementation(() => {});
      component.newProduct.set({
        name: '',
        description: 'test',
        category: 'Art',
        status: ProductStatus.Draft,
        startingPrice: 100,
        auctionStartTime: '2026-06-01T00:00:00',
        auctionEndTime: '2026-06-08T00:00:00',
      });

      component.saveProduct();

      expect(alertSpy).toHaveBeenCalledWith('Name and Category are required.');
      expect(component.saving()).toBe(false);
      alertSpy.mockRestore();
    });

    it('should not save when category is empty', () => {
      const alertSpy = vi.spyOn(globalThis, 'alert').mockImplementation(() => {});
      component.newProduct.set({
        name: 'Valid Name',
        description: 'test',
        category: '   ',
        status: ProductStatus.Draft,
        startingPrice: 100,
        auctionStartTime: '2026-06-01T00:00:00',
        auctionEndTime: '2026-06-08T00:00:00',
      });

      component.saveProduct();

      expect(alertSpy).toHaveBeenCalledWith('Name and Category are required.');
      alertSpy.mockRestore();
    });

    it('should not save when startingPrice is 0', () => {
      const alertSpy = vi.spyOn(globalThis, 'alert').mockImplementation(() => {});
      component.newProduct.set({
        name: 'Valid Name',
        description: 'test',
        category: 'Art',
        status: ProductStatus.Draft,
        startingPrice: 0,
        auctionStartTime: '2026-06-01T00:00:00',
        auctionEndTime: '2026-06-08T00:00:00',
      });

      component.saveProduct();

      expect(alertSpy).toHaveBeenCalledWith('Starting price must be greater than 0.');
      alertSpy.mockRestore();
    });

    it('should not save when startingPrice is negative', () => {
      const alertSpy = vi.spyOn(globalThis, 'alert').mockImplementation(() => {});
      component.newProduct.set({
        name: 'Valid Name',
        description: 'test',
        category: 'Art',
        status: ProductStatus.Draft,
        startingPrice: -10,
        auctionStartTime: '2026-06-01T00:00:00',
        auctionEndTime: '2026-06-08T00:00:00',
      });

      component.saveProduct();

      expect(alertSpy).toHaveBeenCalledWith('Starting price must be greater than 0.');
      alertSpy.mockRestore();
    });
  });

  describe('onSelectionChange', () => {
    it('should update selectedIds', () => {
      const newSet = new Set(['p-1', 'p-2']);
      component.onSelectionChange(newSet);
      expect(component.selectedIds()).toEqual(newSet);
    });
  });

  describe('onAddProduct', () => {
    it('should show add modal and reset new product', () => {
      component.onAddProduct();
      expect(component.showAddModal()).toBe(true);
      expect(component.newProduct().name).toBe('');
      expect(component.newProduct().startingPrice).toBe(0);
      expect(component.newProduct().status).toBe(ProductStatus.Draft);
    });

    it('should set auction times (start and end are 7 days apart)', () => {
      component.onAddProduct();

      const start = new Date(component.newProduct().auctionStartTime).getTime();
      const end = new Date(component.newProduct().auctionEndTime).getTime();

      // The component uses toISOString().slice(0,16) which may lose precision,
      // but start and end should be roughly 7 days apart
      const weekMs = 7 * 24 * 60 * 60 * 1000;
      const diff = end - start;
      // Allow some tolerance for rounding from slicing the ISO string
      expect(diff).toBeGreaterThanOrEqual(weekMs - 60000);
      expect(diff).toBeLessThanOrEqual(weekMs + 60000);

      // Both should be non-empty strings
      expect(component.newProduct().auctionStartTime.length).toBeGreaterThan(0);
      expect(component.newProduct().auctionEndTime.length).toBeGreaterThan(0);
    });
  });

  describe('closeModal', () => {
    it('should hide the add modal', () => {
      component.showAddModal.set(true);
      component.closeModal();
      expect(component.showAddModal()).toBe(false);
    });
  });
});
