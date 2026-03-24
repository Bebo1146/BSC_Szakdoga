export enum ProductStatus {
  Draft = 0,
  Active = 1,
  Sold = 2,
  Expired = 3,
  Cancelled = 4,
}

export enum TransactionStatus {
  Pending = 0,
  PaymentReceived = 1,
  Shipped = 2,
  Delivered = 3,
  Completed = 4,
  Disputed = 5,
  Cancelled = 6,
}

export interface FeedbackDto {
  reviewerId: string;
  reviewerUsername: string;
  rating: number;
  comment: string | null;
  createdAt: string;
}

export interface Product {
  id: string;
  name: string;
  description: string;
  category: string;
  status: ProductStatus;
  imageUrl: string;
  startingPrice: number;
  currentBid: number | null;
  reservePrice: number | null;
  auctionStartTime: string;
  auctionEndTime: string;
  totalBids: number;
  highestBidderId: string | null;
  highestBidderUsername: string | null;
  sellerId: string;
  sellerUsername: string;
  createdAt: string;
  updatedAt: string | null;
  isCompleted: boolean;
  transactionStatus: TransactionStatus | null;
  feedback: FeedbackDto | null;
  isActive: boolean;
  hasEnded: boolean;
  timeRemaining: string | null;
}

export interface NewProduct {
  name: string;
  description: string;
  category: string;
  status: ProductStatus;
  imageUrl: string;
  startingPrice: number;
  reservePrice: number | null;
  auctionStartTime: string;
  auctionEndTime: string;
}