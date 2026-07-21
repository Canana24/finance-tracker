import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ExchangeRate } from '../models/exchange-rate.model';

@Injectable ({providedIn: 'root'})
export class ExchangeRateService {
    private http = inject(HttpClient);

    refreshFromUruguayApi(){
        return this.http.post<{message: string; count: number; rates: ExchangeRate[]}>
        (
            `${environment.apiUrl}/ExchangeRate/refresh-uruguay`, {}
        );
    }
}