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
using System.Configuration;
using System.Threading;
using System.Globalization;
using Renci.SshNet;

namespace Umbrella.Controllers
{
    [Authorize(Roles =
       UserRoleConstant.TransaXAdmin + "," +
       UserRoleConstant.TransaXUser + "," +
       UserRoleConstant.CommerceAdmin + "," +
       UserRoleConstant.CommerceUser
       )]
    public class AvanceController : Controller
    {
        URepository<Commerce> CRepo = new URepository<Commerce>();
        URepository<CUser> CURepo = new URepository<CUser>();
        URepository<AE_MovimientosCuenta> AEmovimientosREPO = new URepository<AE_MovimientosCuenta>();
        URepository<AE_Propuesta> AE_PropuestaREPO = new URepository<InstaTransfer.DataAccess.AE_Propuesta>();
        URepository<AE_Avance> AE_AvanceREPO = new URepository<InstaTransfer.DataAccess.AE_Avance>();
        URepository<AE_EstadoCuenta> AE_EstadoCuentaREPO = new URepository<InstaTransfer.DataAccess.AE_EstadoCuenta>();
        URepository<AE_Archivo> AE_ArchivoREPO = new URepository<InstaTransfer.DataAccess.AE_Archivo>();
        URepository<AE_Dolar> AE_DolarREPO = new URepository<AE_Dolar>();
        URepository<CP_Archivo> CP_ArchivoREPO = new URepository<CP_Archivo>();
        URepository<AE_ValorAccionTR> AE_ValorAccionTRREPO = new URepository<AE_ValorAccionTR>();
        // GET: Avance
        public ActionResult Index()
        {
            List<AE_Avance> ListAvance = new List<AE_Avance>();
            ListAvance = AE_AvanceREPO.GetAllRecords().Where(u => u.IdEstatus == 1 || u.IdEstatus == 2 || u.IdEstatus == 4).OrderByDescending(u => u.Id).ToList();
            return View(ListAvance);
        }

