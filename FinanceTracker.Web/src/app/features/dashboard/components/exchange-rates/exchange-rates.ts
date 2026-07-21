import { Component, inject, signal, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ExchangeRateService } from '../../../../core/services/exchange-rate.service';
import { ExchangeRate } from '../../../../core/models/exchange-rate.model';
import { sign } from 'chart.js/helpers';

@Component({
  selector: 'app-exchange-rates',
  imports: [DecimalPipe],
  templateUrl: './exchange-rates.html',
  styleUrl: './exchange-rates.scss',
})
export class ExchangeRates {
  private exchangeRateService = inject(ExchangeRateService);

  protected readonly rates = signal<ExchangeRate[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly hasError = signal(false);

  ngOnInit(): void {
    this.exchangeRateService.refreshFromUruguayApi().subscribe({
      next: (response) => {
        this.rates.set(response.rates);
        this.isLoading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
      },
    });
  }
}
