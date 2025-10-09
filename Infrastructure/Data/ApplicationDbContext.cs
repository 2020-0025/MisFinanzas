using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.Entities;

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
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<FinancialGoal> FinancialGoals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ====== CONFIGURACIÓN DE RELACIONES ======

            // Category → User (N:1)
            builder.Entity<Category>()
                .HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Transaction → User (N:1)
            builder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Transaction → Category (N:1)
            builder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // No eliminar categoría si tiene transacciones

            // FinancialGoal → User (N:1)
            builder.Entity<FinancialGoal>()
                .HasOne(g => g.User)
                .WithMany(u => u.FinancialGoals)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ====== CONFIGURACIÓN DE ÍNDICES (Performance) ======

            // Índices para búsquedas frecuentes
            builder.Entity<Transaction>()
                .HasIndex(t => t.Date)
                .HasDatabaseName("IX_Transaction_Date");

            builder.Entity<Transaction>()
                .HasIndex(t => t.UserId)
                .HasDatabaseName("IX_Transaction_UserId");

            builder.Entity<Category>()
                .HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Category_UserId");

            builder.Entity<FinancialGoal>()
                .HasIndex(g => g.UserId)
                .HasDatabaseName("IX_FinancialGoal_UserId");

            builder.Entity<FinancialGoal>()
                .HasIndex(g => g.TargetDate)
                .HasDatabaseName("IX_FinancialGoal_TargetDate");

            // ====== CONFIGURACIÓN DE VALORES POR DEFECTO ======

            builder.Entity<Category>()
                .Property(c => c.Icon)
                .HasDefaultValue("📁");

            builder.Entity<Transaction>()
                .Property(t => t.Date)
                .HasDefaultValueSql("datetime('now')");

            builder.Entity<FinancialGoal>()
                .Property(g => g.StartDate)
                .HasDefaultValueSql("datetime('now')");

            builder.Entity<FinancialGoal>()
                .Property(g => g.CurrentAmount)
                .HasDefaultValue(0);
        }
    }
}
