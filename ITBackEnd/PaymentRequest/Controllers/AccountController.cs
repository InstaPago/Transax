using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PaymentRequest.Models;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.BLL.Concrete;
using System;
using PaymentRequest.App_Code;
using System.Net;
using System.IO;
using Rotativa;
using Rotativa.Options;
using System.Collections.Generic;
using System.Text;
using static InstaTransfer.ITLogic.Helpers.SessionHelper;
using BLL.Concrete;
using InstaTransfer.BLL.Models.PaymentUser;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITLogic.Helpers;
using System.Configuration;
using InstaTransfer.BLL.Models.Email;

namespace PaymentRequest.Controllers
{
    //[Authorize]
    public class AccountController : Controller
    {
        #region Variables
        URepository<AE_MovimientosCuenta> AEmovimientosREPO = new URepository<AE_MovimientosCuenta>();
        URepository<AE_Propuesta> AE_PropuestaREPO = new URepository<InstaTransfer.DataAccess.AE_Propuesta>();
        URepository<AE_Avance> AE_AvanceREPO = new URepository<InstaTransfer.DataAccess.AE_Avance>();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        EndUserBLL EUBLL = new EndUserBLL();
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

        #region Constructor
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }
        #endregion

        #region MVC
        //
        // GET: /Account/Login
        [AllowAnonymousOnly]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymousOnly]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            // Variables
            var endUser = new EndUser();

            string redirectUrl = string.Empty;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var _user = UserManager.Find(model.Cedula, model.Password);
            try
            {
                // Verificamos que el usuario ingresado tenga un usuario de solicitud de pago
                if (EUBLL.GetEndUser(_user.Id) == null)
                {
                    ModelState.AddModelError("", "Usuario inválido.");
                    // Error
                    return new JsonResult()
                    {
                        Data = new
                        {
                            success = false,
                            message = BackEndErrors.InvalidUserExceptionMessage,
                            url = Url.Action("Login", "Account")
                        }
                    };
                    //return View(model);
                }
                if (!UserManager.IsInRole((_user != null ? _user.Id : string.Empty), UserRoleConstant.PaymentRequestUser))
                {
                    ModelState.AddModelError("", "Usuario inválido.");
                    // Error
                    return new JsonResult()
                    {
                        Data = new
                        {
                            success = false,
                            message = BackEndErrors.InvalidUserExceptionMessage,
                            url = Url.Action("Login", "Account")
                        }
                    };
                    //return View(model);
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Usuario inválido.");
                // Error
                return new JsonResult()
                {
                    Data = new
                    {
                        success = false,
                        message = BackEndErrors.InvalidUserExceptionMessage,
                        url = Url.Action("Login", "Account")
                    }
                };
                //return View(model);
            }


            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Cedula, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        // Obtenemos el usuario del comercio a partir del usuario logeado
                        var _endUser = EUBLL.GetEndUser(_user.Id);
                        // Guardar el usuario en sesion
                        MyPRSession.Current.EndUser = _endUser;
                        MyPRSession.Current.LoggedIn = true;

                        //return RedirectToLocal(returnUrl);
                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            redirectUrl = Url.Action("Index", "PaymentRequest");
                        }
                        else
                        {
                            redirectUrl = returnUrl;
                        }
                        // Success
                        return new JsonResult()
                        {
                            Data = new
                            {
                                success = true,
                                message = BackEndResources.DeclarationSuccessMessage,
                                url = redirectUrl
                            }
                        };
                    }

                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Usuario inválido.");
                    // Error
                    return new JsonResult()
                    {
                        Data = new
                        {
                            success = false,
                            message = BackEndErrors.InvalidUserExceptionMessage,
                            url = Url.Action("Login", "Account")
                        }
                    };
                    //return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymousOnly]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

        //            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //            // Send an email with this link
        //            // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //            // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //            // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

        //            return RedirectToAction("Index", "Home");
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            // Variables
            EmailHelper emailHelper = new EmailHelper();

            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Cedula);
                if (user == null /*|| !(await UserManager.IsEmailConfirmedAsync(user.Id))*/)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                //For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                //Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                // Construimos el cuerpo del correo
                var messageSB = new StringBuilder();
                messageSB.AppendLine("<p>Ingrese al siguiente enlace para restablecer su contraseña:</p>");
                messageSB.AppendLine("<a href='" + callbackUrl + "' ><img src='" + ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyPaymentRequestUrl].ToString() + "Content/images/btnrestablecer.png' alt='Restablecer Contraseña' width='120' height='37'/></a>");

                // Construimos el encabezado del correo
                EmailModels.PaymentRequestEmailModel emailModel = new EmailModels.PaymentRequestEmailModel
                {
                    From = ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxNoReplyMail],
                    To = new List<string> { user.Email },
                    Subject = "Restablezca su contraseña",
                    DisplayName = "Solicitud de Pagos Transax",
                    Body = messageSB.ToString()
                };

                // Agregamos los estilos al correo y renderizamos vista parcial
                var message = RenderPartialViewToString(ControllerContext.Controller, "Pages", "SolicitudPagoEmail", emailModel);
                // Enviamos el correo
                emailHelper.SendEmailMessage(emailModel.From, emailModel.DisplayName, emailModel.To, emailModel.Subject, message);



                //await UserManager.SendEmailAsync(user.Id, "Restablecer Contraseña", "Por favor restablezca su contraseña haciendo <a href=\"" + callbackUrl + "\">click aqui</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Cedula);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        // POST: /Account/LogOff
        ///Mientras probamos
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            //FormsAuthentication.SignOut();
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }
        #endregion



        #region PaymentRequest

        [HttpPost]
        [AllowAnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<List<string>> Register(DeclarationUserModel model)
        {
            var EUBLL = new EndUserBLL();
            var errorList = new List<string>();

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.userci.ToString(),
                    Email = model.useremail
                };

                // Creamos el usuario con el password establecido
                var result = await UserManager.CreateAsync(user, model.password);
                // Verificamos el resultado de la operacion
                if (result.Succeeded)
                {
                    try
                    {
                        // Asignamos el usuario al rol de administrador por defecto
                        await UserManager.AddToRoleAsync(user.Id, UserRoleConstant.CommerceAdmin);
                        // Inicia sesion con el usuario creado
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "Confirma tu cuenta", "Por favor confirme su cuenta haciendo <a href=\"" + callbackUrl + "\">click aqui</a>");
                        // Asociamos al usuario final
                        EUBLL.Register(model.userci.ToString(), user.Id);
                        // Redireccionamos a la pagina deseada
                        return errorList;
                    }
                    catch
                    {
                        UserManager.Delete(user);
                        AddErrors(new IdentityResult("Ha ocurrido un error por favor contacte al Adminsitrador del sistema. Error 002PR"));

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
                        return errorList;
                    }
                }
                AddErrors(result);
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
            }
            return errorList;
            // If we got this far, something failed, redisplay form

            //return View(model);
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Verifica el estado de la sesion
        /// </summary>
        /// <returns>El estado de la sesion del usuario</returns>
        public JsonResult CheckSession()
        {
            var isLoggedIn = MyPRSession.Current.LoggedIn;
            return new JsonResult
            {
                Data = isLoggedIn,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
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

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return View("Index", "PaymentRequest");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        public ActionResult _Propuesta(int id)
        {


            AE_Propuesta _Propuesta = AE_PropuestaREPO.GetAllRecords().Where(u => u.Id == id).FirstOrDefault();
            List<AE_Avance> avance = AE_AvanceREPO.GetAllRecords(u => u.RifCommerce == _Propuesta.RifCommerce && u.IdEstatus == 1).OrderByDescending(u => u.FechaCreacion).ToList();
            if (avance.ToList().Count > 0)
            {
                ViewBag.Avance = true;
                ViewBag.Idavance = avance.FirstOrDefault().Id;
            }
            else
            {
                ViewBag.Avance = false;
            }

            return View("_Propuesta", _Propuesta);
        }

        public JsonResult _EnviarPropuesta(string IdPropuesta, string _rif)
        {

            try
            {
                InstaTransfer.DataAccess.AE_Propuesta _Propuesta = AE_PropuestaREPO.GetAllRecords().Where(u => u.Id == int.Parse(IdPropuesta)).FirstOrDefault();
                /// PREPARAR EL PDF
                var actionPDF = new Rotativa.ActionAsPdf("_Propuesta", new { id = IdPropuesta })
                {
                    FileName = "Propuesta" + IdPropuesta + ".pdf",
                    PageSize = Size.Letter,
                    PageOrientation = Orientation.Landscape,
                    PageMargins = { Left = 5, Right = 5, Top = 5, Bottom = 5 },
                    PageWidth = 216,
                    PageHeight = 279,
                    CustomSwitches = "--disable-smart-shrinking"
                };
                byte[] applicationPDFData = actionPDF.BuildPdf(ControllerContext);

                //InstaTransfer.ITLogic.Helpers.EmailAttachment MyHelpers.Files.HelperFTP __ftpFiles = new MyHelpers.Files.HelperFTP();

                InstaTransfer.ITLogic.Helpers.EmailAttachment __attach = new InstaTransfer.ITLogic.Helpers.EmailAttachment()
                {
                    FileName = "Propuesta" + IdPropuesta + ".pdf",
                    FileStream = new MemoryStream(applicationPDFData),
                    MimeType = "application/pdf"
                };

                List<InstaTransfer.ITLogic.Helpers.EmailAttachment> __listAttachs = new List<InstaTransfer.ITLogic.Helpers.EmailAttachment>();
                __listAttachs.Add(__attach);

                //ARMAR EL CORREO ELECTRONICO
                //MyHelpers.Email.Email __mailrepo = new MyHelpers.Email.Email();
                InstaTransfer.ITLogic.Helpers.EmailHelper __mailrepo = new InstaTransfer.ITLogic.Helpers.EmailHelper();

                __mailrepo.SendEmailMessageWithAttach(
                                    "atc@transax.com",
                                    //new List<String>() { _Propuesta.Commerce.CUsers.Where(u => u.IsContact).FirstOrDefault().AspNetUser.Email },
                                    new List<string>() { "clarez@legendsoft.com.ve", _Propuesta.Commerce.CUsers.Where(u => u.IsContact).FirstOrDefault().AspNetUser.Email },
                                    "Propuesta avance de efectivo",
                                    "Adjunto encontrara la propuesta.",
                                    __listAttachs
                            );
                return Json(new
                {
                    Success = true,
                    Message = "Propuesta Enviada exitosamente",
                    Redirect = ""
                }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Ha ocurrido un error por favor contacte al administrador: " + ex.Message,
                    Redirect = ""
                }, JsonRequestBehavior.DenyGet);
            }
        }


        public ActionAsPdf _printepropuesta(int _id)
        {

            return new ActionAsPdf("_propuesta", new { id = _id })
            {
                FileName = "propuesta" + _id + ".pdf",
                PageSize = Size.Letter,
                PageOrientation = Orientation.Landscape,
                PageMargins = { Left = 5, Right = 5, Top = 5, Bottom = 5 },
                PageWidth = 216,
                PageHeight = 279,
                CustomSwitches = "--disable-smart-shrinking"
            };
        }

        public bool RevisarSolicitudAPI()
        {
            try
            {
                string Key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJjbGFyZXpAbGVnZW5kc29mdC5jb20udmUiLCJyb2xlIjoiU2NyYXBlckFwaVVzZXIiLCJuYmYiOjE1MDI4MjM3MzcsImV4cCI6MTUwMjgyNTUzNywiaWF0IjoxNTAyODIzNzM3LCJpc3MiOiJodHRwOi8vbXkudG9rZW5pc3N1ZXIuY29tIiwiYXVkIjoiaHR0cDovL215LndlYnNpdGUuY29tIn0.sWDIef7Yu9lA7k4wUd9E0Xg9eKr0LFpEtj9LXwzXc-Q";
                StringBuilder MyString = new StringBuilder("");
                MyString.Append("id=" + "8ff445c0-6ea4-4b9c-a4ae-274d3148f6a6");
                string query = MyString.ToString();
                string url = "http://scrapper.transax.tech/bankstatement/get";
                byte[] queryStream = Encoding.UTF8.GetBytes(query);
                //Llamo al API con el url que contiene los parámetros
                WebRequest req = WebRequest.Create(url) as HttpWebRequest;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = queryStream.Length;
                req.Headers.Add(HttpRequestHeader.Authorization, Key);
                Stream reqStream = req.GetRequestStream();
                reqStream.Write(queryStream, 0, queryStream.Length);
                reqStream.Close();
                //response
                HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                Stream dataStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                GetModelResponse _createmodel = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetModelResponse)) as GetModelResponse;

                if (_createmodel.idstatus == 1)
                {
                    //item.FechaUltimoRequest = DateTime.Now;
                }
                else if (_createmodel.idstatus == 2)
                {
                    //H4sIAAAAAAAEAHNLTc5IVAAB66DUtNSi1LzkzEQFa5fU4uSizILkzMOb8xTIAta++Xkl+dilghNzUrBLgaV5uQBLySuWlgAAAA==
                    var binaryData = Convert.FromBase64String(_createmodel.file.ToString());
                    byte[] finalbytes = InstaTransfer.ITLogic.Helpers.ApiHelper.Decompress(binaryData);
                    System.IO.File.WriteAllBytes(@"C:\Users\carmelo\Desktop\prueba.xls", finalbytes);

                }
                //string[] lines = { _loginmodel.token.ToString() };
                //string ruta = @"C:\Users\carmelo\Desktop\Token.txt";
                //System.IO.File.WriteAllLines(ruta, lines);
                return true;
            }
            catch
            {
                return false;
            }

        }
        public class GetModelResponse
        {
            public bool success { get; set; }
            public List<string> message { get; set; }
            public int idstatus { get; set; }
            public string status { get; set; }
            public object file { get; set; }
        }


        #endregion
    }
}