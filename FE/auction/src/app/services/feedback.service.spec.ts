import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { FeedbackService, FeedbackDto } from './feedback.service';

describe('FeedbackService', () => {
  let service: FeedbackService;
  let httpTesting: HttpTestingController;

  let store: Record<string, string> = {};
  const localStorageMock = {
    getItem: (key: string) => store[key] ?? null,
    setItem: (key: string, value: string) => { store[key] = value; },
    removeItem: (key: string) => { delete store[key]; },
    clear: () => { store = {}; },
  };

  beforeEach(() => {
    store = {};
    Object.defineProperty(globalThis, 'localStorage', { value: localStorageMock, writable: true });

    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(FeedbackService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('submitFeedback', () => {
    it('should POST feedback to correct URL', () => {
      const feedback: FeedbackDto = { rating: 5, comment: 'Excellent!' };

      service.submitFeedback('p-1', feedback).subscribe();

      const req = httpTesting.expectOne('/api/BffProxy/products/p-1/feedback');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(feedback);
      expect(req.request.withCredentials).toBe(true);
      req.flush({ success: true });
    });

    it('should mark feedback as given in localStorage after success', () => {
      const feedback: FeedbackDto = { rating: 4 };

      service.submitFeedback('p-1', feedback).subscribe();

      const req = httpTesting.expectOne('/api/BffProxy/products/p-1/feedback');
      req.flush({ success: true });

      expect(service.hasFeedbackBeenGiven('p-1')).toBe(true);
    });

    it('should handle feedback with null comment', () => {
      const feedback: FeedbackDto = { rating: 3, comment: null };

      service.submitFeedback('p-2', feedback).subscribe();

      const req = httpTesting.expectOne('/api/BffProxy/products/p-2/feedback');
      expect(req.request.body.comment).toBeNull();
      req.flush({ success: true });
    });
  });

  describe('hasFeedbackBeenGiven', () => {
    it('should return false when no feedback submitted', () => {
      expect(service.hasFeedbackBeenGiven('p-1')).toBe(false);
    });

    it('should return true after feedback submitted', () => {
      store['feedback.submitted'] = JSON.stringify(['p-1']);
      expect(service.hasFeedbackBeenGiven('p-1')).toBe(true);
    });

    it('should return false for different product', () => {
      store['feedback.submitted'] = JSON.stringify(['p-1']);
      expect(service.hasFeedbackBeenGiven('p-2')).toBe(false);
    });

    it('should handle corrupted localStorage gracefully', () => {
      store['feedback.submitted'] = 'not-json';
      expect(service.hasFeedbackBeenGiven('p-1')).toBe(false);
    });
  });

  describe('getMyFeedbacks', () => {
    it('should GET user feedbacks', () => {
      service.getMyFeedbacks().subscribe((feedbacks) => {
        expect(feedbacks).toHaveLength(1);
      });

      const req = httpTesting.expectOne('/api/BffProxy/products/my-received-feedback');
      expect(req.request.method).toBe('GET');
      expect(req.request.withCredentials).toBe(true);
      req.flush([{ productName: 'Item 1', rating: 5, comment: 'Great!' }]);
    });
  });
});
