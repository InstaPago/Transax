using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Umbrella.Models;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.BLL.Concrete;
using System.Data.Linq;
using System;
using Umbrella.App_Code;
using System.Net;
using System.IO;
using reCAPTCHA.MVC;
using Rotativa;
using Rotativa.Options;
using System.Collections.Generic;
using System.Text;
using static InstaTransfer.ITLogic.Helpers.SessionHelper;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.BLL.Models.Email;
using System.Configuration;
using InstaTransfer.ITLogic.Helpers;
using Microsoft.Office.Interop.Excel;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Umbrella.Controllers
{
    //[Authorize]
    public class AccountController : Controller
    {
        #region Variables
        URepository<AE_MovimientosCuenta> AEmovimientosREPO = new URepository<AE_MovimientosCuenta>();
        URepository<AE_Propuesta> AE_PropuestaREPO = new URepository<InstaTransfer.DataAccess.AE_Propuesta>();
        URepository<AE_Avance> AE_AvanceREPO = new URepository<InstaTransfer.DataAccess.AE_Avance>();
        URepository<AE_Dolar> AE_DolarREPO = new URepository<AE_Dolar>();
        URepository<CP_Beneficiario> CP_Beneficiario = new URepository<InstaTransfer.DataAccess.CP_Beneficiario>();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        URepository<CUser> CURepo = new URepository<CUser>();
        URepository<CP_Archivo> ArchivoREPO = new URepository<CP_Archivo>();
        URepository<CP_ArchivoEstadoCuenta> EstadoCuentaREPO = new URepository<CP_ArchivoEstadoCuenta>();
        URepository<CP_Archivo> CP_ArchivoREPO = new URepository<CP_Archivo>();
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var _user = UserManager.Find(model.Email, model.Password);
            try
            {
                if (UserManager.IsInRole((_user != null ? _user.Id : string.Empty), UserRoleConstant.CommerceApiUser))
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
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        // Obtenemos el usuario del comercio a partir del usuario logeado
                        var _commerceUser = CURepo.GetCUser(_user.Id);
                        // Guardamos la hora de inicio de sesion del usuario
                        _commerceUser.LastLoginDate = DateTime.Now;
                        // Guardamos los cambios en la base de datos
                        CURepo.SaveChanges();
                        // Guardar el usuario en sesion
                        MySession.Current.CommerceUser = _commerceUser;
                        MySession.Current.LoggedIn = true;
                        Session.Add("_rif", _commerceUser.RifCommerce);
                        // Success
                        var lista = _commerceUser.AspNetUser.AspNetUserRoles.ToList();
                        var isCommerceUser = lista.Where(u => u.RoleId == "1").Count();
                        var isCommerceUserAdmin = lista.Where(u => u.RoleId == "2").Count();
                        var isCommerceNativa = lista.Where(u => u.RoleId == "10").Count();
                        if (isCommerceUser > 0 || isCommerceUserAdmin > 0)
                        {
                            return new JsonResult()
                            {
                                Data = new
                                {
                                    success = true,
                                    message = BackEndResources.LoginSuccess,
                                    url = Url.Action("List", "Avance")
                                }
                            };
                        }
                        else if (isCommerceNativa > 0)
                        {
                            return new JsonResult()
                            {
                                Data = new
                                {
                                    success = true,
                                    message = BackEndResources.LoginSuccess,
                                    url = Url.Action("ListAvance", "Nativa")
                                }
                            };
                        }
                        else
                        {
                            return new JsonResult()
                            {
                                Data = new
                                {
                                    success = true,
                                    message = BackEndResources.LoginSuccess,
                                    url = Url.Action("DashboardFondo", "Home")
                                }
                            };
                        }
                        //return RedirectToLocal(returnUrl);
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
                var user = await UserManager.FindByNameAsync(model.Email);
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
                messageSB.AppendLine("<a href='" + callbackUrl + "' ><img src='" + ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxUrl].ToString() + "Content/images/btnrestablecer.png' alt='Restablecer Contraseña' width='120' height='37'/></a>");

                // Construimos el encabezado del correo
                EmailModels.PaymentRequestEmailModel emailModel = new EmailModels.PaymentRequestEmailModel
                {
                    From = ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyTransaxNoReplyMail],
                    To = new List<string> { model.Email },
                    Subject = "Restablezca su contraseña",
                    DisplayName = "Solicitud de Pagos Transax",
                    Body = messageSB.ToString()
                };

                // Agregamos los estilos al correo y renderizamos vista parcial
                var message = RenderPartialViewToString(ControllerContext.Controller, "Pages", "SolicitudPagoEmail", emailModel);
                // Enviamos el correo
                emailHelper.SendEmailMessage(emailModel.From, emailModel.DisplayName, emailModel.To, emailModel.Subject, message);


                //await UserManager.SendEmailAsync(user.Id, "Restablecer Contraseña", "Por favor restablezca su contraseña haciendo <a href=\"" + callbackUrl + "\">click aqui</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account", model);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation(ForgotPasswordViewModel model)
        {
            return View(model);
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
            var user = await UserManager.FindByNameAsync(model.Email);
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

        [HttpPost]
        public JsonResult ChangePass(string correo, string clave, string idusuario)
        {

            UserManager.RemovePassword(idusuario);
            UserManager.AddPassword(idusuario, clave);

            return Json(new
            {
                success = true,
                message = "Movimiento agregado de forma correcta!"
            }, JsonRequestBehavior.DenyGet);
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

        #region Commerce
        [HttpPost]
        [CaptchaValidator(//PrivateKey = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
        ErrorMessage = "Captcha inválido",
        RequiredMessage = "Debe seleccionar el captcha.")]
        [AllowAnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, bool captchaValid)
        {
            var commerceModel = model.commerceModel;
            var commerceUserModel = model.commerceUserModel;
            URepository<Commerce> cRepo = new URepository<Commerce>();
            CUser commerceUser;
            Commerce commerce;

            #region Test

            //commerceModel = new RegisterCommerceModel();

            //commerceUserModel = new RegisterCommerceUserModel();

            //commerceUserModel.Name = "Alberto";

            //commerceUserModel.LastName = "Rojas";

            //commerceModel.Rif = "J111111111";

            //commerceModel.BusinessName = "PruebaBN";

            //commerceModel.SocialReasonName = "PruebaSRN";

            //commerceModel.Address = "PruebaAddr";

            //commerceModel.Phone = "PruebaP";

            #endregion

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };
                // Todo (Register): Hacer esta validacion con el [Required]
                if (commerceModel != null && commerceUserModel != null)
                {
                    commerceUser = new CUser
                    {
                        Id = Guid.NewGuid(),
                        Name = commerceUserModel.Name,
                        LastName = commerceUserModel.LastName,
                        IdAspNetUser = user.Id,
                        IdCUserStatus = (int)CommerceUserStatus.Active,
                        TestMode = true,
                        IsContact = true,
                        StatusChangeDate = DateTime.Now,
                        CreateDate = DateTime.Now
                    };
                    commerce = new Commerce
                    {
                        Rif = commerceModel.Rif,
                        BusinessName = commerceModel.BusinessName,
                        SocialReasonName = commerceModel.SocialReasonName,
                        Address = commerceModel.Address,
                        Phone = commerceModel.Phone,
                        IdCommerceStatus = (int)InstaTransfer.ITResources.Enums.CommerceStatus.Inactive,
                        WithdrawalFee = (decimal)2.8,
                        Commission = 0,
                        Trust = 15,
                        CUsers = new EntitySet<CUser> { commerceUser },
                        Nativa = false
                    };
                    // Creamos el usuario con el password establecido
                    var result = await UserManager.CreateAsync(user, model.Password);
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
                            // Guardamos la hora de inicio de sesion del usuario
                            commerceUser.LastLoginDate = DateTime.Now;
                            // Añadimos el comercio a la base de datos
                            cRepo.AddEntity(commerce);
                            // Guarda los cambios de la base de datos
                            cRepo.SaveChanges();
                            // Redireccionamos a la pagina deseada
                            return RedirectToAction("Index", "Home");
                        }
                        catch
                        {
                            UserManager.Delete(user);
                            AddErrors(new IdentityResult("Ha ocurrido un error por favor contacte al Adminsitrador del sistema. Error 002IT"));
                            return View(model);
                        }
                    }
                    AddErrors(result);
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Verifica el estado de la sesion
        /// </summary>
        /// <returns>El estado de la sesion del usuario</returns>
        public JsonResult CheckSession()
        {
            var isLoggedIn = MySession.Current.LoggedIn;
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
            return RedirectToAction("Index", "Home");
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
        [AllowAnonymous]
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
        [AllowAnonymous]
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

        [AllowAnonymous]
        public ActionAsPdf _printepropuesta(int _id)
        {

            return new ActionAsPdf("_Propuesta", new { id = _id })
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


        public bool _ProcesarArchivosCobroDiario()
        {
            InstaTransfer.DataAccess.AE_Dolar Dolar = AE_DolarREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).FirstOrDefault();
            //InstaTransfer.BLL.Concrete.Repository._connectionString = "";
            //InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta> AEmovimientosREPO = new InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta>();
            InstaTransfer.BLL.Concrete.URepository<AE_Propuesta> AE_PropuestaREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_Propuesta>();
            InstaTransfer.BLL.Concrete.URepository<AE_Avance> AE_AvanceREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_Avance>();
            InstaTransfer.BLL.Concrete.URepository<AE_EstadoCuenta> AE_EstadoCuentaREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_EstadoCuenta>();
            InstaTransfer.BLL.Concrete.URepository<AE_MovimientosDebito> AE_MovimientosDebitoREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_MovimientosDebito>();
            //C: \Users\carmelo\Desktop\Archivos\Pendientes
            DirectoryInfo d = new DirectoryInfo(ConfigurationManager.AppSettings["RutaGETCobroDiarioPendiente"].ToString());//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.xls"); //Getting Text files
                                                    //FileInfo[] Files2 = d.GetFiles("*.xlsx"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                decimal cobrado = 0;
                decimal cobrando = 0;
                List<InstaTransfer.DataAccess.AE_MovimientosDebito> Lista = new List<AE_MovimientosDebito>();
                List<InstaTransfer.DataAccess.AE_Avance> Listavance = new List<AE_Avance>();
                InstaTransfer.DataAccess.AE_Avance _avance = new AE_Avance();
                Application xlApp = new Application();
                if (!file.FullName.Contains('~'))
                {
                    //Create COM Objects. Create a COM object for everything that is referenced
                    Workbook xlWorkbook = xlApp.Workbooks.Open(file.FullName);
                    _Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                    Range xlRange = xlWorksheet.UsedRange;

                    int rowCount = xlRange.Rows.Count;
                    int colCount = 4;
                    string _rif = file.Name.Substring(0, 10);
                    int _idavance = 0;
                    if (file.Name.Contains("."))
                        _idavance = int.Parse(file.Name.Split('.')[0].Split('_')[4]);
                    else
                        _idavance = int.Parse(file.Name.Split('_')[4]);

                    Listavance = AE_AvanceREPO.GetAllRecords().Where(u => u.RifCommerce == _rif && u.IdEstatus == 1 && u.Id == _idavance).ToList();

                    if (Listavance.Count > 0)
                    {
                        _avance = Listavance.FirstOrDefault();

                        //iterate over the rows and columns and print to the console as it appears in the file
                        //excel is not zero based!!
                        for (int i = 1; i <= rowCount; i++)
                        {
                            InstaTransfer.DataAccess.AE_MovimientosDebito _Movimiento = new AE_MovimientosDebito();

                            bool error = false;
                            if (i > 1)
                            {
                                string item = "";
                                for (int j = 1; j <= colCount; j++)
                                {
                                    //new line
                                    if (j == 1)
                                        Console.Write("\r\n");
                                    try
                                    {
                                        //write the value to the console
                                        if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                                        {
                                            //item = item + xlRange.Cells[i, j].Value2.ToString() + "~";
                                            item = item + xlRange.Cells[i, j].Value2.ToString() + ";";
                                        }
                                    }
                                    catch
                                    {
                                        error = true;

                                    }

                                }
                                if (!error)
                                {
                                    try
                                    {
                                        //Thread.CurrentThread.CurrentCulture = new CultureInfo("es-VE");
                                        var arreglo = item.Split(';');
                                        if (ValidateRow(arreglo[2]))
                                        {
                                            if (BuscarRegistroDebito(arreglo[2].ToString(), arreglo[1].ToString(), arreglo[0], _rif))
                                            {

                                                DateTime conv;
                                                try
                                                {
                                                    //double _d = double.Parse(arreglo[0]);
                                                    conv = DateTime.Parse(arreglo[0]);
                                                }
                                                catch
                                                {
                                                    double _d = double.Parse(arreglo[0]);
                                                    conv = DateTime.FromOADate(_d);
                                                }

                                                _Movimiento.Fecha = conv;
                                                _Movimiento.Referencia = arreglo[1].ToString();
                                                _Movimiento.Descripcion = arreglo[2].ToString();
                                                string lote = "";
                                                try
                                                {
                                                    lote = getBetween(arreglo[2], "L.", " ");
                                                    if (lote == "")
                                                        lote = arreglo[2].Split(' ')[2];
                                                }
                                                catch
                                                {

                                                    lote = "001";
                                                }
                                                _Movimiento.Lote = int.Parse(lote);
                                                //_Movimiento.Monto = decimal.Parse(arreglo[3].Replace('+', ' ').Trim().ToString());
                                                try
                                                {
                                                    string monto = arreglo[3].Replace('+', ' ').Trim().ToString();
                                                    if (monto.Contains(".") && monto.Contains(","))
                                                    {
                                                        string monto2 = monto.Replace(',', ' ').Trim();
                                                        string monto3 = monto2.Replace('.', ' ').Trim();
                                                        string monto4 = monto3.Replace(" ", string.Empty).Trim();
                                                        _Movimiento.Monto = decimal.Parse(monto4) / 100;
                                                    }
                                                    else if (monto.Contains("."))
                                                    {
                                                        //string monto2 = monto.Replace(',', ' ').Trim();
                                                        string monto3 = monto.Replace('.', ' ').Trim();
                                                        string monto4 = monto3.Replace(" ", string.Empty).Trim();
                                                        _Movimiento.Monto = decimal.Parse(monto4) / 100;
                                                    }
                                                    else if (monto.Contains(","))
                                                    {
                                                        string monto3 = monto.Replace(',', ' ').Trim();
                                                        string monto4 = monto3.Replace(" ", string.Empty).Trim();
                                                        _Movimiento.Monto = decimal.Parse(monto4) / 100;
                                                    }
                                                    else
                                                    {
                                                        _Movimiento.Monto = decimal.Parse(monto);
                                                    }







                                                }
                                                catch
                                                {
                                                    _Movimiento.Monto = 10;
                                                }
                                                //_Movimiento.RifCommerce = _rif;
                                                _Movimiento.FechaRegistro = DateTime.Now;
                                                _Movimiento.Activo = true;
                                                AE_MovimientosDebitoREPO.AddEntity(_Movimiento);
                                                Lista.Add(_Movimiento);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        xlApp.Workbooks.Close();
                                        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                                    }

                                }
                            }
                        }
                    }
                }
                if (Lista.Count > 0)
                {
                    //GENERAMOS ESTADOS CUESTA
                    List<AE_EstadoCuenta> estadocuentalist = new List<AE_EstadoCuenta>();
                    List<AE_EstadoCuenta> Cobros = new List<AE_EstadoCuenta>();
                    estadocuentalist = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.IdAvance == _avance.Id).OrderBy(u => u.FechaRegistro).ToList();
                    AE_EstadoCuenta _ultimo = new AE_EstadoCuenta();

                    if (estadocuentalist.Count > 0)
                    {
                        _ultimo = estadocuentalist.FirstOrDefault();

                        //decimal saldofinal = _ultimo.SaldoFinal;
                        decimal pagos = estadocuentalist.Where(u => !u.Abono).Sum(u => u.Monto);
                        decimal saldofinal = (_avance.Reembolso - pagos) * Dolar.Tasa;
                        List<int> distinctLote = Lista.Select(p => p.Lote).Distinct().ToList();

                        if (saldofinal > 0)
                        {

                            foreach (var item in distinctLote)
                            {
                                List<InstaTransfer.DataAccess.AE_MovimientosDebito> _newlistbylote = Lista.Where(u => u.Lote == item).ToList();
                                decimal monto = _newlistbylote.Sum(u => u.Monto);
                                decimal debocobra = (monto * _avance.Porcentaje) / 100;
                                AE_EstadoCuenta estadocuenta = new AE_EstadoCuenta();
                                estadocuenta.MontoBase = monto;
                                estadocuenta.Abono = false;
                                estadocuenta.Estatus = 1;
                                estadocuenta.FechaOperacion = _newlistbylote.FirstOrDefault().Fecha;
                                estadocuenta.FechaRegistro = DateTime.Now;
                                estadocuenta.IdAvance = _avance.Id;
                                estadocuenta.Lote = item;
                                //estadocuenta.MontoBase = monto;
                                cobrando = cobrando + debocobra;
                                if (AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.IdAvance == estadocuenta.IdAvance && u.Lote == estadocuenta.Lote && u.Monto == estadocuenta.Monto && u.MontoBase == estadocuenta.MontoBase).ToList().Count > 0)

                                {


                                }
                                else
                                {

                                    if (debocobra > saldofinal)
                                    {
                                        cobrado = cobrado + saldofinal;
                                        estadocuenta.Monto = saldofinal;
                                        estadocuenta.SaldoFinal = 0;
                                        estadocuenta.SaldoInicial = saldofinal;
                                        saldofinal = saldofinal - saldofinal;
                                        _avance.IdEstatus = 2;
                                        AE_AvanceREPO.SaveChanges();
                                        Cobros.Add(estadocuenta);
                                        AE_EstadoCuentaREPO.AddEntity(estadocuenta);
                                        AE_EstadoCuentaREPO.SaveChanges();
                                        foreach (var imte2 in Lista.Where(u => u.Lote == item))
                                        {
                                            imte2.IdAE_EstadoCuenta = estadocuenta.Id;
                                        }
                                        break;

                                    }
                                    else if (cobrando > _avance.MaximoCobro)
                                    {
                                        decimal calculomonto = 0;
                                        if (cobrado == 0)
                                        {
                                            calculomonto = _avance.MaximoCobro;
                                        }
                                        else
                                        {
                                            calculomonto = _avance.MaximoCobro - cobrado;
                                        }
                                        cobrado = cobrado + calculomonto;
                                        estadocuenta.Monto = calculomonto;
                                        estadocuenta.SaldoFinal = saldofinal - calculomonto;
                                        estadocuenta.SaldoInicial = saldofinal;
                                        saldofinal = saldofinal - calculomonto;
                                        Cobros.Add(estadocuenta);
                                        AE_EstadoCuentaREPO.AddEntity(estadocuenta);
                                        AE_EstadoCuentaREPO.SaveChanges();
                                        foreach (var imte2 in Lista.Where(u => u.Lote == item))
                                        {
                                            imte2.IdAE_EstadoCuenta = estadocuenta.Id;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        cobrado = cobrado + debocobra;
                                        estadocuenta.Monto = debocobra;
                                        estadocuenta.SaldoFinal = saldofinal - debocobra;
                                        estadocuenta.SaldoInicial = saldofinal;
                                        saldofinal = saldofinal - debocobra;
                                    }
                                    Cobros.Add(estadocuenta);
                                    AE_EstadoCuentaREPO.AddEntity(estadocuenta);
                                    AE_EstadoCuentaREPO.SaveChanges();
                                    //ASOCIAMOS ESTACUENTA A MOVIMIENTOS DEBITO
                                    foreach (var imte2 in Lista.Where(u => u.Lote == item))
                                    {
                                        imte2.IdAE_EstadoCuenta = estadocuenta.Id;
                                    }
                                }

                            }

                        }
                        else
                        {
                            _avance.IdEstatus = 2;
                            AE_AvanceREPO.SaveChanges();
                            return true;
                        }
                    }
                    if (Cobros.Count > 0)
                    {
                        bool win = _GenerarArchivo(Cobros);
                        //bool win = false;
                        if (win)
                        {


                            try
                            {
                                xlApp.Workbooks.Close();
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                            }
                            catch { }

                            try
                            {
                                System.IO.File.Move(file.FullName, ConfigurationManager.AppSettings["RutaCobroDiarioProcesado"].ToString() + file.Name);
                                file.Delete();
                            }
                            catch { };
                        }
                        else
                        {
                            try
                            {
                                //xlApp.Workbooks.Close();
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                            }
                            catch { }
                            try
                            {
                                System.IO.File.Move(file.FullName, ConfigurationManager.AppSettings["RutaCobroDiarioFalla"].ToString() + file.Name);
                                xlApp.Workbooks.Close();
                            }
                            catch { }


                        }
                    }
                    else
                    {
                        System.IO.File.Move(file.FullName, ConfigurationManager.AppSettings["RutaCobroDiarioFalla"].ToString() + file.Name);
                        file.Delete();
                    }
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp.Workbooks);
                }
                else
                {
                    try
                    {
                        xlApp.Workbooks.Close();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                    }
                    catch { }
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp.Workbooks);
                    try
                    {
                        System.IO.File.Move(file.FullName, ConfigurationManager.AppSettings["RutaCobroDiarioFalla"].ToString() + file.Name);
                        file.Delete();
                    }
                    catch { }
                    //AE_EstadoCuenta estadocuenta = new AE_EstadoCuenta();
                    //estadocuenta.Abono = false;
                    //estadocuenta.Estatus = 1;
                    //estadocuenta.FechaOperacion = DateTime.Parse(file.Name.Split('_')[1]);
                    //estadocuenta.FechaRegistro = DateTime.Now;
                    //estadocuenta.IdAvance = _avance.Id;
                    //estadocuenta.Lote = 0;
                    //estadocuenta.MontoBase = 0;
                    //AE_EstadoCuentaREPO.AddEntity(estadocuenta);
                    //AE_EstadoCuentaREPO.SaveChanges();
                }


                try
                {
                    //xlApp.Workbooks.Close();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                }
                catch { }
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp.Workbooks);
            }

            return true;
        }

        public bool _GenerarArchivo(List<AE_EstadoCuenta> Cobros)
        {
            int cantidadmovimientos = Cobros.Count();
            int comercio = Cobros.First().AE_Avance.Id;
            string rif = Cobros.First().AE_Avance.RifCommerce;
            string fechaarchivo = DateTime.Now.ToString("yyyyMMddhhmmss");
            //datos fijos
            string registro = "HDR";
            string asociado = "BANESCO";
            string editfact = "E";
            string estanadarEditfact = "D  96A";
            string documento = "DIRDEB";
            string produccion = "P";
            string registrodecontrol = registro + asociado.PadRight(15) + editfact + estanadarEditfact + documento + produccion;
            //encabezado
            string tiporegistro = "01";
            string transaccion = "SUB";
            string condicion = "9";
            string id = Cobros.FirstOrDefault().Id.ToString();
            if (id.Length > 4)
            {
                id = id.Substring((id.Length - 4), 4);
            }
            else if (id.Length < 4)
            {
                id = id.PadLeft(4, '0');
            }
            string numeroorden = DateTime.Now.ToString("yyMMdd");
            numeroorden = numeroorden + id;
            string fecha = DateTime.Now.ToString("yyyyMMddhhmmss");
            string encabezado = tiporegistro + transaccion.PadRight(35) + condicion.PadRight(3) + numeroorden.PadRight(35) + fecha.PadRight(14);
            decimal total = 0;
            //debitos
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {
                string tipo = "03";
                string recibo = cobro.Id.ToString().PadLeft(8, '0');
                decimal _cambio = Math.Round(cobro.Monto, 2);
                _cambio = _cambio * 100;
                total = total + _cambio;
                string montoacobrar = _cambio.ToString().Split(',')[0];
                string moneda = "VEF";
                string numerocuenta = Cobros.FirstOrDefault().AE_Avance.NumeroCuenta;
                string swift = "BANSVECA";
                //string _rif = Cobros.FirstOrDefault().AE_Avance.Commerce.Rif;
                string nombre = Cobros.FirstOrDefault().AE_Avance.Commerce.SocialReasonName.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                string libre = "   ";
                string contrato = rif.Substring((rif.Length - 4), 4);
                string fechavencimiento = "      ";
                string debito = tipo + recibo.PadRight(30)
                    + montoacobrar.PadLeft(15, '0') + moneda + numerocuenta.PadRight(30)
                    + swift.PadRight(11) + rif.PadRight(17) + nombre.PadRight(35)
                    + libre + contrato.PadRight(35) + fechavencimiento;
                _cobros.Add(debito);

            }

            //registro credito
            string _tipo2 = "02";
            string _recibo = Cobros.First().Id.ToString().PadLeft(8, '0');
            string _rif = "J401878105";
            string ordenante = "TECNOLOGIA INSTAPAGO C A";
            //decimal cambio = Math.Round(Cobros.Sum(y => y.Monto), 2);
            //cambio = cambio * 100;
            string _montoabono = total.ToString().Split(',')[0];
            string _moneda = "VEF";
            string _numerocuenta = "01340031810311158627";
            string _swift = "BANSVECA";
            string _fecha = DateTime.Now.ToString("yyyyMMdd");
            string formadepago = "CB";
            string instruordenante = " ";
            string credito = _tipo2 + _recibo.PadRight(30) + _rif.PadRight(17) + ordenante.PadRight(35)
                + _montoabono.PadLeft(15, '0') + _moneda + instruordenante + _numerocuenta.PadRight(35)
                + _swift.PadRight(11) + _fecha + formadepago;

            //_cobros
            string[] lines = { registrodecontrol, encabezado, credito };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            //totalizador
            string _tipo = "04";
            string totalcreditos = "1";
            string debitos = Cobros.Count().ToString();
            string montototal = total.ToString().Split(',')[0];
            string totales = _tipo + totalcreditos.PadLeft(15, '0') + debitos.PadLeft(15, '0') + montototal.PadLeft(15, '0');
            Array.Resize(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = totales;


            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            string ruta = ConfigurationManager.AppSettings["RutaCargoCuenta"].ToString() + rif + "_" + comercio + "_" + fechaarchivo + ".txt";
            System.IO.File.WriteAllLines(ruta, lines);
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("es-VE");
            InstaTransfer.BLL.Concrete.URepository<AE_Archivo> archivoREPO = new InstaTransfer.BLL.Concrete.URepository<AE_Archivo>();
            AE_Archivo _nuevo = new AE_Archivo();
            _nuevo.FechaCreacion = DateTime.Now;
            _nuevo.IdAE_avance = Cobros.FirstOrDefault().AE_Avance.Id;
            _nuevo.Monto = Math.Round(Cobros.Sum(y => y.Monto), 2);
            _nuevo.FechaEjecucion = DateTime.Now;
            _nuevo.Ruta = ruta;
            _nuevo.Valores = numeroorden;
            _nuevo.ConsultaExitosa = false;
            _nuevo.CorreoSoporteEnviado = false;
            _nuevo.IdAE_ArchivosStatus = 1;
            _nuevo.StatusChangeDate = DateTime.Now;
            _nuevo.RutaRespuesta = "nada - escribo servicio COBRO DIARIO";
            archivoREPO.AddEntity(_nuevo);
            archivoREPO.SaveChanges();

            return true;
        }
        public bool _ProcesarArchivosCobroDiarioCVS()
        {

            DirectoryInfo d = new DirectoryInfo(@"C:\Users\carmelo\Desktop\Archivos\csv");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.csv"); //Getting Text files
            //FileInfo[] Files2 = d.GetFiles("*.xlsx"); //Getting Text files
            foreach (FileInfo file in Files)
            {

                String[] values = System.IO.File.ReadAllLines(file.FullName);


            }



            return true;
        }


        #endregion

        #region METODOS_PRIVADOS

        public string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        public bool ValidateRow(string Descripcion)
        {
            InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta> AEmovimientosREPO = new InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta>();
            String referecnias = ConfigurationManager.AppSettings["Referencia"].ToString();
            var lista = referecnias.Split(',');
            foreach (var item in lista)
            {
                if (item == "TT")
                {
                    if (Descripcion.Contains("TT "))
                    {
                        return true;
                    }
                }
                else
                {
                    if (Descripcion.Contains(item))
                    {
                        //if (Descripcion.Contains("L."))
                        return true;
                    }
                }
            }
            return false;

        }

        public bool BuscarRegistro(string Descripcion, string Referencia, string fecha, string _rif)
        {
            InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta> AEmovimientosREPO = new InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta>();
            //double d = double.Parse(fecha);
            //DateTime _fecha = DateTime.FromOADate(d);
            //string fechaformato = fecha.Split('/')[1] + "/" + fecha.Split('/')[0] + "/" + fecha.Split('/')[2];
            DateTime _fecha = DateTime.Parse(fecha);
            List<InstaTransfer.DataAccess.AE_MovimientosCuenta> _movimientos = AEmovimientosREPO.GetAllRecords().Where(u => u.RifCommerce == _rif && u.Activo && u.Fecha == _fecha && u.Descripcion == Descripcion && u.Referencia == Referencia).ToList();
            if (_movimientos.Count() > 0)
            {
                return false;
            }
            return true;

        }

        public bool BuscarRegistroDebito(string Descripcion, string Referencia, string fecha, string _rif)
        {
            InstaTransfer.BLL.Concrete.URepository<AE_MovimientosDebito> AEmovimientosREPO = new InstaTransfer.BLL.Concrete.URepository<AE_MovimientosDebito>();
            //double d = double.Parse(fecha);
            DateTime _fecha;
            try
            {
                _fecha = DateTime.Parse(fecha);
            }
            catch
            {
                double d = double.Parse(fecha);
                _fecha = DateTime.FromOADate(d);
            }

            List<InstaTransfer.DataAccess.AE_MovimientosDebito> _movimientos = AEmovimientosREPO.GetAllRecords().Where(u => u.Descripcion == Descripcion && u.Referencia == Referencia && u.Fecha == _fecha && u.Activo).ToList();
            if (_movimientos.Count() > 0)
            {
                return false;
            }
            return true;

        }

        #endregion

        #region POLAR

        public bool _GenerarArchivoBANPOL(List<AE_EstadoCuenta> Cobros)
        {
            int cantidadmovimientos = Cobros.Count();
            //string comercio = Cobros.First().AE_Avance.Id;
            string rif = Cobros.First().AE_Avance.RifCommerce;
            string fechaarchivo = DateTime.Now.AddDays(0).ToString("ddMMyy.hhmm");
            string id = Cobros.FirstOrDefault().Id.ToString();
            if (id.Length > 4)
            {
                id = id.Substring((id.Length - 4), 4);
            }
            else if (id.Length < 4)
            {
                id = id.PadLeft(4, '0');
            }
            string numeroorden = DateTime.Now.AddDays(0).ToString("yyMMdd");
            string _fecha = DateTime.Now.AddDays(0).ToString("yyyyMMdd");
            numeroorden = numeroorden + id;
            //datos fijos
            string registro = "00";
            //string idtransaccion = "2020040801";
            string asociado = "208515428";
            string ordencobroreferencia = numeroorden;
            string documento = "DIRDEB";
            string banco = "01";
            string fecha = DateTime.Now.AddDays(0).ToString("yyyyMMddhhmmss");
            string registrodecontrol = registro + asociado.PadRight(35) + ordencobroreferencia.PadRight(30) + documento + fecha.PadRight(14) + banco;
            //encabezado
            string tiporegistro = "01";
            string transaccion = "DMI";
            string condicion = "9";


            //string fecha = DateTime.Now.ToString("yyyyMMddhhmmss");
            string encabezado = tiporegistro + transaccion.PadRight(35) + condicion.PadRight(3) + ordencobroreferencia.PadRight(35) + _fecha;

            decimal total = 0;
            //debitos
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {
                string tipo = "03";
                string recibo = cobro.Id.ToString().PadLeft(8, '0');
                decimal _cambio = Math.Round(cobro.Monto, 2);
                _cambio = _cambio * 100;
                total = total + _cambio;
                string montoacobrar = _cambio.ToString().Split(',')[0];
                string moneda = "VES";
                string numerocuenta = Cobros.FirstOrDefault().AE_Avance.NumeroCuenta;
                string swift = "UNIOVECA";
                //string _rif = Cobros.FirstOrDefault().AE_Avance.Commerce.Rif;
                string nombre = Cobros.FirstOrDefault().AE_Avance.Commerce.SocialReasonName.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                string libre = "423";
                string contrato = rif;
                string fechavencimiento = "      ";
                string debito = tipo + recibo.PadRight(30)
                    + montoacobrar.PadLeft(15, '0') + moneda + numerocuenta.PadRight(30)
                    + swift.PadRight(11) + rif.PadRight(17) + nombre.PadRight(35)
                    + libre + contrato.PadRight(35) + fechavencimiento;
                _cobros.Add(debito);

            }

            //registro credito
            string _tipo2 = "02";
            string _recibo = Cobros.First().Id.ToString().PadLeft(8, '0');
            string _rif = "J401878105";
            string ordenante = "TECNOLOGIA INSTAPAGO C A";
            string _montoabono = total.ToString().Split(',')[0];
            string _moneda = "VES";
            string _numerocuenta = "01340031810311158627";
            string _swift = "UNIOVECA";
            //string _fecha = DateTime.Now.ToString("yyyyMMdd");
            string formadepago = "423";
            string instruordenante = " ";
            string credito = _tipo2 + _recibo.PadRight(30) + _rif.PadRight(17) + ordenante.PadRight(35)
                + _montoabono.PadLeft(15, '0') + _moneda + instruordenante + _numerocuenta.PadRight(35)
                + _swift.PadRight(11) + _fecha + formadepago;

            //_cobros
            string[] lines = { registrodecontrol, encabezado, credito };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            //totalizador
            string _tipo = "04";
            string totalcreditos = "1";
            string debitos = Cobros.Count().ToString();
            string montototal = total.ToString().Split(',')[0];
            string totales = _tipo + totalcreditos.PadLeft(15, '0') + debitos.PadLeft(15, '0') + montototal.PadLeft(15, '0');
            Array.Resize(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = totales;


            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            //string ruta = ConfigurationManager.AppSettings["RutaCargoCuenta"].ToString() + rif + "_" + comercio + "_" + fechaarchivo + ".txt";
            string ruta = @"C:\Users\carmelo\Desktop\POLAR\CargoCuentaBanesco\" + "I0005.208515428." + fechaarchivo + ".txt";
            System.IO.File.WriteAllLines(ruta, lines);


            CP_Archivo archivo = new CP_Archivo();
            archivo.IdEmpresa = 1;
            archivo.Nombre = "I0005.208515428." + fechaarchivo + ".txt";
            archivo.Ruta = ruta;
            archivo.Tipo = 1;
            string contenido = "";
            foreach (var item in lines)
            {
                contenido = item + "</br>";
            }
            archivo.Contenido = contenido;
            archivo.FechaLectura = DateTime.Now;
            archivo.FechaCreacion = DateTime.Now;
            archivo.Descripcion = "Cargo cuenta masivo de la empresa Fin Pagos.";
            archivo.IdCP_Archivo = null;
            archivo.ReferenciaOrigen = "Estado de cuenta operaciones de prestamos";
            CP_ArchivoREPO.AddEntity(archivo);
            CP_ArchivoREPO.SaveChanges();

            return true;
        }

        public bool LecturaArchivoSalidaBanesco()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\carmelo\Desktop\POLAR\Salida");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
            List<Estructura> Lista = new List<Estructura>();
            CP_Archivo item = new CP_Archivo();

            foreach (FileInfo file in Files)
            {
                item.Nombre = file.Name;
                item.Ruta = file.FullName;
                item.FechaLectura = DateTime.Now;
                string lineas = "";
                int i = 0;
                string text = System.IO.File.ReadAllText(file.FullName);
                //System.Console.WriteLine("Contents of WriteText.txt = {0}", text);
                // Example #2
                // Read each line of the file into a string array. Each element
                // of the array is one line of the file.
                string[] lines = System.IO.File.ReadAllLines(file.FullName);
                // Display the file contents by using a foreach loop.
                //System.Console.WriteLine("Contents of WriteLines2.txt = ");
                //_cobros
                string[] linesArchivo = { };
                foreach (string line in lines)
                {
                    if (i == 2)
                    {
                        lineas = lineas + line + "<br />";
                        string sep = "\t";
                        
                  
                       string tipo = line.Substring(16, 17).ToString().TrimEnd();

                        if (tipo == "01")
                        {
                            EstructuraSalidaBanescoEncabezado Registro = new EstructuraSalidaBanescoEncabezado();
                            Registro.Trading = line.Substring(0, 14).ToString().TrimEnd();
                            Registro.Filler = line.Substring(14, 15).ToString().TrimEnd();
                            Registro.TipoRegistro = line.Substring(16, 18).ToString().TrimEnd();
                            Registro.__NumeroReferenciaRespuesta = line.Substring(19, 53).ToString().TrimEnd();
                            Registro.__FechaRespuesta = line.Substring(54, 67).ToString().TrimEnd();
                            Registro.__NumeroReferenciaOrdenPago = line.Substring(68, 102).ToString().TrimEnd();
                            Registro.__TipoOrdenPago = line.Substring(103, 105).ToString().TrimEnd();
                            Registro.__CodigoBancoEmisor = line.Substring(106, 116).ToString().TrimEnd();
                            Registro.__NombreBancoEmisor = line.Substring(117, 186).ToString().TrimEnd();
                            Registro.__CodgioEmpresaReceptoraBansta = line.Substring(187, 203).ToString().TrimEnd();
                            Registro.__DescripcionEmpresaReceptoraBansta = line.Substring(204, 273).ToString().TrimEnd();

                            string _line = Registro.__NumeroReferenciaOrdenPago + "|1|";

                            Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                            linesArchivo[linesArchivo.Length - 1] = _line;
                        }
                        else if (tipo == "02")
                        {
                            EstructuraSalidaBanescoDetalle Registro = new EstructuraSalidaBanescoDetalle();
                            Registro.Trading = line.Substring(0, 14).ToString().TrimEnd();
                            Registro.Filler = line.Substring(14, 15).ToString().TrimEnd();
                            Registro.TipoRegistro = line.Substring(16, 17).ToString().TrimEnd();
                            Registro.Indicador = line.Substring(17, 18).ToString().TrimEnd();
                            Registro.NumeroReferencia = line.Substring(19, 53).ToString().TrimEnd();
                            Registro.Fecha = line.Substring(54, 61).ToString().TrimEnd();
                            Registro.Monto = line.Substring(62, 76).ToString().TrimEnd();
                            Registro.Moneda = line.Substring(77, 79).ToString().TrimEnd();
                            Registro.Rif = line.Substring(80, 96).ToString().TrimEnd();
                            Registro.NumeroCuenta = line.Substring(97, 131).ToString().TrimEnd();
                            Registro.BancoBeneficiario = line.Substring(132, 142).ToString().TrimEnd();
                            Registro.BancoBeneficiarioDescripcion = line.Substring(143, 212).ToString().TrimEnd();
                            Registro.CodigoAgencia = line.Substring(213, 215).ToString().TrimEnd();
                            Registro.NombreBeneficiario = line.Substring(216, 285).ToString().TrimEnd();
                            Registro.NumeroCliente = line.Substring(286, 320).ToString().TrimEnd();
                            Registro.FechaVencimiento = line.Substring(321, 326).ToString().TrimEnd();
                            Registro.NumeroSecuenciaArchivo = line.Substring(327, 332).ToString().TrimEnd();

                            string _line = Registro.NumeroCuenta + "|1|";

                            Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                            linesArchivo[linesArchivo.Length - 1] = _line;
                        }
                        else if (tipo == "03")
                        {
                            EstructuraSalidaBanescoEstatus Registro = new EstructuraSalidaBanescoEstatus();
                            Registro.Trading = line.Substring(0, 14).ToString().TrimEnd();
                            Registro.Filler = line.Substring(14, 15).ToString().TrimEnd();
                            Registro.TipoRegistro = line.Substring(16, 17).ToString().TrimEnd();
                            Registro._CodigoEstatus = line.Substring(19, 21).ToString().TrimEnd();
                            Registro._Descripcion = line.Substring(22, 91 ).ToString().TrimEnd();

                            string _line = Registro._CodigoEstatus + "|1|";

                            Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                            linesArchivo[linesArchivo.Length - 1] = _line;
                        }
                        else if (tipo == "04")
                        {
                          
                        }

                    }
                    i++;
                }
                item.Contenido = lineas;
                item.Descripcion = "Archivo Salida Banesco";
                item.Tipo = 1;
                ArchivoREPO.AddEntity(item);
                ArchivoREPO.SaveChanges();


            }
            return true;
        }
        public bool LecturaArchivoBeneficiarioPOLAR()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\carmelo\Desktop\POLAR\Beneficiarios");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
            List<Estructura> Lista = new List<Estructura>();
            CP_Archivo item = new CP_Archivo();

            foreach (FileInfo file in Files)
            {
                item.Nombre = file.Name;
                item.Ruta = file.FullName;
                item.FechaLectura = DateTime.Now;
                string lineas = "";
                string text = System.IO.File.ReadAllText(file.FullName);
                //System.Console.WriteLine("Contents of WriteText.txt = {0}", text);

                // Example #2
                // Read each line of the file into a string array. Each element
                // of the array is one line of the file.
                string[] lines = System.IO.File.ReadAllLines(file.FullName);

                // Display the file contents by using a foreach loop.
                //System.Console.WriteLine("Contents of WriteLines2.txt = ");
                foreach (string line in lines)
                {
                    //lineas = lineas + line + "<br />";
                    // Use a tab to indent each line of the file.
                    //string sep = "\t";
                    ////string[] splitContent = content.Split(sep.ToCharArray());
                    string[] valores = line.Split('|').ToArray();
                    //CP_Beneficiario Beneficiario = new InstaTransfer.DataAccess.CP_Beneficiario();
                    //Beneficiario.Rif = line.Substring(0, 9).ToString().TrimEnd();
                    //Beneficiario.Contrato = line.Substring(13, 21).ToString().TrimEnd();
                    //Beneficiario.TitularCuenta = line.Substring(53, 88).ToString().TrimEnd();
                    //Beneficiario.Cuenta = line.Substring(32, 52).ToString().TrimEnd();
                    CP_Beneficiario Beneficiario = new InstaTransfer.DataAccess.CP_Beneficiario();

                    Beneficiario.CodigoCliente = valores[0];
                    Beneficiario.CodigoMaster = valores[1];
                    Beneficiario.PermisoMaster = valores[2];
                    Beneficiario.Nombre = valores[3];
                    Beneficiario.RazonSocial = valores[4];
                    Beneficiario.Identificacion = valores[5];
                    Beneficiario.IdentificacionPagador = valores[5];
                    Beneficiario.Region = valores[6];
                    Beneficiario.CorreoElectronico = valores[7];
                    Beneficiario.Telefono = valores[8];
                    Beneficiario.TipoIdentificacion = valores[9];
                    Beneficiario.CodigoEstatus = valores[10];
                    Beneficiario.PorcentajeIR_IVA = valores[11];
                    Beneficiario.CodigoDepartamento = valores[12];
                    Beneficiario.Cuenta = valores[13];
                    Beneficiario.FechaCreacion = DateTime.Now;
                    Beneficiario.FechaUltimaActualizacion = DateTime.Now;
                    CP_Beneficiario.AddEntity(Beneficiario);
                    CP_Beneficiario.SaveChanges();
                    //Lista.Add(objeto);
                }
                item.Contenido = lineas;
                item.Descripcion = "Archivo Beneficiarios";
                item.Tipo = 1;
                ArchivoREPO.AddEntity(item);
                ArchivoREPO.SaveChanges();


            }
            return true;
        }
        public bool LecturaArchivoCobroPOLAR()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\carmelo\Desktop\POLAR\COB0002");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
            List<Estructura> Lista = new List<Estructura>();
            CP_Archivo item = new CP_Archivo();
            foreach (FileInfo file in Files)
            {
                string text = System.IO.File.ReadAllText(file.FullName);
                //System.Console.WriteLine("Contents of WriteText.txt = {0}", text);
                item.Nombre = file.Name;
                item.Ruta = file.FullName;
                item.FechaLectura = DateTime.Now;
                string lineas = "";
                // Example #2
                // Read each line of the file into a string array. Each element
                // of the array is one line of the file.
                string[] lines = System.IO.File.ReadAllLines(file.FullName);

                // Display the file contents by using a foreach loop.
                //System.Console.WriteLine("Contents of WriteLines2.txt = ");
                foreach (string line in lines)
                {
                    lineas = lineas + line + "<br />";
                    // Use a tab to indent each line of the file.
                    string[] valores = line.Split('|').ToArray();
                    Estructura objeto = new Estructura();

                    switch (valores.Count())
                    {
                        case 10:
                            objeto.CodigoCliente = valores[0];
                            objeto.DocumentoComercial = valores[1];
                            objeto.Asociacion = valores[2];
                            objeto.Departamento = valores[3];
                            objeto.Monto = valores[4].ToString().Replace('.', ',');
                            objeto.Fecha = valores[5];
                            objeto.Fechavencimiento = valores[6];
                            objeto.TipoOperacion = valores[7];
                            objeto.x = valores[8];
                            objeto.Concepto = valores[9];

                            break;
                        case 11:
                            objeto.CodigoCliente = valores[0];
                            objeto.DocumentoComercial = valores[1];
                            objeto.Asociacion = valores[2];
                            objeto.Departamento = valores[3];
                            objeto.Monto = valores[4].ToString().Replace('.', ',');
                            objeto.Fecha = valores[5];
                            objeto.Fechavencimiento = valores[6];
                            objeto.TipoOperacion = valores[7];
                            objeto.x = valores[8];
                            objeto.Concepto = valores[9];
                            objeto.Comentario = valores[10];
                            break;
                        default:
                            break;
                    }


                    Lista.Add(objeto);
                }
                int largo = Lista.Count();
                int i = 0;
                var groupedCustomerList = Lista.GroupBy(u => u.CodigoCliente).Select(grp => grp.ToList()).ToList();

                item.Contenido = lineas;
                item.Descripcion = "Archivo Cobro Polar - Comercios Evaluado:" + groupedCustomerList.Count();
                item.Tipo = 2;
                ArchivoREPO.AddEntity(item);
                ArchivoREPO.SaveChanges();
                //Generar Estado de Cuenta
                foreach (var ids in groupedCustomerList)
                {
                    CP_ArchivoEstadoCuenta EstadoCuenta = new CP_ArchivoEstadoCuenta();
                    decimal Total = 0;
                    decimal Debito = 0;
                    decimal Credito = 0;
                    string debito = "";
                    string credito = "";
                    string comercio = "";
                    foreach (var row in ids)
                    {
                        try
                        {
                            comercio = row.CodigoCliente;
                            if (row.TipoOperacion == "02")
                            {
                                Credito = Credito + decimal.Parse(row.Monto);
                                credito = credito + " " + row.DocumentoComercial + " " + row.Fecha + " NOTA CREDITO" + " | ";
                            }
                            else
                            {

                                switch (row.TipoOperacion)
                                {
                                    case "01":
                                        Debito = Debito + decimal.Parse(row.Monto);
                                        debito = debito + " " + row.DocumentoComercial + " " + row.Fecha + " FACTURA" + " | ";
                                        break;
                                    case "03":
                                        Debito = Debito + decimal.Parse(row.Monto);
                                        debito = debito + " " + row.DocumentoComercial + " " + row.Fecha + " NOTA DEBITO" + " | ";
                                        break;
                                    case "04":
                                        Debito = Debito + decimal.Parse(row.Monto);
                                        debito = debito + " " + row.DocumentoComercial + " " + row.Fecha + " FACTURA" + " | ";
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                        catch
                        {
                            //crear log de error y guardar en base de datos
                        }
                    }

                    Total = Credito - Debito;
                    EstadoCuenta.ArchivoLecturaPolar = item.Id;
                    EstadoCuenta.MontoCredito = Credito;
                    EstadoCuenta.MontoDebito = Debito;
                    EstadoCuenta.Total = Total;
                    EstadoCuenta.TotalArchivo = Total * -1;
                    EstadoCuenta.DetalleCredito = credito;
                    EstadoCuenta.DetalleDebito = debito;
                    EstadoCuenta.FechaLectura = DateTime.Now;
                    EstadoCuenta.CodigoComercio = comercio;
                    if (EstadoCuenta.Total > 0)
                    {
                        EstadoCuenta.Estatus = 2;
                    }
                    else
                    {
                        EstadoCuenta.Estatus = 1;
                    }
                    EstadoCuentaREPO.AddEntity(EstadoCuenta);
                    EstadoCuentaREPO.SaveChanges();
                }

            }
            return true;
        }
        public void DownloadAll()
        {
            string host = @"sftp.domain.com";
            string username = "myusername";
            string password = "mypassword";

            string remoteDirectory = "/RemotePath/";
            string localDirectory = @"C:\LocalDriveFolder\Downloaded\";

            using (var sftp = new SftpClient(host, username, password))
            {
                sftp.Connect();
                var files = sftp.ListDirectory(remoteDirectory);

                foreach (var file in files)
                {
                    string remoteFileName = file.Name;
                    if ((!file.Name.StartsWith(".")) && ((file.LastWriteTime.Date == DateTime.Today)))
                    {
                        using (Stream file1 = System.IO.File.OpenWrite(localDirectory + remoteFileName))
                        {
                            sftp.DownloadFile(remoteDirectory + remoteFileName, file1);
                        }
                    }
                }

            }
        }

        public void FileUploadSFTP()
        {
            var host = "whateverthehostis.com";
            var port = 22;
            var username = "username";
            var password = "passw0rd";

            // path for file you want to upload
            var uploadFile = @"c:yourfilegoeshere.txt";

            using (var client = new SftpClient(host, port, username, password))
            {
                client.Connect();
                if (client.IsConnected)
                {
                    //Debug.WriteLine("I'm connected to the client");

                    using (var fileStream = new FileStream(uploadFile, FileMode.Open))
                    {

                        client.BufferSize = 4 * 1024; // bypass Payload error large files
                        client.UploadFile(fileStream, Path.GetFileName(uploadFile));
                    }
                }
                else
                {
                    //Debug.WriteLine("I couldn't connect");
                }
            }
        }

        public bool EjercicioCobro()
        {
            List<CP_ArchivoEstadoCuenta> Lista = EstadoCuentaREPO.GetAllRecords().Where(u => u.Estatus == 1).OrderBy(u => u.Id).ToList();
            //int skip = 0;
            //List<CP_ArchivoEstadoCuenta> Segmento = Lista.Skip(skip).Take(50).ToList();
            //bool win = GenerarCobroBanesco(Lista.Take(50).ToList());
            bool win = GenerarCobroBanesco(Lista.ToList());
            bool winv = GenerarValidacionCobroBanesco(Lista.ToList());
            if (win && winv)
            {
                foreach (var item in Lista)
                {
                    item.Estatus = 2;
                    EstadoCuentaREPO.SaveChanges();
                }
            }


            return true;
        }

        public bool GenerarCobroBanesco(List<CP_ArchivoEstadoCuenta> Cobros)
        {
            int cantidadmovimientos = Cobros.Count();
            //string comercio = Cobros.First().AE_Avance.Id;
            //string rif = Cobros.First().AE_Avance.RifCommerce;
            string fechaarchivo = DateTime.Now.AddDays(0).ToString("ddMMyy.hhmm");
            string id = Cobros.FirstOrDefault().Id.ToString();
            if (id.Length > 4)
            {
                id = id.Substring((id.Length - 4), 4);
            }
            else if (id.Length < 4)
            {
                id = id.PadLeft(4, '0');
            }
            string numeroorden = DateTime.Now.AddDays(0).ToString("yyMMdd");
            string _fecha = DateTime.Now.AddDays(0).ToString("yyyyMMdd");
            numeroorden = numeroorden + id;
            //datos fijos
            string registro = "00";
            //string idtransaccion = "2020040801";
            string asociado = "208515428";
            string ordencobroreferencia = numeroorden;
            string documento = "DIRDEB";
            string banco = "01";
            string fecha = DateTime.Now.AddDays(0).ToString("yyyyMMddhhmmss");
            string registrodecontrol = registro + asociado.PadRight(35) + ordencobroreferencia.PadRight(30) + documento + fecha.PadRight(14) + banco;
            //encabezado
            string tiporegistro = "01";
            string transaccion = "DMI";
            string condicion = "9";
      
      
            //string fecha = DateTime.Now.ToString("yyyyMMddhhmmss");
            string encabezado = tiporegistro + transaccion.PadRight(35) + condicion.PadRight(3) + ordencobroreferencia.PadRight(35) + _fecha;
            decimal total = 0;
            //debitos
            int k = 0;
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {
                CP_Beneficiario beneficiario = CP_Beneficiario.GetAllRecords().Where(u => u.CodigoCliente == cobro.CodigoComercio).FirstOrDefault();
                string _cuenta;
                string _nombrecomercial;
                string _rifc;
                if (beneficiario != null && beneficiario.Id > 0)
                {
                    _cuenta = beneficiario.Cuenta;
                    _nombrecomercial = beneficiario.RazonSocial;
                    _rifc = beneficiario.TipoIdentificacion +  beneficiario.Identificacion.PadLeft(9,'0');
                }
                else
                {
                    if (k == 0)
                    {
                        _cuenta = "01340373233733019371";
                        _nombrecomercial = "Carmelo Larez";
                        _rifc = "V018601098";
                    }
                    else if (k == 1)
                    {
                        _cuenta = "01340874278743016046";
                        _nombrecomercial = "Alexyomar Istruriz";
                        _rifc = "V017302339";
                    }
                    else
                    {
                        _cuenta = "01340373233733019371";
                        _nombrecomercial = "Carmelo Larez";
                        _rifc = "V018601098";

                    }
                    k++;
                }
                string tipo = "03";
                string recibo = cobro.Id.ToString().PadLeft(8, '0');
                decimal _cambio = Math.Round(cobro.TotalArchivo, 2);
                _cambio = _cambio * 100;
                total = total + _cambio;
                string montoacobrar = _cambio.ToString().Split(',')[0];
                string moneda = "VES";
                string numerocuenta = _cuenta;
                string swift = "UNIOVECA";
                //string _rif = Cobros.FirstOrDefault().AE_Avance.Commerce.Rif;
                string nombre = _nombrecomercial.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                string libre = "423";
                string contrato = _rifc;
                string fechavencimiento = "       ";
                string debito = tipo + recibo.PadRight(30)
                    + montoacobrar.PadLeft(15, '0') + moneda + numerocuenta.PadRight(30)
                    + swift.PadRight(11) + _rifc.PadRight(17) + nombre.PadRight(35)
                    + libre + contrato.PadRight(35) + fechavencimiento;
                _cobros.Add(debito);

            }

            //registro credito
            string _tipo2 = "02";
            string _recibo = Cobros.First().Id.ToString().PadLeft(8, '0');
            string _rif = "J401878105";
            string ordenante = "TECNOLOGIA INSTAPAGO C A";
            //decimal cambio = Math.Round(Cobros.Sum(y => y.Monto), 2);
            //cambio = cambio * 100;
            string _montoabono = total.ToString().Split(',')[0];
            string _moneda = "VES";
            string _numerocuenta = "01340031810311158627";
            string _swift = "UNIOVECA";
            //string _fecha = DateTime.Now.ToString("yyyyMMdd");
            string formadepago = "423";
            string instruordenante = " ";
            string credito = _tipo2 + _recibo.PadRight(30) + _rif.PadRight(17) + ordenante.PadRight(35)
                + _montoabono.PadLeft(15, '0') + _moneda + instruordenante + _numerocuenta.PadRight(35)
                + _swift.PadRight(11) + _fecha + formadepago;

            //_cobros
            string[] lines = { registrodecontrol, encabezado, credito };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            //totalizador
            string _tipo = "04";
            string totalcreditos = "1";
            string debitos = Cobros.Count().ToString();
            string montototal = total.ToString().Split(',')[0];
            string totales = _tipo + totalcreditos.PadLeft(15, '0') + debitos.PadLeft(15, '0') + montototal.PadLeft(15, '0');
            Array.Resize(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = totales;


            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            string ruta = @"C:\Users\carmelo\Desktop\POLAR\CargoCuentaBanesco\" + "I0005.208515428." + fechaarchivo + ".txt";
            System.IO.File.WriteAllLines(ruta, lines);
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("es-VE");
            //InstaTransfer.BLL.Concrete.URepository<AE_Archivo> archivoREPO = new InstaTransfer.BLL.Concrete.URepository<AE_Archivo>();
            //AE_Archivo _nuevo = new AE_Archivo();
            //_nuevo.FechaCreacion = DateTime.Now;
            //_nuevo.IdAE_avance = Cobros.FirstOrDefault().AE_Avance.Id;
            //_nuevo.Monto = Math.Round(Cobros.Sum(y => y.Monto), 2);
            //_nuevo.FechaEjecucion = DateTime.Now;
            //_nuevo.Ruta = ruta;
            //_nuevo.Valores = numeroorden;
            //_nuevo.ConsultaExitosa = false;
            //_nuevo.CorreoSoporteEnviado = false;
            //_nuevo.IdAE_ArchivosStatus = 1;
            //_nuevo.StatusChangeDate = DateTime.Now;
            //_nuevo.RutaRespuesta = "nada - escribo servicio COBRO DIARIO";
            //archivoREPO.AddEntity(_nuevo);
            //archivoREPO.SaveChanges();

            return true;
        }

        public bool GenerarValidacionCobroBanesco(List<CP_ArchivoEstadoCuenta> Cobros)
        {
            int cantidadmovimientos = Cobros.Count();
            string fechaarchivo = DateTime.Now.AddDays(0).ToString("ddMMyy.hhmm");
     
            string _fecha = DateTime.Now.AddDays(0).ToString("yyyyMMdd");
            //datos fijos
            string registro = "00";
            //BUSCAR RIF BASE DE DATOS
            string rif = "J401878105";
            string Filler = "                          ";
            string Referencia = DateTime.Now.AddDays(0).ToString("yyyyMMddhhmm"); ;
            string documento = "AFILIA";
            string registrodecontrol = registro + rif + Filler + Referencia.PadRight(30) + documento;
     
            int k = 0;
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {
                CP_Beneficiario beneficiario = CP_Beneficiario.GetAllRecords().Where(u => u.CodigoCliente == cobro.CodigoComercio).FirstOrDefault();

                if (beneficiario != null && beneficiario.Id > 0)
                {
                    string _tipodocumento = beneficiario.TipoIdentificacion;
                    string _documento = beneficiario.Identificacion.PadLeft(10,'0');
                    string digito = "0";
                    string tipocuenta = "01";
                    string numerocuenta = beneficiario.Cuenta;
                    string nombre = beneficiario.RazonSocial.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                    string _referencia = cobro.Id.ToString().PadLeft(8, '0');
                    string debito = _tipodocumento + _documento + digito + tipocuenta + numerocuenta.PadLeft(35, '0') + nombre.PadRight(59) + _referencia;
                    _cobros.Add(debito);
                }
                else
                {
                    //if (k == 0)
                    //{
                    //    _cuenta = "01340373233733019371";
                    //    _nombrecomercial = "Carmelo Larez";
                    //    _rifc = "V018601098";
                    //}
                    //else if (k == 1)
                    //{
                    //    _cuenta = "01340874278743016046";
                    //    _nombrecomercial = "Alexyomar Istruriz";
                    //    _rifc = "V017302339";
                    //}
                    //else
                    //{
                    //    _cuenta = "01340373233733019371";
                    //    _nombrecomercial = "Carmelo Larez";
                    //    _rifc = "V018601098";

                    //}
                    //k++;
                }
       
            }
                       
            string[] lines = { registrodecontrol };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            string ruta = @"C:\Users\carmelo\Desktop\POLAR\CargoCuentaVerificacionBanesco\" + "afilia.txt." + fechaarchivo;
            System.IO.File.WriteAllLines(ruta, lines);
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("es-VE");
            //InstaTransfer.BLL.Concrete.URepository<AE_Archivo> archivoREPO = new InstaTransfer.BLL.Concrete.URepository<AE_Archivo>();
            //AE_Archivo _nuevo = new AE_Archivo();
            //_nuevo.FechaCreacion = DateTime.Now;
            //_nuevo.IdAE_avance = Cobros.FirstOrDefault().AE_Avance.Id;
            //_nuevo.Monto = Math.Round(Cobros.Sum(y => y.Monto), 2);
            //_nuevo.FechaEjecucion = DateTime.Now;
            //_nuevo.Ruta = ruta;
            //_nuevo.Valores = numeroorden;
            //_nuevo.ConsultaExitosa = false;
            //_nuevo.CorreoSoporteEnviado = false;
            //_nuevo.IdAE_ArchivosStatus = 1;
            //_nuevo.StatusChangeDate = DateTime.Now;
            //_nuevo.RutaRespuesta = "nada - escribo servicio COBRO DIARIO";
            //archivoREPO.AddEntity(_nuevo);
            //archivoREPO.SaveChanges();

            return true;
        }

        public bool GenerarValidacionCobroBanesco(List<AE_EstadoCuenta> Cobros)
        {
            int cantidadmovimientos = Cobros.Count();
            string fechaarchivo = DateTime.Now.AddDays(0).ToString("ddMMyy.hhmm");

            string _fecha = DateTime.Now.AddDays(0).ToString("yyyyMMdd");
            //datos fijos
            string registro = "00";
            //BUSCAR RIF BASE DE DATOS
            string rif = "J401878105";
            string Filler = "                          ";
            string Referencia = DateTime.Now.AddDays(0).ToString("yyyyMMddhhmm"); ;
            string documento = "AFILIA";
            string registrodecontrol = registro + rif + Filler + Referencia.PadRight(30) + documento;

            int k = 0;
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {
                //CP_Beneficiario beneficiario = CP_Beneficiario.GetAllRecords().Where(u => u.CodigoCliente == cobro.CodigoComercio).FirstOrDefault();

                //if (beneficiario != null && beneficiario.Id > 0)
                //{
                    //string _tipodocumento = cobro.AE_Avance.RifCommerce.Substring(0,1);
                    string _documento = cobro.AE_Avance.RifCommerce.PadLeft(10, '0');
                    string digito = "0";
                    string tipocuenta = "01";
                    string numerocuenta = Cobros.FirstOrDefault().AE_Avance.NumeroCuenta;
                    string nombre = cobro.AE_Avance.Commerce.SocialReasonName.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                    string _referencia = cobro.Id.ToString().PadLeft(8, '0');
                    string debito = _documento + digito + tipocuenta + numerocuenta.PadLeft(35, '0') + nombre.PadRight(59) + _referencia;
                    _cobros.Add(debito);
                //}
                //else
                //{
                    //if (k == 0)
                    //{
                    //    _cuenta = "01340373233733019371";
                    //    _nombrecomercial = "Carmelo Larez";
                    //    _rifc = "V018601098";
                    //}
                    //else if (k == 1)
                    //{
                    //    _cuenta = "01340874278743016046";
                    //    _nombrecomercial = "Alexyomar Istruriz";
                    //    _rifc = "V017302339";
                    //}
                    //else
                    //{
                    //    _cuenta = "01340373233733019371";
                    //    _nombrecomercial = "Carmelo Larez";
                    //    _rifc = "V018601098";

                    //}
                    //k++;
                //}

            }

            string[] lines = { registrodecontrol };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            string ruta = @"C:\Users\carmelo\Desktop\POLAR\CargoCuentaVerificacionBanesco\" + "afilia.txt." + fechaarchivo;
            System.IO.File.WriteAllLines(ruta, lines);

            CP_Archivo archivo = new CP_Archivo();
            archivo.IdEmpresa = 1;
            archivo.Nombre = "afilia.txt";
            archivo.Ruta = ruta;
            archivo.Tipo = 2;
            string contenido = "";
            foreach (var item in lines)
            {
                contenido = item + "</br>";
            }
            archivo.Contenido = contenido;
            archivo.Registro = cantidadmovimientos;
            archivo.FechaLectura = DateTime.Now;
            archivo.FechaCreacion = DateTime.Now;
            archivo.Descripcion = "[VERIFICACIÓN] verificacón de cobros la empresa Fin Pagos.";
            archivo.IdCP_Archivo = null;
            archivo.ReferenciaOrigen = "Estado de cuenta operaciones de prestamos";
            CP_ArchivoREPO.AddEntity(archivo);
            CP_ArchivoREPO.SaveChanges();

            //Thread.CurrentThread.CurrentCulture = new CultureInfo("es-VE");
            //InstaTransfer.BLL.Concrete.URepository<AE_Archivo> archivoREPO = new InstaTransfer.BLL.Concrete.URepository<AE_Archivo>();
            //AE_Archivo _nuevo = new AE_Archivo();
            //_nuevo.FechaCreacion = DateTime.Now;
            //_nuevo.IdAE_avance = Cobros.FirstOrDefault().AE_Avance.Id;
            //_nuevo.Monto = Math.Round(Cobros.Sum(y => y.Monto), 2);
            //_nuevo.FechaEjecucion = DateTime.Now;
            //_nuevo.Ruta = ruta;
            //_nuevo.Valores = numeroorden;
            //_nuevo.ConsultaExitosa = false;
            //_nuevo.CorreoSoporteEnviado = false;
            //_nuevo.IdAE_ArchivosStatus = 1;
            //_nuevo.StatusChangeDate = DateTime.Now;
            //_nuevo.RutaRespuesta = "nada - escribo servicio COBRO DIARIO";
            //archivoREPO.AddEntity(_nuevo);
            //archivoREPO.SaveChanges();

            return true;
        }

        public class Estructura
        {
            public string CodigoCliente { get; set; }
            public string DocumentoComercial { get; set; }
            public string Asociacion { get; set; }

            public string Departamento { get; set; }
            public string Monto { get; set; }

            public string Fecha { get; set; }

            public string Fechavencimiento { get; set; }

            public string TipoOperacion { get; set; }

            public string x { get; set; }

            public string Concepto { get; set; }

            public string Comentario { get; set; }



        }


        public class EstructuraSalidaBanescoDetalle
        {
            public string Trading { get; set; }
            public string Filler { get; set; }
            public string TipoRegistro { get; set; }
            public string Indicador { get; set; }
            public string NumeroReferencia { get; set; }
            public string Fecha { get; set; }
            public string Monto { get; set; }
            public string Moneda { get; set; }
            public string Rif { get; set; }
            public string NumeroCuenta { get; set; }
            public string BancoBeneficiario { get; set; }
            public string BancoBeneficiarioDescripcion { get; set; }
            public string CodigoAgencia { get; set; }
            public string NombreBeneficiario { get; set; }
            public string NumeroCliente { get; set; }
            public string FechaVencimiento { get; set; }
            public string NumeroSecuenciaArchivo { get; set; }
        }

        public class EstructuraSalidaBanescoEstatus
        {
            public string Trading { get; set; }
            public string Filler { get; set; }
            public string TipoRegistro { get; set; }
           
            public string _CodigoEstatus { get; set; }
            public string _Descripcion { get; set; }


        }

        public class EstructuraSalidaBanescoEncabezado
        {
            public string Trading { get; set; }
            public string Filler { get; set; }
            public string TipoRegistro { get; set; }

            public string __NumeroReferenciaRespuesta { get; set; }
            public string __FechaRespuesta { get; set; }
            public string __NumeroReferenciaOrdenPago { get; set; }
            public string __TipoOrdenPago { get; set; }
            public string __CodigoBancoEmisor { get; set; }
            public string __NombreBancoEmisor { get; set; }
            public string __CodgioEmpresaReceptoraBansta { get; set; }
            public string __DescripcionEmpresaReceptoraBansta { get; set; }

        }

        #endregion
    }
}
