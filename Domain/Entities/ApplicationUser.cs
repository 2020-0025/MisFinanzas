using Microsoft.AspNetCore.Identity;

namespace MisFinanzas.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Información personal
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        // Estado de la cuenta
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<ExpenseIncome> ExpensesIncomes { get; set; } = new List<ExpenseIncome>();
        public virtual ICollection<FinancialGoal> FinancialGoals { get; set; } = new List<FinancialGoal>();
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}