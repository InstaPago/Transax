using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models;
using InstaTransfer.BLL.Models.Commerce;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Constants;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PaymentRequest.Models;
using BLL.Concrete;
using static InstaTransfer.ITLogic.Helpers.SessionHelper;
using InstaTransfer.ITResources.Bennington;
using Umbrella.App_Code;

namespace PaymentRequest.Controllers
{
    [Authorize(Roles =
        UserRoleConstant.PaymentRequestUser
        )]
    public class ProfileController : Controller
    {
        #region Variables

        EndUser user = MyPRSession.Current.EndUser;
        BaseSuccessResponse _baseSuccessResponse;
        BaseErrorResponse _baseErrorResponse;

        private ApplicationUserManager _userManager;
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

        // GET: Profile
        [SessionExpireFilter]
        public ActionResult Index()
        {
            return View(user);
        }

        public ActionResult ProfilePanel()
        {
            return PartialView("_ProfilePanel", user);
        }

        [HttpPost]
        public ActionResult TabUserData()
        {
            return PartialView("_TabUserData", user);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult ModifyUser(ModifyEndUserModel model)
        {
            EndUser currentUser = new EndUser();
            var errorList = new List<string>();

            try
            {
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
                using (EndUserBLL EUBLL = new EndUserBLL())
                {
                    // Obtenemos el usuario desde la sesion
                    currentUser = EUBLL.GetEndUserByCI(user.CI);
                    // Guardamos los campos modificados
                    currentUser.Name = model.Name;
                    currentUser.LastName = model.LastName;
                    currentUser.AspNetUser.Email = model.Email;
                    currentUser.Phone = model.Phone;
                    // Salvamos los cambios en base de datos
                    EUBLL.SaveChanges();
                }
                MyPRSession.Current.EndUser = currentUser;
            }
            catch (Exception)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(BenningtonErrors.ModifyUserErrorMessage, BenningtonErrors.ModifyUserErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                return new JsonResult { Data = badresult };
            }
            // Success
            _baseSuccessResponse = new BaseSuccessResponse(BenningtonResources.ModifyUserSuccess);
            var result = new
            {
                success = _baseSuccessResponse.Success,
                message = _baseSuccessResponse.Message
            };
            return new JsonResult { Data = result };
        }
    }


}