using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para nuestras entidades
        public DbSet<Category> Categories { get; set; }
        public DbSet<ExpenseIncome> ExpensesIncomes { get; set; }
        public DbSet<FinancialGoal> FinancialGoals { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Loan> Loans { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ====== RENOMBRAR TABLAS DE IDENTITY ======
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

            // ====== CONFIGURAR APPLICATIONUSER ======
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FirstName)
                    .HasMaxLength(100);

                entity.Property(u => u.LastName)
                    .HasMaxLength(100);

                entity.Property(u => u.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                // Índice para búsquedas por estado
                entity.HasIndex(u => u.IsActive);

                // Ignorar campos de Identity que no usamos
                entity.Ignore(u => u.PhoneNumber);
                entity.Ignore(u => u.PhoneNumberConfirmed);
                entity.Ignore(u => u.TwoFactorEnabled);
                entity.Ignore(u => u.LockoutEnd);
                entity.Ignore(u => u.LockoutEnabled);
                entity.Ignore(u => u.AccessFailedCount);
            });

            // ====== CONFIGURACIÓN DE CATEGORIES ======
            builder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.CategoryId);

                entity.Property(c => c.Title)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(c => c.Icon)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasDefaultValue("📁");

                entity.Property(c => c.Type)
                    .IsRequired();

                // Campos para gastos fijos/recordatorios
                entity.Property(c => c.IsFixedExpense)
                    .HasDefaultValue(false);

                entity.Property(c => c.DayOfMonth)
                    .IsRequired(false);

                entity.Property(c => c.EstimatedAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired(false);

                // Relación con ApplicationUser
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Categories)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(c => c.UserId);
                entity.HasIndex(c => new { c.UserId, c.Title });
                entity.HasIndex(c => new { c.UserId, c.Type });
                entity.HasIndex(c => new { c.UserId, c.IsFixedExpense });

                // Validaciones
                entity.ToTable(t => t.HasCheckConstraint(
                   "CK_Category_DayOfMonth", "\"DayOfMonth\" IS NULL OR (\"DayOfMonth\" >= 1 AND \"DayOfMonth\" <= 31)"));
            });

            // ====== CONFIGURACIÓN DE EXPENSESINCOME ======
            builder.Entity<ExpenseIncome>(entity =>
            {
                entity.HasKey(ei => ei.Id);

                entity.Property(ei => ei.Amount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(ei => ei.Description)
                    .HasMaxLength(500);

                entity.Property(ei => ei.Date)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(ei => ei.Type)
                    .IsRequired();

                entity.Property(ei => ei.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relación con ApplicationUser
                entity.HasOne(ei => ei.User)
                    .WithMany(u => u.ExpensesIncomes)
                    .HasForeignKey(ei => ei.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Category
                entity.HasOne(ei => ei.Category)
                    .WithMany(c => c.ExpensesIncomes)
                    .HasForeignKey(ei => ei.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices
                entity.HasIndex(ei => ei.UserId);
                entity.HasIndex(ei => ei.Date);
                entity.HasIndex(ei => new { ei.UserId, ei.Date });
                entity.HasIndex(ei => new { ei.UserId, ei.Type });

                // Validación: Amount debe ser positivo
                entity.ToTable(t => t.HasCheckConstraint(
                  "CK_ExpenseIncome_Amount", "\"Amount\" > 0"));

                // Ignorar propiedades computadas
                entity.Ignore(ei => ei.FormattedAmount);
                entity.Ignore(ei => ei.TypeDisplay);
            });

            // ====== CONFIGURACIÓN DE FINANCIAL GOALS ======
            builder.Entity<FinancialGoal>(entity =>
            {
                entity.HasKey(g => g.GoalId);

                entity.Property(g => g.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(g => g.Description)
                    .HasMaxLength(500);

                entity.Property(g => g.Icon)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasDefaultValue("🎯");

                entity.Property(g => g.TargetAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(g => g.CurrentAmount)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.Property(g => g.StartDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(g => g.TargetDate)
                    .IsRequired();

                entity.Property(g => g.Status)
                    .IsRequired();

                entity.Property(g => g.CompletedAt)
                    .IsRequired(false);

                // Relación con ApplicationUser
                entity.HasOne(g => g.User)
                    .WithMany(u => u.FinancialGoals)
                    .HasForeignKey(g => g.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(g => g.UserId);
                entity.HasIndex(g => g.TargetDate);
                entity.HasIndex(g => new { g.UserId, g.Status });

                // Validaciones
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Goal_TargetAmount", "\"TargetAmount\" > 0"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Goal_CurrentAmount", "\"CurrentAmount\" >= 0"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Goal_Dates", "\"TargetDate\" >= \"StartDate\""));

                // Ignorar propiedades computadas
                entity.Ignore(g => g.ProgressPercentage);
                entity.Ignore(g => g.RemainingAmount);
                entity.Ignore(g => g.DaysRemaining);
                entity.Ignore(g => g.IsCompleted);
                entity.Ignore(g => g.IsOverdue);
            });

            // ====== CONFIGURACIÓN DE BUDGETS ======
            builder.Entity<Budget>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(b => b.AssignedAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(b => b.SpentAmount)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.Property(b => b.IsActive)
                    .HasDefaultValue(true);

                entity.Property(b => b.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relación con ApplicationUser
                entity.HasOne(b => b.User)
                    .WithMany(u => u.Budgets)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Category
                entity.HasOne(b => b.Category)
                    .WithMany(c => c.Budgets)
                    .HasForeignKey(b => b.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(b => b.UserId);
                entity.HasIndex(b => new { b.UserId, b.Month, b.Year });
                entity.HasIndex(b => new { b.UserId, b.IsActive });
                entity.HasIndex(b => new { b.UserId, b.CategoryId, b.Month, b.Year });

                // Validaciones
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Budget_AssignedAmount", "\"AssignedAmount\" > 0"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Budget_SpentAmount", "\"SpentAmount\" >= 0"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Budget_Month", "\"Month\" BETWEEN 1 AND 12"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Budget_Year", "\"Year\" >= 2020"));

                // Ignorar propiedades computadas
                entity.Ignore(b => b.AvailableAmount);
                entity.Ignore(b => b.UsedPercentage);
                entity.Ignore(b => b.IsOverBudget);
                entity.Ignore(b => b.IsNearLimit);
            });

            // ====== CONFIGURACIÓN DE NOTIFICATIONS ======
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.NotificationId);

                entity.Property(n => n.NotificationDate)
                    .IsRequired();

                entity.Property(n => n.DueDate)
                    .IsRequired();

                entity.Property(n => n.IsRead)
                    .HasDefaultValue(false);

                entity.Property(n => n.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relación con ApplicationUser
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Category
                entity.HasOne(n => n.Category)
                    .WithMany()
                    .HasForeignKey(n => n.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => new { n.UserId, n.IsRead });
                entity.HasIndex(n => n.DueDate);

                // Ignorar propiedades computadas
                entity.Ignore(n => n.DaysUntilDue);
                entity.Ignore(n => n.IsOverdue);
                entity.Ignore(n => n.StatusText);
            });

            // ====== CONFIGURACIÓN DE LOANS ======
            builder.Entity<Loan>(entity =>
            {
                entity.HasKey(l => l.LoanId);

                entity.Property(l => l.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(l => l.Description)
                    .HasMaxLength(500);

                entity.Property(l => l.Icon)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasDefaultValue("🏦");

                entity.Property(l => l.PrincipalAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(l => l.InstallmentAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(l => l.NumberOfInstallments)
                    .IsRequired();

                entity.Property(l => l.DueDay)
                    .IsRequired();

                entity.Property(l => l.StartDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(l => l.IsActive)
                    .HasDefaultValue(true);

                entity.Property(l => l.InstallmentsPaid)
                    .HasDefaultValue(0);

                // Relación con ApplicationUser
                entity.HasOne(l => l.User)
                    .WithMany(u => u.Loans)
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Category
                entity.HasOne(l => l.Category)
                    .WithMany()
                    .HasForeignKey(l => l.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices
                entity.HasIndex(l => l.UserId);
                entity.HasIndex(l => new { l.UserId, l.IsActive });
                entity.HasIndex(l => l.CategoryId);

                // Validaciones
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Loan_PrincipalAmount", "\"PrincipalAmount\" > 0"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Loan_InstallmentAmount", "\"InstallmentAmount\" > 0"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Loan_NumberOfInstallments", "\"NumberOfInstallments\" >= 1 AND \"NumberOfInstallments\" <= 1000"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Loan_DueDay", "\"DueDay\" >= 1 AND \"DueDay\" <= 31"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Loan_InstallmentsPaid", "\"InstallmentsPaid\" >= 0"));

                // Ignorar propiedades computadas
                entity.Ignore(l => l.TotalToPay);
                entity.Ignore(l => l.TotalInterest);
                entity.Ignore(l => l.ApproximateInterestRate);
                entity.Ignore(l => l.RemainingInstallments);
                entity.Ignore(l => l.TotalPaid);
                entity.Ignore(l => l.ProgressPercentage);
                entity.Ignore(l => l.NextPaymentDate);
                entity.Ignore(l => l.IsCompleted);
                entity.Ignore(l => l.InterestRateCategory);
                entity.Ignore(l => l.InterestRateLabel);
            });
        }
    }
}
