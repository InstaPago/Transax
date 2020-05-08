using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbrella.Controllers;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.BLL.Models.PurchaseOrder;
using InstaTransfer.BLL.Models.PaymentUser;

namespace BackEndViewTest
{
    [TestClass]
    public class CommerceControllerTests
    {
        #region AnnulPurchaseOrder
        [TestMethod]
        public void AnnulPurchaseOrderTest_ValidPurchaseOrder_AnnulsPurchaseOrder()
        {
            // arrange
            Guid purchaseOrderId = new Guid("c9d31a91-701a-479b-bbda-88de45c9451b");
            string rif = "J401878105";
            bool expectedValue = true;
            bool actualValue;
            OrderController controller = new OrderController();

            // act
            var response = controller.TryAnnulPurchaseOrder(rif, purchaseOrderId);
            actualValue = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);

            // assert
            Assert.AreEqual(expectedValue, actualValue, "La orden de compra no pudo ser anulada");
        }

        [TestMethod]
        public void AnnulPurchaseOrderTest_ValidPurchaseOrder_ReconciledDeclaration()
        {
            // arrange
            Guid purchaseOrderId = new Guid("675ce7e6-a00c-4f64-8b37-83285909f04c");
            string rif = "J401878105";
            bool expectedValue = false;
            bool actualValue;
            OrderController controller = new OrderController();

            // act
            var result = controller.TryAnnulPurchaseOrder(rif, purchaseOrderId);
            actualValue = (bool)result.Data;
            // assert
            Assert.AreEqual(expectedValue, actualValue, "La orden de compra fue anulada");
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void AnnulPurchaseOrderTest_InvalidPurchaseOrder_ThrowsNullReferenceException()
        {
            // arrange
            Guid purchaseOrderId = new Guid("575ce7e6-a00c-4f64-8b37-83285909f04c");
            string rif = "J401878105";
            OrderController controller = new OrderController();
            bool actualValue;

            // act
            var result = controller.TryAnnulPurchaseOrder(rif, purchaseOrderId);
            actualValue = (bool)result.Data;
            // assert (Manejado por el ExpectedException)

        }
        #endregion

        #region ModifyPurchaseOrder

        /// <summary>
        /// Prueba la modificacion de una orden de compra valida y anulacion de todas sus declaraciones
        /// de no existir una declaracion conciliada
        /// </summary>
        [TestMethod]
        public void ModifyPurchaseOrderTest_ValidPurchaseOrder_ModifiesPurchaseOrder()
        {
            // arrange
            Guid purchaseOrderId = new Guid("8103f8f5-4447-4850-a4e0-103a0110fa21");
            bool expectedValue = true;
            bool actualValue;
            CommerceController controller = new CommerceController();
            PaymentUserModel paymentUserModel = new PaymentUserModel
            {
                userci = 20652831,
                useremail = "albertorojas580@gmail.com"
            };
            PurchaseOrderModel purchaseOrderModel = new PurchaseOrderModel
            {
                id = purchaseOrderId,
                amount = 500,
                paymentuser = paymentUserModel,
                ordernumber = "TRS4561339",
                rif = "J401878105"
            };

            // act
            var response = controller.ModifyPurchaseOrder(purchaseOrderModel);
            actualValue = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);

            // assert
            Assert.AreEqual(expectedValue, actualValue, "La orden de compra no pudo ser modificada");
        }

        /// <summary>
        /// Prueba la modificacion de una orden de compra valida con una declaracion conciliada
        /// </summary>
        [TestMethod]
        public void ModifyPurchaseOrderTest_ValidPurchaseOrder_ReconciledDeclaration()
        {
            // arrange
            Guid purchaseOrderId = new Guid("675ce7e6-a00c-4f64-8b37-83285909f04c");
            bool expectedValue = false;
            bool actualValue;
            CommerceController controller = new CommerceController();
            PaymentUserModel paymentUserModel = new PaymentUserModel
            {
                userci = 20652831,
                useremail = "albertorojas580@gmail.com"
            };
            PurchaseOrderModel purchaseOrderModel = new PurchaseOrderModel
            {
                id = purchaseOrderId,
                amount = 500,
                paymentuser = paymentUserModel,
                ordernumber = "TRS4561338",
                rif = "J401878105"
            };

            // act
            var response = controller.ModifyPurchaseOrder(purchaseOrderModel);
            actualValue = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);

            // assert
            Assert.AreEqual(expectedValue, actualValue, "La orden de compra fue modificada");
        }

        #endregion

    }
}
