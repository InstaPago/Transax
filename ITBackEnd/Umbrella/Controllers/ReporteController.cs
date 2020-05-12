using BLL.Concrete;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.Global;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
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
    public class ReporteController : Controller
    {

        #region Variables

        DeclarationBLL DBLL = new DeclarationBLL();
        CPurchaseOrderBLL CPOBLL = new CPurchaseOrderBLL();
        URepository<UBank> UBRepo = new URepository<UBank>();
        URepository<CUser> CURepo = new URepository<CUser>();
        URepository<AE_Variable> AE_VariablesREPO = new URepository<AE_Variable>();
        URepository<AE_Dolar> AE_DolaresREPO = new URepository<AE_Dolar>();
        URepository<UBankStatementEntry> UBSERepo = new URepository<UBankStatementEntry>();
        URepository<AE_Avance> AE_AvanceREPO = new URepository<InstaTransfer.DataAccess.AE_Avance>();
        URepository<AE_Variable> AE_VariableREPO = new URepository<AE_Variable>();
        URepository<AE_Dolar> AE_DolarREPO = new URepository<AE_Dolar>();
        URepository<AE_Operacion> AE_OperacionREPO = new URepository<AE_Operacion>();
        URepository<AE_OperacionPago> AE_OperacionPagoREPO = new URepository<AE_OperacionPago>();
        URepository<AE_EstadoCuenta> AE_EstadoCuentaREPO = new URepository<AE_EstadoCuenta>();
        URepository<AE_BalanceDiario> AE_BalanceDiarioREPO = new URepository<AE_BalanceDiario>();
        URepository<AE_BalanceAccione> AE_BalanceAccionesREPO = new URepository<AE_BalanceAccione>();
        URepository<AE_ValorAccion> AE_ValorAccionREPO = new URepository<AE_ValorAccion>();
        URepository<AE_ValorAccionTR> AE_ValorAccionTRREPO = new URepository<AE_ValorAccionTR>();
        URepository<AE_AdministradorPago> AE_AdministradorPagoREPO = new URepository<AE_AdministradorPago>();
        URepository<AE_CambioDiario> AE_CambioDiarioREPO = new URepository<AE_CambioDiario>();
        URepository<AE_Cierre> AE_CierreREPO = new URepository<AE_Cierre>();
        //URepository<>
        DateTime lastDashboardUpdate = BackEndGlobals.LastDashboardUpdate;
        int realTimeDeclarationsCount = BackEndGlobals.RealTimeDeclarationsCount;
        int realTimePurchaseOrdersCount = BackEndGlobals.RealTimePurchaseOrdersCount;
        #endregion
        // GET: Reporte
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Cierre()
        {
            List<AE_Cierre> Lista = AE_CierreREPO.GetAllRecords().OrderByDescending(u => u.Id).ToList();
            //ViewBag.Cierres = Lista;
            return View(Lista);
        }
    }
}