using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models;
using InstaTransfer.BLL.Models.Commerce;
using InstaTransfer.BLL.Models.PurchaseOrder;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.BackEnd;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.ITResources.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Umbrella.Models;
using Microsoft.Office.Interop.Excel;
using Rotativa;
using Rotativa.Options;
using System.IO;
using Umbrella.App_Code;
using BLL.Concrete;
using System.Configuration;

namespace Umbrella.Controllers
{
    [Authorize(Roles =
        UserRoleConstant.TransaXAdmin + "," +
        UserRoleConstant.TransaXUser + "," +
        UserRoleConstant.CommerceAdmin + "," +
        UserRoleConstant.CommerceUser
        )]
    public class CommerceController : Controller
    {
        #region Variables
        CommerceBLL CBLL = new CommerceBLL();
        CUserBLL CUBLL = new CUserBLL();
        CPurchaseOrderBLL CPOBLL = new CPurchaseOrderBLL();
        DeclarationBLL DBLL = new DeclarationBLL();

        URepository<AE_MovimientosCuenta> AEmovimientosREPO = new URepository<AE_MovimientosCuenta>();
        URepository<AE_Propuesta> AE_PropuestaREPO = new URepository<InstaTransfer.DataAccess.AE_Propuesta>();
        URepository<AE_Avance> AE_AvanceREPO = new URepository<InstaTransfer.DataAccess.AE_Avance>();
        URepository<AE_EstadoCuenta> AE_EstadoCuentaREPO = new URepository<InstaTransfer.DataAccess.AE_EstadoCuenta>();
        URepository<AE_MovimientosDebito> AE_MovimientosDebitoREPO = new URepository<InstaTransfer.DataAccess.AE_MovimientosDebito>();
        URepository<CommerceBankAccount> CommerceInfoBankREPO = new URepository<InstaTransfer.DataAccess.CommerceBankAccount>();
        URepository<AE_UsuarioBanco> AE_UsuarioBancoREPO = new URepository<InstaTransfer.DataAccess.AE_UsuarioBanco>();
        URepository<AE_SolicitudApi> AE_SolicitudApiREPO = new URepository<InstaTransfer.DataAccess.AE_SolicitudApi>();
        URepository<AE_ProcesoSolicitudApi> AE_ProcesoSolicitudApiREPO = new URepository<InstaTransfer.DataAccess.AE_ProcesoSolicitudApi>();
        URepository<AE_Dolar> AE_DolarREPO = new URepository<InstaTransfer.DataAccess.AE_Dolar>();
        URepository<AE_ValorAccionTR> AE_ValorAccionTRREPO = new URepository<AE_ValorAccionTR>();
        URepository<AE_ArchivoUpload> AE_ArchivoUploadREPO = new URepository<AE_ArchivoUpload>();
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

        // GET: Commerce
        [SessionExpireFilter]
        public ActionResult Index()
        {
            List<Commerce> commerceList = GetCommerceList();
            return View(commerceList);
        }
        [SessionExpireFilter]
        public ActionResult Details(string id)
        {
            ViewBag.commerceRif = id;
            var commerce = GetCommerce(id);
            ViewBag.userid = User.Identity.GetUserId().ToString();

            return View(commerce);
        }
        [HttpPost]
        [ValidateInput(false)]
        [SessionExpireFilter]
        public ActionResult Details(object data, string rif)
        {
            ViewBag.data = data;
            ViewBag.commerceId = rif;
            return PartialView("_Details");
        }

        #region Tabs
        [HttpPost]
        [SessionExpireFilter]
        public ActionResult TabCommerceData(string rif)
        {
            ViewBag.cUser = GetContactUser(rif);

            var commerce = GetCommerce(rif);

            return PartialView("_TabCommerceData", commerce);
        }

        [Authorize(Roles =
        UserRoleConstant.TransaXAdmin + "," +
        UserRoleConstant.TransaXUser + ","
        )]
        [HttpPost]
        public ActionResult TabDeclarations(string rif)
        {
            var declarations = GeDeclarations(rif);

            return PartialView("_TabDeclarations", declarations);
        }
        [HttpGet]
        public ActionResult UsersTable(List<UDeclaration> declarations)
        {
            return PartialView("_DeclarationsTable", declarations);
        }
        [HttpPost]
        public ActionResult TabOrders(string rif)
        {
            var purchaseOrders = GetPurchaseOrders(rif);

            return PartialView("_TabOrders", purchaseOrders);
        }
        [HttpPost]
        public ActionResult TabUsers(string rif)
        {
            var cUsers = GetCUsers(rif);

            return PartialView("_TabUsers", cUsers);
        }
        [HttpGet]
        public ActionResult UsersTable(List<CUser> cuser)
        {
            return PartialView("_UsersTable", cuser);
        }

        #endregion

