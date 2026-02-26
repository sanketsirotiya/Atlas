import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-rebalance-preview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './rebalance-preview.html',
  styleUrl: './rebalance-preview.css',
})
export class RebalancePreview implements OnInit {
  preview: any = null;
  loading = true;
  executing = false;
  executed = false;
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
    this.apiService.previewRebalance(this.clientId).subscribe({
      next: (data) => {
        this.preview = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Failed to load rebalance preview.';
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  execute() {
    this.executing = true;
    this.apiService.executeRebalance(this.clientId).subscribe({
      next: () => {
        this.executed = true;
        this.executing = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Failed to execute rebalance.';
        this.executing = false;
        this.cdr.detectChanges();
      },
    });
  }

  goBack() {
    this.router.navigate(['/clients', this.clientId]);
  }
}
