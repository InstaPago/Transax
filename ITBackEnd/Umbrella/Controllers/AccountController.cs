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
using Microsoft.SqlServer.Server;
using FileHelpers;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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
        URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
        URepository<CP_ArchivoItem> CP_ArchivoItemRepo = new URepository<CP_ArchivoItem>();
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
            //UserManager.RemovePassword("aff48ec0-2e6e-45ab-86e6-57c7a82b24ef");
            //UserManager.AddPassword("aff48ec0-2e6e-45ab-86e6-57c7a82b24ef", "Transax##5");


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
            //aff48ec0 - 2e6e - 45ab - 86e6 - 57c7a82b24ef
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


        #region POLARINI
        public string moveAndDonwloadFilesIni()
        {
            string texto = "";
            try
            {
                var host = "10.148.174.215";
                var port = 5522;
                var username = "instapag";
                var password = "540144017";
                var remoteDirectory = "/IN";
                var backupDirectory = "/INbackup/";

                //string remoteDirectory = "/RemotePath/";
                string localDirectory = @"C:\Apps\Transax\Repo\Polar\Ini\";
                using (var sftp = new SftpClient(host, port, username, password))
                {
                    sftp.Connect();
                    texto = "conecto con el sftp | ";
                    if (sftp.IsConnected)
                    {
                        //Debug.WriteLine("I'm connected to the client");
                        if (sftp.Exists(sftp.WorkingDirectory + remoteDirectory))
                        {
                            texto = texto + "directorio existe" + sftp.WorkingDirectory + remoteDirectory + " | ";
                            //sftp.ChangeDirectory(sftp.WorkingDirectory + remoteDirectory);
                            //texto = texto + "cambie directorio" + sftp.WorkingDirectory + remoteDirectory + "| ";
                            texto = texto + "busco archivos en" + sftp.WorkingDirectory + remoteDirectory + " | ";
                            var files = sftp.ListDirectory(sftp.WorkingDirectory + remoteDirectory);
                            texto = texto + "busque archivos " + sftp.WorkingDirectory + remoteDirectory + "  -> " + files.Count().ToString() + " | ";
                            foreach (var file in files)
                            {
                                texto = texto + "encontre y recorro " + file.Name + " | ";
                                string remoteFileName = file.Name;
                                if ((!file.Name.StartsWith(".")) && file.Name.Contains("BANINI"))
                                {
                                    texto = texto + "encontre y recorro " + localDirectory + remoteFileName + " | ";
                                    try
                                    {
                                        using (Stream file1 = System.IO.File.OpenWrite(localDirectory + remoteFileName))
                                        {
                                            sftp.DownloadFile(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName, file1);
                                            texto = texto + "lo descargue | ";
                                            var inFile = sftp.Get(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName);
                                            texto = texto + "lo muevo " + sftp.WorkingDirectory + backupDirectory + " | ";
                                            inFile.MoveTo(sftp.WorkingDirectory + backupDirectory + remoteFileName);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        texto = texto + "intento descargar y fallo : " + e.Message + " | ";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                texto = texto + e.Message + "  " + e.StackTrace;
                return texto;
            }
            return texto;
        }

        /// <summary>
        /// METODO 1 LEE LA ESTRUCTURA DE POLAR
        /// </summary>
        /// <returns></returns>
        public bool INI_LecturaPolar()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\Carmelo\Desktop\POLAR\_INIPOLAR");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*"); //Getting Text files
            List<EstructuraINI> Lista = new List<EstructuraINI>();
            List<CP_INI> ListaINI = new List<CP_INI>();
            CP_Archivo item = new CP_Archivo();
            string rifreferencia = "";
            string nombrearchivo = "";
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("BANINI0001"))
                {
                    //ALIMENTOS POLAR  J000413126 - IBS :540133497
                    rifreferencia = "J000413126";
                    nombrearchivo = "BANERRORINI0001";

                }
                else if (file.Name.Contains("BANINI0002"))
                {
                    //CERVECERIA POLAR J000063729 - IBS:540130908
                    rifreferencia = "J000063729";
                    nombrearchivo = "BANERRORINI0002";
                }
                else if (file.Name.Contains("BANINI0600"))
                {
                    //PRODUCTOS EFE J000301255  - IBS: 540132787 
                    rifreferencia = "J000301255";
                    nombrearchivo = "BANERRORINI0600";
                }
                else if (file.Name.Contains("BANINI0100"))
                {
                    //PEPSI COLA J301370139 -IBS: 205903844 
                    rifreferencia = "J301370139";
                    nombrearchivo = "BANERRORINI0100";
                }



                string lineas = "";
                string[] lines = System.IO.File.ReadAllLines(file.FullName);

                // Display the file contents by using a foreach loop.
                //System.Console.WriteLine("Contents of WriteLines2.txt = ");
                foreach (string line in lines)
                {
                    lineas = lineas + line + "<br />";
                    // Use a tab to indent each line of the file.
                    string sep = "\t";
                    ////string[] splitContent = content.Split(sep.ToCharArray());
                    string[] valores = line.Split(sep.ToCharArray()).ToArray();

                    EstructuraINI _registro = new EstructuraINI();
                    _registro.Fecha = valores[0];
                    _registro.RifEmpresa = valores[1];
                    _registro.Departamento = valores[2];
                    _registro.CodigoCliente = valores[3];
                    _registro.RifCliente = valores[4];
                    _registro.CuentaBancaria = valores[5];
                    _registro.Nombre = valores[6];
                    int idestatus = 1;
                    if (_registro.CodigoCliente == "")
                    {
                        idestatus = 3;
                        _registro.CodigoError = "005";
                        _registro.ErrorDescripcion = "Rif Incorrecto";
                        _registro.RifCliente = "0";

                    }
                    else if (_registro.CodigoCliente == "")
                    {
                        idestatus = 3;
                        _registro.CodigoError = "005";
                        _registro.ErrorDescripcion = "Codigo Cliente Incorrecto";
                        _registro.RifCliente = "0";
                    }
                    else
                    {
                        var __item = Lista.Where(u => u.RifCliente == _registro.RifCliente
                        //&& u.CuentaBancaria == _registro.CuentaBancaria 
                        && u.CodigoCliente == _registro.CodigoCliente).ToList();

                        var result = CP_IniRepo.GetAllRecords().Where(u => u.RifCliente == _registro.RifCliente
                        //&& u.CuentaBancaria == _registro.CuentaBancaria
                        && u.Estatus == 2
                        && u.CodigoCliente == _registro.CodigoCliente
                        && _registro.Departamento == u.Departamento).FirstOrDefault();

                        if (_registro.CuentaBancaria.Length != 20)
                        {
                            idestatus = 3;
                            _registro.CodigoError = "004";
                            _registro.ErrorDescripcion = "Error en cuenta";
                        }
                        else if (__item != null && __item.Count > 0)
                        {

                            idestatus = 3;
                            _registro.CodigoError = "002";
                            _registro.ErrorDescripcion = "Cliente Duplicado";
                            foreach (var elemento in __item)
                            {
                                elemento.CodigoError = "002";
                                elemento.ErrorDescripcion = "Cliente Duplicado";
                                var ele = ListaINI.Where(u => u.RifCliente == elemento.RifCliente && u.CuentaBancaria == elemento.CuentaBancaria && u.CodigoCliente == elemento.CodigoCliente).ToList();
                                foreach (var elementodos in ele)
                                {
                                    elementodos.Estatus = 3;
                                    elementodos.CodigoError = "002";
                                    elementodos.DescripcionError = "Cliente Duplicado";

                                }
                            }
                        }
                        else if (result != null && result.Id > 0)
                        {
                            idestatus = 3;
                            _registro.CodigoError = "006";
                            _registro.ErrorDescripcion = "Cliente con carga previa";
                        }
                        else
                        {
                            _registro.CodigoError = "";
                            _registro.ErrorDescripcion = "";
                        }
                    }

                    Lista.Add(_registro);
                    CP_INI _Ini = new CP_INI();
                    _Ini.CodigoError = _registro.CodigoError;
                    _Ini.DescripcionError = _registro.ErrorDescripcion;
                    _Ini.Fecha = valores[0];
                    _Ini.RifEmpresa = valores[1];
                    _Ini.Departamento = valores[2];
                    _Ini.CodigoCliente = valores[3];
                    _Ini.ActivoCLI = false;
                    if (valores[4].Length > 2)
                    {
                        string _tipodocumento = valores[4].Substring(0, 1).ToString();
                        string _documento = valores[4].Substring(1, (valores[4].Length - 1)).PadLeft(9, '0');
                        _Ini.RifCliente = _tipodocumento + _documento;
                    }
                    else
                    {
                        _Ini.CodigoError = "003";
                        _Ini.DescripcionError = "Error rif no valido";
                    }
                    //_Ini.RifCliente = valores[4];
                    _Ini.CuentaBancaria = valores[5];
                    _Ini.Nombre = valores[6];

                    _Ini.Estatus = idestatus;
                    _Ini.FechaCreacion = DateTime.Now;
                    _Ini.FechaActualizacion = DateTime.Now;
                    if (_Ini.RifCliente == null || _Ini.RifCliente == "")
                        _Ini.RifCliente = "-";
                    if (_Ini.CodigoCliente == null || _Ini.CodigoCliente == "")
                        _Ini.CodigoCliente = "-";
                    ListaINI.Add(_Ini);
                    //CP_Beneficiario Beneficiario = new InstaTransfer.DataAccess.CP_Beneficiario();
                    //Beneficiario.CodigoCliente = valores[0];
                    //Beneficiario.CodigoMaster = valores[1];
                    //Beneficiario.PermisoMaster = valores[2];
                    //Beneficiario.Nombre = valores[3];
                    //Beneficiario.RazonSocial = valores[6];
                    //Beneficiario.Identificacion = valores[5];
                    //Beneficiario.IdentificacionPagador = valores[5];
                    //Beneficiario.Region = "";
                    //Beneficiario.CorreoElectronico = "";
                    //Beneficiario.Telefono = "";
                    //Beneficiario.TipoIdentificacion = valores[9];
                    //Beneficiario.CodigoEstatus = "";
                    //Beneficiario.PorcentajeIR_IVA = "";
                    //Beneficiario.CodigoDepartamento = valores[2];
                    //Beneficiario.Cuenta = valores[5];
                    //Beneficiario.FechaCreacion = DateTime.Now;
                    //Beneficiario.FechaUltimaActualizacion = DateTime.Now;
                    //CP_Beneficiario.AddEntity(Beneficiario);

                }
                string idarchivo = "";
                bool sinres = false;
                if (Lista.Where(u => u.CodigoError == "").Count() > 0)
                {
                    INI_GenerarAfiliaBanesco(Lista.Where(u => u.CodigoError == "").ToList(), rifreferencia, out idarchivo);
                }
                else
                {

                    sinres = true;

                }
                //CP_Beneficiario.SaveChanges();
                item.Nombre = file.Name;
                item.Ruta = file.FullName;
                item.Registro = 0;
                item.ReferenciaOrigen = "";
                item.ReferenciaArchivoBanco = idarchivo;
                item.FechaCreacion = DateTime.Now;
                item.FechaLectura = DateTime.Now;
                item.Contenido = lineas;
                item.Descripcion = "Lectura Archivo Porlar INI";
                item.Tipo = 1;
                item.EsRespuesta = false;
                item.ContenidoRespuesta = "";
                ArchivoREPO.AddEntity(item);
                ArchivoREPO.SaveChanges();

                foreach (var items in ListaINI)
                {
                    items.IdOrigen = item.Id;
                    CP_IniRepo.AddEntity(items);

                }
                CP_IniRepo.SaveChanges();

                if (sinres)
                {
                    INI_GeneraRespuestaPolar(ListaINI, nombrearchivo);
                }

            }
            return true;
        }

        /// <summary>
        /// METODO 2 GENERA ARCHIVO AFILIA.TXT PARA BANESCO Y LO SUBE AL SFTP
        /// </summary>
        /// <param name="Cobros"></param>
        /// <param name="idarchivo"></param>
        /// <returns></returns>
        public bool INI_GenerarAfiliaBanesco(List<EstructuraINI> Cobros, string rifreferencia, out string idarchivo)
        {
            int cantidadmovimientos = Cobros.Count();
            string fechaarchivo = DateTime.Now.AddDays(0).ToString("ddMMyy.hhmm");

            string _fecha = DateTime.Now.AddDays(0).ToString("yyyyMMdd");
            //datos fijos
            string registro = "00";

            //BUSCAR RIF BASE DE DATOS
            string rif = rifreferencia;
            string Filler = "                          ";
            string Referencia = DateTime.Now.AddDays(0).ToString("ddMMyyyyhhmm");
            idarchivo = Referencia;
            string documento = "AFILIA";
            string registrodecontrol = registro + rif + Filler + Referencia.PadRight(30) + documento;

            int k = 0;

            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {

                string _art = cobro.RifCliente.Substring(1, (cobro.RifCliente.Length - 1));
                string _tipodocumento = cobro.RifCliente.Substring(0, 1).ToString();
                string _documento = _art.PadLeft(10, '0');
                string digito = "0";
                string tipocuenta = "01";
                string numerocuenta = cobro.CuentaBancaria;
                string nombre = cobro.Nombre.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                string _referencia = cobro.CodigoCliente.ToString().PadLeft(8, '0');
                string debito = _tipodocumento + _documento + digito + tipocuenta + numerocuenta.PadLeft(35, '0') + nombre.PadRight(59) + _referencia;
                _cobros.Add(debito);

            }

            string[] lines = { registrodecontrol };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            string ruta = @"C:\Users\carmelo\Desktop\POLAR\_BANESCOSALIDAAFILIA\" + "afilia." + rifreferencia + ".txt";
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

        /// <summary>
        /// METODO 3 LEE SALIDA ARCHIVO AFILIA BANESCO
        /// </summary>
        /// <returns></returns>
        public bool INI_LecturaRespuestaBanesco()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\Carmelo\Desktop\POLAR\_BANESCOSALIDAAFILIA\");
            //Assuming Test is your Folder
            string rutafinal = @"C:\Users\Carmelo\Desktop\POLAR\SalidaINIBackup\";
            string nombrearchivo = "";
            FileInfo[] Files = d.GetFiles("*"); //Getting Text files

            CP_Archivo item = new CP_Archivo();
            CP_Archivo getCP = new CP_Archivo();
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("RAFIL"))
                {
                    if (file.Name.Contains("540133497RAFIL"))
                    {
                        //ALIMENTOS POLAR  J000413126 - IBS :540133497
                        nombrearchivo = "BANERRORINI0001";

                    }
                    else if (file.Name.Contains("540130908RAFIL"))
                    {
                        //CERVECERIA POLAR J000063729 - IBS:540130908
                        nombrearchivo = "BANERRORINI0002";
                    }
                    else if (file.Name.Contains("540132787RAFIL"))
                    {
                        //PRODUCTOS EFE J000301255  - IBS: 540132787 
                        nombrearchivo = "BANERRORINI0600";
                    }
                    else if (file.Name.Contains("205903844RAFIL"))
                    {
                        //PEPSI COLA J301370139 -IBS: 205903844 
                        nombrearchivo = "BANERRORINI0100";
                    }
                    //item.Nombre = file.Name;
                    //item.Ruta = file.FullName;
                    //item.FechaLectura = DateTime.Now;
                    string lineas = "";
                    int i = 1;
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
                    List<CP_INI> ListCP = new List<CP_INI>();
                    foreach (string line in lines)
                    {

                        lineas = lineas + line + "<br />";
                        if (i == 1)
                        {
                            string __NumeroReferenciaRespuesta = line.Substring(37, 14).ToString().TrimEnd();
                            getCP = ArchivoREPO.GetAllRecords().Where(u => u.ReferenciaArchivoBanco == __NumeroReferenciaRespuesta).FirstOrDefault();
                            if (getCP != null && getCP.Id > 0)
                                ListCP = CP_IniRepo.GetAllRecords().Where(u => u.IdOrigen == getCP.Id).ToList();
                        }
                        else
                        {
                            var identificacion = line.Substring(0, 1).ToString() + line.Substring(2, 9).ToString();
                            var respuesta = line.Substring(11, 1).ToString().TrimEnd();
                            var tipocuenta = line.Substring(12, 2).ToString().TrimEnd();
                            var cuenta = line.Substring(14, 35).ToString().TrimStart(new Char[] { '0' });
                            cuenta = "0" + cuenta;
                            var nombre = line.Substring(49, 59).ToString().TrimEnd();
                            var result = ListCP.Where(u => u.RifCliente.Trim() == identificacion.Trim() && u.CuentaBancaria.Trim() == cuenta.Trim() && u.Estatus == 1).FirstOrDefault();
                            if (result == null)
                            {
                                var _identificacion = line.Substring(0, 1).ToString() + line.Substring(1, 9).ToString();
                                result = ListCP.Where(u => u.RifCliente.Trim() == _identificacion.Trim() && u.CuentaBancaria.Trim() == cuenta.Trim() && u.Estatus == 1).FirstOrDefault();
                            }
                            if (result != null && result.Id > 0)
                            {
                                if (respuesta == "1")
                                {
                                    result.CodigoError = "001";
                                    result.DescripcionError = "Correcto";
                                    result.Estatus = 2;

                                }
                                else if (respuesta == "3")
                                {
                                    result.CodigoError = "005";
                                    result.DescripcionError = "Cuenta no pertenece al RIF";
                                    result.Estatus = 3;
                                }
                                else if (respuesta == "2")
                                {
                                    result.CodigoError = "003";
                                    result.DescripcionError = "Cuenta bancaria no corresponde";
                                    result.Estatus = 3;
                                }
                                else if (respuesta == "4")
                                {
                                    result.CodigoError = "004";
                                    result.DescripcionError = "Cuenta conjunta";
                                    result.Estatus = 3;
                                }

                                CP_IniRepo.SaveChanges();
                            }

                        }
                        i++;
                    }


                    INI_GeneraRespuestaPolar(ListCP.Where(u => u.CodigoError != "001").ToList(), nombrearchivo);

                    //getCP.FechaLectura = DateTime.Now;najada
                    //getCP.ContenidoRespuesta = lineas;
                    //getCP.EsRespuesta = true;
                    //ArchivoREPO.SaveChanges();
                    file.MoveTo(rutafinal + file.Name + ".txt");
                    //item.Contenido = lineas;
                    //item.Descripcion = "[FINPAGO] RESPUESTA Cargo cuenta masivo.";
                    //item.Tipo = 1;
                    //item.IdEmpresa = 1;
                    //item.IdReferencia = Guid.NewGuid();
                    //item.

                    //ArchivoREPO.AddEntity(item);
                }
                else
                {

                    file.MoveTo(rutafinal + file.Name + ".txt");
                }


            }
            return true;
        }

        /// <summary>
        /// METODO 4  CREA ARCHIVO PARA POLAR REPSUESTA FIN DEL CICLO
        /// </summary>
        /// <param name="Cobros"></param>
        /// <returns></returns>
        public bool INI_GeneraRespuestaPolar(List<CP_INI> Cobros, string nombrearchivo)
        {
            int k = 0;
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            string sep = "\t";
            foreach (var cobro in Cobros)
            {

                string fecha = cobro.Fecha + sep;
                string Rifempresa = cobro.RifEmpresa + sep;
                string Departamento = cobro.Departamento + sep;
                string Cliente = cobro.CodigoCliente + sep;
                string RifCliente = cobro.RifCliente + sep;
                string Cuenta = cobro.CuentaBancaria + sep;
                string NombreCliente = cobro.Nombre + sep;
                string cod = cobro.CodigoError + sep;
                string error = cobro.DescripcionError;
                string debito = fecha + Rifempresa + Departamento + Cliente + RifCliente + Cuenta + NombreCliente + cod + error;
                _cobros.Add(debito);

            }
            if (_cobros.Count > 0)
            {
                string[] lines = { };
                foreach (var _item in _cobros)
                {
                    Array.Resize(ref lines, lines.Length + 1);
                    lines[lines.Length - 1] = _item;
                }

                // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
                // and then closes the file.  You do NOT need to call Flush() or Close().
                string ruta = @"C:\Users\Carmelo\Desktop\POLAR\_INIERRORPOLAR\" + nombrearchivo;
                System.IO.File.WriteAllLines(ruta, lines);
            }
            else
            {

                string line = "Sin Error";
                // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
                // and then closes the file.  You do NOT need to call Flush() or Close().
                string ruta = @"C:\Users\Carmelo\Desktop\POLAR\_INIERRORPOLAR\" + nombrearchivo;
                System.IO.File.WriteAllText(ruta, line);

            }
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
        #endregion

        #region CLI
        /// <summary>
        /// LEE ARCHIVO CLI PARA ACTIVAR O DESACTIVAR EN LA BASE DE DATOS
        /// </summary>
        /// <returns></returns>
        public bool CLI_lecturaPolar()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\carmelo\Desktop\POLAR\_CLIPOLAR");
            FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
            string nombrearchivo = "";
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("CLI0001"))
                {
                    //ALIMENTOS POLAR  J000413126 - IBS :540133497

                    nombrearchivo = "BANERRORCLI0001";

                }
                else if (file.Name.Contains("CLI0002"))
                {
                    //CERVECERIA POLAR J000063729 - IBS:540130908

                    nombrearchivo = "BANERRORCLI0002";
                }
                else if (file.Name.Contains("CLI0600"))
                {
                    //PRODUCTOS EFE J000301255  - IBS: 540132787 

                    nombrearchivo = "BANERRORCLI0600";
                }
                else if (file.Name.Contains("CLI0100"))
                {
                    //PEPSI COLA J301370139 -IBS: 205903844 

                    nombrearchivo = "BANERRORCLI0100";
                }

                List<string> errores = new List<string>();
                bool vencido = false;
                string[] lines = System.IO.File.ReadAllLines(file.FullName);
                int ultimalinea = lines.Count();
                var contenidoultimalinea = lines[ultimalinea - 1];
                string fechaarchivo = contenidoultimalinea.Substring(0, 4) + "/" + contenidoultimalinea.Substring(4, 2) + "/" + contenidoultimalinea.Substring(6, 2);
                if (DateTime.Now > DateTime.Parse(fechaarchivo).AddHours(48))
                {
                    vencido = true;

                }
                if (!vencido)
                {
                    //var engine = new FileHelperEngine<_EstructuraCLI>();
                    //var result = engine.ReadFile(file.FullName);
                    int j = 0;
                    foreach (var _item in lines)
                    {
                        if (j != (ultimalinea - 1))
                        {
                            var elemento = _item.Split('|');
                            if (elemento.Count() < 13)
                            {
                                string lineafinal = _item + "|0002|Formato Errado";
                                errores.Add(lineafinal);
                            }
                            else
                            {

                                CP_INI beneficiario = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == elemento[0] && u.Departamento == elemento[12] && u.Estatus == 2).FirstOrDefault();
                                if (beneficiario != null && beneficiario.Id > 0)
                                {
                                    beneficiario.ActivoCLI = elemento[10] == "0" ? false : true;
                                    beneficiario.FechaVencimiento = elemento[16] == "1" ? true : false;
                                    CP_IniRepo.SaveChanges();
                                }
                                else
                                {
                                    string lineafinal = _item + "|0001|Cliente sin carga Incial INI";
                                    errores.Add(lineafinal);
                                }

                            }
                        }
                        j++;
                    }

                    if (errores.Count > 0)
                    {
                        string[] _lines = { };
                        foreach (var _item in errores)
                        {
                            Array.Resize(ref _lines, _lines.Length + 1);
                            _lines[_lines.Length - 1] = _item;
                        }
                        string ruta = @"C:\Users\carmelo\Desktop\POLAR\_CLIERROR\" + nombrearchivo + ".txt";
                        System.IO.File.WriteAllLines(ruta, _lines);

                    }
                    else
                    {

                        string line = "Sin Errores";
                        string ruta = @"C:\Users\carmelo\Desktop\POLAR\_CLIERROR\" + nombrearchivo + ".txt";
                        System.IO.File.WriteAllText(ruta, line);

                    }
                }
                else
                {

                    int j = 0;
                    foreach (var _item in lines)
                    {
                        var elemento = _item.Split('|');
                        if (j != (ultimalinea - 1))
                        {

                            string lineafinal = _item + "|0001|Error Documento Fecha";
                            errores.Add(lineafinal);

                        }
                        j++;
                    }

                    if (errores.Count > 0)
                    {
                        string[] _lines = { };
                        foreach (var _item in errores)
                        {
                            Array.Resize(ref _lines, _lines.Length + 1);
                            _lines[_lines.Length - 1] = _item;
                        }
                        string ruta = @"C:\Users\carmelo\Desktop\POLAR\_CLIERROR\" + nombrearchivo + ".txt";
                        System.IO.File.WriteAllLines(ruta, _lines);

                    }
                    else
                    {
                        string line = "Sin Errores";
                        string ruta = @"C:\Users\carmelo\Desktop\POLAR\_CLIERROR\" + nombrearchivo + ".txt";
                        System.IO.File.WriteAllText(ruta, line);
                    }
                }

            }
            return true;
        }

        #endregion

        #region POLARCOB
        public bool COB_LecturaPOLAR()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\CarmeloLarez\POLAR\_COBPOLAR");//Assuming Test is your Folder
            List<CP_ArchivoItem> ListItemsFile = new List<CP_ArchivoItem>();
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            string referencia = "";
            string asociado = "";
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("BANCOB"))
                {
                    List<EstructuraCOB> Lista = new List<EstructuraCOB>();
                    CP_Archivo item = new CP_Archivo();
                    if (file.Name.Contains("BANCOB0001"))
                    {
                        asociado = "540133497";
                        //ALIMENTOS POLAR  J000413126 - IBS :540133497
                        //rifreferencia = "J000413126";
                        referencia = "0001";

                    }
                    else if (file.Name.Contains("BANCOB0002"))
                    {
                        asociado = "540130908";

                        //CERVECERIA POLAR J000063729 - IBS:540130908
                        //rifreferencia = "J000063729";
                        referencia = "0002";
                    }
                    else if (file.Name.Contains("BANCOB0600"))
                    {
                        asociado = "540132787";
                        //PRODUCTOS EFE J000301255  - IBS: 540132787 
                        //rifreferencia = "J000301255";
                        referencia = "0600";
                    }
                    else if (file.Name.Contains("BANCOB0100"))
                    {
                        asociado = "205903844";
                        //PEPSI COLA J301370139 -IBS: 205903844 
                        //rifreferencia = "J301370139";
                        referencia = "0100";

                    }

                    bool vencido = false;
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
                    int ultimalinea = lines.Count();
                    var contenidoultimalinea = lines[ultimalinea - 1];
                    string fechaarchivo = contenidoultimalinea.Substring(0, 4) + "/" + contenidoultimalinea.Substring(4, 2) + "/" + contenidoultimalinea.Substring(6, 2);
                    if (DateTime.Now > DateTime.Parse(fechaarchivo).AddHours(23))
                    {
                        vencido = true;

                    }

                    List<_EstructuraCOB> Archivo = new List<_EstructuraCOB>();
                    List<CP_ArchivoItem> CPITEMS = new List<CP_ArchivoItem>();
                    var engine = new FileHelperEngine<_EstructuraCOB>();
                    var result = engine.ReadFile(file.FullName);
                    int j = 0;
                    foreach (_EstructuraCOB _item in result)
                    {
                        if (j != (ultimalinea - 1))
                        {

                            Archivo.Add(_item);
                            CP_ArchivoItem _AI = new CP_ArchivoItem();
                            _AI.CodigoCliente = _item.CodigoCliente;
                            _AI.CodigoDepartamento = _item.CodigoDepartamento;
                            _AI.Datetime = DateTime.Now;
                            _AI.DocumentoComercial = _item.DocumentoComercial;
                            _AI.Estatus = 1;
                            _AI.FechaEmision = _item.FechaEmision;
                            _AI.FechaVencimiento = _item.Fechavencimiento;
                            _AI.Monto = _item.Monto;
                            _AI.MontoIVA = _item.MontoIva;
                            _AI.OBS = _item.OBS;
                            _AI.Referencia = _item.Referencia;
                            _AI.TipoDocumento = _item.TipoDocumento;


                            CPITEMS.Add(_AI);

                            //CP_ArchivoItemRepo.AddEntity(_AI);
                        }
                        j++;
                    }
                    //RONDA DE ERRORES BASICOS
                    foreach (var __item in CPITEMS)
                    {
                        if (__item.DescripcionError == null)
                        {
                            if (vencido)
                            {
                                __item.CodigoError = 001;
                                __item.DescripcionError = "ERROR EN FECHA DE ARCHIVO";
                                goto Finish;
                            }

                            if (__item.TipoDocumento == "04")
                            {
                                __item.CodigoError = 004;
                                __item.DescripcionError = "RETENCION - NO PROCESADO";
                                goto Finish;
                            }

                            //falta agregar que esta activo en 
                            CP_INI beneficiario = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == __item.CodigoCliente
                            && u.Departamento == __item.CodigoDepartamento
                            && u.Estatus == 2).FirstOrDefault();

                            if (beneficiario != null && beneficiario.Id > 0)
                            {
                                //ADICIONA LESTA ACTIVO POR CLI
                                if (!beneficiario.ActivoCLI)
                                {
                                    __item.CodigoError = 005;
                                    __item.DescripcionError = "CLIENTE INACTIVO PARA COBRO";
                                    goto Finish;
                                }

                            }
                            else
                            {
                                //092 nota sin factura 028 cuenta beneficiario invalidad 016 cuenta bloqueada 070 monto invalido
                                __item.CodigoError = 006;
                                __item.DescripcionError = "CLIENTE SIN CARGA PREVIA";
                                goto Finish;
                            }


                            //aqui validacion de flag nuevo agregado en el clic
                            if (DateTime.Now.Date <= DateTime.Parse(__item.FechaVencimiento) && beneficiario.FechaVencimiento)
                            {
                                __item.CodigoError = 003;
                                __item.DescripcionError = "REGISTRO NO VENCIDO";
                                goto Finish;
                            }


                            Regex regex = new Regex(@"^[0-9]{0,15}$");
                            if (__item.TipoDocumento == "01" || __item.TipoDocumento == "03")
                            {
                                //if (!regex.IsMatch(__item.DocumentoComercial) || __item.DocumentoComercial.ToString().Length < 1)
                                //{
                                //    __item.CodigoError = 073;
                                //    __item.DescripcionError = "NUMERO DE DOCUMENTO NO VALIDO";
                                //    goto Finish;
                                //}

                                //debo ir base de datos y ver si ya fue cobrado
                                var duplicado = EstadoCuentaREPO.GetAllRecords().Where(u => u.CodigoComercio == beneficiario.CodigoCliente && u.NumeroDocumento == __item.DocumentoComercial && u.Estatus == 2).ToList();
                                if (duplicado.Count() > 1)
                                {
                                    foreach (var objeto in duplicado)
                                    {
                                        __item.CodigoError = 002;
                                        __item.DescripcionError = "DOCUMENTO DUPLICADO - COBRO REALIZADO";
                                        goto Finish;
                                    }
                                }
                            }

                            var resultduplciado = CPITEMS.Where(u => u.DocumentoComercial == __item.DocumentoComercial && u.Referencia == __item.Referencia).ToList();

                            if (resultduplciado.Count() > 1)
                            {
                                foreach (var objeto in resultduplciado)
                                {
                                    __item.CodigoError = 002;
                                    __item.DescripcionError = "DOCUMENTO DUPLICADO";
                                    goto Finish;
                                }
                            }



                            if (__item.TipoDocumento == "02")
                            {
                                var reusltfacutra = CPITEMS.Where(u => u.DocumentoComercial
                                == __item.Referencia && u.Referencia
                                == "FACTURA").ToList();

                                if (reusltfacutra.Count() >= 1)
                                {

                                }
                                else
                                {
                                    __item.CodigoError = 092;
                                    __item.DescripcionError = "NOTA DE CREDITO SIN DOCUMENTO";
                                    goto Finish;
                                }

                            }


                        }
                        Finish:
                        string hola = "";
                    }


                    //var groupedCustomerList1 = CPITEMS.GroupBy(u => u.DocumentoComercial).Select(grp => grp.ToList()).ToList();
                    //SEGUNDA RONDA VALIDAR MONTOS
                    List<CP_ArchivoItem> ListaConsultaI = new List<CP_ArchivoItem>();

                    foreach (var item2 in CPITEMS.Where(u => u.DescripcionError == null).ToList())
                    {
                        bool _win = true;
                        decimal _Total = 0;
                        decimal _Debito = 0;
                        decimal _Credito = 0;
                        string _debito = "";
                        string _credito = "";
                        string _comercio = "";

                        List<CP_ArchivoItem> list = new List<CP_ArchivoItem>();
                        //list.Add(_row);

                        list = CPITEMS.Where(u => (u.DocumentoComercial == item2.DocumentoComercial || u.Referencia == item2.DocumentoComercial
                        || u.DocumentoComercial == item2.Referencia) && u.DescripcionError == null).ToList();

                        foreach (var row in list)
                        {

                            if (!ListaConsultaI.Any(u => u.DocumentoComercial == row.DocumentoComercial && u.Referencia == row.Referencia))
                            {
                                try
                                {
                                    _comercio = row.CodigoCliente;
                                    if (row.TipoDocumento == "02")
                                    {
                                        _Credito = _Credito + decimal.Parse(row.Monto.Replace('.', ','));
                                        _credito = _credito + " " + row.DocumentoComercial + " " + row.FechaEmision + " NOTA CREDITO" + " | ";
                                    }
                                    else
                                    {

                                        switch (row.TipoDocumento)
                                        {
                                            case "01":
                                                _Debito = _Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                                _debito = _debito + " " + row.DocumentoComercial + " " + row.FechaEmision + "FACTURA" + " | ";
                                                break;
                                            case "03":
                                                _Debito = _Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                                _debito = _debito + " " + row.DocumentoComercial + " " + row.FechaEmision + "NOTA DEBITO" + " | ";
                                                break;
                                            case "04":
                                                _Debito = _Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                                _debito = _debito + " " + row.DocumentoComercial + " " + row.FechaEmision + "FACTURA" + " | ";
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
                                ListaConsultaI.Add(row);
                            }
                            else
                            {
                                _win = false;
                            }

                        }

                        if (_win)
                        {
                            //_Total = _Debito - _Credito;

                            if (_Credito > _Debito)
                            {
                                foreach (var row in list)
                                {
                                    row.DescripcionError = "SALDO A FAVOR DEL CLIENTE";
                                    row.CodigoError = 070;

                                }
                            }
                            else if (_Credito == _Debito)
                            {
                                foreach (var row in list)
                                {
                                    row.DescripcionError = "MONTO INVALIDO";
                                    row.CodigoError = 071;

                                }

                            }
                            else
                            {

                            }
                        }

                    }

                    ERRORCOB_GenerarPOLAR(CPITEMS.Where(u => u.CodigoError != null).ToList(), referencia);

                    item.Registro = 0;
                    item.ReferenciaOrigen = "";
                    item.ReferenciaArchivoBanco = "";
                    item.FechaCreacion = DateTime.Now;
                    item.FechaLectura = DateTime.Now;
                    item.Contenido = lineas;
                    item.Descripcion = "Lectura Archivo Porlar COB";
                    item.Tipo = 1;
                    item.EsRespuesta = false;
                    item.ContenidoRespuesta = "";


                    CPITEMS = CPITEMS.Where(u => u.CodigoError == null).ToList();
                    int largo = CPITEMS.Count();
                    int i = 0;
                    //var groupedCustomerList = CPITEMS.GroupBy(u => u.DocumentoComercial).Select(grp => grp.ToList()).ToList();

                    item.Contenido = lineas;
                    item.Descripcion = "Archivo Cobro Polar - Comercios Evaluado:" + CPITEMS.Count();
                    item.Tipo = 2;
                    ArchivoREPO.AddEntity(item);
                    ArchivoREPO.SaveChanges();
                    List<CP_ArchivoItem> ListaConsulta = new List<CP_ArchivoItem>();
                    List<CP_ArchivoEstadoCuenta> ListaEstado = new List<CP_ArchivoEstadoCuenta>();
                    //Generar Estado de Cuenta
                    foreach (var _row in CPITEMS)
                    {
                        CP_ArchivoEstadoCuenta EstadoCuenta = new CP_ArchivoEstadoCuenta();
                        bool _win = true;
                        decimal Total = 0;
                        decimal Debito = 0;
                        decimal Credito = 0;
                        string debito = "";
                        string credito = "";
                        string comercio = "";
                        string tipodocumento = "";
                        string numerodocumento = "";
                        string fechavencimiento = "";
                        string fechapago = "";

                        List<CP_ArchivoItem> list = new List<CP_ArchivoItem>();
                        //list.Add(_row);

                        list = CPITEMS.Where(u => (u.DocumentoComercial == _row.DocumentoComercial || u.Referencia == _row.DocumentoComercial
                                || u.DocumentoComercial == _row.Referencia) && u.DescripcionError == null).ToList();

                        foreach (var row in list)
                        {
                            if (!ListaConsulta.Any(u => u.DocumentoComercial == row.DocumentoComercial && u.Referencia == row.Referencia))
                            {
                                try
                                {
                                    comercio = row.CodigoCliente;
                                    if (row.TipoDocumento == "02")
                                    {
                                        Credito = Credito + decimal.Parse(row.Monto.Replace('.', ','));
                                        credito = credito + " " + row.DocumentoComercial + ";" + row.Monto + ";" + row.FechaEmision + ";" + row.FechaVencimiento + ";" + row.Referencia + ";" + "NC" + " | ";
                                    }
                                    else
                                    {

                                        switch (row.TipoDocumento)
                                        {
                                            case "01":
                                                Debito = Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                                debito = debito + " " + row.DocumentoComercial + ";" + row.Monto + ";" + row.FechaEmision + ";" + row.FechaVencimiento + ";" + row.Referencia + ";" + "FACTURA" + " | ";
                                                tipodocumento = "FACTURA";
                                                numerodocumento = row.DocumentoComercial;
                                                fechavencimiento = row.FechaVencimiento;
                                                fechapago = row.FechaEmision;
                                                break;
                                            case "03":
                                                Debito = Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                                debito = debito + " " + row.DocumentoComercial + ";" + row.Monto + ";" + row.FechaEmision + ";" + row.FechaVencimiento + ";" + row.Referencia + ";" + "ND" + " | ";
                                                tipodocumento = "NOTADEBITO";
                                                numerodocumento = row.DocumentoComercial;
                                                fechavencimiento = row.FechaVencimiento;
                                                fechapago = row.FechaEmision;
                                                break;
                                            case "04":
                                                Debito = Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                                debito = debito + " " + row.DocumentoComercial + ";" + row.Monto + ";" + row.FechaEmision + ";" + row.FechaVencimiento + ";" + row.Referencia + ";" + "FACTURA" + " | ";
                                                break;

                                            default:
                                                break;
                                        }
                                    }

                                    ListaConsulta.Add(row);
                                }
                                catch
                                {
                                    //crear log de error y guardar en base de datos
                                }
                            }
                            else
                            {
                                _win = false;
                            }
                        }
                        if (_win)
                        {
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
                            EstadoCuenta.TipoDocumento = tipodocumento;
                            EstadoCuenta.NumeroDocumento = numerodocumento;
                            EstadoCuenta.FechaVencimiento = fechavencimiento;
                            EstadoCuenta.FechaPago = fechapago;


                            if (EstadoCuenta.Total > 0)
                            {
                                EstadoCuenta.Estatus = 2;
                            }
                            else
                            {
                                EstadoCuenta.Estatus = 1;
                            }
                            EstadoCuentaREPO.AddEntity(EstadoCuenta);

                        }




                    }
                    EstadoCuentaREPO.SaveChanges();
                    ListaEstado = EstadoCuentaREPO.GetAllRecords().Where(u => u.ArchivoLecturaPolar == item.Id && u.Estatus == 1).ToList();
                    if (!vencido)
                    {
                        if (ListaEstado.Count > 0)
                            COB_GenerarCobroBanesco(ListaEstado, asociado);
                        else
                            ERRORCOB_GenerarPOLAR(CPITEMS.Where(u => u.CodigoError != null).ToList(), referencia);
                    }


                }
            }
            return true;
        }

        public bool ERRORCOB_GenerarPOLAR(List<CP_ArchivoItem> _items, string codigo)
        {
            if (_items.Count > 0)
            {
                List<string> _cobros = new List<string>();
                int i = 0;
                foreach (var cobro in _items)
                {
                    if (i <= (_items.Count() - 1))
                    {
                        string item = cobro.CodigoError.ToString().PadLeft(3, '0').ToString() + "|" +
                            cobro.CodigoCliente + "|" +
                            cobro.DocumentoComercial + "|" +
                            cobro.Referencia + "|" +
                            cobro.CodigoDepartamento + "|" +
                            cobro.Monto + "|" +
                            cobro.FechaEmision + "|" +
                            cobro.FechaVencimiento + "|" +
                            cobro.TipoDocumento + "|" +
                            cobro.MontoIVA + "|" + (cobro.OBS == "" ? "X" : cobro.OBS) + "|" +
                            cobro.DescripcionError;

                        _cobros.Add(item);
                    }
                    i++;
                }

                if (_cobros.Count() > 0)
                {
                    string[] lines = { };

                    foreach (var _item in _cobros)
                    {
                        Array.Resize(ref lines, lines.Length + 1);
                        lines[lines.Length - 1] = _item;
                    }
                    string ruta = @"C:\Users\carmelo\Desktop\POLAR\_COBERRORPOLAR\" + "BANERRORCOB" + codigo + ".txt";
                    System.IO.File.WriteAllLines(ruta, lines);
                }
                else
                {

                    string line = "Sin Errores";
                    string ruta = @"C:\Users\carmelo\Desktop\POLAR\_COBERRORPOLAR\" + "BANERRORCOB" + codigo + ".txt";
                    System.IO.File.WriteAllText(ruta, line);

                }
                return true;
            }
            else {
                string line = "Sin Cobros";
                string ruta = @"C:\Users\carmelo\Desktop\POLAR\_COBERRORPOLAR\" + "BANPAG" + codigo + ".txt";
                System.IO.File.WriteAllText(ruta, line);
                return true;
            }
        }

        //public bool COB_LecturaPOLAR()
        //{
        //    DirectoryInfo d = new DirectoryInfo(@"C:\Users\carmelo\Desktop\POLAR\COB0002");//Assuming Test is your Folder
        //    FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
        //    List<EstructuraCOB> Lista = new List<EstructuraCOB>();
        //    CP_Archivo item = new CP_Archivo();
        //    foreach (FileInfo file in Files)
        //    {
        //        string text = System.IO.File.ReadAllText(file.FullName);
        //        //System.Console.WriteLine("Contents of WriteText.txt = {0}", text);
        //        item.Nombre = file.Name;
        //        item.Ruta = file.FullName;
        //        item.FechaLectura = DateTime.Now;
        //        string lineas = "";
        //        // Example #2
        //        // Read each line of the file into a string array. Each element
        //        // of the array is one line of the file.
        //        string[] lines = System.IO.File.ReadAllLines(file.FullName);
        //        List<_EstructuraCOB> Archivo = new List<_EstructuraCOB>();
        //        List<CP_ArchivoItem> CPITEMS = new List<CP_ArchivoItem>();
        //        var engine = new FileHelperEngine<_EstructuraCOB>();
        //        var result = engine.ReadFile(file.FullName);
        //        foreach (_EstructuraCOB _item in result)
        //        {
        //            Archivo.Add(_item);
        //            CP_ArchivoItem _AI = new CP_ArchivoItem();
        //            _AI.CodigoCliente = _item.CodigoCliente;
        //            _AI.CodigoDepartamento = _item.CodigoDepartamento;
        //            _AI.Datetime = DateTime.Now;
        //            _AI.DocumentoComercial = _item.DocumentoComercial;
        //            _AI.Estatus = 1;
        //            _AI.FechaEmision = _item.FechaEmision;
        //            _AI.FechaVencimiento = _item.Fechavencimiento;
        //            _AI.Monto = _item.Monto;
        //            _AI.MontoIVA = _item.MontoIva;
        //            _AI.OBS = _item.OBS;
        //            _AI.Referencia = _item.Referencia;
        //            _AI.TipoDocumento = _item.TipoDocumento;

        //            if (!(DateTime.Now <= DateTime.Parse(_item.Fechavencimiento)))
        //            {
        //                _AI.CodigoError = 001;
        //                _AI.DescripcionError = "DOCUMENTO VENCIDO";
        //            }


        //            CP_INI beneficiario = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == _item.CodigoCliente && u.Estatus == 2).FirstOrDefault();
        //            if (beneficiario != null && beneficiario.Id > 0)
        //            {
        //                if (beneficiario.Estatus != 2)
        //                {
        //                    _AI.CodigoError = 001;
        //                    _AI.DescripcionError = "CLIENTE INACTIVO PARA COBRO";
        //                }
        //            }
        //            else
        //            {
        //                //092 nota sin factura 028 cuenta beneficiario invalidad 016 cuenta bloqueada 070 monto invalido
        //                _AI.CodigoError = 001;
        //                _AI.DescripcionError = "CLIENTE SIN CARGA PREVIA";
        //            }

        //            CPITEMS.Add(_AI);
        //            CP_ArchivoItemRepo.AddEntity(_AI);

        //        }
        //        CP_ArchivoItemRepo.SaveChanges();
        //        //CP_Beneficiario.SaveChanges();

        //        item.Registro = 0;
        //        item.ReferenciaOrigen = "";
        //        item.ReferenciaArchivoBanco = "";
        //        item.FechaCreacion = DateTime.Now;
        //        item.FechaLectura = DateTime.Now;
        //        item.Contenido = lineas;
        //        item.Descripcion = "Lectura Archivo Porlar COB";
        //        item.Tipo = 1;
        //        item.EsRespuesta = false;
        //        item.ContenidoRespuesta = "";
        //        // Display the file contents by using a foreach loop.
        //        //System.Console.WriteLine("Contents of WriteLines2.txt = ");
        //        //foreach (string line in lines)
        //        //{





        //        //    lineas = lineas + line + "<br />";
        //        //    // Use a tab to indent each line of the file.
        //        //    string[] valores = line.Split('|').ToArray();
        //        //    EstructuraCOB objeto = new EstructuraCOB();



        //        //    switch (valores.Count())
        //        //    {
        //        //        case 10:
        //        //            objeto.CodigoCliente = valores[0];
        //        //            objeto.DocumentoComercial = valores[1];
        //        //            objeto.Referencia = valores[2];
        //        //            objeto.CodigoDepartamento = valores[3];
        //        //            objeto.Monto = valores[4].ToString().Replace('.', ',');
        //        //            objeto.FechaEmision = valores[5];
        //        //            objeto.Fechavencimiento = valores[6];
        //        //            objeto.TipoDocumento = valores[7];
        //        //            objeto.x = valores[8];
        //        //            objeto.Concepto = valores[9];

        //        //            break;
        //        //        case 11:
        //        //            objeto.CodigoCliente = valores[0];
        //        //            objeto.DocumentoComercial = valores[1];
        //        //            objeto.Referencia = valores[2];
        //        //            objeto.CodigoDepartamento = valores[3];
        //        //            objeto.Monto = valores[4].ToString().Replace('.', ',');
        //        //            objeto.FechaEmision = valores[5];
        //        //            objeto.Fechavencimiento = valores[6];
        //        //            objeto.TipoDocumento = valores[7];
        //        //            objeto.x = valores[8];
        //        //            objeto.Concepto = valores[9];
        //        //            objeto.OBS = valores[10];
        //        //            break;
        //        //        default:
        //        //            break;
        //        //    }


        //        //    Lista.Add(objeto);
        //        //}
        //        CPITEMS = CPITEMS.Where(u => u.CodigoError == null).ToList();
        //        int largo = CPITEMS.Count();
        //        int i = 0;
        //        var groupedCustomerList = CPITEMS.GroupBy(u => u.DocumentoComercial).Select(grp => grp.ToList()).ToList();

        //        item.Contenido = lineas;
        //        item.Descripcion = "Archivo Cobro Polar - Comercios Evaluado:" + groupedCustomerList.Count();
        //        item.Tipo = 2;
        //        ArchivoREPO.AddEntity(item);
        //        ArchivoREPO.SaveChanges();
        //        //Generar Estado de Cuenta
        //        foreach (var ids in groupedCustomerList)
        //        {
        //            CP_ArchivoEstadoCuenta EstadoCuenta = new CP_ArchivoEstadoCuenta();
        //            decimal Total = 0;
        //            decimal Debito = 0;
        //            decimal Credito = 0;
        //            string debito = "";
        //            string credito = "";
        //            string comercio = "";
        //            foreach (var row in ids)
        //            {
        //                try
        //                {
        //                    comercio = row.CodigoCliente;
        //                    if (row.TipoDocumento == "02")
        //                    {
        //                        Credito = Credito + decimal.Parse(row.Monto);
        //                        credito = credito + " " + row.DocumentoComercial + " " + row.FechaEmision + " NOTA CREDITO" + " | ";
        //                    }
        //                    else
        //                    {

        //                        switch (row.TipoDocumento)
        //                        {
        //                            case "01":
        //                                Debito = Debito + decimal.Parse(row.Monto);
        //                                debito = debito + " " + row.DocumentoComercial + " " + row.FechaEmision + " FACTURA" + " | ";
        //                                break;
        //                            case "03":
        //                                Debito = Debito + decimal.Parse(row.Monto);
        //                                debito = debito + " " + row.DocumentoComercial + " " + row.FechaEmision + " NOTA DEBITO" + " | ";
        //                                break;
        //                            case "04":
        //                                Debito = Debito + decimal.Parse(row.Monto);
        //                                debito = debito + " " + row.DocumentoComercial + " " + row.FechaEmision + " FACTURA" + " | ";
        //                                break;

        //                            default:
        //                                break;
        //                        }
        //                    }
        //                }
        //                catch
        //                {
        //                    //crear log de error y guardar en base de datos
        //                }
        //            }

        //            Total = Credito - Debito;
        //            EstadoCuenta.ArchivoLecturaPolar = item.Id;
        //            EstadoCuenta.MontoCredito = Credito;
        //            EstadoCuenta.MontoDebito = Debito;
        //            EstadoCuenta.Total = Total;
        //            EstadoCuenta.TotalArchivo = Total * -1;
        //            EstadoCuenta.DetalleCredito = credito;
        //            EstadoCuenta.DetalleDebito = debito;
        //            EstadoCuenta.FechaLectura = DateTime.Now;
        //            EstadoCuenta.CodigoComercio = comercio;
        //            if (EstadoCuenta.Total > 0)
        //            {
        //                EstadoCuenta.Estatus = 2;
        //            }
        //            else
        //            {
        //                EstadoCuenta.Estatus = 1;
        //            }
        //            EstadoCuentaREPO.AddEntity(EstadoCuenta);
        //            EstadoCuentaREPO.SaveChanges();
        //        }

        //    }
        //    return true;
        //}

        //public bool COB_EjercicioCobro()
        //{
        //    List<CP_ArchivoEstadoCuenta> Lista = EstadoCuentaREPO.GetAllRecords().Where(u => u.Estatus == 1 && u.ArchivoCobroBanesco == 39).OrderBy(u => u.Id).ToList();
        //    //int skip = 0;
        //    //List<CP_ArchivoEstadoCuenta> Segmento = Lista.Skip(skip).Take(50).ToList();
        //    //bool win = GenerarCobroBanesco(Lista.Take(50).ToList());
        //    //bool win = GenerarCobroBanesco(Lista.ToList());
        //    bool win = GenerarCobroBanescoFinPago(Lista.ToList());
        //    bool winv = GenerarValidacionCobroBanesco(Lista.ToList());
        //    if (win && winv)
        //    {
        //        foreach (var item in Lista)
        //        {
        //            item.Estatus = 2;
        //            EstadoCuentaREPO.SaveChanges();
        //        }
        //    }


        //    return true;
        //}

        public bool COB_GenerarCobroBanesco(List<CP_ArchivoEstadoCuenta> Cobros, string _asociado)
        {
            List<CP_Archivo> Archivos = new List<CP_Archivo>();
            int i = 0;
            foreach (var cobro in Cobros)
            {
                string lineas = "";
                CP_Archivo item = new CP_Archivo();

                item.FechaLectura = DateTime.Now;
                item.Registro = 1;
                item.ReferenciaOrigen = "";
                item.ReferenciaArchivoBanco = "";
                item.FechaCreacion = DateTime.Now;
                item.FechaLectura = DateTime.Now;
                item.Descripcion = "Lectura Archivo Porlar COB";
                item.Tipo = 3;
                item.EsRespuesta = false;
                item.ContenidoRespuesta = "";
                item.Descripcion = "Archivo Cobro Enviado Banesco";
                item.Tipo = 2;
                //string comercio = Cobros.First().AE_Avance.Id;
                //string rif = Cobros.First().AE_Avance.RifCommerce;
                string fechaarchivo = DateTime.Now.AddDays(0).ToString("ddMMyy.ssff");
                //string id = Cobros.FirstOrDefault().Id.ToString();
                //if (id.Length > 4)
                //{
                //    id = id.Substring((id.Length - 4), 4);
                //}
                //else if (id.Length < 4)
                //{
                //    id = id.PadLeft(4, '0');
                //}
                string numeroorden = DateTime.Now.AddDays(0).ToString("yyMMddssff");
                string _fecha = DateTime.Now.AddDays(0).ToString("yyyyMMdd");
                //numeroorden = numeroorden;
                //datos fijos
                string registro = "00";
                //string idtransaccion = "2020040801";Codigo
                string asociado = _asociado;
                //
                //asociado = "540207829";
                string _rif = "";
                string ordenante = "";
                string _numerocuenta = "";
                //
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

                CP_INI beneficiario = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == cobro.CodigoComercio && u.Estatus == 2).FirstOrDefault();
                string _cuenta = "";
                string _nombrecomercial = "";
                string _rifc = "";
                if (beneficiario != null && beneficiario.Id > 0)
                {
                    _cuenta = beneficiario.CuentaBancaria;
                    _nombrecomercial = beneficiario.Nombre;
                    _rifc = beneficiario.RifCliente.PadLeft(9, '0');
                }

                string tipo = "03";
                string recibo = cobro.NumeroDocumento.Substring(cobro.NumeroDocumento.Length - 7, 7).PadLeft(8, '0');
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



                //registro credito
                string _tipo2 = "02";
                string _recibo = cobro.NumeroDocumento.Substring(cobro.NumeroDocumento.Length - 7, 7).PadLeft(8, '0');
                //Cobros.First().Id.ToString().PadLeft(8, '0');



                if (asociado == "540132787")
                {
                    _rif = "J000301255";
                    ordenante = "EFE C A";
                    _numerocuenta = "01340850598503004455";
                    //_numerocuenta = "01340850578503004652";
                }
                else if (asociado == "205903844")
                {
                    _rif = "J301370139";
                    ordenante = "PEPSI C A";
                    _numerocuenta = "01340850598503004195";
                    //_numerocuenta = "01340850588503004268";
                }
                else if (asociado == "540133497")
                {
                    _rif = "J000413126";
                    ordenante = "ALIMENTOS POLAR C A";
                    _numerocuenta = "01340375913751013514";
                    //_numerocuenta = "01340850568503004482";

                }
                else if (asociado == "540130908")
                {
                    _rif = "J000063729";
                    ordenante = "CERVECERIA C A";
                    _numerocuenta = "01340850598503004357";
                    //_numerocuenta = "01340375993751007271";
                }


                //string _rif = "J401878105";
                //string ordenante = "TECNOLOGIA INSTAPAGO C A";
                //string _numerocuenta = "01340031810311158627";
                //decimal cambio = Math.Round(Cobros.Sum(y => y.Monto), 2);
                //cambio = cambio * 100;
                string _montoabono = total.ToString().Split(',')[0];
                string _moneda = "VES";
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
                string debitos = "1";
                string montototal = total.ToString().Split(',')[0];
                string totales = _tipo + totalcreditos.PadLeft(15, '0') + debitos.PadLeft(15, '0') + montototal.PadLeft(15, '0');
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = totales;


                // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
                // and then closes the file.  You do NOT need to call Flush() or Close().
                string ruta = @"C:\Users\carmelo\Desktop\POLAR\_COBROBANESCO\" + "I0005." + asociado + "." + fechaarchivo + i++;
                System.IO.File.WriteAllLines(ruta, lines);
                item.Contenido = registrodecontrol + "|" + encabezado + "|" + credito;
                item.ReferenciaArchivoBanco = numeroorden;
                item.Nombre = "I0005." + asociado + "." + fechaarchivo + ".txt";
                item.Ruta = ruta;

                CP_ArchivoREPO.AddEntity(item);
                CP_ArchivoREPO.SaveChanges();
                cobro.ArchivoLecturaPolar = item.Id;
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
            }

            EstadoCuentaREPO.SaveChanges();
            return true;
        }

        public bool COB_LecturaArchivoSalidaBanesco()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\Carmelo\Desktop\POLAR\_BANESCOSALIDACOBRO\");
            //Assuming Test is your Folder
            string rutafinal = @"C:\Users\Carmelo\Desktop\POLAR\SalidaBanesco\";
            FileInfo[] Files = d.GetFiles("*"); //Getting Text files
            List<EstructuraCOB> Lista = new List<EstructuraCOB>();
            CP_Archivo item = new CP_Archivo();
            CP_Archivo getCP = new CP_Archivo();
            List<CP_ArchivoEstadoCuenta> ItemsArchivo = new List<CP_ArchivoEstadoCuenta>();
            List<PAG> ListaPAG = new List<PAG>();
            List<string> Empresas = new List<string>();
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("_O0002"))
                {
                    string departamento = "";
                    string ordenante = "";
                    string _numerocuenta = "";
                    if (file.Name.Contains("540132787"))
                    {
                        departamento = "0600";
                        ordenante = "EFE C A";
                        _numerocuenta = "01340850598503004455";
                        Empresas.Add(departamento);
                    }
                    else if (file.Name.Contains("205903844"))
                    {
                        departamento = "0100";
                        ordenante = "PEPSI C A";
                        _numerocuenta = "01340850598503004195";
                        Empresas.Add(departamento);
                    }
                    else if (file.Name.Contains("540133497"))
                    {
                        departamento = "0001";
                        ordenante = "ALIMENTOS POLAR C A";
                        _numerocuenta = "01340375913751013514";
                        Empresas.Add(departamento);

                    }
                    else if (file.Name.Contains("540130908"))
                    {
                        departamento = "0002";
                        ordenante = "CERVECERIA C A";
                        _numerocuenta = "01340850598503004357";
                        Empresas.Add(departamento);
                    }
                    //item.Nombre = file.Name;
                    //item.Ruta = file.FullName;
                    //item.FechaLectura = DateTime.Now;
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
                    CP_ArchivoEstadoCuenta elemento = new CP_ArchivoEstadoCuenta();
                    CP_INI cp_ini = new CP_INI();
                    //string IDarchivo = "";
                    EstructuraSalidaBanescoDetalle Registrolinea2 = new EstructuraSalidaBanescoDetalle();

                    foreach (string line in lines)
                    {
                        //if (i == 2)
                        //{
                        lineas = lineas + line + "<br />";
                        string sep = "\t";


                        string tipo = line.Substring(16, 2).ToString().TrimEnd();

                        if (tipo == "01")
                        {

                            EstructuraSalidaBanescoEncabezado Registro = new EstructuraSalidaBanescoEncabezado();
                            Registro.Trading = line.Substring(0, 15).ToString().TrimEnd();
                            Registro.Filler = line.Substring(15, 2).ToString().TrimEnd();
                            Registro.TipoRegistro = line.Substring(16, 2).ToString().TrimEnd();
                            Registro.__NumeroReferenciaRespuesta = line.Substring(19, 10).ToString().TrimEnd();
                            //IDarchivo = line.Substring(19, 10).ToString().TrimEnd();
                            //Registro.__FechaRespuesta = line.Substring(54, 67).ToString().TrimEnd();
                            //Registro.__NumeroReferenciaOrdenPago = line.Substring(68, 102).ToString().TrimEnd();
                            //Registro.__TipoOrdenPago = line.Substring(103, 105).ToString().TrimEnd();
                            //Registro.__CodigoBancoEmisor = line.Substring(106, 116).ToString().TrimEnd();
                            //Registro.__NombreBancoEmisor = line.Substring(117, 186).ToString().TrimEnd();
                            //Registro.__CodgioEmpresaReceptoraBansta = line.Substring(187, 203).ToString().TrimEnd();
                            //Registro.__DescripcionEmpresaReceptoraBansta = line.Substring(204, 273).ToString().TrimEnd();

                            //string _line = Registro.__NumeroReferenciaOrdenPago + "|1|";

                            //Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                            //linesArchivo[linesArchivo.Length - 1] = _line;

                            getCP = ArchivoREPO.GetAllRecords().Where(u => u.ReferenciaArchivoBanco == Registro.__NumeroReferenciaRespuesta).FirstOrDefault();
                            ItemsArchivo = EstadoCuentaREPO.GetAllRecords().Where(u => u.ArchivoLecturaPolar == getCP.Id).ToList();

                        }
                        else if (tipo == "02")
                        {
                            if (i > 1)
                            {
                                Registrolinea2.Trading = line.Substring(0, 15).ToString().TrimEnd();
                                Registrolinea2.Filler = line.Substring(15, 2).ToString().TrimEnd();
                                Registrolinea2.TipoRegistro = line.Substring(16, 2).ToString().TrimEnd();
                                //Registrolinea2.NumeroReferencia = line.Substring(18, 10).ToString().TrimEnd();
                                Registrolinea2.NumeroReferencia = line.Substring(18, 10).ToString().TrimEnd();
                                //Registro.Fecha = line.Substring(54, 61).ToString().TrimEnd();
                                //Registro.Monto = line.Substring(62, 76).ToString().TrimEnd();
                                //Registro.Moneda = line.Substring(77, 79).ToString().TrimEnd();
                                //Registro.Rif = line.Substring(80, 96).ToString().TrimEnd();
                                //Registro.NumeroCuenta = line.Substring(97, 131).ToString().TrimEnd();
                                //Registro.BancoBeneficiario = line.Substring(132, 142).ToString().TrimEnd();
                                //Registro.BancoBeneficiarioDescripcion = line.Substring(143, 212).ToString().TrimEnd();
                                //Registro.CodigoAgencia = line.Substring(213, 215).ToString().TrimEnd();
                                //Registro.NombreBeneficiario = line.Substring(216, 285).ToString().TrimEnd();
                                //Registro.NumeroCliente = line.Substring(286, 320).ToString().TrimEnd();
                                //Registro.FechaVencimiento = line.Substring(321, 326).ToString().TrimEnd();
                                //Registro.NumeroSecuenciaArchivo = line.Substring(327, 332).ToString().TrimEnd();

                                //string _line = Registro.NumeroCuenta + "|1|";

                                //Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                                //linesArchivo[linesArchivo.Length - 1] = _line;

                                //elemento = ItemsArchivo.Where(u => u.Id == int.Parse(Registrolinea2.NumeroReferencia.Substring(1, (Registrolinea2.NumeroReferencia.Length - 1)))).FirstOrDefault();
                                //ItemsArchivo = EstadoCuentaREPO.GetAllRecords().Where(u => u.ArchivoLecturaPolar == getCP.Id).ToList();
                                elemento = ItemsArchivo.FirstOrDefault();
                                cp_ini = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == elemento.CodigoComercio && u.Estatus == 2).FirstOrDefault();

                            }



                        }
                        else if (tipo == "03")
                        {
                            if (i > 2)
                            {
                                EstructuraSalidaBanescoDetalle Registro = new EstructuraSalidaBanescoDetalle();
                                Registro.Trading = line.Substring(0, 15).ToString().TrimEnd();
                                Registro.Filler = line.Substring(15, 2).ToString().TrimEnd();
                                Registro.TipoRegistro = line.Substring(16, 2).ToString().TrimEnd();
                                //Registro.NumeroReferencia = line.Substring(20, 2).ToString().TrimEnd();
                                string codigoError = line.Substring(20, 2).ToString().TrimEnd();
                                Registro.BancoBeneficiarioDescripcion = line.Substring(22, (line.Length - 22)).ToString().TrimEnd();
                                PAG itempag = new PAG();
                                PAGLinea1 Linea1 = new PAGLinea1();
                                //Linea1.ReferenciaPago = Registrolinea2.NumeroReferencia;
                                Linea1.ReferenciaPago = elemento.NumeroDocumento.Substring((elemento.NumeroDocumento.Length - 8), 8).ToString();
                                Linea1.NumeroRegistro = "1";
                                Linea1.CodigoCliente = elemento.CodigoComercio;
                                Linea1.TipoPagador = cp_ini.RifCliente.Substring(0, 1).ToString();
                                Linea1.IdentificacionPagador = cp_ini.RifCliente.Substring(1, 9).ToString();
                                Linea1.RazonSocial = cp_ini.Nombre;
                                if (codigoError == "74")
                                {
                                    Linea1.EstatusPago = Registro.BancoBeneficiarioDescripcion;
                                    if (Linea1.EstatusPago == "COBRO EXITOSO")
                                    {
                                        Linea1.EstatusPago = "PAGADO";
                                        elemento.Estatus = 2;
                                        EstadoCuentaREPO.SaveChanges();
                                    }
                                }
                                else
                                {
                                    Linea1.EstatusPago = "RECHAZADO - " + codigoError + " : " + Registro.BancoBeneficiarioDescripcion;
                                }
                                Linea1.CuentaCliente = cp_ini.CuentaBancaria;
                                itempag.Linea1 = Linea1;

                                //AQUI DEBO ITERAR ENTRE DEBITOS Y CREDITOS ... VAMOS CON LA FACTURA PRIMERO
                                List<PAGLinea2> listalinea2 = new List<PAGLinea2>();


                                string[] Debito = elemento.DetalleDebito.Split('|');
                                foreach (var _debito in Debito)
                                {
                                    string[] objetodebito = _debito.Split(';');
                                    if (objetodebito.Count() > 4)
                                    {
                                        PAGLinea2 Linea2 = new PAGLinea2();
                                        Linea2.Referenciapago = elemento.NumeroDocumento.Substring((elemento.NumeroDocumento.Length - 8), 8).ToString();
                                        Linea2.NumeroRegistro = "2";

                                        Linea2.NumeroDocumento = elemento.NumeroDocumento.TrimStart().TrimEnd();
                                        Linea2.TipoDocumento = "01";
                                        Linea2.Referencia = elemento.TipoDocumento;
                                        Linea2.FechaVencimiento = elemento.FechaVencimiento;
                                        Linea2.FechaPago = elemento.FechaPago;

                                        Linea2.MontoIva = "0,00";
                                        Linea2.MontoRetencion = "0,00";
                                        Linea2.MontoNeto = objetodebito[1].ToString().Replace('.', ',');// elemento.MontoDebito.ToString();
                                        Linea2.NumeroComprobanteIR = "X";
                                        Linea2.FechaEmisionComporbanteIR = "X";
                                        listalinea2.Add(Linea2);
                                    }
                                }

                                string[] Credito = elemento.DetalleCredito.Split('|');
                                foreach (var _credito in Credito)
                                {
                                    string[] objetocredito = _credito.Split(';');
                                    if (objetocredito.Count() > 4)
                                    {
                                        PAGLinea2 Linea2 = new PAGLinea2();
                                        Linea2.Referenciapago = elemento.NumeroDocumento.Substring((elemento.NumeroDocumento.Length - 8), 8).ToString();
                                        Linea2.NumeroRegistro = "2";

                                        Linea2.NumeroDocumento = objetocredito[0].ToString().TrimStart().TrimEnd();// elemento.NumeroDocumento.TrimStart().TrimEnd();
                                        Linea2.TipoDocumento = "02";
                                        Linea2.Referencia = objetocredito[4].ToString();
                                        Linea2.FechaVencimiento = objetocredito[3].ToString();
                                        Linea2.FechaPago = objetocredito[2].ToString();

                                        Linea2.MontoIva = "0,00";
                                        Linea2.MontoRetencion = "0,00";
                                        Linea2.MontoNeto = objetocredito[1].ToString().Replace('.', ',');// elemento.MontoDebito.ToString();
                                        Linea2.NumeroComprobanteIR = "X";
                                        Linea2.FechaEmisionComporbanteIR = "X";
                                        listalinea2.Add(Linea2);
                                    }
                                }

                                //Linea2.Referenciapago = Registrolinea2.NumeroReferencia;

                                //List<PAGLinea2> listalinea2 = new List<PAGLinea2>();
                                ///aqui debo agregar las notas de credito
                                //string[] _itm = elemento.DetalleCredito.Split('|');
                                //foreach (var __ele in _itm)
                                //{
                                //    string[] obe = __ele.Split(';');

                                //    if (obe.Count() == 5)
                                //    {
                                //        PAGLinea2 LineaX = new PAGLinea2();

                                //        LineaX.Referenciapago = Registrolinea2.NumeroReferencia;
                                //        LineaX.NumeroRegistro = "2";

                                //        LineaX.NumeroDocumento = obe[0].TrimStart().TrimEnd();
                                //        LineaX.TipoDocumento = "01";
                                //        LineaX.Referencia = obe[4];
                                //        LineaX.FechaVencimiento = obe[3];
                                //        LineaX.FechaPago = obe[2];

                                //        LineaX.MontoIva = "0,00";
                                //        LineaX.MontoRetencion = "0,00";
                                //        LineaX.MontoNeto = obe[1];
                                //        LineaX.NumeroComprobanteIR = "X";
                                //        LineaX.FechaEmisionComporbanteIR = "X";
                                //        listalinea2.Add(LineaX);
                                //    }
                                //}

                                itempag.Linea2 = listalinea2;

                                PAGLinea3 Linea3 = new PAGLinea3();
                                //Linea3.Referenciapago = Registrolinea2.NumeroReferencia;
                                Linea3.Referenciapago = elemento.NumeroDocumento.Substring((elemento.NumeroDocumento.Length - 8), 8).ToString();
                                Linea3.NumeroRegistro = "4";
                                Linea3.CodigoDepartamento = departamento;
                                Linea3.TipoTransaccion = "03";
                                Linea3.NumerocCuenta = _numerocuenta;
                                Linea3.Subtotal = elemento.TotalArchivo.ToString();
                                Linea3.Totaldebito = "X";
                                Linea3.TotalCargootrosBancos = elemento.TotalArchivo.ToString();
                                Linea3.TotaltarjetaCredito = "X";
                                Linea3.SubtotalCheques = "X";
                                Linea3.SubtotalEfectivo = "X";
                                Linea3.TotalDeposito = "X";

                                itempag.Linea3 = Linea3;

                                itempag.departamento = departamento;
                                ListaPAG.Add(itempag);
                            }
                            //string _line = Registro._CodigoEstatus + "|1|";

                            //Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                            //linesArchivo[linesArchivo.Length - 1] = _line;
                        }
                        else if (tipo == "04")
                        {

                        }

                        //}
                        i++;
                    }
                    getCP.FechaLectura = DateTime.Now;
                    getCP.ContenidoRespuesta = lineas;
                    getCP.EsRespuesta = true;
                    //ArchivoREPO.SaveChanges();



                    //file.MoveTo(rutafinal + file.Name + ".txt");
                    //item.Contenido = lineas;
                    //item.Descripcion = "[FINPAGO] RESPUESTA Cargo cuenta masivo.";
                    //item.Tipo = 1;
                    //item.IdEmpresa = 1;
                    //item.IdReferencia = Guid.NewGuid();
                    //item.

                    //ArchivoREPO.AddEntity(item);
                }
                else
                {

                    //file.MoveTo(rutafinal + file.Name + ".txt");
                }


            }
            PAG_Construir(ListaPAG, Empresas.Distinct().ToList());
            return true;
        }

        public bool PAG_Construir(List<PAG> items, List<string> empresas)
        {
            foreach (var empresa in empresas)
            {
                List<string> _cobros = new List<string>();

                int i = 0;
                foreach (var cobro in items.Where(u => u.departamento == empresa).ToList())
                {
                    string line1 =
                    cobro.Linea1.ReferenciaPago.ToString().PadLeft(18, '0') + "|" +
                    cobro.Linea1.NumeroRegistro + "|" +
                    cobro.Linea1.CodigoCliente + "|" +
                    cobro.Linea1.TipoPagador + "|" +
                    cobro.Linea1.IdentificacionPagador + "|" +
                    cobro.Linea1.RazonSocial + "|" +
                    cobro.Linea1.EstatusPago + "|" +
                    cobro.Linea1.CuentaCliente;
                    _cobros.Add(line1);

                    foreach (var item in cobro.Linea2)
                    {
                        string line2 =
                        item.Referenciapago.ToString().PadLeft(18, '0') + "|" +
                        item.NumeroRegistro + "|" +
                        item.NumeroDocumento + "|" +
                        item.TipoDocumento + "|" +
                        item.Referencia + "|" +
                        item.FechaVencimiento + "|" +
                        item.FechaPago + "|" +
                        item.MontoIva + "|" +
                        item.MontoRetencion + "|" +
                        item.MontoNeto + "|" +
                        item.NumeroComprobanteIR + "|" +
                        item.FechaEmisionComporbanteIR;

                        _cobros.Add(line2);
                    }
                    string line3 =
                    cobro.Linea3.Referenciapago.ToString().PadLeft(18, '0') + "|" +
                    cobro.Linea3.NumeroRegistro + "|" +
                    cobro.Linea3.CodigoDepartamento + "|" +
                    cobro.Linea3.TipoTransaccion + "|" +
                    cobro.Linea3.NumerocCuenta + "|" +
                    cobro.Linea3.Subtotal + "|" +
                    cobro.Linea3.Totaldebito + "|" +
                    cobro.Linea3.TotalCargootrosBancos + "|" +
                    cobro.Linea3.TotaltarjetaCredito + "|" +
                    cobro.Linea3.SubtotalCheques + "|" +
                    cobro.Linea3.SubtotalEfectivo + "|" +
                    cobro.Linea3.TotalDeposito;

                    _cobros.Add(line3);

                }
                //_cobros
                string[] lines = { };
                foreach (var _item in _cobros)
                {
                    Array.Resize(ref lines, lines.Length + 1);
                    lines[lines.Length - 1] = _item;
                }
                string ruta = @"C:\Users\carmelo\Desktop\POLAR\CargoCuentaBanesco\" + "BANPAG" + empresa + ".txt";
                System.IO.File.WriteAllLines(ruta, lines);
            }
            return true;


        }

        public bool EjercicioCobroCOB()
        {
            List<CP_ArchivoEstadoCuenta> Lista = EstadoCuentaREPO.GetAllRecords().Where(u => u.ArchivoLecturaPolar == 102 && u.Estatus == 1 && u.TotalArchivo > 0).OrderBy(u => u.Id).ToList();
            CP_Archivo item = CP_ArchivoREPO.GetAllRecords().Where(u => u.Id == Lista.FirstOrDefault().ArchivoLecturaPolar).FirstOrDefault();
            string asociado = "";
            if (item.Nombre.Contains("BANCOB0001"))
            {
                asociado = "540133497";
                //ALIMENTOS POLAR  J000413126 - IBS :540133497
                //rifreferencia = "J000413126";
                //referencia = "0001";

            }
            else if (item.Nombre.Contains("BANCOB0002"))
            {
                asociado = "540130908";
                //CERVECERIA POLAR J000063729 - IBS:540130908
                //rifreferencia = "J000063729";
                //referencia = "0002";
            }
            else if (item.Nombre.Contains("BANCOB0600"))
            {
                asociado = "540132787";
                //PRODUCTOS EFE J000301255  - IBS: 540132787 
                //rifreferencia = "J000301255";
                //referencia = "0600";
            }
            else if (item.Nombre.Contains("BANCOB0100"))
            {
                asociado = "205903844";
                //PEPSI COLA J301370139 -IBS: 205903844 
                //rifreferencia = "J301370139";
                //referencia = "0100";
            }
            //int skip = 0;
            //List<CP_ArchivoEstadoCuenta> Segmento = Lista.Skip(skip).Take(50).ToList();
            //bool win = GenerarCobroBanesco(Lista.Take(50).ToList());
            //bool win = GenerarCobroBanesco(Lista.ToList());
            bool win = COB_GenerarCobroBanesco(Lista, asociado);
            //bool winv = GenerarValidacionCobroBanesco(Lista.ToList());
            //if (win && winv)
            //{
            //    foreach (var item in Lista)
            //    {
            //        item.Estatus = 2;
            //        EstadoCuentaREPO.SaveChanges();
            //    }
            //}


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
        public bool _GenerarArchivoBANPOLFINPAGO(List<AE_EstadoCuenta> Cobros)
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
            string _rif = "J410066105";
            string ordenante = "FINPAGOS TECNOLOGIA C A";
            string _montoabono = total.ToString().Split(',')[0];
            string _moneda = "VES";
            string _numerocuenta = "01340031870311158436";
            string _swift = "BANSVECA";
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
            DirectoryInfo d = new DirectoryInfo(@"C:\Apps\Transax\Repo\RespuestaBanesco\");
            //Assuming Test is your Folder
            string rutafinal = @"C:\Apps\Transax\Repo\RespuestaBanescoBackUp\";
            FileInfo[] Files = d.GetFiles("*"); //Getting Text files
            List<EstructuraCOB> Lista = new List<EstructuraCOB>();
            CP_Archivo item = new CP_Archivo();
            CP_Archivo getCP = new CP_Archivo();
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("0002"))
                {
                    //item.Nombre = file.Name;
                    //item.Ruta = file.FullName;
                    //item.FechaLectura = DateTime.Now;
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
                        //if (i == 2)
                        //{
                        lineas = lineas + line + "<br />";
                        string sep = "\t";


                        string tipo = line.Substring(16, 2).ToString().TrimEnd();

                        if (tipo == "01")
                        {
                            EstructuraSalidaBanescoEncabezado Registro = new EstructuraSalidaBanescoEncabezado();
                            Registro.Trading = line.Substring(0, 14).ToString().TrimEnd();
                            Registro.Filler = line.Substring(14, 15).ToString().TrimEnd();
                            Registro.TipoRegistro = line.Substring(16, 18).ToString().TrimEnd();
                            Registro.__NumeroReferenciaRespuesta = line.Substring(19, 34).ToString().TrimEnd();
                            //Registro.__FechaRespuesta = line.Substring(54, 67).ToString().TrimEnd();
                            //Registro.__NumeroReferenciaOrdenPago = line.Substring(68, 102).ToString().TrimEnd();
                            //Registro.__TipoOrdenPago = line.Substring(103, 105).ToString().TrimEnd();
                            //Registro.__CodigoBancoEmisor = line.Substring(106, 116).ToString().TrimEnd();
                            //Registro.__NombreBancoEmisor = line.Substring(117, 186).ToString().TrimEnd();
                            //Registro.__CodgioEmpresaReceptoraBansta = line.Substring(187, 203).ToString().TrimEnd();
                            //Registro.__DescripcionEmpresaReceptoraBansta = line.Substring(204, 273).ToString().TrimEnd();

                            //string _line = Registro.__NumeroReferenciaOrdenPago + "|1|";

                            //Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                            //linesArchivo[linesArchivo.Length - 1] = _line;

                            getCP = ArchivoREPO.GetAllRecords().Where(u => u.ReferenciaArchivoBanco == Registro.__NumeroReferenciaRespuesta).FirstOrDefault();

                        }
                        else if (tipo == "02")
                        {
                            //EstructuraSalidaBanescoDetalle Registro = new EstructuraSalidaBanescoDetalle();
                            //Registro.Trading = line.Substring(0, 14).ToString().TrimEnd();
                            //Registro.Filler = line.Substring(1y bajando la respuesta exploto 4, 15).ToString().TrimEnd();
                            //Registro.TipoRegistro = line.Substring(16, 17).ToString().TrimEnd();
                            //Registro.Indicador = line.Substring(17, 18).ToString().TrimEnd();
                            //Registro.NumeroReferencia = line.Substring(19, 53).ToString().TrimEnd();
                            //Registro.Fecha = line.Substring(54, 61).ToString().TrimEnd();
                            //Registro.Monto = line.Substring(62, 76).ToString().TrimEnd();
                            //Registro.Moneda = line.Substring(77, 79).ToString().TrimEnd();
                            //Registro.Rif = line.Substring(80, 96).ToString().TrimEnd();
                            //Registro.NumeroCuenta = line.Substring(97, 131).ToString().TrimEnd();
                            //Registro.BancoBeneficiario = line.Substring(132, 142).ToString().TrimEnd();
                            //Registro.BancoBeneficiarioDescripcion = line.Substring(143, 212).ToString().TrimEnd();
                            //Registro.CodigoAgencia = line.Substring(213, 215).ToString().TrimEnd();
                            //Registro.NombreBeneficiario = line.Substring(216, 285).ToString().TrimEnd();
                            //Registro.NumeroCliente = line.Substring(286, 320).ToString().TrimEnd();
                            //Registro.FechaVencimiento = line.Substring(321, 326).ToString().TrimEnd();
                            //Registro.NumeroSecuenciaArchivo = line.Substring(327, 332).ToString().TrimEnd();

                            //string _line = Registro.NumeroCuenta + "|1|";

                            //Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                            //linesArchivo[linesArchivo.Length - 1] = _line;
                        }
                        else if (tipo == "03")
                        {
                            //EstructuraSalidaBanescoEstatus Registro = new EstructuraSalidaBanescoEstatus();
                            //Registro.Trading = line.Substring(0, 14).ToString().TrimEnd();
                            //Registro.Filler = line.Substring(14, 15).ToString().TrimEnd();
                            //Registro.TipoRegistro = line.Substring(16, 17).ToString().TrimEnd();
                            //Registro._CodigoEstatus = line.Substring(19, 21).ToString().TrimEnd();
                            //Registro._Descripcion = line.Substring(22, 91).ToString().TrimEnd();

                            //string _line = Registro._CodigoEstatus + "|1|";

                            //Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                            //linesArchivo[linesArchivo.Length - 1] = _line;
                        }
                        else if (tipo == "04")
                        {

                        }

                        //}
                        //i++;
                    }
                    getCP.FechaLectura = DateTime.Now;
                    getCP.ContenidoRespuesta = lineas;
                    getCP.EsRespuesta = true;
                    ArchivoREPO.SaveChanges();

                    file.MoveTo(rutafinal + file.Name + ".txt");
                    //item.Contenido = lineas;
                    //item.Descripcion = "[FINPAGO] RESPUESTA Cargo cuenta masivo.";
                    //item.Tipo = 1;
                    //item.IdEmpresa = 1;
                    //item.IdReferencia = Guid.NewGuid();
                    //item.

                    //ArchivoREPO.AddEntity(item);
                }
                else
                {

                    file.MoveTo(rutafinal + file.Name + ".txt");
                }


            }
            return true;
        }
        public bool LecturaArchivoBeneficiarioPOLAR()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\carmelo\Desktop\POLAR\Beneficiarios");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
            List<EstructuraCOB> Lista = new List<EstructuraCOB>();
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
            List<EstructuraCOB> Lista = new List<EstructuraCOB>();
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
                    EstructuraCOB objeto = new EstructuraCOB();
                    if (valores.Count() < 10)
                        break;

                    bool win = true;
                    switch (valores.Count())
                    {
                        case 10:
                            objeto.CodigoCliente = valores[0];
                            objeto.DocumentoComercial = valores[1];
                            objeto.Referencia = valores[2];
                            objeto.CodigoDepartamento = valores[3];
                            objeto.Monto = valores[4].ToString().Replace('.', ',');
                            objeto.FechaEmision = valores[5];
                            objeto.Fechavencimiento = valores[6];
                            objeto.TipoDocumento = valores[7];
                            objeto.x = valores[8];
                            objeto.Concepto = valores[9];

                            break;
                        case 11:
                            objeto.CodigoCliente = valores[0];
                            objeto.DocumentoComercial = valores[1];
                            objeto.Referencia = valores[2];
                            objeto.CodigoDepartamento = valores[3];
                            objeto.Monto = valores[4].ToString().Replace('.', ',');
                            objeto.FechaEmision = valores[5];
                            objeto.Fechavencimiento = valores[6];
                            objeto.TipoDocumento = valores[7];
                            objeto.x = valores[8];
                            objeto.Concepto = valores[9];
                            objeto.OBS = valores[10];
                            break;
                        default:

                            break;
                    }


                    if (!(DateTime.Now <= DateTime.Parse(objeto.Fechavencimiento)))
                    {
                        win = false;
                    }


                    CP_INI beneficiario = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == objeto.CodigoCliente && u.Estatus == 2).FirstOrDefault();
                    if (beneficiario != null && beneficiario.Id > 0)
                    {
                        if (beneficiario.Estatus != 2)
                        {
                            win = false;
                        }
                    }
                    else
                    {
                        win = false;
                    }
                    if (win)
                        Lista.Add(objeto);
                }
                int largo = Lista.Count();
                int i = 0;
                var groupedCustomerList = Lista.GroupBy(u => u.DocumentoComercial).Select(grp => grp.ToList()).ToList();

                item.Contenido = lineas;
                item.Descripcion = "Archivo Cobro Polar - Comercios Evaluado:" + groupedCustomerList.Count();
                item.Tipo = 2;
                item.Registro = 0;
                item.ReferenciaOrigen = "";
                item.ReferenciaArchivoBanco = "";
                item.FechaCreacion = DateTime.Now;
                item.FechaLectura = DateTime.Now;
                item.Descripcion = "Lectura Archivo Porlar COB";
                item.Tipo = 1;
                item.EsRespuesta = false;
                item.ContenidoRespuesta = "";
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
                            if (row.TipoDocumento == "02")
                            {
                                Credito = Credito + decimal.Parse(row.Monto);
                                credito = credito + " " + row.DocumentoComercial + " " + row.FechaEmision + " NOTA CREDITO" + " | ";
                            }
                            else
                            {

                                switch (row.TipoDocumento)
                                {
                                    case "01":
                                        Debito = Debito + decimal.Parse(row.Monto);
                                        debito = debito + " " + row.DocumentoComercial + " " + row.FechaEmision + " FACTURA" + " | ";
                                        break;
                                    case "03":
                                        Debito = Debito + decimal.Parse(row.Monto);
                                        debito = debito + " " + row.DocumentoComercial + " " + row.FechaEmision + " NOTA DEBITO" + " | ";
                                        break;
                                    case "04":
                                        Debito = Debito + decimal.Parse(row.Monto);
                                        debito = debito + " " + row.DocumentoComercial + " " + row.FechaEmision + " FACTURA" + " | ";
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
        public string DownloadAll()
        {
            string texto = "";
            try
            {
                var host = "10.148.174.215";
                var port = 5522;
                var username = "instapag";
                var password = "540144017";
                var remoteDirectory = "/OUT";
                var backupDirectory = "/OUTbackup/";

                //string remoteDirectory = "/RemotePath/";
                string localDirectory = @"C:\Apps\Transax\Repo\RespuestaBanesco\";
                using (var sftp = new SftpClient(host, port, username, password))
                {

                    sftp.Connect();
                    texto = "conecto con el sftp | ";
                    if (sftp.IsConnected)
                    {
                        //Debug.WriteLine("I'm connected to the client");
                        if (sftp.Exists(sftp.WorkingDirectory + remoteDirectory))
                        {
                            texto = texto + "directorio existe" + sftp.WorkingDirectory + remoteDirectory + " | ";
                            //sftp.ChangeDirectory(sftp.WorkingDirectory + remoteDirectory);
                            //texto = texto + "cambie directorio" + sftp.WorkingDirectory + remoteDirectory + "| ";
                            texto = texto + "busco archivos en" + sftp.WorkingDirectory + remoteDirectory + " | ";
                            var files = sftp.ListDirectory(sftp.WorkingDirectory + remoteDirectory);
                            texto = texto + "busque archivos " + sftp.WorkingDirectory + remoteDirectory + "  -> " + files.Count().ToString() + " | ";
                            foreach (var file in files)
                            {
                                texto = texto + "encontre y recorro " + file.Name + " | ";
                                string remoteFileName = file.Name;
                                if ((!file.Name.StartsWith(".")) && ((file.LastWriteTime.Date == DateTime.Today)))
                                {
                                    texto = texto + "encontre y recorro " + localDirectory + remoteFileName + " | ";
                                    try
                                    {
                                        using (Stream file1 = System.IO.File.OpenWrite(localDirectory + remoteFileName))
                                        {

                                            sftp.DownloadFile(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName, file1);
                                            texto = texto + "lo descargue | ";
                                            var inFile = sftp.Get(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName);
                                            texto = texto + "lo muevo " + sftp.WorkingDirectory + backupDirectory + " | ";
                                            inFile.MoveTo(sftp.WorkingDirectory + backupDirectory + remoteFileName);
                                        }
                                    }
                                    catch (Exception e)
                                    {


                                        texto = texto + "intento descargar y fallo : " + e.Message + " | ";

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                texto = texto + e.Message + "  " + e.StackTrace;
                return texto;
            }
            return texto;
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
            //bool win = GenerarCobroBanesco(Lista.ToList());
            bool win = GenerarCobroBanescoFinPago(Lista.ToList());
            //bool winv = GenerarValidacionCobroBanesco(Lista.ToList());
            //if (win && winv)
            //{
            //    foreach (var item in Lista)
            //    {
            //        item.Estatus = 2;
            //        EstadoCuentaREPO.SaveChanges();
            //    }
            //}


            return true;
        }
        public bool GenerarCobroBanesco(List<CP_ArchivoEstadoCuenta> Cobros)
        {
            int cantidadmovimientos = Cobros.Count();
            //string comercio = Cobros.First().AE_Avance.Id;
            //string rif = Cobros.First().AE_Avance.RifCommerce;
            string fechaarchivo = DateTime.Now.AddDays(1).ToString("ddMMyy.hhmm");
            string id = Cobros.FirstOrDefault().Id.ToString();
            if (id.Length > 4)
            {
                id = id.Substring((id.Length - 4), 4);
            }
            else if (id.Length < 4)
            {
                id = id.PadLeft(4, '0');
            }
            string numeroorden = DateTime.Now.AddDays(1).ToString("yyMMdd");
            string _fecha = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            numeroorden = numeroorden + id;
            //datos fijos
            string registro = "00";
            //string idtransaccion = "2020040801";
            string asociado = "208515428";
            string ordencobroreferencia = numeroorden;
            string documento = "DIRDEB";
            string banco = "01";
            string fecha = DateTime.Now.AddDays(1).ToString("yyyyMMddhhmmss");
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
                    _rifc = beneficiario.TipoIdentificacion + beneficiario.Identificacion.PadLeft(9, '0');
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
        public bool GenerarCobroBanescoFinPago(List<CP_ArchivoEstadoCuenta> Cobros)
        {
            //ALIMENTOS POLAR  J000413126 - IBS :540133497
            int cantidadmovimientos = Cobros.Count();
            //string comercio = Cobros.First().AE_Avance.Id;
            //string rif = Cobros.First().AE_Avance.RifCommerce;
            string fechaarchivo = DateTime.Now.AddDays(1).ToString("ddMMyy.hhmm");
            string id = Cobros.FirstOrDefault().Id.ToString();
            if (id.Length > 4)
            {
                id = id.Substring((id.Length - 4), 4);
            }
            else if (id.Length < 4)
            {
                id = id.PadLeft(4, '0');
            }
            string numeroorden = DateTime.Now.AddDays(1).ToString("yyMMdd");
            string _fecha = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            numeroorden = numeroorden + id;
            //datos fijos
            string registro = "00";
            //string idtransaccion = "2020040801";
            string asociado = "540133497";
            string ordencobroreferencia = numeroorden;
            string documento = "DIRDEB";
            string banco = "01";
            string fecha = DateTime.Now.AddDays(1).ToString("yyyyMMddhhmmss");
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
                CP_INI beneficiario = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == cobro.CodigoComercio).FirstOrDefault();
                string _cuenta = "";
                string _nombrecomercial = "";
                string _rifc = "";
                if (beneficiario != null && beneficiario.Id > 0)
                {
                    _cuenta = beneficiario.CuentaBancaria;
                    _nombrecomercial = beneficiario.Nombre;
                    _rifc = beneficiario.RifCliente.PadLeft(9, '0');
                }
                //else
                //{
                //    if (k == 0)
                //    {
                //        _cuenta = "01340373233733019371";
                //        _nombrecomercial = "Carmelo Larez";
                //        _rifc = "V018601098";
                //    }
                //    else if (k == 1)
                //    {
                //        _cuenta = "01340874278743016046";
                //        _nombrecomercial = "Alexyomar Istruriz";
                //        _rifc = "V017302339";
                //    }
                //    else
                //    {
                //        _cuenta = "01340373233733019371";
                //        _nombrecomercial = "Carmelo Larez";
                //        _rifc = "V018601098";

                //    }
                //    k++;
                //}
                string tipo = "03";
                string recibo = cobro.ReferenciaItem.Substring(2, (cobro.ReferenciaItem.Length - 2)).ToString().PadLeft(8, '0');
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
            string _rif = "J000413126";
            string ordenante = "ALIMENTOS PORLAR COMERCIAL C A";
            //decimal cambio = Math.Round(Cobros.Sum(y => y.Monto), 2);
            //cambio = cambio * 100;
            string _montoabono = total.ToString().Split(',')[0];
            string _moneda = "VES";
            string _numerocuenta = "01340375375101351400";
            string _swift = "BANSVECA";
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
            string ruta = @"C:\Users\carmelo\Desktop\POLAR\CargoCuentaBanesco\" + "I0005.540133497." + fechaarchivo + ".txt";
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
                    string _documento = beneficiario.Identificacion.PadLeft(10, '0');
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

        public class PAG
        {
            public string departamento { get; set; }
            public PAGLinea1 Linea1 { get; set; }
            public List<PAGLinea2> Linea2 { get; set; }
            public PAGLinea3 Linea3 { get; set; }
        }
        public class PAGLinea1
        {
            public string ReferenciaPago { get; set; }

            public string NumeroRegistro { get; set; }

            public string CodigoCliente { get; set; }

            public string TipoPagador { get; set; }


            public string IdentificacionPagador { get; set; }

            public string RazonSocial { get; set; }
            public string EstatusPago { get; set; }
            public string CuentaCliente { get; set; }

        }
        public class PAGLinea2
        {
            public string Referenciapago { get; set; }

            public string NumeroRegistro { get; set; }
            public string NumeroDocumento { get; set; }

            public string TipoDocumento { get; set; }
            public string Referencia { get; set; }

            public string FechaVencimiento { get; set; }
            public string FechaPago { get; set; }
            public string MontoIva { get; set; }
            public string MontoRetencion { get; set; }

            public string MontoNeto { get; set; }
            public string NumeroComprobanteIR { get; set; }
            public string FechaEmisionComporbanteIR { get; set; }


        }
        public class PAGLinea3
        {
            public string Referenciapago { get; set; }

            public string NumeroRegistro { get; set; }
            public string CodigoDepartamento { get; set; }

            public string TipoTransaccion { get; set; }
            public string NumerocCuenta { get; set; }

            public string Subtotal { get; set; }
            public string Totaldebito { get; set; }
            public string TotalCargootrosBancos { get; set; }
            public string TotaltarjetaCredito { get; set; }

            public string SubtotalCheques { get; set; }
            public string SubtotalEfectivo { get; set; }
            public string TotalDeposito { get; set; }


        }

        [DelimitedRecord("|")]
        public class _EstructuraCLI
        {
            [FieldOptional]
            [FieldOrder(1)]
            public string CodigoCliente { get; set; }
            [FieldOptional]
            [FieldOrder(2)]
            public string CodigoMaster { get; set; }
            [FieldOptional]
            [FieldOrder(3)]
            public string PermisoMaster { get; set; }
            [FieldOptional]
            [FieldOrder(4)]
            public string Nombre { get; set; }
            [FieldOptional]
            [FieldOrder(5)]
            public string RazonSocial { get; set; }
            [FieldOptional]
            [FieldOrder(6)]
            public string IdentificacionPagador { get; set; }
            [FieldOptional]
            [FieldOrder(7)]
            public string Region { get; set; }
            [FieldOptional]
            [FieldOrder(8)]
            public string Correo { get; set; }
            [FieldOptional]
            [FieldOrder(9)]
            public string Telefono { get; set; }
            [FieldOptional]
            [FieldOrder(10)]
            public string TipoIdentificacion { get; set; }

            [FieldOptional]
            [FieldOrder(11)]
            public string CodigoEstatus { get; set; }
            [FieldOptional]
            [FieldOrder(12)]
            public string IRIVA { get; set; }

            [FieldOptional]
            [FieldOrder(13)]
            public string CodigoDepartamento { get; set; }
            //[FieldOptional]
            //[FieldOrder(14)]
            //public string DescripcionRechazo { get; set; }
        }


        [DelimitedRecord("|")]
        public class _EstructuraCOB
        {
            [FieldOptional]
            [FieldOrder(1)]
            public string CodigoCliente { get; set; }
            [FieldOptional]
            [FieldOrder(2)]
            public string DocumentoComercial { get; set; }
            [FieldOptional]
            [FieldOrder(3)]
            public string Referencia { get; set; }
            [FieldOptional]
            [FieldOrder(4)]
            public string CodigoDepartamento { get; set; }
            [FieldOptional]
            [FieldOrder(5)]
            public string Monto { get; set; }
            [FieldOptional]
            [FieldOrder(6)]
            public string FechaEmision { get; set; }
            [FieldOptional]
            [FieldOrder(7)]
            public string Fechavencimiento { get; set; }
            [FieldOptional]
            [FieldOrder(8)]
            public string TipoDocumento { get; set; }
            [FieldOptional]
            [FieldOrder(9)]
            public string x { get; set; }
            [FieldOptional]
            [FieldOrder(10)]
            public string MontoIva { get; set; }

            [FieldOptional]
            [FieldOrder(11)]
            public string Concepto { get; set; }
            [FieldOptional]
            [FieldOrder(12)]
            public string OBS { get; set; }

            [FieldOptional]
            [FieldOrder(13)]
            public string DescripcionError { get; set; }
            [FieldOptional]
            [FieldOrder(14)]
            public string CodigoError { get; set; }
        }
        public class EstructuraCOB
        {

            public string CodigoCliente { get; set; }
            public string DocumentoComercial { get; set; }
            public string Referencia { get; set; }

            public string CodigoDepartamento { get; set; }
            public string Monto { get; set; }

            public string FechaEmision { get; set; }

            public string Fechavencimiento { get; set; }

            public string TipoDocumento { get; set; }

            public string x { get; set; }

            public string MontoIva { get; set; }

            public string Concepto { get; set; }

            public string OBS { get; set; }

            public string DescripcionError { get; set; }

            public string CodigoError { get; set; }
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
        public class EstructuraINI
        {
            public string Fecha { get; set; }
            public string RifEmpresa { get; set; }
            public string Departamento { get; set; }
            public string CodigoCliente { get; set; }
            public string RifCliente { get; set; }
            public string CuentaBancaria { get; set; }
            public string Nombre { get; set; }
            public string CodigoError { get; set; }

            public string ErrorDescripcion { get; set; }

        }
        #endregion
    }
}
