import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  let cloned = req.clone({ withCredentials: true });

  return next(cloned);
};