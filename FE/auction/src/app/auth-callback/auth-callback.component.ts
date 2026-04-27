import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-auth-callback',
  template: `<p>Signing in...</p>`,
  standalone: true,
})
export class AuthCallbackComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    this.authService.handleFrontendCallback()
    .subscribe((ok) => {
      if (ok) {
        this.router.navigate(['/']);
      } else {
        this.router.navigate(['/login'], { replaceUrl: true });
      }
    });
  }
}