        #region CRUD
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult ModifyCommerce(ModifyCommerceModel model)
        {
            Commerce commerce = new Commerce();
            try
            {
                // Obtenemos el comercio a partir del rif
                commerce = GetCommerce(model.Rif);
                // Guardamos los campos modificados
                commerce.SocialReasonName = model.SocialReasonName;
                commerce.Phone = model.Phone;
                commerce.BusinessName = model.BusinessName;
                commerce.Address = model.Address;
                // Verificamos que el usuario este en el rol correcto
                if (User.IsInRole(UserRoleConstant.TransaXAdmin) || User.IsInRole(UserRoleConstant.TransaXUser))
                {
                    commerce.WithdrawalFee = decimal.Parse(model.WithdrawalFee.Trim());

                    commerce.Trust = decimal.Parse(model.Trust.Trim());
                }
                // Salvamos los cambios en base de datos
                CBLL.SaveChanges();
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

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult ModifyPurchaseOrder(PurchaseOrderModel model)
        {
            CPurchaseOrder purchaseOrder = new CPurchaseOrder();
            List<UDeclaration> declarations;

            try
            {
                // Obtenemos la orden a partir del rif y id
                purchaseOrder = GetPurchaseOrder(model.rif, model.id);
                // Obtenemos la lista de declaraciones de la orden
                declarations = purchaseOrder.UDeclarations.ToList();
                // Anulamos todas las declaraciones asociadas
                foreach (var declaration in declarations)
                {
                    // Anulamos cada declaracion asociada
                    var _result = DBLL.TryAnnulDeclaration(purchaseOrder.RifCommerce, declaration.Id, false);
                    // Verificamos el estado del resultado
                    //var IsAnulled = _result.Data.
                    // Verificamos el resultado de la operación
                    //if (!_result.Data)
                    //{
                    //    throw new PurchaseOrderDeclarationException(BackEndErrors.PurchaseOrderDeclarationExceptionMessage, BackEndErrors.PurchaseOrderDeclarationExceptionCode);
                    //}
                }
                // Guardamos los campos modificados
                purchaseOrder.Amount = model.amount;
                purchaseOrder.EndUserCI = model.paymentuser.userci;
                purchaseOrder.EndUserEmail = model.paymentuser.useremail;
                purchaseOrder.OrderNumber = model.ordernumber;
                // Salvamos los cambios en base de datos
                CPOBLL.SaveChanges();
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
            // Success
            _baseSuccessResponse = new BaseSuccessResponse(BackEndResources.BackEndSuccessMessage);
            var result = new
            {
                success = _baseSuccessResponse.Success,
                message = _baseSuccessResponse.Message
            };
            return new JsonResult { Data = result };
        }

        #endregion

        #region Methods

        #region SidebarDetails

        [HttpPost]
        public ActionResult SideUsers(string userID)
        {
            // Obtenemos el usuario desde el id del usuario actual
            var cUser = GetCUser(userID);
            // Retornamos la vista prcial con el usuario actual
            return PartialView("_SideUsers", cUser);
        }
        #endregion

        #region DataAccess

        /// <summary>
        /// Obtiene el rol del usuario del comercio especifico
        /// </summary>
        /// <param name="id">Id del usuario del comercio especifico</param>
        /// <returns><see cref="AspNetRole"/> del usuario del comercio</returns>
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
        /// Devuelve todos los comercios en la base de datos
        /// </summary>
        /// <returns>Lista de <see cref="Commerce"/></returns>
        public List<Commerce> GetCommerceList()
        {
            List<Commerce> commerceList = new List<Commerce>();

            commerceList = CBLL.GetAllRecords().ToList();

            return commerceList;
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
        /// Devuelve el usuario de contacto del comercio especifico
        /// </summary>
        /// <param name="id">Rif del comercio</param>
        /// <returns><see cref="CUser"/> de contacto</returns>
        public CUser GetContactUser(string rif)
        {
            CUser cUser = new CUser();

            cUser = CUBLL.GetAllRecords(c => c.IsContact.Equals(true) && c.RifCommerce == rif).FirstOrDefault();

            return cUser;
        }

        /// <summary>
        /// Retorna la declaracion seleccionada por rif del comercio y un id especifico
        /// </summary>
        /// <param name="rif">Comercio asociado a la declaracion</param>
        /// <param name="idDeclaration">Id de la declaracion especifica</param>
        /// <returns><see cref="UDeclaration"/> especifica</returns>
        public UDeclaration GeDeclaration(string rif, Guid idDeclaration)
        {
            // Seteamos las variables
            UDeclaration declaration = new UDeclaration();

            // Obtenemos la declaracion asociada al comercio y por el id desde la base de datos
            declaration = DBLL.GetDeclaration(rif, idDeclaration);

            return declaration;
        }

        /// <summary>
        /// Retorna la lista de declaraciones para el comercio actual
        /// </summary>
        /// <returns>Lista de declaraciones</returns>
        public List<UDeclaration> GeDeclarations(string rif)
        {
            // Seteamos las variables
            List<UDeclaration> declarationList = new List<UDeclaration>();

            // Obtenemos la lista de declaraciones asociadas al comercio perteneciente al usuario actual
            declarationList = DBLL.GetDeclarations(rif).ToList();

            return declarationList;
        }

        /// <summary>
        /// Retorna la lista de declaraciones asociadas a una orden de compra para el comercio actual
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="cPurchaseOrderId">Id de la orden de compra</param>
        /// <returns>Lista de <see cref="CPurchaseOrder"/></returns>
        public List<UDeclaration> GeDeclarations(Guid cPurchaseOrderId, string rif)
        {
            // Seteamos las variables
            List<UDeclaration> declarationList = new List<UDeclaration>();

            // Obtenemos la lista de declaraciones asociadas al comercio perteneciente al usuario actual
            declarationList = DBLL.GetDeclarations(cPurchaseOrderId, rif);

            return declarationList;
        }

        /// <summary>
        /// Retorna la lista de ordenes de compra para el comercio actual
        /// </summary>
        /// <returns>Lista de <see cref="CPurchaseOrder"/></returns>
        public List<CPurchaseOrder> GetPurchaseOrders(string rif)
        {
            // Seteamos las variables
            List<CPurchaseOrder> purchaseOrderList = new List<CPurchaseOrder>();

            // Obtenemos la lista de declaraciones asociadas al comercio perteneciente al usuario actual
            purchaseOrderList = CPOBLL.GetPurchaseOrders(rif).ToList();

            return purchaseOrderList;
        }

        /// <summary>
        /// Retorna la orden de compra seleccionada por rif del comercio y un id especifico
        /// </summary>
        /// <param name="rif">Comercio asociado a la orden de compra</param>
        /// <param name="idDeclaration">Id de la orden de compra especifica</param>
        /// <returns><see cref="CPurchaseOrder"/> especifica</returns>
        public CPurchaseOrder GetPurchaseOrder(string rif, Guid idPurchaseOrder)
        {
            // Seteamos las variables
            CPurchaseOrder purchaseOrder = new CPurchaseOrder();

            // Obtenemos la declaracion asociada al comercio y por el id desde la base de datos
            purchaseOrder = CPOBLL.GetPurchaseOrder(rif, idPurchaseOrder);

            return purchaseOrder;
        }

        /// <summary>
        /// Retorna la lista de ordenes de compra asociadas a una declaracion para el comercio actual
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="declarationId">Id de la declaracion asociada</param>
        /// <returns>Lista de <see cref="UDeclaration"/></returns>
        public CPurchaseOrder GetPurchaseOrder(Guid declarationId, string rif)
        {
            // Seteamos las variables
            CPurchaseOrder purchaseOrder = new CPurchaseOrder();

            // Obtenemos la orden de compra asociada a la declaracion perteneciente al usuario actual
            purchaseOrder = CPOBLL.GetPurchaseOrder(declarationId, rif);

            return purchaseOrder;
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

        #region Logic

        /// <summary>
        /// Cambia el estado del comercio entre activo e inactivo
        /// </summary>
        /// <param name="rif">Rif del comercio a modificar</param>
        /// <returns>Resultado de la operacion</returns>
        public JsonResult ToggleCommerceStatus(string rif)
        {
            // Inicializamos las variables y obtenemos la data
            Commerce commerce = GetCommerce(rif);
            int cStatus = commerce.IdCommerceStatus;

            try
            {
                // Cambiamos los estados del comercio segun el estado actual
                switch (cStatus)
                {
                    case (int)InstaTransfer.ITResources.Enums.CommerceStatus.Active:
                        {
                            commerce.IdCommerceStatus = (int)InstaTransfer.ITResources.Enums.CommerceStatus.Inactive;
                            break;
                        }
                    case (int)InstaTransfer.ITResources.Enums.CommerceStatus.Inactive:
                        {
                            commerce.IdCommerceStatus = (int)InstaTransfer.ITResources.Enums.CommerceStatus.Active;
                            break;
                        }
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                //Devolvemos el error
                return new JsonResult()
                {
                    Data = new
                    {
                        Success = false,
                        Msg = "Modificación del estado fallida"
                    }
                };
            }
            // Guardamos los cambios en la base de datos
            CBLL.SaveChanges();
            //Devolvemos la respuesta de exito
            return new JsonResult()
            {
                Data = new
                {
                    Success = true,
                    Msg = "Modificacion del estado exitosa"
                }
            };

        }

        #endregion

        #endregion

        #region AE

        public JsonResult ChangeStatusBank(string IdBankAccount)
        {
            // Inicializamos las variables y obtenemos la data
            InstaTransfer.DataAccess.CommerceBankAccount bank = CommerceInfoBankREPO.GetAllRecords().Where(u => u.Id == Guid.Parse(IdBankAccount)).SingleOrDefault();
            int cStatus = bank.IdStatus;

            try
            {
                // Cambiamos los estados del comercio segun el estado actual
                switch (cStatus)
                {
                    case (int)1:
                        {
                            bank.IdStatus = (int)2;
                            break;
                        }
                    case (int)2:
                        {
                            bank.IdStatus = (int)1;
                            break;
                        }
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                //Devolvemos el error
                return new JsonResult()
                {
                    Data = new
                    {
                        Success = false,
                        Msg = "Modificación del estado fallida"
                    }
                };
            }
            // Guardamos los cambios en la base de datos
            CommerceInfoBankREPO.SaveChanges();
            //Devolvemos la respuesta de exito
            return new JsonResult()
            {
                Data = new
                {
                    Success = true,
                    Msg = "Modificacion del estado exitosa"
                }
            };

        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _IniciarProcesoPropuesta(string Usuario, string Clave, string IdCuenta, string rif)
        {
            var cuenta = CBLL.GetAllRecords().Where(u => u.Rif == rif).FirstOrDefault().CommerceBankAccounts.Where(e => e.Id == Guid.Parse(IdCuenta)).FirstOrDefault();
            List<AE_UsuarioBanco> usuarios = AE_UsuarioBancoREPO.GetAllRecords(u => u.AccountNumber == cuenta.AccountNumber && u.Password == Clave && u.Username == Usuario).ToList();
            string tick = DateTime.Now.Ticks.ToString();
            // proceso
            AE_ProcesoSolicitudApi _newproceso = new AE_ProcesoSolicitudApi();
            _newproceso.Fecha = DateTime.Now;
            _newproceso.Estatus = true;
            _newproceso.RifCommerce = rif;
            AE_ProcesoSolicitudApiREPO.AddEntity(_newproceso);
            AE_ProcesoSolicitudApiREPO.SaveChanges();

            if (usuarios.Count > 0)
            {
                DateTime fecha = DateTime.Now.AddDays(-1);
                for (int i = 0; i < 4; i++)
                {
                    InstaTransfer.DataAccess.AE_SolicitudApi solicitud = new AE_SolicitudApi();
                    solicitud.Activa = true;
                    solicitud.FechaInicio = fecha.AddDays(-30);
                    solicitud.FechaFin = fecha;
                    fecha = fecha.AddDays(-30);
                    solicitud.Id = rif + "_" + usuarios.FirstOrDefault().Id + "_" + DateTime.Now.ToString("ddMMyy") + "_" + i.ToString() + "_" + tick;
                    solicitud.RifCommerce = rif;
                    solicitud.Reprocesar = false;
                    solicitud.Procesado = false;
                    solicitud.Intentos = 0;
                    solicitud.Analisis = true;
                    solicitud.IdProceso = _newproceso.Id;
                    AE_SolicitudApiREPO.AddEntity(solicitud);
                }
                AE_SolicitudApiREPO.SaveChanges();
            }
            else
            {
                InstaTransfer.DataAccess.AE_UsuarioBanco item = new AE_UsuarioBanco();
                item.AccountNumber = cuenta.AccountNumber;
                item.Active = true;
                item.Password = Clave;
                item.Username = Usuario;
                item.RifCommerce = rif;
                AE_UsuarioBancoREPO.AddEntity(item);
                AE_UsuarioBancoREPO.SaveChanges();
                DateTime fecha = DateTime.Now;
                for (int i = 0; i < 4; i++)
                {
                    InstaTransfer.DataAccess.AE_SolicitudApi solicitud = new AE_SolicitudApi();
                    solicitud.Activa = true;
                    solicitud.FechaInicio = fecha.AddDays(-30);
                    solicitud.FechaFin = fecha;
                    fecha = fecha.AddDays(-30);
                    solicitud.Id = rif + "_" + item.Id + "_" + DateTime.Now.ToString("ddMMyy") + "_" + i.ToString() + "_" + tick;
                    solicitud.RifCommerce = rif;
                    solicitud.Reprocesar = false;
                    solicitud.Procesado = false;
                    solicitud.Intentos = 0;
                    solicitud.Analisis = true;
                    solicitud.IdProceso = _newproceso.Id;
                    AE_SolicitudApiREPO.AddEntity(solicitud);
                    AE_SolicitudApiREPO.SaveChanges();
                }
            }

            return Json(new
            {
                success = true,
                message = "Cuenta agregada de forma correcta!"
            }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _IniciarProcesoCobro(string _fecha, int idavance)
        {
            var avance = AE_AvanceREPO.GetAllRecords().Where(u => u.Id == idavance).FirstOrDefault();

            DateTime fecha = DateTime.Parse(_fecha);

            InstaTransfer.DataAccess.AE_SolicitudApi nueva = new AE_SolicitudApi();
            nueva.RifCommerce = avance.RifCommerce;
            nueva.FechaInicio = fecha;
            nueva.FechaFin = fecha;
            nueva.Procesado = false;
            nueva.Reprocesar = false;
            nueva.Intentos = 0;
            nueva.Id = avance.RifCommerce + "_" + _fecha.Replace('/', '-') + "_" + DateTime.Now.ToString("ddMMyyhhss") + "_CD_" + avance.Id;
            nueva.Activa = true;
            nueva.Analisis = false;
            AE_SolicitudApiREPO.AddEntity(nueva);
            AE_SolicitudApiREPO.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Solicitud creada de forma exitosa!"
            }, JsonRequestBehavior.DenyGet);
        }


        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _DeleteBankInfo(string id)
        {
            try
            {
                var item = CommerceInfoBankREPO.GetAllRecords().Where(u => u.Id == Guid.Parse(id)).FirstOrDefault();

                var avance = AE_AvanceREPO.GetAllRecords().Where(u => u.NumeroCuenta == item.AccountNumber).ToList();

                if (avance.Count > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No podemos eliminar esta cuenta"
                    }, JsonRequestBehavior.DenyGet);

                }
                else
                {

                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = e.Message
                }, JsonRequestBehavior.DenyGet);
            }
            return Json(new
            {
                success = true,
                message = "Cuenta agregada de forma correcta!"
            }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _AddBankInfo(string rif, string idbanco, string numerocuenta, string alias, string IsCashOut, string IsAvanceefectivo, string IsScraper)
        {
            try
            {
                var item = CommerceInfoBankREPO.GetAllRecords().Where(u => u.AccountNumber == numerocuenta.Trim()).ToList();
                if (item.Count > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "el número de cuenta ya fue registrado"
                    }, JsonRequestBehavior.DenyGet);


                }
                else
                {
                    InstaTransfer.DataAccess.CommerceBankAccount _item = new CommerceBankAccount();
                    _item.AccountNumber = numerocuenta.Trim();
                    _item.RifCommerce = rif;
                    _item.Alias = alias;
                    _item.IdStatus = 1;
                    _item.IdType = 1;
                    _item.Id = Guid.NewGuid();
                    if (IsAvanceefectivo != null)
                        _item.IsAvanceEfectivo = bool.Parse(IsAvanceefectivo);
                    else
                        _item.IsAvanceEfectivo = false;
                    if (IsCashOut != null)
                        _item.IsCashOut = bool.Parse(IsCashOut);
                    else
                        _item.IsCashOut = false;
                    if (IsScraper != null)
                        _item.IsScraper = bool.Parse(IsScraper);
                    else
                        _item.IsScraper = false;
                    _item.IdUBank = idbanco;

                    CommerceInfoBankREPO.AddEntity(_item);
                    CommerceInfoBankREPO.SaveChanges();

                }

            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = e.Message
                }, JsonRequestBehavior.DenyGet);
            }
            return Json(new
            {
                success = true,
                message = "Cuenta agregada de forma correcta!"
            }, JsonRequestBehavior.DenyGet);

        }
        public ActionResult _InfoBank(string rif)
        {
            var obejto = CBLL.GetAllRecords().Where(u => u.Rif == rif).FirstOrDefault().CommerceBankAccounts.ToList();
            ViewBag.rif = rif;
            return View(obejto);
        }

        public ActionResult TabCashAdvance(string rif)
        {
            var _cUsers = GetCUsers(rif).FirstOrDefault();
            List<InstaTransfer.DataAccess.AE_MovimientosCuenta> Movimientos = _cUsers.Commerce.AE_MovimientosCuentas.Where(u => u.Activo).ToList();
            Movimientos = Movimientos.OrderByDescending(u => u.Fecha).ToList();
            ViewBag.Movimientos = Movimientos;
            List<AE_Propuesta> propuesta = AE_PropuestaREPO.GetAllRecords(u => u.RifCommerce == rif && u.Estatus).ToList();
            ViewBag.Cuentas = _cUsers.Commerce.CommerceBankAccounts.ToList();

            AE_ProcesoSolicitudApi proceso = AE_ProcesoSolicitudApiREPO.GetAllRecords().Where(u => u.RifCommerce == rif && u.Estatus).ToList().LastOrDefault();
            if (proceso != null && proceso.Id > 0)
            {
                ViewBag.Proceso = true;
                ViewBag.Registros = AE_SolicitudApiREPO.GetAllRecords().Where(u => u.IdProceso == proceso.Id).ToList();
            }
            else
            {
                ViewBag.Proceso = false;
            }
            if (propuesta.ToList().Count > 0)
            {
                ViewBag.Propuesta = propuesta.FirstOrDefault().Estatus;
                ViewBag.Idpropuesta = propuesta.FirstOrDefault().Id;
            }
            else
            {
                ViewBag.Propuesta = false;
            }
            List<AE_Avance> avance = AE_AvanceREPO.GetAllRecords(u => u.RifCommerce == rif && u.IdEstatus == 1).OrderByDescending(u => u.FechaCreacion).ToList();
            if (avance.ToList().Count > 0)
            {
                ViewBag.Avance = true;
                ViewBag.Idavance = avance.FirstOrDefault().Id;
            }
            else
            {
                ViewBag.Avance = false;
            }
            return PartialView("_TabCashAdvance", _cUsers);
        }

        public ActionResult TabCashAdvanceHistory(string rif)
        {
            List<AE_Avance> ListAvance = new List<AE_Avance>();
            ListAvance = AE_AvanceREPO.GetAllRecords().Where(u => u.RifCommerce == rif && (u.IdEstatus == 1 || u.IdEstatus == 2)).ToList();

            return PartialView("_TabCashAdvanceHistory", ListAvance);
        }

        public ActionResult TabDocumentos(string rif)
        {
            List<AE_ArchivoUpload> Archivos = AE_ArchivoUploadREPO.GetAllRecords().Where(u => u.RifCommerce == rif).ToList();

            return PartialView("_TabDocumentos", Archivos);
        }

        public ActionResult _Progreso(string rif)
        {
            int avance = 0;
            List<string> lista = new List<string>();
            AE_ProcesoSolicitudApi proceso = AE_ProcesoSolicitudApiREPO.GetAllRecords().Where(u => u.RifCommerce == rif && u.Estatus).ToList().LastOrDefault();
            if (proceso != null && proceso.Id > 0)
            {
                //    ViewBag.Proceso = true;

                List<AE_SolicitudApi> registros = AE_SolicitudApiREPO.GetAllRecords().Where(u => u.IdProceso == proceso.Id).ToList();
                ViewBag.Registros = registros;
                foreach (var item in registros)
                {
                    bool win = System.IO.File.Exists(System.Configuration.ConfigurationManager.AppSettings["RutaAnalisisProcesado"].ToString() + item.Id + ".xls");
                    if (win)
                    {
                        lista.Add(item.Id);
                        avance = avance + 5;
                    }
                }
            }

            ViewBag.AvanceArchivos = avance;
            ViewBag.ListaArchivosListos = lista;
            //else
            //{
            //    ViewBag.Proceso = false;
            //}
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _EliminarMovimientos(string _rif)
        {
            List<InstaTransfer.DataAccess.AE_MovimientosCuenta> Movimientos = AEmovimientosREPO.GetAllRecords().Where(u => u.RifCommerce == _rif && u.Activo).ToList();
            AEmovimientosREPO.DeleteGroup(u => u.RifCommerce == _rif && u.Activo);
            AEmovimientosREPO.SaveChanges();

            List<InstaTransfer.DataAccess.AE_ProcesoSolicitudApi> procesos = AE_ProcesoSolicitudApiREPO.GetAllRecords().Where(u => u.RifCommerce == _rif && u.Estatus).ToList();
            if (procesos.Count > 0)
            {
                foreach (var item in procesos)
                {
                    item.Estatus = false;
                }
                AE_ProcesoSolicitudApiREPO.SaveChanges();
            }

            //bool _mostar = Movimientos.Sum(u => u.Monto) > 0 ? true : false;
            return Json(new
            {
                success = true,
                //message = z + " - Archivo(s) procesado(s) de forma correcta.",
                mostrar = false
            }, JsonRequestBehavior.DenyGet);
        }

        public JsonResult _GetRegistros(string rif)
        {

            List<InstaTransfer.DataAccess.AE_MovimientosCuenta> Movimientos = AEmovimientosREPO.GetAllRecords().Where(u => u.RifCommerce == rif && u.Activo).ToList();
            Movimientos = Movimientos.OrderByDescending(u => u.Fecha).ToList();
            bool _mostar = Movimientos.Sum(u => u.Monto) > 0 ? true : false;
            if (Movimientos.Count > 1)
            {
                return Json(new
                {
                    success = true,
                    cantidad = Movimientos.Count(),
                    desde = Movimientos.LastOrDefault().Fecha.ToString("dd/MM/yyy"),
                    hasta = Movimientos.FirstOrDefault().Fecha.ToString("dd/MM/yyy"),
                    validohasta = Movimientos.FirstOrDefault().Fecha.AddDays(7).ToString("dd/MM/yyy"),
                    mostrar = _mostar
                }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new
                {
                    success = true,
                    cantidad = 0,
                    desde = 0,
                    hasta = 0,
                    validohasta = "-",
                    mostrar = _mostar
                }, JsonRequestBehavior.DenyGet);

            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult UploadEstadoCuenta(List<HttpPostedFileBase> files, string _rif)
        {
            int z = 0;
            foreach (var file in files)
            {
                string _filename = DateTime.Now.ToString("hh_mm_ss") + file.FileName;
                List<string> _texto = new List<string>();
                List<InstaTransfer.DataAccess.AE_MovimientosCuenta> ListaMovimientos = new List<AE_MovimientosCuenta>();
                bool win = SaveFile(file, _filename);

                if (win)
                {
                    try
                    {
                        string ruta = System.Configuration.ConfigurationManager.AppSettings["RutaPropuestaMontando"];
                        //Create COM Objects. Create a COM object for everything that is referenced
                        Application xlApp = new Application();
                        Workbook xlWorkbook = xlApp.Workbooks.Open(ruta + _filename);
                        _Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                        Range xlRange = xlWorksheet.UsedRange;

                        int rowCount = xlRange.Rows.Count;
                        int colCount = 4;

                        //iterate over the rows and columns and print to the console as it appears in the file
                        //excel is not zero based!!
                        for (int i = 1; i <= rowCount; i++)
                        {
                            InstaTransfer.DataAccess.AE_MovimientosCuenta _Movimiento = new AE_MovimientosCuenta();

                            bool error = false;
                            if (i > 0)
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
                                            item = item + xlRange.Cells[i, j].Value2.ToString() + "~";
                                        }

                                    }
                                    catch
                                    {
                                        error = true;

                                    }

                                }
                                try
                                {
                                    var _arreglo = item.Split('~');
                                    decimal.Parse(_arreglo[3].ToString());
                                }
                                catch { error = true; }
                                if (!error)
                                {
                                    var arreglo = item.Split('~');
                                    if (ValidateRow(arreglo[2]))
                                    {
                                        if (BuscarRegistro(arreglo[2].ToString(), arreglo[1].ToString(), arreglo[0], _rif))
                                        {
                                            double d = double.Parse(arreglo[0]);
                                            DateTime conv = DateTime.FromOADate(d);
                                            _Movimiento.Fecha = conv;
                                            _Movimiento.Referencia = arreglo[1].ToString();
                                            _Movimiento.Descripcion = arreglo[2].ToString();
                                            _Movimiento.Monto = decimal.Parse(arreglo[3].ToString());
                                            _Movimiento.RifCommerce = _rif;
                                            _Movimiento.FechaRegistro = DateTime.Now;
                                            _Movimiento.Activo = true;
                                            AEmovimientosREPO.AddEntity(_Movimiento);
                                            AEmovimientosREPO.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        //if (BuscarRegistro(arreglo[2].ToString(), arreglo[1].ToString(), arreglo[0], _rif))
                                        //{
                                        double d = double.Parse(arreglo[0]);
                                        DateTime conv = DateTime.FromOADate(d);
                                        _Movimiento.Fecha = conv;
                                        _Movimiento.Referencia = arreglo[1].ToString();
                                        _Movimiento.Descripcion = arreglo[2].ToString();
                                        _Movimiento.Monto = 0;
                                        _Movimiento.RifCommerce = _rif;
                                        _Movimiento.FechaRegistro = DateTime.Now;
                                        _Movimiento.Activo = true;
                                        AEmovimientosREPO.AddEntity(_Movimiento);
                                        AEmovimientosREPO.SaveChanges();
                                        //}
                                    }


                                }
                            }
                        }
                        xlApp.Workbooks.Close();
                        xlApp.Quit();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                        //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp.Workbooks);
                        //xlApp = null;
                        //GC.Collect();

                    }
                    catch (Exception e)
                    {
                        throw e;
                        //return Json(new
                        //{
                        //    success = false,
                        //    message = e.Message
                        //}, JsonRequestBehavior.DenyGet);
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "No logramos procesar el archivo."
                    }, JsonRequestBehavior.DenyGet);
                }
                z++;
            }

            List<InstaTransfer.DataAccess.AE_MovimientosCuenta> Movimientos = AEmovimientosREPO.GetAllRecords().Where(u => u.RifCommerce == _rif && u.Activo).ToList();
            Movimientos = Movimientos.OrderByDescending(u => u.Fecha).ToList();
            bool _mostar = Movimientos.Sum(u => u.Monto) > 0 ? true : false;

            return Json(new
            {
                success = true,
                message = z + " - Archivo(s) procesado(s) de forma correcta.",
                cantidad = Movimientos.Count(),
                desde = Movimientos.LastOrDefault().Fecha.ToString("dd/MM/yyy"),
                hasta = Movimientos.FirstOrDefault().Fecha.ToString("dd/MM/yyy"),
                validohasta = Movimientos.FirstOrDefault().Fecha.AddDays(7).ToString("dd/MM/yyy"),
                mostrar = _mostar
            }, JsonRequestBehavior.DenyGet);
            //return Json(new
            //{
            //    success = true,
            //    message = "Procesamos" + z + "Archivos de fprma correcta."
            //}, JsonRequestBehavior.DenyGet);

        }



        //public JsonResult CobroDiario(string _rif)
        //{
        //    AE_Avance _avance = AE_AvanceREPO.GetAllRecords(u => u.RifCommerce == _rif).FirstOrDefault();
        //    try
        //    {
        //        List<InstaTransfer.DataAccess.AE_MovimientosDebito> Lista = new List<AE_MovimientosDebito>();
        //        //Create COM Objects. Create a COM object for everything that is referenced
        //        Application xlApp = new Application();
        //        Workbook xlWorkbook = xlApp.Workbooks.Open(@"C:\Users\carmelo\Documents\varios\ajax.xls");
        //        _Worksheet xlWorksheet = xlWorkbook.Sheets[1];
        //        Range xlRange = xlWorksheet.UsedRange;

        //        int rowCount = xlRange.Rows.Count;
        //        int colCount = 4;

        //        //iterate over the rows and columns and print to the console as it appears in the file
        //        //excel is not zero based!!
        //        for (int i = 1; i <= rowCount; i++)
        //        {
        //            InstaTransfer.DataAccess.AE_MovimientosDebito _Movimiento = new AE_MovimientosDebito();

        //            bool error = false;
        //            if (i > 1)
        //            {
        //                string item = "";
        //                for (int j = 1; j <= colCount; j++)
        //                {

        //                    //new line
        //                    if (j == 1)
        //                        Console.Write("\r\n");
        //                    try
        //                    {
        //                        //write the value to the console
        //                        if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
        //                        {
        //                            item = item + xlRange.Cells[i, j].Value2.ToString() + "~";
        //                        }
        //                    }
        //                    catch
        //                    {
        //                        error = true;

        //                    }

        //                }
        //                if (!error)
        //                {
        //                    var arreglo = item.Split('~');
        //                    if (ValidateRow(arreglo[2]))
        //                    {
        //                        //if (BuscarRegistro(arreglo[2].ToString(), arreglo[1].ToString()))
        //                        //{
        //                        double d = double.Parse(arreglo[0]);
        //                        DateTime conv = DateTime.FromOADate(d);
        //                        _Movimiento.Fecha = conv;
        //                        _Movimiento.Referencia = arreglo[1].ToString();
        //                        _Movimiento.Descripcion = arreglo[2].ToString();
        //                        string lote = "";
        //                        try
        //                        {
        //                            lote = getBetween(arreglo[2], "L.", " ");
        //                            if (lote == "")
        //                                lote = arreglo[2].Split(' ')[2];
        //                        }
        //                        catch
        //                        {

        //                            lote = "001";
        //                        }
        //                        _Movimiento.Lote = int.Parse(lote);
        //                        _Movimiento.Monto = decimal.Parse(arreglo[3].ToString());
        //                        //_Movimiento.RifCommerce = _rif;
        //                        _Movimiento.FechaRegistro = DateTime.Now;
        //                        _Movimiento.Activo = true;
        //                        AE_MovimientosDebitoREPO.AddEntity(_Movimiento);
        //                        Lista.Add(_Movimiento);
        //                        //}
        //                    }

        //                }
        //            }
        //        }

        //        //GENERAMOS ESTADOS CUESTA
        //        AE_EstadoCuenta _ultimo = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.IdAvance == _avance.Id).OrderBy(u => u.FechaRegistro).FirstOrDefault();
        //        decimal saldofinal = _ultimo.SaldoFinal;
        //        List<int> distinctLote = Lista.Select(p => p.Lote).Distinct().ToList();
        //        foreach (var item in distinctLote)
        //        {
        //            List<InstaTransfer.DataAccess.AE_MovimientosDebito> _newlistbylote = Lista.Where(u => u.Lote == item).ToList();
        //            decimal monto = _newlistbylote.Sum(u => u.Monto);
        //            decimal debocobra = (monto * _avance.Porcentaje) / 100;
        //            AE_EstadoCuenta estadocuenta = new AE_EstadoCuenta();

        //            estadocuenta.Abono = false;
        //            estadocuenta.Estatus = 1;
        //            estadocuenta.FechaOperacion = _newlistbylote.FirstOrDefault().Fecha;
        //            estadocuenta.FechaRegistro = DateTime.Now;
        //            estadocuenta.IdAvance = _avance.Id;
        //            estadocuenta.Lote = item;
        //            estadocuenta.MontoBase = monto;
        //            if (debocobra > _avance.MaximoCobro)
        //            {
        //                estadocuenta.Monto = _avance.MaximoCobro;
        //                estadocuenta.SaldoFinal = saldofinal - _avance.MaximoCobro;
        //                estadocuenta.SaldoInicial = saldofinal;
        //                saldofinal = saldofinal - _avance.MaximoCobro;
        //            }
        //            else
        //            {
        //                estadocuenta.Monto = debocobra;
        //                estadocuenta.SaldoFinal = saldofinal - debocobra;
        //                estadocuenta.SaldoInicial = saldofinal;
        //                saldofinal = saldofinal - debocobra;
        //            }

        //            AE_EstadoCuentaREPO.AddEntity(estadocuenta);
        //            AE_EstadoCuentaREPO.SaveChanges();
        //            //ASOCIAMOS ESTACUENTA A MOVIMIENTOS DEBITO
        //            foreach (var imte2 in Lista.Where(u => u.Lote == item))
        //            {
        //                imte2.IdAE_EstadoCuenta = estadocuenta.Id;
        //            }

        //        }

        //        AE_MovimientosDebitoREPO.SaveChanges();
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = e.Message
        //        }, JsonRequestBehavior.DenyGet);
        //    }
        //    return Json(new
        //    {
        //        success = true,
        //        message = "Archivo procesado de forma correcta."
        //    }, JsonRequestBehavior.DenyGet);

        //}

        public static string getBetween(string strSource, string strStart, string strEnd)
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
            String referecnias = System.Configuration.ConfigurationManager.AppSettings["Referencia"];
            var lista = referecnias.Split(',');
            foreach (var item in lista)
            {
                if (Descripcion.Contains(item))
                {

                    return true;
                }
            }
            return false;

        }

        public bool BuscarRegistro(string Descripcion, string Referencia, string fecha, string _rif)
        {
            double d = double.Parse(fecha);
            DateTime _fecha = DateTime.FromOADate(d);
            //DateTime _fecha = DateTime.Parse(_Fecha);
            List<InstaTransfer.DataAccess.AE_MovimientosCuenta> _movimientos = AEmovimientosREPO.GetAllRecords().Where(u => u.RifCommerce == _rif && u.Activo && u.Referencia == Referencia && u.Descripcion == Descripcion && u.Fecha == _fecha).ToList();
            if (_movimientos.Count() > 0)
            {
                return false;
            }
            return true;

        }

        public bool BuscarRegistro2(string Descripcion, string Referencia, string fecha, string _rif)
        {
            //double d = double.Parse(fecha);
            DateTime _fecha = DateTime.Parse(fecha);
            //DateTime _fecha = DateTime.Parse(_Fecha);
            List<InstaTransfer.DataAccess.AE_MovimientosCuenta> _movimientos = AEmovimientosREPO.GetAllRecords().Where(u => u.RifCommerce == _rif && u.Referencia == Referencia && u.Descripcion == Descripcion && u.Fecha == _fecha).ToList();
            if (_movimientos.Count() > 0)
            {
                return false;
            }
            return true;

        }

        public ActionResult _GenerarPropuesta(string rif, bool _modo)
        {
            InstaTransfer.DataAccess.AE_Dolar Dolar = AE_DolarREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).ToList().FirstOrDefault();
            decimal tasa = Dolar.Tasa;
            List<InstaTransfer.DataAccess.AE_MovimientosCuenta> _ListaMovimientos = new List<InstaTransfer.DataAccess.AE_MovimientosCuenta>();

            _ListaMovimientos = AEmovimientosREPO.GetAllRecords().Where(u => u.RifCommerce == rif).ToList();
            _ListaMovimientos = _ListaMovimientos.OrderBy(u => u.Fecha).ToList();

            DateTime fechainicio = DateTime.Parse(_ListaMovimientos.First().Fecha.ToString());
            DateTime fechafin = DateTime.Parse(_ListaMovimientos.Last().Fecha.ToString());
            List<Montos> _ListaMontos = new List<Montos>();
            List<Montos> _ListaMontosDefinitivo = new List<Montos>();
            decimal _factor = Math.Round(_ListaMovimientos.First().Commerce.Trust, 2);
            fechafin = fechafin.AddDays(1);
            while (fechainicio < fechafin)
            {
                if (fechainicio.DayOfWeek == DayOfWeek.Saturday || fechainicio.DayOfWeek == DayOfWeek.Sunday)
                {

                }
                else
                {
                    decimal monto = _ListaMovimientos.Where(u => u.Fecha.ToShortDateString() == fechainicio.ToShortDateString()).Sum(u => u.Monto);
                    Montos _item = new Montos();
                    _item.Fecha = fechainicio;
                    _item.Monto = monto;
                    _ListaMontosDefinitivo.Add(_item);
                }
                fechainicio = fechainicio.AddDays(1);
            }
            //VARIABLES GENERALES
            _ListaMontos = _ListaMontosDefinitivo.OrderByDescending(u => u.Fecha).ToList();
            bool evidencia = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["ActivarEvidencia"].ToString());
            if (evidencia)
            {
                string ruta = System.Configuration.ConfigurationManager.AppSettings["RutaPropuestaEvidencia"].ToString() + "Movimientos_" + DateTime.Now.ToString("ddMMyyhhss") + ".txt";
                List<string> lista = new List<string>();
                foreach (var item in _ListaMontos.OrderBy(u => u.Fecha).ToList())
                {
                    lista.Add(item.Monto.ToString());
                }
                System.IO.File.WriteAllLines(ruta, lista.ToArray());
            }
            //CALCULAMOS DIA DE EXITO
            int diasexito = _ListaMontos.Where(u => u.Monto > 0).Count();
            int diasnoexito = _ListaMontos.Where(u => u.Monto == 0).Count();
            //dividimos ESTO ES PARA = INGRESO DIARIO PROMEDIO
            decimal porcetajeexito = decimal.Parse(diasexito.ToString()) / decimal.Parse(_ListaMontos.Count().ToString());
            decimal _porcetajeexitocrudo = decimal.Parse(diasexito.ToString()) / decimal.Parse(_ListaMontos.Count().ToString());
            int _porcetajeexito = int.Parse(Math.Round(porcetajeexito * 100).ToString());

