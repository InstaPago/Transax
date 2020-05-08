using BLL.Concrete;
using InstaTransfer.BLL.Models.Email;
using InstaTransfer.BLL.Models;
using InstaTransfer.BLL.Models.PaymentRequest;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.ITResources.General;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static InstaTransfer.ITLogic.Helpers.SessionHelper;
using InstaTransfer.ITExceptions.BackEnd;
using InstaTransfer.ITResources.Enums;
using Umbrella.App_Code;
using System.Net;

namespace Umbrella.Controllers
{
    [Authorize(Roles =
  UserRoleConstant.CommerceAdmin + "," +
  UserRoleConstant.CommerceUser
 )]
    public class PaymentRequestController : Controller
    {
        #region Variables

        PaymentRequestBLL PRBLL = new PaymentRequestBLL();
        PaymentRequestStatusBLL PRSBLL = new PaymentRequestStatusBLL();
        BaseSuccessResponse _baseSuccessResponse;
        BaseErrorResponse _baseErrorResponse;
        CUser cuser = MySession.Current.CommerceUser;

        private ApplicationUserManager _userManager;
        #endregion

        #region Actions
        // GET: PaymentRequest
        [SessionExpireFilter]
        public ActionResult Index()
        {
            var statusList = new List<InstaTransfer.DataAccess.PaymentRequestStatus>();
            var Requests = GetRequests();

            // Lista de estados para el filtro
            statusList = PRSBLL.GetAllRecords().ToList();
            ViewBag.statusList = statusList;

            return View(Requests);
        }

        /// <summary>
        /// Metodo para solicitar un pago
        /// </summary>
        /// <returns>Resultado</returns>
        [HttpPost]
        [Authorize(Roles =
          UserRoleConstant.CommerceAdmin + "," +
          UserRoleConstant.CommerceUser
        )]

