import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Ensure the HttpOnly session cookie is sent with the request
  let cloned = req.clone({ withCredentials: true });

  return next(cloned);
};