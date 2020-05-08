using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models.PaymentRequest;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Concrete
{
    public class PaymentRequestBLL : Repository<PaymentRequest>
    {
        public PaymentRequest CreatePaymentRequest(PaymentRequestModel requestModel, CUser cuser)
        {
            // Inicializamos
            PaymentRequest request = new PaymentRequest();
            var order = new CPurchaseOrder();
            var amount = Convert.ToDecimal(requestModel.Amount, System.Globalization.CultureInfo.InstalledUICulture);
            EndUser endUser;
            // Construimos la solicitud de pago
            request.Id = Guid.NewGuid();
            request.Amount = amount;
            request.Description = requestModel.Description;
            request.RifCommerce = cuser.RifCommerce;
            request.RequestEmail = requestModel.RequestEmail;
            request.IdPaymentRequestStatus = (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Pending;
            request.TimesSent = 0;
            request.CreateDate = DateTime.Now;
            request.ChangeDate = DateTime.Now;

            // Verificamos si existe el usuario pagador
            using (EndUserBLL EUBLL = new EndUserBLL())
            {
                endUser = EUBLL.GetEndUserByCI(requestModel.UserCI);
            }
            // Asignamos el usuario a la solicitud
            if (endUser != null)
            {
                request.IdEndUser = endUser.Id;
            }
            else
            {
                // Creamos el usuario pagador asociado si no existe
                request.EndUser = new EndUser
                {
                    Id = Guid.NewGuid(),
                    CI = requestModel.UserCI,
                    Name = requestModel.UserName,
                    LastName = requestModel.UserLastName,
                    Phone = requestModel.UserPhone
                };
            }


            // Creamos la orden de compra asociada
            using (CPurchaseOrderBLL CPOBLL = new CPurchaseOrderBLL())
            {
                order = CPOBLL.CreatePurchaseOrder("Solicitud Pago ID: " + request.Id, amount, cuser, requestModel.UserCI, requestModel.RequestEmail);
            }
            request.IdCPurchaseOrder = order.Id;

            // Agregamos la tabla a la base de datos
            AddEntity(request);
            // Guardamos los cambios
            SaveChanges();

            // Devolvemos la solicitud
            return request;
        }
    }
}
