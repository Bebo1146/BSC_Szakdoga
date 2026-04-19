import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../services/theme.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {
  readonly themeService = inject(ThemeService);
  readonly authService = inject(AuthService);

  username: string | null = null;

  readonly isAdminDomain = window.location.hostname === 'admin.auction.local';

  constructor() {
    this.authService.preferredName$
    .subscribe(name => {
      this.username = name
        ? name.charAt(0).toUpperCase() + name.slice(1)
        : null;
    });
    console.log(window.location.hostname, window.location.port);
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  logout(): void {
    window.location.href = '/api/auth/logout';
  }
}
