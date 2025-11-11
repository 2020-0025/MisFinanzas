using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace MisFinanzas.Domain.Entities
{
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Debe ser mayor a 0")]
        [Display(Name = "Monto del préstamo")]
        public decimal PrincipalAmount { get; set; } // Monto real prestado

        [Required(ErrorMessage = "Este campo es requerido")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Debe ser mayor a 0")]
        [Display(Name = "Cuota mensual")]
        public decimal InstallmentAmount { get; set; } // Cuota mensual

        [Required(ErrorMessage = "Este campo es requerido")]
        [Range(1, 1000, ErrorMessage = "Debe estar entre 1 y 1000")]
        [Display(Name = "Cantidad de cuotas")]
        public int NumberOfInstallments { get; set; } // Cantidad de cuotas

        [Required(ErrorMessage = "Este campo es requerido")]
        [Range(1, 31, ErrorMessage = "Debe estar entre 1 y 31")]
        [Display(Name = "Día de pago")]
        public int DueDay { get; set; } // Día del mes para pagar

        [Required(ErrorMessage = "Este campo es requerido")]
        [Display(Name = "Fecha de inicio")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Este campo es requerido")]
        [StringLength(10, ErrorMessage = "Máximo 10 caracteres")]
        [Display(Name = "Icono")]
        public string Icon { get; set; } = "🏦";

        public bool IsActive { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int InstallmentsPaid { get; set; } = 0; // Cuántas cuotas pagadas

        // Foreign Keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        public int CategoryId { get; set; } // Categoría auto-creada (se asigna en servidor)

        // Navigation Properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Category? Category { get; set; }

        // Computed Properties
        [NotMapped]
        public decimal TotalToPay => InstallmentAmount * NumberOfInstallments;

        [NotMapped]
        public decimal TotalInterest => TotalToPay - PrincipalAmount;

        [NotMapped]
        public decimal ApproximateInterestRate
        {
            get
            {
                if (PrincipalAmount <= 0 || NumberOfInstallments <= 0) return 0;

                // Tasa de interés simple aproximada anual
                decimal interestRate = (TotalInterest / PrincipalAmount) * (12m / NumberOfInstallments) * 100m;
                return Math.Round(interestRate, 2);
            }
        }

        [NotMapped]
        public int RemainingInstallments => Math.Max(NumberOfInstallments - InstallmentsPaid, 0);

        [NotMapped]
        public decimal TotalPaid => InstallmentsPaid * InstallmentAmount;

        [NotMapped]
        public decimal ProgressPercentage
        {
            get
            {
                if (NumberOfInstallments == 0) return 0;
                var percentage = ((decimal)InstallmentsPaid / NumberOfInstallments) * 100m;
                return Math.Min(percentage, 100);
            }
        }

        [NotMapped]
        public DateTime NextPaymentDate
        {
            get
            {
                if (!IsActive || InstallmentsPaid >= NumberOfInstallments)
                    return DateTime.MinValue;

                var today = DateTime.Now;
                var nextPayment = new DateTime(today.Year, today.Month, Math.Min(DueDay, DateTime.DaysInMonth(today.Year, today.Month)));

                // Si ya pasó el día de pago este mes, calcular para el próximo mes
                if (nextPayment < today)
                {
                    nextPayment = nextPayment.AddMonths(1);
                    nextPayment = new DateTime(nextPayment.Year, nextPayment.Month, Math.Min(DueDay, DateTime.DaysInMonth(nextPayment.Year, nextPayment.Month)));
                }

                return nextPayment;
            }
        }

        [NotMapped]
        public bool IsCompleted => InstallmentsPaid >= NumberOfInstallments;

        [NotMapped]
        public string InterestRateCategory
        {
            get
            {
                var rate = ApproximateInterestRate;
                if (rate <= 15) return "favorable";
                if (rate <= 30) return "moderada";
                return "alta";
            }
        }

        [NotMapped]
        public string InterestRateLabel
        {
            get
            {
                return InterestRateCategory switch
                {
                    "favorable" => "Tasa favorable",
                    "moderada" => "Tasa moderada",
                    "alta" => "Tasa alta - Considerar refinanciar",
                    _ => ""
                };
            }
        }
    }
}