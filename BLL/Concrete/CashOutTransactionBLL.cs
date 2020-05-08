using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models.CashOut;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Enums;
using System;

namespace BLL.Concrete
{
    public class CashOutTransactionBLL : Repository<CashOutTransaction>
    {
        /// <summary>
        /// Metodo para crear una solicitud de retiro
        /// </summary>
        /// <param name="request">Modelo de la solicitud</param>
        /// <param name="commerce">Comercio asociado</param>
        public void CreateCashOutRequest(CashOutRequest request, Commerce commerce)
        {
            // Inicializamos
            CashOutTransaction cashOut = new CashOutTransaction();
            var totals = new CashOutTotals();
            var amount = Convert.ToDecimal(request.Amount, System.Globalization.CultureInfo.InstalledUICulture);

            // Totalizamos los montos de la solicitud
            using (CommerceBalanceBLL CBBLL = new CommerceBalanceBLL())
            {
                totals = CBBLL.GetRequestTotals(amount, commerce);
            }

            // Construimos la transaccion de cashout
            cashOut.Id = Guid.NewGuid();
            cashOut.Amount = amount;
            cashOut.Description = request.Description;
            cashOut.CreateDate = DateTime.Now;
            cashOut.IdCommerceBankAccount = request.BankAccountId;
            cashOut.IdStatus = (int)CashOutStatus.Pending;
            cashOut.Commission = totals.BalanceTaxes.Commission;
            cashOut.IVA = totals.BalanceTaxes.CommissionIVA;
            cashOut.CommissionPercentage = totals.BalanceTaxes.CommissionPercentage;
            cashOut.Timestamp = DateTime.Now;
            cashOut.StatusChangeDate = DateTime.Now;

            // Agregamos la tabla a la base de datos
            AddEntity(cashOut);
            // Guardamos los cambios
            SaveChanges();

            // Agregamos los balances
            using (CommerceBalanceBLL CBBLL = new CommerceBalanceBLL())
            {
                CBBLL.AddBalance("Solicitud Retiro ID: " + cashOut.Id + " - Monto", commerce.Rif, totals.TotalCashOut, cashOut.Id, CommerceBalanceType.CashOut);
                CBBLL.AddBalance("Solicitud Retiro ID: " + cashOut.Id + " - Comision Fija", commerce.Rif, totals.BalanceTaxes.Commission, cashOut.Id, CommerceBalanceType.CashOut);
                CBBLL.AddBalance("Solicitud Retiro ID: " + cashOut.Id + " - Comision Porcentual", commerce.Rif, totals.BalanceTaxes.CommissionPercentage, cashOut.Id, CommerceBalanceType.CashOut);
                CBBLL.AddBalance("Solicitud Retiro ID: " + cashOut.Id + " - IVA", commerce.Rif, totals.BalanceTaxes.CommissionIVA, cashOut.Id, CommerceBalanceType.CashOut);
            }
        }

        /// <summary>
        /// Metodo para aprobar una solicitud de retiro
        /// </summary>
        /// <param name="requestId">Id de la solicitud</param>
        /// <param name="commerce">Comercio asociado</param>
        public void ApproveCashOutRequest(Guid requestId, Commerce commerce)
        {
            // Inicializamos
            CashOutTransaction cashOut = GetEntity(requestId);

            // Actualizamos la transaccion de cashout
            cashOut.IdStatus = (int)CashOutStatus.Approved;
            cashOut.Timestamp = DateTime.Now;
            cashOut.StatusChangeDate = DateTime.Now;

            // Guardamos los cambios
            SaveChanges();
        }

        /// <summary>
        /// Metodo para completar una solicitud de retiro
        /// </summary>
        /// <param name="requestId">Id de la solicitud</param>
        /// <param name="commerce">Comercio asociado</param>
        public void CompleteCashOutRequest(Guid requestId, Commerce commerce)
        {
            // Inicializamos
            CashOutTransaction cashOut = GetEntity(requestId);

            // Actualizamos la transaccion de cashout
            cashOut.IdStatus = (int)CashOutStatus.Completed;
            cashOut.Timestamp = DateTime.Now;
            cashOut.StatusChangeDate = DateTime.Now;

            // Guardamos los cambios
            SaveChanges();
        }

