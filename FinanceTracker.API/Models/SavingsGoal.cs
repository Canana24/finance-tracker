using System;
using System.Collections.Generic;

namespace FinanceTracker.API.Models;

public partial class SavingsGoal
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CurrencyId { get; set; }

    public string Name { get; set; } = null!;

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public DateTime? Deadline { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public bool IsActive { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
