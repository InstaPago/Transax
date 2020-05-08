using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models.CashOut;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Concrete
{
    public class CommerceBalanceBLL : Repository<CommerceBalance>
    {
        /// <summary>
        /// Agrega un registro al balance del comercio
        /// </summary>
        /// <param name="description">Descripcion de la transaccion</param>
        /// <param name="rifCommerce">Rif del comercio</param>
        /// <param name="amount">Monto de la transaccion</param>
        /// <param name="idTransaction">Id de la transaccion</param>
        /// <param name="type">Tipo de transaccion</param>
        public void AddBalance(string description, string rifCommerce, decimal amount, Guid idTransaction, CommerceBalanceType type)
        {
            // Obtenemos el ultimo registro del balance del comercio
            CommerceBalance lastBalance = GetLastBalance(rifCommerce);
            CommerceBalance currentBalance = new CommerceBalance();

            // Verificamos que exista el ultimo balance para el comercio
            if (lastBalance != null)
            {
                // Verificamos el tipo de transaccion y creamos un registro nuevo
                switch (type)
                {
                    case CommerceBalanceType.Declaration:
                        currentBalance = new CommerceBalance()
                        {
                            Id = Guid.NewGuid(),
                            Description = description,
                            CreateDate = DateTime.Now,
                            RifCommerce = rifCommerce,
                            IdDeclaration = idTransaction,
                            Amount = amount,
                            PreviousBalance = lastBalance.CurrentBalance,
                            CurrentBalance = lastBalance.CurrentBalance + amount
                        };
                        break;
                    case CommerceBalanceType.CashOut:
                        currentBalance = new CommerceBalance()
                        {
                            Id = Guid.NewGuid(),
                            Description = description,
                            CreateDate = DateTime.Now,
                            RifCommerce = rifCommerce,
                            IdCashOutTransaction = idTransaction,
                            Amount = amount,
                            PreviousBalance = lastBalance.CurrentBalance,
                            CurrentBalance = lastBalance.CurrentBalance - amount
                        };
                        break;
                }
            }
            // No existen balances anteriores
            else
            {
                switch (type)
                {
                    case CommerceBalanceType.Declaration:
                        currentBalance = new CommerceBalance()
                        {
                            Id = Guid.NewGuid(),
                            Description = description,
                            CreateDate = DateTime.Now,
                            RifCommerce = rifCommerce,
                            IdDeclaration = idTransaction,
                            Amount = amount,
                            PreviousBalance = 0,
                            CurrentBalance = 0 + amount
                        };
                        break;
                    case CommerceBalanceType.CashOut:
                        currentBalance = new CommerceBalance()
                        {
                            Id = Guid.NewGuid(),
                            Description = description,
                            CreateDate = DateTime.Now,
                            RifCommerce = rifCommerce,
                            IdCashOutTransaction = idTransaction,
                            Amount = amount,
                            PreviousBalance = 0,
                            CurrentBalance = 0

                        };
                        break;
                }
            }
            // Agregamos el registro a la base de datos
            AddEntity(currentBalance);
            // Guardamos los cambios
            SaveChanges();
        }

        /// <summary>
        /// Obtiene el ultimo registro del balance de un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio especifico</param>
        /// <returns>Ultimo balance del comercio</returns>
        public CommerceBalance GetLastBalance(string rif)
        {
            // Obtenemos el ultimo registro insertado
            return GetAllRecords(cb => cb.RifCommerce == rif).OrderByDescending(cb => cb.CreateDate).FirstOrDefault();
        }

        /// <summary>
        /// Retorna la totalizacion de la solicitud de retiro
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="commerce"></param>
        /// <returns></returns>
        public CashOutTotals GetRequestTotals(decimal amount, Commerce commerce)
        {
            // Obtenemos y guardamos las comisiones e impuestos
            var taxes = GetTaxes(amount, commerce);
            // Obtenemos y guardamos los totales del balance
            var balance = GetBalance(commerce);
            // Obtenemos el total de la transaccion
            var totals = new CashOutTotals()
            {
                Amount = amount,
                BalanceTotals = balance,
                BalanceTaxes = taxes,
                TotalCashOut = amount - taxes.CommissionTotal
            };
            // Retornamos los totales
            return totals;
        }

        /// <summary>
        /// Retorna la totalizacion del balance del comercio
        /// </summary>
        /// <param name="commerce">Comercio especifico</param>
        /// <returns>Totalizacion del balance</returns>
        public CashOutTotals.Balance GetBalance(Commerce commerce)
        {
            // Inicializamos las variables
            CashOutTotals.Balance totals = new CashOutTotals.Balance();
            decimal currentBalance;
            decimal positiveBalance;
            decimal negativeBalance;
            CommerceBalance lastBalance = new CommerceBalance();
            // Obtenemos el ultimo registro en balance
            lastBalance = GetLastBalance(commerce.Rif);

            // Calculamos los totales
            currentBalance = lastBalance != null ? lastBalance.CurrentBalance : 0;
            positiveBalance = GetAllRecords(cb => cb.RifCommerce == commerce.Rif && cb.Amount >= 0 && cb.IdCashOutTransaction.Equals(null) && cb.IdDeclaration != null).Select(cb => cb.Amount).AsEnumerable().DefaultIfEmpty(0).Sum();
            negativeBalance = GetAllRecords(cb => cb.RifCommerce == commerce.Rif && cb.Amount >= 0 && cb.IdDeclaration.Equals(null) && cb.IdCashOutTransaction != null).Select(cb => cb.Amount).AsEnumerable().DefaultIfEmpty(0).Sum();

            // Construimos el modelo de los totales
            totals.CurrentBalance = currentBalance;
            totals.PositiveBalance = positiveBalance;
            totals.NegativeBalance = negativeBalance;

            // Retornamos el modelo
            return totals;

        }

        /// <summary>
        /// Retorna las comisiones del comercio y monto a retirar asociado
        /// </summary>
        /// <param name="commerce">Comercio asociado</param>
        /// <param name="amount">Monto a retirar</param>
        /// <returns>Modelo de comisiones del retiro</returns>
        public CashOutTotals.Taxes GetTaxes(decimal amount, Commerce commerce)
        {
            // Inicializamos las variables
            CashOutTotals.Taxes taxes = new CashOutTotals.Taxes();
            decimal commissionFixed;
            decimal commissionPercentage;
            decimal commissionIVA;
            
            // Obtenemos las comisiones desde el app.config
            decimal IVA = decimal.Parse(System.Configuration.ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyIVA]);
            // Obtenemos las comisiones desde la base de datos
            decimal commerceCommissionFixed = commerce.Commission;
            decimal commerceCommissionPercentage = commerce.WithdrawalFee;
            // Calculamos los totales
            commissionFixed = commerceCommissionFixed;
            commissionPercentage = commerceCommissionPercentage * amount;
            commissionIVA = ((commissionPercentage + commissionFixed) * IVA) / 100;

            // Construimos el modelo de las comisiones
            taxes.Commission = commissionFixed;
            taxes.CommissionPercentage = commissionPercentage;
            taxes.CommissionIVA = commissionIVA;

            //Retornamos el modelo
            return taxes;

        }
    }
}
