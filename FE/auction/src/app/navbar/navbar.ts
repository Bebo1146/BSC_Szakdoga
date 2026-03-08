import { Component, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../services/theme.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {
  private readonly router = inject(Router);
  readonly themeService = inject(ThemeService);

  storeName: string = 'Store';

  constructor() {
    const storedName = localStorage.getItem('storeName');
    if (storedName) {
      this.storeName = storedName;
    }
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }
}
