using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using System.Configuration;
using Renci.SshNet;
using System.Globalization;
using System.Threading;
using FileHelpers;

namespace ProcesoInventarioPAG
{
    public class Program
    {
        URepository<CP_Archivo> ArchivoREPO = new URepository<CP_Archivo>();
        URepository<CP_ArchivoEstadoCuenta> EstadoCuentaREPO = new URepository<CP_ArchivoEstadoCuenta>();
        URepository<CP_Archivo> CP_ArchivoREPO = new URepository<CP_Archivo>();
        URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
        URepository<CP_ArchivoItem> CP_ArchivoItemRepo = new URepository<CP_ArchivoItem>();


        public bool COB_ValidacionArchivoSalidaBanesco()
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;
            Thread.CurrentThread.CurrentCulture = __newCulture;
            string RUTAOUTCOB = ConfigurationManager.AppSettings["rutaLecturaSalidaBanescoR"].ToString();
            DirectoryInfo d = new DirectoryInfo(RUTAOUTCOB);
            //Assuming Test is your Folder
            string RUTAOUTPAG = ConfigurationManager.AppSettings["rutaSalidaPAG"].ToString();
            string RUTABACKCOBROS = ConfigurationManager.AppSettings["rutaBackUpCobros"].ToString();
            string rutafinal = RUTAOUTPAG;
            FileInfo[] Files = d.GetFiles(); //Getting Text files

