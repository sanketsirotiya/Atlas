import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { Sidebar } from '../../../shared/components/sidebar/sidebar';

@Component({
  selector: 'app-client-detail',
  standalone: true,
  imports: [CommonModule, Sidebar],
  templateUrl: './client-detail.html',
  styleUrl: './client-detail.css',
})
export class ClientDetail implements OnInit {
  client: any = null;
  loading = true;
  error = '';
  clientId = '';

  constructor(
    private route: ActivatedRoute,
    private apiService: ApiService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.clientId = this.route.snapshot.paramMap.get('id')!;
    this.apiService.getClient(this.clientId).subscribe({
      next: (data) => {
        this.client = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Failed to load client.';
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  goToPortfolio() {
    this.router.navigate(['/clients', this.clientId, 'portfolio']);
  }
  goToDrift() {
    this.router.navigate(['/clients', this.clientId, 'drift']);
  }
  goToRebalance() {
    this.router.navigate(['/clients', this.clientId, 'rebalance']);
  }
  goToSuitability() {
    this.router.navigate(['/clients', this.clientId, 'suitability']);
  }
  goBack() {
    this.router.navigate(['/clients']);
  }
  logout() {
    this.authService.logout();
  }
}
