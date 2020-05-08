using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbrella.Controllers;
using System.Collections.Generic;
using Umbrella.Models;

namespace BackEndViewTest
{
    [TestClass]
    public class DashboardControllerTests
    {
        #region TransactionsByDateAndStatus

        /// <summary>
        /// Devuelve el numero de transacciones (Declaraciones y Órdenes) por fecha y status
        /// </summary>
        [TestMethod]
        public void GetTransactionsCountByDateAndStatusCountTest_ValidCommerce_ReturnsTransactionsCount()
        {
            // arrange
            string rif = "J401878105";
            bool expectedValue = true;
            bool actualValue;
            DashboardViewModels.TransactionsByDateAndStatusModel transactionsCountModel;
            HomeController controller = new HomeController();

            // act
            actualValue = controller.GetTransactionsCountByDateAndStatus(rif, out transactionsCountModel);

            // assert
            Assert.AreEqual(expectedValue, actualValue, "Consulta fallida");
        }

        #endregion

        #region TotalAmountByDateAndBank

        /// <summary>
        /// Devuelve el monto total de las declaraciones conciliadas agrupado por bancos emisores
        /// </summary>
        [TestMethod]
        public void GetTotalAmountByDateAndBankTest_ValidCommerce_ReturnsTotalBankAmounts()
        {
            // arrange
            string rif = "J401878105";
            bool expectedValue = true;
            bool actualValue;
            List<DashboardViewModels.TotalBankAmountModel> bankTotals;
            HomeController controller = new HomeController();

            // act
            actualValue = controller.GetTotalAmountByDateAndBank(rif, out bankTotals);

            // assert
            Assert.AreEqual(expectedValue, actualValue, "Consulta fallida");
        }

        #endregion

        #region TopCommerces

        /// <summary>
        /// Devuelve el monto total y numero de declaraciones conciliadas agrupado por comercios
        /// </summary>
        [TestMethod]
        public void GetTopCommercesByDateAndTransactionsTests_ReturnsTop10CommercesByTotalAmount()
        {
            // arrange
            bool expectedValue = true;
            bool actualValue;
            DashboardViewModels model = new DashboardViewModels();
            HomeController controller = new HomeController();
            List<DashboardViewModels.TopCommercesModel> topCommerces;
            // act
            actualValue = controller.GetTopCommercesByDateAndTransactions(out topCommerces);

            // assert
            Assert.AreEqual(expectedValue, actualValue, "Consulta fallida");
        }

        #endregion
    }
}
