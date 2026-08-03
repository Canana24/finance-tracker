using System;
using System.Collections.Generic;

namespace FinanceTracker.API.Models;

public partial class User
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();

    public virtual ICollection<SharedExpense> SharedExpenses { get; set; } = new List<SharedExpense>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