            decimal _variableA = decimal.Parse("0,2");
            decimal _variableB = decimal.Parse("0,8");
            decimal _variableCobranza = decimal.Parse("0,15");
            decimal _variableN = decimal.Parse("3,00");
            decimal _prima = decimal.Parse("1,20");
            decimal _prima1 = decimal.Parse("1,12");
            decimal _prima2 = decimal.Parse("1,10");
            decimal _prima3 = decimal.Parse("1,10");
            int plazo = 30;

            //GENERALES
            int totalregistro = _ListaMontos.Count();
            double countT1 = totalregistro * 0.35;
            int _countT1 = int.Parse(Math.Ceiling(countT1).ToString());
            //CALCULAMOS PROMEDIO
            //var bola = _ListaMontos.Take(_countT1).Sum(u => u.Monto);
            decimal _pomediot1 = Math.Round((_ListaMontos.Take(_countT1).Sum(u => u.Monto) / _countT1), 2);
            decimal _pomediot2 = Math.Round((_ListaMontos.Sum(u => u.Monto) / (totalregistro)), 2);
            decimal _promedio = Math.Round(((_variableA * _pomediot1) + (_variableB * _pomediot2)), 2);

            // CALCULAMOS T1 33 %
            var _Lista = _ListaMontos.Take(_countT1).Select(y => y.Monto).ToArray();
            decimal T1 = GetPromedio(_Lista);
            //CALCULO T2 100 %
            var _ListaT2 = _ListaMontos.Select(y => y.Monto).ToArray();
            decimal T2 = GetPromedio(_ListaT2);