            List<string> ArchivosPendientes = new List<string>();
            List<string> ArchivosRepsuesta = new List<string>();
            List<string> ArchivosEnviados = new List<string>();
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("_O0002"))
                {
                    string departamento = "";
                    string ordenante = "";
                    string _numerocuenta = "";
                    if (file.Name.Contains("540132787"))
                    {
                        departamento = "540132787";
                        ordenante = "EFE C A";
                        _numerocuenta = "01340850598503004455";
                        //Empresas.Add(departamento);
                    }
                    else if (file.Name.Contains("205903844"))
                    {
                        departamento = "205903844";
                        ordenante = "PEPSI C A";
                        _numerocuenta = "01340850598503004195";
                        //Empresas.Add(departamento);
                    }
                    else if (file.Name.Contains("540133497"))
                    {
                        departamento = "540133497";
                        ordenante = "ALIMENTOS POLAR C A";
                        _numerocuenta = "01340375913751013514";
                        //Empresas.Add(departamento);

                    }
                    else if (file.Name.Contains("540130908"))
                    {
                        departamento = "540130908";
                        ordenante = "CERVECERIA C A";
                        _numerocuenta = "01340850598503004357";
                        //Empresas.Add(departamento);
                    }
                    //item.Nombre = file.Name;
                    //item.Ruta = file.FullName;
                    //item.FechaLectura = DateTime.Now;
                    string lineas = "";
                    int i = 0;
                    string text = System.IO.File.ReadAllText(file.FullName);
                    //System.Console.WriteLine("Contents of WriteText.txt = {0}", text);
                    // Example #2
                    // Read each line of the file into a string array. Each element
                    // of the array is one line of the file.
                    string[] lines = System.IO.File.ReadAllLines(file.FullName);
                    // Display the file contents by using a foreach loop.
                    //System.Console.WriteLine("Contents of WriteLines2.txt = ");
                    //_cobros
                    string[] linesArchivo = { };
                    CP_ArchivoEstadoCuenta elemento = new CP_ArchivoEstadoCuenta();
                    CP_INI cp_ini = new CP_INI();
                    //string IDarchivo = "";
                    EstructuraSalidaBanescoDetalle Registrolinea2 = new EstructuraSalidaBanescoDetalle();

                    foreach (string line in lines)
                    {
                        //if (i == 2)
                        //{
                        lineas = lineas + line + "<br />";
                        string sep = "\t";


                        string tipo = line.Substring(16, 2).ToString().TrimEnd();

                        if (tipo == "01")
                        {

                            EstructuraSalidaBanescoEncabezado Registro = new EstructuraSalidaBanescoEncabezado();
                            Registro.Trading = line.Substring(0, 15).ToString().TrimEnd();
                            Registro.Filler = line.Substring(15, 2).ToString().TrimEnd();
                            Registro.TipoRegistro = line.Substring(16, 2).ToString().TrimEnd();
                            Registro.__NumeroReferenciaRespuesta = line.Substring(19, 10).ToString().TrimEnd();

                            ArchivosRepsuesta.Add(Registro.__NumeroReferenciaRespuesta + " - Empresa :" + departamento);
                            //IDarchivo = line.Substring(19, 10).ToString().TrimEnd();
                            //Registro.__FechaRespuesta = line.Substring(54, 67).ToString().TrimEnd();
                            //Registro.__NumeroReferenciaOrdenPago = line.Substring(68, 102).ToString().TrimEnd();
                            //Registro.__TipoOrdenPago = line.Substring(103, 105).ToString().TrimEnd();
                            //Registro.__CodigoBancoEmisor = line.Substring(106, 116).ToString().TrimEnd();
                            //Registro.__NombreBancoEmisor = line.Substring(117, 186).ToString().TrimEnd();
                            //Registro.__CodgioEmpresaReceptoraBansta = line.Substring(187, 203).ToString().TrimEnd();
                            //Registro.__DescripcionEmpresaReceptoraBansta = line.Substring(204, 273).ToString().TrimEnd();

                            //string _line = Registro.__NumeroReferenciaOrdenPago + "|1|";

                            //Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                            //linesArchivo[linesArchivo.Length - 1] = _line;

                            //   getCP = ArchivoREPO.GetAllRecords().Where(u => u.ReferenciaArchivoBanco == Registro.__NumeroReferenciaRespuesta).FirstOrDefault();
                            // ItemsArchivo = EstadoCuentaREPO.GetAllRecords().Where(u => u.ArchivoLecturaPolar == getCP.Id).ToList();

                        }
                
                        i++;
                    }
                }
                else
                {
                    //file.MoveTo(rutafinal + file.Name + ".txt");
                }
            }

            string RUTABACKCOB = ConfigurationManager.AppSettings["rutaLecturaBackupCobrosBanescoR"].ToString();
            DirectoryInfo e = new DirectoryInfo(RUTABACKCOB);
            FileInfo[] _Files = e.GetFiles().Where(u => u.LastWriteTime > DateTime.Now.AddHours(-(int.Parse(DateTime.Now.Hour.ToString())))).ToArray();

            foreach (var file in _Files)
            {
                if (file.Name.Contains("I0005"))
                {
                    string departamento = "";
                    string ordenante = "";
                    string _numerocuenta = "";
                    if (file.Name.Contains("540132787") )
                    {
                        departamento = "540132787";
                        ordenante = "EFE C A";
                        _numerocuenta = "01340850598503004455";
                        //Empresas.Add(departamento);
                    }
                    else if (file.Name.Contains("205903844"))
                    {
                        departamento = "205903844";
                        ordenante = "PEPSI C A";
                        _numerocuenta = "01340850598503004195";
                        //Empresas.Add(departamento);
                    }
                    else if (file.Name.Contains("540133497"))
                    {
                        departamento = "540133497";
                        ordenante = "ALIMENTOS POLAR C A";
                        _numerocuenta = "01340375913751013514";
                        //Empresas.Add(departamento);

                    }
                    else if (file.Name.Contains("540130908"))
                    {
                        departamento = "540130908";
                        ordenante = "CERVECERIA C A";
                        _numerocuenta = "01340850598503004357";
                        //Empresas.Add(departamento);
                    }
                    //item.Nombre = file.Name;
                    //item.Ruta = file.FullName;
                    //item.FechaLectura = DateTime.Now;
                    string lineas = "";
                    int i = 0;
                    string text = System.IO.File.ReadAllText(file.FullName);
                    //System.Console.WriteLine("Contents of WriteText.txt = {0}", text);
                    // Example #2
                    // Read each line of the file into a string array. Each element
                    // of the array is one line of the file.
                    string[] lines = System.IO.File.ReadAllLines(file.FullName);
                    // Display the file contents by using a foreach loop.
                    //System.Console.WriteLine("Contents of WriteLines2.txt = ");
                    //_cobros
                    string[] linesArchivo = { };
                    CP_ArchivoEstadoCuenta elemento = new CP_ArchivoEstadoCuenta();
                    CP_INI cp_ini = new CP_INI();
                    //string IDarchivo = "";
                    EstructuraSalidaBanescoDetalle Registrolinea2 = new EstructuraSalidaBanescoDetalle();

                    foreach (string line in lines)
                    {
                        //if (i == 2)
                        //{
                        lineas = lineas + line + "<br />";
                        string sep = "\t";
                        if (i == 0)
                        {

                            EstructuraSalidaBanescoEncabezado Registro = new EstructuraSalidaBanescoEncabezado();
                            Registro.__NumeroReferenciaRespuesta = line.Substring(37, 10).ToString().TrimEnd();
                            ArchivosEnviados.Add(Registro.__NumeroReferenciaRespuesta + " - Empresa :" + departamento);
                            break;

                        }
                     
                        i++;
                    }

                }
            }

            Console.WriteLine("Archivos Enviados");
            string valores = "";
            foreach (var item in ArchivosEnviados.OrderBy(q => q).ToList())
            {
                Console.WriteLine(item.ToString() + "\n");
                valores = valores + item.ToString() + Environment.NewLine;
            }
            Console.WriteLine("Respuesta Recibidas");
            valores =  valores + "respuesta" + Environment.NewLine;
            foreach (var item in ArchivosRepsuesta.OrderBy(q => q).ToList())
            {
                Console.WriteLine(item.ToString() + "\n");
                valores = valores + item.ToString() + Environment.NewLine;
            }
            Console.WriteLine("Pendientes:");
            valores = valores + "Pendientes" + Environment.NewLine;
            foreach (var item in ArchivosEnviados)
            {
                if (!ArchivosRepsuesta.Contains(item))
                {
                    Console.WriteLine(item.ToString() + "\n");
                    valores = valores + item.ToString() + Environment.NewLine;
                    ArchivosPendientes.Add(item.ToString());
                }
            }

            string RUTALOGS = @"D:\AppsTest\DebugVALIDADOR\" + "LOGS-VALIDADOR" + DateTime.Now.ToString("dd-MM-yy-ss-mm") + ".txt";
            System.IO.File.WriteAllText(RUTALOGS, valores.ToString());
            

            return true;
        }

        public class PAG
        {
            public string departamento { get; set; }
            public PAGLinea1 Linea1 { get; set; }
            public List<PAGLinea2> Linea2 { get; set; }
            public PAGLinea3 Linea3 { get; set; }
        }
        public class PAGLinea1
        {
            public string ReferenciaPago { get; set; }

            public string NumeroRegistro { get; set; }

            public string CodigoCliente { get; set; }

            public string TipoPagador { get; set; }


            public string IdentificacionPagador { get; set; }

            public string RazonSocial { get; set; }
            public string EstatusPago { get; set; }
            public string CuentaCliente { get; set; }

        }
        public class PAGLinea2
        {
            public string Referenciapago { get; set; }

            public string NumeroRegistro { get; set; }
            public string NumeroDocumento { get; set; }

            public string TipoDocumento { get; set; }
            public string Referencia { get; set; }

            public string FechaVencimiento { get; set; }
            public string FechaPago { get; set; }
            public string MontoIva { get; set; }
            public string MontoRetencion { get; set; }

            public string MontoNeto { get; set; }
            public string NumeroComprobanteIR { get; set; }
            public string FechaEmisionComporbanteIR { get; set; }


        }
        public class PAGLinea3
        {
            public string Referenciapago { get; set; }

            public string NumeroRegistro { get; set; }
            public string CodigoDepartamento { get; set; }

            public string TipoTransaccion { get; set; }
            public string NumerocCuenta { get; set; }

            public string Subtotal { get; set; }
            public string Totaldebito { get; set; }
            public string TotalCargootrosBancos { get; set; }
            public string TotaltarjetaCredito { get; set; }

            public string SubtotalCheques { get; set; }
            public string SubtotalEfectivo { get; set; }
            public string TotalDeposito { get; set; }


        }

        [DelimitedRecord("|")]
        public class _EstructuraCLI
        {
            [FieldOptional]
            [FieldOrder(1)]
            public string CodigoCliente { get; set; }
            [FieldOptional]
            [FieldOrder(2)]
            public string CodigoMaster { get; set; }
            [FieldOptional]
            [FieldOrder(3)]
            public string PermisoMaster { get; set; }
            [FieldOptional]
            [FieldOrder(4)]
            public string Nombre { get; set; }
            [FieldOptional]
            [FieldOrder(5)]
            public string RazonSocial { get; set; }
            [FieldOptional]
            [FieldOrder(6)]
            public string IdentificacionPagador { get; set; }
            [FieldOptional]
            [FieldOrder(7)]
            public string Region { get; set; }
            [FieldOptional]
            [FieldOrder(8)]
            public string Correo { get; set; }
            [FieldOptional]
            [FieldOrder(9)]
            public string Telefono { get; set; }
            [FieldOptional]
            [FieldOrder(10)]
            public string TipoIdentificacion { get; set; }

            [FieldOptional]
            [FieldOrder(11)]
            public string CodigoEstatus { get; set; }
            [FieldOptional]
            [FieldOrder(12)]
            public string IRIVA { get; set; }

            [FieldOptional]
            [FieldOrder(13)]
            public string CodigoDepartamento { get; set; }
            //[FieldOptional]
            //[FieldOrder(14)]
            //public string DescripcionRechazo { get; set; }
        }
        [DelimitedRecord("|")]
        public class _EstructuraCOB
        {
            [FieldOptional]
            [FieldOrder(1)]
            public string CodigoCliente { get; set; }
            [FieldOptional]
            [FieldOrder(2)]
            public string DocumentoComercial { get; set; }
            [FieldOptional]
            [FieldOrder(3)]
            public string Referencia { get; set; }
            [FieldOptional]
            [FieldOrder(4)]
            public string CodigoDepartamento { get; set; }
            [FieldOptional]
            [FieldOrder(5)]
            public string Monto { get; set; }
            [FieldOptional]
            [FieldOrder(6)]
            public string FechaEmision { get; set; }
            [FieldOptional]
            [FieldOrder(7)]
            public string Fechavencimiento { get; set; }
            [FieldOptional]
            [FieldOrder(8)]
            public string TipoDocumento { get; set; }
            [FieldOptional]
            [FieldOrder(9)]
            public string x { get; set; }
            [FieldOptional]
            [FieldOrder(10)]
            public string MontoIva { get; set; }

            [FieldOptional]
            [FieldOrder(11)]
            public string Concepto { get; set; }
            [FieldOptional]
            [FieldOrder(12)]
            public string OBS { get; set; }

            [FieldOptional]
            [FieldOrder(13)]
            public string DescripcionError { get; set; }
            [FieldOptional]
            [FieldOrder(14)]
            public string CodigoError { get; set; }
        }
        public class EstructuraCOB
        {

            public string CodigoCliente { get; set; }
            public string DocumentoComercial { get; set; }
            public string Referencia { get; set; }

            public string CodigoDepartamento { get; set; }
            public string Monto { get; set; }

            public string FechaEmision { get; set; }

            public string Fechavencimiento { get; set; }

            public string TipoDocumento { get; set; }

            public string x { get; set; }

            public string MontoIva { get; set; }

            public string Concepto { get; set; }

            public string OBS { get; set; }

            public string DescripcionError { get; set; }

            public string CodigoError { get; set; }
        }
        public class EstructuraSalidaBanescoDetalle
        {
            public string Trading { get; set; }
            public string Filler { get; set; }
            public string TipoRegistro { get; set; }
            public string Indicador { get; set; }
            public string NumeroReferencia { get; set; }
            public string Fecha { get; set; }
            public string Monto { get; set; }
            public string Moneda { get; set; }
            public string Rif { get; set; }
            public string NumeroCuenta { get; set; }
            public string BancoBeneficiario { get; set; }
            public string BancoBeneficiarioDescripcion { get; set; }
            public string CodigoAgencia { get; set; }
            public string NombreBeneficiario { get; set; }
            public string NumeroCliente { get; set; }
            public string FechaVencimiento { get; set; }
            public string NumeroSecuenciaArchivo { get; set; }
        }
        public class EstructuraSalidaBanescoEstatus
        {
            public string Trading { get; set; }
            public string Filler { get; set; }
            public string TipoRegistro { get; set; }

            public string _CodigoEstatus { get; set; }
            public string _Descripcion { get; set; }


        }
        public class EstructuraSalidaBanescoEncabezado
        {
            public string Trading { get; set; }
            public string Filler { get; set; }
            public string TipoRegistro { get; set; }

            public string __NumeroReferenciaRespuesta { get; set; }
            public string __FechaRespuesta { get; set; }
            public string __NumeroReferenciaOrdenPago { get; set; }
            public string __TipoOrdenPago { get; set; }
            public string __CodigoBancoEmisor { get; set; }
            public string __NombreBancoEmisor { get; set; }
            public string __CodgioEmpresaReceptoraBansta { get; set; }
            public string __DescripcionEmpresaReceptoraBansta { get; set; }

        }
        static void Main(string[] args)
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;
            Thread.CurrentThread.CurrentCulture = __newCulture;


            Program p = new Program();
            StringBuilder Logs = new StringBuilder();
            Logs.Append("Iniciamos busqueda en carpeta de co bros\r\n");
            p.COB_ValidacionArchivoSalidaBanesco();

            //Console.WriteLine("Procesamos cobros para generar pag \r\n");

            //Console.WriteLine("Fin procesamiento \r\n");


            //string RUTALOGS = ConfigurationManager.AppSettings["rutaLogs"].ToString() + "LOGS" + DateTime.Now.ToString("dd-MM-yy-ss-mm") + ".txt";
            //System.IO.File.WriteAllText(RUTALOGS, Logs.ToString());

            Console.ReadLine();
        }

    }

}
