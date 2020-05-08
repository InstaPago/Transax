using InstaTransfer.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Umbrella.Models
{
    public class DashboardViewModels
    {
        #region Variables

        /// <summary>
        /// Lista de Bancos por monto total de transacciones
        /// </summary>
        public List<TotalBankAmountModel> TotalBankAmountModelList { get; set; }

        /// <summary>
        /// Lista de Comercios por monto y número de transacciones
        /// </summary>
        public List<TopCommercesModel> TopCommercesModelList { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor de los modelos del dashboard
        /// </summary>
        public DashboardViewModels()
        {
            this.TotalBankAmountModelList = new List<TotalBankAmountModel>();
            this.TopCommercesModelList = new List<TopCommercesModel>();
        }

        #endregion

        #region Models

        /// <summary>
        /// Modelo base de resumen del dia
        /// </summary>
        public class DaySummaryBaseModel
        {
            /// <summary>
            /// Resultado de la operacion
            /// </summary>
            public bool Result { get; set; }

            /// <summary>
            /// Numero de transacciones
            /// </summary>
            public int Count { get; set; }

            /// <summary>
            /// Porcentaje de transacciones
            /// </summary>
            public int Percentage { get; set; }


        }

        /// <summary>
        /// Modelo de resumen del dia
        /// </summary>
        public class DaySummaryModel
        {
            /// <summary>
            /// Numero de declaraciones anuladas del dia
            /// </summary>
            public class AnnuledDeclaration : DaySummaryBaseModel { };
            public AnnuledDeclaration AnnuledDeclarations { get; set; }

            /// <summary>
            /// Numero de declaraciones conciliadas del dia
            /// </summary>
            public class ReconciledDeclaration : DaySummaryBaseModel { }
            public ReconciledDeclaration ReconciledDeclarations { get; set; }

            /// <summary>
            /// Numero de declaraciones por conciliar del dia
            /// </summary>
            public class PendingDeclaration : DaySummaryBaseModel { }
            public PendingDeclaration PendingDeclarations { get; set; }

            /// <summary>
            /// Numero de ordenes anuladas del dia
            /// </summary>
            public class AnnuledPurchaseOrder : DaySummaryBaseModel { }
            public AnnuledPurchaseOrder AnnuledPurchaseOrders { get; set; }

            /// <summary>
            /// Numero de ordenes declaradas del dia
            /// </summary>
            public class DeclaredPurchaseOrder : DaySummaryBaseModel { }
            public DeclaredPurchaseOrder DeclaredPurchaseOrders { get; set; }

            /// <summary>
            /// Numero de ordenes por declarar del dia
            /// </summary>
            public class PendingPurchaseOrder : DaySummaryBaseModel { }
            public PendingPurchaseOrder PendingPurchaseOrders { get; set; }

            public DaySummaryModel()
            {
                this.AnnuledDeclarations = new AnnuledDeclaration();
                this.ReconciledDeclarations = new ReconciledDeclaration();
                this.PendingDeclarations = new PendingDeclaration();
                this.AnnuledPurchaseOrders = new AnnuledPurchaseOrder();
                this.DeclaredPurchaseOrders = new DeclaredPurchaseOrder();
                this.PendingPurchaseOrders = new PendingPurchaseOrder();
            }
        }

        /// <summary>
        /// Modelo Top de Comercios por monto y número de transacciones
        /// </summary>
        public class TopCommercesModel
        {
            /// <summary>
            /// Comercio
            /// </summary>
            public string Commerce { get; set; }

            /// <summary>
            /// Numero de transacciones
            /// </summary>
            public int Count { get; set; }

            /// <summary>
            /// Monto Total de transacciones
            /// </summary>
            public decimal TotalAmount { get; set; }

        }


        /// <summary>
        /// Modelo del monto total por banco
        /// </summary>
        public class TotalBankAmountModel
        {
            /// <summary>
            /// Banco
            /// </summary>
            public string Bank { get; set; }

            /// <summary>
            /// Monto Total de transacciones
            /// </summary>
            public decimal TotalAmount { get; set; }

        }

        /// <summary>
        /// Modelo creditos por cuenta pote
        /// </summary>
        public class TotalBankAccountCreditsModel
        {
            /// <summary>
            /// Nombre de la Cuenta Pote
            /// </summary>
            public string BankAccount { get; set; }

            /// <summary>
            /// Banco
            /// </summary>
            public string ReceivingBank { get; set; }

            /// <summary>
            /// Monto Total de creditos en cuenta
            /// </summary>
            public decimal TotalAmount { get; set; }

        }

        /// <summary>
        /// Modelo creditos por banco emisor
        /// </summary>
        public class TotalBankCreditsModel
        {
            /// <summary>
            /// Nombre de la Cuenta Pote
            /// </summary>
            public string BankAccount { get; set; }

            /// <summary>
            /// Banco
            /// </summary>
            public string IssuingBank { get; set; }

            /// <summary>
            /// Monto Total de creditos en cuenta
            /// </summary>
            public decimal TotalAmount { get; set; }

        }

        /// <summary>
        /// Modelo del numero de transacciones (Declaraciones y Ordenes) por fecha y estado
        /// </summary>
        public class TransactionsByDateAndStatusModel
        {
            /// <summary>
            /// Numero de declaraciones anuladas del mes
            /// </summary>
            public class AnnuledDeclaration : BaseTransactionModel { };
            public AnnuledDeclaration AnnuledDeclarations { get; set; }

            /// <summary>
            /// Numero de declaraciones conciliadas del mes
            /// </summary>
            public class ReconciledDeclaration : BaseTransactionModel { }
            public ReconciledDeclaration ReconciledDeclarations { get; set; }

            /// <summary>
            /// Numero de declaraciones por conciliar del mes
            /// </summary>
            public class PendingDeclaration : BaseTransactionModel { }
            public PendingDeclaration PendingDeclarations { get; set; }

            /// <summary>
            /// Numero de ordenes anuladas del mes
            /// </summary>
            public class AnnuledPurchaseOrder : BaseTransactionModel { }
            public AnnuledPurchaseOrder AnnuledPurchaseOrders { get; set; }

            /// <summary>
            /// Numero de ordenes declaradas del mes
            /// </summary>
            public class DeclaredPurchaseOrder : BaseTransactionModel { }
            public DeclaredPurchaseOrder DeclaredPurchaseOrders { get; set; }

            /// <summary>
            /// Numero de ordenes por declarar del mes
            /// </summary>
            public class PendingPurchaseOrder : BaseTransactionModel { }
            public PendingPurchaseOrder PendingPurchaseOrders { get; set; }

            /// <summary>
            /// Numero de ordenes declaradas y conciliadas del mes
            /// </summary>
            public class DeclaredReconciledPurchaseOrder : BaseTransactionModel { }
            public DeclaredReconciledPurchaseOrder DeclaredReconciledPurchaseOrders { get; set; }

            public TransactionsByDateAndStatusModel()
            {
                this.AnnuledDeclarations = new AnnuledDeclaration();
                this.ReconciledDeclarations = new ReconciledDeclaration();
                this.PendingDeclarations = new PendingDeclaration();
                this.AnnuledPurchaseOrders = new AnnuledPurchaseOrder();
                this.DeclaredPurchaseOrders = new DeclaredPurchaseOrder();
                this.PendingPurchaseOrders = new PendingPurchaseOrder();
                this.DeclaredReconciledPurchaseOrders = new DeclaredReconciledPurchaseOrder();
            }
        }

        #endregion

    }
}