import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-drift-report',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './drift-report.html',
  styleUrl: './drift-report.css',
})
export class DriftReport implements OnInit {
  drift: any = null;
  loading = true;
  error = '';
  clientId = '';

  constructor(
    private route: ActivatedRoute,
    private apiService: ApiService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.clientId = this.route.snapshot.paramMap.get('id')!;
    this.apiService.getDrift(this.clientId).subscribe({
      next: (data) => {
        this.drift = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Failed to load drift report.';
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  goBack() {
    this.router.navigate(['/clients', this.clientId]);
  }
  goToRebalance() {
    this.router.navigate(['/clients', this.clientId, 'rebalance']);
  }

  getDriftClass(drift: number): string {
    const abs = Math.abs(drift);
    if (abs > 10) return 'text-red-600 font-semibold';
    if (abs > 5) return 'text-yellow-600 font-semibold';
    return 'text-green-600';
  }

  getBarWidth(actual: number): string {
    return Math.min(actual * 2, 100) + '%';
  }
}
