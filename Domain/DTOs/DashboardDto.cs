namespace MisFinanzas.Domain.DTOs
{
    public class DashboardDto
    {

        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance { get; set; }
        public int TotalGoals { get; set; }
        public int CompletedGoals { get; set; }

        public List<CategoryExpenseDto> CategoryExpenses { get; set; } = new();
        public List<TransactionDto> RecentTransactions { get; set; } = new();
        public List<FinancialGoalDto> ActiveGoals { get; set; } = new();
    }

    public class CategoryExpenseDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        
        
        // Actualizado para usar RD$
        public string FormattedAmount => Amount.ToString("C2");  // RD$1,234.56
    }
}