        /// <summary>
        /// Metodo para anular una solicitud de retiro
        /// </summary>
        /// <param name="requestId">Id de la solicitud</param>
        /// <param name="commerce">Comercio asociado</param>
        public void AnnullCashOutRequest(Guid requestId, Commerce commerce)
        {
            // Inicializamos
            CashOutTransaction cashOut = GetEntity(requestId);
            var totals = new CashOutTotals();

            // Totalizamos los montos de la solicitud
            using (CommerceBalanceBLL CBBLL = new CommerceBalanceBLL())
            {
                totals = CBBLL.GetRequestTotals(cashOut.Amount, commerce);
            }

            // Actualizamos la transaccion de cashout
            cashOut.IdStatus = (int)CashOutStatus.Annulled;
            cashOut.Timestamp = DateTime.Now;
            cashOut.StatusChangeDate = DateTime.Now;

            // Guardamos los cambios
            SaveChanges();

            // Agregamos los balances
            using (CommerceBalanceBLL CBBLL = new CommerceBalanceBLL())
            {
                CBBLL.AddBalance("Anulacion Retiro ID: " + cashOut.Id + " - Monto", commerce.Rif, totals.TotalCashOut * -1, cashOut.Id, CommerceBalanceType.CashOut);
                CBBLL.AddBalance("Anulacion Retiro ID: " + cashOut.Id + " - Comision Fija", commerce.Rif, totals.BalanceTaxes.Commission * -1, cashOut.Id, CommerceBalanceType.CashOut);
                CBBLL.AddBalance("Anulacion Retiro ID: " + cashOut.Id + " - Comision Porcentual", commerce.Rif, totals.BalanceTaxes.CommissionPercentage * -1, cashOut.Id, CommerceBalanceType.CashOut);
                CBBLL.AddBalance("Anulacion Retiro ID: " + cashOut.Id + " - IVA", commerce.Rif, totals.BalanceTaxes.CommissionIVA * -1, cashOut.Id, CommerceBalanceType.CashOut);
            }
        }

        /// <summary>
        /// Metodo para rechazar una solicitud de retiro
        /// </summary>
        /// <param name="requestId">Id de la solicitud</param>
        /// <param name="commerce">Comercio asociado</param>
        public void RejectCashOutRequest(Guid requestId, Commerce commerce)
        {
            // Inicializamos
            CashOutTransaction cashOut = GetEntity(requestId);
            var totals = new CashOutTotals();

            // Totalizamos los montos de la solicitud
            using (CommerceBalanceBLL CBBLL = new CommerceBalanceBLL())
            {
                totals = CBBLL.GetRequestTotals(cashOut.Amount, commerce);
            }

            // Actualizamos la transaccion de cashout
            cashOut.IdStatus = (int)CashOutStatus.Rejected;
            cashOut.Timestamp = DateTime.Now;
            cashOut.StatusChangeDate = DateTime.Now;

            // Guardamos los cambios
            SaveChanges();

            // Agregamos los balances
            using (CommerceBalanceBLL CBBLL = new CommerceBalanceBLL())
            {
                CBBLL.AddBalance("Rechazo Retiro ID: " + cashOut.Id + " - Monto", commerce.Rif, totals.TotalCashOut * -1, cashOut.Id, CommerceBalanceType.CashOut);
                CBBLL.AddBalance("Rechazo Retiro ID: " + cashOut.Id + " - Comision Fija", commerce.Rif, totals.BalanceTaxes.Commission * -1, cashOut.Id, CommerceBalanceType.CashOut);
                CBBLL.AddBalance("Rechazo Retiro ID: " + cashOut.Id + " - Comision Porcentual", commerce.Rif, totals.BalanceTaxes.CommissionPercentage * -1, cashOut.Id, CommerceBalanceType.CashOut);
                CBBLL.AddBalance("Rechazo Retiro ID: " + cashOut.Id + " - IVA", commerce.Rif, totals.BalanceTaxes.CommissionIVA * -1, cashOut.Id, CommerceBalanceType.CashOut);
            }
        }
    }
}
