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
using System.Net.Mail;
using System.Net;

namespace ProcesoPAG
{
    public class Program
    {
        URepository<CP_Archivo> ArchivoREPO = new URepository<CP_Archivo>();
        URepository<CP_ArchivoEstadoCuenta> EstadoCuentaREPO = new URepository<CP_ArchivoEstadoCuenta>();
        URepository<CP_Archivo> CP_ArchivoREPO = new URepository<CP_Archivo>();
        URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
        URepository<CP_ArchivoItem> CP_ArchivoItemRepo = new URepository<CP_ArchivoItem>();

        static void Main(string[] args)
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;
            Thread.CurrentThread.CurrentCulture = __newCulture;


            Program p = new Program();
            StringBuilder Logs = new StringBuilder();
            try
            {


                Logs.Append("Iniciamos busqueda en carpeta de cobros\r\n");
                p.COB_LecturaArchivoSalidaBanesco();

                Console.WriteLine("Procesamos cobros para generar pag \r\n");
                p.PAG_UploadAndMove();
                Console.WriteLine("Fin procesamiento \r\n");

                string RUTALOGS = ConfigurationManager.AppSettings["rutaLogs"].ToString() + "LOGS" + DateTime.Now.ToString("dd-MM-yy-ss-mm") + ".txt";
                File.WriteAllText(RUTALOGS, Logs.ToString());
            }
            catch (Exception e)
            {
                p.enviarCorreoFallo(e);

            }


        }