            //CALCULAMOS LA MEDIANA AJUSTADA
            decimal media = (_variableA * T1) + (_variableB * T2);
            int totalregistrosin0 = _ListaMontos.Where(u => u.Monto > 0).Count();
            double countT1sin0 = totalregistrosin0 * 0.35;
            int _countT1sin0 = int.Parse(Math.Ceiling(countT1sin0).ToString());

            //var _ListaNueva = _ListaMontos.Where(u => u.Monto > 0).Take(_countT1sin0).Select(y => y.Monto).ToArray();
            var _ListaNueva = _ListaMontos.Take(_countT1).Select(y => y.Monto).Where(u => u > 0).ToArray();
            var _ListaNueva2 = _ListaMontos.Where(u => u.Monto > 0).Select(y => y.Monto).ToArray();
            decimal mediananueva = (_variableA * GetPromedio(_ListaNueva)) + (_variableB * GetPromedio(_ListaNueva2));
            //double _media = double.Parse(media.ToString());

            //CALCULANDO DESVIACION ESTANDAR T1
            var desvT1 = Math.Round(StandardDeviation(_ListaMontos.Take(_countT1).Select(y => y.Monto).ToList()), 2);
            //CALCULANDO DESVIACION ESTANDAR T2
            var desvT2 = Math.Round(StandardDeviation(_ListaMontos.Select(y => y.Monto).ToList()), 2);
            //CALCULANDO DESVIACION AJUSTADA
            decimal desvAJU = Math.Round(((_variableA * decimal.Parse(desvT1.ToString())) + (_variableB * decimal.Parse(desvT2.ToString()))), 2);
            //agrgeamos 
            decimal ingresopromedio = Math.Round(mediananueva, 2) * _porcetajeexitocrudo;
            decimal factorseguridad = ingresopromedio * (_factor / 100);
            ingresopromedio = ingresopromedio - factorseguridad;
            decimal cobranzapromdedio = ingresopromedio * _variableCobranza;
            //CALCULAMOS MAXIMO DEBITO

