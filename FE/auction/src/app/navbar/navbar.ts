import { Component, inject } from '@angular/core';
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

  storeName: string | null = null;

  constructor() {
    this.authService.preferredName$
    .subscribe(name => {
      console.log('Preferred name updated:', name);
      this.storeName = name;
    });
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }
}
