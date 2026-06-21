using System;
using System.Collections.Generic;

namespace FinanceTracker.API.Models;

public partial class ExchangeRate
{
    public int Id { get; set; }

    public int CurrencyId { get; set; }

    public decimal Rate { get; set; }

    public DateTime Date { get; set; }

    public virtual Currency Currency { get; set; } = null!;
}