            //CALCULO AVANCE EFECTIVO 1
            //decimal _stepavance1 = mediananueva * _variableCobranza;
            //double helpavance1 = double.Parse(_stepavance1.ToString());
            //helpavance1 = Math.Round(helpavance1 / 1000d, 0) * 1000;
            //decimal _stepavance1 = mediananueva * _variableCobranza;
            decimal avance1 = decimal.Parse(cobranzapromdedio.ToString()) * plazo;
            avance1 = decimal.Parse((Math.Round(double.Parse(avance1.ToString()) / 10000d, 0) * 10000).ToString());
            avance1 = avance1 / _prima;
            //avance1 = decimal.Parse((Math.Round(double.Parse(avance1.ToString()) / 10000d, 0) * 10000).ToString());
            decimal reembolso1 = avance1 * _prima1;

            //decimal mcd = Math.Round((avance1 / 55), 2);
            //decimal mcd = Math.Round((cobranzapromdedio * plazo), 2);
            decimal mcd = Math.Round(((_promedio + (_variableN * desvAJU)) * _variableCobranza), 2);
            //decimal mcd = Math.Round((cobranzapromedio * plazo), 2);
            mcd = decimal.Parse((Math.Round(double.Parse(mcd.ToString()) / 1000d, 0) * 1000).ToString());
            //mcd = decimal.Parse((Math.Round(double.Parse(mcd.ToString()) / 1000d, 0) * 1000).ToString());
            //mcd = decimal.Parse((Math.Round(double.Parse(mcd.ToString()) / 1000d, 0) * 1000).ToString());
            mcd = RetornaRedondeadoUP(mcd);


