using System;
using System.Collections.Generic;

namespace FinanceTracker.API.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int CategoryId { get; set; }

    public int CurrencyId { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime Date { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public bool IsActive { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
