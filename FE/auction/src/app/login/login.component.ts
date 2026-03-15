import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  error = '';

  ngOnInit(): void {
    // Start the OAuth flow automatically when the component is loaded
    this.onSubmit();
  }

  onSubmit(): void {
    this.error = '';
    // Start OAuth flow (backend will build Keycloak URL and return it)
    this.authService.login()
    .subscribe({
      next: () => {
        // navigation won't happen because browser will be redirected to Keycloak
        // keep this here as a fallback
        this.router.navigate(['/']);
      },
      error: () => {
        this.error = 'Login failed';
      },
    });
  }
}