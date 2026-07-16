import { Component, ElementRef, effect, inject, input, viewChild } from '@angular/core';
import { Chart, DoughnutController, ArcElement, Tooltip, Legend } from 'chart.js';
import { CategoryExpense } from '../../../../core/models/report.model';
import { ThemeService } from '../../../../core/services/theme.service';

Chart.register(DoughnutController, ArcElement, Tooltip, Legend);

@Component({
  selector: 'app-category-expenses-chart',
  imports: [],
  templateUrl: './category-expenses-chart.html',
  styleUrl: './category-expenses-chart.scss',
})

export class CategoryExpensesChart {
  readonly expenses =  input.required<CategoryExpense []>();

  private themeService = inject(ThemeService);
  private canvasReference = viewChild.required<ElementRef<HTMLCanvasElement>>('chartCanvas');

  private chartInstance: Chart<'doughnut'> | null = null;

  constructor() {
    effect(() => {
      const expenseData = this.expenses();
      this.themeService.currentTheme(); // redibuja al cambiar el tema
      this.renderChart(expenseData);
    });
  }

  private renderChart (expenses: CategoryExpense[]): void{
    this.chartInstance?.destroy();

    const canvasElement= this.canvasReference().nativeElement;
    const rootStyles = getComputedStyle(document.documentElement);

    const readToken = (tokenName: string) =>
      rootStyles.getPropertyValue(tokenName).trim();

    const colorPalette = [
      readToken('--chart-1'), readToken('--chart-2'), readToken('--chart-3'),
      readToken('--chart-4'), readToken('--chart-5'), readToken('--chart-6'),
    ];

    this.chartInstance = new Chart(canvasElement, {
      type: 'doughnut',
      
      data: {
        labels:expenses.map(expense => expense.categoryName),
        datasets: [{
          data: expenses.map(expense => expense.total),
          backgroundColor: expenses.map((_,index) => colorPalette[index % colorPalette.length]),
          borderColor: readToken('--surface'),
          borderWidth: 2,
        }],
      },

      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '65%',
        plugins: {
          legend: {
            position: 'bottom',
            labels: {
              color: readToken('--ink-2'),
              font: { family: "'IBM Plex Sans', sans-serif", size: 13 },
              padding: 16,
              usePointStyle: true,
            },
          },
          tooltip: {
            backgroundColor: readToken('--surface-2'),
            titleColor: readToken('--ink'),
            bodyColor: readToken('--ink-2'),
            borderColor: readToken('--border'),
            borderWidth: 1,
            bodyFont: { family: "'IBM Plex Mono', monospace" },
          },
        },
      }
    })
  }
}
