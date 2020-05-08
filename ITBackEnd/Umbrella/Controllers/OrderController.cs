using BLL.Concrete;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.BackEnd;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.ITResources.Enums;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbrella.App_Code;

namespace Umbrella.Controllers
{
    [Authorize(Roles =
      UserRoleConstant.TransaXAdmin + "," +
      UserRoleConstant.TransaXUser + "," +
      UserRoleConstant.CommerceAdmin + "," +
      UserRoleConstant.CommerceUser
      )]
    public class OrderController : Controller
    {

        URepository<Commerce> CRepo = new URepository<Commerce>();
        CUserBLL CUBLL = new CUserBLL();
        DeclarationBLL DBLL = new DeclarationBLL();
        CPurchaseOrderBLL CPOBLL = new CPurchaseOrderBLL();
        Repository<CPurchaseOrderStatus> CPOSRepo = new Repository<CPurchaseOrderStatus>();

        BaseSuccessResponse _baseSuccessResponse;
        BaseErrorResponse _baseErrorResponse;

        private ApplicationUserManager _userManager;
        // GET: Order
        [SessionExpireFilter]
        public ActionResult Index()
        {
            var statusList = new List<CPurchaseOrderStatus>();
            var Orders = GetOrders();

            // Lista de estados para el filtro
            statusList = CPOSRepo.GetAllRecords().ToList();
            ViewBag.statusList = statusList;

            return View(Orders);
        }
        /// <summary>
        /// Devuelve la pestaña de ordenes
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <returns></returns>
        [HttpPost]
        [SessionExpireFilter]
        public ActionResult TabOrders(string rif)
        {
            var purchaseOrders = GetPurchaseOrders(rif);

            return PartialView("_TabOrders", purchaseOrders);
        }

        [HttpPost]
        public ActionResult SideOrder(Guid Id, string rif, bool? back, Guid? backId)
        {
            // Variables
            var purchaseOrder = new CPurchaseOrder();
            var DBLL = new DeclarationBLL();
            var CPOBLL = new CPurchaseOrderBLL();

            // Verificamos que la ventana no permita regresar al registro anterior
            if (back.Equals(null) || back.Equals(false))
            {
                // Le indicamos a la vista que no pinte el boton de regreso
                ViewBag.back = false;
                // Verificamos que exista un registro anterior
                if (backId != null && backId.Value.ToString() != string.Empty)
                {
                    // Obtenemos la orden de compra y declaraciones desde el id y rif del registro anterior
                    ViewBag.declarationsList = DBLL.GetDeclarations(backId.Value, rif);
                    purchaseOrder = CPOBLL.GetPurchaseOrder(rif, backId.Value);
                }
                else
                {
                    // Obtenemos la orden de compra y declaraciones desde el id y rif del registro actual
                    ViewBag.declarationsList = DBLL.GetDeclarations(Id, rif);
                    purchaseOrder = CPOBLL.GetPurchaseOrder(rif, Id);
                }
            }
            else
            {
                // Le indicamos a la vista que muestre el boton de regreso y el id del registro anterior
                ViewBag.back = true;
                ViewBag.backId = backId;
                // Obtenemos la orden de compra y declaraciones desde el id y rif del registro actual
                //ViewBag.declarationsList = GeDeclarations(Id, rif);
                //purchaseOrder = GetPurchaseOrder(rif, Id);
                ViewBag.declarationsList = DBLL.GetDeclarations(backId.Value, rif);
                purchaseOrder = CPOBLL.GetPurchaseOrder(rif, backId.Value);
            }
            // Retornamos la vista parcial con la orden de compra actual o anterior
            return PartialView("_SideOrder", purchaseOrder);
        }

        public List<CPurchaseOrder> GetOrders()
        {
            // Seteamos las variables
            List<CPurchaseOrder> OrderList = new List<CPurchaseOrder>();

            //OrderList = CPORepo.GetAllRecords().ToList();

            CUser currentUser = new CUser();

            currentUser = CUBLL.GetCUser(User.Identity.GetUserId());

            if (User.IsInRole(UserRoleConstant.TransaXAdmin) || (User.IsInRole(UserRoleConstant.TransaXUser)))
            {
                OrderList = CPOBLL.GetAllRecords().ToList();
            }
            else if (User.IsInRole(UserRoleConstant.CommerceAdmin) || (User.IsInRole(UserRoleConstant.CommerceUser)))
            {
                OrderList = CPOBLL.GetAllRecords(ud => ud.RifCommerce == currentUser.RifCommerce).ToList();
            }

            return OrderList;
        }

        /// <summary>
        /// Retorna la lista de ordenes de compra para el comercio actual
        /// </summary>
        /// <returns>Lista de <see cref="CPurchaseOrder"/></returns>
        [SessionExpireFilter]
        public List<CPurchaseOrder> GetPurchaseOrders(string rif)
        {
            // Seteamos las variables
            List<CPurchaseOrder> purchaseOrderList = new List<CPurchaseOrder>();

            // Obtenemos la lista de declaraciones asociadas al comercio perteneciente al usuario actual
            purchaseOrderList = CPOBLL.GetPurchaseOrders(rif).ToList();

            return purchaseOrderList;
        }

        /// <summary>
        /// Anula una orden de compra específica y declaraciones asociadas
        /// </summary>
        /// <param name="purchaseOrderId">Id de la orden de compra a anular</param>
        /// <param name="rif">Rif del comercio asociado</param>
        /// <returns>Resultado de la operacion</returns>
        public JsonResult TryAnnulPurchaseOrder(string rif, Guid purchaseOrderId)
        {
            // Variables
            bool isAnnulled;

            try
            {
                isAnnulled = CPOBLL.TryAnnulPurchaseOrder(purchaseOrderId, true);
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
            // Success
            _baseSuccessResponse = new BaseSuccessResponse(BackEndResources.AnnulOrderSuccess);
            var result = new
            {
                success = _baseSuccessResponse.Success,
                message = _baseSuccessResponse.Message
            };
            return new JsonResult { Data = result };

        }

    }
}