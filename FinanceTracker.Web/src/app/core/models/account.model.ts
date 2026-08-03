export interface Account 
{
    id: number;
    name: string;
    balance: number;
    currencyCode: string;
    currencySymbol: string;
    createdAt: string;
}

export interface CreateAccountRequest
{
    name: string;
    currencyId: number;
    initialBalance: number;
}

export interface UpdateAccountRequest
{
    name: string;
    currencyId: number;
}