        public ActionResult RequestPayment()
        {
            // Instanciamos las variables
            var request = new PaymentRequestModel();

            // Retornamos la vista
            return PartialView("_RequestPayment", request);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Crea una nueva solicitud de pago
        /// </summary>
        /// <param name="request">Solicitud a crear</param>
        /// <returns>Resultado de la operacion</returns>
        public JsonResult CreatePaymentRequest(PaymentRequestModel requestModel)
        {
            // Inicializamos
            var amount = Convert.ToDecimal(requestModel.Amount, CultureInfo.InvariantCulture);
            PaymentRequest request = new InstaTransfer.DataAccess.PaymentRequest();
            var errorList = new List<string>();

            // Verificamos ModelState
            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        if (error.ErrorMessage != string.Empty)
                        {
                            errorList.Add(error.ErrorMessage);
                        }
                    }
                }
                _baseErrorResponse = new BaseErrorResponse(errorList, null);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.Message,
                };
                return new JsonResult { Data = badresult };
            }

            try
            {
                //Creamos el request en la base de datos
                using (PaymentRequestBLL PRBLL = new PaymentRequestBLL())
                {
                    request = PRBLL.CreatePaymentRequest(requestModel, cuser);
                }

                // Armamos el cuerpo del correo

                // Enviamos el correo de la solicitud
                SendMail(request.Id);
            }
            catch (Exception)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.PaymentRequestErrorMessage, BackEndErrors.PaymentRequestErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }

            // Success
            return new JsonResult()
            {
                Data = new
                {
                    success = true,
                    message = BackEndResources.CashOutRequestSuccess,
                    url = Url.Action("Index", "PaymentRequest")
                }
            };
        }

        #endregion

        #region DataAccess

        /// <summary>
        /// Intenta la anulacion de la solicitud de pago
        /// </summary>
        /// <param name="idPaymentRequest">Id de la solicitud</param>
        /// <returns>Resultado de la operacion</returns>
        public JsonResult TryAnnulPaymentRequest(Guid idPaymentRequest)
        {
            // Variables
            PaymentRequest request;

            bool isAnnulled;

            // Obtenemos la solicitud
            request = PRBLL.GetEntity(idPaymentRequest);

            try
            {
                // Validamos que la solicitud exista
                if (request != null)
                {
                    // Cambiamos los estados segun el estado actual
                    switch (request.IdPaymentRequestStatus)
                    {
                        // Validamos si la solicitud ya esta anulada
                        case (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Annulled:
                            {
                                throw new PaymentRequestAnnulledException(BackEndErrors.PaymentRequestAnnulledExceptionMessage, BackEndErrors.PaymentRequestAnnulledExceptionCode);
                            }
                        // Validamos si la solicitud esta procesada
                        case (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.DeclaredReconciled:
                            {
                                throw new PaymentRequestProcessedException(BackEndErrors.PaymentRequestProcessedExceptionMessage, BackEndErrors.PaymentRequestProcessedExceptionCode);
                            }
                        // Cambiamos el estado a anulado por defecto (pendiente y conciliada)
                        default:
                            {
                                using (CPurchaseOrderBLL CPOBLL = new CPurchaseOrderBLL())
                                {
                                    // Anulamos cada declaracion asociada
                                    isAnnulled = CPOBLL.TryAnnulPurchaseOrder(request.IdCPurchaseOrder.Value, true);
                                }
                                // Verificamos el resultado de la operación
                                if (!isAnnulled)
                                {
                                    throw new PurchaseOrderAnnulmentException(BackEndErrors.PurchaseOrderAnnulmentExceptionMessage, BackEndErrors.PurchaseOrderAnnulmentExceptionCode);
                                }
                                break;
                            }
                    }
                }
            }
            catch (PaymentRequestProcessedException e)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(e.MessageException, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }
            catch (PaymentRequestAnnulledException e)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(e.MessageException, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }
            catch (PurchaseOrderAnnulmentException e)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(e.MessageException, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }
            catch (PurchaseOrderAnnulledException e)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(e.MessageException, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }
            catch (PurchaseOrderDeclarationException e)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(e.MessageException, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }
            catch (Exception)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.BackEndErrorMessage, BackEndErrors.BackEndErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }
            // Guardamos los cambios en la base de datos
            PRBLL.SaveChanges();
            // Success
            _baseSuccessResponse = new BaseSuccessResponse(BackEndResources.AnnulPaymentRequestSuccess);
            var result = new
            {
                success = _baseSuccessResponse.Success,
                message = _baseSuccessResponse.Message
            };
            return new JsonResult { Data = result };

        }

        /// <summary>
        /// Obtiene las solicitudes de pago del comercio actual
        /// </summary>
        /// <returns>Lista de solicitudes de pago</returns>
        public List<PaymentRequest> GetRequests()
        {
            // Inicializamos las variables
            List<InstaTransfer.DataAccess.PaymentRequest> requestList = new List<InstaTransfer.DataAccess.PaymentRequest>();
            // Obtenemos las solicitudes asociadas al comercio actual
            requestList = PRBLL.GetAllRecords(pr => pr.RifCommerce == MySession.Current.CommerceUser.RifCommerce).ToList();
            // Devolvemos la lista de solicitudes
            return requestList;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Envia el correo de la solicitud de pago
        /// </summary>
        /// <param name="idRequest">Id de la solicitud</param>
        /// <returns>Resultado de la operacion</returns>
        public JsonResult SendMail(Guid idRequest)
        {
            // Variables
            EmailHelper emailHelper = new EmailHelper();
            PaymentRequest request = PRBLL.GetEntity(idRequest);
            UBankAccount bankAccount;
            List<string> instructions;
            int i = 0;
            try
            {
                if(request.IdPaymentRequestStatus != (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Pending)
                {
                    throw new PaymentRequestProcessedException(BackEndErrors.PaymentRequestProcessedExceptionMessage, BackEndErrors.PaymentRequestProcessedExceptionCode);
                }

                if (request != null)
                {
                    // Obtenemos la cuenta
                    using (UBankAccountBLL UBABLL = new UBankAccountBLL())
                    {
                        bankAccount = UBABLL.GetBankAccount("J410066105", "0134");
                    }

                    // Creamos la lista ordenada de instrucciones
                    instructions = bankAccount.UBankInstructions.OrderBy(x => x.Order).Select(x => x.Description).ToList();

                    // Construimos el cuerpo del correo
                    var messageSB = new StringBuilder();
                    messageSB.AppendLine("<p><strong>" + cuser.Commerce.BusinessName.Trim() + "</strong> (" + cuser.Commerce.SocialReasonName.Trim() + ") le solicita que declare el siguiente pago:</p>");
                    messageSB.AppendLine("<h4>" + request.Description + "</h4>");
                    messageSB.AppendLine("<strong> Bs. " + request.Amount.ToString("N2") + "</strong><br /><br />");
                    messageSB.AppendLine("<h4>Instrucciones del pago</h4>");

                    foreach (var instrucction in instructions)
                    {
                        i++;
                        messageSB.AppendLine(i + ". " + instrucction + "<br />");
                    }
                    messageSB.AppendLine("<strong> Número de Cuenta " + bankAccount.AccountType.Trim() + ": </strong>" + bankAccount.AccountNumber + "<br />");
                    messageSB.AppendLine("<strong> Beneficiario: </strong>" + bankAccount.USocialReason.Name.Trim() + "<br />");
                    messageSB.AppendLine("<strong> Rif: </strong>" + bankAccount.USocialReason.Id + "<br />");
                    messageSB.AppendLine("<strong> Correo Electrónico: </strong>" + ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxInfoMail] + "<br /><br />");
                    messageSB.AppendLine("<h4>Una vez realizada su transferencia declare su pago a través del siguiente enlace:</p>");
                    messageSB.AppendLine("<a href='" + ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyPaymentRequestUrl] + request.Id + "' ><img src='" + ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxUrl].ToString() + "Content/images/btndeclarar.png' alt='Declarar en Transax' width='120' height='37'/></a>");

                    // Construimos el encabezado del correo
                    EmailModels.PaymentRequestEmailModel emailModel = new EmailModels.PaymentRequestEmailModel
                    {
                        From = ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxNoReplyMail],
                        To = new List<string> { request.RequestEmail },
                        Subject = string.Format("{0} ({1}) le ha solicitado un pago", cuser.Commerce.BusinessName.Trim(), cuser.Commerce.SocialReasonName.Trim()),
                        DisplayName = "Solicitud de Pagos Transax",
                        EndUserName = string.Format("{0} {1}", request.EndUser.Name.Trim(), request.EndUser.LastName.Trim()),
                        Body = messageSB.ToString()
                    };

                    // Agregamos los estilos al correo y renderizamos vista parcial
                    var message = RenderPartialViewToString(ControllerContext.Controller, "Pages", "SolicitudPagoEmail", emailModel);
                    // Enviamos el correo
                    emailHelper.SendEmailMessage(emailModel.From, emailModel.DisplayName, emailModel.To, emailModel.Subject, message);

                    // Actualizamos la Solicitud
                    request.TimesSent++;
                    request.ChangeDate = DateTime.Now;
                    // Guardamos los cambios
                    PRBLL.SaveChanges();
                }
            }
            catch (PaymentRequestProcessedException e)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(e.Message, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }
            catch (Exception)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.PaymentRequestSendErrorMessage, BackEndErrors.PaymentRequestSendErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }
            // Success
            return new JsonResult()
            {
                Data = new
                {
                    success = true,
                    message = BackEndResources.PaymentRequestSuccess,
                    url = Url.Action("Index", "PaymentRequest")
                }
            };

        }

        /// <summary>
        /// Renderiza una vista parcial como un string
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="controllerName"></param>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string RenderPartialViewToString(ControllerBase controller, string controllerName, string viewName, object model)
        {
            var oldModel = controller.ViewData.Model;
            controller.ViewData.Model = model;
            try
            {
                using (var sw = new StringWriter())
                {

                    controller.ControllerContext.RouteData.Values["controller"] = controllerName;

                    var viewResult = ViewEngines.Engines.FindView(controller.ControllerContext, viewName, null);

                    var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);

                    viewResult.View.Render(viewContext, sw);

                    controller.ViewData.Model = oldModel;
                    return sw.GetStringBuilder().ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}