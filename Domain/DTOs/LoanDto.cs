namespace MisFinanzas.Domain.DTOs
{
    public class LoanDto
    {

        public int LoanId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InstallmentAmount { get; set; }
        public int NumberOfInstallments { get; set; }
        public int DueDay { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public string Icon { get; set; } = "🏦";
        public bool IsActive { get; set; } = true;
        public int InstallmentsPaid { get; set; } = 0;

        // Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public int CategoryId { get; set; }

        // Computed Properties
        public decimal TotalToPay => InstallmentAmount * NumberOfInstallments;

        public decimal TotalInterest => TotalToPay - PrincipalAmount;

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

        public int RemainingInstallments => Math.Max(NumberOfInstallments - InstallmentsPaid, 0);

        public decimal TotalPaid => InstallmentsPaid * InstallmentAmount;

        public decimal ProgressPercentage
        {
            get
            {
                if (NumberOfInstallments == 0) return 0;
                var percentage = ((decimal)InstallmentsPaid / NumberOfInstallments) * 100m;
                return Math.Min(percentage, 100);
            }
        }

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

        public bool IsCompleted => InstallmentsPaid >= NumberOfInstallments;

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
