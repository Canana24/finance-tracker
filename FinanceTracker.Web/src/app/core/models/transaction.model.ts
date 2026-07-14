import {CategoryType} from './category.model';

export interface Transaction 
{
    id: number;
    accountId: number;
    accountName: string;
    categoryName: string;
    amount: number;
    type: CategoryType;
    description: string | null;
    date: string;
}

export interface CreateTransactionRequest
{
    accountId: number;
    categoryId: number;
    amount: number;
    type: CategoryType;
    description: string | null;
    date: string;
}

export interface UpdateTransactionRequest
{
    categoryId: number;
    amount: number;
    description: string | null;
    date: string;
}