import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { MonthlySummary, CategoryExpense, MonthlyEvolution } from '../models/report.model';

@Injectable ({providedIn: 'root'})
export class ReportService {
    private http = inject(HttpClient);

    getMonthlySummary (month: number, year: number){
        const parameters = new HttpParams ().set('month',month).set('year',year);

        return this.http.get<MonthlySummary>(
            `${environment.apiUrl}/Report/monthly-summary`,
        { params: parameters }
        );
    }

    getExpensesByCategory(month: number, year: number){
        const parameters = new HttpParams ().set('month',month).set('year',year);

        return this.http.get<CategoryExpense[]>(
            `${environment.apiUrl}/Report/expenses-by-category`,
        { params: parameters }
        );
    }

    getMonthlyEvolution(year: number){
        const parameters = new HttpParams ().set('year',year);

        return this.http.get<MonthlyEvolution[]>(
        `${environment.apiUrl}/Report/monthly-evolution`,
        { params: parameters }
        );
    }
}