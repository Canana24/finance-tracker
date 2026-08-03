import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Account, CreateAccountRequest, UpdateAccountRequest } from '../models/account.model';

@Injectable({providedIn: 'root'})
export class AccountService {
    private http = inject(HttpClient);
    private readonly baseURL = `${environment.apiUrl}/Account`;

    getAllAccounts(){
        return this.http.get<Account[]>(this.baseURL);
    }

    createAccount(account: CreateAccountRequest){
        return this.http.post<Account>(this.baseURL, account);
    }

    updateAccount(accountId: number, account: UpdateAccountRequest) {
        return this.http.put<Account>(`${this.baseURL}/${accountId}`, account)
    }

    deleteAccount(accountId: number){
        return this.http.delete<void>(`${this.baseURL}/${accountId}`);
    }
}