        public string PAG_UploadAndMove()
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;
            ///MUEVE ARCHIVOS ERROR COB Y PAG SIN MOVIMIENTOS
            string texto = "";
            var host = "10.148.174.215";
            var port = 5522;
            var username = "instapag";
            var password = "540144017";
            //var workingdirectory = ConfigurationManager.AppSettings["rutaFtpOut"].ToString();
            var workingdirectory = "/OUTtoPolar";
            //var workingdirectory = "/OUT";
            // path for file you want to upload 
            string RUTACOBRO = ConfigurationManager.AppSettings["rutaSalidaPAGR"].ToString();
            string RUTABACKUPCOB = ConfigurationManager.AppSettings["rutaBackUpPag"].ToString();
            InstaTransfer.BLL.Concrete.URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
            DirectoryInfo d = new DirectoryInfo(RUTACOBRO);
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            Console.WriteLine("procesando:" + Files.Count() + " archivos \r\n");
            texto = texto + "se encontraron:" + Files.Count() + "archivos \r\n";
            StringBuilder __filenames = new StringBuilder();
            StringBuilder sftpfiles = new StringBuilder();
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("PAG"))
                {
                    __filenames.Append(file.FullName + "<br />");
                    Console.WriteLine("procesando:" + file.Name + " \r\n");
                    string uploadFile = file.FullName.ToString();
                    using (var client = new SftpClient(host, port, username, password))
                    {
                        client.Connect();
                        if (client.IsConnected)
                        {
                            try
                            {
                                //Debug.WriteLine("I'm connected to the client");
                                if (client.Exists(client.WorkingDirectory + workingdirectory))
                                {
                                    Console.WriteLine("cambiando directorio \r\n");
                                    client.ChangeDirectory(client.WorkingDirectory + workingdirectory);
                                    using (var fileStream = new FileStream(uploadFile, FileMode.Open))
                                    {

                                        client.BufferSize = 4 * 1024; // bypass Payload error large files
                                        texto = texto + "subiendo archivo :" + file.Name + "\r\n";
                                        client.UploadFile(fileStream, Path.GetFileName(uploadFile));
                                    }
                                    sftpfiles.Append(client.WorkingDirectory + "/" + file.Name + "<br />");
                                }
                                Console.WriteLine("moviendo \r\n");
                                texto = texto + "moviendo :" + file.Name + "\r\n";
                                string final = RUTABACKUPCOB + file.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                                string[] lines = System.IO.File.ReadAllLines(file.FullName);
                                System.IO.File.WriteAllLines(final, lines);
                                texto = texto + "borrando :" + file.Name + "\r\n";
                                Console.WriteLine("borrando \r\n");
                                file.Delete();
                            }
                            catch (Exception e)
                            {

                                texto = texto + e.Message + "\r\n";
                                enviarCorreoFallo(e);
                            }

                        }
                        else
                        {
                            texto = texto + "No conecta" + "\r\n";
                            //Debug.WriteLine("I couldn't connect");
                        }
                    }
                }

            }


            if (Files.Count() > 0)
            {
                __filenames.Append("<br/><p>Archivos enviados via SFTP:</p>");
                __filenames.Append(sftpfiles.ToString());
                enviarCorreo(__filenames.ToString());
            }
               
            return texto;
        }

        public bool COB_LecturaArchivoSalidaBanesco()
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
            List<EstructuraCOB> Lista = new List<EstructuraCOB>();
            CP_Archivo item = new CP_Archivo();
            CP_Archivo getCP = new CP_Archivo();
            List<CP_ArchivoEstadoCuenta> ItemsArchivo = new List<CP_ArchivoEstadoCuenta>();
            List<PAG> ListaPAG = new List<PAG>();
            List<string> Empresas = new List<string>();
            List<String> ListaPendientes = new List<String>();
            List<String> ListaPendientesTemporal = new List<String>();

            foreach (FileInfo file in Files)
            {
                try
                {
                    ItemArchivo[] Registros;
                    if (file.Name.Contains(".csv") || file.Name.Contains(".CSV"))
                    {
                        Console.WriteLine(file.FullName);
                        var fileHelperEngine = new FileHelperEngine<ItemArchivo>();
                        Registros = fileHelperEngine.ReadFile(file.FullName);
                        Console.WriteLine(Registros.ToList().Count);
                        //VALIDACION RESPUESTAS RECIBIDAS 
                        foreach (var __item in Registros)
                        {
                            try
                            {
                                //if (file.Name.Contains("_O0002"))
                                //{
                                string IBS = __item.IBS.Replace("\"", "").Trim();
                                string departamento = "";
                                string ordenante = "";
                                string _numerocuenta = "";
                                string empresaid = "";
                                Console.WriteLine(IBS);
                                if (IBS.Contains("540132787"))
                                {
                                    empresaid = "540132787";
                                    departamento = "0600";
                                    ordenante = "EFE C A";
                                    _numerocuenta = "01340850598503004455";
                                    Empresas.Add(departamento);
                                }
                                else if (IBS.Contains("205903844"))
                                {
                                    empresaid = "205903844";
                                    departamento = "0100";
                                    ordenante = "PEPSI C A";
                                    _numerocuenta = "01340850598503004195";
                                    Empresas.Add(departamento);
                                }
                                else if (IBS.Contains("540133497"))
                                {
                                    empresaid = "540133497";
                                    departamento = "0001";
                                    ordenante = "ALIMENTOS POLAR C A";
                                    _numerocuenta = "01340375913751013514";
                                    Empresas.Add(departamento);

                                }
                                else if (IBS.Contains("540130908"))
                                {
                                    empresaid = "540130908";
                                    departamento = "0002";
                                    ordenante = "CERVECERIA C A";
                                    _numerocuenta = "01340850598503004357";
                                    Empresas.Add(departamento);
                                }
                                else if (IBS.Contains("210374095"))
                                {
                                    Console.WriteLine("Es fin pago");
                                    empresaid = "210374095";
                                    departamento = "0004";
                                    ordenante = "FIN PAGOS C A";
                                    _numerocuenta = "01340031870311158436";
                                    Empresas.Add(departamento);
                                }

                                string[] linesArchivo = { };
                                CP_ArchivoEstadoCuenta elemento = new CP_ArchivoEstadoCuenta();
                                CP_INI cp_ini = new CP_INI();
                                //string IDarchivo = "";
                                EstructuraSalidaBanescoDetalle Registrolinea2 = new EstructuraSalidaBanescoDetalle();

                                string __NumeroReferencia = __item.REF.Replace("\"", "").Trim();
                                getCP = ArchivoREPO.GetAllRecords().Where(u => u.ReferenciaArchivoBanco == __item.NUMDOC.Replace("\"", "").Trim() && u.Contenido.Contains(empresaid) && u.Contenido.Contains(__NumeroReferencia)).OrderByDescending(u => u.FechaCreacion).FirstOrDefault();
                                ItemsArchivo = EstadoCuentaREPO.GetAllRecords().Where(u => u.ArchivoLecturaPolar == getCP.Id).ToList();

                                elemento = ItemsArchivo.FirstOrDefault();
                                cp_ini = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == elemento.CodigoComercio && u.Estatus == 2).FirstOrDefault();

                                string referencia = ChangeString(elemento.NumeroDocumento.Substring((elemento.NumeroDocumento.Length - 8), 8).ToString());

                                PAG itempag = new PAG();
                                PAGLinea1 Linea1 = new PAGLinea1();
                                Linea1.ReferenciaPago = referencia;
                                Linea1.NumeroRegistro = "1";
                                Linea1.CodigoCliente = elemento.CodigoComercio;
                                Linea1.TipoPagador = cp_ini.RifCliente.Substring(0, 1).ToString();
                                Linea1.IdentificacionPagador = cp_ini.RifCliente.Substring(1, 9).ToString();
                                Linea1.RazonSocial = cp_ini.Nombre;
                                if (__item.CODRES_DET.Replace("\"", "").Trim() == "074")
                                {
                                    Console.WriteLine("verifico");
                                    Linea1.EstatusPago = __item.CODRES_DET.Replace("\"", "").Replace("0", "").Trim();
                                    if (Linea1.EstatusPago == "74")
                                    {
                                        Linea1.EstatusPago = "PAGADO";
                                        elemento.Estatus = 2;
                                        EstadoCuentaREPO.SaveChanges();
                                    }
                                }
                                else
                                {
                                    Linea1.EstatusPago = "RECHAZADO - " + __item.CODRES_DET.Replace("\"", "").Replace("0", "").Trim() + " : " + __item.DESCRIPCION_DET.Replace("\"", "").Trim();
                                }
                                Linea1.CuentaCliente = cp_ini.CuentaBancaria;
                                itempag.Linea1 = Linea1;

                                //AQUI DEBO ITERAR ENTRE DEBITOS Y CREDITOS ... VAMOS CON LA FACTURA PRIMERO
                                List<PAGLinea2> listalinea2 = new List<PAGLinea2>();
                                string[] Debito = elemento.DetalleDebito.Split('|');
                                foreach (var _debito in Debito)
                                {
                                    string[] objetodebito = _debito.Split(';');
                                    if (objetodebito.Count() > 4)
                                    {
                                        PAGLinea2 Linea2 = new PAGLinea2();
                                        Linea2.Referenciapago = referencia;
                                        Linea2.NumeroRegistro = "2";

                                        Linea2.NumeroDocumento = elemento.NumeroDocumento.TrimStart().TrimEnd();
                                        Linea2.TipoDocumento = "01";
                                        Linea2.Referencia = elemento.TipoDocumento;
                                        Linea2.FechaVencimiento = elemento.FechaVencimiento;
                                        Linea2.FechaPago = elemento.FechaPago;

                                        Linea2.MontoIva = "0,00";
                                        Linea2.MontoRetencion = "0,00";
                                        Linea2.MontoNeto = objetodebito[1].ToString().Replace('.', ',');// elemento.MontoDebito.ToString();
                                        Linea2.NumeroComprobanteIR = "X";
                                        Linea2.FechaEmisionComporbanteIR = "X";
                                        listalinea2.Add(Linea2);
                                    }
                                }

                                string[] Credito = elemento.DetalleCredito.Split('|');
                                foreach (var _credito in Credito)
                                {
                                    string[] objetocredito = _credito.Split(';');
                                    if (objetocredito.Count() > 4)
                                    {
                                        PAGLinea2 Linea2 = new PAGLinea2();
                                        Linea2.Referenciapago = referencia;
                                        Linea2.NumeroRegistro = "2";

                                        Linea2.NumeroDocumento = objetocredito[0].ToString().TrimStart().TrimEnd();// elemento.NumeroDocumento.TrimStart().TrimEnd();
                                        Linea2.TipoDocumento = "02";
                                        Linea2.Referencia = objetocredito[4].ToString();
                                        Linea2.FechaVencimiento = objetocredito[3].ToString();
                                        Linea2.FechaPago = objetocredito[2].ToString();

                                        Linea2.MontoIva = "0,00";
                                        Linea2.MontoRetencion = "0,00";
                                        Linea2.MontoNeto = objetocredito[1].ToString().Replace('.', ',');// elemento.MontoDebito.ToString();
                                        Linea2.NumeroComprobanteIR = "X";
                                        Linea2.FechaEmisionComporbanteIR = "X";
                                        listalinea2.Add(Linea2);
                                    }
                                }

                                itempag.Linea2 = listalinea2;

                                PAGLinea3 Linea3 = new PAGLinea3();
                                Linea3.Referenciapago = referencia;
                                Linea3.NumeroRegistro = "4";
                                Linea3.CodigoDepartamento = departamento;
                                Linea3.TipoTransaccion = "03";
                                Linea3.NumerocCuenta = _numerocuenta;
                                Linea3.Subtotal = elemento.TotalArchivo.ToString();
                                Linea3.Totaldebito = "X";
                                Linea3.TotalCargootrosBancos = elemento.TotalArchivo.ToString();
                                Linea3.TotaltarjetaCredito = "X";
                                Linea3.SubtotalCheques = "X";
                                Linea3.SubtotalEfectivo = "X";
                                Linea3.TotalDeposito = "X";

                                itempag.Linea3 = Linea3;

                                itempag.departamento = departamento;
                                Console.WriteLine("agregando");
                                ListaPAG.Add(itempag);
                            }
                            catch (Exception e)
                            {
                            }
                        }

                    }
                    else
                    {
                        if (file.Name.Contains("_O0002"))
                        {

                        }
                        else
                        {
                            file.MoveTo(rutafinal + file.Name + ".txt");
                        }


                    }
                }
                catch (Exception e)
                {

                    //string final = RUTABACKCOBROS + file.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                    //string[] lines = System.IO.File.ReadAllLines(file.FullName);
                    //System.IO.File.WriteAllLines(final, lines);
                    ////texto = texto + "borrando :" + ele.Name + "\r\n";
                    //Console.WriteLine("borrando \r\n");
                    //file.Delete();
                    enviarCorreoFallo(e);

                }
            }

            //ListaPendientes = COB_ValidacionArchivoSalidaBanesco();

            //foreach (var pendiente in ListaPendientes.Distinct().ToList())
            //{
            //    CP_Archivo _getCP = new CP_Archivo();
            //    List<CP_ArchivoEstadoCuenta> _ItemsArchivo = new List<CP_ArchivoEstadoCuenta>();
            //    CP_ArchivoEstadoCuenta elemento = new CP_ArchivoEstadoCuenta();
            //    CP_INI cp_ini = new CP_INI();
            //    _getCP = ArchivoREPO.GetAllRecords().Where(u => u.ReferenciaArchivoBanco == pendiente).FirstOrDefault();
            //    _ItemsArchivo = EstadoCuentaREPO.GetAllRecords().Where(u => u.ArchivoLecturaPolar == _getCP.Id).ToList();
            //    elemento = _ItemsArchivo.FirstOrDefault();
            //    cp_ini = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == elemento.CodigoComercio && u.Estatus == 2).FirstOrDefault();

            //    string departamento = "";
            //    string ordenante = "";
            //    string _numerocuenta = "";
            //    if (_getCP.Contenido.Contains("540132787"))
            //    {
            //        departamento = "0600";
            //        ordenante = "EFE C A";
            //        _numerocuenta = "01340850598503004455";
            //        Empresas.Add(departamento);
            //    }
            //    else if (_getCP.Contenido.Contains("205903844"))
            //    {
            //        departamento = "0100";
            //        ordenante = "PEPSI C A";
            //        _numerocuenta = "01340850598503004195";
            //        Empresas.Add(departamento);
            //    }
            //    else if (_getCP.Contenido.Contains("540133497"))
            //    {
            //        departamento = "0001";
            //        ordenante = "ALIMENTOS POLAR C A";
            //        _numerocuenta = "01340375913751013514";
            //        Empresas.Add(departamento);

            //    }
            //    else if (_getCP.Contenido.Contains("540130908"))
            //    {
            //        departamento = "0002";
            //        ordenante = "CERVECERIA C A";
            //        _numerocuenta = "01340850598503004357";
            //        Empresas.Add(departamento);
            //    }
            //    PAG itempag = new PAG();
            //    PAGLinea1 Linea1 = new PAGLinea1();

            //    string referencia = ChangeString(elemento.NumeroDocumento.Substring((elemento.NumeroDocumento.Length - 8), 8).ToString());
            //    //Linea1.ReferenciaPago = Registrolinea2.NumeroReferencia;
            //    Linea1.ReferenciaPago = referencia;
            //    Linea1.NumeroRegistro = "1";
            //    Linea1.CodigoCliente = elemento.CodigoComercio;
            //    Linea1.TipoPagador = cp_ini.RifCliente.Substring(0, 1).ToString();
            //    Linea1.IdentificacionPagador = cp_ini.RifCliente.Substring(1, 9).ToString();
            //    Linea1.RazonSocial = cp_ini.Nombre;
            //    Linea1.EstatusPago = "RECHAZADO - " + "21" + " : " + "SALDO INSUFICIENTE SR";

            //    Linea1.CuentaCliente = cp_ini.CuentaBancaria;
            //    itempag.Linea1 = Linea1;

            //    List<PAGLinea2> listalinea2 = new List<PAGLinea2>();


            //    string[] Debito = elemento.DetalleDebito.Split('|');
            //    foreach (var _debito in Debito)
            //    {
            //        string[] objetodebito = _debito.Split(';');
            //        if (objetodebito.Count() > 4)
            //        {
            //            PAGLinea2 Linea2 = new PAGLinea2();
            //            Linea2.Referenciapago = referencia;
            //            Linea2.NumeroRegistro = "2";

            //            Linea2.NumeroDocumento = elemento.NumeroDocumento.TrimStart().TrimEnd();
            //            Linea2.TipoDocumento = "01";
            //            Linea2.Referencia = elemento.TipoDocumento;
            //            Linea2.FechaVencimiento = elemento.FechaVencimiento;
            //            Linea2.FechaPago = elemento.FechaPago;

            //            Linea2.MontoIva = "0,00";
            //            Linea2.MontoRetencion = "0,00";
            //            Linea2.MontoNeto = objetodebito[1].ToString().Replace('.', ',');// elemento.MontoDebito.ToString();
            //            Linea2.NumeroComprobanteIR = "X";
            //            Linea2.FechaEmisionComporbanteIR = "X";
            //            listalinea2.Add(Linea2);
            //        }
            //    }

            //    string[] Credito = elemento.DetalleCredito.Split('|');
            //    foreach (var _credito in Credito)
            //    {
            //        string[] objetocredito = _credito.Split(';');
            //        if (objetocredito.Count() > 4)
            //        {
            //            PAGLinea2 Linea2 = new PAGLinea2();
            //            Linea2.Referenciapago = referencia;
            //            Linea2.NumeroRegistro = "2";

            //            Linea2.NumeroDocumento = objetocredito[0].ToString().TrimStart().TrimEnd();// elemento.NumeroDocumento.TrimStart().TrimEnd();
            //            Linea2.TipoDocumento = "02";
            //            Linea2.Referencia = objetocredito[4].ToString();
            //            Linea2.FechaVencimiento = objetocredito[3].ToString();
            //            Linea2.FechaPago = objetocredito[2].ToString();

            //            Linea2.MontoIva = "0,00";
            //            Linea2.MontoRetencion = "0,00";
            //            Linea2.MontoNeto = objetocredito[1].ToString().Replace('.', ',');// elemento.MontoDebito.ToString();
            //            Linea2.NumeroComprobanteIR = "X";
            //            Linea2.FechaEmisionComporbanteIR = "X";
            //            listalinea2.Add(Linea2);
            //        }
            //    }

            //    itempag.Linea2 = listalinea2;

            //    PAGLinea3 Linea3 = new PAGLinea3();
            //    //Linea3.Referenciapago = Registrolinea2.NumeroReferencia;
            //    Linea3.Referenciapago = referencia;
            //    Linea3.NumeroRegistro = "4";
            //    Linea3.CodigoDepartamento = departamento;
            //    Linea3.TipoTransaccion = "03";
            //    Linea3.NumerocCuenta = _numerocuenta;
            //    Linea3.Subtotal = elemento.TotalArchivo.ToString();
            //    Linea3.Totaldebito = "X";
            //    Linea3.TotalCargootrosBancos = elemento.TotalArchivo.ToString();
            //    Linea3.TotaltarjetaCredito = "X";
            //    Linea3.SubtotalCheques = "X";
            //    Linea3.SubtotalEfectivo = "X";
            //    Linea3.TotalDeposito = "X";

            //    itempag.Linea3 = Linea3;

            //    itempag.departamento = departamento;
            //    ListaPAG.Add(itempag);
            //}

            Console.WriteLine("mande a generar pag");
            PAG_Construir(ListaPAG, Empresas.Distinct().ToList());

            string RUTAOUTCOB2 = ConfigurationManager.AppSettings["rutaLecturaSalidaBanescoR"].ToString();
            DirectoryInfo d2 = new DirectoryInfo(RUTAOUTCOB2);
            FileInfo[] Files2 = d2.GetFiles();


            foreach (var ele in Files2)
            {

    
                string final = RUTABACKCOBROS + ele.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                string[] lines = System.IO.File.ReadAllLines(ele.FullName);
                System.IO.File.WriteAllLines(final, lines);
                //texto = texto + "borrando :" + ele.Name + "\r\n";
                Console.WriteLine("borrando \r\n");
                ele.Delete();
            }

          

            return true;
        }

        public string ChangeString(string factura)
        {

            string str = factura;

            string[] alpha = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string nuevostring = string.Empty;

            foreach (char c in str)
            {
                bool isnumber = alpha.Contains(c.ToString());
                if (isnumber)
                    nuevostring = nuevostring + c.ToString();
                else
                    nuevostring = nuevostring + "7";
            }


            //Console.Write("The letters in '{0}' are: '", str);

            //Console.WriteLine("'");
            //Console.WriteLine("Each letter in '{0}' is:", nuevostring);


            return nuevostring;
        }

        public List<String> COB_ValidacionArchivoSalidaBanesco()
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

                ItemArchivo[] Registros;
                if (file.Name.Contains(".csv") || file.Name.Contains(".CSV"))
                {

                    var fileHelperEngine = new FileHelperEngine<ItemArchivo>();
                    Registros = fileHelperEngine.ReadFile(file.FullName);

                    //VALIDACION RESPUESTAS RECIBIDAS 
                    foreach (var __item in Registros.Skip(1))
                    {
                        //if (file.Name.Contains("_O0002"))
                        //{
                        string IBS = __item.IBS.Replace("\"", "").Trim();
                        string departamento = "";
                        string ordenante = "";
                        string _numerocuenta = "";
                        if (IBS.Contains("540132787"))
                        {
                            //empresaid = "540132787";
                            departamento = "0600";
                            ordenante = "EFE C A";
                            _numerocuenta = "01340850598503004455";
                            //Empresas.Add(departamento);
                        }
                        else if (IBS.Contains("205903844"))
                        {
                            //empresaid = "205903844";
                            departamento = "0100";
                            ordenante = "PEPSI C A";
                            _numerocuenta = "01340850598503004195";
                            //Empresas.Add(departamento);
                        }
                        else if (IBS.Contains("540133497"))
                        {
                            //empresaid = "540133497";
                            departamento = "0001";
                            ordenante = "ALIMENTOS POLAR C A";
                            _numerocuenta = "01340375913751013514";
                            //Empresas.Add(departamento);

                        }
                        else if (IBS.Contains("540130908"))
                        {
                            //empresaid = "540130908";
                            departamento = "0002";
                            ordenante = "CERVECERIA C A";
                            _numerocuenta = "01340850598503004357";
                            //Empresas.Add(departamento);
                        }
                        else if (IBS.Contains("540148559"))
                        {
                            //empresaid = "540148559";
                            departamento = "0004";
                            ordenante = "INSTAPAGO";
                            _numerocuenta = "01340850598503004357";
                            //Empresas.Add(departamento);
                        }
                        ArchivosRepsuesta.Add(__item.NUMDOC.Replace("\"", "").Trim());


                    }

                    //if (file.Name.Contains("_O0002"))
                    //{
                    //    string departamento = "";
                    //    string ordenante = "";
                    //    string _numerocuenta = "";
                    //    if (file.Name.Contains("540132787"))
                    //    {
                    //        departamento = "540132787";
                    //        ordenante = "EFE C A";
                    //        _numerocuenta = "01340850598503004455";
                    //        //Empresas.Add(departamento);
                    //    }
                    //    else if (file.Name.Contains("205903844"))
                    //    {
                    //        departamento = "205903844";
                    //        ordenante = "PEPSI C A";
                    //        _numerocuenta = "01340850598503004195";
                    //        //Empresas.Add(departamento);
                    //    }
                    //    else if (file.Name.Contains("540133497"))
                    //    {
                    //        departamento = "540133497";
                    //        ordenante = "ALIMENTOS POLAR C A";
                    //        _numerocuenta = "01340375913751013514";
                    //        //Empresas.Add(departamento);

                    //    }
                    //    else if (file.Name.Contains("540130908"))
                    //    {
                    //        departamento = "540130908";
                    //        ordenante = "CERVECERIA C A";
                    //        _numerocuenta = "01340850598503004357";
                    //        //Empresas.Add(departamento);
                    //    }
                    //    //item.Nombre = file.Name;
                    //    //item.Ruta = file.FullName;
                    //    //item.FechaLectura = DateTime.Now;
                    //    string lineas = "";
                    //    int i = 0;
                    //    string text = System.IO.File.ReadAllText(file.FullName);
                    //    //System.Console.WriteLine("Contents of WriteText.txt = {0}", text);
                    //    // Example #2
                    //    // Read each line of the file into a string array. Each element
                    //    // of the array is one line of the file.
                    //    string[] lines = System.IO.File.ReadAllLines(file.FullName);
                    //    // Display the file contents by using a foreach loop.
                    //    //System.Console.WriteLine("Contents of WriteLines2.txt = ");
                    //    //_cobros
                    //    string[] linesArchivo = { };
                    //    CP_ArchivoEstadoCuenta elemento = new CP_ArchivoEstadoCuenta();
                    //    CP_INI cp_ini = new CP_INI();
                    //    //string IDarchivo = "";
                    //    EstructuraSalidaBanescoDetalle Registrolinea2 = new EstructuraSalidaBanescoDetalle();

                    //    foreach (string line in lines)
                    //    {
                    //        //if (i == 2)
                    //        //{
                    //        lineas = lineas + line + "<br />";
                    //        string sep = "\t";


                    //        string tipo = line.Substring(16, 2).ToString().TrimEnd();

                    //        if (tipo == "01")
                    //        {

                    //            EstructuraSalidaBanescoEncabezado Registro = new EstructuraSalidaBanescoEncabezado();
                    //            Registro.Trading = line.Substring(0, 15).ToString().TrimEnd();
                    //            Registro.Filler = line.Substring(15, 2).ToString().TrimEnd();
                    //            Registro.TipoRegistro = line.Substring(16, 2).ToString().TrimEnd();
                    //            Registro.__NumeroReferenciaRespuesta = line.Substring(19, 12).ToString().TrimEnd();
                    //            if (Registro.__NumeroReferenciaRespuesta != "DCREPETIDO")
                    //            {
                    //                ArchivosRepsuesta.Add(Registro.__NumeroReferenciaRespuesta);
                    //            }


                    //        }

                    //        i++;
                    //    }
                    //}
                    //else
                    //{
                    //    //file.MoveTo(rutafinal + file.Name + ".txt");
                    //}
                }
            }
            //hola
            string RUTABACKCOB = ConfigurationManager.AppSettings["rutaCobroBanescoTemporalR"].ToString();
            DirectoryInfo e = new DirectoryInfo(RUTABACKCOB);
            //FileInfo[] _Files = e.GetFiles().Where(u => u.LastWriteTime > DateTime.Now.AddHours(-(int.Parse(DateTime.Now.Hour.ToString())))).ToArray();
            FileInfo[] _Files = e.GetFiles().ToArray();

            foreach (var file in _Files)
            {
                if (file.Name.Contains("IXDP"))
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
                    else if (file.Name.Contains("540148559"))
                    {
                        //empresaid = "540148559";
                        departamento = "0004";
                        ordenante = "INSTAPAGO";
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
                            Registro.__NumeroReferenciaRespuesta = line.Substring(37, 12).ToString().TrimEnd();
                            ArchivosEnviados.Add(Registro.__NumeroReferenciaRespuesta);
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
            valores = valores + "respuesta" + Environment.NewLine;
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

            return ArchivosPendientes;
        }

        public bool PAG_Construir(List<PAG> items, List<string> empresas)
        {
            string RUTAOUTPAG = ConfigurationManager.AppSettings["rutaSalidaPAG"].ToString();
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;
            Thread.CurrentThread.CurrentCulture = __newCulture;
            foreach (var empresa in empresas)
            {
                List<string> _cobros = new List<string>();

                int i = 0;
                foreach (var cobro in items.Where(u => u.departamento == empresa).ToList())
                {
                    string line1 =
                    cobro.Linea1.ReferenciaPago.ToString().PadLeft(18, '0') + "|" +
                    cobro.Linea1.NumeroRegistro + "|" +
                    cobro.Linea1.CodigoCliente + "|" +
                    cobro.Linea1.TipoPagador + "|" +
                    cobro.Linea1.IdentificacionPagador + "|" +
                    cobro.Linea1.RazonSocial + "|" +
                    cobro.Linea1.EstatusPago + "|" +
                    cobro.Linea1.CuentaCliente;
                    _cobros.Add(line1);

                    foreach (var item in cobro.Linea2)
                    {
                        string line2 =
                        item.Referenciapago.ToString().PadLeft(18, '0') + "|" +
                        item.NumeroRegistro + "|" +
                        item.NumeroDocumento + "|" +
                        item.TipoDocumento + "|" +
                        item.Referencia + "|" +
                        item.FechaVencimiento + "|" +
                        item.FechaPago + "|" +
                        item.MontoIva + "|" +
                        item.MontoRetencion + "|" +
                        item.MontoNeto + "|" +
                        item.NumeroComprobanteIR + "|" +
                        item.FechaEmisionComporbanteIR;

                        _cobros.Add(line2);
                    }
                    string line3 =
                    cobro.Linea3.Referenciapago.ToString().PadLeft(18, '0') + "|" +
                    cobro.Linea3.NumeroRegistro + "|" +
                    cobro.Linea3.CodigoDepartamento + "|" +
                    cobro.Linea3.TipoTransaccion + "|" +
                    cobro.Linea3.NumerocCuenta + "|" +
                    cobro.Linea3.Subtotal + "|" +
                    cobro.Linea3.Totaldebito + "|" +
                    cobro.Linea3.TotalCargootrosBancos + "|" +
                    cobro.Linea3.TotaltarjetaCredito + "|" +
                    cobro.Linea3.SubtotalCheques + "|" +
                    cobro.Linea3.SubtotalEfectivo + "|" +
                    cobro.Linea3.TotalDeposito;

                    _cobros.Add(line3);

                }
                //_cobros
                string[] lines = { };
                foreach (var _item in _cobros)
                {
                    Array.Resize(ref lines, lines.Length + 1);
                    lines[lines.Length - 1] = _item;
                }

                string ruta = RUTAOUTPAG + "BANPAG" + empresa + ".txt";
                System.IO.File.WriteAllLines(ruta, lines);
            }
            return true;


        }


        public bool enviarCorreo(string message)
        {
            Console.WriteLine("aqui viene el correo");

            try
            {
                var mailmessage = new MailMessage(new MailAddress("notificacion@instapago.com", "Robot InstaPago"),
             new MailAddress("soporte@instapago.com", "soporte@instapago.com"));
                mailmessage.BodyEncoding = System.Text.Encoding.Default;
                mailmessage.Subject = "[CEC POLAR] - Proceso de respuesta PAG " + DateTime.Now.ToString("dd/MM/yyyy");
                mailmessage.Body = "<p>Buenos dias.</p> <p>Sirva la presente para hacer de su conocimiento que se ha ejecutado satisfactoriamente el proceso de escritura de archivos PAG del grupo de empresas POLAR.</p> <p>Archivos de respuesta generados</p>" + message;
                mailmessage.IsBodyHtml = true;
                mailmessage.To.Add(ConfigurationManager.AppSettings["SMTP_TO"]);


                SmtpClient smtpMail = new SmtpClient(System.Configuration.ConfigurationManager.AppSettings["SMTP_ADDRESS"],
                    int.Parse(System.Configuration.ConfigurationManager.AppSettings["SMTP_PORT"]));
                smtpMail.EnableSsl = false;
                smtpMail.Timeout = 5000;

                smtpMail.Send(mailmessage);


                return true;


            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return false;
            }



        }

        public bool enviarCorreoFallo(Exception ex)
        {
            Console.WriteLine("aqui viene el correo de falla");

            try
            {
                var mailmessage = new MailMessage(new MailAddress("notificacion@instapago.com", "Robot InstaPago"),
               new MailAddress("soporte@instapago.com", "soporte@instapago.com"));
                mailmessage.BodyEncoding = System.Text.Encoding.Default;
                mailmessage.Subject = "[CEC POLAR] - FALLA PAG " + DateTime.Now.ToString("dd/MM/yyyy");
                mailmessage.Body = "<p>Error al procesar el servicio de PAG:</p>" + ex.Message + "<br/><br/>" + ex.StackTrace;
                mailmessage.IsBodyHtml = true;
                mailmessage.Priority = MailPriority.High;

                SmtpClient smtpMail = new SmtpClient(System.Configuration.ConfigurationManager.AppSettings["SMTP_ADDRESS"], int.Parse(System.Configuration.ConfigurationManager.AppSettings["SMTP_PORT"]));
                smtpMail.EnableSsl = false;
                smtpMail.Timeout = 5000;

                smtpMail.Send(mailmessage);


                return true;

            }
            catch (Exception a)
            {

                Console.WriteLine(a.Message);
                return false;
            }



        }



        [DelimitedRecord(",")]
        [IgnoreFirst()]
        [IgnoreEmptyLines]
        public class ItemArchivo
        {

            public string IBS { get; set; }

            public string NUMDOC { get; set; }

            public string REF { get; set; }




            public string CODRES { get; set; }

            public string DESCRIPCION { get; set; }

            public string FECHA { get; set; }

            public string CODRES_DET { get; set; }

            public string DESCRIPCION_DET { get; set; }




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


    }

}
