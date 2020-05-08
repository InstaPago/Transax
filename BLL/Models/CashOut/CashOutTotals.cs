namespace InstaTransfer.BLL.Models.CashOut
{
    /// <summary>
    /// Modelo de totalizacion de retiros
    /// </summary>
    public class CashOutTotals
    {
        /// <summary>
        /// Montos totales del balance
        /// </summary>
        public class Balance
        {
            /// <summary>
            /// Total Disponible
            /// </summary>
            public decimal CurrentBalance { get; set; }
            /// <summary>
            /// Total Ingresos
            /// </summary>
            public decimal PositiveBalance { get; set; }
            /// <summary>
            /// Total Retirado
            /// </summary>
            public decimal NegativeBalance { get; set; }
            /// <summary>
            /// Constructor de la clase
            /// </summary>
            public Balance()
            {
                CurrentBalance = 0;
                PositiveBalance = 0;
                NegativeBalance = 0;
            }
        }
        /// <summary>
        /// Comisiones de los retiros
        /// </summary>
        public class Taxes
        {
            /// <summary>
            /// Comision fija
            /// </summary>
            public decimal Commission { get; set; }
            /// <summary>
            /// Porcentaje de comision
            /// </summary>
            public decimal CommissionPercentage { get; set; }
            /// <summary>
            /// IVA sobre la comision
            /// </summary>
            public decimal CommissionIVA { get; set; }
            /// <summary>
            /// Total de las comisiones 
            /// </summary>
            public decimal CommissionTotal { get { return Commission + CommissionPercentage + CommissionIVA; } }
            /// <summary>
            /// Constructor de la clase
            /// </summary>
            public Taxes()
            {
                Commission = 0;
                CommissionPercentage = 0;
                CommissionIVA = 0;
            }
        }

        /// <summary>
        /// Montos totales del balance
        /// </summary>
        public Balance BalanceTotals { get; set; }
        /// <summary>
        /// Comisiones de los retiros
        /// </summary>
        public Taxes BalanceTaxes { get; set; }
        /// <summary>
        /// Total a Retirar. Monto - comisiones
        /// </summary>
        public decimal TotalCashOut { get; set; }
        /// <summary>
        /// Monto a retirar
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public CashOutTotals()
        {
            BalanceTotals = new Balance();
            BalanceTaxes = new Taxes();
            TotalCashOut = 0;
            Amount = 0;
        }
    }
}