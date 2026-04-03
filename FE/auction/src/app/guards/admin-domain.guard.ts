import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';

export const adminDomainGuard: CanActivateFn = () => {
  const router = inject(Router);
  const { hostname, port } = window.location;

  if (hostname === 'admin.auction.local' && port === '9443') {
    return true;
  }

  router.navigate(['/not-found']);
  return false;
};