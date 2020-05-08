using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InstaTransfer.ITReconciliatorService.Reconciliator;
using InstaTransfer.DataAccess;
using BLL.Concrete;

namespace InstaTransfer.ITReconciliatorServiceTest
{
    [TestClass]
    public class ReconciliatorServiceTests
    {
        [TestMethod]
        public void RunReconciliatorTest()
        { 
            // arrange
            bool expectedValue = true;
            bool actualValue;
            int successReconciliationsCount;
            int failedReconciliationsCount;
            int pendingReconciliationsCount;
            int requestsCount;
            ReconciliatorService service = new ReconciliatorService();

            // act
            actualValue = service.RunReconciliator(out successReconciliationsCount, out failedReconciliationsCount, out pendingReconciliationsCount, out requestsCount);

            // assert
            Assert.AreEqual(expectedValue, actualValue, "Conciliacion fallida");
        }

        [TestMethod]
        public void SendMailTest()
        {
            // arrange
            bool expectedValue = true;
            bool actualValue;
            PaymentRequest request = new PaymentRequest();

            using (PaymentRequestBLL PRBLL = new PaymentRequestBLL())
            {
                request = PRBLL.GetEntity(Guid.Parse("013B7CE2-D3F7-41EC-AAA2-BBA2E410B4C5"));
            }
            ReconciliatorService service = new ReconciliatorService();

            // act
            actualValue = service.SendMail(request);

            // assert
            Assert.AreEqual(expectedValue, actualValue, "Envio de correo fallido");
        }
    }
}
