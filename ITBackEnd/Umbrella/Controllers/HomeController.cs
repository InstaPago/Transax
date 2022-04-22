using BLL.Concrete;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.Global;
using Microsoft.AspNet.Identity;
using Microsoft.Office.Interop.Excel;
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
    UserRoleConstant.Inversionista + "," +
    UserRoleConstant.Comercio + "," +
    UserRoleConstant.CommerceUser
    )]
    public class HomeController : Controller
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
        DateTime lastDashboardUpdate = BackEndGlobals.LastDashboardUpdate;
        int realTimeDeclarationsCount = BackEndGlobals.RealTimeDeclarationsCount;
        int realTimePurchaseOrdersCount = BackEndGlobals.RealTimePurchaseOrdersCount;
        #endregion

        [SessionExpireFilter]
        public ActionResult Index()
        {
            return View();
        }

        [SessionExpireFilter]
        public ActionResult Gasto()
        {
            return View();
        }

        [SessionExpireFilter]
        public ActionResult Configuracion(string _fecha)
        {
            DateTime fecha = DateTime.Now;
            if (_fecha != null && _fecha != string.Empty)
            {
                fecha = DateTime.Parse(_fecha);
                fecha = fecha.AddHours(23);
            }

            decimal acciones = 0;
            decimal accionesExistentes = 0;
            var balanceAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.Id).ToList();
            if (balanceAcciones.Count > 0)
            {
                acciones = AE_ValorAccionREPO.GetAllRecords().Take(5).OrderByDescending(u => u.FechaOperacion).FirstOrDefault().ValorAccion;
                accionesExistentes = balanceAcciones.FirstOrDefault().TotalAcciones;
            }
            ViewBag.ListaAvanceHistorico = AE_AvanceREPO.GetAllRecords().Where(u => (u.Id == 192 || u.Id == 193 || u.Id == 198 || u.Id == 200 || u.Id == 201 || u.Id == 203 || u.Id == 205 || u.Id > 205 ) && (u.IdEstatus == 1 || u.IdEstatus == 2)).ToList();
            ViewBag.MovimientosHistorico = AE_EstadoCuentaREPO.GetAllRecords().Where(u => !u.Abono && (u.AE_Avance.IdEstatus == 1 || u.AE_Avance.IdEstatus == 2)).ToList();
            ViewBag.Acciones = acciones;
            ViewBag.AccionesExistentes = accionesExistentes;
            ViewBag.Dolar = AE_DolaresREPO.GetAllRecords().Take(30).OrderByDescending(u => u.FechaValor).ToList();
            ViewBag.ValorAccion = AE_ValorAccionREPO.GetAllRecords().Take(30).OrderByDescending(u => u.FechaOperacion).ToList();
            ViewBag.ValorAccionTR = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.FechaOperacion).ToList();
            ViewBag.BalanceDiario = AE_BalanceDiarioREPO.GetAllRecords().Take(30).OrderByDescending(u => u.FechaOperaicon).ToList();
            ViewBag.CambioDiario = AE_CambioDiarioREPO.GetAllRecords().Where(u => u.FechaRegistro.Day == fecha.Day && u.FechaRegistro.Month == fecha.Month && u.FechaRegistro.Year == fecha.Year).ToList();
            List<AE_Avance> ListAvance = new List<AE_Avance>();
            ListAvance = AE_AvanceREPO.GetAllRecords().Where(u => u.IdEstatus == 1).ToList();

            decimal reembolso = 0;
            decimal cobrado = 0;
            decimal prestado = 0;
            foreach (var item in ListAvance)
            {
                cobrado = cobrado + item.AE_EstadoCuentas.Where(u => !u.Abono && u.FechaOperacion < fecha && u.SoloUtilidad).Sum(u => u.Monto);
                reembolso = reembolso + item.Reembolso;
                prestado = prestado + item.Avance;
            }

            ViewBag.Cobrado = cobrado;
            ViewBag.Reembolso = reembolso;
            ViewBag.Prestado = prestado;

            //DateTime Fecha = DateTime.Now;
            List<CobroDiarioGeneral> Lista = new List<CobroDiarioGeneral>();
            while (DateTime.Now >= fecha)
            {
                CobroDiarioGeneral item = new CobroDiarioGeneral();
                List<AE_EstadoCuenta> EstadoCuenta = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.AE_Avance.IdEstatus == 1 && u.FechaOperacion.Day == fecha.Day && u.FechaOperacion.Month == fecha.Month && u.FechaOperacion.Year == fecha.Year && !u.Abono).ToList();
                if (EstadoCuenta.Count > 0)
                {
                    item.Monto = EstadoCuenta.Sum(u => u.Monto);
                }
                else
                {
                    item.Monto = 0;
                }

                item.Fecha = fecha;
                item.Lista = EstadoCuenta;
                Lista.Add(item);
                fecha = fecha.AddDays(1);
            }
            ViewBag.CobroDiarios = Lista;

            return View();
        }


        public class CobroDiarioGeneral
        {
            public decimal Monto { get; set; }
            public DateTime Fecha { get; set; }

            public List<AE_EstadoCuenta> Lista { get; set; }
        }

        public bool AjustarValorAccion()
        {

            //OBTENER VALORES
            AE_ValorAccionTR ValorAccionTR = new AE_ValorAccionTR();
            ValorAccionTR.FechaCreacionRegistro = DateTime.Now;
            ValorAccionTR.FechaOperacion = DateTime.Now;
            ValorAccionTR.FechaUltimaActualizacion = DateTime.Now;
            //ValorAccionTR.UtilidadReportada = decimal.Parse(totalgananciadiaria.Replace('.', ','));
            AE_BalanceDiario Balance = AE_BalanceDiarioREPO.GetAllRecords().OrderByDescending(u => u.FechaCreacionRegistro).FirstOrDefault();
            ValorAccionTR.GastoReportado = 0;
            decimal Montoencuenta = Balance.TotalCuentaUsd;
            decimal Cobros = 0;
            decimal PendientePorcobrar = 0;

            foreach (var item in AE_AvanceREPO.GetAllRecords().Where(u => u.IdEstatus == 1))
            {
                decimal porcen = (item.Reembolso - item.Avance) * 100 / item.Avance;
                decimal montocobrado = item.AE_EstadoCuentas.Where(u => u.Abono == false && u.FechaOperacion < DateTime.Parse("01/09/2019")).Sum(u => u.Monto);
                //montocobrado = (montocobrado - (montocobrado * item.GastoBanco / 100));
                Cobros = Cobros + montocobrado;
                decimal _PendientePorcobrar = item.Reembolso - montocobrado;
                //_PendientePorcobrar = _PendientePorcobrar - (_PendientePorcobrar * porcen / 100);
                _PendientePorcobrar = _PendientePorcobrar - (_PendientePorcobrar / (1 + porcen));
                PendientePorcobrar = PendientePorcobrar + _PendientePorcobrar;
            }
            ValorAccionTR.NuevoCapital = PendientePorcobrar + Montoencuenta;
            ValorAccionTR.CapitalInicial = 0;
            ValorAccionTR.PagoCapitalInversionista = 0;
            ValorAccionTR.PagoUtilidadMesInversionista = 0;
            ValorAccionTR.TotalAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.FechaRegistro).FirstOrDefault().TotalAcciones;
            ValorAccionTR.PagoUtilidadAdministrador = 0;
            ValorAccionTR.ValorAccion = (PendientePorcobrar + Montoencuenta) / ValorAccionTR.TotalAcciones;
            ValorAccionTR.UtilidadReportada = 0;
            //AE_ValorAccionTRREPO.AddEntity(ValorAccionTR);
            //AE_ValorAccionTRREPO.SaveChanges();
            return true;
        }



        [SessionExpireFilter]
        public ActionResult Dashboard()
        {
            List<AE_Avance> ListAvance = new List<AE_Avance>();
            ListAvance = AE_AvanceREPO.GetAllRecords().Where(u => u.IdEstatus == 1 || u.IdEstatus == 2).ToList();
            ViewBag.CantidadAvancesActivos = ListAvance.Where(u => u.IdEstatus == 1).Count();
            ViewBag.CantidadAvancesRealizados = ListAvance.Count();
            ViewBag.CantidadAvancesRealizadosMes = ListAvance.Where(u => u.FechaCreacion.Month == DateTime.Now.Month && u.FechaCreacion.Year == DateTime.Now.Year).Count();
            ViewBag.CantidadAvancesRealizadosAno = ListAvance.Where(u => u.FechaCreacion.Year == DateTime.Now.Year).Count();
            int cantidad = ListAvance.Where(u => u.FechaCreacion.Month == DateTime.Now.Month && u.FechaCreacion.Year == DateTime.Now.Year).Count();
            if (cantidad != 0)
            {
                ViewBag.Promedio = ListAvance.Where(u => u.FechaCreacion.Month == DateTime.Now.Month && u.FechaCreacion.Year == DateTime.Now.Year).Sum(u => u.Avance) / cantidad;
            }
            else
            {
                ViewBag.Promedio = 0;
            }

            decimal reembolso = 0;
            decimal cobrado = 0;
            decimal prestado = 0;
            DateTime Fecha = DateTime.Now.AddDays(-20);
            List<CobroDiarioGeneral> Lista = new List<CobroDiarioGeneral>();
            while (DateTime.Now >= Fecha)
            {
                CobroDiarioGeneral item = new CobroDiarioGeneral();
                List<AE_EstadoCuenta> EstadoCuenta = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.FechaOperacion.Day == Fecha.Day && u.FechaOperacion.Month == Fecha.Month && u.FechaOperacion.Year == Fecha.Year && !u.Abono).ToList();
                if (EstadoCuenta.Count > 0)
                {
                    item.Monto = EstadoCuenta.Sum(u => u.Monto);
                }
                else
                {
                    item.Monto = 0;
                }
                //item.Fecha =
                item.Fecha = Fecha;
                item.Lista = EstadoCuenta;
                Lista.Add(item);
                Fecha = Fecha.AddDays(1);
            }
            ViewBag.CobroDiario = Lista;
            foreach (var item in ListAvance.Where(u => u.IdEstatus == 1).ToList())
            {
                decimal porcentajeoperacion = (item.Reembolso - item.Avance) * 100 / item.Avance;
                //decimal _cobrado = item.AE_EstadoCuentas.Where(u => !u.Abono && !u.SoloUtilidad).Sum(u => u.Monto);
                decimal _cobrado = 0;
                //reembolso = reembolso + (item.Reembolso - _cobrado);
                if (item.Modalidad)
                {

                    if (item.Id == 234)
                    { 
                    
                    }

                    _cobrado = item.AE_EstadoCuentas.Where(u => !u.Abono && !u.SoloUtilidad).Sum(u => u.Monto);
                    decimal _utilidadcobrada = item.AE_EstadoCuentas.Where(u => !u.Abono && u.SoloUtilidad).Sum(u => u.Monto);
                    decimal _utilidadesperada = (item.Reembolso - item.Avance) - _utilidadcobrada;
                    if (_utilidadesperada <= 0)
                        _utilidadesperada = 0;

                    reembolso = reembolso + (item.Avance - _cobrado) + _utilidadesperada;
                    //reembolso = reembolso + (item.Avance - _cobrado);
                }
                else {
                    decimal t_cobrado = item.AE_EstadoCuentas.Where(u => !u.Abono && !u.SoloUtilidad).Sum(u => u.Monto);

                    _cobrado = t_cobrado / (1 + (porcentajeoperacion/100));

                    reembolso = reembolso + (item.Reembolso - t_cobrado);
                }

                cobrado = cobrado + (_cobrado);
            
                if (item.Modalidad)
                {
                    prestado = prestado + (item.Avance - _cobrado);
                }
                else
                {
                    prestado = prestado + (item.Avance - _cobrado);
                }
            }

            ViewBag.Cobrado = cobrado;
            ViewBag.Reembolso = reembolso;
            ViewBag.Prestado = prestado;
            ViewBag.PendientePorCobrar = reembolso;

            ViewBag.Avances = ListAvance;


            // Declaramos variables
            DashboardViewModels.TransactionsByDateAndStatusModel transactionsModel;
            CUser currentUser = MySession.Current.CommerceUser;
            int currentDayPendingDeclarationsCount;
            int realTimeDeclarationsCount;
            int currentDayDeclarationsCount;
            decimal currentDayDeclarationsTotalAmount;
            int currentMonthDeclarationsCount;
            decimal currentMonthDeclarationsTotalAmount;
            int reconciledDeclarationsCount;
            decimal reconciledDeclarationsTotalAmount;
            // Obtenemos las transacciones desde la base de datos
            bool result = GetTransactionsCountByDateAndStatus(out transactionsModel);


            // Evaluamos resultado y devolvemos vistas
            if (result)
            {
                // Evaluamos el rol del usuario y 
                if (User.IsInRole(UserRoleConstant.TransaXAdmin) || User.IsInRole(UserRoleConstant.TransaXUser) || User.IsInRole(UserRoleConstant.Inversionista))
                {

                    // Listamos los top 10 comercios
                    List<DashboardViewModels.TopCommercesModel> topCommerces;
                    GetTopCommercesByDateAndTransactions(out topCommerces);
                    ViewBag.topCommerces = topCommerces;
                    // Creditos en cuenta por banco receptor
                    List<DashboardViewModels.TotalBankAccountCreditsModel> bankAccountCredits;
                    GetBankAccountCreditsBySocialReason(out bankAccountCredits);
                    ViewBag.bankAccountCredits = bankAccountCredits;
                    // Creditos en cuenta por banco emisor
                    List<DashboardViewModels.TotalBankCreditsModel> bankCredits;
                    GetBankCreditsBySocialReason(out bankCredits);
                    ViewBag.bankCredits = bankCredits;

                    // Creditos en cuenta
                    decimal creditsTotalAmount;
                    GetTotalCredits(out creditsTotalAmount);
                    ViewBag.creditsTotalAmount = creditsTotalAmount;
                    // Declaraciones pendientes del dia
                    GetCurrentDayPendingDeclarationsCount(out currentDayPendingDeclarationsCount);
                    ViewBag.currentDayPendingDeclarationsCount = currentDayPendingDeclarationsCount;
                    // Declaraciones pendientes en tiempo real
                    GetRealTimeDeclarationsCount(out realTimeDeclarationsCount);
                    ViewBag.realTimeDeclarationsCount = realTimeDeclarationsCount;
                    // Declaraciones conciliadas de hoy
                    GetCurrentDayDeclarationsCountAndTotalAmount(out currentDayDeclarationsCount, out currentDayDeclarationsTotalAmount);
                    ViewBag.currentDayDeclarationsCount = currentDayDeclarationsCount;
                    ViewBag.currentDayDeclarationsTotalAmount = currentDayDeclarationsTotalAmount;
                    // Declaraciones conciliadas del mes
                    GetCurrentMonthDeclarationsCountAndTotalAmount(out currentMonthDeclarationsCount, out currentMonthDeclarationsTotalAmount);
                    ViewBag.currentMonthDeclarationsCount = currentMonthDeclarationsCount;
                    ViewBag.currentMonthDeclarationsTotalAmount = currentMonthDeclarationsTotalAmount;
                    // Declaraciones conciliadas de por vida
                    GetReconciledDeclarationsCountAndTotalAmount(out reconciledDeclarationsCount, out reconciledDeclarationsTotalAmount);
                    ViewBag.reconciledDeclarationsCount = reconciledDeclarationsCount;
                    ViewBag.reconciledDeclarationsTotalAmount = reconciledDeclarationsTotalAmount;

                    // Total Declaraciones Conciliadas por Banco Emisor
                    List<DashboardViewModels.TotalBankAmountModel> bankTotalAmounts;

                    GetTotalAmountByDateAndBank(out bankTotalAmounts);

                    var bankNames = bankTotalAmounts.Select(b => b.Bank).ToArray();
                    var bankTotals = bankTotalAmounts.Select(b => b.TotalAmount).ToArray();

                    ViewBag.bankNames = bankNames;
                    ViewBag.bankTotals = bankTotals;

                    // Total Creditos en Cuenta por Banco Emisor
                    var chart = GetChartData();
                    ViewBag.chartData = chart;

                    // Devolvemos el dashboard correspondiente
                    return PartialView("_DashboardAvance", transactionsModel);
                }
                else if (User.IsInRole(UserRoleConstant.CommerceAdmin) || User.IsInRole(UserRoleConstant.CommerceUser))
                {
                    // Variables
                    CommerceBalanceBLL CBBLL = new CommerceBalanceBLL();
                    // Resumen del día
                    DashboardViewModels.DaySummaryModel daySummary = new DashboardViewModels.DaySummaryModel();
                    GetDaySummary(currentUser.RifCommerce, out daySummary);
                    ViewBag.daySummary = daySummary;

                    // Saldo Disponible
                    var lastBalance = CBBLL.GetLastBalance(currentUser.RifCommerce);
                    ViewBag.currentBalanceTotalAmount = lastBalance == null ? Convert.ToDecimal(0) : lastBalance.CurrentBalance;
                    // Declaraciones pendientes del dia
                    GetCurrentDayPendingDeclarationsCount(currentUser.RifCommerce, out currentDayPendingDeclarationsCount);
                    ViewBag.currentDayPendingDeclarationsCount = currentDayPendingDeclarationsCount;
                    // Declaraciones pendientes en tiempo real
                    GetRealTimeDeclarationsCount(currentUser.RifCommerce, out realTimeDeclarationsCount);
                    ViewBag.realTimeDeclarationsCount = realTimeDeclarationsCount;
                    // Declaraciones conciliadas de hoy
                    GetCurrentDayDeclarationsCountAndTotalAmount(currentUser.RifCommerce, out currentDayDeclarationsCount, out currentDayDeclarationsTotalAmount);
                    ViewBag.currentDayDeclarationsCount = currentDayDeclarationsCount;
                    ViewBag.currentDayDeclarationsTotalAmount = currentDayDeclarationsTotalAmount;
                    // Declaraciones conciliadas del mes
                    GetCurrentMonthDeclarationsCountAndTotalAmount(currentUser.RifCommerce, out currentMonthDeclarationsCount, out currentMonthDeclarationsTotalAmount);
                    ViewBag.currentMonthDeclarationsCount = currentMonthDeclarationsCount;
                    ViewBag.currentMonthDeclarationsTotalAmount = currentMonthDeclarationsTotalAmount;
                    // Declaraciones conciliadas de por vida
                    GetReconciledDeclarationsCountAndTotalAmount(currentUser.RifCommerce, out reconciledDeclarationsCount, out reconciledDeclarationsTotalAmount);
                    ViewBag.reconciledDeclarationsCount = reconciledDeclarationsCount;
                    ViewBag.reconciledDeclarationsTotalAmount = reconciledDeclarationsTotalAmount;

                    // Historial de transacciones
                    var chart = GetTimeScaleComboChartData(currentUser.RifCommerce, DateTime.Now.AddDays(-31), DateTime.Now, DeclarationStatus.Reconciled, PurchaseOrderStatus.Declared);
                    ViewBag.chartData = chart;



                    return PartialView("_DashboardCommerce", transactionsModel);
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return View();
            }

        }

        public bool BalancearFondo()
        {

            //OBTENER VALORES
            decimal totalutilidafondo = 0;
            decimal totalutilidinver = 0;
            decimal utilidadacumulado = 0;
            decimal retitocapital = 0;
            AE_ValorAccionTR ValorAccionTR = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.FechaCreacionRegistro).FirstOrDefault();
            List<AE_Operacion> Operaciones = AE_OperacionREPO.GetAllRecords().Where(u => u.IdEstatus == 1).ToList();
            foreach (var item in Operaciones)
            {
                ValorAccionTR = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.FechaCreacionRegistro).FirstOrDefault();
                decimal accionesviejas = item.RepresentacionFondo;
                decimal total = item.RepresentacionFondo * ValorAccionTR.ValorAccion;
                decimal utilidad = total - item.Monto;
                decimal utilidadreal = utilidad * item.PorcentajeGanancia / 100;
                decimal utilidadfondo = utilidad * (100 - item.PorcentajeGanancia) / 100;
                decimal montoactual = item.Monto;
                totalutilidinver = totalutilidinver + utilidadreal;
                totalutilidafondo = totalutilidafondo + utilidadfondo;

                //EVALUAMO SI NO HAY SOLICITUD DE RETIRO DE UTILIDAD
                if (item.RetiraUtilidadMes)
                {
                    //CREAMOS REGISTRO PAGO REINVERSION
                    AE_OperacionPago Pago = new AE_OperacionPago();
                    Pago.Date = DateTime.Now;
                    Pago.IdOperacion = item.Id;
                    Pago.Moneda = 1;
                    Pago.Monto = utilidadreal;
                    Pago.Tasa = 1;
                    Pago.FechaPago = DateTime.Now;
                    Pago.TipoPagoCapital = false;
                    Pago.TipoPagoUtilidad = true;
                    Pago.TipoReinversionUtilidad = false;
                    AE_OperacionPagoREPO.AddEntity(Pago);
                    AE_OperacionPagoREPO.SaveChanges();
                    utilidadacumulado = utilidadacumulado + utilidadreal;
                    //AJUSTAMOS OPERACION
                    decimal accionesnuevas = (montoactual) / ValorAccionTR.ValorAccion;
                    item.Monto = item.Monto;
                    item.MontoGanado = item.MontoGanado;
                    item.RepresentacionFondo = accionesnuevas;
                    AE_OperacionREPO.SaveChanges();

                    //AJUSTAMOS EL BALANCE DE LAS ACCIONES
                    decimal ajusteacciones = accionesnuevas - accionesviejas;
                    AE_BalanceAccione elemento = new AE_BalanceAccione();
                    var balanceAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.Id).FirstOrDefault();

                    elemento.TotaCapital = item.Monto + utilidadreal;
                    elemento.AccionesRetiradas = 0;
                    elemento.CapitalEntrada = 0;
                    elemento.CapitalSalida = 0;
                    elemento.FactorInicial = 0;
                    elemento.FechaOperacion = DateTime.Now;
                    elemento.FechaRegistro = DateTime.Now;
                    elemento.FechaUltimaActualizacion = DateTime.Now;
                    elemento.TotalAccionesExistentes = 0;
                    elemento.AccionesEntrantes = 0;
                    elemento.AccionesRetiradas = 0;
                    elemento.AccionesAjustadas = ajusteacciones;
                    elemento.TotalAcciones = balanceAcciones.TotalAcciones + ajusteacciones;
                    elemento.IdInversionista = item.IdInversionista;
                    elemento.ValorAcciones = ValorAccionTR.ValorAccion;
                    AE_BalanceAccionesREPO.AddEntity(elemento);
                    AE_BalanceAccionesREPO.SaveChanges();

                    AE_ValorAccionTR _NewValorAccionTR = new AE_ValorAccionTR();
                    _NewValorAccionTR.FechaCreacionRegistro = DateTime.Now;
                    _NewValorAccionTR.FechaOperacion = DateTime.Now;
                    _NewValorAccionTR.FechaUltimaActualizacion = DateTime.Now;

                    _NewValorAccionTR.GastoReportado = 0;
                    _NewValorAccionTR.CapitalInicial = ValorAccionTR.NuevoCapital;
                    _NewValorAccionTR.CapitalPorCobrar = ValorAccionTR.CapitalPorCobrar;
                    _NewValorAccionTR.AbonoCapital = 0;
                    _NewValorAccionTR.UtilidadReportada = 0;
                    //ValorAccionTR.SaldoUSD = Ultimo.SaldoUSD + decimal.Parse(totalcobrodiario.Replace('.', ','));
                    _NewValorAccionTR.SaldoUSD = ValorAccionTR.SaldoUSD - utilidadreal;
                    _NewValorAccionTR.TotalCobroDiario = 0;
                    //ValorAccionTR.NuevoCapital = Ultimo.NuevoCapital + decimal.Parse(totalgananciadiaria.Replace('.', ','));

                    _NewValorAccionTR.UsdTransito = 0;
                    _NewValorAccionTR.UsdVenezuela = 0;
                    _NewValorAccionTR.CuentaVenezuela = 0;
                    _NewValorAccionTR.NuevoCapital = ValorAccionTR.NuevoCapital - utilidadreal;

                    _NewValorAccionTR.PagoCapitalInversionista = 0;
                    _NewValorAccionTR.PagoUtilidadMesInversionista = utilidadreal * -1;
                    _NewValorAccionTR.PagoUtilidadAdministrador = 0;
                    _NewValorAccionTR.TotalAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.FechaRegistro).FirstOrDefault().TotalAcciones;
                    _NewValorAccionTR.ValorAccion = ValorAccionTR.ValorAccion;

                    AE_ValorAccionTRREPO.AddEntity(_NewValorAccionTR);
                    AE_ValorAccionTRREPO.SaveChanges();

                }
                else if (item.SeRetiraFondo)
                {
                    //CREAMOS REGISTRO PAGO REINVERSION
                    AE_OperacionPago Pago = new AE_OperacionPago();
                    Pago.Date = DateTime.Now;
                    Pago.IdOperacion = item.Id;
                    Pago.Moneda = 1;
                    Pago.Monto = item.Monto + utilidadreal;
                    Pago.Tasa = 1;
                    Pago.FechaPago = DateTime.Now;
                    Pago.TipoPagoCapital = true;
                    Pago.TipoPagoUtilidad = false;
                    Pago.TipoReinversionUtilidad = false;
                    AE_OperacionPagoREPO.AddEntity(Pago);
                    AE_OperacionPagoREPO.SaveChanges();
                    retitocapital = retitocapital + (item.Monto + utilidadreal);
                    //AJUSTAMOS OPERACION
                    decimal accionesnuevas = (montoactual + utilidadreal) / ValorAccionTR.ValorAccion;
                    item.Monto = item.Monto + utilidadreal;
                    item.MontoGanado = item.MontoGanado + utilidadreal;
                    item.RepresentacionFondo = accionesnuevas;
                    item.IdEstatus = 2;
                    AE_OperacionREPO.SaveChanges();

                    //AJUSTAMOS EL BALANCE DE LAS ACCIONES
                    decimal ajusteacciones = accionesnuevas - accionesviejas;
                    AE_BalanceAccione elemento = new AE_BalanceAccione();
                    var balanceAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.Id).FirstOrDefault();

                    elemento.TotaCapital = item.Monto + utilidadreal;
                    elemento.AccionesRetiradas = 0;
                    elemento.CapitalEntrada = 0;
                    elemento.CapitalSalida = 0;
                    elemento.FactorInicial = 0;
                    elemento.FechaOperacion = DateTime.Now;
                    elemento.FechaRegistro = DateTime.Now;
                    elemento.FechaUltimaActualizacion = DateTime.Now;
                    elemento.TotalAccionesExistentes = 0;
                    elemento.AccionesEntrantes = 0;
                    elemento.AccionesRetiradas = accionesviejas;
                    elemento.AccionesAjustadas = 0;
                    elemento.TotalAcciones = balanceAcciones.TotalAcciones - accionesviejas;
                    elemento.IdInversionista = item.IdInversionista;
                    elemento.ValorAcciones = ValorAccionTR.ValorAccion;
                    AE_BalanceAccionesREPO.AddEntity(elemento);
                    AE_BalanceAccionesREPO.SaveChanges();

                    AE_ValorAccionTR _NewValorAccionTR = new AE_ValorAccionTR();
                    _NewValorAccionTR.FechaCreacionRegistro = DateTime.Now;
                    _NewValorAccionTR.FechaOperacion = DateTime.Now;
                    _NewValorAccionTR.FechaUltimaActualizacion = DateTime.Now;

                    _NewValorAccionTR.GastoReportado = 0;
                    _NewValorAccionTR.CapitalInicial = ValorAccionTR.NuevoCapital;
                    _NewValorAccionTR.CapitalPorCobrar = ValorAccionTR.CapitalPorCobrar;
                    _NewValorAccionTR.AbonoCapital = 0;
                    _NewValorAccionTR.UtilidadReportada = 0;
                    //ValorAccionTR.SaldoUSD = Ultimo.SaldoUSD + decimal.Parse(totalcobrodiario.Replace('.', ','));
                    _NewValorAccionTR.SaldoUSD = ValorAccionTR.SaldoUSD - item.Monto;
                    _NewValorAccionTR.TotalCobroDiario = 0;
                    //ValorAccionTR.NuevoCapital = Ultimo.NuevoCapital + decimal.Parse(totalgananciadiaria.Replace('.', ','));

                    _NewValorAccionTR.UsdTransito = 0;
                    _NewValorAccionTR.UsdVenezuela = 0;
                    _NewValorAccionTR.CuentaVenezuela = 0;
                    _NewValorAccionTR.NuevoCapital = ValorAccionTR.NuevoCapital - item.Monto;

                    _NewValorAccionTR.PagoCapitalInversionista = item.Monto * -1;
                    _NewValorAccionTR.PagoUtilidadMesInversionista = 0;
                    _NewValorAccionTR.PagoUtilidadAdministrador = 0;
                    _NewValorAccionTR.TotalAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.FechaRegistro).FirstOrDefault().TotalAcciones;
                    _NewValorAccionTR.ValorAccion = ValorAccionTR.ValorAccion;

                    AE_ValorAccionTRREPO.AddEntity(_NewValorAccionTR);
                    AE_ValorAccionTRREPO.SaveChanges();

                }
                else
                {
                    //CREAMOS REGISTRO PAGO REINVERSION
                    AE_OperacionPago Pago = new AE_OperacionPago();
                    Pago.Date = DateTime.Now;
                    Pago.IdOperacion = item.Id;
                    Pago.Moneda = 1;
                    Pago.Monto = utilidadreal;
                    Pago.Tasa = 1;
                    Pago.FechaPago = DateTime.Now;
                    Pago.TipoPagoCapital = false;
                    Pago.TipoPagoUtilidad = false;
                    Pago.TipoReinversionUtilidad = true;
                    AE_OperacionPagoREPO.AddEntity(Pago);
                    AE_OperacionPagoREPO.SaveChanges();

                    //AJUSTAMOS OPERACION
                    decimal accionesnuevas = (montoactual + utilidadreal) / ValorAccionTR.ValorAccion;
                    item.Monto = item.Monto + utilidadreal;
                    item.MontoGanado = item.MontoGanado + utilidadreal;
                    item.RepresentacionFondo = accionesnuevas;
                    AE_OperacionREPO.SaveChanges();

                    //AJUSTAMOS EL BALANCE DE LAS ACCIONES
                    decimal ajusteacciones = accionesnuevas - accionesviejas;
                    AE_BalanceAccione elemento = new AE_BalanceAccione();
                    var balanceAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.Id).FirstOrDefault();

                    elemento.TotaCapital = item.Monto + utilidadreal;
                    elemento.AccionesRetiradas = 0;
                    elemento.CapitalEntrada = 0;
                    elemento.CapitalSalida = 0;
                    elemento.FactorInicial = 0;
                    elemento.FechaOperacion = DateTime.Now;
                    elemento.FechaRegistro = DateTime.Now;
                    elemento.FechaUltimaActualizacion = DateTime.Now;
                    elemento.TotalAccionesExistentes = 0;
                    elemento.AccionesEntrantes = 0;
                    elemento.AccionesRetiradas = 0;
                    elemento.AccionesAjustadas = ajusteacciones;
                    elemento.TotalAcciones = balanceAcciones.TotalAcciones + ajusteacciones;
                    elemento.IdInversionista = item.IdInversionista;
                    elemento.ValorAcciones = ValorAccionTR.ValorAccion;
                    AE_BalanceAccionesREPO.AddEntity(elemento);
                    AE_BalanceAccionesREPO.SaveChanges();


                }

                //PAGAMOS AL FONDO
                AE_AdministradorPago Admin = new AE_AdministradorPago();
                Admin.FechaRegistro = DateTime.Now;
                Admin.IdOperacion = item.Id;
                Admin.Moneda = 1;
                Admin.Monto = utilidadfondo;
                Admin.PagoUtilidad = true;
                Admin.Tasa = 1;
                AE_AdministradorPagoREPO.AddEntity(Admin);
                AE_AdministradorPagoREPO.SaveChanges();

                //AJUSTAMOS EL BALANCE
                //List<AE_ValorAccionTR> ListValorAccionTR = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.FechaCreacionRegistro).Take(5).ToList();


            }
            ValorAccionTR = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.FechaCreacionRegistro).FirstOrDefault();
            //var _balanceAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.Id).FirstOrDefault();
            AE_ValorAccionTR NewValorAccionTR = new AE_ValorAccionTR();
            NewValorAccionTR.FechaCreacionRegistro = DateTime.Now;
            NewValorAccionTR.FechaOperacion = DateTime.Now;
            NewValorAccionTR.FechaUltimaActualizacion = DateTime.Now;

            NewValorAccionTR.GastoReportado = 0;
            NewValorAccionTR.CapitalInicial = ValorAccionTR.NuevoCapital;
            NewValorAccionTR.CapitalPorCobrar = ValorAccionTR.CapitalPorCobrar;
            NewValorAccionTR.AbonoCapital = 0;
            NewValorAccionTR.UtilidadReportada = 0;
            //ValorAccionTR.SaldoUSD = Ultimo.SaldoUSD + decimal.Parse(totalcobrodiario.Replace('.', ','));
            NewValorAccionTR.SaldoUSD = ValorAccionTR.SaldoUSD - totalutilidafondo;
            NewValorAccionTR.TotalCobroDiario = 0;
            //ValorAccionTR.NuevoCapital = Ultimo.NuevoCapital + decimal.Parse(totalgananciadiaria.Replace('.', ','));

            NewValorAccionTR.UsdTransito = 0;
            NewValorAccionTR.UsdVenezuela = 0;
            NewValorAccionTR.CuentaVenezuela = 0;
            NewValorAccionTR.NuevoCapital = ValorAccionTR.NuevoCapital - totalutilidafondo;

            NewValorAccionTR.PagoCapitalInversionista = 0;
            NewValorAccionTR.PagoUtilidadAdministrador = totalutilidafondo * -1;
            NewValorAccionTR.PagoUtilidadMesInversionista = 0;
            NewValorAccionTR.TotalAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.FechaRegistro).FirstOrDefault().TotalAcciones;
            NewValorAccionTR.ValorAccion = ValorAccionTR.ValorAccion;

            AE_ValorAccionTRREPO.AddEntity(NewValorAccionTR);
            AE_ValorAccionTRREPO.SaveChanges();


            AE_Cierre Ultimo = AE_CierreREPO.GetAllRecords().OrderByDescending(u => u.Date).FirstOrDefault();
            AE_Cierre Nuevo = new AE_Cierre();
            Nuevo.CapitalFInalMes = NewValorAccionTR.CapitalNuevoIngreso;
            Nuevo.CapitalPrimeroMes = Ultimo.CapitalFInalMes;
            Nuevo.Date = DateTime.Now;
            Nuevo.Mes = DateTime.Now.AddMonths(-1).ToString("MMMM");
            Nuevo.MontoAdministrador = totalutilidafondo;
            Nuevo.MontoInversionista = totalutilidinver;
            Nuevo.PagoUtilida = utilidadacumulado;
            Nuevo.RetiroCapital = retitocapital;
            Nuevo.ValorAccionInicio = Ultimo.ValorAccionFin;
            Nuevo.ValorAccionFin = NewValorAccionTR.ValorAccion;
            Nuevo.Rendimiento = ((Nuevo.ValorAccionFin - Nuevo.ValorAccionInicio) * 100) / Nuevo.ValorAccionInicio;
            AE_CierreREPO.AddEntity(Nuevo);
            AE_CierreREPO.SaveChanges();

            return true;
        }



        public bool CerrarFondo()
        {
            List<AE_Avance> Avances = AE_AvanceREPO.GetAllRecords(u => u.IdEstatus == 1).ToList();

            foreach (var item in Avances)
            {
                List<AE_EstadoCuenta> estadocuenta = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.IdAvance == item.Id).ToList();
                decimal totalpagado = estadocuenta.Where(u => u.Abono == false && u.SoloUtilidad == false).Sum(u => u.Monto);
                decimal capitalprestado = 0;
                if (item.Modalidad)
                {
                    capitalprestado = item.Avance;
                }
                else
                {
                    capitalprestado = item.Reembolso;
                }

                decimal resto = capitalprestado - totalpagado;
                AE_EstadoCuenta estadocuentapago = new AE_EstadoCuenta();
                estadocuentapago.Abono = false;
                estadocuentapago.Descripcion = "Pago";
                estadocuentapago.Efectivo = false;
                estadocuentapago.EfectivoCambiado = false;
                estadocuentapago.Estatus = 1;
                estadocuentapago.FechaOperacion = DateTime.Now;
                estadocuentapago.FechaRegistro = DateTime.Now;
                estadocuentapago.IdAvance = item.Id;
                estadocuentapago.Lote = 99;
                estadocuentapago.Monto = resto;
                estadocuentapago.MontoBase = 0;
                estadocuentapago.MontoBs = 0;
                estadocuentapago.SaldoFinal = 0;
                estadocuentapago.SaldoInicial = 0;
                estadocuentapago.SoloUtilidad = false;
                estadocuentapago.Tasa = 0;
                AE_EstadoCuentaREPO.AddEntity(estadocuentapago);
                AE_EstadoCuentaREPO.SaveChanges();


            }
            return true;
        }


        public bool AjustarValorAccion()
        {

            //OBTENER VALORES
            AE_ValorAccionTR ValorAccionTR = new AE_ValorAccionTR();
            ValorAccionTR.FechaCreacionRegistro = DateTime.Now;
            ValorAccionTR.FechaOperacion = DateTime.Now;
            ValorAccionTR.FechaUltimaActualizacion = DateTime.Now;
            //ValorAccionTR.UtilidadReportada = decimal.Parse(totalgananciadiaria.Replace('.', ','));
            AE_BalanceDiario Balance = AE_BalanceDiarioREPO.GetAllRecords().OrderByDescending(u => u.FechaCreacionRegistro).FirstOrDefault();
            ValorAccionTR.GastoReportado = 0;
            decimal Montoencuenta = Balance.TotalCuentaUsd;
            decimal Cobros = 0;
            decimal PendientePorcobrar = 0;

            foreach (var item in AE_AvanceREPO.GetAllRecords().Where(u => u.IdEstatus == 1))
            {
                decimal porcen = (item.Reembolso - item.Avance) * 100 / item.Avance;
                decimal montocobrado = item.AE_EstadoCuentas.Where(u => u.Abono == false && u.FechaOperacion < DateTime.Parse("01/09/2019")).Sum(u => u.Monto);
                //montocobrado = (montocobrado - (montocobrado * item.GastoBanco / 100));
                Cobros = Cobros + montocobrado;
                decimal _PendientePorcobrar = item.Reembolso - montocobrado;
                //_PendientePorcobrar = _PendientePorcobrar - (_PendientePorcobrar * porcen / 100);
                _PendientePorcobrar = _PendientePorcobrar - (_PendientePorcobrar / (1 + porcen));
                PendientePorcobrar = PendientePorcobrar + _PendientePorcobrar;
            }
            ValorAccionTR.NuevoCapital = PendientePorcobrar + Montoencuenta;
            ValorAccionTR.CapitalInicial = 0;
            ValorAccionTR.PagoCapitalInversionista = 0;
            ValorAccionTR.PagoUtilidadMesInversionista = 0;
            ValorAccionTR.TotalAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.FechaRegistro).FirstOrDefault().TotalAcciones;
            ValorAccionTR.PagoUtilidadAdministrador = 0;
            ValorAccionTR.ValorAccion = (PendientePorcobrar + Montoencuenta) / ValorAccionTR.TotalAcciones;
            ValorAccionTR.UtilidadReportada = 0;
            //AE_ValorAccionTRREPO.AddEntity(ValorAccionTR);
            //AE_ValorAccionTRREPO.SaveChanges();
            return true;
        }

        #region Transax

        #region UBankStatementEntries

        /// <summary>
        /// Obtiene la suma de todos los creditos en cuenta
        /// </summary>
        /// <returns>True - Operacion exitosa. False operacion fallida.</returns>
        public bool GetTotalCredits(out decimal totalAmount)
        {
            try
            {
                // Obtenemos la lista de declaraciones
                var entries = UBSERepo.GetCredits();
                // Obtenemos la cuenta de las declaraciones por estado y creadas el mes actual
                int entriesCount = entries.Count();
                // Obtenemos el monto total de las declaraciones
                decimal entriesTotalAmount = entriesCount != 0 ? entries.Sum(d => d.Amount) : 0;
                // Retornamos el monto total
                totalAmount = entriesTotalAmount;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                totalAmount = 0;
                return false;
            }
        }

        /// <summary>
        /// Obtiene el monto total de creditos por banco receptor y cuenta bancaria
        /// </summary>
        /// <param name="bankAccountCredits">Modelo para total de creditos en cuenta por empresa</param>
        /// <returns>True - Operación exitosa. False - Operación fallida.</returns>
        public bool GetBankAccountCreditsBySocialReason(out List<DashboardViewModels.TotalBankAccountCreditsModel> bankAccountCredits)
        {
            var model = new DashboardViewModels();
            bankAccountCredits = new List<DashboardViewModels.TotalBankAccountCreditsModel>();
            try
            {
                // Obtenemos los movimientos bancarios
                var entries = UBSERepo.GetCredits();
                // Agrupamos por empresa y banco
                var bankAccount = entries.GroupBy(ubse => new { BankAccountName = ubse.UBankStatement.USocialReason.Name, Bank = ubse.UBankStatement.UBank.Name });
                // Seleccionamos la suma de los montos para cada empresa y banco
                var bankAccountTotals = bankAccount.Select(group => new
                {
                    BankAccountName = group.Key.BankAccountName,
                    Bank = group.Key.Bank,
                    TotalAmount = group.Sum(ubse => ubse.Amount)
                });
                // Construimos el modelo
                foreach (var bankAccountTotal in bankAccountTotals)
                {
                    // Añadimos los registros a la lista del modelo
                    bankAccountCredits.Add(new DashboardViewModels.TotalBankAccountCreditsModel
                    {
                        BankAccount = bankAccountTotal.BankAccountName,
                        ReceivingBank = bankAccountTotal.Bank,
                        TotalAmount = bankAccountTotal.TotalAmount
                    });
                }
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                return false;
            }
        }

        /// <summary>
        /// Obtiene el monto total de los creditos por banco emisor y cuenta bancaria
        /// </summary>
        /// <param name="bankCredits">Modelo para total de creditos en cuenta por empresa</param>
        /// <returns>True - Operación exitosa. False - Operación fallida.</returns>
        public bool GetBankCreditsBySocialReason(out List<DashboardViewModels.TotalBankCreditsModel> bankCredits)
        {
            var model = new DashboardViewModels();
            bankCredits = new List<DashboardViewModels.TotalBankCreditsModel>();
            try
            {
                // Obtenemos los movimientos bancarios
                var entries = UBSERepo.GetCredits();
                // Agrupamos por empresa y banco
                var bankAccount = entries.GroupBy(ubse => new { BankAccountName = ubse.UBankStatement.USocialReason.Name, Bank = ubse.UBank.Name });
                // Seleccionamos la suma de los montos para cada empresa y banco
                var bankAccountTotals = bankAccount.Select(group => new
                {
                    BankAccountName = group.Key.BankAccountName,
                    Bank = group.Key.Bank,
                    TotalAmount = group.Sum(ubse => ubse.Amount)
                });
                // Construimos el modelo
                foreach (var bankAccountTotal in bankAccountTotals)
                {
                    // Añadimos los registros a la lista del modelo
                    bankCredits.Add(new DashboardViewModels.TotalBankCreditsModel
                    {
                        BankAccount = bankAccountTotal.BankAccountName,
                        IssuingBank = bankAccountTotal.Bank != null ? bankAccountTotal.Bank : "No Identificado",
                        TotalAmount = bankAccountTotal.TotalAmount
                    });
                }
                // Ordenamos por Cuenta
                bankCredits = bankCredits.OrderBy(c => c.BankAccount).ToList();
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                return false;
            }
        }

        #endregion

        #region Transactions

        public bool GetTransactionsCountByDateAndStatus(string rif, out DashboardViewModels.TransactionsByDateAndStatusModel model)
        {
            model = new DashboardViewModels.TransactionsByDateAndStatusModel();

            try
            {
                // Declaraciones Anuladas
                int AnnuledDeclarationsCount;
                decimal AnnuledDeclarationsTotalAmount;
                model.AnnuledDeclarations.Result = GeDeclarationsCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, DeclarationStatus.Annulled, out AnnuledDeclarationsCount, out AnnuledDeclarationsTotalAmount);
                model.AnnuledDeclarations.Count = AnnuledDeclarationsCount;
                model.AnnuledDeclarations.TotalAmount = AnnuledDeclarationsTotalAmount;
                // Declaraciones Conciliadas
                int ReconciledDeclarationsCount;
                decimal ReconciledDeclarationsTotalAmount;
                model.ReconciledDeclarations.Result = GeDeclarationsCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, DeclarationStatus.Reconciled, out ReconciledDeclarationsCount, out ReconciledDeclarationsTotalAmount);
                model.ReconciledDeclarations.Count = ReconciledDeclarationsCount;
                model.ReconciledDeclarations.TotalAmount = ReconciledDeclarationsTotalAmount;
                // Declaraciones Pendientes
                int PendingDeclarationsCount;
                decimal PendingDeclarationsTotalAmount;
                model.PendingDeclarations.Result = GeDeclarationsCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, DeclarationStatus.ReconciliationPending, out PendingDeclarationsCount, out PendingDeclarationsTotalAmount);
                model.PendingDeclarations.Count = PendingDeclarationsCount;
                model.PendingDeclarations.TotalAmount = PendingDeclarationsTotalAmount;
                // Ordenes de Compra Pendientes
                int AnnuledPurchaseOrdersCount;
                decimal AnnuledPurchaseOrdersTotalAmount;
                model.AnnuledPurchaseOrders.Result = GetPurchaseOrdersCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, PurchaseOrderStatus.Annulled, out AnnuledPurchaseOrdersCount, out AnnuledPurchaseOrdersTotalAmount);
                model.AnnuledPurchaseOrders.Count = AnnuledPurchaseOrdersCount;
                model.AnnuledPurchaseOrders.TotalAmount = AnnuledPurchaseOrdersTotalAmount;
                // Ordenes de Compra Declaradas
                int DeclaredPurchaseOrdersCount;
                decimal DeclaredPurchaseOrdersTotalAmount;
                model.DeclaredPurchaseOrders.Result = GetPurchaseOrdersCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, PurchaseOrderStatus.Declared, out DeclaredPurchaseOrdersCount, out DeclaredPurchaseOrdersTotalAmount);
                model.DeclaredPurchaseOrders.Count = DeclaredPurchaseOrdersCount;
                model.DeclaredPurchaseOrders.TotalAmount = DeclaredPurchaseOrdersTotalAmount;
                // Ordenes de Compra Pendientes
                int PendingPurchaseOrdersCount;
                decimal PendingPurchaseOrdersTotalAmount;
                model.PendingPurchaseOrders.Result = GetPurchaseOrdersCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, PurchaseOrderStatus.DeclarationPending, out PendingPurchaseOrdersCount, out PendingPurchaseOrdersTotalAmount);
                model.PendingPurchaseOrders.Count = PendingPurchaseOrdersCount;
                model.PendingPurchaseOrders.TotalAmount = PendingPurchaseOrdersTotalAmount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                return false;
            }
        }

        public bool GetTransactionsCountByDateAndStatus(out DashboardViewModels.TransactionsByDateAndStatusModel model)
        {
            model = new DashboardViewModels.TransactionsByDateAndStatusModel();

            try
            {
                // Declaraciones Anuladas
                int AnnuledDeclarationsCount;
                decimal AnnuledDeclarationsTotalAmount;
                model.AnnuledDeclarations.Result = GeDeclarationsCountAndTotalAmountByDateAndStatus(DateTime.Now, DateType.Month, DeclarationStatus.Annulled, out AnnuledDeclarationsCount, out AnnuledDeclarationsTotalAmount);
                model.AnnuledDeclarations.Count = AnnuledDeclarationsCount;
                model.AnnuledDeclarations.TotalAmount = AnnuledDeclarationsTotalAmount;
                // Declaraciones Conciliadas
                int ReconciledDeclarationsCount;
                decimal ReconciledDeclarationsTotalAmount;
                model.ReconciledDeclarations.Result = GeDeclarationsCountAndTotalAmountByDateAndStatus(DateTime.Now, DateType.Month, DeclarationStatus.Reconciled, out ReconciledDeclarationsCount, out ReconciledDeclarationsTotalAmount);
                model.ReconciledDeclarations.Count = ReconciledDeclarationsCount;
                model.ReconciledDeclarations.TotalAmount = ReconciledDeclarationsTotalAmount;
                // Declaraciones Pendientes
                int PendingDeclarationsCount;
                decimal PendingDeclarationsTotalAmount;
                model.PendingDeclarations.Result = GeDeclarationsCountAndTotalAmountByDateAndStatus(DateTime.Now, DateType.Month, DeclarationStatus.ReconciliationPending, out PendingDeclarationsCount, out PendingDeclarationsTotalAmount);
                model.PendingDeclarations.Count = PendingDeclarationsCount;
                model.PendingDeclarations.TotalAmount = PendingDeclarationsTotalAmount;
                // Ordenes de Compra Pendientes
                int AnnuledPurchaseOrdersCount;
                decimal AnnuledPurchaseOrdersTotalAmount;
                model.AnnuledPurchaseOrders.Result = GetPurchaseOrdersCountAndTotalAmountByDateAndStatus(DateTime.Now, DateType.Month, PurchaseOrderStatus.Annulled, out AnnuledPurchaseOrdersCount, out AnnuledPurchaseOrdersTotalAmount);
                model.AnnuledPurchaseOrders.Count = AnnuledPurchaseOrdersCount;
                model.AnnuledPurchaseOrders.TotalAmount = AnnuledPurchaseOrdersTotalAmount;
                // Ordenes de Compra Declaradas
                int DeclaredPurchaseOrdersCount;
                decimal DeclaredPurchaseOrdersTotalAmount;
                model.DeclaredPurchaseOrders.Result = GetPurchaseOrdersCountAndTotalAmountByDateAndStatus(DateTime.Now, DateType.Month, PurchaseOrderStatus.Declared, out DeclaredPurchaseOrdersCount, out DeclaredPurchaseOrdersTotalAmount);
                model.DeclaredPurchaseOrders.Count = DeclaredPurchaseOrdersCount;
                model.DeclaredPurchaseOrders.TotalAmount = DeclaredPurchaseOrdersTotalAmount;
                // Ordenes de Compra Pendientes
                int PendingPurchaseOrdersCount;
                decimal PendingPurchaseOrdersTotalAmount;
                model.PendingPurchaseOrders.Result = GetPurchaseOrdersCountAndTotalAmountByDateAndStatus(DateTime.Now, DateType.Month, PurchaseOrderStatus.DeclarationPending, out PendingPurchaseOrdersCount, out PendingPurchaseOrdersTotalAmount);
                model.PendingPurchaseOrders.Count = PendingPurchaseOrdersCount;
                model.PendingPurchaseOrders.TotalAmount = PendingPurchaseOrdersTotalAmount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                return false;
            }
        }

        #region UDeclaration

        public bool GeDeclarationsCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las declaraciones del comercio
                int declarationCount = DBLL.GetDeclarations(rif).Count();
                // Retornamos la cuenta
                result = declarationCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }
        }
        public bool GeDeclarationsCount(out int result)
        {
            try
            {
                // Obtenemos la cuenta de las declaraciones del comercio
                int declarationCount = DBLL.GetAllRecords().Count();
                // Retornamos la cuenta
                result = declarationCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }
        }

        #region Status

        public bool GetPendingDeclarationsCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las declaraciones pendientes
                int declarationCount = DBLL.GetDeclarationsByStatus(rif, DeclarationStatus.ReconciliationPending).Count();
                // Retornamos la cuenta
                result = declarationCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }
        }

        public bool GetAnnuledDeclarationsCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las declaraciones anuladas
                int declarationCount = DBLL.GetDeclarationsByStatus(rif, DeclarationStatus.Annulled).Count();
                // Retornamos la cuenta
                result = declarationCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }
        }

        public bool GetReconciledDeclarationsCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las declaraciones conciliadas
                int declarationCount = DBLL.GetDeclarationsByStatus(rif, DeclarationStatus.Reconciled).Count();
                // Retornamos la cuenta
                result = declarationCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }
        }

        public bool GetReconciledDeclarationsCount(out int result)
        {
            try
            {
                // Obtenemos la cuenta de las declaraciones conciliadas
                int declarationCount = DBLL.GetDeclarationsByStatus(DeclarationStatus.Reconciled).Count();
                // Retornamos la cuenta
                result = declarationCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }
        }

        #endregion

        #region Date

        public bool GetRealTimeDeclarationsCount(out int result)
        {
            try
            {
                // Obtenemos la cuenta de las declaraciones en tiempo real
                realTimeDeclarationsCount = DBLL.GetDeclarationsByDate(lastDashboardUpdate, DateType.RealTime).Count();
                // Actualizamos el tiempo de la ultima consulta
                BackEndGlobals.LastDashboardUpdate = DateTime.Now;
                // Retornamos la cuenta
                result = realTimeDeclarationsCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }
        }
        public bool GetCurrentDayDeclarationsCountAndTotalAmount(out int count, out decimal totalAmount)
        {
            try
            {
                // Obtenemos la lista de declaraciones
                var declarations = DBLL.GetDeclarationsByDateAndStatus(DateTime.Now, DateType.Day, DeclarationStatus.Reconciled);
                // Obtenemos la cuenta de las declaraciones por estado y creadas el dia actual
                int declarationCount = declarations.Count();
                // Obtenemos el monto total de las declaraciones
                decimal declarationsTotalAmount = declarationCount != 0 ? declarations.Sum(d => d.Amount) : 0;
                // Retornamos la cuenta
                count = declarationCount;
                // Retornamos el monto total
                totalAmount = declarationsTotalAmount;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                totalAmount = 0;
                return false;
            }
        }
        public bool GetCurrentDayPendingDeclarationsCount(out int count)
        {
            try
            {
                // Obtenemos la cuenta de las declaraciones creadas el mes actual
                int declarationCount = DBLL.GetDeclarationsByDateAndStatus(DateTime.Today, DateType.Day, DeclarationStatus.ReconciliationPending).Count();
                // Retornamos la cuenta
                count = declarationCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                return false;
            }
        }
        public bool GetCurrentMonthDeclarationsCountAndTotalAmount(out int count, out decimal totalAmount)
        {
            try
            {
                // Obtenemos la lista de declaraciones
                var declarations = DBLL.GetDeclarationsByDateAndStatus(DateTime.Now, DateType.Month, DeclarationStatus.Reconciled);
                // Obtenemos la cuenta de las declaraciones por estado y creadas el mes actual
                int declarationCount = declarations.Count();
                // Obtenemos el monto total de las declaraciones
                decimal declarationsTotalAmount = declarationCount != 0 ? declarations.Sum(d => d.Amount) : 0;
                // Retornamos la cuenta
                count = declarationCount;
                // Retornamos el monto total
                totalAmount = declarationsTotalAmount;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                totalAmount = 0;
                return false;
            }
        }
        public bool GetReconciledDeclarationsCountAndTotalAmount(out int count, out decimal totalAmount)
        {
            try
            {
                // Obtenemos la lista de declaraciones
                var declarations = DBLL.GetDeclarationsByStatus(DeclarationStatus.Reconciled);
                // Obtenemos la cuenta de las declaraciones por estado y creadas el mes actual
                int declarationCount = declarations.Count();
                // Obtenemos el monto total de las declaraciones
                decimal declarationsTotalAmount = declarationCount != 0 ? declarations.Sum(d => d.Amount) : 0;
                // Retornamos la cuenta
                count = declarationCount;
                // Retornamos el monto total
                totalAmount = declarationsTotalAmount;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                totalAmount = 0;
                return false;
            }
        }

        #endregion

        #region Amount

        public bool GetCurrentDayPendingDeclarationsTotalAmount(string rif, out decimal result)
        {
            try
            {
                // Obtenemos las declaraciones creadas el dia de hoy
                List<UDeclaration> declarations = DBLL.GetDeclarationsByDateAndStatus(rif, DateTime.Today, DateType.Day, DeclarationStatus.ReconciliationPending);
                // Obtenemos el monto total de las declaraciones
                var totalAmount = declarations.Sum(ud => ud.Amount);
                // Retornamos el total
                result = totalAmount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        public bool GetCurrentMonthPendingDeclarationsTotalAmount(string rif, out decimal result)
        {
            try
            {
                // Obtenemos las declaraciones creadas el mes actual
                List<UDeclaration> declarations = DBLL.GetDeclarationsByDateAndStatus(rif, DateTime.Today, DateType.Month, DeclarationStatus.ReconciliationPending);
                // Obtenemos el monto total de las declaraciones
                var totalAmount = declarations.Sum(ud => ud.Amount);
                // Retornamos el total
                result = totalAmount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        public bool GetCurrentDayReconciledDeclarationsTotalAmount(string rif, out decimal result)
        {
            try
            {
                // Obtenemos las declaraciones creadas el dia de hoy
                List<UDeclaration> declarations = DBLL.GetDeclarationsByDateAndStatus(rif, DateTime.Today, DateType.Day, DeclarationStatus.Reconciled);
                // Obtenemos el monto total de las declaraciones
                var totalAmount = declarations.Sum(ud => ud.Amount);
                // Retornamos el total
                result = totalAmount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        public bool GetCurrentMonthReconciledDeclarationsTotalAmount(string rif, out decimal result)
        {
            try
            {
                // Obtenemos las declaraciones creadas el mes actual
                List<UDeclaration> declarations = DBLL.GetDeclarationsByDateAndStatus(rif, DateTime.Today, DateType.Month, DeclarationStatus.Reconciled);
                // Obtenemos el monto total de las declaraciones
                var totalAmount = declarations.Sum(ud => ud.Amount);
                // Retornamos el total
                result = totalAmount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        #endregion

        #endregion

        #region CPurchaseOrder

        public bool GetPurchaseOrdersCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las ordenes de compra del comercio
                int purchaseOrderCount = CPOBLL.GetPurchaseOrders(rif).Count();
                // Retornamos la cuenta
                result = purchaseOrderCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        #region Status

        public bool GetReconciledDeclarationPurchaseOrdersCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las ordenes de compra con declaraciones conciliadas
                int purchaseOrderCount = CPOBLL.GetPurchaseOrdersByStatus(rif, PurchaseOrderStatus.DeclarationPending).Count();
                // Retornamos la cuenta
                result = purchaseOrderCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        public bool GetPendingPurchaseOrdersCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las ordenes de compra pendientes
                int purchaseOrderCount = CPOBLL.GetPurchaseOrdersByStatus(rif, PurchaseOrderStatus.DeclarationPending).Count();
                // Retornamos la cuenta
                result = purchaseOrderCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        public bool GetAnnuledPurchaseOrdersCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las ordenes de compra anuladas
                int purchaseOrderCount = CPOBLL.GetPurchaseOrdersByStatus(rif, PurchaseOrderStatus.Annulled).Count();
                // Retornamos la cuenta
                result = purchaseOrderCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        public bool GetDeclaredPurchaseOrdersCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las ordenes de compra declaradas
                int purchaseOrderCount = CPOBLL.GetPurchaseOrdersByStatus(rif, PurchaseOrderStatus.Declared).Count();
                // Retornamos la cuenta
                result = purchaseOrderCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }
        }

        #endregion

        #region Date

        public bool GetRealTimePurchaseOrdersCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las ordenes en tiempo real
                realTimePurchaseOrdersCount = CPOBLL.GetPurchaseOrdersByDate(rif, lastDashboardUpdate, DateType.RealTime).Count();
                // Actualizamos el tiempo de la ultima consulta
                BackEndGlobals.LastDashboardUpdate = DateTime.Now;
                // Retornamos la cuenta
                result = realTimePurchaseOrdersCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        public bool GetCurrentDayPurchaseOrdersCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las ordenes de compra creadas el dia de hoy
                int purchaseOrderCount = CPOBLL.GetPurchaseOrdersByDate(rif, DateTime.Today, DateType.Day).Count();
                // Retornamos la cuenta
                result = purchaseOrderCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        public bool GetCurrentMonthPurchaseOrdersCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las ordenes de compra creadas el mes actual
                int purchaseOrderCount = CPOBLL.GetPurchaseOrdersByDate(rif, DateTime.Today, DateType.Month).Count();
                // Retornamos la cuenta
                result = purchaseOrderCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        #endregion

        #region Amount

        public bool GetCurrentDayPendingPurchaseOrdersTotalAmount(string rif, out decimal result)
        {
            try
            {
                // Obtenemos las ordenes de compra creadas el dia de hoy
                List<CPurchaseOrder> purchaseOrders = CPOBLL.GetPurchaseOrdersByDateAndStatus(rif, DateTime.Today, DateType.Day, PurchaseOrderStatus.DeclarationPending);
                // Obtenemos el monto total de las ordenes de compra
                var totalAmount = purchaseOrders.Sum(ud => ud.Amount);
                // Retornamos el total
                result = totalAmount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        public bool GetCurrentMonthPendingPurchaseOrdersTotalAmount(string rif, out decimal result)
        {
            try
            {
                // Obtenemos las ordenes de compra creadas el mes actual
                List<CPurchaseOrder> purchaseOrders = CPOBLL.GetPurchaseOrdersByDateAndStatus(rif, DateTime.Today, DateType.Month, PurchaseOrderStatus.DeclarationPending);
                // Obtenemos el monto total de las ordenes de compra
                var totalAmount = purchaseOrders.Sum(ud => ud.Amount);
                // Retornamos el total
                result = totalAmount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        public bool GetCurrentDayDeclaredPurchaseOrdersTotalAmount(string rif, out decimal result)
        {
            try
            {
                // Obtenemos las ordenes de compra creadas el dia de hoy
                List<CPurchaseOrder> purchaseOrders = CPOBLL.GetPurchaseOrdersByDateAndStatus(rif, DateTime.Today, DateType.Day, PurchaseOrderStatus.DeclarationPending);
                // Obtenemos el monto total de las ordenes de compra
                var totalAmount = purchaseOrders.Sum(ud => ud.Amount);
                // Retornamos el total
                result = totalAmount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }

        public bool GetCurrentMonthDeclaredPurchaseOrdersTotalAmount(string rif, out decimal result)
        {
            try
            {
                // Obtenemos las ordenes de compra creadas el mes actual
                List<CPurchaseOrder> purchaseOrders = CPOBLL.GetPurchaseOrdersByDateAndStatus(rif, DateTime.Today, DateType.Month, PurchaseOrderStatus.DeclarationPending);
                // Obtenemos el monto total de las ordenes de compra
                var totalAmount = purchaseOrders.Sum(ud => ud.Amount);
                // Retornamos el total
                result = totalAmount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }

        }


        #endregion

        #endregion

        #endregion

        #region TopCommerces

        /// <summary>
        /// Top 10 de comercios del mes por declaraciones conciliadas
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool GetTopCommercesByDateAndTransactions(out List<DashboardViewModels.TopCommercesModel> topCommerces)
        {
            var model = new DashboardViewModels();
            topCommerces = new List<DashboardViewModels.TopCommercesModel>();

            try
            {
                // Obtenemos las declaraciones conciliadas del mes
                var declarations = DBLL.GetDeclarationsByDateAndStatus(DateTime.Now, DateType.Month, DeclarationStatus.Reconciled).ToList();
                // Agrupamos por bancos
                var commerces = declarations.GroupBy(ud => ud.Commerce.BusinessName);
                // Seleccionamos la suma de los montos y cuenta para cada comercio
                var commerceTotals = commerces.Select(group => new
                {
                    Commerce = group.Key,
                    Count = group.Count(),
                    TotalAmount = group.Sum(ud => ud.Amount)
                });
                // Construimos el modelo
                foreach (var commerceTotal in commerceTotals)
                {
                    // Añadimos los registros a la lista del modelo
                    model.TopCommercesModelList.Add(new DashboardViewModels.TopCommercesModel
                    {
                        Commerce = commerceTotal.Commerce,
                        Count = commerceTotal.Count,
                        TotalAmount = commerceTotal.TotalAmount
                    });
                }
                // Filtramos el top 10
                topCommerces = model.TopCommercesModelList.OrderByDescending(c => c.TotalAmount).Take(10).ToList();
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                return false;
            }
        }

        #endregion

        #region TransactionsByDateAndStatus

        // Metodo de envío
        /// <summary>
        /// Obtiene el numero de transacciones (Declaraciones y Ordenes) por fecha y estatus
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <returns>True - Operacion Exitosa. False - Operacion Fallida.</returns>
        public bool GetCurrentMonthTransactionsByStatus(string rif, out DashboardViewModels.TransactionsByDateAndStatusModel model)
        {
            model = new DashboardViewModels.TransactionsByDateAndStatusModel();

            try
            {
                // Declaraciones Anuladas
                int AnnuledDeclarationsCount;
                decimal AnnuledDeclarationsTotalAmount;
                model.AnnuledDeclarations.Result = GeDeclarationsCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, DeclarationStatus.Annulled, out AnnuledDeclarationsCount, out AnnuledDeclarationsTotalAmount);
                model.AnnuledDeclarations.Count = AnnuledDeclarationsCount;
                model.AnnuledDeclarations.TotalAmount = AnnuledDeclarationsTotalAmount;
                // Declaraciones Conciliadas
                int ReconciledDeclarationsCount;
                decimal ReconciledDeclarationsTotalAmount;
                model.ReconciledDeclarations.Result = GeDeclarationsCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, DeclarationStatus.Reconciled, out ReconciledDeclarationsCount, out ReconciledDeclarationsTotalAmount);
                model.ReconciledDeclarations.Count = ReconciledDeclarationsCount;
                model.ReconciledDeclarations.TotalAmount = ReconciledDeclarationsTotalAmount;
                // Declaraciones Pendientes
                int PendingDeclarationsCount;
                decimal PendingDeclarationsTotalAmount;
                model.PendingDeclarations.Result = GeDeclarationsCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, DeclarationStatus.ReconciliationPending, out PendingDeclarationsCount, out PendingDeclarationsTotalAmount);
                model.PendingDeclarations.Count = PendingDeclarationsCount;
                model.PendingDeclarations.TotalAmount = PendingDeclarationsTotalAmount;
                // Ordenes de Compra Pendientes
                int AnnuledPurchaseOrdersCount;
                decimal AnnuledPurchaseOrdersTotalAmount;
                model.AnnuledPurchaseOrders.Result = GetPurchaseOrdersCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, PurchaseOrderStatus.Annulled, out AnnuledPurchaseOrdersCount, out AnnuledPurchaseOrdersTotalAmount);
                model.AnnuledPurchaseOrders.Count = AnnuledPurchaseOrdersCount;
                model.AnnuledPurchaseOrders.TotalAmount = AnnuledPurchaseOrdersTotalAmount;
                // Ordenes de Compra Declaradas
                int DeclaredPurchaseOrdersCount;
                decimal DeclaredPurchaseOrdersTotalAmount;
                model.DeclaredPurchaseOrders.Result = GetPurchaseOrdersCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, PurchaseOrderStatus.Declared, out DeclaredPurchaseOrdersCount, out DeclaredPurchaseOrdersTotalAmount);
                model.DeclaredPurchaseOrders.Count = DeclaredPurchaseOrdersCount;
                model.DeclaredPurchaseOrders.TotalAmount = DeclaredPurchaseOrdersTotalAmount;
                // Ordenes de Compra Pendientes
                int PendingPurchaseOrdersCount;
                decimal PendingPurchaseOrdersTotalAmount;
                model.PendingPurchaseOrders.Result = GetPurchaseOrdersCountAndTotalAmountByDateAndStatus(rif, DateTime.Now, DateType.Month, PurchaseOrderStatus.DeclarationPending, out PendingPurchaseOrdersCount, out PendingPurchaseOrdersTotalAmount);
                model.PendingPurchaseOrders.Count = PendingPurchaseOrdersCount;
                model.PendingPurchaseOrders.TotalAmount = PendingPurchaseOrdersTotalAmount;

                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                return false;
            }

        }

        /// <summary>
        /// Obtiene el numero de declaraciones por fecha y estado para un comercio específico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="date">Fecha de la declaracion (DateTime.Now)</param>
        /// <param name="dateType">Tipo de fecha entre mes, año, y tiempo real</param>
        /// <param name="status">Estado de la declaracion</param>
        /// <param name="result">Cuenta de las declaraciones</param>
        /// <param name="count">Monto total de las declaraciones</param>
        /// <returns>True - Operación exitosa. False - Operación fallida.</returns>
        public bool GeDeclarationsCountAndTotalAmountByDateAndStatus(string rif, DateTime date, DateType dateType, DeclarationStatus status, out int count, out decimal totalAmount)
        {
            try
            {
                // Obtenemos la lista de declaraciones
                var declarations = DBLL.GetDeclarationsByDateAndStatus(rif, date, dateType, status);
                // Obtenemos la cuenta de las declaraciones por estado y creadas el mes actual
                int declarationCount = declarations.Count();
                // Obtenemos el monto total de las declaraciones
                decimal declarationsTotalAmount = declarationCount != 0 ? declarations.Sum(d => d.Amount) : 0;
                // Retornamos la cuenta
                count = declarationCount;
                // Retornamos el monto total
                totalAmount = declarationsTotalAmount;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                totalAmount = 0;
                return false;
            }
        }

        /// <summary>
        /// Obtiene el numero de declaraciones por fecha y estado para un comercio específico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="date">Fecha de la declaracion (DateTime.Now)</param>
        /// <param name="dateType">Tipo de fecha entre mes, año, y tiempo real</param>
        /// <param name="status">Estado de la declaracion</param>
        /// <param name="result">Cuenta de las declaraciones</param>
        /// <param name="count">Monto total de las declaraciones</param>
        /// <returns>True - Operación exitosa. False - Operación fallida.</returns>
        public bool GeDeclarationsCountAndTotalAmountByDateAndStatus(DateTime date, DateType dateType, DeclarationStatus status, out int count, out decimal totalAmount)
        {
            try
            {
                // Obtenemos la lista de declaraciones
                var declarations = DBLL.GetDeclarationsByDateAndStatus(date, dateType, status);
                // Obtenemos la cuenta de las declaraciones por estado y creadas el mes actual
                int declarationCount = declarations.Count();
                // Obtenemos el monto total de las declaraciones
                decimal declarationsTotalAmount = declarationCount != 0 ? declarations.Sum(d => d.Amount) : 0;
                // Retornamos la cuenta
                count = declarationCount;
                // Retornamos el monto total
                totalAmount = declarationsTotalAmount;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                totalAmount = 0;
                return false;
            }
        }

        /// <summary>
        /// Obtiene el numero de ordenes de compra por fecha y estado para un comercio específico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="date">Fecha de la orden (DateTime.Now)</param>
        /// <param name="dateType">Tipo de fecha entre mes, año, y tiempo real</param>
        /// <param name="status">Estado de la orden</param>
        /// <param name="count">Cuenta de las órdenes</param>
        /// <param name="totalAmount">Monto total de las ordenes</param>
        /// <returns>True - Operación exitosa. False - Operación fallida.</returns>
        public bool GetPurchaseOrdersCountAndTotalAmountByDateAndStatus(string rif, DateTime date, DateType dateType, PurchaseOrderStatus status, out int count, out decimal totalAmount)
        {
            try
            {
                // Obtenemos la lista de ordenes
                var purchaseOrders = CPOBLL.GetPurchaseOrdersByDateAndStatus(rif, date, dateType, status);
                // Obtenemos la cuenta de las ordenes por estado y creadas el mes actual
                int purchaseOrdersCount = purchaseOrders.Count();
                // Obtenemos el monto total de las ordenes
                decimal purchaseOrdersTotalAmount = purchaseOrdersCount != 0 ? purchaseOrders.Sum(d => d.Amount) : 0;
                // Retornamos la cuenta
                count = purchaseOrdersCount;
                // Retornamos el monto total
                totalAmount = purchaseOrdersTotalAmount;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                totalAmount = 0;
                return false;
            }
        }

        /// <summary>
        /// Obtiene el numero de ordenes de compra por fecha y estado para un comercio específico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="date">Fecha de la orden (DateTime.Now)</param>
        /// <param name="dateType">Tipo de fecha entre mes, año, y tiempo real</param>
        /// <param name="status">Estado de la orden</param>
        /// <param name="count">Cuenta de las órdenes</param>
        /// <param name="totalAmount">Monto total de las ordenes</param>
        /// <returns>True - Operación exitosa. False - Operación fallida.</returns>
        public bool GetPurchaseOrdersCountAndTotalAmountByDateAndStatus(DateTime date, DateType dateType, PurchaseOrderStatus status, out int count, out decimal totalAmount)
        {
            try
            {
                // Obtenemos la lista de ordenes
                var purchaseOrders = CPOBLL.GetPurchaseOrdersByDateAndStatus(date, dateType, status);
                // Obtenemos la cuenta de las ordenes por estado y creadas el mes actual
                int purchaseOrdersCount = purchaseOrders.Count();
                // Obtenemos el monto total de las ordenes
                decimal purchaseOrdersTotalAmount = purchaseOrdersCount != 0 ? purchaseOrders.Sum(d => d.Amount) : 0;
                // Retornamos la cuenta
                count = purchaseOrdersCount;
                // Retornamos el monto total
                totalAmount = purchaseOrdersTotalAmount;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                totalAmount = 0;
                return false;
            }
        }

        #endregion

        #region TotalAmountByDateAndBank
        /// <summary>
        /// Devuelve la lista de monto total de las declaraciones conciliadas por banco
        /// </summary>
        /// <param name="bankAmountList">Lista de modelo de monto por banco</param>
        /// <returns>Lista de bancos y montos de declaraciones conciliadas</returns>
        public bool GetTotalAmountByDateAndBank(out List<DashboardViewModels.TotalBankAmountModel> bankAmountList)
        {
            var model = new DashboardViewModels();
            bankAmountList = new List<DashboardViewModels.TotalBankAmountModel>();
            try
            {
                // Obtenemos las declaraciones conciliadas del mes
                var declarations = DBLL.GetDeclarationsByDateAndStatus(DateTime.Now, DateType.Month, DeclarationStatus.Reconciled).ToList();
                // Agrupamos por bancos
                var banks = declarations.GroupBy(ud => ud.UBank.Name);
                // Seleccionamos la suma de los montos para cada banco
                var bankTotals = banks.Select(group => new
                {
                    Bank = group.Key,
                    TotalAmount = group.Sum(ud => ud.Amount)
                });
                // Construimos el modelo
                foreach (var bankTotal in bankTotals)
                {
                    // Añadimos los registros a la lista del modelo
                    bankAmountList.Add(new DashboardViewModels.TotalBankAmountModel
                    {
                        Bank = bankTotal.Bank,
                        TotalAmount = bankTotal.TotalAmount
                    });
                }
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                return false;
            }
        }

        #endregion

        #region ChartJS

        public ChartModels.MultiBarChartModel GetChartData()
        {
            List<DashboardViewModels.TotalBankCreditsModel> bankCredits;

            GetBankCreditsBySocialReason(out bankCredits);

            var issuingBankNames = bankCredits.Select(b => b.IssuingBank).ToArray();
            var creditsTotals = bankCredits.Select(b => b.TotalAmount).ToArray();
            var accountNames = bankCredits.Select(b => b.BankAccount).Distinct().ToArray();
            var bankList = issuingBankNames.Where(b => b != "No Identificado").Distinct();

            var chartModel = new ChartModels.MultiBarChartModel();

            chartModel.labels = bankList.ToArray(); ;
            Random rnd = new Random();

            foreach (var accountName in accountNames)
            {
                var dataset = new ChartModels.MultiBarDataset();


                var color = "#544c9f"; /*String.Format("#{0:X6}", rnd.Next(0x1000000));*/
                var borderColor = "#22bdd6";
                dataset.borderWidth = "3";
                dataset.backgroundColor = color;
                dataset.hoverBorderColor = borderColor;
                dataset.label = accountName.Trim(); ;
                var dataList = new List<decimal>();


                foreach (var bank in bankList)
                {
                    decimal totalAmount = 0;
                    totalAmount = bankCredits.Where(b => b.IssuingBank == bank &&
                                                         b.BankAccount == accountName).Select(b => b.TotalAmount).FirstOrDefault();
                    dataList.Add(totalAmount);
                }
                dataset.data = dataList.ToArray();

                chartModel.datasets.Add(dataset);
            }

            var returnData = new List<string>();

            ViewBag.issuingBankNames = issuingBankNames;
            ViewBag.creditsTotals = creditsTotals;

            return chartModel;

        }

        #endregion

        #endregion

        #region Commerce

        #region TotalAmountByDateAndBank
        /// <summary>
        /// Devuelve la lista de monto total de las declaraciones conciliadas por banco para un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="model">Modelo de monto por banco</param>
        /// <returns></returns>
        public bool GetTotalAmountByDateAndBank(string rif, out List<DashboardViewModels.TotalBankAmountModel> bankAmountList)
        {
            var model = new DashboardViewModels();
            bankAmountList = new List<DashboardViewModels.TotalBankAmountModel>();
            try
            {
                // Obtenemos las declaraciones conciliadas del mes
                var declarations = DBLL.GetDeclarationsByDateAndStatus(rif, DateTime.Now, DateType.Month, DeclarationStatus.Reconciled).ToList();
                // Agrupamos por bancos
                var banks = declarations.GroupBy(ud => ud.UBank.Name);
                // Seleccionamos la suma de los montos para cada banco
                var bankTotals = banks.Select(group => new
                {
                    Bank = group.Key,
                    TotalAmount = group.Sum(ud => ud.Amount)
                });
                // Construimos el modelo
                foreach (var bankTotal in bankTotals)
                {
                    // Añadimos los registros a la lista del modelo
                    bankAmountList.Add(new DashboardViewModels.TotalBankAmountModel
                    {
                        Bank = bankTotal.Bank,
                        TotalAmount = bankTotal.TotalAmount
                    });
                }
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                return false;
            }
        }

        #endregion

        #region Transactions

        #region UDeclaration

        #region Date
        public bool GetRealTimeDeclarationsCount(string rif, out int result)
        {
            try
            {
                // Obtenemos la cuenta de las declaraciones en tiempo real
                realTimeDeclarationsCount = DBLL.GetDeclarationsByDate(rif, lastDashboardUpdate, DateType.RealTime).Count();
                // Actualizamos el tiempo de la ultima consulta
                BackEndGlobals.LastDashboardUpdate = DateTime.Now;
                // Retornamos la cuenta
                result = realTimeDeclarationsCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                result = 0;
                return false;
            }
        }

        /// <summary>
        /// Retorna el numero de declaraciones pendientes para un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="count">Numero de declaraciones pendientes</param>
        /// <returns>True - Operacion exitosa. False - Operacion fallida.</returns>
        public bool GetCurrentDayPendingDeclarationsCount(string rif, out int count)
        {
            try
            {
                // Obtenemos la cuenta de las declaraciones creadas el mes actual
                int declarationCount = DBLL.GetDeclarationsByDateAndStatus(rif, DateTime.Today, DateType.Day, DeclarationStatus.ReconciliationPending).Count();
                // Retornamos la cuenta
                count = declarationCount;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                return false;
            }
        }

        /// <summary>
        /// Retorna el monto total y numero de declaraciones conciliadas del dia
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="count">Numero de declaraciones conciliadas</param>
        /// <param name="totalAmount">Monto total de declaraciones conciliadas</param>
        /// <returns></returns>
        public bool GetCurrentDayDeclarationsCountAndTotalAmount(string rif, out int count, out decimal totalAmount)
        {
            try
            {
                // Obtenemos la lista de declaraciones
                var declarations = DBLL.GetDeclarationsByDateAndStatus(rif, DateTime.Now, DateType.Day, DeclarationStatus.Reconciled);
                // Obtenemos la cuenta de las declaraciones por estado y creadas el dia actual
                int declarationCount = declarations.Count();
                // Obtenemos el monto total de las declaraciones
                decimal declarationsTotalAmount = declarationCount != 0 ? declarations.Sum(d => d.Amount) : 0;
                // Retornamos la cuenta
                count = declarationCount;
                // Retornamos el monto total
                totalAmount = declarationsTotalAmount;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                totalAmount = 0;
                return false;
            }
        }

        /// <summary>
        /// Retorna el monto total y el numero de declaraciones conciliadas del mes para un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="count">Numero de declaraciones conciliadas</param>
        /// <param name="totalAmount">Monto total de declaraciones conciliadas</param>
        /// <returns>True - Operacion exitosa. False - Operacion fallida.</returns>
        public bool GetCurrentMonthDeclarationsCountAndTotalAmount(string rif, out int count, out decimal totalAmount)
        {
            try
            {
                // Obtenemos la lista de declaraciones
                var declarations = DBLL.GetDeclarationsByDateAndStatus(rif, DateTime.Now, DateType.Month, DeclarationStatus.Reconciled);
                // Obtenemos la cuenta de las declaraciones por estado y creadas el mes actual
                int declarationCount = declarations.Count();
                // Obtenemos el monto total de las declaraciones
                decimal declarationsTotalAmount = declarationCount != 0 ? declarations.Sum(d => d.Amount) : 0;
                // Retornamos la cuenta
                count = declarationCount;
                // Retornamos el monto total
                totalAmount = declarationsTotalAmount;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                totalAmount = 0;
                return false;
            }
        }

        /// <summary>
        /// Retorna el monto total y numero de todas las declaraciones conciliadas de un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="count">Numero de declaraciones conciliadas</param>
        /// <param name="totalAmount">Monto total de todas las declaraciones conciliadas</param>
        /// <returns></returns>
        public bool GetReconciledDeclarationsCountAndTotalAmount(string rif, out int count, out decimal totalAmount)
        {
            try
            {
                // Obtenemos la lista de declaraciones
                var declarations = DBLL.GetDeclarationsByStatus(rif, DeclarationStatus.Reconciled);
                // Obtenemos la cuenta de las declaraciones por estado y creadas el mes actual
                int declarationCount = declarations.Count();
                // Obtenemos el monto total de las declaraciones
                decimal declarationsTotalAmount = declarationCount != 0 ? declarations.Sum(d => d.Amount) : 0;
                // Retornamos la cuenta
                count = declarationCount;
                // Retornamos el monto total
                totalAmount = declarationsTotalAmount;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                totalAmount = 0;
                return false;
            }
        }

        #endregion

        #endregion

        #region CPurchaseOrder

        #endregion

        #endregion

        #region DaySummary

        public bool GetDaySummary(string rif, out DashboardViewModels.DaySummaryModel daySummaryModel)
        {
            daySummaryModel = new DashboardViewModels.DaySummaryModel();

            try
            {
                // Declaraciones Anuladas
                int AnnuledDeclarationsCount;
                int AnnuledDeclarationsPercentage;
                daySummaryModel.AnnuledDeclarations.Result = GeDeclarationsCountAndPercentageByDateAndStatus(rif, DateTime.Now, DateType.Day, DeclarationStatus.Annulled, out AnnuledDeclarationsCount, out AnnuledDeclarationsPercentage);
                daySummaryModel.AnnuledDeclarations.Count = AnnuledDeclarationsCount;
                daySummaryModel.AnnuledDeclarations.Percentage = AnnuledDeclarationsPercentage;
                // Declaraciones Conciliadas
                int ReconciledDeclarationsCount;
                int ReconciledDeclarationsPercentage;
                daySummaryModel.ReconciledDeclarations.Result = GeDeclarationsCountAndPercentageByDateAndStatus(rif, DateTime.Now, DateType.Day, DeclarationStatus.Reconciled, out ReconciledDeclarationsCount, out ReconciledDeclarationsPercentage);
                daySummaryModel.ReconciledDeclarations.Count = ReconciledDeclarationsCount;
                daySummaryModel.ReconciledDeclarations.Percentage = ReconciledDeclarationsPercentage;
                // Declaraciones Pendientes
                int PendingDeclarationsCount;
                int PendingDeclarationsPercentage;
                daySummaryModel.PendingDeclarations.Result = GeDeclarationsCountAndPercentageByDateAndStatus(rif, DateTime.Now, DateType.Day, DeclarationStatus.ReconciliationPending, out PendingDeclarationsCount, out PendingDeclarationsPercentage);
                daySummaryModel.PendingDeclarations.Count = PendingDeclarationsCount;
                daySummaryModel.PendingDeclarations.Percentage = PendingDeclarationsPercentage;
                // Ordenes de Compra Pendientes
                int AnnuledPurchaseOrdersCount;
                int AnnuledPurchaseOrdersPercentage;
                daySummaryModel.AnnuledPurchaseOrders.Result = GetPurchaseOrdersCountAndPercentageByDateAndStatus(rif, DateTime.Now, DateType.Day, PurchaseOrderStatus.Annulled, out AnnuledPurchaseOrdersCount, out AnnuledPurchaseOrdersPercentage);
                daySummaryModel.AnnuledPurchaseOrders.Count = AnnuledPurchaseOrdersCount;
                daySummaryModel.AnnuledPurchaseOrders.Percentage = AnnuledPurchaseOrdersPercentage;
                // Ordenes de Compra Declaradas
                int DeclaredPurchaseOrdersCount;
                int DeclaredPurchaseOrdersPercentage;
                daySummaryModel.DeclaredPurchaseOrders.Result = GetPurchaseOrdersCountAndPercentageByDateAndStatus(rif, DateTime.Now, DateType.Day, PurchaseOrderStatus.Declared, out DeclaredPurchaseOrdersCount, out DeclaredPurchaseOrdersPercentage);
                daySummaryModel.DeclaredPurchaseOrders.Count = DeclaredPurchaseOrdersCount;
                daySummaryModel.DeclaredPurchaseOrders.Percentage = DeclaredPurchaseOrdersPercentage;
                // Ordenes de Compra Pendientes
                int PendingPurchaseOrdersCount;
                int PendingPurchaseOrdersPercentage;
                daySummaryModel.PendingPurchaseOrders.Result = GetPurchaseOrdersCountAndPercentageByDateAndStatus(rif, DateTime.Now, DateType.Day, PurchaseOrderStatus.DeclarationPending, out PendingPurchaseOrdersCount, out PendingPurchaseOrdersPercentage);
                daySummaryModel.PendingPurchaseOrders.Count = PendingPurchaseOrdersCount;
                daySummaryModel.PendingPurchaseOrders.Percentage = PendingPurchaseOrdersPercentage;
                // Success
                return true;
            }
            catch (Exception)
            {
                // Error
                return false;
            }
        }

        /// <summary>
        /// Obtiene el numero de declaraciones y porcentaje por fecha y estado para un comercio específico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="date">Fecha de la declaracion (DateTime.Now)</param>
        /// <param name="dateType">Tipo de fecha entre mes, año, y tiempo real</param>
        /// <param name="status">Estado de la declaracion</param>
        /// <param name="count">Cuenta de las declaraciones</param>
        /// <param name="percentage">Porcentaje de las declaraciones</param>
        /// <returns>True - Operación exitosa. False - Operación fallida.</returns>
        public bool GeDeclarationsCountAndPercentageByDateAndStatus(string rif, DateTime date, DateType dateType, DeclarationStatus status, out int count, out int percentage)
        {
            try
            {
                // Obtenemos la lista de declaraciones
                var declarations = DBLL.GetDeclarationsByDateAndStatus(rif, date, dateType, status);
                var allDeclarations = DBLL.GetDeclarationsByDate(rif, date, dateType);
                // Obtenemos la cuenta de las declaraciones por estado y creadas el actual
                int declarationCount = declarations.Count();
                int allDeclarationsCount = allDeclarations.Count();
                // Obtenemos el porcentaje de las declaraciones en base al total
                int declarationsPercentage = allDeclarationsCount != 0 ? (int)Math.Round((double)(100 * declarationCount) / allDeclarationsCount) : 0;
                // Retornamos la cuenta
                count = declarationCount;
                // Retornamos el monto total
                percentage = declarationsPercentage;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                percentage = 0;
                return false;
            }
        }

        /// <summary>
        /// Obtiene el numero de órdenes y porcentaje por fecha y estado para un comercio específico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="date">Fecha de la orden (DateTime.Now)</param>
        /// <param name="dateType">Tipo de fecha entre mes, año, y tiempo real</param>
        /// <param name="status">Estado de la orden</param>
        /// <param name="count">Cuenta de las órdenes</param>
        /// <param name="percentage">Porcentaje de las órdenes</param>
        /// <returns>True - Operación exitosa. False - Operación fallida.</returns>
        public bool GetPurchaseOrdersCountAndPercentageByDateAndStatus(string rif, DateTime date, DateType dateType, PurchaseOrderStatus status, out int count, out int percentage)
        {
            try
            {
                // Obtenemos la lista de órdenes
                var purchaseOrders = CPOBLL.GetPurchaseOrdersByDateAndStatus(rif, date, dateType, status);
                var allPurchaseOrders = CPOBLL.GetPurchaseOrdersByDate(rif, date, dateType);
                // Obtenemos la cuenta de las órdenes por estado y creadas el actual
                int purchaseOrdersCount = purchaseOrders.Count();
                int allPurchaseOrdersCount = allPurchaseOrders.Count();
                // Obtenemos el porcentaje de las órdenes en base al total
                int purchaseOrdersPercentage = allPurchaseOrdersCount != 0 ? (int)Math.Round((double)(100 * purchaseOrdersCount) / allPurchaseOrdersCount) : 0;
                // Retornamos la cuenta
                count = purchaseOrdersCount;
                // Retornamos el monto total
                percentage = purchaseOrdersPercentage;
                //Success
                return true;
            }
            catch (Exception)
            {
                // Error
                count = 0;
                percentage = 0;
                return false;
            }
        }

        #endregion

        #region ChartJS

        public ChartModels.TimeScaleChartModel GetTimeScaleComboChartData(string rif, DateTime startDate, DateTime endDate, DeclarationStatus declarationStatus, PurchaseOrderStatus purchaseOrderStatus)
        {
            // Declaramos variables
            var chartModel = new ChartModels.TimeScaleChartModel();
            var declarationsDataList = new List<decimal>();
            var purchaseOrdersDataList = new List<decimal>();
            var labels = new List<string>();
            List<UDeclaration> declarations;
            List<CPurchaseOrder> reconciledPurchaseOrders;
            List<CPurchaseOrder> declaredPurchaseOrders;
            DateTime currentDate;
            int totalDays = (endDate - startDate).Days;
            currentDate = startDate;

            // Obtenemos registros desde la base de datos
            declarations = DBLL.GetDeclarationsByDateRangeAndStatus(rif, startDate, endDate, declarationStatus).ToList();
            reconciledPurchaseOrders = CPOBLL.GetPurchaseOrdersByDateRangeAndStatus(rif, startDate, endDate, PurchaseOrderStatus.DeclaredReconciled).ToList();
            declaredPurchaseOrders = CPOBLL.GetPurchaseOrdersByDateRangeAndStatus(rif, startDate, endDate, PurchaseOrderStatus.Declared).ToList();

            // Recorremos el rango de fechas
            for (int i = 0; i <= totalDays; i++)
            {
                // Obtenemos la lista de registros filtrados
                var currenDeclarations = declarations.Where(ud => ud.StatusChangeDate.Day == currentDate.Day && ud.StatusChangeDate.Month == currentDate.Month && ud.StatusChangeDate.Year == currentDate.Year).ToList();
                var currentReconciledPurchaseOrders = reconciledPurchaseOrders.Where(po => po.StatusChangeDate.Day == currentDate.Day && po.StatusChangeDate.Month == currentDate.Month && po.StatusChangeDate.Year == currentDate.Year).ToList();
                var currentDeclaredPurchaseOrders = declaredPurchaseOrders.Where(po => po.StatusChangeDate.Day == currentDate.Day && po.StatusChangeDate.Month == currentDate.Month && po.StatusChangeDate.Year == currentDate.Year).ToList();
                // Obtenemos la cuenta de las declaraciones por estado y creadas el dia actual
                int currenDeclarationCount = currenDeclarations.Count();
                int currentReconciledPurchaseOrdersCount = currentReconciledPurchaseOrders.Count();
                int currentDeclaredPurchaseOrdersCount = currentDeclaredPurchaseOrders.Count();
                // Obtenemos el monto total
                decimal currenDeclarationsTotalAmount = currenDeclarationCount != 0 ? currenDeclarations.Sum(d => d.Amount) : 0;
                decimal currentReconciledPurchaseOrdersTotalAmount = currentReconciledPurchaseOrdersCount != 0 ? currentReconciledPurchaseOrders.Sum(po => po.Amount) : 0;
                decimal currentDeclaredPurchaseOrdersTotalAmount = currentDeclaredPurchaseOrdersCount != 0 ? currentDeclaredPurchaseOrders.Sum(po => po.Amount) : 0;
                // Total órdenes
                decimal currentPurchaseOrdersTotalAmount = currentReconciledPurchaseOrdersTotalAmount + currentDeclaredPurchaseOrdersTotalAmount;
                // Armamos el modelo de datos
                declarationsDataList.Add(currenDeclarationsTotalAmount);
                purchaseOrdersDataList.Add(currentPurchaseOrdersTotalAmount);
                // Añadimos el label
                labels.Add(currentDate.ToString("yyyy-MM-dd"));
                // Aumentamos los dias
                currentDate = currentDate.AddDays(1);
            }
            // Armamos el modelo del grafico
            chartModel.declarationsDataset.data = declarationsDataList.ToArray();
            chartModel.purchaseOrdersDataset.data = purchaseOrdersDataList.ToArray();
            chartModel.labels = labels.ToArray();

            return chartModel;

        }


        #endregion

        #endregion

        public JsonResult _GuardarConfiguracion(string totalbancosBS, string montodolaresBS, string totaldolares, string pendienteporcobrar, string totalactivos, string totalbancospBS, string montodolarespBS, string totaldolaresp, string totalpasivos, string cantidadaccion, string valoraccion, string nuevovalor, DateTime? fechaoperacion, string tasadolares, string dolarestransito, string totalgastos, string totalcobrodiario, string totalgananciadiaria, string totalcobrodiariobruto, string TasaUtilizada, string AcumuladoTasa, string pass)
        {
            if (pass != "10563")
            {
                return Json(new
                {
                    success = false,
                    message = "PIN Incorrecto!"
                }, JsonRequestBehavior.DenyGet);
            }
            else
            {

                List<AE_EstadoCuenta> EstadoCuenta = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.AE_Avance.IdEstatus == 1 && u.FechaOperacion.Day == DateTime.Now.AddDays(-0).Day && u.FechaOperacion.Month == DateTime.Now.Month && u.FechaOperacion.Year == DateTime.Now.Year && !u.Abono && !u.RecibidoEnDolares).ToList();

                foreach (var item in EstadoCuenta)
                {
                    item.Tasa = decimal.Parse(TasaUtilizada.Replace('.', ','));
                    item.Monto = item.MontoBs.Value / decimal.Parse(TasaUtilizada.Replace('.', ','));
                    AE_EstadoCuentaREPO.SaveChanges();

                }

                List<AE_ValorAccionTR> ListValorAccionTR = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.FechaCreacionRegistro).Take(5).ToList();
                if (ListValorAccionTR.Count > 0)
                {
                    AE_ValorAccionTR Ultimo = ListValorAccionTR.FirstOrDefault();
                    AE_ValorAccionTR ValorAccionTR = new AE_ValorAccionTR();
                    ValorAccionTR.FechaCreacionRegistro = DateTime.Now.AddDays(-0);
                    ValorAccionTR.FechaOperacion = DateTime.Now.AddDays(-0);
                    ValorAccionTR.FechaUltimaActualizacion = DateTime.Now.AddDays(-0);

                    ValorAccionTR.GastoReportado = decimal.Parse(totalgastos.Replace('.', ',')) * -1;
                    ValorAccionTR.CapitalInicial = Ultimo.NuevoCapital;
                    ValorAccionTR.CapitalPorCobrar = Ultimo.CapitalPorCobrar - (decimal.Parse(totalcobrodiario.Replace('.', ',')) - decimal.Parse(totalgananciadiaria.Replace('.', ',')));
                    ValorAccionTR.AbonoCapital = decimal.Parse(totalcobrodiario.Replace('.', ',')) - decimal.Parse(totalgananciadiaria.Replace('.', ','));
                    ValorAccionTR.UtilidadReportada = decimal.Parse(totalgananciadiaria.Replace('.', ','));
                    //ValorAccionTR.SaldoUSD = Ultimo.SaldoUSD + decimal.Parse(totalcobrodiario.Replace('.', ','));
                    ValorAccionTR.SaldoUSD = decimal.Parse(totaldolares.Replace('.', ',')) + (decimal.Parse(totalbancosBS.Replace('.', ',')) / decimal.Parse(tasadolares.Replace('.', ',')));
                    ValorAccionTR.TotalCobroDiario = decimal.Parse(totalcobrodiariobruto.Replace('.', ','));
                    //ValorAccionTR.NuevoCapital = Ultimo.NuevoCapital + decimal.Parse(totalgananciadiaria.Replace('.', ','));

                    ValorAccionTR.UsdTransito = decimal.Parse(dolarestransito.Replace('.', ','));
                    ValorAccionTR.UsdVenezuela = decimal.Parse(totalbancosBS.Replace('.', ',')) / decimal.Parse(tasadolares.Replace('.', ','));
                    ValorAccionTR.CuentaVenezuela = decimal.Parse(totalbancosBS.Replace('.', ','));
                    ValorAccionTR.NuevoCapital = ValorAccionTR.CapitalPorCobrar + ValorAccionTR.SaldoUSD;

                    ValorAccionTR.PagoCapitalInversionista = 0;
                    ValorAccionTR.PagoUtilidadMesInversionista = 0;
                    ValorAccionTR.TotalAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.FechaRegistro).FirstOrDefault().TotalAcciones;
                    ValorAccionTR.PagoUtilidadAdministrador = 0;
                    ValorAccionTR.ValorAccion = ValorAccionTR.NuevoCapital / ValorAccionTR.TotalAcciones;
                    ValorAccionTR.Tasa = decimal.Parse(TasaUtilizada.Replace('.', ','));
                    ValorAccionTR.TasaDiferencia = decimal.Parse(AcumuladoTasa.Replace('.', ','));

                    AE_ValorAccionTRREPO.AddEntity(ValorAccionTR);
                    AE_ValorAccionTRREPO.SaveChanges();
                }
                else
                {

                    AE_ValorAccionTR ValorAccionTR = new AE_ValorAccionTR();
                    ValorAccionTR.FechaCreacionRegistro = DateTime.Now.AddDays(-0);
                    ValorAccionTR.FechaOperacion = DateTime.Now.AddDays(-0);
                    ValorAccionTR.FechaUltimaActualizacion = DateTime.Now.AddDays(-0);
                    //ValorAccionTR.UtilidadReportada = decimal.Parse(totalgananciadiaria.Replace('.', ','));
                    ValorAccionTR.GastoReportado = 0;
                    decimal Montoencuenta = decimal.Parse(totaldolares.Replace('.', ','));
                    decimal Cobros = 0;
                    decimal PendientePorcobrar = 0;

                    foreach (var item in AE_AvanceREPO.GetAllRecords().Where(u => u.IdEstatus == 1))
                    {
                        decimal porcen = (item.Reembolso - item.Avance) * 100 / item.Avance;
                        decimal montocobrado = item.AE_EstadoCuentas.Where(u => u.Abono == false).Sum(u => u.Monto);
                        //montocobrado = (montocobrado - (montocobrado * item.GastoBanco / 100));
                        Cobros = Cobros + montocobrado;
                        decimal _PendientePorcobrar = item.Reembolso - montocobrado;
                        _PendientePorcobrar = _PendientePorcobrar - (_PendientePorcobrar * porcen / 100);
                        PendientePorcobrar = PendientePorcobrar + _PendientePorcobrar;
                    }
                    ValorAccionTR.NuevoCapital = PendientePorcobrar + Montoencuenta;
                    ValorAccionTR.CapitalInicial = 0;
                    ValorAccionTR.PagoCapitalInversionista = 0;
                    ValorAccionTR.PagoUtilidadMesInversionista = 0;
                    ValorAccionTR.TotalAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.FechaRegistro).FirstOrDefault().TotalAcciones;
                    ValorAccionTR.PagoUtilidadAdministrador = 0;
                    ValorAccionTR.ValorAccion = (PendientePorcobrar + Montoencuenta) / ValorAccionTR.TotalAcciones;
                    ValorAccionTR.UtilidadReportada = decimal.Parse(totalgananciadiaria.Replace('.', ','));
                    AE_ValorAccionTRREPO.AddEntity(ValorAccionTR);
                    AE_ValorAccionTRREPO.SaveChanges();
                }

                try
                {
                    AE_BalanceDiario NuevoBalance = new AE_BalanceDiario();
                    NuevoBalance.FechaCreacionRegistro = DateTime.Now.AddDays(-0);
                    NuevoBalance.FechaUltimaActualizacion = DateTime.Now.AddDays(-0);
                    if (fechaoperacion != null)
                        NuevoBalance.FechaOperaicon = fechaoperacion.Value;
                    else
                        NuevoBalance.FechaOperaicon = DateTime.Now.AddDays(-0);

                    //ACTIVOS
                    NuevoBalance.TotalCuentaBs = decimal.Parse(totalbancosBS.Replace('.', ','));
                    NuevoBalance.TotalCuentaEstimadoUsd = decimal.Parse(montodolaresBS.Replace('.', ','));
                    NuevoBalance.TotalCuentaUsd = decimal.Parse(totaldolares.Replace('.', ','));
                    NuevoBalance.TotalPorCobrarEstimadoUsd = decimal.Parse(pendienteporcobrar.Replace('.', ','));
                    NuevoBalance.TotalPorCobrarBs = decimal.Parse(pendienteporcobrar.Replace('.', ',')) * decimal.Parse(tasadolares.Replace('.', ','));
                    NuevoBalance.TotalActivos = decimal.Parse(totalactivos.Replace('.', ','));
                    //PASIVOS
                    NuevoBalance.TotalPasivoBs = decimal.Parse(totalbancospBS.Replace('.', ','));
                    NuevoBalance.TotalPasivosEstimadoUsd = decimal.Parse(montodolarespBS.Replace('.', ','));
                    NuevoBalance.TotalPasivoUsd = decimal.Parse(totaldolaresp.Replace('.', ','));
                    NuevoBalance.TotalPasivos = decimal.Parse(totalpasivos.Replace('.', ','));
                    //CAPITAL
                    NuevoBalance.TotalCapital = decimal.Parse(totalactivos.Replace('.', ',')) - decimal.Parse(totalpasivos.Replace('.', ','));
                    NuevoBalance.TasaUtilizada = decimal.Parse(tasadolares.Replace('.', ','));
                    //TRANSITO
                    NuevoBalance.TotalExcluirUsd = decimal.Parse(dolarestransito.Replace('.', ','));
                    NuevoBalance.TotalExcluirBs = decimal.Parse(totalgastos.Replace('.', ','));
                    NuevoBalance.TotalCobroDiario = decimal.Parse(totalcobrodiario.Replace('.', ','));
                    NuevoBalance.TotalGananciaDiaria = decimal.Parse(totalgananciadiaria.Replace('.', ','));
                    AE_BalanceDiarioREPO.AddEntity(NuevoBalance);
                    AE_BalanceDiarioREPO.SaveChanges();

                    var balanceacciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.FechaOperacion).FirstOrDefault();
                    var valoracciones = AE_ValorAccionREPO.GetAllRecords().OrderByDescending(u => u.Id).Take(10).ToList();
                    if (valoracciones != null && valoracciones.Count() > 0)
                    {
                        AE_ValorAccion item = valoracciones.FirstOrDefault();

                        AE_ValorAccion NuevoItem = new AE_ValorAccion();
                        NuevoItem.FechaCreacionRegistro = DateTime.Now.AddDays(-0);
                        NuevoItem.FechaOperacion = DateTime.Now.AddDays(-0);
                        NuevoItem.FechaUltimaActualizacion = DateTime.Now.AddDays(-0);
                        NuevoItem.TotalAcciones = balanceacciones.TotalAcciones;
                        NuevoItem.TotalCapitalusd = NuevoBalance.TotalCapital;
                        NuevoItem.ValorAccion = NuevoBalance.TotalCapital / balanceacciones.TotalAcciones;
                        NuevoItem.IdBalanceDiario = NuevoBalance.Id;
                        NuevoItem.IdBalanceAcciones = item.IdBalanceAcciones;
                        AE_ValorAccionREPO.AddEntity(NuevoItem);
                        AE_ValorAccionREPO.SaveChanges();

                    }

                    //decimal nuevaacciones = 0;
                    //AE_BalanceAccione elemento = new AE_BalanceAccione();
                    //var balanceAcciones = AE_BalanceAccioneREPO.GetAllRecords().ToList();
                    //if (balanceAcciones.Count > 0)
                    //{
                    //    var _valoracciones = AE_ValorAccionREPO.GetAllRecords().OrderByDescending(u => u.Id).Take(10).ToList();
                    //    var item = balanceAcciones.OrderByDescending(u => u.Id).FirstOrDefault();
                    //    nuevaacciones = 0;
                    //    elemento.TotaCapital = NuevoBalance.TotalCapital;
                    //    elemento.AccionesRetiradas = 0;
                    //    elemento.CapitalEntrada = 0;
                    //    elemento.CapitalSalida = 0;
                    //    elemento.FactorInicial = item.FactorInicial;
                    //    elemento.FechaOperacion = DateTime.Now;
                    //    elemento.FechaRegistro = DateTime.Now;
                    //    elemento.FechaUltimaActualizacion = DateTime.Now;
                    //    elemento.TotalAccionesExistentes = item.TotalAccionesExistentes;
                    //    elemento.AccionesEntrantes = nuevaacciones;
                    //    elemento.TotalAcciones = item.TotalAcciones + nuevaacciones;
                    //    elemento.IdInversionista = Guid.Parse(Inversionista);
                    //    elemento.ValorAcciones = item.ValorAcciones;
                    //    AE_BalanceAccioneREPO.AddEntity(elemento);
                    //    AE_BalanceAccioneREPO.SaveChanges();

                    //    Operacion.RepresentacionFondo = nuevaacciones;
                    //    AE_OperacionREPO.SaveChanges();

                    //}
                    try
                    {
                        List<AE_Dolar> Dolar = AE_DolarREPO.GetAllRecords().Where(u => u.FechaValor.Day == DateTime.Now.AddDays(-0).Day && u.FechaValor.Month == DateTime.Now.Month && u.FechaValor.Year == DateTime.Now.Year).ToList();
                        if (Dolar != null && Dolar.Count > 0)
                        {
                            AE_Dolar tt = Dolar.FirstOrDefault();
                            tt.TasaUtilizada = decimal.Parse(TasaUtilizada.Replace('.', ','));
                            AE_DolarREPO.SaveChanges();
                        }
                    }
                    catch { }



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
                    message = "Balance agregado de forma correcta!"
                }, JsonRequestBehavior.DenyGet);
            }

        }

        public JsonResult _GuardarDolar(decimal tasa, DateTime fecha)
        {
            try
            {
                List<AE_Dolar> dolar = AE_DolaresREPO.GetAllRecords().Where(u => u.FechaValor.Day == fecha.Day && u.FechaValor.Month == fecha.Month && u.FechaValor.Year == fecha.Year).ToList();
                if (dolar.Count > 0)
                {
                    var ultimodolar = AE_DolaresREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).Skip(1).FirstOrDefault();
                    if (ultimodolar != null && ultimodolar.Id > 0)
                    {
                        decimal a = 1 / tasa;
                        decimal b = 1 / ultimodolar.Tasa;
                        decimal c = (a / 1) / (b / 1);
                        decimal d = (c - 1) * 100;

                        dolar.FirstOrDefault().PromedioAcumulado = d;
                        dolar.FirstOrDefault().Tasa = tasa;
                        AE_DolaresREPO.SaveChanges();
                    }
                    else
                    {
                        dolar.FirstOrDefault().Tasa = tasa;
                        AE_DolaresREPO.SaveChanges();
                    }
                }
                else
                {
                    var ultimodolar = AE_DolaresREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).FirstOrDefault();
                    if (ultimodolar != null && ultimodolar.Id > 0)
                    {
                        decimal a = 1 / tasa;
                        decimal b = 1 / ultimodolar.Tasa;
                        decimal c = (a / 1) / (b / 1);
                        decimal d = (c - 1) * 100;

                        AE_Dolar item = new AE_Dolar();
                        item.Tasa = tasa;
                        item.FechaValor = fecha;
                        item.IdUsuario = "pendiente";
                        item.PromedioAcumulado = d;
                        item.Date = DateTime.Now;

                        AE_DolaresREPO.AddEntity(item);
                        AE_DolaresREPO.SaveChanges();
                    }
                    else
                    {
                        AE_Dolar item = new AE_Dolar();
                        item.Tasa = tasa;
                        item.FechaValor = fecha;
                        item.IdUsuario = "pendiente";
                        item.PromedioAcumulado = 0;
                        item.Date = DateTime.Now;

                        AE_DolaresREPO.AddEntity(item);
                        AE_DolaresREPO.SaveChanges();
                    }

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
                message = "Movimiento agregado de forma correcta!"
            }, JsonRequestBehavior.DenyGet);

        }

        public JsonResult _GuardarCambio(decimal cambiotasa, decimal cambiobs, DateTime cambiofecha, string cambiodescripcion)
        {
            try
            {
                List<AE_Dolar> dolar = AE_DolaresREPO.GetAllRecords().Where(u => u.FechaValor.Day == cambiofecha.Day && u.FechaValor.Month == cambiofecha.Month && u.FechaValor.Year == cambiofecha.Year).ToList();
                if (dolar.Count > 0)
                {
                    AE_CambioDiario Cambio = new AE_CambioDiario();
                    Cambio.FechaRegistro = cambiofecha;
                    Cambio.Descripcion = cambiodescripcion;
                    Cambio.MontoBs = cambiobs;
                    Cambio.TasaUtilizada = cambiotasa;
                    Cambio.TasaPromedio = 0;
                    Cambio.IdEstatus = 2;
                    AE_CambioDiarioREPO.AddEntity(Cambio);
                    AE_CambioDiarioREPO.SaveChanges();

                    List<AE_CambioDiario> ListaCambio = AE_CambioDiarioREPO.GetAllRecords().Where(u => u.FechaRegistro.Day == cambiofecha.Day && u.FechaRegistro.Month == cambiofecha.Month && u.FechaRegistro.Year == cambiofecha.Year).ToList();
                    decimal totalbs = 0;
                    decimal totalusd = 0;
                    foreach (var item in ListaCambio)
                    {
                        totalbs = totalbs + item.MontoBs;
                        totalusd = totalusd + (item.MontoBs / item.TasaUtilizada);
                    }
                    decimal tasapromedio = Math.Round(totalbs, 2) / Math.Round(totalusd, 2);
                    var ultimodolar = AE_DolaresREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).FirstOrDefault();
                    ultimodolar.Tasa = tasapromedio;
                    AE_DolaresREPO.SaveChanges();
                    //if (ultimodolar != null && ultimodolar.Id > 0)
                    //{
                    //    decimal a = 1 / tasa;
                    //    decimal b = 1 / ultimodolar.Tasa;
                    //    decimal c = (a / 1) / (b / 1);
                    //    decimal d = (c - 1) * 100;

                    //    dolar.FirstOrDefault().PromedioAcumulado = d;
                    //    dolar.FirstOrDefault().Tasa = tasa;
                    //    AE_DolaresREPO.SaveChanges();
                    //}
                    //else
                    //{
                    //    dolar.FirstOrDefault().Tasa = tasa;
                    //    AE_DolaresREPO.SaveChanges();
                    //}
                }
                else
                {

                    AE_CambioDiario Cambio = new AE_CambioDiario();
                    Cambio.FechaRegistro = cambiofecha;
                    Cambio.Descripcion = cambiodescripcion;
                    Cambio.MontoBs = cambiobs;
                    Cambio.TasaUtilizada = cambiotasa;
                    Cambio.TasaPromedio = 0;
                    Cambio.IdEstatus = 2;
                    AE_CambioDiarioREPO.AddEntity(Cambio);
                    AE_CambioDiarioREPO.SaveChanges();

                    List<AE_CambioDiario> ListaCambio = AE_CambioDiarioREPO.GetAllRecords().Where(u => u.FechaRegistro.Day == cambiofecha.Day && u.FechaRegistro.Month == cambiofecha.Month && u.FechaRegistro.Year == cambiofecha.Year).ToList();
                    decimal totalbs = 0;
                    decimal totalusd = 0;
                    foreach (var _item in ListaCambio)
                    {
                        totalbs = totalbs + _item.MontoBs;
                        totalusd = totalusd + (_item.MontoBs / _item.TasaUtilizada);
                    }
                    decimal tasapromedio = Math.Round(totalbs, 2) / Math.Round(totalusd, 2);
                    //var ultimodolar = AE_DolaresREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).FirstOrDefault();
                    //if (ultimodolar != null && ultimodolar.Id > 0)
                    //{
                    //    decimal a = 1 / tasa;
                    //    decimal b = 1 / ultimodolar.Tasa;
                    //    decimal c = (a / 1) / (b / 1);
                    //    decimal d = (c - 1) * 100;

                    //    AE_Dolar item = new AE_Dolar();
                    //    item.Tasa = tasa;
                    //    item.FechaValor = fecha;
                    //    item.IdUsuario = "pendiente";
                    //    item.PromedioAcumulado = d;
                    //    item.Date = DateTime.Now;

                    //    AE_DolaresREPO.AddEntity(item);
                    //    AE_DolaresREPO.SaveChanges();
                    //}
                    //else
                    //{
                    AE_Dolar item = new AE_Dolar();
                    item.Tasa = tasapromedio;
                    item.FechaValor = cambiofecha;
                    item.IdUsuario = "pendiente";
                    item.PromedioAcumulado = 0;
                    item.Date = DateTime.Now;

                    AE_DolaresREPO.AddEntity(item);
                    AE_DolaresREPO.SaveChanges();
                    //}

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
                message = "Movimiento agregado de forma correcta!"
            }, JsonRequestBehavior.DenyGet);

        }

        public JsonResult _ActualizarTasa()
        {
            try
            {
                var Dolar = AE_DolaresREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).FirstOrDefault();
                List<AE_EstadoCuenta> EstadoCuenta = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.AE_Avance.IdEstatus == 1 && u.FechaOperacion.Day == DateTime.Now.Day && u.FechaOperacion.Month == DateTime.Now.Month && u.FechaOperacion.Year == DateTime.Now.Year && !u.Abono && !u.RecibidoEnDolares).ToList();
                if (EstadoCuenta.Count > 0)
                {
                    foreach (var item in EstadoCuenta)
                    {
                        item.Tasa = Dolar.Tasa;
                        item.Monto = (item.MontoBs.Value / Dolar.Tasa);
                    }
                    AE_EstadoCuentaREPO.SaveChanges();
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
                message = "Movimiento agregado de forma correcta!"
            }, JsonRequestBehavior.DenyGet);

        }

        public JsonResult _ActualizarEstatus(int id, int estatus)
        {
            try
            {

                var Cambio = AE_CambioDiarioREPO.GetAllRecords().Where(u => u.Id == id).FirstOrDefault();
                Cambio.IdEstatus = estatus;
                AE_CambioDiarioREPO.SaveChanges();

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
                message = "Ajuste realizado de forma correcta!"
            }, JsonRequestBehavior.DenyGet);

        }

        public JsonResult _ActualizarEstatusEfectivo(int id, bool estatus)
        {
            try
            {

                var Cambio = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.Id == id).FirstOrDefault();
                Cambio.EfectivoCambiado = estatus;
                AE_EstadoCuentaREPO.SaveChanges();

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
                message = "Ajuste realizado de forma correcta!"
            }, JsonRequestBehavior.DenyGet);

        }


        public ActionResult DashboardFondo()
        {
            decimal acciones = 0;
            decimal accionesTR = 0;
            decimal accionesExistentes = 0;
            var balanceAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.Id).ToList();
            if (balanceAcciones.Count > 0)
            {
                acciones = AE_ValorAccionREPO.GetAllRecords().Take(5).OrderByDescending(u => u.FechaOperacion).FirstOrDefault().ValorAccion;
                accionesExistentes = balanceAcciones.FirstOrDefault().TotalAcciones;
            }
            accionesTR = AE_ValorAccionTRREPO.GetAllRecords().Take(5).OrderByDescending(u => u.FechaOperacion).FirstOrDefault().ValorAccion;
            ViewBag.Acciones = acciones;
            ViewBag.AccionesTR = accionesTR;
            ViewBag.AccionesExistentes = accionesExistentes;
            ViewBag.Opreaciones = AE_OperacionREPO.GetAllRecords().Where(u => u.IdEstatus == 1).ToList();
            var ValorAccion = AE_ValorAccionREPO.GetAllRecords().Take(15).OrderByDescending(u => u.FechaOperacion).ToList();
            var ValorAccionTR = AE_ValorAccionTRREPO.GetAllRecords().Take(15).OrderByDescending(u => u.FechaOperacion).ToList();
            var BalanceDiario = AE_BalanceDiarioREPO.GetAllRecords().Take(15).OrderByDescending(u => u.FechaOperaicon).ToList();
            List<decimal> Capital = new List<decimal>();
            List<decimal> Accion = new List<decimal>();
            List<decimal> AccionTR = new List<decimal>();
            List<string> _fechas = new List<string>();
            DateTime _fechainiciobalance;
            if (BalanceDiario.Count > 0)
                _fechainiciobalance = BalanceDiario.OrderBy(u => u.FechaOperaicon).First().FechaOperaicon;
            else
                _fechainiciobalance = DateTime.Now;
            DateTime _fechaFin = new DateTime();
            int cantidad = ValorAccionTR.Count();
            int itemsl = BalanceDiario.GroupBy(u => u.FechaOperaicon.ToString("dd/MM/yyyy")).Count();
            if (cantidad == 15)
            {
                foreach (var item in ValorAccionTR.OrderBy(u => u.FechaOperacion).ToList())
                {
                    Capital.Add(item.NuevoCapital);
                    _fechas.Add(item.FechaOperacion.ToString("dd/MM"));
                }
            }
            else
            {
                foreach (var item in ValorAccionTR.OrderBy(u => u.FechaOperacion))
                {
                    Capital.Add(item.NuevoCapital);
                    _fechas.Add(item.FechaOperacion.ToString("dd/MM"));
                }
                _fechaFin = _fechainiciobalance.AddDays(15 - cantidad);
                _fechainiciobalance = _fechainiciobalance.AddDays(itemsl);

                while (_fechainiciobalance <= _fechaFin)
                {
                    _fechas.Add(_fechainiciobalance.ToString("dd/MM"));
                    _fechainiciobalance = _fechainiciobalance.AddDays(1);
                }
            }

            foreach (var item in ValorAccion.OrderBy(u => u.FechaOperacion).ToList())
            {
                Accion.Add(item.ValorAccion);
            }


            if (ValorAccionTR.Count < 15)
            {
                int a = 15 - ValorAccionTR.Count();
                while (a > 1)
                {
                    AccionTR.Add(0);
                    a--;
                }
            }
            foreach (var item in ValorAccionTR.OrderBy(u => u.FechaOperacion).ToList())
            {
                AccionTR.Add(item.ValorAccion);
            }

            ViewBag.fechas = _fechas.ToArray();
            ViewBag.Capital = Capital.ToArray();
            ViewBag.ValorAccion = Accion.ToArray();
            ViewBag.ValorAccionTR = AccionTR.ToArray();

            var operaciones = AE_OperacionREPO.GetAllRecords().Where(u => u.IdEstatus == 1).ToList();
            List<OperacionesActivas> lista = new List<OperacionesActivas>();
            if (operaciones.Count > 0)
            {
                foreach (var item in operaciones)
                {
                    OperacionesActivas elemento = new OperacionesActivas();
                    elemento.label = item.CUser.Name + " " + item.CUser.LastName;
                    elemento.value = Math.Round((item.RepresentacionFondo * 100 / accionesExistentes), 2);
                    lista.Add(elemento);
                }

            }
            else
            {

                OperacionesActivas elemento = new OperacionesActivas();
                elemento.label = "No activo";
                elemento.value = 0;
                lista.Add(elemento);

            }
            var json = new JavaScriptSerializer().Serialize(lista);
            ViewBag.OperacionesActivas = json;
            return View("_dashboardFondo");
        }

        public class OperacionesActivas
        {
            public string label { get; set; }
            public decimal value { get; set; }
        }

    }
}