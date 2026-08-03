import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Currency } from '../models/currency.model';

@Injectable({providedIn: 'root'})
export class CurrencyService {
    private http = inject(HttpClient);

    getAllCurrencies() {
        return this.http.get<Currency[]>(`${environment.apiUrl}/Currency`);
    }
}