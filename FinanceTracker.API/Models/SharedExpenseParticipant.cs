using System;
using System.Collections.Generic;

namespace FinanceTracker.API.Models;

public partial class SharedExpenseParticipant
{
    public int Id { get; set; }

    public int SharedExpenseId { get; set; }

    public string Name { get; set; } = null!;

    public decimal AmountOwed { get; set; }

    public bool IsPaid { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public bool IsActive { get; set; }

    public virtual SharedExpense SharedExpense { get; set; } = null!;
}