            //CALCULO AVANCE EFECTIVO 2
            decimal avance2 = decimal.Parse("1,50") * avance1;
            avance2 = Math.Round(avance2, 2);
            //avance2 = decimal.Parse((Math.Round(double.Parse(avance2.ToString()) / 10000d, 0) * 10000).ToString());
            //decimal porcentajecobranza2 = (_prima * avance2) / (mediananueva * plazo);
            //porcentajecobranza2 = porcentajecobranza2 * 100;
            //porcentajecobranza2 = Math.Floor(porcentajecobranza2);
            //porcentajecobranza2 = porcentajecobranza2 / 100;
            decimal reembolso2 = avance2 * _prima2;
            decimal porcentajecobranza2 = decimal.Parse("0,22");
            decimal mcd2 = Math.Round(((_promedio + (_variableN * desvAJU)) * porcentajecobranza2), 2);
            //decimal mcd2 = Math.Round((avance2/55), 2);
            //mcd2 = decimal.Parse((Math.Round(double.Parse(mcd2.ToString()) / 1000d, 0) * 1000).ToString());
            mcd2 = RetornaRedondeadoUP(mcd2);


            //CALCULO AVANCE EFECTIVO 3
            decimal avance3 = decimal.Parse("2,00") * avance1;
            //avance3 = decimal.Parse((Math.Round(double.Parse(avance3.ToString()) / 10000d, 0) * 10000).ToString());
            avance3 = Math.Round(avance3, 2);
            //double _avance3 = double.Parse(avance3.ToString());
            //_avance3 = Math.Round(_avance3 / 1000d, 2) * 1000;
            //avance3 = decimal.Parse(_avance3.ToString());
            //decimal porcentajecobranza3 = (_prima * avance3) / (mediananueva * plazo);
            decimal porcentajecobranza3 = decimal.Parse("0,30");
            //porcentajecobranza3 = Math.Floor(porcentajecobranza3);
            porcentajecobranza3 = porcentajecobranza3 * 100;
            porcentajecobranza3 = Math.Floor(porcentajecobranza3);
            porcentajecobranza3 = porcentajecobranza3 / 100;

            decimal reembolso3 = avance3 * _prima3;
            decimal mcd3 = Math.Round(((_promedio + (_variableN * desvAJU)) * porcentajecobranza3), 2);
            //decimal mcd3 = Math.Round((avance3 / 55), 2);
            //mcd3 = decimal.Parse((Math.Round(double.Parse(mcd3.ToString()) / 1000d, 0) * 1000).ToString());
            mcd3 = RetornaRedondeadoUP(mcd3);
            //GUARDO EN BD

            AE_Propuesta _Propuesta = new AE_Propuesta();
            //avance
            _Propuesta.AvanceOpcion1 = Math.Round(avance1 / tasa, 2);
            _Propuesta.AvanceOpcion2 = Math.Round(avance2 / tasa, 2);
            _Propuesta.AvanceOpcion3 = Math.Round(avance3 / tasa, 2);
            _Propuesta.AvanceOpcion1Bs = Math.Round(avance1, 2);
            _Propuesta.AvanceOpcion2Bs = Math.Round(avance2, 2);
            _Propuesta.AvanceOpcion3Bs = Math.Round(avance3, 2);
            //porcentaje
            _Propuesta.PorcentajeOpcion1 = 15;
            _Propuesta.PorcentajeOpcion2 = int.Parse(Math.Round(porcentajecobranza2 * 100, 0).ToString());
            _Propuesta.PorcentajeOpcion3 = int.Parse(Math.Round(porcentajecobranza3 * 100, 0).ToString());
            //reembolso
            _Propuesta.ReembolsoOpcion1 = Math.Round(reembolso1 / tasa, 2);
            _Propuesta.ReembolsoOpcion2 = Math.Round(reembolso2 / tasa, 2);
            _Propuesta.ReembolsoOpcion3 = Math.Round(reembolso3 / tasa, 2);
            _Propuesta.ReembolsoOpcion1Bs = Math.Round(reembolso1, 2);
            _Propuesta.ReembolsoOpcion2Bs = Math.Round(reembolso2, 2);
            _Propuesta.ReembolsoOpcion3Bs = Math.Round(reembolso3, 2);
            //maximo cobro
            _Propuesta.MaximoCobroOpcion1 = Math.Round(mcd / tasa, 2);
            _Propuesta.MaximoCobroOpcion2 = Math.Round(mcd2 / tasa, 2);
            _Propuesta.MaximoCobroOpcion3 = Math.Round(mcd3 / tasa, 2);
            _Propuesta.MaximoCobroOpcion1Bs = Math.Round(mcd, 2);
            _Propuesta.MaximoCobroOpcion2Bs = Math.Round(mcd2, 2);
            _Propuesta.MaximoCobroOpcion3Bs = Math.Round(mcd3, 2);
            _Propuesta.Modalidad = _modo;
            //datos adicionales
            _Propuesta.RifCommerce = rif;
            _Propuesta.FechaCreacion = DateTime.Now;
            _Propuesta.Estatus = true;
            _Propuesta.Tasa = tasa;
            AE_PropuestaREPO.AddEntity(_Propuesta);
            AE_PropuestaREPO.SaveChanges();


