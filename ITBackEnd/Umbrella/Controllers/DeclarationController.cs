using BLL.Concrete;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.BackEnd;
using InstaTransfer.ITLogic;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.ITResources.Enums;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Umbrella.App_Code;

namespace Umbrella.Controllers
{
    [Authorize(Roles = UserRoleConstant.TransaXAdmin + "," +
        UserRoleConstant.TransaXUser + "," +
        UserRoleConstant.CommerceAdmin + "," +
        UserRoleConstant.CommerceUser)]
    public class DeclarationController : Controller
    {
        DeclarationBLL DBLL = new DeclarationBLL();
        Repository<UDeclarationStatus> UDSRepo = new Repository<UDeclarationStatus>();
        URepository<UBankStatementEntry> UBSERepo = new URepository<UBankStatementEntry>();
        CPurchaseOrderBLL CPOBLL = new CPurchaseOrderBLL();
        CUserBLL CUBLL = new CUserBLL();

        BaseSuccessResponse _baseSuccessResponse;
        BaseErrorResponse _baseErrorResponse;

        // GET: Declaration
        [Authorize(Roles = UserRoleConstant.TransaXAdmin + "," +
            UserRoleConstant.TransaXUser)]
        [SessionExpireFilter]
        public ActionResult Index(bool? isApproved)
        {
            var statusList = new List<UDeclarationStatus>();

            #region Test

            //ApproveDeclaration(Guid.Parse("a1056dac-b45b-47ca-8bf5-b2e7eb7e20d8"), Guid.Parse("98F76C74-0C18-405A-80BB-5F9B71176284"));

            //GetPosibleEntries(Guid.Parse("1e8f4436-35a7-4be4-bff7-041493b093b4"));

            #endregion

            var declarations = GetDeclarations();
            if (isApproved != null)
            {
                ViewBag.isApproved = isApproved;
            }
            else
            {
                ViewBag.isApproved = null;
            }

            // Lista de estados para el filtro
            statusList = UDSRepo.GetAllRecords().ToList();
            ViewBag.statusList = statusList;


            return View(declarations);
        }
        public List<string[]> GetData()
        {
            List<UDeclaration> declarations = GetDeclarations();
            var result = from u in declarations
                         select new[] {
                           u.TransactionDate.ToString("yyyy/MM/dd", new System.Globalization.CultureInfo("es-VE")),
                           u.Reference.Trim(),
                           u.Amount.ToString("#,##0.00", new System.Globalization.CultureInfo("es-VE")),
                           u.IdCUser != null ?  CUBLL.GetAllRecords(cu => cu.Id == u.IdCUser).FirstOrDefault().AspNetUser.UserName.Trim() : string.Empty,
                           u.RifCommerce != null ? u.RifCommerce : string.Empty,
                           u.UDeclarationStatus.Description.Trim(),
                           u.Id.ToString().Trim(),
                           u.Commerce.BusinessName.Trim()
                        };
            List<string[]> resultList = result.ToList();

            return resultList;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Reconcile(Guid id, bool? back, Guid? backId)
        {
            var declaration = DBLL.GetEntity(id);

            ViewBag.posibleentries = GetPosibleEntries(id);
            ViewBag.backId = backId;

            return PartialView("_Reconcile", declaration);
        }
        //[HttpPost]
        //[ValidateInput(false)]
        //public ActionResult Details(object data, string id)
        //{
        //    ViewBag.data = data;
        //    ViewBag.commerceId = id;
        //    return PartialView("_Details");
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id">Id de la declaracion</param>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="back">Opcion para regresar</param>
        /// <param name="backId">Id de la orden</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SideDeclarations(Guid Id, string rif, bool? back, Guid? backId)
        {
            // Instanciamos la declaracion
            var declaration = new UDeclaration();
            // Verificamos que la ventana no permita regresar al registro anterior
            if (back.Equals(null) || back.Equals(false))
            {
                // Le indicamos a la vista que no pinte el boton de regreso
                ViewBag.back = false;
                // Verificamos que exista un registro anterior
                if (backId != null && backId.Value.ToString() != string.Empty)
                {
                    // Obtenemos la orden de compra y declaracion desde el id y rif del registro anterior
                    ViewBag.purchaseOrder = CPOBLL.GetPurchaseOrder(backId.Value, rif);
                    declaration = DBLL.GetDeclaration(rif, backId.Value);
                }
                else
                {
                    // Obtenemos la orden de compra y declaracion desde el id y rif del registro actual
                    ViewBag.purchaseOrder = CPOBLL.GetPurchaseOrder(Id, rif);
                    declaration = DBLL.GetDeclaration(rif, Id);
                }
            }
            else
            {
                // Le indicamos a la vista que muestre el boton de regreso y el id del registro anterior
                ViewBag.back = true;
                ViewBag.backId = backId;
                // Obtenemos la orden de compra y declaracion desde el id y rif del registro actual
                ViewBag.purchaseOrder = CPOBLL.GetPurchaseOrder(Id, rif);
                declaration = DBLL.GetDeclaration(rif, Id);
            }
            // Retornamos la vista parcial con la declaracion actual o anterior
            return PartialView("_SideDeclarations", declaration);
        }

        /// <summary>
        /// Anula una declaración específica y devuelve el resultado de la operación
        /// </summary>
        /// <param name="declarationId">Id de la declaración a anular</param>
        /// <param name="rif">Rif del comercio asociado</param>
        /// <param name="IsAnulled">Indica si la declaración pudo ser anulada</param>
        /// <returns>Resultado de la operacion</returns>
        public JsonResult TryAnnulDeclaration(string rif, Guid declarationId, bool? annulOrder)
        {
            // Inicializamos las variables y obtenemos la data
            UDeclaration declaration = DBLL.GetDeclaration(rif, declarationId);
            int dStatus = declaration.IdUDeclarationStatus;

            try
            {
                // Intentamos anular la declaracion
                DBLL.TryAnnulDeclaration(rif, declarationId, annulOrder);
            }
            catch (DeclarationAnnulledException e)
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
            catch (DeclarationReconciledException e)
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
            DBLL.SaveChanges();
            // Success
            _baseSuccessResponse = new BaseSuccessResponse(BackEndResources.BackEndSuccessMessage);
            var result = new
            {
                success = _baseSuccessResponse.Success,
                message = _baseSuccessResponse.Message
            };
            return new JsonResult { Data = result };
        }

        /// <summary>
        /// Metodo que confirma la declaracion hecha para un movimiento especifico
        /// </summary>
        /// <param name="declarationId">Id de la declaracion a confirmar</param>
        /// <param name="entryId">Id del movimiento asociado a la declaracion</param>
        /// <returns>True - La declaracion fue confirmada. False - La declaracion no fue confirmada.</returns>
        public JsonResult ReconcileDeclaration(Guid declarationId, Guid entryId)
        {
            Commerce commerce = new Commerce();
            UDeclaration declaration = new UDeclaration();
            UBankStatementEntry entry = new UBankStatementEntry();
            CommerceBalanceBLL CBBLL = new CommerceBalanceBLL();
            CommerceBLL CBLL = new CommerceBLL();
            //UDeclarationStatus declarationStatus = new UDeclarationStatus();
            try
            {
                // Obtenemos el comercio asociado a la declaracion
                commerce = CBLL.GetCommerce(declarationId);
                // Obtenemos el objeto de la declaracion a partir del id
                declaration = DBLL.GetEntity(declarationId);
                // Obtenemos el objeto del entry a partir del id
                entry = UBSERepo.GetAllRecords(e => e.Id == entryId).FirstOrDefault();
                // Verificamos que el entry y la declaracion obtenidos no sean nulos, la declaracion siga pendiente y realizo las validaciones nuevamente
                if (entry != null && declaration != null &&
                    entry.Amount == declaration.Amount &&
                    entry.UDeclarations.Count == 0 &&
                    entry.UBankStatement.IdUBank_Receiver == declaration.IdUBank &&
                    declaration.IdUDeclarationStatus == (int)DeclarationStatus.ReconciliationPending)
                {
                    // Le asignamos el entry a la declaracion
                    declaration.IdUBankStatementEntry = entry.Id;
                    // Cambiamos el estado de la declaracion a conciliada
                    declaration.IdUDeclarationStatus = (int)DeclarationStatus.Reconciled;
                    // Cambiamos el estado de la orden de compra a declarada y conciliada
                    declaration.CPurchaseOrder.IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.DeclaredReconciled;
                    // Guardamos los cambios de la base de datos
                    DBLL.SaveChanges();
                    // Registramos la transaccion en el balance del comercio
                    CBBLL.AddBalance("Conciliación Manual", commerce.Rif, declaration.Amount, declaration.Id, CommerceBalanceType.Declaration);                   
                    
                    // Retornamos true para indicar que la confirmacion fue exitosa
                    return new JsonResult()
                    {
                        Data = new
                        {
                            success = true,
                            message = BackEndResources.ReconcileDeclarationSuccess,
                            url = Url.Action("Index", "Declaration", new { isApproved = true })
                        }
                    };
                }
                // Si llega hasta este punto retornamos false ya que la confirmacion fue fallida.
                return new JsonResult()
                {
                    Data = new
                    {
                        success = false,
                        message = BackEndResources.ReconcileDeclarationFail,
                        url = Url.Action("Index", "Declaration", new { isApproved = false })
                    }
                };
            }
            catch (Exception)
            {
                // Si ocurrio algun error retornamos que la confirmacion fue fallida
                return new JsonResult()
                {
                    Data = new
                    {
                        success = false,
                        message = BackEndResources.ReconcileDeclarationFail,
                        url = Url.Action("Index", "Declaration", new { isApproved = false })
                    }
                };
            }

        }

        /// <summary>
        /// Metodo para obtener todas las declaraciones en el sistema
        /// </summary>
        /// <returns>Lista de <see cref="UDeclaration"/> registradas</returns>
        public List<UDeclaration> GetDeclarations()
        {
            // Variables
            List<UDeclaration> declarationList = new List<UDeclaration>();
            CUser currentUser = new CUser();

            // Usuario actual
            currentUser = CUBLL.GetCUser(User.Identity.GetUserId());

            // Declaraciones segun rol
            if (User.IsInRole(UserRoleConstant.TransaXAdmin) || (User.IsInRole(UserRoleConstant.TransaXUser)))
            {
                declarationList = DBLL.GetAllRecords().ToList();
            }
            else if (User.IsInRole(UserRoleConstant.CommerceAdmin) || (User.IsInRole(UserRoleConstant.CommerceUser)))
            {
                declarationList = DBLL.GetAllRecords(ud => ud.RifCommerce == currentUser.RifCommerce).ToList();
            }

            return declarationList;
        }

        /// <summary>
        /// Metodo que obtiene los posibles movimientos a asociar para una 
        /// declaracion especifica.
        /// </summary>
        /// <param name="declarationId">Id de la declaracion a asociar.</param>
        /// <returns>Liste de <see cref="UBankStatementEntry"/> posibles.</returns>
        public List<UBankStatementEntry> GetPosibleEntries(Guid declarationId)
        {
            // Seteo las variables
            List<UBankStatementEntry> entryList = new List<UBankStatementEntry>();
            UDeclaration declaration = new UDeclaration();
            // Obtengo la declaracion a partir del id
            declaration = DBLL.GetEntity(declarationId);
            /// Obtengo todos los movimientos que coincidan en monto y banco con la declaracion especificada
            /// y que no esten asociadas anteriormente a otra declaracion.
            entryList = UBSERepo.GetAllRecords(ubse => ubse.Amount == declaration.Amount &&
                                                       ubse.UBankStatement.IdUBank_Receiver == declaration.IdUBank &&
                                                       ubse.UDeclarations.Count == 0).ToList();

            return entryList;
        }

    }
}