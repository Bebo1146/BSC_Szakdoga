import { Product, ProductStatus, TransactionStatus, NewProduct } from './product.model';

describe('Product Model', () => {
  // ──────────────────────────────────────────────
  // ProductStatus enum
  // ──────────────────────────────────────────────
  describe('ProductStatus enum', () => {
    it('should have correct numeric values', () => {
      expect(ProductStatus.Draft).toBe(0);
      expect(ProductStatus.Active).toBe(1);
      expect(ProductStatus.Sold).toBe(2);
      expect(ProductStatus.Expired).toBe(3);
      expect(ProductStatus.Cancelled).toBe(4);
    });

    it('should have 5 members', () => {
      // Numeric enums in TS have reverse mappings, so divide by 2
      const members = Object.keys(ProductStatus).filter((k) => isNaN(Number(k)));
      expect(members).toHaveLength(5);
    });

    it('should support reverse mapping', () => {
      expect(ProductStatus[0]).toBe('Draft');
      expect(ProductStatus[1]).toBe('Active');
      expect(ProductStatus[2]).toBe('Sold');
      expect(ProductStatus[3]).toBe('Expired');
      expect(ProductStatus[4]).toBe('Cancelled');
    });
  });

  // ──────────────────────────────────────────────
  // TransactionStatus enum
  // ──────────────────────────────────────────────
  describe('TransactionStatus enum', () => {
    it('should have correct numeric values', () => {
      expect(TransactionStatus.Pending).toBe(0);
      expect(TransactionStatus.PaymentReceived).toBe(1);
      expect(TransactionStatus.Shipped).toBe(2);
      expect(TransactionStatus.Delivered).toBe(3);
      expect(TransactionStatus.Completed).toBe(4);
      expect(TransactionStatus.Disputed).toBe(5);
      expect(TransactionStatus.Cancelled).toBe(6);
    });

    it('should have 7 members', () => {
      const members = Object.keys(TransactionStatus).filter((k) => isNaN(Number(k)));
      expect(members).toHaveLength(7);
    });
  });

  // ──────────────────────────────────────────────
  // Product interface type-checking (compile-time + runtime shape)
  // ──────────────────────────────────────────────
  describe('Product interface', () => {
    it('should accept a valid product object', () => {
      const product: Product = {
        id: 'p-123',
        name: 'Vintage Watch',
        description: 'A beautiful vintage watch',
        category: 'Jewelry',
        status: ProductStatus.Active,
        startingPrice: 500,
        currentBid: 750,
        auctionStartTime: '2026-01-01T00:00:00Z',
        auctionEndTime: '2026-01-08T00:00:00Z',
        totalBids: 5,
        highestBidderId: 'user-42',
        highestBidderUsername: 'topbidder',
        sellerId: 'user-1',
        sellerUsername: 'seller1',
        createdAt: '2025-12-30T00:00:00Z',
        updatedAt: '2026-01-02T12:00:00Z',
        isCompleted: false,
        transactionStatus: null,
        feedback: null,
        isActive: true,
        hasEnded: false,
        timeRemaining: '5.12:30:00',
        timeRemainingSeconds: 475800,
      };

      expect(product.id).toBe('p-123');
      expect(product.status).toBe(ProductStatus.Active);
      expect(product.isActive).toBe(true);
      expect(product.hasEnded).toBe(false);
    });

    it('should accept nullable fields as null', () => {
      const product: Product = {
        id: 'p-456',
        name: 'Empty Auction',
        description: '',
        category: 'Other',
        status: ProductStatus.Draft,
        startingPrice: 10,
        currentBid: null,
        auctionStartTime: '2026-06-01T00:00:00Z',
        auctionEndTime: '2026-06-08T00:00:00Z',
        totalBids: 0,
        highestBidderId: null,
        highestBidderUsername: null,
        sellerId: 'user-1',
        sellerUsername: 'seller1',
        createdAt: '2026-05-30T00:00:00Z',
        updatedAt: null,
        isCompleted: false,
        transactionStatus: null,
        feedback: null,
        isActive: false,
        hasEnded: false,
        timeRemaining: null,
      };

      expect(product.currentBid).toBeNull();
      expect(product.highestBidderId).toBeNull();
      expect(product.feedback).toBeNull();
      expect(product.timeRemainingSeconds).toBeUndefined();
    });

    it('should accept product with feedback', () => {
      const product: Product = {
        id: 'p-789',
        name: 'Completed Item',
        description: 'Sold item with feedback',
        category: 'Art',
        status: ProductStatus.Sold,
        startingPrice: 100,
        currentBid: 350,
        auctionStartTime: '2025-01-01T00:00:00Z',
        auctionEndTime: '2025-01-08T00:00:00Z',
        totalBids: 10,
        highestBidderId: 'user-99',
        highestBidderUsername: 'winner',
        sellerId: 'user-1',
        sellerUsername: 'seller1',
        createdAt: '2024-12-30T00:00:00Z',
        updatedAt: '2025-01-08T00:00:01Z',
        isCompleted: true,
        transactionStatus: TransactionStatus.Completed,
        feedback: {
          reviewerId: 'user-99',
          reviewerUsername: 'winner',
          rating: 5,
          comment: 'Great item!',
          createdAt: '2025-01-10T00:00:00Z',
        },
        isActive: false,
        hasEnded: true,
        timeRemaining: null,
      };

      expect(product.feedback).toBeDefined();
      expect(product.feedback!.rating).toBe(5);
      expect(product.transactionStatus).toBe(TransactionStatus.Completed);
      expect(product.isCompleted).toBe(true);
    });
  });

  // ──────────────────────────────────────────────
  // NewProduct interface
  // ──────────────────────────────────────────────
  describe('NewProduct interface', () => {
    it('should accept a valid new product', () => {
      const newProduct: NewProduct = {
        name: 'New Auction Item',
        description: 'Brand new item',
        category: 'Electronics',
        status: ProductStatus.Draft,
        startingPrice: 200,
        auctionStartTime: '2026-06-01T00:00:00Z',
        auctionEndTime: '2026-06-08T00:00:00Z',
      };

      expect(newProduct.name).toBe('New Auction Item');
      expect(newProduct.status).toBe(ProductStatus.Draft);
    });
  });
});
