import { ProductTableComponent } from './product-table.component';
import { ProductStatus } from '../models/product.model';

describe('ProductTableComponent – pure logic', () => {
  let component: ProductTableComponent;

  beforeEach(() => {
    component = Object.create(ProductTableComponent.prototype);
    component.products = [];
    component.selectedIds = new Set();
  });

  describe('formatTimeRemaining', () => {
    it('should return "Ended" for null', () => {
      expect(component.formatTimeRemaining(null)).toBe('Ended');
    });

    it('should return "Ended" for undefined', () => {
      expect(component.formatTimeRemaining(undefined)).toBe('Ended');
    });

    it('should return "Ended" for 0', () => {
      expect(component.formatTimeRemaining(0)).toBe('Ended');
    });

    it('should return "Ended" for negative values', () => {
      expect(component.formatTimeRemaining(-100)).toBe('Ended');
    });

    it('should return "Ended" for NaN', () => {
      expect(component.formatTimeRemaining(NaN)).toBe('Ended');
    });

    it('should format seconds only', () => {
      expect(component.formatTimeRemaining(45)).toBe('45s');
    });

    it('should format minutes and seconds', () => {
      expect(component.formatTimeRemaining(930)).toBe('15m 30s');
    });

    it('should format hours and minutes', () => {
      expect(component.formatTimeRemaining(3660)).toBe('1h 1m');
    });

    it('should format days and hours', () => {
      // 2 days + 5 hours = 2*86400 + 5*3600 = 190800
      expect(component.formatTimeRemaining(190800)).toBe('2d 5h');
    });

    it('should format exactly 1 day', () => {
      expect(component.formatTimeRemaining(86400)).toBe('1d 0h');
    });

    it('should format exactly 1 hour', () => {
      expect(component.formatTimeRemaining(3600)).toBe('1h 0m');
    });

    it('should format exactly 1 minute', () => {
      expect(component.formatTimeRemaining(60)).toBe('1m 0s');
    });

    it('should format 1 second', () => {
      expect(component.formatTimeRemaining(1)).toBe('1s');
    });
  });

  describe('formatCurrency', () => {
    it('should return "—" for null', () => {
      expect(component.formatCurrency(null)).toBe('—');
    });

    it('should format zero', () => {
      expect(component.formatCurrency(0)).toBe('$0.00');
    });

    it('should format integer values', () => {
      expect(component.formatCurrency(100)).toBe('$100.00');
    });

    it('should format decimal values', () => {
      expect(component.formatCurrency(49.5)).toBe('$49.50');
    });

    it('should format large values', () => {
      expect(component.formatCurrency(999999)).toBe('$999999.00');
    });
  });

  describe('statusLabel', () => {
    it('should return "Draft" for Draft status', () => {
      expect(component.statusLabel(ProductStatus.Draft)).toBe('Draft');
    });

    it('should return "Active" for Active status', () => {
      expect(component.statusLabel(ProductStatus.Active)).toBe('Active');
    });

    it('should return "Sold" for Sold status', () => {
      expect(component.statusLabel(ProductStatus.Sold)).toBe('Sold');
    });

    it('should return "Expired" for Expired status', () => {
      expect(component.statusLabel(ProductStatus.Expired)).toBe('Expired');
    });

    it('should return "Cancelled" for Cancelled status', () => {
      expect(component.statusLabel(ProductStatus.Cancelled)).toBe('Cancelled');
    });

    it('should return "Unknown" for unrecognized status', () => {
      expect(component.statusLabel(99 as ProductStatus)).toBe('Unknown');
    });
  });

  describe('isAllSelected', () => {
    it('should return false when products list is empty', () => {
      component.products = [];
      component.selectedIds = new Set();
      expect(component.isAllSelected()).toBe(false);
    });

    it('should return true when all products are selected', () => {
      component.products = [
        { id: 'p-1' } as any,
        { id: 'p-2' } as any,
      ];
      component.selectedIds = new Set(['p-1', 'p-2']);
      expect(component.isAllSelected()).toBe(true);
    });

    it('should return false when not all products are selected', () => {
      component.products = [
        { id: 'p-1' } as any,
        { id: 'p-2' } as any,
      ];
      component.selectedIds = new Set(['p-1']);
      expect(component.isAllSelected()).toBe(false);
    });
  });

  describe('toggleAll', () => {
    let emittedSet: Set<string> | null;

    beforeEach(() => {
      emittedSet = null;
      component.selectionChange = { emit: (val: Set<string>) => { emittedSet = val; } } as any;
      component.products = [
        { id: 'p-1' } as any,
        { id: 'p-2' } as any,
        { id: 'p-3' } as any,
      ];
      component.selectedIds = new Set();
    });

    it('should select all products when checked is true', () => {
      component.toggleAll(true);
      expect(emittedSet).toBeDefined();
      expect(emittedSet!.has('p-1')).toBe(true);
      expect(emittedSet!.has('p-2')).toBe(true);
      expect(emittedSet!.has('p-3')).toBe(true);
    });

    it('should deselect all products when checked is false', () => {
      component.selectedIds = new Set(['p-1', 'p-2', 'p-3']);
      component.toggleAll(false);
      expect(emittedSet).toBeDefined();
      expect(emittedSet!.has('p-1')).toBe(false);
      expect(emittedSet!.has('p-2')).toBe(false);
      expect(emittedSet!.has('p-3')).toBe(false);
    });
  });

  describe('toggleOne', () => {
    let emittedSet: Set<string> | null;

    beforeEach(() => {
      emittedSet = null;
      component.selectionChange = { emit: (val: Set<string>) => { emittedSet = val; } } as any;
      component.selectedIds = new Set(['p-1']);
    });

    it('should add an id when checked is true', () => {
      component.toggleOne('p-2', true);
      expect(emittedSet!.has('p-2')).toBe(true);
      expect(emittedSet!.has('p-1')).toBe(true);
    });

    it('should remove an id when checked is false', () => {
      component.toggleOne('p-1', false);
      expect(emittedSet!.has('p-1')).toBe(false);
    });
  });
});
