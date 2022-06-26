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

namespace Umbrella.Controllers
{
    [Authorize(Roles =
       UserRoleConstant.TransaXAdmin + "," +
       UserRoleConstant.TransaXUser + "," +
       UserRoleConstant.CommerceAdmin + "," +
               UserRoleConstant.Inversionista + "," +
       UserRoleConstant.CommerceUser
       )]
    public class OperacionController : Controller
    {
        URepository<AE_Operacion> AE_OperacionREPO = new URepository<AE_Operacion>();
        URepository<Commerce> CRepo = new URepository<Commerce>();
        URepository<CUser> CURepo = new URepository<CUser>();
        URepository<AE_Dolar> AE_DolarREPO = new URepository<AE_Dolar>(); 
        URepository<AE_MovimientosCuenta> AEmovimientosREPO = new URepository<AE_MovimientosCuenta>();
        URepository<AE_Propuesta> AE_PropuestaREPO = new URepository<InstaTransfer.DataAccess.AE_Propuesta>();
        URepository<AE_Avance> AE_AvanceREPO = new URepository<InstaTransfer.DataAccess.AE_Avance>();
        URepository<AE_EstadoCuenta> AE_EstadoCuentaREPO = new URepository<InstaTransfer.DataAccess.AE_EstadoCuenta>();
        URepository<AE_Archivo> AE_ArchivoREPO = new URepository<InstaTransfer.DataAccess.AE_Archivo>();
        URepository<AE_Variable> AE_VariableREPO = new URepository<AE_Variable>();
        URepository<AE_BalanceAccione> AE_BalanceAccioneREPO = new URepository<AE_BalanceAccione>();
        URepository<AE_ValorAccion> AE_ValorAccionREPO = new URepository<AE_ValorAccion>();
        URepository<AE_BalanceDiario> AE_BalanceDiarioREPO = new URepository<AE_BalanceDiario>();
        URepository<AE_ValorAccionTR> AE_ValorAccionTRREPO = new URepository<AE_ValorAccionTR>();
        URepository<AE_OperacionPago> AE_OperacionPagoREPO = new URepository<AE_OperacionPago>();
        URepository<AE_AdministradorPago> AE_AdministradorPagoREPO = new URepository<AE_AdministradorPago>();
        URepository<AE_GastoFondo> AE_GastoFondoREPO = new URepository<AE_GastoFondo>();
        URepository<AE_Cierre> AE_CierreREPO = new URepository<AE_Cierre>();
        URepository<AE_OperacionAporteCapital> AE_OperacionAporteCapitalREPO = new URepository<AE_OperacionAporteCapital>();
    
        URepository<AE_BalanceAccione> AE_BalanceAccionesREPO = new URepository<AE_BalanceAccione>();

        // GET: Avance
        public ActionResult Index()
        {
    
            List<AE_Operacion> Lista = AE_OperacionREPO.GetAllRecords().Where(u => u.IdEstatus == 1 && u.CUser.IdAspNetUser.ToString() ==  User.Identity.GetUserId()).ToList();
            return View(Lista);
        }

