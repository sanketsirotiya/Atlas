import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { Sidebar } from '../../../shared/components/sidebar/sidebar';

@Component({
  selector: 'app-suitability-check',
  standalone: true,
  imports: [CommonModule, FormsModule, Sidebar],
  templateUrl: './suitability-check.html',
  styleUrl: './suitability-check.css',
})
export class SuitabilityCheck implements OnInit {
  clientId = '';
  clientName = '';
  assetClass = 'USEquity';
  transactionType = 'Buy';
  amount = 10000;

  result: any = null;
  loading = false;
  error = '';

  assetClasses = [
    'CanadianEquity',
    'USEquity',
    'InternationalEquity',
    'CanadianBonds',
    'GlobalBonds',
  ];
  transactionTypes = ['Buy', 'Sell'];

  constructor(
    private route: ActivatedRoute,
    private apiService: ApiService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.clientId = this.route.snapshot.paramMap.get('id')!;
    this.apiService.getClient(this.clientId).subscribe({
      next: (data) => {
        this.clientName = data.fullName;
        this.cdr.detectChanges();
      },
    });
  }

  resetResult() {
    this.result = null;
    this.error = '';
  }

  check() {
    this.loading = true;
    this.result = null;
    this.error = '';

    this.apiService
      .checkSuitability({
        clientId: this.clientId,
        assetClass: this.assetClass,
        transactionType: this.transactionType,
        amount: this.amount,
      })
      .subscribe({
        next: (data) => {
          this.result = data;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.error = 'Failed to run suitability check.';
          this.loading = false;
          this.cdr.detectChanges();
        },
      });
  }

  goBack() {
    this.router.navigate(['/clients', this.clientId]);
  }
}
