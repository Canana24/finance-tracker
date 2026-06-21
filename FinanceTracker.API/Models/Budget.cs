using System;
using System.Collections.Generic;

namespace FinanceTracker.API.Models;

public partial class Budget
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public int CurrencyId { get; set; }

    public decimal Amount { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public bool IsActive { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
