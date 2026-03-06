import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sidebar.html',
})
export class Sidebar {
  @Input() clientId?: string;

  constructor(
    public router: Router,
    private authService: AuthService,
  ) {}

  isActive(segment: string): boolean {
    const url = this.router.url;
    if (segment === 'clients' && (url === '/clients' || url.endsWith('/clients'))) return true;
    if (segment !== 'clients' && this.clientId) {
      return url.includes(`/${segment}`);
    }
    return false;
  }

  navigate(path: string) {
    this.router.navigate([path]);
  }

  logout() {
    this.authService.logout();
  }
}