        public ActionResult Historico()
        {
            
            ViewBag.BalanceAcciones = AE_BalanceAccioneREPO.GetAllRecords().OrderByDescending(u => u.Id).ToList();
            ViewBag.GastoFondo = AE_GastoFondoREPO.GetAllRecords().OrderByDescending(u => u.Id).ToList();
            List<AE_Cierre> Lista = AE_CierreREPO.GetAllRecords().OrderByDescending(u => u.Id).ToList();
            ViewBag.Cierres = Lista;
            return View();
        }
        public JsonResult _GuardarGasto(decimal gastousd,  DateTime gastofecha, string gastodescripcion)
        {
            try
            {

                AE_GastoFondo Gasto = new AE_GastoFondo();
                Gasto.Descripcion = gastodescripcion;
                Gasto.Monto = gastousd;
                Gasto.FechaRegsitro = gastofecha;
                AE_GastoFondoREPO.AddEntity(Gasto);
                AE_GastoFondoREPO.SaveChanges();



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
        public ActionResult Pagos()
        {

            List<AE_AdministradorPago> AdministradorPago = AE_AdministradorPagoREPO.GetAllRecords().OrderByDescending(u => u.FechaRegistro).ToList();
            List<AE_OperacionPago> OperacionPago = AE_OperacionPagoREPO.GetAllRecords(u => u.TipoPagoUtilidad).OrderByDescending(u => u.FechaPago).ToList();
            List<AE_OperacionPago> OperacionPagoReinversion = AE_OperacionPagoREPO.GetAllRecords(u => u.TipoReinversionUtilidad).OrderByDescending(u => u.FechaPago).ToList();
            List<AE_OperacionPago> OperacionPagoCapital = AE_OperacionPagoREPO.GetAllRecords(u => u.TipoPagoCapital).OrderByDescending(u => u.FechaPago).ToList();
            ViewBag.AdminPago = AdministradorPago;
            ViewBag.OperacionPago = OperacionPago;
            ViewBag.OperacionPagoReinversion = OperacionPagoReinversion;
            ViewBag.OperacionPagoCapital = OperacionPagoCapital;
            return View();
          
        }

        public ActionResult Fondo()
        {
            List<AE_Operacion> Lista = AE_OperacionREPO.GetAllRecords().Where(u => u.IdEstatus == 1 || u.IdEstatus == 2).ToList();
            return View(Lista);
        }
        public ActionResult AddOperacion()
        {
            List<CUser> users = new List<CUser>();
            users = CURepo.GetAllRecords(cu => cu.AspNetUser.AspNetUserRoles.FirstOrDefault().AspNetRole.Name.Contains("TransaXInversionista")).ToList();
            ViewBag.Users = users;
            return PartialView("_AddOperacion");
        }

        public JsonResult CreateOperacion(DateTime FechaInicio, DateTime FechaFin, decimal Monto, string Inversionista, decimal PorcentajeG, int Estatus, int Tipo  )
        {
            AE_Operacion Operacion = new AE_Operacion();
            try
            {
                Operacion.IdInversionista = Guid.Parse(Inversionista);
                Operacion.Monto = Monto;
                Operacion.MontoInicial = Monto;
                Operacion.MontoFinal = 0;
                Operacion.MontoGanado = 0;
                Operacion.TipoPago = Tipo;
                Operacion.IdEstatus = Estatus;
                Operacion.Date = DateTime.Now;
                Operacion.FechaInicioOperacion = FechaInicio;
                Operacion.FechaFinOperacion = FechaFin;
                Operacion.PorcentajeGanancia = PorcentajeG;
                Operacion.RepresentacionFondo = 0;
                Operacion.RetiraUtilidadMes = false;
                Operacion.SeRetiraFondo = false; 
                AE_OperacionREPO.AddEntity(Operacion);
                AE_OperacionREPO.SaveChanges();

               
                decimal nuevaacciones = 0;
                AE_BalanceAccione elemento = new AE_BalanceAccione();
                var balanceAcciones = AE_BalanceAccioneREPO.GetAllRecords().ToList();
                if (balanceAcciones.Count > 0)
                {
                    var _valoracciones = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.Id).Take(10).ToList();
                    var item = balanceAcciones.OrderByDescending(u => u.Id).FirstOrDefault();
                    nuevaacciones = Monto / _valoracciones.FirstOrDefault().ValorAccion;
                    elemento.TotaCapital = Monto;
                    elemento.AccionesRetiradas = 0;
                    elemento.CapitalEntrada = Monto;
                    elemento.CapitalSalida = 0;
                    elemento.FactorInicial = 0;
                    elemento.FechaOperacion = DateTime.Now;
                    elemento.FechaRegistro = DateTime.Now;
                    elemento.FechaUltimaActualizacion = DateTime.Now;
                    elemento.TotalAccionesExistentes = item.TotalAccionesExistentes;
                    elemento.AccionesEntrantes = nuevaacciones;
                    elemento.TotalAcciones = item.TotalAcciones + nuevaacciones;
                    elemento.IdInversionista = Guid.Parse(Inversionista);
                    elemento.ValorAcciones = item.ValorAcciones;
                    elemento.AccionesAjustadas = 0;
                    AE_BalanceAccioneREPO.AddEntity(elemento);
                    AE_BalanceAccioneREPO.SaveChanges();

                    Operacion.RepresentacionFondo = nuevaacciones;
                    AE_OperacionREPO.SaveChanges();

                }
                else {

                    decimal _valoracciones = 0;
                    _valoracciones = Monto / 1000;
          
                    elemento.AccionesEntrantes = 1000;
                    elemento.TotaCapital = 0;
                    elemento.AccionesRetiradas = 0;
                    elemento.AccionesAjustadas = 0;
                    elemento.CapitalEntrada = Monto;
                    elemento.CapitalSalida = 0;
                    elemento.FactorInicial = 1000;
                    elemento.FechaOperacion = DateTime.Now;
                    elemento.FechaRegistro = DateTime.Now;
                    elemento.FechaUltimaActualizacion = DateTime.Now;
                    elemento.TotalAccionesExistentes = 0;
                    elemento.TotalAcciones = 1000;
                    elemento.IdInversionista = Guid.Parse(Inversionista);
                    elemento.ValorAcciones = _valoracciones;
                    AE_BalanceAccioneREPO.AddEntity(elemento);
                    AE_BalanceAccioneREPO.SaveChanges();

                    Operacion.RepresentacionFondo = 1000;
                    AE_OperacionREPO.SaveChanges();
                }

                ///ACTUALIZO BALANCE DIARIO AGEGANDO ESTE DINERO QUE ENTRA
                var balancediario = AE_BalanceDiarioREPO.GetAllRecords().OrderByDescending(u => u.FechaOperaicon).FirstOrDefault(); 

                AE_BalanceDiario NuevoBalance = new AE_BalanceDiario();
                NuevoBalance.FechaCreacionRegistro = DateTime.Now;
                NuevoBalance.FechaUltimaActualizacion = DateTime.Now;
                NuevoBalance.FechaOperaicon = DateTime.Now;

                //ACTIVOS
                NuevoBalance.TotalCuentaBs = decimal.Parse("0");
                NuevoBalance.TotalCuentaEstimadoUsd = decimal.Parse("0");
                NuevoBalance.TotalCuentaUsd = decimal.Parse("0");
                NuevoBalance.TotalPorCobrarBs = balancediario.TotalPorCobrarBs;
                NuevoBalance.TotalActivos = balancediario.TotalActivos;
                //PASIVOS
                NuevoBalance.TotalPasivoBs = balancediario.TotalPasivoBs;
                NuevoBalance.TotalPasivosEstimadoUsd = balancediario.TotalPasivosEstimadoUsd;
                NuevoBalance.TotalPasivoUsd = balancediario.TotalPasivoUsd;
                NuevoBalance.TotalPasivos = balancediario.TotalPasivos;
                //CAPITAL
                if (balancediario != null && balancediario.Id > 0)
                {
                    NuevoBalance.TotalCapital = balancediario.TotalCapital + Monto;
                }
                else {
                    NuevoBalance.TotalCapital = Monto;
                }
                NuevoBalance.TasaUtilizada = balancediario.TasaUtilizada;
                NuevoBalance.TotalGananciaDiaria = 0;
                NuevoBalance.TotalCobroDiario = 0;

                AE_BalanceDiarioREPO.AddEntity(NuevoBalance);
                AE_BalanceDiarioREPO.SaveChanges();
                ///VALOR ACCION PROYECTADA
                var valoracciones = AE_ValorAccionREPO.GetAllRecords().OrderByDescending(u => u.Id).Take(10).ToList();
                if (valoracciones != null && valoracciones.Count() > 0)
                {
                    AE_ValorAccion item = valoracciones.FirstOrDefault();

                    AE_ValorAccion NuevoItem = new AE_ValorAccion();
                    NuevoItem.FechaCreacionRegistro = DateTime.Now;
                    NuevoItem.FechaOperacion = DateTime.Now;
                    NuevoItem.FechaUltimaActualizacion = DateTime.Now;
                    NuevoItem.TotalAcciones = elemento.TotalAcciones;
                    NuevoItem.TotalCapitalusd = NuevoBalance.TotalCapital;
                    NuevoItem.ValorAccion = NuevoBalance.TotalCapital / elemento.TotalAcciones;
                    NuevoItem.IdBalanceDiario = NuevoBalance.Id;
                    NuevoItem.IdBalanceAcciones = elemento.Id;
                    AE_ValorAccionREPO.AddEntity(NuevoItem);
                    AE_ValorAccionREPO.SaveChanges();

                }
                else {

                    AE_ValorAccion NuevoItem = new AE_ValorAccion();
                    NuevoItem.FechaCreacionRegistro = DateTime.Now;
                    NuevoItem.FechaOperacion = DateTime.Now;
                    NuevoItem.FechaUltimaActualizacion = DateTime.Now;
                    NuevoItem.TotalAcciones = elemento.TotalAcciones;
                    NuevoItem.TotalCapitalusd = NuevoBalance.TotalCapital;
                    NuevoItem.ValorAccion =  NuevoBalance.TotalCapital / elemento.TotalAcciones;
                    NuevoItem.IdBalanceDiario = NuevoBalance.Id;
                    NuevoItem.IdBalanceAcciones = elemento.Id;
                    AE_ValorAccionREPO.AddEntity(NuevoItem);
                    AE_ValorAccionREPO.SaveChanges();
                }

                //VALOR ACCION TIEMPO REAL
                var _valoraccionesTR = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.Id).Take(10).ToList();
                if (_valoraccionesTR != null && _valoraccionesTR.Count() > 0)
                {
                    AE_ValorAccionTR ultimovaloraacion = _valoraccionesTR.FirstOrDefault();

                    AE_ValorAccionTR NuevoItem = new AE_ValorAccionTR();
                    NuevoItem.FechaCreacionRegistro = DateTime.Now.AddDays(0);
                    NuevoItem.FechaOperacion = DateTime.Now.AddDays(0);
                    NuevoItem.FechaUltimaActualizacion = DateTime.Now.AddDays(0);
                    NuevoItem.TotalAcciones = elemento.TotalAcciones;
                    NuevoItem.CapitalInicial = ultimovaloraacion.NuevoCapital;
                    NuevoItem.UtilidadReportada = 0;
                    NuevoItem.GastoReportado = 0;
                    NuevoItem.SaldoUSD = Operacion.Monto + ultimovaloraacion.SaldoUSD;
                    NuevoItem.CapitalPorCobrar = ultimovaloraacion.CapitalPorCobrar;
                    NuevoItem.PagoCapitalInversionista = 0;
                    NuevoItem.PagoUtilidadAdministrador = 0;
                    NuevoItem.PagoUtilidadMesInversionista = 0;
                    NuevoItem.CapitalNuevoIngreso = Operacion.Monto;
                    NuevoItem.NuevoCapital = Operacion.Monto + ultimovaloraacion.NuevoCapital;
                    NuevoItem.ValorAccion = ultimovaloraacion.ValorAccion;
                    AE_ValorAccionTRREPO.AddEntity(NuevoItem);
                    AE_ValorAccionTRREPO.SaveChanges();

                }
                else
                {

                    //AE_ValorAccion NuevoItem = new AE_ValorAccion();
                    //NuevoItem.FechaCreacionRegistro = DateTime.Now;
                    //NuevoItem.FechaOperacion = DateTime.Now;
                    //NuevoItem.FechaUltimaActualizacion = DateTime.Now;
                    //NuevoItem.TotalAcciones = elemento.TotalAcciones;
                    //NuevoItem.TotalCapitalusd = NuevoBalance.TotalCapital;
                    //NuevoItem.ValorAccion = NuevoBalance.TotalCapital / elemento.TotalAcciones;
                    //NuevoItem.IdBalanceDiario = NuevoBalance.Id;
                    //NuevoItem.IdBalanceAcciones = elemento.Id;
                    //AE_ValorAccionREPO.AddEntity(NuevoItem);
                    //AE_ValorAccionREPO.SaveChanges();


                    AE_ValorAccionTR NuevoItem = new AE_ValorAccionTR();
                    NuevoItem.FechaCreacionRegistro = DateTime.Now;
                    NuevoItem.FechaOperacion = DateTime.Now;
                    NuevoItem.FechaUltimaActualizacion = DateTime.Now;
                    NuevoItem.TotalAcciones = elemento.TotalAcciones;
                    NuevoItem.CapitalInicial = 0;
                    NuevoItem.UtilidadReportada = 0;
                    NuevoItem.GastoReportado = 0;
                    NuevoItem.PagoCapitalInversionista = 0;
                    NuevoItem.PagoUtilidadMesInversionista = 0;
                    NuevoItem.PagoUtilidadAdministrador = 0;
                    NuevoItem.CapitalNuevoIngreso = Operacion.Monto;
                    NuevoItem.NuevoCapital = NuevoBalance.TotalCapital;
                    NuevoItem.ValorAccion = NuevoBalance.TotalCapital / elemento.TotalAcciones;
                    AE_ValorAccionTRREPO.AddEntity(NuevoItem);
                    AE_ValorAccionTRREPO.SaveChanges();
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


        public JsonResult AddCapitalOperacion(decimal Monto, string Inversionista, int idoperacion)
        {
            AE_Operacion Operacion = AE_OperacionREPO.GetAllRecords().Where(u => u.Id == idoperacion).FirstOrDefault();
            AE_OperacionAporteCapital Aporte = new AE_OperacionAporteCapital();
            Aporte.FechaRegistro = DateTime.Now;
            Aporte.IdOperacion = idoperacion;
            Aporte.Moneda = 1;
            Aporte.Monto = Monto;
            Aporte.CapitalOperacionInicial = Operacion.Monto;
            Aporte.CapitalDespuesAporte = Operacion.Monto + Monto;
            AE_OperacionAporteCapitalREPO.AddEntity(Aporte);
            AE_OperacionAporteCapitalREPO.SaveChanges();

            Operacion.Monto = Operacion.Monto + Monto;
            AE_OperacionREPO.SaveChanges();

            try
            {
                decimal nuevaacciones = 0;
                AE_BalanceAccione elemento = new AE_BalanceAccione();
                var balanceAcciones = AE_BalanceAccioneREPO.GetAllRecords().ToList();
                if (balanceAcciones.Count > 0)
                {
                    var _valoracciones = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.Id).Take(10).ToList();
                    var item = balanceAcciones.OrderByDescending(u => u.Id).FirstOrDefault();
                    nuevaacciones = Monto / _valoracciones.FirstOrDefault().ValorAccion;
                    elemento.TotaCapital = Monto;
                    elemento.AccionesRetiradas = 0;
                    elemento.CapitalEntrada = Monto;
                    elemento.CapitalSalida = 0;
                    elemento.FactorInicial = 0;
                    elemento.FechaOperacion = DateTime.Now;
                    elemento.FechaRegistro = DateTime.Now;
                    elemento.FechaUltimaActualizacion = DateTime.Now;
                    elemento.TotalAccionesExistentes = item.TotalAccionesExistentes;
                    elemento.AccionesEntrantes = nuevaacciones;
                    elemento.TotalAcciones = item.TotalAcciones + nuevaacciones;
                    elemento.IdInversionista = Guid.Parse(Inversionista);
                    elemento.ValorAcciones = item.ValorAcciones;
                    elemento.AccionesAjustadas = 0;
                    AE_BalanceAccioneREPO.AddEntity(elemento);
                    AE_BalanceAccioneREPO.SaveChanges();

                    //Operacion.RepresentacionFondo = nuevaacciones;
                    //AE_OperacionREPO.SaveChanges();

                }
                else
                {

                    decimal _valoracciones = 0;
                    _valoracciones = Monto / 1000;

                    elemento.AccionesEntrantes = 1000;
                    elemento.TotaCapital = 0;
                    elemento.AccionesRetiradas = 0;
                    elemento.AccionesAjustadas = 0;
                    elemento.CapitalEntrada = Monto;
                    elemento.CapitalSalida = 0;
                    elemento.FactorInicial = 1000;
                    elemento.FechaOperacion = DateTime.Now;
                    elemento.FechaRegistro = DateTime.Now;
                    elemento.FechaUltimaActualizacion = DateTime.Now;
                    elemento.TotalAccionesExistentes = 0;
                    elemento.TotalAcciones = 1000;
                    elemento.IdInversionista = Guid.Parse(Inversionista);
                    elemento.ValorAcciones = _valoracciones;
                    AE_BalanceAccioneREPO.AddEntity(elemento);
                    AE_BalanceAccioneREPO.SaveChanges();

                    Operacion.RepresentacionFondo = 1000;
                    AE_OperacionREPO.SaveChanges();
                }

                ///ACTUALIZO BALANCE DIARIO AGEGANDO ESTE DINERO QUE ENTRA
                var balancediario = AE_BalanceDiarioREPO.GetAllRecords().OrderByDescending(u => u.FechaOperaicon).FirstOrDefault();

                AE_BalanceDiario NuevoBalance = new AE_BalanceDiario();
                NuevoBalance.FechaCreacionRegistro = DateTime.Now;
                NuevoBalance.FechaUltimaActualizacion = DateTime.Now;
                NuevoBalance.FechaOperaicon = DateTime.Now;

                //ACTIVOS
                NuevoBalance.TotalCuentaBs = decimal.Parse("0");
                NuevoBalance.TotalCuentaEstimadoUsd = decimal.Parse("0");
                NuevoBalance.TotalCuentaUsd = decimal.Parse("0");
                NuevoBalance.TotalPorCobrarBs = balancediario.TotalPorCobrarBs;
                NuevoBalance.TotalActivos = balancediario.TotalActivos;
                //PASIVOS
                NuevoBalance.TotalPasivoBs = balancediario.TotalPasivoBs;
                NuevoBalance.TotalPasivosEstimadoUsd = balancediario.TotalPasivosEstimadoUsd;
                NuevoBalance.TotalPasivoUsd = balancediario.TotalPasivoUsd;
                NuevoBalance.TotalPasivos = balancediario.TotalPasivos;
                //CAPITAL
                if (balancediario != null && balancediario.Id > 0)
                {
                    NuevoBalance.TotalCapital = balancediario.TotalCapital + Monto;
                }
                else
                {
                    NuevoBalance.TotalCapital = Monto;
                }
                NuevoBalance.TasaUtilizada = balancediario.TasaUtilizada;
                NuevoBalance.TotalGananciaDiaria = 0;
                NuevoBalance.TotalCobroDiario = 0;

                AE_BalanceDiarioREPO.AddEntity(NuevoBalance);
                AE_BalanceDiarioREPO.SaveChanges();
                ///VALOR ACCION PROYECTADA
                var valoracciones = AE_ValorAccionREPO.GetAllRecords().OrderByDescending(u => u.Id).Take(10).ToList();
                if (valoracciones != null && valoracciones.Count() > 0)
                {
                    AE_ValorAccion item = valoracciones.FirstOrDefault();

                    AE_ValorAccion NuevoItem = new AE_ValorAccion();
                    NuevoItem.FechaCreacionRegistro = DateTime.Now;
                    NuevoItem.FechaOperacion = DateTime.Now;
                    NuevoItem.FechaUltimaActualizacion = DateTime.Now;
                    NuevoItem.TotalAcciones = elemento.TotalAcciones;
                    NuevoItem.TotalCapitalusd = NuevoBalance.TotalCapital;
                    NuevoItem.ValorAccion = NuevoBalance.TotalCapital / elemento.TotalAcciones;
                    NuevoItem.IdBalanceDiario = NuevoBalance.Id;
                    NuevoItem.IdBalanceAcciones = elemento.Id;
                    AE_ValorAccionREPO.AddEntity(NuevoItem);
                    AE_ValorAccionREPO.SaveChanges();

                }
                else
                {

                    AE_ValorAccion NuevoItem = new AE_ValorAccion();
                    NuevoItem.FechaCreacionRegistro = DateTime.Now;
                    NuevoItem.FechaOperacion = DateTime.Now;
                    NuevoItem.FechaUltimaActualizacion = DateTime.Now;
                    NuevoItem.TotalAcciones = elemento.TotalAcciones;
                    NuevoItem.TotalCapitalusd = NuevoBalance.TotalCapital;
                    NuevoItem.ValorAccion = NuevoBalance.TotalCapital / elemento.TotalAcciones;
                    NuevoItem.IdBalanceDiario = NuevoBalance.Id;
                    NuevoItem.IdBalanceAcciones = elemento.Id;
                    AE_ValorAccionREPO.AddEntity(NuevoItem);
                    AE_ValorAccionREPO.SaveChanges();
                }

                //VALOR ACCION TIEMPO REAL
                var _valoraccionesTR = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.Id).Take(10).ToList();
                if (_valoraccionesTR != null && _valoraccionesTR.Count() > 0)
                {
                    AE_ValorAccionTR ultimovaloraacion = _valoraccionesTR.FirstOrDefault();

                    AE_ValorAccionTR NuevoItem = new AE_ValorAccionTR();
                    NuevoItem.FechaCreacionRegistro = DateTime.Now;
                    NuevoItem.FechaOperacion = DateTime.Now;
                    NuevoItem.FechaUltimaActualizacion = DateTime.Now;
                    NuevoItem.TotalAcciones = elemento.TotalAcciones;
                    NuevoItem.CapitalInicial = ultimovaloraacion.NuevoCapital;
                    NuevoItem.UtilidadReportada = 0;
                    NuevoItem.GastoReportado = 0;
                    NuevoItem.SaldoUSD = Operacion.Monto + ultimovaloraacion.SaldoUSD;
                    NuevoItem.CapitalPorCobrar = ultimovaloraacion.CapitalPorCobrar;
                    NuevoItem.PagoCapitalInversionista = 0;
                    NuevoItem.PagoUtilidadAdministrador = 0;
                    NuevoItem.PagoUtilidadMesInversionista = 0;
                    NuevoItem.CapitalNuevoIngreso = Operacion.Monto;
                    NuevoItem.NuevoCapital = Operacion.Monto + ultimovaloraacion.NuevoCapital;
                    NuevoItem.ValorAccion = ultimovaloraacion.ValorAccion;
                    AE_ValorAccionTRREPO.AddEntity(NuevoItem);
                    AE_ValorAccionTRREPO.SaveChanges();

                }
                else
                {

                    AE_ValorAccionTR NuevoItem = new AE_ValorAccionTR();
                    NuevoItem.FechaCreacionRegistro = DateTime.Now;
                    NuevoItem.FechaOperacion = DateTime.Now;
                    NuevoItem.FechaUltimaActualizacion = DateTime.Now;
                    NuevoItem.TotalAcciones = elemento.TotalAcciones;
                    NuevoItem.CapitalInicial = 0;
                    NuevoItem.UtilidadReportada = 0;
                    NuevoItem.GastoReportado = 0;
                    NuevoItem.PagoCapitalInversionista = 0;
                    NuevoItem.PagoUtilidadMesInversionista = 0;
                    NuevoItem.PagoUtilidadAdministrador = 0;
                    NuevoItem.CapitalNuevoIngreso = Monto;
                    NuevoItem.NuevoCapital = NuevoBalance.TotalCapital;
                    NuevoItem.ValorAccion = NuevoBalance.TotalCapital / elemento.TotalAcciones;
                    AE_ValorAccionTRREPO.AddEntity(NuevoItem);
                    AE_ValorAccionTRREPO.SaveChanges();
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

        public JsonResult ChangeStatusMes(int idoperacion, bool estatus)
        {
            try {

                var ope = AE_OperacionREPO.GetAllRecords().Where(u => u.Id == idoperacion).FirstOrDefault();
                ope.RetiraUtilidadMes = estatus;
                AE_OperacionREPO.SaveChanges();

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

        public JsonResult ChangeStatusFondo(int idoperacion, bool estatus)
        {
            try
            {

                var ope = AE_OperacionREPO.GetAllRecords().Where(u => u.Id == idoperacion).FirstOrDefault();
                ope.SeRetiraFondo = estatus;
                AE_OperacionREPO.SaveChanges();

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

        public ActionResult Details(int Id)
        {
            ViewBag.Dolar = AE_DolarREPO.GetAllRecords().Take(30).OrderByDescending(u => u.FechaValor).ToList();
            List<AE_Variable> Variables = AE_VariableREPO.GetAllRecords().ToList();
            ViewBag.Variables = Variables;
            AE_Operacion Operacion = AE_OperacionREPO.GetAllRecords().Where(u => u.Id == Id).FirstOrDefault();
            List<AE_Avance> ListAvance = new List<AE_Avance>();
            ListAvance = AE_AvanceREPO.GetAllRecords().Where(u => u.IdEstatus == 1 || u.IdEstatus == 2 && u.FechaInicioCobro > Operacion.FechaInicioOperacion).ToList();
            ViewBag.Balance = AE_BalanceDiarioREPO.GetAllRecords().Take(10).OrderByDescending(u => u.FechaOperaicon).ToList();
            ViewBag.Acciones = AE_ValorAccionTRREPO.GetAllRecords().Take(10).OrderByDescending(u => u.FechaOperacion).ToList();
            ViewBag.Avances = ListAvance;
            decimal reembolso = 0;
            decimal cobrado = 0;
            decimal prestado = 0;
            foreach (var item in ListAvance.Where(u => u.IdEstatus == 1).ToList())
            {
                cobrado = cobrado + item.AE_EstadoCuentas.Where(u => !u.Abono).Sum(u => u.Monto);
                reembolso = reembolso + item.Reembolso;
                prestado = prestado + item.Avance;
            }

            ViewBag.Cobrado = cobrado;
            ViewBag.Reembolso = reembolso;
            ViewBag.Prestado = prestado;

            return View(Operacion);
        }

        public ActionResult Reporte(int Id)
        {
            ViewBag.Dolar = AE_DolarREPO.GetAllRecords().Take(30).OrderByDescending(u => u.FechaValor).ToList();
            List<AE_Variable> Variables = AE_VariableREPO.GetAllRecords().ToList();
            ViewBag.Variables = Variables;
            AE_Operacion Operacion = AE_OperacionREPO.GetAllRecords().Where(u => u.Id == Id).FirstOrDefault();
            List<AE_OperacionPago> OperacioPago = Operacion.AE_OperacionPagos.OrderBy(u => u.Id).ToList();
            List<AE_OperacionAporteCapital> AporteCapital = Operacion.AE_OperacionAporteCapitals.OrderBy(u => u.Id).ToList();
            List<AE_Avance> ListAvance = new List<AE_Avance>();
            ListAvance = AE_AvanceREPO.GetAllRecords().Where(u => u.IdEstatus == 1 || u.IdEstatus == 2 && u.FechaInicioCobro > Operacion.FechaInicioOperacion).ToList();
            ViewBag.Balance = AE_BalanceDiarioREPO.GetAllRecords().Take(10).OrderByDescending(u => u.FechaOperaicon).ToList();
            List<AE_ValorAccionTR> ValorAccionTR = AE_ValorAccionTRREPO.GetAllRecords().Take(10).OrderByDescending(u => u.FechaOperacion).ToList();
            ViewBag.Acciones = ValorAccionTR;
            ViewBag.Avances = ListAvance;
            ViewBag.Operacion = Operacion;
            ViewBag.OperacionPago = OperacioPago;
            ViewBag.AporteCapital = AporteCapital;
            decimal rendimientototal = ((decimal.Parse(ValorAccionTR.FirstOrDefault().ValorAccion.ToString()) - decimal.Parse("27,62")) * 100 / decimal.Parse("27,62"));
            ViewBag.RedimientoGlobal = rendimientototal;
            decimal repartido = AE_OperacionREPO.GetAllRecords().Sum(u => u.MontoGanado);
            ViewBag.Repartido = Math.Round(repartido, 2).ToString("N2");
            decimal reembolso = 0;
            decimal cobrado = 0;
            decimal prestado = 0;
            foreach (var item in ListAvance.Where(u => u.IdEstatus == 1).ToList())
            {
                cobrado = cobrado + item.AE_EstadoCuentas.Where(u => !u.Abono).Sum(u => u.Monto);
                reembolso = reembolso + item.Reembolso;
                prestado = prestado + item.Avance;
            }

            ViewBag.Cobrado = cobrado;
            ViewBag.Reembolso = reembolso;
            ViewBag.Prestado = prestado;

            return View(Operacion);
        }

        //CASO PUNTUAL
        public bool RetirarInversionDiaNoCierre(int idinversion)
        {

            //OBTENER VALORES
            decimal totalutilidafondo = 0;
            decimal totalutilidinver = 0;
            decimal utilidadacumulado = 0;
            decimal retitocapital = 0;
            AE_ValorAccionTR ValorAccionTR = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.FechaCreacionRegistro).FirstOrDefault();
            List<AE_Operacion> Operaciones = AE_OperacionREPO.GetAllRecords().Where(u => u.IdEstatus == 1 && u.Id == idinversion).ToList();
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
         
                if (item.SeRetiraFondo)
                {
                    //CREAMOS REGISTRO PAGO REINVERSION
                    AE_OperacionPago Pago = new AE_OperacionPago();
                    Pago.Date = DateTime.Now;
                    Pago.IdOperacion = item.Id;
                    Pago.Moneda = 1;
                    Pago.Monto = item.Monto;
                    Pago.Tasa = 1;
                    Pago.FechaPago = DateTime.Now;
                    Pago.TipoPagoCapital = true;
                    Pago.TipoPagoUtilidad = false;
                    Pago.TipoReinversionUtilidad = false;
                    AE_OperacionPagoREPO.AddEntity(Pago);
                    AE_OperacionPagoREPO.SaveChanges();
                    retitocapital = retitocapital + (item.Monto + utilidadreal);
                    //AJUSTAMOS OPERACION
                    //decimal accionesnuevas = (montoactual + utilidadreal) / ValorAccionTR.ValorAccion;
                    //item.Monto = item.Monto + utilidadreal;
                    //item.MontoGanado = item.MontoGanado + utilidadreal;
                    //item.RepresentacionFondo = accionesnuevas;
                    item.IdEstatus = 2;
                    AE_OperacionREPO.SaveChanges();

                    //AJUSTAMOS EL BALANCE DE LAS ACCIONES
                    decimal ajusteacciones = accionesviejas;
                    AE_BalanceAccione elemento = new AE_BalanceAccione();
                    var balanceAcciones = AE_BalanceAccionesREPO.GetAllRecords().OrderByDescending(u => u.Id).FirstOrDefault();
                    elemento.TotaCapital = item.Monto;
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
                    _NewValorAccionTR.FechaCreacionRegistro = DateTime.Now.AddDays(-0);
                    _NewValorAccionTR.FechaOperacion = DateTime.Now.AddDays(-0);
                    _NewValorAccionTR.FechaUltimaActualizacion = DateTime.Now.AddDays(-0);
                    _NewValorAccionTR.GastoReportado = 0;
                    _NewValorAccionTR.CapitalInicial = ValorAccionTR.NuevoCapital;
                    _NewValorAccionTR.CapitalPorCobrar = ValorAccionTR.CapitalPorCobrar;
                    _NewValorAccionTR.AbonoCapital = 0;
                    _NewValorAccionTR.UtilidadReportada = 0;
                    _NewValorAccionTR.SaldoUSD = ValorAccionTR.SaldoUSD - item.Monto;
                    _NewValorAccionTR.TotalCobroDiario = 0;
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

            }

            return true;
        }


    }
}