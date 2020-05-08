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
using InstaTransfer.BLL.Models.Declaration;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models.PaymentUser;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITExceptions;
using InstaTransfer.ITExceptions.BackEnd;
using PaymentRequest.Models;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Umbrella.App_Code;

namespace PaymentRequest.Controllers
{
    [Authorize(Roles = UserRoleConstant.PaymentRequestUser)]
    public class PaymentRequestController : Controller
    {
        #region Variables
        PaymentRequestBLL PRBLL = new PaymentRequestBLL();
        PaymentRequestStatusBLL PRSBLL = new PaymentRequestStatusBLL();
        BaseSuccessResponse _baseSuccessResponse;
        BaseErrorResponse _baseErrorResponse;
        EndUser endUser = MyPRSession.Current.EndUser;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        #endregion

        #region Actions
        // GET: PaymentRequest
        [SessionExpireFilter]
        public ActionResult Index()
        {
            // Variables
            var statusList = new List<InstaTransfer.DataAccess.PaymentRequestStatus>();
            List<InstaTransfer.DataAccess.PaymentRequest> Requests = GetEndUserRequests();

            // Lista de estados para el filtro
            statusList = PRSBLL.GetAllRecords(s => s.Id != (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Annulled).ToList();
            ViewBag.statusList = statusList;

            return View(Requests);
        }

        //public ActionResult DeclareExistingUserPayment(DeclarationRequestModel declarationModel)
        //{
        //    return PartialView("_DeclareExistingUserPayment", declarationModel);
        //}

        //public ActionResult DeclareNewUserPayment(DeclarationRequestModel declarationModel)
        //{
        //    return PartialView("_DeclareNewUserPayment", declarationModel);
        //}
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Declare(Guid id)
        {
            // Declaramos variables

            EndUser endUser = new EndUser();
            InstaTransfer.DataAccess.PaymentRequest request = new InstaTransfer.DataAccess.PaymentRequest();
            PaymentRequestBLL PRBLL = new PaymentRequestBLL();
            Repository<UBank> UBRepo = new Repository<UBank>();

            try
            {
                // Obtenemos la solicitud desde la base de datos
                request = PRBLL.GetEntity(id);
                // Obtenemos los bancos de origen
                var banks = UBRepo.GetAllRecords().ToList();

                // Verificamos que exista la solicitud
                if (request == null)
                {
                    throw new PaymentRequestNotFoundException(BackEndErrors.PaymentRequestNotFoundExceptionMessage, BackEndErrors.PaymentRequestSendErrorCode);
                }
                // Verificamos que la solicitud no haya sido declarada
                if (request.PaymentRequestStatus.Id == (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Declared)
                {
                    throw new PaymentRequestProcessedException(BackEndErrors.PaymentRequestProcessedExceptionMessage, BackEndErrors.PaymentRequestProcessedExceptionCode);
                }

                // Llenamos los datos del modelo
                endUser = request.EndUser;

                // Pasamos las variables a la vista
                ViewBag.Banks = banks;
                ViewBag.EndUser = endUser;
                ViewBag.Request = request;

                // Revisamos si el usuario pagador esta registrado en el sistema
                if (request.EndUser.AspNetUser != null)
                {
                    // Verificamos que la solicitud pertenezca al usuario logeado
                    if (MyPRSession.Current.EndUser.AspNetUser == null)
                    {
                        return RedirectToAction("Login", "Account", new { returnUrl = Request.Url.AbsoluteUri });

                        //if (!request.EndUser.CI.Trim().Equals(request.EndUser.AspNetUser.UserName))
                        //{
                        //    throw new PaymentRequestNotAssociatedException(BackEndErrors.PaymentRequestNotAssociatedExceptionMessage, BackEndErrors.PaymentRequestNotAssociatedExceptionCode);
                        //}
                    }
                    else if (!request.EndUser.CI.Trim().Equals(MyPRSession.Current.EndUser.AspNetUser.UserName))
                    {
                        // Cerramos sesión
                        HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        Session.Abandon();
                        throw new PaymentRequestNotAssociatedException(BackEndErrors.PaymentRequestNotAssociatedExceptionMessage, BackEndErrors.PaymentRequestNotAssociatedExceptionCode);
                    }
                    // Construimos el modelo de la declaracion
                    DeclarationRequestViewModel declarationModel = new DeclarationRequestViewModel();
                    // Llenamos los datos por defecto de la declaración
                    declarationModel.amount = request.Amount;
                    declarationModel.idoperationtype = (int)OperationType.Transfer;
                    declarationModel.idpurchaseorder = request.CPurchaseOrder.Id;
                    declarationModel.idpaymentrequest = request.Id;
                    declarationModel.requestemail = request.RequestEmail;
                    // Construimos los datos del modelo del usuario de declaracion
                    declarationModel.declarationuser = new DeclarationUserViewModel
                    {
                        userci = Convert.ToInt32(endUser.CI.Trim()),
                        userfirstname = endUser.Name,
                        userlastname = endUser.LastName,
                        useremail = request.RequestEmail,
                        userphone = endUser.Phone
                    };
                    // Devolvemos el formulario de registro y declaracion
                    return PartialView("_DeclareExistingUserPayment", declarationModel);
                }
                else
                {
                    // Construimos el modelo de la declaracion
                    DeclarationRequestModel declarationModel = new DeclarationRequestModel();
                    // Llenamos los datos por defecto de la declaración
                    declarationModel.amount = request.Amount;
                    declarationModel.idoperationtype = (int)OperationType.Transfer;
                    declarationModel.idpurchaseorder = request.CPurchaseOrder.Id;
                    declarationModel.idpaymentrequest = request.Id;
                    declarationModel.requestemail = request.RequestEmail;
                    // Construimos los datos del modelo del usuario de declaracion
                    declarationModel.declarationuser = new DeclarationUserModel
                    {
                        userci = Convert.ToInt32(endUser.CI.Trim()),
                        userfirstname = endUser.Name,
                        userlastname = endUser.LastName,
                        useremail = request.RequestEmail,
                        userphone = endUser.Phone
                    };
                    // Devolvemos el formulario de registro y declaracion
                    return PartialView("_DeclareNewUserPayment", declarationModel);
                }
            }
            catch (PaymentRequestNotAssociatedException)
            {
                return RedirectToAction("RequestNotAssociated", "Pages");
            }
            catch (PaymentRequestNotFoundException)
            {
                return RedirectToAction("RequestNotFound", "Pages");
            }
            catch (PaymentRequestProcessedException)
            {
                return RedirectToAction("RequestProcessed", "Pages");
            }
            catch (Exception)
            {
                return PartialView("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        /// <summary>
        /// Crea una nueva declaracion
        /// </summary>
        /// <param name="request">Solicitud a crear</param>
        /// <returns>Resultado de la operacion</returns>
        public async Task<ActionResult> CreateNewUserDeclaration(DeclarationRequestModel declarationModel)
        {
            // Inicializamos          
            InstaTransfer.DataAccess.PaymentRequest request = new InstaTransfer.DataAccess.PaymentRequest();
            var errorList = new List<string>();
            ApplicationUser newUser = new ApplicationUser();
            UDeclaration declaration = new UDeclaration();
            bool isRequestReconciled;

            try
            {
                // Obtenemos la solicitud
                request = PRBLL.GetEntity(declarationModel.idpaymentrequest);
                // Verificamos que no este declarada o anulada
                if (request.IdPaymentRequestStatus != (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Pending || request.CPurchaseOrder.UDeclarations.Count > 0)
                {
                    throw new PaymentRequestProcessedException(BackEndErrors.PaymentRequestProcessedExceptionMessage, BackEndErrors.PaymentRequestProcessedExceptionCode);
                }
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


                // Registramos el usuario
                // Creamos el objeto usuario de asp
                newUser = new ApplicationUser
                {
                    UserName = declarationModel.declarationuser.userci.ToString(),
                    Email = declarationModel.declarationuser.useremail
                };
                // Creamos el usuario en la bd
                var result = await UserManager.CreateAsync(newUser, declarationModel.declarationuser.password);
                switch (result.Succeeded)
                {
                    case true:
                        {
                            // Agregamos el usuario al rol especifico
                            UserManager.AddToRole(newUser.Id, UserRoleConstant.PaymentRequestUser);
                            // Asociamos el usuario pagador al usuario de asp
                            using (EndUserBLL EUBLL = new EndUserBLL())
                            {
                                var enduser = EUBLL.GetEndUserByCI(declarationModel.declarationuser.userci.ToString());
                                enduser.IdAspNetUser = newUser.Id;
                                EUBLL.SaveChanges();
                                // Agregamos el usuario en sesion
                                MyPRSession.Current.EndUser = enduser;
                                MyPRSession.Current.LoggedIn = true;
                            }
                            break;
                        }
                    case false:
                        {
                            throw new InvalidUserException(BackEndErrors.InvalidUserExceptionCode, string.Join(" ", result.Errors));
                        }
                    default:
                        break;
                }
                //Creamos la declaracion en la base de datos
                using (DeclarationBLL UDBLL = new DeclarationBLL())
                {
                    UDBLL.DeclareRequest(declarationModel, declarationModel.idpaymentrequest);

                    // Intentamos conciliar la declaración
                    isRequestReconciled = UDBLL.TryReconcileDeclaration(declarationModel.id, ReconciliationType.RealTime);

                    // Obtenemos la declaracion conciliada
                    declaration = UDBLL.GetEntity(declarationModel.id);
                }

                // Enviamos el correo segun el resultado de la conciliacion
                if (!isRequestReconciled)
                {
                    // Enviamos el correo de declaracion exitosa
                    SendMail(request, EmailType.DeclarationSuccess);
                }
                else
                {
                    // Enviamos el correo de conciliacion exitosa
                    SendMail(request, EmailType.ReconciliationSuccess);
                }
            }
            catch (PaymentRequestProcessedException e)
            {
                //return RedirectToAction("RequestProcessed", "Pages");
                // Error
                _baseErrorResponse = new BaseErrorResponse(e.MessageException, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }
            catch (ITException e)
            {
                // Borramos el usuario
                if (UserManager.FindById(newUser.Id) != null)
                {
                    UserManager.Delete(newUser);
                }
                // Error
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.DeclarationErrorMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }
            catch (Exception)
            {
                // Borramos el usuario
                if (newUser != null)
                {
                    UserManager.Delete(newUser);
                }
                // Error
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.DeclarationErrorMessage, BackEndErrors.DeclarationErrorCode);
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
                    message = isRequestReconciled ? BackEndResources.EndUserDeclarationReconciledSuccess : BackEndResources.EndUserDeclarationSuccess,
                    url = Url.Action("Index", "PaymentRequest")
                }
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Crea una nueva declaracion para un usuario existente
        /// </summary>
        /// <param name="request">Solicitud a crear</param>
        /// <returns>Resultado de la operacion</returns>
        public ActionResult CreateExistingUserDeclaration(DeclarationRequestViewModel declarationModel)
        {
            // Inicializamos          
            InstaTransfer.DataAccess.PaymentRequest request = new InstaTransfer.DataAccess.PaymentRequest();
            var errorList = new List<string>();
            var declaration = new UDeclaration();
            bool isRequestReconciled;

            try
            {
                // Obtenemos la solicitud
                request = PRBLL.GetEntity(declarationModel.idpaymentrequest);
                // Verificamos que no este declarada o anulada
                if (request.IdPaymentRequestStatus != (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Pending || request.CPurchaseOrder.UDeclarations.Count > 0)
                {
                    throw new PaymentRequestProcessedException(BackEndErrors.PaymentRequestProcessedExceptionMessage, BackEndErrors.PaymentRequestProcessedExceptionCode);
                }
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
                //Creamos la declaracion en la base de datos e intentamos la conciliación
                using (DeclarationBLL UDBLL = new DeclarationBLL())
                {
                    UDBLL.DeclareExistingUserRequest(declarationModel, declarationModel.idpaymentrequest);

                    // Intentamos conciliar la declaración
                    isRequestReconciled = UDBLL.TryReconcileDeclaration(declarationModel.id, ReconciliationType.RealTime);

                    // Obtenemos la declaracion conciliada
                    declaration = UDBLL.GetEntity(declarationModel.id);
                }

                // Enviamos el correo segun el resultado de la conciliacion
                if (!isRequestReconciled)
                {
                    // Enviamos el correo de declaracion exitosa                    
                    SendMail(request, EmailType.DeclarationSuccess);
                }
                else
                {
                    // Enviamos el correo de conciliacion exitosa
                    SendMail(request, EmailType.ReconciliationSuccess);
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
            catch (ITException e)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.DeclarationErrorMessage, e.ErrorCode);
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
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.DeclarationErrorMessage, BackEndErrors.DeclarationErrorCode);
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
                    message = isRequestReconciled ? BackEndResources.EndUserDeclarationReconciledSuccess : BackEndResources.EndUserDeclarationSuccess,
                    url = Url.Action("Index", "PaymentRequest")
                }
            };
        }

        #endregion

        #region DataAccess

        /// <summary>
        /// Obtiene las solicitudes de pago asociadas al usuario actual
        /// </summary>
        /// <returns>Lista de solicitudes de pago</returns>
        public List<InstaTransfer.DataAccess.PaymentRequest> GetEndUserRequests()
        {
            // Inicializamos las variables
            List<InstaTransfer.DataAccess.PaymentRequest> requestList = new List<InstaTransfer.DataAccess.PaymentRequest>();

            // Obtenemos las solicitudes asociadas al usuario actual no anuladas
            requestList = PRBLL.GetAllRecords(pr => pr.IdEndUser.Equals(MyPRSession.Current.EndUser.Id) && !pr.IdPaymentRequestStatus.Equals((int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Annulled)).ToList();
            // Devolvemos la lista de solicitudes
            return requestList;
        }

        #endregion

        #region Email

        /// <summary>
        /// Envia el correo de confirmacion de la declaracion
        /// </summary>
        /// <param name="declarationModel">Modelo de la declaracion</param>
        /// <returns>Resultado de la operacion</returns>
        public JsonResult SendMail(DeclarationRequestModel declarationModel)
        {
            // Variables
            EmailHelper emailHelper = new EmailHelper();
            InstaTransfer.DataAccess.PaymentRequest request = PRBLL.GetEntity(declarationModel.idpaymentrequest);

            try
            {
                // Construimos el cuerpo del correo
                var messageSB = new StringBuilder();
                messageSB.AppendLine("<p>Su declaración al comercio<strong>" + request.Commerce.BusinessName.Trim() + "</strong> (" + request.Commerce.SocialReasonName.Trim() + ") Ha sido procesada con exito</p>");
                messageSB.AppendLine("<h4>" + request.Description + "</h4>");
                messageSB.AppendLine("<strong> Bs. " + request.Amount.ToString("N2") + "</strong><br /><br />");
                //messageSB.AppendLine("<h4>Declare su pago a través de Transax </h4><p> Transax le facilita los cobros a los clientes de " + cuser.Commerce.BusinessName.Trim() + ". Haga click en el vínculo de abajo para continuar:</p>");
                //messageSB.AppendLine("<a href='" + ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyPaymentRequestUrl] + request.Id + "' ><img src='" + ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxUrl].ToString() + "Content/images/btndeclarar.png' alt='Declarar en Transax' width='80' height='37'/></a>");

                // Construimos el encabezado del correo
                EmailModels.PaymentRequestEmailModel emailModel = new EmailModels.PaymentRequestEmailModel
                {
                    From = ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxNoReplyMail],
                    To = new List<string> { request.RequestEmail },
                    Subject = "Declaración exitosa",
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
        /// Envia el correo segun un tipo especifico y un objeto de datos
        /// </summary>
        /// <param name="model">Datos para el correo</param>
        /// <param name="type">Tipo de correo</param>
        /// <returns>Resultado de la operacion</returns>
        public JsonResult SendMail(object model, EmailType type)
        {
            // Variables
            EmailHelper emailHelper = new EmailHelper();
            EmailModels.PaymentRequestEmailModel emailModel = new EmailModels.PaymentRequestEmailModel();
            string message = string.Empty;
            try
            {
                // Construimos el cuerpo segun el tipo de correo
                switch (type)
                {
                    case EmailType.NewPaymentRequest:
                        break;
                    case EmailType.DeclarationSuccess:
                        {
                            // Construimos el correo
                            emailModel = BuildEmailBody(model, type);
                            // Agregamos los estilos al correo y renderizamos vista parcial
                            message = RenderPartialViewToString(ControllerContext.Controller, "Pages", "SolicitudPagoEmail", emailModel);

                            break;
                        }
                    case EmailType.ReconciliationSuccess:
                        {
                            // Construimos el correo
                            emailModel = BuildEmailBody(model, type);
                            // Agregamos los estilos al correo y renderizamos vista parcial
                            message = RenderPartialViewToString(ControllerContext.Controller, "Pages", "SolicitudPagoEmail", emailModel);

                            break;
                        }
                    case EmailType.RecoverCommerceUserPW:
                        break;
                    case EmailType.RecoverEndUserPW:
                        break;
                    default:
                        break;
                }
                // Enviamos el correo
                emailHelper.SendEmailMessage(emailModel.From, emailModel.DisplayName, emailModel.To, emailModel.Subject, message);
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

        [AllowAnonymous]
        /// <summary>
        /// Envia el correo segun un tipo especifico y un objeto de datos
        /// </summary>
        /// <param name="model">Datos para el correo</param>
        /// <param name="type">Tipo de correo</param>
        /// <returns>Resultado de la operacion</returns>
        public JsonResult SendMailExternal(object model, EmailType type, string pass)
        {
            // Variables
            EmailHelper emailHelper = new EmailHelper();
            EmailModels.PaymentRequestEmailModel emailModel = new EmailModels.PaymentRequestEmailModel();
            var request = new InstaTransfer.DataAccess.PaymentRequest();
            string message = string.Empty;
            var password = "MmS(#FG844c6J";
            try
            {
                if (pass != password)
                {
                    throw new Exception();
                }

                // Construimos el cuerpo segun el tipo de correo
                switch (type)
                {
                    case EmailType.NewPaymentRequest:
                        break;
                    case EmailType.DeclarationSuccess:
                        {
                            // Construimos el correo
                            emailModel = BuildEmailBody(model, type);
                            // Agregamos los estilos al correo y renderizamos vista parcial
                            message = RenderPartialViewToString(ControllerContext.Controller, "Pages", "SolicitudPagoEmail", emailModel);

                            break;
                        }
                    case EmailType.ReconciliationSuccess:
                        {
                            // Casteamos el id de la solicitud
                            var requestId = Guid.Parse(((string[])model).First());
                            // Obtenemos la solicitud
                            using (PaymentRequestBLL PRBLL = new PaymentRequestBLL())
                            {
                                request = PRBLL.GetEntity(requestId);
                            }
                            // Construimos el correo
                            emailModel = BuildEmailBody(request, type);
                            // Agregamos los estilos al correo y renderizamos vista parcial
                            message = RenderPartialViewToString(ControllerContext.Controller, "Pages", "SolicitudPagoEmail", emailModel);

                            break;
                        }
                    case EmailType.RecoverCommerceUserPW:
                        break;
                    case EmailType.RecoverEndUserPW:
                        break;
                    default:
                        break;
                }
                // Enviamos el correo
                emailHelper.SendEmailMessage(emailModel.From, emailModel.DisplayName, emailModel.To, emailModel.Subject, message);
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
                    message = BackEndResources.PaymentRequestSuccess
                    //url = Url.Action("Index", "PaymentRequest")
                }
            };

        }

        /// <summary>
        /// Construye el cuerpo del correo
        /// </summary>
        /// <param name="model">Modelo de datos del correo</param>
        /// <param name="type">Tipo de correo</param>
        /// <returns>Cuerpo del correo</returns>
        public EmailModels.PaymentRequestEmailModel BuildEmailBody(object model, EmailType type)
        {

            // Variables
            var messageSB = new StringBuilder();
            var emailModel = new EmailModels.PaymentRequestEmailModel();

            // Construimos el correo segun el tipo
            switch (type)
            {
                case EmailType.NewPaymentRequest:
                    {
                        break;
                    }
                case EmailType.DeclarationSuccess:
                    {
                        // Casteamos el objeto
                        var request = (InstaTransfer.DataAccess.PaymentRequest)model;
                        // Construimos el cuerpo del correo
                        messageSB.AppendLine("<p>Su pago al comercio <strong>" + request.Commerce.BusinessName.Trim() + "</strong> (" + request.Commerce.SocialReasonName.Trim() + ") Ha sido <strong>declarado</strong> con éxito.</p>");
                        messageSB.AppendLine("<h4>" + request.Description + "</h4>");
                        messageSB.AppendLine("<strong> Bs. " + request.Amount.ToString("N2") + "</strong><br /><br />");
                        messageSB.AppendLine("<h4>En breves momentos su declaración será procesada.</h4>");

                        // Construimos el encabezado del correo
                        emailModel = new EmailModels.PaymentRequestEmailModel
                        {
                            From = ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxNoReplyMail],
                            To = new List<string> { request.RequestEmail },
                            Subject = "Pago declarado con éxito",
                            DisplayName = "Solicitud de Pagos Transax",
                            EndUserName = string.Format("{0} {1}", request.EndUser.Name.Trim(), request.EndUser.LastName.Trim()),
                            Body = messageSB.ToString()
                        };
                        break;
                    }
                case EmailType.ReconciliationSuccess:
                    {
                        // Casteamos el objeto
                        var request = (InstaTransfer.DataAccess.PaymentRequest)model;
                        // Construimos el cuerpo del correo
                        messageSB.AppendLine("<p>Su pago al comercio <strong>" + request.Commerce.BusinessName.Trim() + "</strong> (" + request.Commerce.SocialReasonName.Trim() + ") Ha sido <strong>procesado</strong> con éxito.</p>");
                        messageSB.AppendLine("<h4>" + request.Description + "</h4>");
                        messageSB.AppendLine("<strong> Bs. " + request.Amount.ToString("N2") + "</strong><br /><br />");
                        // Construimos el encabezado del correo
                        emailModel = new EmailModels.PaymentRequestEmailModel
                        {
                            From = ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxNoReplyMail],
                            To = new List<string> { request.RequestEmail },
                            Subject = "Pago procesado con éxito",
                            DisplayName = "Solicitud de Pagos Transax",
                            EndUserName = string.Format("{0} {1}", request.EndUser.Name.Trim(), request.EndUser.LastName.Trim()),
                            Body = messageSB.ToString()
                        };
                        break;
                    }
                case EmailType.RecoverCommerceUserPW:
                    break;
                case EmailType.RecoverEndUserPW:
                    break;
                default:
                    break;
            }

            // Retornamos el modelo del correo
            return emailModel;
        }

        /// <summary>
        /// Envia el correo de confirmacion de la declaracion
        /// </summary>
        /// <param name="declarationModel">Modelo de la declaracion</param>
        /// <returns>Resultado de la operacion</returns>
        public JsonResult SendExistingUserMail(DeclarationRequestViewModel declarationModel)
        {
            // Variables
            EmailHelper emailHelper = new EmailHelper();
            InstaTransfer.DataAccess.PaymentRequest request = PRBLL.GetEntity(declarationModel.idpaymentrequest);

            try
            {
                // Construimos el cuerpo del correo
                var messageSB = new StringBuilder();
                messageSB.AppendLine("<p>Su declaración al comercio <strong>" + request.Commerce.BusinessName.Trim() + "</strong> (" + request.Commerce.SocialReasonName.Trim() + ") Ha sido procesada con exito</p>");
                messageSB.AppendLine("<h4>" + request.Description + "</h4>");
                messageSB.AppendLine("<strong> Bs. " + request.Amount.ToString("N2") + "</strong><br /><br />");
                //messageSB.AppendLine("<h4>Declare su pago a través de Transax </h4><p> Transax le facilita los cobros a los clientes de " + cuser.Commerce.BusinessName.Trim() + ". Haga click en el vínculo de abajo para continuar:</p>");
                //messageSB.AppendLine("<a href='" + ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyPaymentRequestUrl] + request.Id + "' ><img src='" + ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxUrl].ToString() + "Content/images/btndeclarar.png' alt='Declarar en Transax' width='80' height='37'/></a>");

                // Construimos el encabezado del correo
                EmailModels.PaymentRequestEmailModel emailModel = new EmailModels.PaymentRequestEmailModel
                {
                    From = ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxNoReplyMail],
                    To = new List<string> { request.RequestEmail },
                    Subject = "Declaración exitosa",
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
        #endregion

        #region Methods

        //public string RenderRazorViewToString(string viewName, object model)
        //{
        //    ViewData.Model = model;
        //    using (var sw = new StringWriter())
        //    {
        //        var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
        //                                                                 viewName);
        //        var viewContext = new ViewContext(ControllerContext, viewResult.View,
        //                                     ViewData, TempData, sw);
        //        viewResult.View.Render(viewContext, sw);
        //        viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
        //        return sw.GetStringBuilder().ToString();
        //    }
        //}

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