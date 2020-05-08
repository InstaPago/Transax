using BLL.Concrete;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.BackEnd;
using InstaTransfer.ITLogic;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITLogic.Security;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.ITResources.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Umbrella.App_Code;
using Umbrella.Models;
using static InstaTransfer.ITLogic.Helpers.SessionHelper;

namespace Umbrella.Controllers
{
    [Authorize(Roles = UserRoleConstant.TransaXAdmin + "," + UserRoleConstant.CommerceAdmin)]
    public class CUserController : Controller
    {
        #region Variables
        URepository<CUser> CURepo = new URepository<CUser>();

        URepository<AspNetUserRole> Roles = new URepository<AspNetUserRole>();

        BaseSuccessResponse _baseSuccessResponse;
        BaseErrorResponse _baseErrorResponse;
        private ApplicationRoleManager _roleManager;
        private ApplicationUserManager _userManager;
        List<string> errorList = new List<string>();

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
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
        public CUserController()
        {
        }

        public CUserController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }
        #endregion

        // GET: CUser
        [SessionExpireFilter]
        public ActionResult Index()
        {
            ViewBag.userid = User.Identity.GetUserId().ToString();
            var userList = GetCUsers();
            var data = GetData();
            var roles = GetRoles();
            List<Models.SelectTypehead> SelectedListRoomTypePeriod = new List<Models.SelectTypehead>();
            foreach (var Item in roles)
            {
                SelectedListRoomTypePeriod.Add(new Models.SelectTypehead
                {
                    id = Item.Id.Trim(),
                    name = Item.Name.ToString()
                });
            }


            CUser user = new CUser();

            ViewBag.Roles = roles;
            ViewBag.RolesJson = Newtonsoft.Json.JsonConvert.SerializeObject(SelectedListRoomTypePeriod, Newtonsoft.Json.Formatting.None, new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
            ViewBag.UsersData = data;

            return View(userList);



        }
        [HttpPost]
        [SessionExpireFilter]
        public ActionResult AddCUser()
        {
            var roles = GetRoles();
            List<Models.SelectTypehead> SelectedListRoomTypePeriod = new List<Models.SelectTypehead>();
            foreach (var Item in roles)
            {
                SelectedListRoomTypePeriod.Add(new Models.SelectTypehead
                {
                    id = Item.Id.Trim(),
                    name = Item.Name.ToString()
                });
            }


            CUser user = new CUser();

            ViewBag.Roles = roles;
            return PartialView("_AddCUser");
        }
        //[HttpPost]
        //[ValidateInput(false)]
        //public ActionResult Details(object data, string id)
        //{
        //    CUser user;

        //    using (CUserBLL CUBLL = new CUserBLL())
        //    {
        //        user = CUBLL.GetCUser(id);
        //    }

        //    ViewBag.data = data;
        //    ViewBag.userId = id;
        //    return PartialView("_Details", user);
        //}

        #region CRUD
        /// <summary>
        /// Crea usuarios para el comercio
        /// </summary>
        /// <param name="model">Datos del usuario a crear.</param>
        /// <returns>Resultado de la operacion. True - Exito. False - Fallido.</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> CreateCUser(RegisterViewModel model)
        {
            ApplicationUser newUser = new ApplicationUser();
            CUser newCUser;

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
                _baseErrorResponse = new BaseErrorResponse(errorList, BackEndErrors.InvalidUserExceptionCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    url = Url.Action("Index", "CUser", new { isCreated = false })
                };
                return new JsonResult { Data = badresult };
            }

            try
            {
                // Creamos el objeto usuario de asp
                newUser = new ApplicationUser { UserName = model.Email, Email = model.Email };
                // Creamos el usuario en la bd
                var result = await UserManager.CreateAsync(newUser, model.Password);
                
                switch (result.Succeeded)
                {
                    case true:
                        {
                            // Agregamos el usuario al rol especifico
                            //UserManager.AddToRole(newUser.Id, model.commerceUserModel.Role);
                            AspNetUserRole _item = new AspNetUserRole();
                            _item.RoleId = model.commerceUserModel.Role;
                            _item.UserId = newUser.Id;
                            Roles.AddEntity(_item);
                            Roles.SaveChanges();
                            break;
                        }
                    case false:
                        {
                            throw new InvalidUserException(BackEndErrors.InvalidUserExceptionCode, string.Join(" ", result.Errors));
                        }
                    default:
                        break;
                }

                // Creamos el usuario del comercio
                newCUser = new CUser
                {
                    Id = Guid.NewGuid(),
                    Name = model.commerceUserModel.Name,
                    LastName = model.commerceUserModel.LastName,
                    IdAspNetUser = newUser.Id,
                    RifCommerce = CURepo.GetCUser(User.Identity.GetUserId()).RifCommerce,
                    IdCUserStatus = (int)CommerceUserStatus.Active,
                    StatusChangeDate = DateTime.Now,
                    CreateDate = DateTime.Now,
                    /// Todo(Create Cuser): Obtener el modo de prueba desde el switchery a traves del modelo
                    TestMode = true
                    //TestMode = model.commerceUserModel.TestMode
                };
                // Agregamos el usuario del comercio a la bd
                CURepo.AddEntity(newCUser);
                // Guardamos los cambios a la bd
                CURepo.SaveChanges();
            }
            catch (InvalidUserException e)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(e.ErrorCode, e.MessageException);
                // Retornamos el resultado
                return new JsonResult()
                {
                    Data = new
                    {
                        success = _baseErrorResponse.Success,
                        message = _baseErrorResponse.ResponseMessage,
                        url = Url.Action("Index", "CUser", new { isCreated = false })
                    }
                };
            }
            catch (Exception e)
            {
                // Devolvemos los cambios de la creacion de usuarios
                var result = UserManager.DeleteAsync(newUser);
                // Error
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.InvalidUserExceptionCode, BackEndErrors.InvalidUserExceptionMessage);
                // Retornamos el resultado
                return new JsonResult()
                {
                    Data = new
                    {
                        success = _baseErrorResponse.Success,
                        message = _baseErrorResponse.ResponseMessage,
                        url = Url.Action("Index", "CUser", new { isCreated = false })
                    }
                };
            }
            // Success
            _baseSuccessResponse = new BaseSuccessResponse(BackEndResources.CreateCUserSuccessMessage);
            // Retornamos el resultado
            return new JsonResult()
            {
                Data = new
                {
                    success = _baseSuccessResponse.Success,
                    message = _baseSuccessResponse.Message,
                    url = Url.Action("Index", "CUser", new { isCreated = true })
                }
            };
        }

        /// <summary>
        /// Modifica un usuario del comercio
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public JsonResult ModifyCUser(ModifyCUserModel model)
        {
            CUser cUser = new CUser();
            int ContactUsers = 0;
            
            try
            {
                var role = RoleManager.FindById(model.RoleId);
                // Obtenemos el usuario a partir del id
                using (CUserBLL CUBLL = new CUserBLL())
                {
                    cUser = CUBLL.GetEntity(model.id);

                    // Guardamos los campos modificados
                    cUser.Name = model.Name.Trim();
                    cUser.LastName = model.LastName.Trim();
                    cUser.TestMode = model.TestMode;
                    // Modificamos el rol
                    UserManager.RemoveFromRoles(cUser.AspNetUser.Id, UserManager.GetRoles(cUser.AspNetUser.Id.ToString()).ToArray());
                    UserManager.AddToRole(cUser.AspNetUser.Id, role.Name);
                    //cUser.AspNetUser.AspNetUserRoles.FirstOrDefault().RoleId = model.RoleId;

                    // Revisamos si existe otro usuario de contacto
                    ContactUsers = CUBLL.GetAllRecords(cu => cu.IsContact).Count();

                    if (ContactUsers == 0)
                    {
                        cUser.IsContact = model.IsContact;
                    }

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
        [HttpPost]
        public ActionResult SideUsers(string userID)
        {
            // Obtenemos el usuario desde el id del usuario actual
            var _CUser = GetCUser(userID);

            if (_CUser.Id == MySession.Current.CommerceUser.Id)
            {
                // Retornamos la vista parcial con el usuario actual
                return PartialView("_SideUsers", _CUser);
            }
            else
            {
                // Construimos el modelo
                var userModel = new ModifyCUserModel
                {
                    id = _CUser.Id,
                    Email = _CUser.AspNetUser.Email,
                    Name = _CUser.Name.Trim(),
                    LastName = _CUser.LastName.Trim(),
                    RoleId = _CUser.AspNetUser.AspNetUserRoles.FirstOrDefault().AspNetRole.Id,
                    Status = _CUser.CUserStatus,
                    TestMode = _CUser.TestMode,
                    IsContact = _CUser.IsContact
                };

                // Obtenemos los roles
                var roles = GetRoles();
                ViewBag.Roles = roles;

                // Retornamos la vista de modificacion de usuario
                return PartialView("_ModifyCUser", userModel);
            }

        }

        #endregion

        #region DataAccess

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
            cUser = CURepo.GetCUser(userID);
            // Retornamos el usuario del comercio
            return cUser;
        }

        public List<CUser> GetCUsers()
        {
            List<CUser> users = new List<CUser>();
            var currentUser = CURepo.GetCUser(User.Identity.GetUserId());


            if (User.IsInRole(UserRoleConstant.TransaXAdmin))
            {
                users = CURepo.GetAllRecords(cu => cu.AspNetUser.AspNetUserRoles.FirstOrDefault().AspNetRole.Name.Contains("TransaX")).ToList();
            }
            else if (User.IsInRole(UserRoleConstant.CommerceAdmin))
            {
                users = CURepo.GetAllRecords(cu => cu.AspNetUser.AspNetUserRoles.FirstOrDefault().AspNetRole.Name.Contains("Commerce") && cu.RifCommerce == currentUser.RifCommerce).ToList();
            }



            return users;
        }

        public List<AspNetRole> GetRoles()
        {
            var roleList = new List<AspNetRole>();
            // Obtenemos todos los roles de la BD
            var ANRRepo = new Repository<AspNetRole>();
            roleList = ANRRepo.GetAllRecords().ToList();

            if (User.IsInRole(UserRoleConstant.CommerceAdmin) || User.IsInRole(UserRoleConstant.CommerceUser))
            {
                return roleList.Where(r => r.Name.StartsWith("Commerce")).ToList();
            }
            else if (User.IsInRole(UserRoleConstant.TransaXAdmin) || User.IsInRole(UserRoleConstant.TransaXUser))
            {
                return roleList.Where(r => r.Name.StartsWith("TransaX")).ToList();
            }
            return roleList;
        }

        public List<USocialReason> GetSocialReasons()
        {
            List<USocialReason> socialReasonList = new List<USocialReason>();
            Command commandGetAllSocialReasons;
            try
            {
                //Obtiene la instancia del comando
                commandGetAllSocialReasons = CommandFactory.GetCommandGetAllSocialReasons();
                //ejecuta el comando deseado
                commandGetAllSocialReasons.Execute();
                //asigno el resultado a la lista de razon social
                socialReasonList = (List<USocialReason>)commandGetAllSocialReasons.Receiver;
                //reviso si la lista de razon social no esta vacia
                if (socialReasonList == null)
                {
                    //throw new NullEntryListException();
                }

            }
            catch (Exception)
            {
                // No hacer nada
            }
            return socialReasonList;
        }
        #endregion

        #region Methods

        public List<string[]> GetData()
        {

            List<CUser> users = GetCUsers();
            int __counttotal = users.Count;
            var result = from u in users
                         select new[] {
                           u.Name.Trim(),
                           u.LastName.Trim(),
                           u.AspNetUser.UserName,
                           UserManager.GetRoles(u.AspNetUser.Id).FirstOrDefault(),
                           u.CUserStatus.Description.Trim(),
                           u.TestMode.ToString().Trim(),
                           u.Id.ToString()
                        };
            List<string[]> resultList = result.ToList();

            return resultList;
        }

        #endregion

    }
}
