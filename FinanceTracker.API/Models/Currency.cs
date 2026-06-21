using System;
using System.Collections.Generic;

namespace FinanceTracker.API.Models;

public partial class Currency
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Symbol { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<ExchangeRate> ExchangeRates { get; set; } = new List<ExchangeRate>();

    public virtual ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();

    public virtual ICollection<SharedExpense> SharedExpenses { get; set; } = new List<SharedExpense>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
