using BLL.Concrete;
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
using Umbrella.App_Code;
using Umbrella.Models;
using static InstaTransfer.ITLogic.Helpers.SessionHelper;

namespace Umbrella.Controllers
{
    [Authorize(Roles =
        UserRoleConstant.TransaXAdmin + "," +
        UserRoleConstant.TransaXUser + "," +
        UserRoleConstant.CommerceAdmin + "," +
        UserRoleConstant.CommerceUser + ","
        )]
    public class ProfileController : Controller
    {
        #region Variables
        CommerceBLL CBLL = new CommerceBLL();
        CUserBLL CUBLL = new CUserBLL();

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
            var userdata = GetCUser(User.Identity.GetUserId().ToString());
            ViewBag.userid = User.Identity.GetUserId().ToString();
            return View(userdata);
        }
        [SessionExpireFilter]
        public ActionResult ProfilePanel()
        {
            var userdata = GetCUser(User.Identity.GetUserId().ToString());
            ViewBag.role = GetCUserRole(userdata.Id);
            ViewBag.userid = User.Identity.GetUserId().ToString();
            return PartialView("_ProfilePanel", userdata);
        }

        /// <summary>
        /// Tab de los datos del usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns></returns>
        [HttpPost]
        [SessionExpireFilter]
        public ActionResult TabUserData(Guid userId)
        {
            var user = MySession.Current.CommerceUser;

            return PartialView("_TabUserData", user);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult ModifyCUser(ModifyCUserModel model)
        {
            CUser user = new CUser();
            try
            {
                // Obtenemos el usuario a partir del id
                using (CUserBLL CUBLL = new CUserBLL())
                {
                    user = CUBLL.GetEntity(model.id);
                    // Guardamos los campos modificados
                    user.Name = model.Name;
                    user.LastName = model.LastName;

                    // Salvamos los cambios en base de datos
                    CUBLL.SaveChanges();
                }
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
            // Success
            _baseSuccessResponse = new BaseSuccessResponse(BackEndResources.BackEndSuccessMessage);
            var result = new
            {
                success = _baseSuccessResponse.Success,
                message = _baseSuccessResponse.Message
            };
            return new JsonResult { Data = result };
        }

        #region DataAccess

        public AspNetRole GetCUserRole(Guid id)
        {
            // Obtenemos el usuario desde el id del usuario actual
            var cuser = CUBLL.GetEntity(id);
            // Obtenemos el AspNetRole del usuario
            var role = cuser.AspNetUser.AspNetUserRoles.FirstOrDefault().AspNetRole;
            // Retornamos el AspNetRole
            return role;
        }

        /// <summary>
        /// Devuelve el comercio por un rif especifido
        /// </summary>
        /// <param name="id">Rif del comercio</param>
        /// <returns><see cref="Commerce"/> especifico</returns>
        public Commerce GetCommerce(string rif)
        {
            Commerce commerce = new Commerce();

            commerce = CBLL.GetCommerce(rif);

            return commerce;
        }
        /// <summary>
        /// Retorna la lista de usuarios del comercio actual
        /// </summary>
        /// <returns>Lista de <see cref="CUser"/></returns>
        public List<CUser> GetCUsers(string rif)
        {
            // Seteamos las variables
            List<CUser> cUserList = new List<CUser>();

            // Obtenemos la lista de declaraciones asociadas al comercio perteneciente al usuario actual
            cUserList = CUBLL.GetCUsers(rif).ToList();

            return cUserList;
        }
        /// <summary>
        /// Obtiene el usuario del comercio desde un id especifico
        /// </summary>
        /// <param name="userID">Id del usuario del comercio</param>
        /// <returns><see cref="CUser"/> del comercio</returns>
        public CUser GetCUser(string userID)
        {
            // Inicializamos el usuario
            CUser cUser = new CUser();
            // Obtenemos el usuario desde la base de datos
            cUser = CUBLL.GetCUser(userID);
            // Retornamos el usuario del comercio
            return cUser;
        }

        #endregion

    }


}