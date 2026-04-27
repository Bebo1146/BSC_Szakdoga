import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
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

  ngOnInit(): void {
    this.authService.login().subscribe({
      next: (response: any) => {
        const redirectUrl = response?.redirectUrl ?? response?.url ?? response;
        if (typeof redirectUrl === 'string') {
          window.location.href = redirectUrl;
        }
      },
      error: () => {
        // Only show the page if login initiation fails
      },
    });
  }
}