            ViewBag.avance1 = Math.Round(avance1 / tasa, 2);
            ViewBag.porcentaje1 = "15%";
            ViewBag.reembolso1 = Math.Round(reembolso1 / tasa, 2);
            ViewBag.avance2 = Math.Round(avance2 / tasa, 2);
            ViewBag.porcentaje2 = Math.Round(porcentajecobranza2 * 100, 0);
            ViewBag.reembolso2 = Math.Round(reembolso2 / tasa, 2);
            ViewBag.Mcd2 = (mcd2 / tasa);
            ViewBag.avance3 = Math.Round(avance3 / tasa, 2);
            ViewBag.porcentaje3 = Math.Round(porcentajecobranza3 * 100, 0);
            ViewBag.reembolso3 = Math.Round(reembolso3 / tasa, 2);
            ViewBag.Mcd3 = (mcd3 / tasa);
            ViewBag.T1 = Math.Round(T1, 2);
            ViewBag.T2 = Math.Round(T2, 2);
            ViewBag.Media = Math.Round(mediananueva, 2);
            ViewBag.Promedio = Math.Round(_promedio, 2);
            ViewBag.T1d = _countT1;
            ViewBag.T2d = totalregistro;
            ViewBag.DesvT1 = desvT1;
            ViewBag.DesvT2 = desvT2;
            ViewBag.DesvAJU = desvAJU;
            ViewBag.Mcd = (mcd / tasa);
            ViewBag.Avance = false;
            ViewBag.Cuentas = _Propuesta.Commerce.CommerceBankAccounts.ToList();
            ViewBag.Usuarios = _Propuesta.Commerce.AE_UsuarioBancos.ToList();
            //ViewBag.Modo = _modo;
            return RedirectToAction("_Propuesta", new { id = _Propuesta.Id, modo = _modo });

        }

        public ActionResult _Propuesta(int id)
        {

            AE_Propuesta _Propuesta = AE_PropuestaREPO.GetAllRecords().Where(u => u.Id == id).FirstOrDefault();
            ViewBag.Modo = _Propuesta.Modalidad;
            ViewBag.Cuentas = _Propuesta.Commerce.CommerceBankAccounts.ToList();
            ViewBag.Usuarios = _Propuesta.Commerce.AE_UsuarioBancos.ToList();
            List<AE_Avance> avance = AE_AvanceREPO.GetAllRecords(u => u.RifCommerce == _Propuesta.RifCommerce && u.IdEstatus == 1).OrderByDescending(u => u.FechaCreacion).ToList();
            if (avance.ToList().Count > 0)
            {
                ViewBag.Avance = true;
                ViewBag.Idavance = avance.FirstOrDefault().Id;
                List<AE_EstadoCuenta> estadocuenta = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.IdAvance == avance.First().Id && !u.Abono).ToList();
                decimal totalpagado = estadocuenta.Sum(u => u.Monto);
                decimal porcentaje = (totalpagado / avance.FirstOrDefault().Avance) * 100;
                int _por = int.Parse(Math.Round(porcentaje).ToString());
                ViewBag.Porcentaje = _por;
            }
            else
            {
                ViewBag.Avance = false;
            }

