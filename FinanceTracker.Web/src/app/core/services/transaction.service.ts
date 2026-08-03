import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Transaction, CreateTransactionRequest, UpdateTransactionRequest } from '../models/transaction.model';

@Injectable({providedIn: 'root'})
export class TransactionService {
    private http = inject(HttpClient);
    private readonly baseUrl = `${environment.apiUrl}/Transaction`;

    getAllTransactions() {
        return this.http.get<Transaction[]>(this.baseUrl);
    }

    createTransaction(transaction: CreateTransactionRequest) {
        return this.http.post<Transaction>(this.baseUrl, transaction);
    }

    updateTransaction(transactionId: number, transaction: UpdateTransactionRequest){
        return this.http.put<Transaction>(`${this.baseUrl}/${transactionId}`, transaction);
    }

    deleteTransaction(transactionId: number) {
        return this.http.delete<Transaction>(`${this.baseUrl}/${transactionId}`);
    }
}