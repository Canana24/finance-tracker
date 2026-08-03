export interface MonthlySummary
{
    month: number;
    year: number;
    totalIncome: number;
    totalExpense: number;
    savings: number;
    transactionCount: number;
}

export interface CategoryExpense
{
    categoryId: number;
    categoryName: string;
    total: number;
    percentage: number;
}

export interface MonthlyEvolution
{
    month: number;
    income: number;
    expense: number;
    savings: number;
}