            return View("_Propuesta", _Propuesta);
        }

        public ActionAsPdf _printePropuesta(int _id)
        {
            return new ActionAsPdf("_Propuesta", new { id = _id })
            {
                FileName = "Propuesta" + _id + ".pdf",
                PageSize = Size.Letter,
                PageOrientation = Orientation.Portrait,
                PageMargins = { Left = 0, Right = 0, Top = 0, Bottom = 0 },
                PageWidth = 216,
                PageHeight = 279,
                CustomSwitches = "--disable-smart-shrinking"
            };
        }

        public ActionResult _Avance(int id)
        {

            AE_Avance _Avance = AE_AvanceREPO.GetAllRecords().Where(u => u.Id == id).FirstOrDefault();


            return View("_Avance", _Avance);
        }

        public JsonResult _GenerarAvance(int idpropuesta, int seleccion, string usuario, string clave, string idusuario, string idcuenta, string modo)
        {
            InstaTransfer.DataAccess.AE_Dolar Dolar = AE_DolarREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).ToList().FirstOrDefault();
            decimal tasa = Dolar.Tasa;
            AE_Propuesta _Propuesta = AE_PropuestaREPO.GetEntity(idpropuesta);
            AE_Avance avance = new AE_Avance();
            InstaTransfer.DataAccess.AE_UsuarioBanco _Usuario = new AE_UsuarioBanco();

            if (usuario == string.Empty || clave == string.Empty)
            {
                if (idusuario != string.Empty)
                {
                    _Usuario = AE_UsuarioBancoREPO.GetAllRecords().Where(u => u.Id == int.Parse(idusuario)).FirstOrDefault();
                }
                else
                {
                    var _result = new
                    {
                        success = false
                    };
                    return new JsonResult { Data = _result };
                }
            }
            else
            {
                InstaTransfer.DataAccess.CommerceBankAccount _bank = CommerceInfoBankREPO.GetAllRecords().Where(u => u.Id == Guid.Parse(idcuenta)).FirstOrDefault();
                _Usuario.Password = clave;
                _Usuario.RifCommerce = _Propuesta.RifCommerce;
                _Usuario.Username = usuario;
                _Usuario.AccountNumber = _bank.AccountNumber;
                AE_UsuarioBancoREPO.SaveChanges();
                //CommerceInfoBankREPO.SaveChanges();
            }

            avance.IdPropuesta = _Propuesta.Id;
            avance.FechaCreacion = DateTime.Now;
            avance.RifCommerce = _Propuesta.RifCommerce;
            switch (seleccion)
            {
                case 1:
                    avance.Reembolso = _Propuesta.ReembolsoOpcion1;
                    avance.ReembolsoBs = _Propuesta.ReembolsoOpcion1Bs;
                    avance.Avance = _Propuesta.AvanceOpcion1;
                    avance.AvanceBs = _Propuesta.AvanceOpcion1Bs;
                    avance.MaximoCobro = _Propuesta.MaximoCobroOpcion1;
                    avance.Porcentaje = _Propuesta.PorcentajeOpcion1;
                    break;
                case 2:
                    avance.Reembolso = _Propuesta.ReembolsoOpcion2;
                    avance.ReembolsoBs = _Propuesta.ReembolsoOpcion2Bs;
                    avance.Avance = _Propuesta.AvanceOpcion2;
                    avance.AvanceBs = _Propuesta.AvanceOpcion2Bs;
                    avance.MaximoCobro = _Propuesta.MaximoCobroOpcion2;
                    avance.Porcentaje = _Propuesta.PorcentajeOpcion2;
                    break;
                case 3:
                    avance.Reembolso = _Propuesta.ReembolsoOpcion3;
                    avance.ReembolsoBs = _Propuesta.ReembolsoOpcion3Bs;
                    avance.Avance = _Propuesta.AvanceOpcion3;
                    avance.AvanceBs = _Propuesta.AvanceOpcion3Bs;
                    avance.MaximoCobro = _Propuesta.MaximoCobroOpcion3;
                    avance.Porcentaje = _Propuesta.PorcentajeOpcion3;
                    break;
                default:
                    break;
            }
            avance.AprobadoNativa = false;
            avance.Modalidad = bool.Parse(modo);
            avance.PorcentajePrestamo = (avance.Reembolso - avance.Avance) * 100 / avance.Avance;
            avance.TasaUtilizada = tasa;
            if (_Propuesta.Commerce.Nativa)
            {
                avance.IdEstatus = 4;
            }
            else
            {
                avance.IdEstatus = 1;
            }
            avance.NumeroCuenta = _Usuario.AccountNumber;
            avance.Usuario = _Usuario.Username;
            avance.Clave = _Usuario.Password;

            AE_AvanceREPO.AddEntity(avance);
            AE_AvanceREPO.SaveChanges();
            //MOVIMIENTO
            AE_EstadoCuenta Estadocuenta = new AE_EstadoCuenta();
            Estadocuenta.IdAvance = avance.Id;
            Estadocuenta.Monto = avance.Reembolso;
            Estadocuenta.Abono = true;
            Estadocuenta.Estatus = 1;
            Estadocuenta.FechaOperacion = DateTime.Now;
            Estadocuenta.FechaRegistro = DateTime.Now;
            Estadocuenta.SaldoInicial = 0;
            Estadocuenta.SaldoFinal = avance.Reembolso;
            Estadocuenta.Efectivo = false;
            Estadocuenta.EfectivoCambiado = false;
            AE_EstadoCuentaREPO.AddEntity(Estadocuenta);
            AE_EstadoCuentaREPO.SaveChanges();

            _Propuesta.Estatus = false;
            AE_PropuestaREPO.SaveChanges();
            try
            {
                List<AE_ValorAccionTR> ListValorAccionTR = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.FechaCreacionRegistro).Take(5).ToList();
                if (ListValorAccionTR.Count > 0)
                {
                    AE_ValorAccionTR Ultimo = ListValorAccionTR.FirstOrDefault();
                    AE_ValorAccionTR ValorAccionTR = new AE_ValorAccionTR();
                    ValorAccionTR.FechaCreacionRegistro = DateTime.Now;
                    ValorAccionTR.FechaOperacion = DateTime.Now;
                    ValorAccionTR.FechaUltimaActualizacion = DateTime.Now;
                    ValorAccionTR.GastoReportado = 0;
                    ValorAccionTR.CapitalInicial = Ultimo.NuevoCapital;
                    ValorAccionTR.CapitalPorCobrar = Ultimo.CapitalPorCobrar + avance.Avance;
                    ValorAccionTR.AbonoCapital = 0;
                    ValorAccionTR.UtilidadReportada = 0;
                    ValorAccionTR.SaldoUSD = Ultimo.SaldoUSD - avance.Avance;
                    ValorAccionTR.Prestamo = avance.Avance;
                    ValorAccionTR.TotalCobroDiario = 0;
                    ValorAccionTR.UsdTransito = 0;
                    ValorAccionTR.UsdVenezuela = 0;
                    ValorAccionTR.CuentaVenezuela = 0;
                    ValorAccionTR.NuevoCapital = Ultimo.NuevoCapital;
                    ValorAccionTR.PagoCapitalInversionista = 0;
                    ValorAccionTR.PagoUtilidadAdministrador = 0;
                    ValorAccionTR.TotalAcciones = Ultimo.TotalAcciones;
                    ValorAccionTR.PagoUtilidadMesInversionista = 0;
                    ValorAccionTR.ValorAccion = Ultimo.ValorAccion;

                    AE_ValorAccionTRREPO.AddEntity(ValorAccionTR);
                    AE_ValorAccionTRREPO.SaveChanges();
                }
            }
            catch (Exception e) { }
            List<AE_MovimientosCuenta> Movimientos = AEmovimientosREPO.GetAllRecords().Where(u => u.RifCommerce == avance.RifCommerce).ToList();
            foreach (var item in Movimientos)
            {
                item.Activo = false;
            }
            AEmovimientosREPO.SaveChanges();

            List<AE_ProcesoSolicitudApi> Procesos = AE_ProcesoSolicitudApiREPO.GetAllRecords().Where(u => u.RifCommerce == avance.RifCommerce).ToList();
            if (Procesos.Count > 0)
            {
                foreach (var item in Procesos)
                {
                    item.Estatus = false;
                }
                AE_ProcesoSolicitudApiREPO.SaveChanges();
            }
            var result = new
            {
                id = avance.Id,
                success = true

            };

            return new JsonResult { Data = result };
        }

        public class Montos
        {

            public DateTime Fecha { get; set; }
            public decimal Monto { get; set; }
        }

        public decimal GetMedian(decimal[] sourceNumbers)
        {
            //Framework 2.0 version of this method. there is an easier way in F4        
            if (sourceNumbers == null || sourceNumbers.Length == 0)
                throw new System.Exception("Median of empty array not defined.");

            //make sure the list is sorted, but use a new array
            decimal[] sortedPNumbers = (decimal[])sourceNumbers.Clone();
            Array.Sort(sortedPNumbers);

            //get the median
            int size = sortedPNumbers.Length;
            int mid = size / 2;
            decimal median = (size % 2 != 0) ? (decimal)sortedPNumbers[mid] : ((decimal)sortedPNumbers[mid] + (decimal)sortedPNumbers[mid - 1]) / 2;
            return median;
        }

        public decimal GetPromedio(decimal[] sourceNumbers)
        {
            return sourceNumbers.Average();
        }

        public static double StandardDeviation(List<decimal> valueList)
        {
            double M = 0.0;
            double S = 0.0;
            int k = 1;
            foreach (double value in valueList)
            {
                double tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 2));
        }

        public decimal RetornaRedondeado(decimal valor)
        {
            if (valor >= 0 && valor <= 100)
            {
                valor = Math.Round(valor);
            }
            else if (valor > 100 && valor <= 10000)
            {
                valor = Math.Round(valor / 100) * 100;
            }
            else if (valor > 10000 && valor <= 100000)
            {
                valor = Math.Round(valor / 1000) * 1000;
            }
            else if (valor > 100000 && valor <= 999999)
            {
                valor = Math.Round(valor / 1000) * 1000;
            }
            else if (valor >= 1000000 && valor <= 9999999)
            {
                valor = Math.Round(valor / 10000) * 10000;
            }
            else if (valor >= 10000000 && valor <= 99999999)
            {
                valor = Math.Round(valor / 100000) * 100000;
            }
            else if (valor >= 100000000)
            {
                valor = Math.Round(valor / 1000000) * 1000000;
            }

            return valor;

        }

        public decimal RetornaRedondeadoUP(decimal valor)
        {
            if (valor >= 0 && valor <= 100)
            {
                valor = Math.Ceiling(valor);
            }
            else if (valor > 100 && valor <= 10000)
            {
                valor = Math.Ceiling(valor / 100) * 100;
            }
            else if (valor > 10000 && valor <= 100000)
            {
                valor = Math.Ceiling(valor / 1000) * 1000;
            }
            else if (valor > 100000 && valor <= 999999)
            {
                valor = Math.Ceiling(valor / 1000) * 1000;
            }
            else if (valor >= 1000000 && valor <= 9999999)
            {
                valor = Math.Ceiling(valor / 1000) * 1000;
            }
            else if (valor >= 10000000 && valor <= 99999999)
            {
                valor = Math.Ceiling(valor / 100000) * 100000;
            }
            else if (valor >= 100000000)
            {
                valor = Math.Ceiling(valor / 1000000) * 1000000;
            }

            return valor;

        }

        public JsonResult _EliminarPropuesta(string IdPropuesta, string _rif)
        {

            try
            {
                InstaTransfer.DataAccess.AE_Propuesta _Propuesta = AE_PropuestaREPO.GetAllRecords().Where(u => u.Id == int.Parse(IdPropuesta)).FirstOrDefault();
                AE_PropuestaREPO.DeleteEntity(_Propuesta.Id);
                AE_PropuestaREPO.SaveChanges();

                AEmovimientosREPO.DeleteGroup(u => u.RifCommerce == _rif && u.Activo);
                AEmovimientosREPO.SaveChanges();

                return Json(new
                {
                    Success = true,
                    Message = "Propuesta Eliminada exitosamente",
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

        public bool SaveFile(HttpPostedFileBase file, string _filename)
        {
            try
            {
                string savePath = System.Configuration.ConfigurationManager.AppSettings["RutaPropuestaMontando"].ToString();
                // Specify the path to save the uploaded file to.
                //string savePath = @"C:\Users\carmelo\Documents\montando\";

                // Get the name of the file to upload.
                string fileName = _filename;

                // Create the path and file name to check for duplicates.
                string pathToCheck = savePath + fileName;

                // Create a temporary file name to use for checking duplicates.
                string tempfileName = "";

                // Check to see if a file already exists with the
                // same name as the file to upload.        
                if (System.IO.File.Exists(pathToCheck))
                {
                    int counter = 2;
                    while (System.IO.File.Exists(pathToCheck))
                    {
                        // if a file with this name already exists,
                        // prefix the filename with a number.
                        tempfileName = counter.ToString() + fileName;
                        pathToCheck = savePath + tempfileName;
                        counter++;
                    }

                }
                else
                {
                    // Notify the user that the file was saved successfully.

                }

                // Append the name of the file to upload to the path.
                savePath += fileName;

                // Call the SaveAs method to save the uploaded
                // file to the specified directory.
                file.SaveAs(savePath);

                return true;
            }
            catch (Exception e) { return false; }

        }
        #endregion
    }
}
