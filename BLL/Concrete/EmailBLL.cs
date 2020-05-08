using InstaTransfer.BLL.Models;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BLL.Concrete
{
    /// <summary>
    /// Logica de construccion y envio de correos
    /// </summary>
    public class EmailBLL
    {
        ///// <summary>
        ///// Envia el correo segun un tipo especifico y un objeto de datos
        ///// </summary>
        ///// <param name="model">Datos para el correo</param>
        ///// <param name="type">Tipo de correo</param>
        ///// <returns>Resultado de la operacion</returns>
        //public JsonResult SendMail(object model, EmailType type)
        //{
        //    // Variables
        //    EmailHelper emailHelper = new EmailHelper();
        //    EmailModels.PaymentRequestEmailModel emailModel = new EmailModels.PaymentRequestEmailModel();
        //    string message = string.Empty;
        //    try
        //    {
        //        // Construimos el cuerpo segun el tipo de correo
        //        switch (type)
        //        {
        //            case EmailType.NewPaymentRequest:
        //                break;
        //            case EmailType.DeclarationSuccess:
        //                {
        //                    // Construimos el correo
        //                    emailModel = BuildEmailBody(model, type);
        //                    // Agregamos los estilos al correo y renderizamos vista parcial
        //                    message = RenderPartialViewToString(ControllerContext.Controller, "Pages", "SolicitudPagoEmail", emailModel);

        //                    break;
        //                }
        //            case EmailType.ReconciliationSuccess:
        //                {
        //                    // Construimos el correo
        //                    emailModel = BuildEmailBody(model, type);
        //                    // Agregamos los estilos al correo y renderizamos vista parcial
        //                    message = RenderPartialViewToString(ControllerContext.Controller, "Pages", "SolicitudPagoEmail", emailModel);

        //                    break;
        //                }
        //            case EmailType.RecoverCommerceUserPW:
        //                break;
        //            case EmailType.RecoverEndUserPW:
        //                break;
        //            default:
        //                break;
        //        }
        //        // Enviamos el correo
        //        emailHelper.SendEmailMessage(emailModel.From, emailModel.DisplayName, emailModel.To, emailModel.Subject, message);
        //    }
        //    catch (Exception)
        //    {
        //        // Error
        //        _baseErrorResponse = new BaseErrorResponse(BackEndErrors.PaymentRequestSendErrorMessage, BackEndErrors.PaymentRequestSendErrorCode);
        //        var badresult = new
        //        {
        //            success = _baseErrorResponse.Success,
        //            message = _baseErrorResponse.ResponseMessage
        //        };
        //        return new JsonResult { Data = badresult };
        //    }
        //    // Success
        //    return new JsonResult()
        //    {
        //        Data = new
        //        {
        //            success = true,
        //            message = BackEndResources.PaymentRequestSuccess
        //        }
        //    };

        //}

    }
}