        public ActionResult Masivo()
        {
            DateTime fecha = DateTime.Now;
            List<CP_Archivo> List = new List<CP_Archivo>();
            List = CP_ArchivoREPO.GetAllRecords().OrderByDescending(u => u.Id).ToList();
            ViewBag.CP_Archivo = List;

            List<AE_EstadoCuenta> ListaEstadoCuenta = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.AE_Avance.IdEstatus == 1 && u.FechaOperacion.Day == fecha.AddDays(0).Day && u.FechaOperacion.Month == fecha.Month && u.FechaOperacion.Year == fecha.Year && u.Monto > 0 && !u.Abono).ToList();
            ViewBag.CobroDiarios = ListaEstadoCuenta;

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GenerarArchivoMasivoCobroBanesco(List<int> ids)
        {
            List<AE_EstadoCuenta> Registros = new List<AE_EstadoCuenta>();
            var RegistrosBD = AE_EstadoCuentaREPO.GetAllRecords();
            foreach (var item in ids)
            {
                AE_EstadoCuenta _item = RegistrosBD.Where(u => u.Id == item).FirstOrDefault();
                Registros.Add(_item);
            }

            try
            {
                //bool win = _GenerarArchivoBANPOL(Registros);
                bool win = _GenerarArchivoBANPOLFINPAGO(Registros);

                if (win)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Operacion realizada de forma correcta!"
                    }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Error Generando el archivo"
                    }, JsonRequestBehavior.DenyGet); ;
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
        }

        public JsonResult GenerarVerificacionMasivoCobroBanesco(List<int> ids)
        {
            List<AE_EstadoCuenta> Registros = new List<AE_EstadoCuenta>();
            var RegistrosBD = AE_EstadoCuentaREPO.GetAllRecords();
            foreach (var item in ids)
            {
                AE_EstadoCuenta _item = RegistrosBD.Where(u => u.Id == item).FirstOrDefault();
                Registros.Add(_item);
            }

            try
            {
                bool win = GenerarValidacionCobroBanesco(Registros);
                if (win)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Operacion realizada de forma correcta!"
                    }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Error Generando el archivo"
                    }, JsonRequestBehavior.DenyGet); ;
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
        }

        public ActionResult List()
        {
            string id = User.Identity.GetUserId();
            Commerce commerce = CURepo.GetAllRecords().Where(u => u.IdAspNetUser == id).FirstOrDefault().Commerce;
            List<AE_Avance> ListAvance = new List<AE_Avance>();
            ListAvance = AE_AvanceREPO.GetAllRecords().Where(u => (u.IdEstatus == 1 || u.IdEstatus == 2 || u.IdEstatus == 4) && u.RifCommerce == commerce.Rif).OrderByDescending(u => u.Id).ToList();

            return View(ListAvance);
        }

        public ActionResult Details(int id)
        {
            List<AE_ValorAccionTR> ListValorAccionTR = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.FechaCreacionRegistro).Take(5).ToList();
            AE_ValorAccionTR Ultimo = ListValorAccionTR.FirstOrDefault();
            ViewBag.RT = Ultimo;

            InstaTransfer.DataAccess.AE_Dolar Dolar = AE_DolarREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).FirstOrDefault();
            ViewBag.Tasa = Dolar.Tasa;

            ViewBag.Tasas = AE_DolarREPO.GetAllRecords().ToList();
            List <AE_Avance> ListAvance = new List<AE_Avance>();
            ListAvance = AE_AvanceREPO.GetAllRecords().Where(u => u.Id == id).ToList();
            List<AE_EstadoCuenta> estadocuenta = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.IdAvance == ListAvance.FirstOrDefault().Id).ToList();
            //AE_Avance _item = AE_AvanceREPO.GetAllRecords().Where(u => u.RifCommerce == ListAvance.FirstOrDefault().RifCommerce).ToList().FirstOrDefault();
            AE_Avance _item = ListAvance.FirstOrDefault();
            List<AE_Archivo> Archivos = AE_ArchivoREPO.GetAllRecords().Where(u => u.IdAE_avance == _item.Id).ToList();
            List<DateTime> Fechas = estadocuenta.Where(u => u.Abono == false).OrderBy(i => i.FechaOperacion).Select(p => p.FechaOperacion).Distinct().ToList();
            List<string> _fechas = new List<string>();
            List<decimal> _montos = new List<decimal>();
            List<decimal> _montobase = new List<decimal>();
            
            decimal interesevigtentes = 0;

            decimal montointereses = _item.Reembolso - _item.Avance;
            decimal porcentajeoperacion = (_item.Reembolso - _item.Avance) * 100 / _item.Avance;
            List<AvanceController.Interes> Interes = new List<Interes>();
            Interes _interes = new Interes();
            _interes.capital = _item.Avance;
            _interes.monto = montointereses;
            DateTime FI;
            if (_item.FechaInicioCobro == null)
            {
                montointereses = _item.Reembolso - _item.Avance;
                Interes.Add(_interes);
            }
            else
            {

                FI = _item.FechaInicioCobro.Value;
                _interes.fecha = FI;
                Interes.Add(_interes);
                if (_item.FechaInicioCobro.Value > DateTime.Now)
                {
                    montointereses = _item.Reembolso - _item.Avance;
                }
                else
                {
                    montointereses = _item.Reembolso - _item.Avance;
                    FI = FI.AddMonths(1);

                    while (FI < DateTime.Now)
                    {
                        List<AE_EstadoCuenta> EstadoCuenta = estadocuenta.Where(u => u.Abono == false && u.SoloUtilidad == false && u.FechaOperacion < FI).ToList();
                        decimal montoactualcapital = _item.Avance - EstadoCuenta.Sum(u => u.Monto);
                        decimal nuevosinteres = montoactualcapital * porcentajeoperacion / 100;
                        Interes item = new Interes();
                        item.capital = montoactualcapital;
                        item.fecha = FI;
                        item.monto = nuevosinteres;
                        Interes.Add(item);
                        FI = FI.AddMonths(1);
                        montointereses = montointereses + nuevosinteres;
                        interesevigtentes = nuevosinteres;

                    }
                    ViewBag.FechaVenceInteres = FI;
                }
            }
            
            DateTime _fechainicio = estadocuenta.First().FechaOperacion;
            if (_fechainicio > DateTime.Now)
            {
                _fechainicio = estadocuenta.Last().FechaOperacion;
            }
            DateTime _fechaFin = new DateTime();

            if (_item.Reembolso == estadocuenta.Where(u => u.Abono == false).Sum(u => u.Monto) || _item.Reembolso < estadocuenta.Where(u => u.Abono == false).Sum(u => u.Monto))
            {
                _fechaFin = estadocuenta.Where(u => u.Monto > 0 && !u.Abono).OrderBy(u => u.FechaOperacion).Last().FechaOperacion.AddDays(1);
            }
            else
            {
                _fechaFin = DateTime.Now.AddDays(1);
            }

            while (_fechainicio <= _fechaFin)
            {
                if (!_fechas.Contains(_fechainicio.ToString("dd/MM/yy")))
                {
                    _fechas.Add(_fechainicio.ToString("dd/MM/yy"));
                    if (_item.Id == 192 || _item.Id == 193 || _item.Id == 198 || _item.Id == 200 || _item.Id == 201 || _item.Id == 203 || _item.Id == 205 || _item.Id > 205)
                    {
                        _montos.Add(decimal.Parse(estadocuenta.Where(u => u.FechaOperacion.ToString("dd/MM/yy") == _fechainicio.ToString("dd/MM/yy") && !u.Abono).Sum(u => u.MontoBs.Value).ToString()));
                    }
                    else
                    {
                        _montos.Add(decimal.Parse(estadocuenta.Where(u => u.FechaOperacion.ToString("dd/MM/yy") == _fechainicio.ToString("dd/MM/yy") && !u.Abono).Sum(u => u.Monto).ToString()));

                    }

                    //_montos.Add(decimal.Parse(estadocuenta.Where(u => u.FechaOperacion.ToString("dd/MM/yy") == _fechainicio.ToString("dd/MM/yy") && !u.Abono).Sum(u => u.Monto).ToString()));

                    _montobase.Add(decimal.Parse(estadocuenta.Where(u => u.FechaOperacion.ToString("dd/MM/yy") == _fechainicio.ToString("dd/MM/yy") && !u.Abono).Sum(u => u.MontoBase == null ? 0 : u.MontoBase.Value).ToString()));
                }
                _fechainicio = _fechainicio.AddDays(1);
            }
            ViewBag.InteresesList = Interes;
            ViewBag.fechas = _fechas.ToArray();
            ViewBag.montos = _montos.ToArray();
            ViewBag.montobase = _montobase.ToArray();
            ViewBag.sugeridointereses = montointereses;
     
        
            decimal totalpagado = estadocuenta.Where(u => u.Abono == false && u.SoloUtilidad == false).Sum(u => u.Monto);
            decimal totalintereses = estadocuenta.Where(u => u.Abono == false && u.SoloUtilidad == true).Sum(u => u.Monto);
            if (totalintereses >= montointereses)
            {
                ViewBag.interesevigtentes = 0;
            }
            else {

                ViewBag.interesevigtentes = interesevigtentes;
                
            }
     

            ViewBag.totalpagado = totalpagado;
            ViewBag.totalintereses = totalintereses;
            decimal porcentaje = 0;
            if (ListAvance.FirstOrDefault().Modalidad)
            {
                porcentaje = (totalpagado / ListAvance.FirstOrDefault().Avance) * 100;
            }
            else
            {
                porcentaje = (totalpagado / ListAvance.FirstOrDefault().Reembolso) * 100;
            }
            //decimal porcentaje = (totalpagado / ListAvance.FirstOrDefault().Reembolso) * 100;
            int _por = int.Parse(Math.Round(porcentaje).ToString());



            ViewBag.porcentaje = _por;
            ViewBag.Archivos = Archivos;
            ViewBag.EstadoCuenta = estadocuenta;
            return View(ListAvance.FirstOrDefault());
        }

        public bool Proceso()
        {

            //InstaTransfer.BLL.Concrete.Repository._connectionString = "";
            InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta> AEmovimientosREPO = new InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta>();
            InstaTransfer.BLL.Concrete.URepository<AE_Propuesta> AE_PropuestaREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_Propuesta>();
            InstaTransfer.BLL.Concrete.URepository<AE_Avance> AE_AvanceREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_Avance>();
            InstaTransfer.BLL.Concrete.URepository<AE_EstadoCuenta> AE_EstadoCuentaREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_EstadoCuenta>();
            InstaTransfer.BLL.Concrete.URepository<AE_MovimientosDebito> AE_MovimientosDebitoREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_MovimientosDebito>();
            //C: \Users\carmelo\Desktop\Archivos\Pendientes
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\carmelo\Desktop\Archivos\Pendientes");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.xls"); //Getting Text files
                                                    //FileInfo[] Files2 = d.GetFiles("*.xlsx"); //Getting Text files

            foreach (FileInfo file in Files)
            {
                List<InstaTransfer.DataAccess.AE_MovimientosDebito> Lista = new List<AE_MovimientosDebito>();
                List<InstaTransfer.DataAccess.AE_Avance> Listavance = new List<AE_Avance>();
                InstaTransfer.DataAccess.AE_Avance _avance = new AE_Avance();
                Application xlApp = new Application();
                if (!file.FullName.Contains('~'))
                {

                    //Create COM Objects. Create a COM object for everything that is referenced

                    Workbook xlWorkbook = xlApp.Workbooks.Open(file.FullName);
                    _Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                    Range xlRange = xlWorksheet.UsedRange;

                    int rowCount = xlRange.Rows.Count;
                    int colCount = 4;
                    string _rif = file.Name.Substring(0, 10);

                    Listavance = AE_AvanceREPO.GetAllRecords().Where(u => u.RifCommerce == _rif && u.IdEstatus == 1).ToList();

                    if (Listavance.Count > 0)
                    {
                        _avance = Listavance.FirstOrDefault();

                        //iterate over the rows and columns and print to the console as it appears in the file
                        //excel is not zero based!!
                        for (int i = 1; i <= rowCount; i++)
                        {
                            InstaTransfer.DataAccess.AE_MovimientosDebito _Movimiento = new AE_MovimientosDebito();

                            bool error = false;
                            if (i > 1)
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
                                            item = item + xlRange.Cells[i, j].Value2.ToString();
                                        }
                                    }
                                    catch
                                    {
                                        error = true;

                                    }

                                }
                                if (!error)
                                {
                                    try
                                    {
                                        var arreglo = item.Split(';');
                                        if (ValidateRow(arreglo[2]))
                                        {
                                            //if (BuscarRegistro(arreglo[2].ToString(), arreglo[1].ToString()))
                                            //{
                                            //string _d = double.Parse(arreglo[0]);
                                            DateTime conv = DateTime.Parse(arreglo[0]);
                                            _Movimiento.Fecha = conv;
                                            _Movimiento.Referencia = arreglo[1].ToString();
                                            _Movimiento.Descripcion = arreglo[2].ToString();
                                            string lote = "";
                                            try
                                            {
                                                lote = getBetween(arreglo[2], "L.", " ");
                                                if (lote == "")
                                                    lote = arreglo[2].Split(' ')[2];
                                            }
                                            catch
                                            {

                                                lote = "001";
                                            }
                                            _Movimiento.Lote = int.Parse(lote);
                                            _Movimiento.Monto = decimal.Parse(arreglo[3].ToString());
                                            //_Movimiento.RifCommerce = _rif;
                                            _Movimiento.FechaRegistro = DateTime.Now;
                                            _Movimiento.Activo = true;
                                            AE_MovimientosDebitoREPO.AddEntity(_Movimiento);
                                            Lista.Add(_Movimiento);
                                            //}
                                        }
                                    }
                                    catch { }

                                }
                            }
                        }
                    }
                }
                if (Lista.Count > 0)
                {
                    //GENERAMOS ESTADOS CUESTA
                    List<AE_EstadoCuenta> estadocuentalist = new List<AE_EstadoCuenta>();
                    List<AE_EstadoCuenta> Cobros = new List<AE_EstadoCuenta>();
                    estadocuentalist = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.IdAvance == _avance.Id).OrderBy(u => u.FechaRegistro).ToList();
                    AE_EstadoCuenta _ultimo = new AE_EstadoCuenta();

                    if (estadocuentalist.Count > 0)
                    {
                        _ultimo = estadocuentalist.FirstOrDefault();

                        decimal saldofinal = _ultimo.SaldoFinal;
                        List<int> distinctLote = Lista.Select(p => p.Lote).Distinct().ToList();

                        foreach (var item in distinctLote)
                        {
                            List<InstaTransfer.DataAccess.AE_MovimientosDebito> _newlistbylote = Lista.Where(u => u.Lote == item).ToList();
                            decimal monto = _newlistbylote.Sum(u => u.Monto);
                            decimal debocobra = (monto * _avance.Porcentaje) / 100;
                            AE_EstadoCuenta estadocuenta = new AE_EstadoCuenta();

                            estadocuenta.Abono = false;
                            estadocuenta.Estatus = 1;
                            estadocuenta.FechaOperacion = _newlistbylote.FirstOrDefault().Fecha;
                            estadocuenta.FechaRegistro = DateTime.Now;
                            estadocuenta.IdAvance = _avance.Id;
                            estadocuenta.Lote = item;
                            estadocuenta.MontoBase = monto;
                            if (debocobra > _avance.MaximoCobro)
                            {
                                estadocuenta.Monto = _avance.MaximoCobro;
                                estadocuenta.SaldoFinal = saldofinal - _avance.MaximoCobro;
                                estadocuenta.SaldoInicial = saldofinal;
                                saldofinal = saldofinal - _avance.MaximoCobro;
                            }
                            else
                            {
                                estadocuenta.Monto = debocobra;
                                estadocuenta.SaldoFinal = saldofinal - debocobra;
                                estadocuenta.SaldoInicial = saldofinal;
                                saldofinal = saldofinal - debocobra;
                            }
                            Cobros.Add(estadocuenta);
                            AE_EstadoCuentaREPO.AddEntity(estadocuenta);
                            AE_EstadoCuentaREPO.SaveChanges();
                            //ASOCIAMOS ESTACUENTA A MOVIMIENTOS DEBITO
                            foreach (var imte2 in Lista.Where(u => u.Lote == item))
                            {
                                imte2.IdAE_EstadoCuenta = estadocuenta.Id;
                            }

                        }

                    }

                    // bool win = GenerarArchivo(Cobros);
                    bool win = true;
                    if (win)
                    {
                        AE_MovimientosDebitoREPO.SaveChanges();
                        xlApp.Workbooks.Close();
                        System.IO.File.Move(file.FullName, @"C:\Users\carmelo\Desktop\Archivos\Procesado\" + file.Name);
                        file.Delete();
                    }
                    else
                    {
                        xlApp.Workbooks.Close();
                        System.IO.File.Move(file.FullName, @"C:\Users\carmelo\Desktop\Archivos\Fallas\" + file.Name);
                    }
                }
            }

            return true;
        }
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
                    if (Descripcion.Contains("L"))
                        return true;
                    else
                        return false;
                }
            }
            return false;

        }

        public bool GenerarArchivo()
        {
            List<AE_EstadoCuenta> Cobros = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.Id == 7314).ToList();
            int cantidadmovimientos = Cobros.Count();
            int comercio = Cobros.First().AE_Avance.Id;
            string fechaarchivo = DateTime.Now.ToString("yyyyMMddhhmmss");
            string rif = Cobros.First().AE_Avance.RifCommerce;
            //datos fijos
            string registro = "HDR";
            string asociado = "BANESCO";
            string editfact = "E";
            string estanadarEditfact = "D  96A";
            string documento = "DIRDEB";
            string produccion = "P";
            string registrodecontrol = registro + asociado.PadRight(15) + editfact + estanadarEditfact + documento + produccion;
            //encabezado
            string tiporegistro = "01";
            string transaccion = "SUB";
            string condicion = "9";
            string id = Cobros.FirstOrDefault().Id.ToString();
            string numeroorden = DateTime.Now.ToString("yyyyMMdd") + id.Substring((id.Length - 2), 2);
            string fecha = DateTime.Now.ToString("yyyyMMddhhmmss");
            string encabezado = tiporegistro + transaccion.PadRight(35) + condicion.PadRight(3) + numeroorden.PadRight(35) + fecha.PadRight(14);

            //registro credito
            string _tipo2 = "02";
            string _recibo = Cobros.First().Id.ToString().PadLeft(8, '0');
            string _rif = "J410066105";
            string ordenante = "FINPAGOS TECNOLOGIA C A";
            decimal cambio = Math.Round(Cobros.Sum(y => y.Monto), 2);
            cambio = cambio * 100;
            string _montoabono = cambio.ToString().Split(',')[0];
            string _moneda = "VEF";
            string _numerocuenta = "01340031870311158436";
            string _swift = "BANSVECA";
            string _fecha = DateTime.Now.ToString("yyyyMMdd");
            string formadepago = "CB";
            string instruordenante = " ";
            string credito = _tipo2 + _recibo.PadRight(30) + _rif.PadRight(17) + ordenante.PadRight(35)
                + _montoabono.PadLeft(15, '0') + _moneda + instruordenante + _numerocuenta.PadRight(35)
                + _swift.PadRight(11) + _fecha + formadepago;

            //debitos
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {
                string tipo = "03";
                string recibo = cobro.Id.ToString().PadLeft(8, '0');
                decimal _cambio = Math.Round(cobro.Monto, 2);
                _cambio = _cambio * 100;
                string montoacobrar = _cambio.ToString().Split(',')[0];
                string moneda = "VEF";
                string numerocuenta = Cobros.FirstOrDefault().AE_Avance.NumeroCuenta;
                string swift = "BANSVECA";
                //string _rif = Cobros.FirstOrDefault().AE_Avance.Commerce.Rif;
                string nombre = Cobros.FirstOrDefault().AE_Avance.Commerce.SocialReasonName.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                string libre = "   ";
                string contrato = rif.Substring((rif.Length - 4), 4);
                string fechavencimiento = "      ";
                string debito = tipo + recibo.PadRight(30)
                    + montoacobrar.PadLeft(15, '0') + moneda + numerocuenta.PadRight(30)
                    + swift.PadRight(11) + rif.PadRight(17) + nombre.PadRight(35)
                    + libre + contrato.PadRight(35) + fechavencimiento;
                _cobros.Add(debito);

            }
            //_cobros
            string[] lines = { registrodecontrol, encabezado, credito };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }


            //totalizador
            string _tipo = "04";
            string totalcreditos = "1";
            string debitos = Cobros.Count().ToString();
            string montototal = cambio.ToString().Split(',')[0];
            string totales = _tipo + totalcreditos.PadLeft(15, '0') + debitos.PadLeft(15, '0') + montototal.PadLeft(15, '0');
            Array.Resize(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = totales;


            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            string ruta = @"C:\Users\carmelo\Desktop\Prueba2.txt";
            System.IO.File.WriteAllLines(ruta, lines);
            return true;
        }

        public bool GenerarArchivoPanama()
        {
            //List<AE_EstadoCuenta> Cobros = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.Id == 839 || u.Id == 840).ToList();
            //Idbeneficiario
            //string rif = "";
            //decimal total = 0;
            //int cantidadmovimientos = Cobros.Count;
            //int comercio = Cobros.First().AE_Avance.Id;
            //string fechaarchivo = DateTime.Now.ToString("yyyyMMddhhmmss");
            //datos fijos

            string registro = "HDR";
            string asociado = "BANESCO";
            string editfact = "E";
            string estanadarEditfact = "D  96A";
            string documento = "DIRDEB";
            string produccion = "P";
            string registrodecontrol = registro + asociado.PadRight(15) + editfact + estanadarEditfact + documento + produccion;
            //encabezado
            //string tiporegistro = "01";
            //string transaccion = "SUB";
            //string condicion = "9";
            //string id = Cobros.FirstOrDefault().Id.ToString();
            //string numeroorden = DateTime.Now.ToString("yyMMdd") + id.Substring((id.Length - 2), 2);
            //string fecha = DateTime.Now.ToString("yyyyMMddhhmmss");
            //string encabezado = tiporegistro + transaccion.PadRight(35) + condicion.PadRight(3) + numeroorden.PadRight(35) + fecha.PadRight(14);



            //debitos
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            //foreach (var cobro in Cobros)
            //{
            string IdBeneficiario = "EMPTYCOMTEST001";
            string NombreBeneficiario = "BFI HOLDINGS VENEZUELA CA";
            int Ruta = int.Parse("58");
            //Ruta = Ruta * 100;
            //total = total + _cambio;
            string _Ruta = Ruta.ToString();/*.Split(',')[0];*/
            string numerocuenta = "201800022656";
            string tipocuenta = "03";
            string transaccion = "D";
            string adenda = "20180508 test1 piloto";
            //string _rif = Cobros.FirstOrDefault().AE_Avance.Commerce.Rif;
            decimal Monto = Math.Round(decimal.Parse("1,00"), 2);
            string _Monto = Monto.ToString().Replace(',', '.').ToString();
            string debito = IdBeneficiario.PadLeft(15) + NombreBeneficiario.Substring(0, 22).PadLeft(22)
                + _Ruta.PadLeft(9, '0') + numerocuenta.PadLeft(17)
                + tipocuenta + _Monto.PadLeft(11, '0') + transaccion + adenda.PadLeft(25);
            _cobros.Add(debito);

            //}

            ////registro credito
            //string _tipo2 = "02";
            //string _recibo = Cobros.First().Id.ToString().PadLeft(8, '0');
            //string _rif = "J401878105";
            //string ordenante = "TECNOLOGIA INSTAPAGO C A";
            ////decimal cambio = Math.Round(Cobros.Sum(y => y.Monto), 2);
            ////cambio = cambio * 100;
            //string _montoabono = total.ToString().Split(',')[0];
            //string _moneda = "VEF";
            //string _numerocuenta = "01340874278741009116";
            //string _swift = "BANSVECA";
            //string _fecha = DateTime.Now.ToString("yyyyMMdd");
            //string formadepago = "CB";
            //string instruordenante = " ";
            //string credito = _tipo2 + _recibo.PadRight(30) + _rif.PadRight(17) + ordenante.PadRight(35)
            //    + _montoabono.PadLeft(15, '0') + _moneda + instruordenante + _numerocuenta.PadRight(35)
            //    + _swift.PadRight(11) + _fecha + formadepago;
            //foreach (var cobro in Cobros)
            //{
            //string tipo = "03";
            //string recibo = Cobros.First().Id.ToString().PadLeft(8, '0');
            //string montoacobrar = cambio.ToString().Split(',')[0];
            //string moneda = "VEF";
            //string numerocuenta = Cobros.FirstOrDefault().AE_Avance.NumeroCuenta;
            //string swift = "BANSVECA";
            //string rif = Cobros.FirstOrDefault().AE_Avance.Commerce.Rif;
            //string nombre = Cobros.FirstOrDefault().AE_Avance.Commerce.SocialReasonName.Replace(',', ' ').Replace('.', ' ');
            //string contrato = rif.Substring(rif.Length - 4, rif.Length);
            //string libre = "   ";
            //string debito = tipo + recibo.PadRight(30)
            //    + montoacobrar.PadLeft(15, '0') + moneda + numerocuenta.PadRight(30)
            //    + swift.PadRight(11) + rif.PadRight(17) + nombre.PadRight(35)
            //    + libre + contrato.PadRight(35);
            //_cobros.Add(debito);

            ////}   


            //_cobros
            string[] lines = { };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }


            ////totalizador
            //string _tipo = "04";
            //string totalcreditos = "1";
            //string debitos = Cobros.Count().ToString();
            //string montototal = total.ToString().Split(',')[0];
            //string totales = _tipo + totalcreditos.PadLeft(15, '0') + debitos.PadLeft(15, '0') + montototal.PadLeft(15, '0');
            Array.Resize(ref lines, lines.Length + 1);
            //lines[lines.Length - 1] = totales;


            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            string ruta = @"C:\Users\carmelo\Desktop\BanescoPanama.txt";
            System.IO.File.WriteAllLines(ruta, lines);

            return true;
        }
        public FileResult lnkfilepath_Click(string ruta) // ur link button 
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(ruta);
            try
            {
                string[] _ruta = ruta.Split((char)92);
                string fileName = _ruta[5];
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch
            {
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "ProblemaNombre");
            }

        }
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult _UpdateConfig(int id, string numerocuenta, string usuario, string clave, string avance, string retorno, string maximo, string FechaInicioCobro, string gastobanco)
        {
            try
            {
                var item = AE_AvanceREPO.GetAllRecords().Where(u => u.Id == id).FirstOrDefault();
                item.NumeroCuenta = numerocuenta;
                item.Usuario = usuario;
                item.Clave = clave;
                item.Avance = decimal.Parse(avance);
                item.MaximoCobro = decimal.Parse(maximo);
                item.Reembolso = decimal.Parse(retorno);
                item.GastoBanco = decimal.Parse(gastobanco);
                if (FechaInicioCobro != null)
                {
                    if (FechaInicioCobro == "")
                    {
                        item.FechaInicioCobro = null;
                    }
                    else
                    {
                        item.FechaInicioCobro = DateTime.Parse(FechaInicioCobro);
                    }
                }
                else
                {
                    item.FechaInicioCobro = null;
                }
                AE_AvanceREPO.SaveChanges();


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
                message = "Cambios actualizados de forma correcta!"
            }, JsonRequestBehavior.DenyGet);

        }

        public JsonResult _UpdateConfigPropuesta(int __id, string __avance1, string __reembolso1, string __maximo1, string __avance2, string __reembolso2, string __maximo2, string __avance3, string __reembolso3, string __maximo3)
        {
            try
            {
                var item = AE_PropuestaREPO.GetAllRecords().Where(u => u.Id == __id).FirstOrDefault();
                item.MaximoCobroOpcion1 = decimal.Parse(__maximo1);
                item.MaximoCobroOpcion2 = decimal.Parse(__maximo2);
                item.MaximoCobroOpcion3 = decimal.Parse(__maximo3);
                item.ReembolsoOpcion1 = decimal.Parse(__reembolso1);
                item.ReembolsoOpcion2 = decimal.Parse(__reembolso2);
                item.ReembolsoOpcion3 = decimal.Parse(__reembolso3);
                item.AvanceOpcion1 = decimal.Parse(__avance1);
                item.AvanceOpcion2 = decimal.Parse(__avance2);
                item.AvanceOpcion3 = decimal.Parse(__avance3);

                AE_PropuestaREPO.SaveChanges();


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
                message = "Cambios actualizados de forma correcta!"
            }, JsonRequestBehavior.DenyGet);

        }

        public JsonResult AddMovimiento(int id, decimal monto, int lote, string fecha, string tasa, bool soloutilidad, bool efectivo , bool moneda)
        {
            InstaTransfer.DataAccess.AE_Avance Avance = AE_AvanceREPO.GetAllRecords().Where(u => u.Id == id).FirstOrDefault();

            if (DateTime.Parse(fecha).ToString("dd/MM/yy") != DateTime.Now.ToString("dd/MM/yy"))
            {
                return Json(new
                {
                    success = false,
                    message = "Fecha mal registrada"
                }, JsonRequestBehavior.DenyGet);
            }

            if (Avance.Modalidad)
            {
                try
                {
                    List<AE_EstadoCuenta> ListaCobros = new List<AE_EstadoCuenta>();
                    decimal _tasa = decimal.Parse(tasa);
                    //InstaTransfer.DataAccess.AE_Dolar Dolar = AE_DolarREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).FirstOrDefault();
                    AE_EstadoCuenta estadocuenta = new AE_EstadoCuenta();
                    estadocuenta.MontoBase = monto;
                    estadocuenta.Abono = false;
                    estadocuenta.Estatus = 1;
                    estadocuenta.FechaOperacion = DateTime.Parse(fecha);
                    estadocuenta.FechaRegistro = DateTime.Now;
                    estadocuenta.IdAvance = id;
                    estadocuenta.Lote = lote;
                    estadocuenta.Monto = monto / _tasa;
                    estadocuenta.MontoBs = monto;
                    estadocuenta.Tasa = _tasa;
                    estadocuenta.SaldoInicial = 77;
                    estadocuenta.SaldoFinal = 77;
                    estadocuenta.EfectivoCambiado = false;
                    estadocuenta.Efectivo = efectivo;
                    estadocuenta.SoloUtilidad = soloutilidad;
                    estadocuenta.RecibidoEnDolares = moneda;
                    AE_EstadoCuentaREPO.AddEntity(estadocuenta);
                    AE_EstadoCuentaREPO.SaveChanges();
                    ListaCobros.Add(estadocuenta);
                    _GenerarArchivo(ListaCobros);

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
            else
            {
                try
                {
                    decimal _tasa = decimal.Parse(tasa);
                    //InstaTransfer.DataAccess.AE_Dolar Dolar = AE_DolarREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).FirstOrDefault();
                    List<AE_EstadoCuenta> ListaCobros = new List<AE_EstadoCuenta>();
                    AE_EstadoCuenta estadocuenta = new AE_EstadoCuenta();
                    estadocuenta.MontoBase = monto;
                    estadocuenta.Abono = false;
                    estadocuenta.Estatus = 1;
                    estadocuenta.FechaOperacion = DateTime.Parse(fecha);
                    estadocuenta.FechaRegistro = DateTime.Now;
                    estadocuenta.IdAvance = id;
                    estadocuenta.Lote = lote;
                    estadocuenta.Monto = monto / _tasa;
                    estadocuenta.MontoBs = monto;
                    estadocuenta.Tasa = _tasa;
                    estadocuenta.SaldoInicial = 77;
                    estadocuenta.SaldoFinal = 77;
                    estadocuenta.EfectivoCambiado = false;
                    estadocuenta.Efectivo = efectivo;
                    estadocuenta.SoloUtilidad = soloutilidad;
                    estadocuenta.RecibidoEnDolares = moneda;
                    AE_EstadoCuentaREPO.AddEntity(estadocuenta);
                    AE_EstadoCuentaREPO.SaveChanges();
                    ListaCobros.Add(estadocuenta);
                    _GenerarArchivo(ListaCobros);

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

        }

        public JsonResult EditMovimiento(int id, decimal monto, int lote, string fecha, string tasa, bool moneda)
        {
            if (DateTime.Parse(fecha).ToString("dd/MM/yy") != DateTime.Now.ToString("dd/MM/yy"))
            {
                return Json(new
                {
                    success = false,
                    message = "Fecha mal registrada"
                }, JsonRequestBehavior.DenyGet);
            }
            AE_EstadoCuenta estadocuenta = AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.Id == id).FirstOrDefault();
            try
            {

                //estadocuenta.MontoBase = monto;
                //estadocuenta.Abono = false;
                //estadocuenta.Estatus = 1;
                //estadocuenta.FechaOperacion = DateTime.Parse(fecha);
                //estadocuenta.FechaRegistro = DateTime.Now;
                //estadocuenta.IdAvance = id;
                estadocuenta.Lote = lote;
                estadocuenta.Monto = monto / decimal.Parse(tasa);
                estadocuenta.MontoBs = monto;
                estadocuenta.SaldoInicial = monto;
                estadocuenta.SaldoFinal = monto;
                estadocuenta.Tasa = decimal.Parse(tasa);
                estadocuenta.RecibidoEnDolares = moneda;
                //AE_EstadoCuentaREPO.AddEntity(estadocuenta);
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
                message = "Movimiento agregado de forma correcta!"
            }, JsonRequestBehavior.DenyGet);

        }

        public JsonResult FinalizarOperacion(int id, string pass)
        {
            if (pass == "10563")
            {
                AE_Avance avance = AE_AvanceREPO.GetAllRecords().Where(u => u.Id == id).FirstOrDefault();
                try
                {

                    avance.IdEstatus = 2;
                    AE_AvanceREPO.SaveChanges();

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
                    message = "Operacion finalizada de forma correcta!"
                }, JsonRequestBehavior.DenyGet);
            }
            else {
                return Json(new
                {
                    success = false,
                    message = "PIN Incorrecto!"
                }, JsonRequestBehavior.DenyGet);

            }
                   
        }

        public bool _GenerarArchivo(List<AE_EstadoCuenta> Cobros)
        {
            int cantidadmovimientos = Cobros.Count();
            int comercio = Cobros.First().AE_Avance.Id;
            string rif = Cobros.First().AE_Avance.RifCommerce;
            string fechaarchivo = DateTime.Now.ToString("yyyyMMddhhmmss");
            //datos fijos
            string registro = "HDR";
            string asociado = "BANESCO";
            string editfact = "E";
            string estanadarEditfact = "D  96A";
            string documento = "DIRDEB";
            string produccion = "P";
            string registrodecontrol = registro + asociado.PadRight(15) + editfact + estanadarEditfact + documento + produccion;
            //encabezado
            string tiporegistro = "01";
            string transaccion = "SUB";
            string condicion = "9";
            string id = Cobros.FirstOrDefault().Id.ToString();
            if (id.Length > 4)
            {
                id = id.Substring((id.Length - 4), 4);
            }
            else if (id.Length < 4)
            {
                id = id.PadLeft(4, '0');
            }
            string numeroorden = DateTime.Now.ToString("yyMMdd");
            numeroorden = numeroorden + id;
            string fecha = DateTime.Now.ToString("yyyyMMddhhmmss");
            string encabezado = tiporegistro + transaccion.PadRight(35) + condicion.PadRight(3) + numeroorden.PadRight(35) + fecha.PadRight(14);
            decimal total = 0;
            //debitos
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {
                string tipo = "03";
                string recibo = cobro.Id.ToString().PadLeft(8, '0');
                decimal _cambio = Math.Round(cobro.MontoBs.Value, 2);
                _cambio = _cambio * 100;
                total = total + _cambio;
                string montoacobrar = _cambio.ToString().Split(',')[0];
                string moneda = "VES";
                string numerocuenta = Cobros.FirstOrDefault().AE_Avance.NumeroCuenta;
                string swift = "BANSVECA";
                //string _rif = Cobros.FirstOrDefault().AE_Avance.Commerce.Rif;
                string nombre = Cobros.FirstOrDefault().AE_Avance.Commerce.SocialReasonName.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                string libre = "   ";
                string contrato = rif.Substring((rif.Length - 4), 4);
                string fechavencimiento = "      ";
                string debito = tipo + recibo.PadRight(30)
                    + montoacobrar.PadLeft(15, '0') + moneda + numerocuenta.PadRight(30)
                    + swift.PadRight(11) + rif.PadRight(17) + nombre.PadRight(35)
                    + libre + contrato.PadRight(35) + fechavencimiento;
                _cobros.Add(debito);

            }

            //registro credito
            string _tipo2 = "02";
            string _recibo = Cobros.First().Id.ToString().PadLeft(8, '0');
            string _rif = "J410066105";
            string ordenante = "FINPAGOS TECNOLOGIA C A";
            //decimal cambio = Math.Round(Cobros.Sum(y => y.Monto), 2);
            //cambio = cambio * 100;
            string _montoabono = total.ToString().Split(',')[0];
            string _moneda = "VES";
            string _numerocuenta = "01340031870311158436";
            string _swift = "BANSVECA";
            string _fecha = DateTime.Now.ToString("yyyyMMdd");
            string formadepago = "CB";
            string instruordenante = " ";
            string credito = _tipo2 + _recibo.PadRight(30) + _rif.PadRight(17) + ordenante.PadRight(35)
                + _montoabono.PadLeft(15, '0') + _moneda + instruordenante + _numerocuenta.PadRight(35)
                + _swift.PadRight(11) + _fecha + formadepago;

            //_cobros
            string[] lines = { registrodecontrol, encabezado, credito };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            //totalizador
            string _tipo = "04";
            string totalcreditos = "1";
            string debitos = Cobros.Count().ToString();
            string montototal = total.ToString().Split(',')[0];
            string totales = _tipo + totalcreditos.PadLeft(15, '0') + debitos.PadLeft(15, '0') + montototal.PadLeft(15, '0');
            Array.Resize(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = totales;


            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            string ruta = ConfigurationManager.AppSettings["RutaCargoCuenta"].ToString() + rif + "_" + comercio + "_" + fechaarchivo + ".txt";
            System.IO.File.WriteAllLines(ruta, lines);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-VE");
            InstaTransfer.BLL.Concrete.URepository<AE_Archivo> archivoREPO = new InstaTransfer.BLL.Concrete.URepository<AE_Archivo>();
            AE_Archivo _nuevo = new AE_Archivo();
            _nuevo.FechaCreacion = DateTime.Now;
            _nuevo.IdAE_avance = Cobros.FirstOrDefault().AE_Avance.Id;
            _nuevo.Monto = Math.Round(Cobros.Sum(y => y.MontoBs.Value), 2);
            _nuevo.FechaEjecucion = DateTime.Now;
            _nuevo.Ruta = ruta;
            _nuevo.Valores = numeroorden;
            _nuevo.ConsultaExitosa = false;
            _nuevo.CorreoSoporteEnviado = false;
            _nuevo.IdAE_ArchivosStatus = 1;
            _nuevo.StatusChangeDate = DateTime.Now;
            _nuevo.RutaRespuesta = "nada - escribo servicio COBRO DIARIO";
            archivoREPO.AddEntity(_nuevo);
            archivoREPO.SaveChanges();

            return true;
        }

        public bool _GenerarArchivoBANPOL(List<AE_EstadoCuenta> Cobros)
        {
            int cantidadmovimientos = Cobros.Count();
            //string comercio = Cobros.First().AE_Avance.Id;
            string rif = Cobros.First().AE_Avance.RifCommerce;
            string fechaarchivo = DateTime.Now.AddHours(14).ToString("ddMMyy.hhmm");
            string id = Cobros.FirstOrDefault().Id.ToString();
            if (id.Length > 4)
            {
                id = id.Substring((id.Length - 4), 4);
            }
            else if (id.Length < 4)
            {
                id = id.PadLeft(4, '0');
            }
            string numeroorden = DateTime.Now.AddHours(14).ToString("yyMMdd");
            string _fecha = DateTime.Now.AddHours(14).ToString("yyyyMMdd");
            numeroorden = numeroorden + id;
            //datos fijos
            string registro = "00";
            //string idtransaccion = "2020040801";
            string asociado = "208515428";
            string ordencobroreferencia = numeroorden;
            string documento = "DIRDEB";
            string banco = "01";
            string fecha = DateTime.Now.AddHours(14).ToString("yyyyMMddhhmmss");
            string registrodecontrol = registro + asociado.PadRight(35) + ordencobroreferencia.PadRight(30) + documento + fecha.PadRight(14) + banco;
            //encabezado
            string tiporegistro = "01";
            string transaccion = "DMI";
            string condicion = "9";


            //string fecha = DateTime.Now.ToString("yyyyMMddhhmmss");
            string encabezado = tiporegistro + transaccion.PadRight(35) + condicion.PadRight(3) + ordencobroreferencia.PadRight(35) + _fecha;

            decimal total = 0;
            //debitos
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {
                string tipo = "03";
                string recibo = cobro.Id.ToString().PadLeft(8, '0');
                decimal _cambio = Math.Round(cobro.Monto, 2);
                _cambio = _cambio * 100;
                total = total + _cambio;
                string montoacobrar = _cambio.ToString().Split(',')[0];
                string moneda = "VES";
                string numerocuenta = Cobros.FirstOrDefault().AE_Avance.NumeroCuenta;
                string swift = "UNIOVECA";
                //string _rif = Cobros.FirstOrDefault().AE_Avance.Commerce.Rif;
                string nombre = Cobros.FirstOrDefault().AE_Avance.Commerce.SocialReasonName.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                string libre = "423";
                string contrato = rif;
                string fechavencimiento = "      ";
                string debito = tipo + recibo.PadRight(30)
                    + montoacobrar.PadLeft(15, '0') + moneda + numerocuenta.PadRight(30)
                    + swift.PadRight(11) + rif.PadRight(17) + nombre.PadRight(35)
                    + libre + contrato.PadRight(35) + fechavencimiento;
                _cobros.Add(debito);

            }

            //registro credito
            string _tipo2 = "02";
            string _recibo = Cobros.First().Id.ToString().PadLeft(8, '0');
            string _rif = "J401878105";
            string ordenante = "TECNOLOGIA INSTAPAGO C A";
            string _montoabono = total.ToString().Split(',')[0];
            string _moneda = "VES";
            string _numerocuenta = "01340031810311158627";
            string _swift = "UNIOVECA";
            //string _fecha = DateTime.Now.ToString("yyyyMMdd");
            string formadepago = "423";
            string instruordenante = " ";
            string credito = _tipo2 + _recibo.PadRight(30) + _rif.PadRight(17) + ordenante.PadRight(35)
                + _montoabono.PadLeft(15, '0') + _moneda + instruordenante + _numerocuenta.PadRight(35)
                + _swift.PadRight(11) + _fecha + formadepago;

            //_cobros
            string[] lines = { registrodecontrol, encabezado, credito };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            //totalizador
            string _tipo = "04";
            string totalcreditos = "1";
            string debitos = Cobros.Count().ToString();
            string montototal = total.ToString().Split(',')[0];
            string totales = _tipo + totalcreditos.PadLeft(15, '0') + debitos.PadLeft(15, '0') + montototal.PadLeft(15, '0');
            Array.Resize(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = totales;


            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            //string ruta = ConfigurationManager.AppSettings["RutaCargoCuenta"].ToString() + rif + "_" + comercio + "_" + fechaarchivo + ".txt";
            string ruta = @"C:\Users\carmelo\Desktop\POLAR\CargoCuentaBanesco\" + "I0005.208515428." + fechaarchivo + ".txt";
            System.IO.File.WriteAllLines(ruta, lines);


            CP_Archivo archivo = new CP_Archivo();
            archivo.IdEmpresa = 1;
            archivo.Nombre = "I0005.208515428." + fechaarchivo + ".txt";
            archivo.Ruta = ruta;
            archivo.Tipo = 1;
            string contenido = "";
            foreach (var item in lines)
            {
                contenido = item + "</br>";
            }
            archivo.Contenido = contenido;
            archivo.Registro = cantidadmovimientos;
            archivo.FechaLectura = DateTime.Now;
            archivo.FechaCreacion = DateTime.Now;
            archivo.Descripcion = "[CARGO EN CUENTA] Cargo cuenta masivo de la empresa Fin Pagos.";
            archivo.IdCP_Archivo = null;
            archivo.ReferenciaOrigen = "Estado de cuenta operaciones de prestamos";
            CP_ArchivoREPO.AddEntity(archivo);
            CP_ArchivoREPO.SaveChanges();

            return true;
        }

        public bool _GenerarArchivoBANPOLFINPAGO(List<AE_EstadoCuenta> Cobros)
        {
            int cantidadmovimientos = Cobros.Count();
            //string comercio = Cobros.First().AE_Avance.Id;
            //string rif = Cobros.First().AE_Avance.RifCommerce;
            string fechaarchivo = DateTime.Now.AddDays(0).ToString("ddMMyy.hhmm");
            string id = Cobros.FirstOrDefault().Id.ToString();
            if (id.Length > 4)
            {
                id = id.Substring((id.Length - 4), 4);
            }
            else if (id.Length < 4)
            {
                id = id.PadLeft(4, '0');
            }
            string numeroorden = DateTime.Now.AddDays(0).ToString("yyMMdd");
            string _fecha = DateTime.Now.AddDays(0).ToString("yyyyMMdd");
            numeroorden = numeroorden + id;
            //datos fijos
            string registro = "00";
            //string idtransaccion = "2020040801";
            string asociado = "210374095";
            string ordencobroreferencia = numeroorden;
            string documento = "DIRDEB";
            string banco = "01";
            string fecha = DateTime.Now.AddDays(0).ToString("yyyyMMddhhmmss");
            string registrodecontrol = registro + asociado.PadRight(35) + ordencobroreferencia.PadRight(30) + documento + fecha.PadRight(14) + banco;
            //encabezado
            string tiporegistro = "01";
            string transaccion = "DMI";
            string condicion = "9";
            //string fecha = DateTime.Now.ToString("yyyyMMddhhmmss");
            string encabezado = tiporegistro + transaccion.PadRight(35) + condicion.PadRight(3) + ordencobroreferencia.PadRight(35) + _fecha;

            decimal total = 0;
            //debitos
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {
                string rif = cobro.AE_Avance.RifCommerce.ToUpper();
                string tipo = "03";
                string recibo = cobro.Id.ToString().PadLeft(8, '0');
                decimal _cambio = Math.Round(cobro.MontoBs.Value, 2);
                _cambio = _cambio * 100;
                total = total + _cambio;
                string montoacobrar = _cambio.ToString().Split(',')[0];
                string moneda = "VES";
                string numerocuenta = cobro.AE_Avance.NumeroCuenta;
                string swift = "UNIOVECA";
                //string _rif = Cobros.FirstOrDefault().AE_Avance.Commerce.Rif;
                string nombre = cobro.AE_Avance.Commerce.SocialReasonName.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                string libre = "423";
                string contrato = rif;
                string fechavencimiento = "      ";
                string debito = tipo + recibo.PadRight(30)
                    + montoacobrar.PadLeft(15, '0') + moneda + numerocuenta.PadRight(30)
                    + swift.PadRight(11) + rif.PadRight(17) + nombre.PadRight(35)
                    + libre + contrato.PadRight(35) + fechavencimiento;
                _cobros.Add(debito);

            }

            //registro credito
            string _tipo2 = "02";
            string _recibo = Cobros.First().Id.ToString().PadLeft(8, '0');
            string _rif = "J410066105";
            string ordenante = "FINPAGOS TECNOLOGIA C A";
            //decimal cambio = Math.Round(Cobros.Sum(y => y.Monto), 2);
            //cambio = cambio * 100;
            string _montoabono = total.ToString().Split(',')[0];
            string _moneda = "VES";
            string _numerocuenta = "01340031870311158436";
            string _swift = "UNIOVECA";
            //string _fecha = DateTime.Now.ToString("yyyyMMdd");
            string formadepago = "423";
            string instruordenante = " ";
            string credito = _tipo2 + _recibo.PadRight(30) + _rif.PadRight(17) + ordenante.PadRight(35)
                + _montoabono.PadLeft(15, '0') + _moneda + instruordenante + _numerocuenta.PadRight(35)
                + _swift.PadRight(11) + _fecha + formadepago;

            //_cobros
            string[] lines = { registrodecontrol, encabezado, credito };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            //totalizador
            string _tipo = "04";
            string totalcreditos = "1";
            string debitos = Cobros.Count().ToString();
            string montototal = total.ToString().Split(',')[0];
            string totales = _tipo + totalcreditos.PadLeft(15, '0') + debitos.PadLeft(15, '0') + montototal.PadLeft(15, '0');
            Array.Resize(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = totales;

            string ruta = @"C:\Apps\Transax\Repo\MasivoBanesco\" + "I0005." + asociado + "." + fechaarchivo + ".txt";
            System.IO.File.WriteAllLines(ruta, lines);

            CP_Archivo archivo = new CP_Archivo();
            archivo.IdEmpresa = 1;
            archivo.Nombre = "I0005." + asociado + "." + fechaarchivo + ".txt";
            archivo.Ruta = ruta;
            archivo.Tipo = 1;
            string contenido = "";
            foreach (var item in lines)
            {
                contenido = contenido + item + "</br>";
            }
            archivo.Contenido = contenido;
            archivo.Registro = cantidadmovimientos;
            archivo.FechaLectura = DateTime.Now;
            archivo.FechaCreacion = DateTime.Now;
            archivo.Descripcion = "[CARGO EN CUENTA] Cargo cuenta masivo de la empresa Fin Pagos.";
            archivo.IdCP_Archivo = null;
            Guid x = Guid.NewGuid();
            archivo.IdReferencia = x;
            archivo.EsRespuesta = false;
            archivo.ReferenciaOrigen = "Estado de cuenta operaciones de prestamos";
            archivo.ReferenciaArchivoBanco = ordencobroreferencia;
            CP_ArchivoREPO.AddEntity(archivo);
            CP_ArchivoREPO.SaveChanges();

            return true;
        }

        public bool GenerarValidacionCobroBanesco(List<AE_EstadoCuenta> Cobros)
        {
            int cantidadmovimientos = Cobros.Count();
            string fechaarchivo = DateTime.Now.AddDays(0).ToString("ddMMyy.hhmm");

            string _fecha = DateTime.Now.AddDays(0).ToString("yyyyMMdd");
            //datos fijos
            string registro = "00";
            //BUSCAR RIF BASE DE DATOS
            string rif = "J401878105";
            string Filler = "                          ";
            string Referencia = DateTime.Now.AddDays(0).ToString("yyyyMMddhhmm"); ;
            string documento = "AFILIA";
            string registrodecontrol = registro + rif + Filler + Referencia.PadRight(30) + documento;

            int k = 0;
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {
                //CP_Beneficiario beneficiario = CP_Beneficiario.GetAllRecords().Where(u => u.CodigoCliente == cobro.CodigoComercio).FirstOrDefault();

                //if (beneficiario != null && beneficiario.Id > 0)
                //{
                //string _tipodocumento = cobro.AE_Avance.RifCommerce.Substring(0,1);
                string _documento = cobro.AE_Avance.RifCommerce.PadLeft(10, '0');
                string digito = "0";
                string tipocuenta = "01";
                string numerocuenta = Cobros.FirstOrDefault().AE_Avance.NumeroCuenta;
                string nombre = cobro.AE_Avance.Commerce.SocialReasonName.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                string _referencia = cobro.Id.ToString().PadLeft(8, '0');
                string debito = _documento + digito + tipocuenta + numerocuenta.PadLeft(35, '0') + nombre.PadRight(59) + _referencia;
                _cobros.Add(debito);
                //}
                //else
                //{
                //if (k == 0)
                //{
                //    _cuenta = "01340373233733019371";
                //    _nombrecomercial = "Carmelo Larez";
                //    _rifc = "V018601098";
                //}
                //else if (k == 1)
                //{
                //    _cuenta = "01340874278743016046";
                //    _nombrecomercial = "Alexyomar Istruriz";
                //    _rifc = "V017302339";
                //}
                //else
                //{
                //    _cuenta = "01340373233733019371";
                //    _nombrecomercial = "Carmelo Larez";
                //    _rifc = "V018601098";

                //}
                //k++;
                //}

            }

            string[] lines = { registrodecontrol };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            string ruta = @"C:\Users\carmelo\Desktop\POLAR\CargoCuentaVerificacionBanesco\" + "afilia.txt." + fechaarchivo;
            System.IO.File.WriteAllLines(ruta, lines);

            CP_Archivo archivo = new CP_Archivo();
            archivo.IdEmpresa = 1;
            archivo.Nombre = "afilia.txt";
            archivo.Ruta = ruta;
            archivo.Tipo = 2;
            string contenido = "";
            foreach (var item in lines)
            {
                contenido = item + "</br>";
            }
            archivo.Contenido = contenido;
            archivo.Registro = cantidadmovimientos;
            archivo.FechaLectura = DateTime.Now;
            archivo.FechaCreacion = DateTime.Now;
            archivo.Descripcion = "[VERIFICACIÓN] verificacón de cobros la empresa Fin Pagos.";
            archivo.IdCP_Archivo = null;
            archivo.ReferenciaOrigen = "Estado de cuenta operaciones de prestamos";
            CP_ArchivoREPO.AddEntity(archivo);
            CP_ArchivoREPO.SaveChanges();

            //Thread.CurrentThread.CurrentCulture = new CultureInfo("es-VE");
            //InstaTransfer.BLL.Concrete.URepository<AE_Archivo> archivoREPO = new InstaTransfer.BLL.Concrete.URepository<AE_Archivo>();
            //AE_Archivo _nuevo = new AE_Archivo();
            //_nuevo.FechaCreacion = DateTime.Now;
            //_nuevo.IdAE_avance = Cobros.FirstOrDefault().AE_Avance.Id;
            //_nuevo.Monto = Math.Round(Cobros.Sum(y => y.Monto), 2);
            //_nuevo.FechaEjecucion = DateTime.Now;
            //_nuevo.Ruta = ruta;
            //_nuevo.Valores = numeroorden;
            //_nuevo.ConsultaExitosa = false;
            //_nuevo.CorreoSoporteEnviado = false;
            //_nuevo.IdAE_ArchivosStatus = 1;
            //_nuevo.StatusChangeDate = DateTime.Now;
            //_nuevo.RutaRespuesta = "nada - escribo servicio COBRO DIARIO";
            //archivoREPO.AddEntity(_nuevo);
            //archivoREPO.SaveChanges();

            return true;
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult EnviarArchivoBanco(int id)
        {
            CP_Archivo archivo = CP_ArchivoREPO.GetAllRecords().Where(u => u.Id == id).FirstOrDefault();
            try
            {
                FileUploadSFTP(archivo.Ruta);
                //archivo.IdEstatus = 2;
                //CP_ArchivoREPO.SaveChanges();

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
                message = "Operacion finalizada de forma correcta!"
            }, JsonRequestBehavior.DenyGet);

        }

        public void FileUploadSFTP(string uploadfile)
        {
            var host = "10.148.174.215";
            var port = 5522;
            var username = "instapag";
            var password = "540144017";
            var workingdirectory = "/INtoAS400p";
            // path for file you want to upload 
            // path for file you want to upload 
            var uploadFile = uploadfile;

            using (var client = new SftpClient(host, port, username, password))
            {
                client.Connect();
                if (client.IsConnected)
                {
                    //Debug.WriteLine("I'm connected to the client");
                    if (client.Exists(client.WorkingDirectory + workingdirectory))
                    {
                        client.ChangeDirectory(client.WorkingDirectory +  workingdirectory);
                        using (var fileStream = new FileStream(uploadFile, FileMode.Open))
                        {

                            client.BufferSize = 4 * 1024; // bypass Payload error large files
                            client.UploadFile(fileStream, Path.GetFileName(uploadFile));
                        }
                    }
                }
                else
                {
                    //Debug.WriteLine("I couldn't connect");
                }
            }
        }


        public class Interes
        {
            public decimal monto { get; set; }
            public DateTime fecha { get; set; }
            public decimal capital { get; set; }
        }

    }
}