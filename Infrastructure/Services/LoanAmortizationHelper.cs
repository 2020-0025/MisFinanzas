namespace MisFinanzas.Infrastructure.Services
{
    public static class LoanAmortizationHelper
    {
        // Calcula la tasa de interés aproximada cuando no se proporciona
        public static decimal CalculateApproximateInterestRate(
            decimal principalAmount,
            decimal installmentAmount,
            int numberOfInstallments)
        {
            if (principalAmount <= 0 || numberOfInstallments <= 0) return 0;

            decimal totalToPay = installmentAmount * numberOfInstallments;
            decimal totalInterest = totalToPay - principalAmount;

            // Tasa simple aproximada anual
            decimal interestRate = (totalInterest / principalAmount) * (12m / numberOfInstallments) * 100m;
            return Math.Max(0, Math.Round(interestRate, 2));
        }

        // Calcula la cuota mensual usando el método francés
        public static decimal CalculateMonthlyPayment(
            decimal principalAmount,
            decimal annualInterestRate,
            int numberOfInstallments)
        {
            if (annualInterestRate == 0)
            {
                return principalAmount / numberOfInstallments;
            }

            decimal monthlyRate = annualInterestRate / 100m / 12m;
            decimal payment = principalAmount *
                (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), numberOfInstallments)) /
                ((decimal)Math.Pow((double)(1 + monthlyRate), numberOfInstallments) - 1);

            return Math.Round(payment, 2);
        }

        // Genera tabla de amortización completa (Método Francés)
        public static List<(int Number, DateTime DueDate, decimal Principal, decimal Interest, decimal Total, decimal Balance)>
            GenerateAmortizationSchedule(
                decimal principalAmount,
                decimal annualInterestRate,
                int numberOfInstallments,
                DateTime startDate,
                int dueDay)
        {
            var schedule = new List<(int, DateTime, decimal, decimal, decimal, decimal)>();
            decimal monthlyRate = annualInterestRate / 100m / 12m;
            decimal remainingBalance = principalAmount;

            // Calcular cuota fija
            decimal monthlyPayment = CalculateMonthlyPayment(principalAmount, annualInterestRate, numberOfInstallments);

            for (int i = 1; i <= numberOfInstallments; i++)
            {
                // Calcular fecha de vencimiento
                DateTime dueDate = startDate.AddMonths(i);
                int maxDaysInMonth = DateTime.DaysInMonth(dueDate.Year, dueDate.Month);
                int actualDay = Math.Min(dueDay, maxDaysInMonth);
                dueDate = new DateTime(dueDate.Year, dueDate.Month, actualDay);

                // Calcular interés de esta cuota
                decimal interestAmount = remainingBalance * monthlyRate;

                // Calcular capital de esta cuota
                decimal principalPayment = monthlyPayment - interestAmount;

                // Ajustar última cuota si es necesario (por redondeos)
                if (i == numberOfInstallments)
                {
                    principalPayment = remainingBalance;
                    monthlyPayment = principalPayment + interestAmount;
                }

                // Actualizar saldo
                remainingBalance -= principalPayment;

                schedule.Add((
                    i,
                    dueDate,
                    Math.Round(principalPayment, 2),
                    Math.Round(interestAmount, 2),
                    Math.Round(monthlyPayment, 2),
                    Math.Round(Math.Max(0, remainingBalance), 2)
                ));
            }

            return schedule;
        }
    }
}
