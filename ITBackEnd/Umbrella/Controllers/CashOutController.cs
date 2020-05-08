using BLL.Concrete;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models;
using InstaTransfer.BLL.Models.CashOut;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.BackEnd;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Constants;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
      UserRoleConstant.CommerceUser
     )]
    public class CashOutController : Controller
    {
        #region Variables
        URepository<Commerce> CRepo = new URepository<Commerce>();
        Repository<CashOutTransactionStatus> COTSRepo = new Repository<CashOutTransactionStatus>();
        CUserBLL CUBLL = new CUserBLL();
        CashOutTransactionBLL COTBLL = new CashOutTransactionBLL();
        BaseSuccessResponse _baseSuccessResponse;
        BaseErrorResponse _baseErrorResponse;

        private ApplicationUserManager _userManager;
        #endregion

        #region Actions

        // GET: Order
        [SessionExpireFilter]
        public ActionResult Index()
        {
            var statusList = new List<CashOutTransactionStatus>();
            var CashOuts = GetCashOuts();

            // Lista de estados para el filtro
            statusList = COTSRepo.GetAllRecords().ToList();
            ViewBag.statusList = statusList;

            return View(CashOuts);
        }

        /// <summary>
        /// Metodo que muestra los retiros especificos en el tab del comercio
        /// </summary>
        /// <param name="rif">Rif del coemrcio</param>
        /// <returns>Vista del tab de retiros</returns>
        [HttpPost]
        [SessionExpireFilter]
        public ActionResult TabCashOuts(string rif)
        {
            // Obtenemos todos los retiros del comercio
            var cashOuts = GetCashOuts(rif);
            // Retornamos el tab de retiros del comercio
            return PartialView("_TabCashOuts", cashOuts);
        }

        /// <summary>
        /// Metodo que muestra el sidebar del retiro seleccionado
        /// </summary>
        /// <param name="Id">Id del retiro</param>
        /// <param name="rif">Rif del coemrcio</param>
        /// <returns>Viste del sidebar del retiro especifico</returns>
        [HttpPost]
        public ActionResult SideCashOut(Guid idCashOut, string rif)
        {
            // Instanciamos el retiro
            var cashOut = COTBLL.GetEntity(idCashOut);
            // Retornamos el sidebar con el retiro seleccionado
            return PartialView("_SideCashOut", cashOut);
        }

        /// <summary>
        /// Metodo para solicitar un retiro
        /// </summary>
        /// <returns>Resultado</returns>
        [HttpPost]
        [Authorize(Roles =
          UserRoleConstant.CommerceAdmin + "," +
          UserRoleConstant.CommerceUser
        )]
        public ActionResult RequestCashOut()
        {
            // Instanciamos las variables
            var cashOut = new CashOutRequest();
            var totals = new CashOutTotals();
            var commerce = MySession.Current.CommerceUser.Commerce;
            var commissionFixed = commerce.Commission;
            var bankAccounts = commerce.CommerceBankAccounts.ToList();
            var minAmount = decimal.Parse(System.Configuration.ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyMinCashOut]) + commissionFixed;
            // Obtenemos los totales del balance
            using (CommerceBalanceBLL CBBLL = new CommerceBalanceBLL())
            {
                totals = CBBLL.GetRequestTotals(minAmount, commerce);
            }
            // Obtenemos el porcentaje de la comision
            var commissionPercentage = commerce.WithdrawalFee;
            // Asignamos los valores
            var IVAString = System.Configuration.ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyIVA];
            var IVA = decimal.Parse(IVAString) / 100;
            ViewBag.totals = totals;
            ViewBag.commissionPercentage = commissionPercentage;
            ViewBag.commissionPercentageString = commissionPercentage.ToString("P1");
            ViewBag.bankAccounts = bankAccounts;
            ViewBag.IVA = IVA;
            ViewBag.IVAString = IVAString;
            // Retornamos el sidebar con el formulario de retiro
            return PartialView("_RequestCashOut", cashOut);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Crea una nueva solicitud de retiro
        /// </summary>
        /// <param name="request">Solicitud a crear</param>
        /// <returns>Resultado de la operacion</returns>
        public JsonResult CreateCashOutRequest(CashOutRequest request)
        {
            // Inicializamos
            var commerce = MySession.Current.CommerceUser.Commerce;
            var amount = Convert.ToDecimal(request.Amount, CultureInfo.InvariantCulture);
            var commissionFixed = commerce.Commission;
            var minAmount = decimal.Parse(System.Configuration.ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyMinCashOut]) + commissionFixed;
            var errorList = new List<string>();

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

            // Verificamos que tenga fondos suficientes
            if (amount < minAmount)
            {
                throw new InsufficientFundsException(BackEndErrors.InsufficientFundsErrorMessage, BackEndErrors.InsufficientFundsErrorCode);
            }

            try
            {
                // Creamos el request en la base de datos
                using (CashOutTransactionBLL CTBLL = new CashOutTransactionBLL())
                {
                    CTBLL.CreateCashOutRequest(request, commerce);
                }
            }
            catch (InsufficientFundsException e)
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
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.CashOutRequestCreateErrorMessage, BackEndErrors.CashOutRequestCreateErrorCode);
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
                    message = BackEndResources.CashOutRequestSuccess,
                    url = Url.Action("Index", "CashOut")
                }
            };
        }

        /// <summary>
        /// Completa una solicitud especifica
        /// </summary>
        /// <param name="requestId">Id de la solicitud</param>
        /// <returnsResultado></returns>
        [HttpPost]
        [Authorize(Roles =
          UserRoleConstant.TransaXAdmin + "," +
          UserRoleConstant.TransaXUser
        )]
        public JsonResult CompleteCashOutRequest(Guid requestId)
        {
            var commerce = MySession.Current.CommerceUser.Commerce;

            try
            {
                using (CashOutTransactionBLL CTBLL = new CashOutTransactionBLL())
                {
                    CTBLL.CompleteCashOutRequest(requestId, commerce);
                }
            }
            catch (Exception)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.CashOutRequestCompleteErrorMessage, BackEndErrors.CashOutRequestCompleteErrorCode);
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
                    message = BackEndResources.CashOutRequestCompleteSuccessMessage,
                    url = Url.Action("Index", "CashOut")
                }
            };
        }


        /// <summary>
        /// Aprueba una solicitud especifica
        /// </summary>
        /// <param name="requestId">Id de la solicitud</param>
        /// <returns>Resultado</returns>
        [HttpPost]
        [Authorize(Roles =
          UserRoleConstant.TransaXAdmin + "," +
          UserRoleConstant.TransaXUser
        )]
        public JsonResult ApproveCashOutRequest(Guid requestId)
        {
            var commerce = new Commerce();
            var cashout = new CashOutTransaction();

            using (CashOutTransactionBLL COTBLL = new CashOutTransactionBLL())
            {
                cashout = COTBLL.GetEntity(requestId);
            }

            commerce = cashout.CommerceBalances.First().Commerce;

            try
            {
                using (CashOutTransactionBLL CTBLL = new CashOutTransactionBLL())
                {
                    CTBLL.ApproveCashOutRequest(requestId, commerce);
                }
            }
            catch (Exception)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.CashOutRequestApproveErrorMessage, BackEndErrors.CashOutRequestApproveErrorCode);
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
                    message = BackEndResources.CashOutRequestApproveSuccessMessage,
                    url = Url.Action("Index", "CashOut")
                }
            };
        }

        /// <summary>
        /// Anula una solicitud especifica
        /// </summary>
        /// <param name="requestId">Id de la solicitud</param>
        /// <returns>Resultado</returns>
        [HttpPost]
        [Authorize(Roles =
          UserRoleConstant.CommerceAdmin + "," +
          UserRoleConstant.CommerceUser
        )]
        public JsonResult AnnullCashOutRequest(Guid requestId)
        {
            var commerce = MySession.Current.CommerceUser.Commerce;

            try
            {
                using (CashOutTransactionBLL CTBLL = new CashOutTransactionBLL())
                {
                    CTBLL.AnnullCashOutRequest(requestId, commerce);
                }
            }
            catch (Exception)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.CashOutRequestAnnullErrorMessage, BackEndErrors.CashOutRequestAnnullErrorCode);
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
                    message = BackEndResources.CashOutRequestAnnullSuccessMessage,
                    url = Url.Action("Index", "CashOut")
                }
            };
        }

        /// <summary>
        /// Rechaza una solicitud especifica
        /// </summary>
        /// <param name="requestId">Id de la solicitud</param>
        /// <returnsResultado></returns>
        [HttpPost]
        [Authorize(Roles =
          UserRoleConstant.TransaXAdmin + "," +
          UserRoleConstant.TransaXUser
        )]
        public JsonResult RejectCashOutRequest(Guid requestId)
        {
            var commerce = new Commerce();
            var cashout = new CashOutTransaction();

            using (CashOutTransactionBLL COTBLL = new CashOutTransactionBLL())
            {
                cashout = COTBLL.GetEntity(requestId);
            }

            commerce = cashout.CommerceBalances.First().Commerce;

            try
            {
                using (CashOutTransactionBLL CTBLL = new CashOutTransactionBLL())
                {
                    CTBLL.RejectCashOutRequest(requestId, commerce);
                }
            }
            catch (Exception)
            {
                // Error
                _baseErrorResponse = new BaseErrorResponse(BackEndErrors.CashOutRequestRejectErrorMessage, BackEndErrors.CashOutRequestRejectErrorCode);
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
                    message = BackEndResources.CashOutRequestRejectSuccessMessage,
                    url = Url.Action("Index", "CashOut")
                }
            };
        }


        #endregion

        #region DataAccess

        /// <summary>
        /// Obtiene las solicitudes de retiro del sistema segun el rol del usuario
        /// </summary>
        /// <returns></returns>
        public List<CashOutTransaction> GetCashOuts()
        {
            // Inicializamos las variables
            List<CashOutTransaction> cashOutList = new List<CashOutTransaction>();

            // Verificamos el rol del usuario y obtenemos los registros completos o especificos
            if (User.IsInRole(UserRoleConstant.TransaXAdmin) || (User.IsInRole(UserRoleConstant.TransaXUser)))
            {
                cashOutList = COTBLL.GetAllRecords().ToList();
            }
            else if (User.IsInRole(UserRoleConstant.CommerceAdmin) || (User.IsInRole(UserRoleConstant.CommerceUser)))
            {
                cashOutList = COTBLL.GetAllRecords(cot => cot.CommerceBankAccount.RifCommerce == MySession.Current.CommerceUser.RifCommerce).ToList();
            }
            // Devolvemos la lista de retiros
            return cashOutList;
        }

        /// <summary>
        /// Obtiene las solicitudes de retiro de un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio especifico</param>
        /// <returns>Lista de solicitudes de retiro</returns>
        public List<CashOutTransaction> GetCashOuts(string rif)
        {
            // Seteamos las variables
            List<CashOutTransaction> cashOutList = new List<CashOutTransaction>();
            // Obtenemos la lista de todos los retiros del comercio
            cashOutList = COTBLL.GetAllRecords(cot => cot.CommerceBankAccount.RifCommerce == rif).ToList();
            // Devolvemos la lista de retiros
            return cashOutList;
        }
        #endregion

    }
}