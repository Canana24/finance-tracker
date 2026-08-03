import { Component, ElementRef, effect, inject, input, viewChild } from '@angular/core';
import { Chart, BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend } from 'chart.js';
import { MonthlyEvolution } from '../../../../core/models/report.model';
import { ThemeService } from '../../../../core/services/theme.service';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend);

const MONTH_LABELS = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];

@Component({
  selector: 'app-monthly-evolution-chart',
  imports: [],
  templateUrl: './monthly-evolution-chart.html',
  styleUrl: './monthly-evolution-chart.scss',
})
export class MonthlyEvolutionChart {
  readonly evolution = input.required<MonthlyEvolution[]> ();

  private themeService = inject(ThemeService);
  private canvasReference = viewChild.required<ElementRef<HTMLCanvasElement>>('chartCanvas');

  private chartInstance: Chart<'bar'> | null = null;

  constructor() {
    effect(() => {
      const evolutionData= this.evolution();
      this.themeService.currentTheme();
      this.renderChart(evolutionData);
    });
  }

   private renderChart(evolution: MonthlyEvolution[]): void {
    this.chartInstance?.destroy();

    const canvasElement = this.canvasReference().nativeElement;
    const rootStyles = getComputedStyle(document.documentElement);
    const readToken = (tokenName: string) => rootStyles.getPropertyValue(tokenName).trim();

    this.chartInstance = new Chart(canvasElement, {
      type: 'bar',
      data: {
        labels: MONTH_LABELS,
        datasets: [
          {
            label: 'Ingresos',
            data: evolution.map(month => month.income),
            backgroundColor: readToken('--income'),
            borderRadius: 4,
          },
          {
            label: 'Gastos',
            data: evolution.map(month => month.expense),
            backgroundColor: readToken('--expense'),
            borderRadius: 4,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
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
        scales: {
          x: {
            grid: { display: false },
            ticks: {
              color: readToken('--ink-3'),
              font: { family: "'IBM Plex Sans', sans-serif" },
            },
          },
          y: {
            grid: { color: readToken('--border') },
            ticks: {
              color: readToken('--ink-3'),
              font: { family: "'IBM Plex Mono', monospace" },
            },
          },
        },
      },
    });
  }
}
