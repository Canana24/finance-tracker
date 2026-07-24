import { Component, inject, signal, OnInit } from '@angular/core';
import { ReportService } from '../../core/services/report.service';
import { MonthlySummary, CategoryExpense, MonthlyEvolution } from '../../core/models/report.model';
import { CategoryExpensesChart } from './components/category-expenses-chart/category-expenses-chart';
import { MonthlyEvolutionChart } from './components/monthly-evolution-chart/monthly-evolution-chart';
import { ExchangeRates } from './components/exchange-rates/exchange-rates';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  imports: [CategoryExpensesChart, MonthlyEvolutionChart, ExchangeRates, DecimalPipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private reportService = inject(ReportService);

  protected readonly summary = signal<MonthlySummary | null> (null);
  protected readonly categoryExpenses = signal<CategoryExpense []> ([]);
  protected readonly monthlyEvolution = signal<MonthlyEvolution[]>([]);
  protected readonly isLoading = signal(true);

  ngOnInit(): void {
    const currentDate = new Date();
    const currentMonth = currentDate.getMonth() + 1;
    const currentYear = currentDate.getFullYear();

    this.reportService.getMonthlySummary(currentMonth, currentYear).subscribe({
      next: (data) => this.summary.set(data),
      error: () => this.isLoading.set(false),
    });

    this.reportService.getExpensesByCategory(currentMonth, currentYear).subscribe({
      next: (data) => {
        this.categoryExpenses.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });

    this.reportService.getMonthlyEvolution(currentYear).subscribe({
      next:(data) => this.monthlyEvolution.set(data),
      error: () => this.isLoading.set(false),
    });
  }
}
