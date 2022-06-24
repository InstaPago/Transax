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
using System.Text.RegularExpressions;
using FileHelpers;
using System.Globalization;
using System.Threading;
using System.IO.Compression;

namespace ProcesoCOB
{
    public class Program
    {
        URepository<CP_Archivo> ArchivoREPO = new URepository<CP_Archivo>();
        URepository<CP_ArchivoEstadoCuenta> EstadoCuentaREPO = new URepository<CP_ArchivoEstadoCuenta>();
        URepository<CP_Archivo> CP_ArchivoREPO = new URepository<CP_Archivo>();
        URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
        URepository<CP_ArchivoItem> CP_ArchivoItemRepo = new URepository<CP_ArchivoItem>();
        static int GlobalCounter = 1;

        public string COBERROR_UploadAndMove()
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;
            Thread.CurrentThread.CurrentCulture = __newCulture;
            ///MUEVE ARCHIVOS ERROR COB Y PAG SIN MOVIMIENTOS
            string texto = "";
            var host = "10.148.174.215";
            var port = 5522;
            var username = "instapag";
            var password = "540144017";

            var workingdirectory = "/OUTtoPolar";
            //var workingdirectory = "/OUT";
            // path for file you want to upload 
            string RUTAERRORCLI = ConfigurationManager.AppSettings["rutaErrorCOBR"].ToString();
            string RUTABACKUPCLI = ConfigurationManager.AppSettings["rutaBackUpCOB"].ToString();
            InstaTransfer.BLL.Concrete.URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
            DirectoryInfo d = new DirectoryInfo(RUTAERRORCLI);
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            Console.WriteLine("procesando:" + Files.Count() + " archivos \r\n");
            texto = texto + "se encontraron:" + Files.Count() + "archivos \r\n";
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("BANERRORCOB") || file.Name.Contains("PAG"))
                {
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
                                }
                                Console.WriteLine("moviendo \r\n");
                                texto = texto + "moviendo :" + file.Name + "\r\n";
                                string final = RUTABACKUPCLI + file.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                                string[] lines = System.IO.File.ReadAllLines(file.FullName);
                                System.IO.File.WriteAllLines(final, lines);
                                texto = texto + "borrando :" + file.Name + "\r\n";
                                Console.WriteLine("borrando \r\n");
                                file.Delete();
                            }
                            catch (Exception e)
                            {

                                texto = texto + e.Message + "\r\n";

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

            return texto;
        }

        public string COBRO_UploadAndMove()
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
            var workingdirectory = "/INtoAS400p";
            //var workingdirectory = "/OUT";
            // path for file you want to upload 
            string RUTACOBRO = ConfigurationManager.AppSettings["rutaCobroBanescoR"].ToString();
            string RUTABACKUPCOB = ConfigurationManager.AppSettings["rutaBackUpCOB"].ToString();
            InstaTransfer.BLL.Concrete.URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
            DirectoryInfo d = new DirectoryInfo(RUTACOBRO);
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            Console.WriteLine("procesando:" + Files.Count() + " archivos \r\n");
            texto = texto + "se encontraron:" + Files.Count() + "archivos \r\n";

            string startPath = RUTACOBRO;
            string zipPath = RUTACOBRO + @"\zipped\cobros.zip";
  

            ZipFile.CreateFromDirectory(startPath, zipPath);



            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("IXDP"))
                {
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

            return texto;
        }

        public string COB_DownloadAndMove()
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;
            string texto = "";
            try
            {
                var host = "10.148.174.215";
                var port = 5522;
                var username = "instapag";
                var password = "540144017";
                var remoteDirectory = "/IN";
                var backupDirectory = "/INbackup/";
                string RUTALECTURACLI = ConfigurationManager.AppSettings["rutaLecturaCOB"].ToString();
                //string remoteDirectory = "/RemotePath/";
                string localDirectory = RUTALECTURACLI;
                using (var sftp = new SftpClient(host, port, username, password))
                {
                    sftp.Connect();
                    texto = "conecto con el sftp \r\n ";
                    if (sftp.IsConnected)
                    {
                        //Debug.WriteLine("I'm connected to the client");
                        if (sftp.Exists(sftp.WorkingDirectory + remoteDirectory))
                        {
                            texto = texto + "directorio existe" + sftp.WorkingDirectory + remoteDirectory + " \r\n ";
                            //sftp.ChangeDirectory(sftp.WorkingDirectory + remoteDirectory);
                            //texto = texto + "cambie directorio" + sftp.WorkingDirectory + remoteDirectory + "| ";
                            texto = texto + "busco archivos CLI en" + sftp.WorkingDirectory + remoteDirectory + " \r\n ";
                            var files = sftp.ListDirectory(sftp.WorkingDirectory + remoteDirectory);
                            //texto = texto + "busque archivos " + sftp.WorkingDirectory + remoteDirectory + "  -> " + files.Count().ToString() + " | ";
                            foreach (var file in files)
                            {

                                if (file.Name.Contains("BANCOB"))
                                {
                                    texto = texto + "encontre: " + file.Name + "\r\n";
                                    string remoteFileName = file.Name;
                                    //texto = texto + "encontre y recorro " + localDirectory + remoteFileName + " | ";
                                    try
                                    {
                                        using (Stream file1 = System.IO.File.OpenWrite(localDirectory + remoteFileName))
                                        {
                                            sftp.DownloadFile(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName, file1);
                                            texto = texto + "lo descargue \r\n";
                                            var inFile = sftp.Get(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName);
                                            texto = texto + "lo movi \r\n" + sftp.WorkingDirectory + backupDirectory + " | ";
                                            inFile.MoveTo(sftp.WorkingDirectory + backupDirectory + remoteFileName + DateTime.Now.ToString("dd-MM-yy-mm-ss"));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        texto = texto + "intento descargar y fallo : " + e.Message + " \r\n ";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        texto = texto + "No conecta" + "\r\n";
                    }
                }
            }
            catch (Exception e)
            {
                texto = texto + e.Message + "  " + e.StackTrace;
                Console.WriteLine(texto);
                return texto;
            }
            Console.WriteLine(texto);
            return texto;


        }

        public string COB_LecturaPOLAR()
        {
            GlobalCounter = 1;
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;
            Thread.CurrentThread.CurrentCulture = __newCulture;
            string RUTALECTURACOB = ConfigurationManager.AppSettings["rutaLecturaCOBR"].ToString();
            string RUTABACKUPCOB = ConfigurationManager.AppSettings["rutaBackUpCOB"].ToString();
            DirectoryInfo d = new DirectoryInfo(RUTALECTURACOB);//Assuming Test is your Folder
            List<CP_ArchivoItem> ListItemsFile = new List<CP_ArchivoItem>();
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            Console.WriteLine("procesando:" + Files.Count() + " \r\n");
            string referencia = "";
            string asociado = "";
            foreach (FileInfo file in Files)
            {
                Console.WriteLine("procesando:" + file.Name + " \r\n");
                List<EstructuraCOB> Lista = new List<EstructuraCOB>();
                CP_Archivo item = new CP_Archivo();
                if (file.Name.Contains("BANCOB0001"))
                {
                    asociado = "540133497";
                    //ALIMENTOS POLAR  J000413126 - IBS :540133497
                    //rifreferencia = "J000413126";
                    referencia = "0001";

                }
                else if (file.Name.Contains("BANCOB0002"))
                {
                    asociado = "540130908";

                    //CERVECERIA POLAR J000063729 - IBS:540130908
                    //rifreferencia = "J000063729";
                    referencia = "0002";
                }
                else if (file.Name.Contains("BANCOB0600"))
                {
                    asociado = "540132787";
                    //PRODUCTOS EFE J000301255  - IBS: 540132787 
                    //rifreferencia = "J000301255";
                    referencia = "0600";
                }
                else if (file.Name.Contains("BANCOB0100"))
                {
                    asociado = "205903844";
                    //PEPSI COLA J301370139 -IBS: 205903844 
                    //rifreferencia = "J301370139";
                    referencia = "0100";

                }

                bool vencido = false;
                string text = System.IO.File.ReadAllText(file.FullName);
                //System.Console.WriteLine("Contents of WriteText.txt = {0}", text);
                item.Nombre = file.Name;
                item.Ruta = file.FullName;
                item.FechaLectura = DateTime.Now;
                string lineas = "";
                // Example #2
                // Read each line of the file into a string array. Each element
                // of the array is one line of the file.
                string[] lines = System.IO.File.ReadAllLines(file.FullName);
                int ultimalinea = lines.Count();
                var contenidoultimalinea = lines[ultimalinea - 1];
                string fechaarchivo = contenidoultimalinea.Substring(0, 4) + "/" + contenidoultimalinea.Substring(4, 2) + "/" + contenidoultimalinea.Substring(6, 2);
                //if (DateTime.Now > DateTime.Parse(fechaarchivo).AddHours(23))
                //{
                //    vencido = true;
                //    string line = "Sin Cobros";
                //    string RUTAERRORCOB = ConfigurationManager.AppSettings["rutaErrorCOB"].ToString();
                //    string ruta = RUTAERRORCOB + "BANPAG" + referencia + ".txt";
                //    System.IO.File.WriteAllText(ruta, line);
                //}

                List<_EstructuraCOB> Archivo = new List<_EstructuraCOB>();
                List<CP_ArchivoItem> CPITEMS = new List<CP_ArchivoItem>();
                var engine = new FileHelperEngine<_EstructuraCOB>();
                var result = engine.ReadFile(file.FullName);
                int j = 0;
                foreach (_EstructuraCOB _item in result)
                {
                    if (j != (ultimalinea - 1))
                    {

                        Archivo.Add(_item);
                        CP_ArchivoItem _AI = new CP_ArchivoItem();
                        _AI.CodigoCliente = _item.CodigoCliente;
                        _AI.CodigoDepartamento = _item.CodigoDepartamento;
                        _AI.Datetime = DateTime.Now;
                        _AI.DocumentoComercial = _item.DocumentoComercial;
                        _AI.Estatus = 1;
                        _AI.FechaEmision = _item.FechaEmision;
                        _AI.FechaVencimiento = _item.Fechavencimiento;
                        _AI.Monto = _item.Monto;
                        _AI.MontoIVA = _item.MontoIva;
                        _AI.OBS = _item.OBS;
                        _AI.Referencia = _item.Referencia;
                        _AI.TipoDocumento = _item.TipoDocumento;


                        CPITEMS.Add(_AI);

                        //CP_ArchivoItemRepo.AddEntity(_AI);
                    }
                    j++;
                }
                //RONDA DE ERRORES BASICOS
                foreach (var __item in CPITEMS)
                {
                    if (__item.DescripcionError == null)
                    {
                        if (vencido)
                        {
                            __item.CodigoError = 001;
                            __item.DescripcionError = "ERROR EN FECHA DE ARCHIVO";
                            goto Finish;
                        }

                        if (__item.TipoDocumento == "04")
                        {
                            __item.CodigoError = 004;
                            __item.DescripcionError = "RETENCION - NO PROCESADO";
                            goto Finish;
                        }

                        //falta agregar que esta activo en 
                        CP_INI beneficiario = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == __item.CodigoCliente
                        && u.Departamento == __item.CodigoDepartamento
                        && u.Estatus == 2).FirstOrDefault();

                        if (beneficiario != null && beneficiario.Id > 0)
                        {
                            //ADICIONA LESTA ACTIVO POR CLI
                            if (!beneficiario.ActivoCLI)
                            {
                                __item.CodigoError = 005;
                                __item.DescripcionError = "CLIENTE INACTIVO PARA COBRO";
                                goto Finish;
                            }

                        }
                        else
                        {
                            //092 nota sin factura 028 cuenta beneficiario invalidad 016 cuenta bloqueada 070 monto invalido
                            __item.CodigoError = 006;
                            __item.DescripcionError = "CLIENTE SIN CARGA PREVIA";
                            goto Finish;
                        }


                        //aqui validacion de flag nuevo agregado en el clic
                        if (DateTime.Now.Date <= DateTime.Parse(__item.FechaVencimiento) && beneficiario.FechaVencimiento)
                        {
                            __item.CodigoError = 003;
                            __item.DescripcionError = "REGISTRO NO VENCIDO";
                            goto Finish;
                        }


                        Regex regex = new Regex(@"^[0-9]{0,15}$");
                        if (__item.TipoDocumento == "01" || __item.TipoDocumento == "03")
                        {
                            if (__item.DocumentoComercial.ToString().Length < 1 || __item.DocumentoComercial.ToString() == "X")
                            {
                                __item.CodigoError = 073;
                                __item.DescripcionError = "NUMERO DE DOCUMENTO NO VALIDO";
                                goto Finish;
                            }

                            //debo ir base de datos y ver si ya fue cobrado
                            var duplicado = EstadoCuentaREPO.GetAllRecords().Where(u => u.CodigoComercio == beneficiario.CodigoCliente && u.NumeroDocumento == __item.DocumentoComercial && u.Estatus == 2).ToList();
                            if (duplicado.Count() > 0)
                            {
                                foreach (var objeto in duplicado)
                                {
                                    __item.CodigoError = 002;
                                    __item.DescripcionError = "DOCUMENTO DUPLICADO - COBRO REALIZADO";
                                    goto Finish;
                                }
                            }
                        }

                        var resultduplciado = CPITEMS.Where(u => u.DocumentoComercial == __item.DocumentoComercial && u.Referencia == __item.Referencia).ToList();

                        if (resultduplciado.Count() > 1)
                        {
                            foreach (var objeto in resultduplciado)
                            {
                                __item.CodigoError = 002;
                                __item.DescripcionError = "DOCUMENTO DUPLICADO";
                                goto Finish;
                            }
                        }



                        if (__item.TipoDocumento == "02")
                        {
                            var reusltfacutra = CPITEMS.Where(u => u.DocumentoComercial
                            == __item.Referencia && u.Referencia
                            == "FACTURA").ToList();

                            if (reusltfacutra.Count() >= 1)
                            {

                            }
                            else
                            {
                                __item.CodigoError = 092;
                                __item.DescripcionError = "NOTA DE CREDITO SIN DOCUMENTO";
                                goto Finish;
                            }

                        }


                    }
                Finish:
                    string hola = "";
                }


                //var groupedCustomerList1 = CPITEMS.GroupBy(u => u.DocumentoComercial).Select(grp => grp.ToList()).ToList();
                //SEGUNDA RONDA VALIDAR MONTOS
                List<CP_ArchivoItem> ListaConsultaI = new List<CP_ArchivoItem>();

                foreach (var item2 in CPITEMS.Where(u => u.DescripcionError == null).ToList())
                {
                    bool _win = true;
                    decimal _Total = 0;
                    decimal _Debito = 0;
                    decimal _Credito = 0;
                    string _debito = "";
                    string _credito = "";
                    string _comercio = "";

                    List<CP_ArchivoItem> list = new List<CP_ArchivoItem>();
                    //list.Add(_row);

                    list = CPITEMS.Where(u => (u.DocumentoComercial == item2.DocumentoComercial || u.Referencia == item2.DocumentoComercial
                    || u.DocumentoComercial == item2.Referencia) && u.DescripcionError == null).ToList();

                    foreach (var row in list)
                    {

                        if (!ListaConsultaI.Any(u => u.DocumentoComercial == row.DocumentoComercial && u.Referencia == row.Referencia))
                        {
                            try
                            {
                                _comercio = row.CodigoCliente;
                                if (row.TipoDocumento == "02")
                                {
                                    _Credito = _Credito + decimal.Parse(row.Monto.Replace('.', ','));
                                    _credito = _credito + " " + row.DocumentoComercial + " " + row.FechaEmision + " NOTA CREDITO" + " | ";
                                }
                                else
                                {

                                    switch (row.TipoDocumento)
                                    {
                                        case "01":
                                            _Debito = _Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                            _debito = _debito + " " + row.DocumentoComercial + " " + row.FechaEmision + "FACTURA" + " | ";
                                            break;
                                        case "03":
                                            _Debito = _Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                            _debito = _debito + " " + row.DocumentoComercial + " " + row.FechaEmision + "NOTA DEBITO" + " | ";
                                            break;
                                        case "04":
                                            _Debito = _Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                            _debito = _debito + " " + row.DocumentoComercial + " " + row.FechaEmision + "FACTURA" + " | ";
                                            break;

                                        default:
                                            break;
                                    }
                                }
                            }
                            catch
                            {
                                //crear log de error y guardar en base de datos
                            }
                            ListaConsultaI.Add(row);
                        }
                        else
                        {
                            _win = false;
                        }

                    }

                    if (_win)
                    {
                        //_Total = _Debito - _Credito;

                        if (_Credito > _Debito)
                        {
                            foreach (var row in list)
                            {
                                row.DescripcionError = "SALDO A FAVOR DEL CLIENTE";
                                row.CodigoError = 070;

                            }
                        }
                        else if (_Credito == _Debito)
                        {
                            foreach (var row in list)
                            {
                                row.DescripcionError = "MONTO INVALIDO";
                                row.CodigoError = 071;

                            }

                        }
                        else
                        {

                        }
                    }

                }
                Console.WriteLine("generando errores:" + file.Name + " \r\n");

                ERRORCOB_GenerarPOLAR(CPITEMS.Where(u => u.CodigoError != null).ToList(), referencia);

                item.Registro = 0;
                item.ReferenciaOrigen = "";
                item.ReferenciaArchivoBanco = "";
                item.FechaCreacion = DateTime.Now;
                item.FechaLectura = DateTime.Now;
                item.Contenido = lineas;
                item.Descripcion = "Lectura Archivo Porlar COB";
                item.Tipo = 1;
                item.EsRespuesta = false;
                item.ContenidoRespuesta = "";


                CPITEMS = CPITEMS.Where(u => u.CodigoError == null).ToList();
                int largo = CPITEMS.Count();
                int i = 0;
                //var groupedCustomerList = CPITEMS.GroupBy(u => u.DocumentoComercial).Select(grp => grp.ToList()).ToList();

                item.Contenido = lineas;
                item.Descripcion = "Archivo Cobro Polar - Comercios Evaluado:" + CPITEMS.Count();
                item.Tipo = 2;
                ArchivoREPO.AddEntity(item);
                ArchivoREPO.SaveChanges();
                List<CP_ArchivoItem> ListaConsulta = new List<CP_ArchivoItem>();
                List<CP_ArchivoEstadoCuenta> ListaEstado = new List<CP_ArchivoEstadoCuenta>();
                //Generar Estado de Cuenta
                foreach (var _row in CPITEMS)
                {
                    CP_ArchivoEstadoCuenta EstadoCuenta = new CP_ArchivoEstadoCuenta();
                    bool _win = true;
                    decimal Total = 0;
                    decimal Debito = 0;
                    decimal Credito = 0;
                    string debito = "";
                    string credito = "";
                    string comercio = "";
                    string tipodocumento = "";
                    string numerodocumento = "";
                    string fechavencimiento = "";
                    string fechapago = "";

                    List<CP_ArchivoItem> list = new List<CP_ArchivoItem>();
                    //list.Add(_row);

                    list = CPITEMS.Where(u => (u.DocumentoComercial == _row.DocumentoComercial || u.Referencia == _row.DocumentoComercial
                            || u.DocumentoComercial == _row.Referencia) && u.DescripcionError == null).ToList();

                    foreach (var row in list)
                    {
                        if (!ListaConsulta.Any(u => u.DocumentoComercial == row.DocumentoComercial && u.Referencia == row.Referencia))
                        {
                            try
                            {
                                comercio = row.CodigoCliente;
                                if (row.TipoDocumento == "02")
                                {
                                    Credito = Credito + decimal.Parse(row.Monto.Replace('.', ','));
                                    credito = credito + " " + row.DocumentoComercial + ";" + row.Monto + ";" + row.FechaEmision + ";" + row.FechaVencimiento + ";" + row.Referencia + ";" + "NC" + " | ";
                                }
                                else
                                {

                                    switch (row.TipoDocumento)
                                    {
                                        case "01":
                                            Debito = Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                            debito = debito + " " + row.DocumentoComercial + ";" + row.Monto + ";" + row.FechaEmision + ";" + row.FechaVencimiento + ";" + row.Referencia + ";" + "FACTURA" + " | ";
                                            tipodocumento = "FACTURA";
                                            numerodocumento = row.DocumentoComercial;
                                            fechavencimiento = row.FechaVencimiento;
                                            fechapago = row.FechaEmision;
                                            break;
                                        case "03":
                                            Debito = Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                            debito = debito + " " + row.DocumentoComercial + ";" + row.Monto + ";" + row.FechaEmision + ";" + row.FechaVencimiento + ";" + row.Referencia + ";" + "ND" + " | ";
                                            tipodocumento = "NOTADEBITO";
                                            numerodocumento = row.DocumentoComercial;
                                            fechavencimiento = row.FechaVencimiento;
                                            fechapago = row.FechaEmision;
                                            break;
                                        case "04":
                                            Debito = Debito + decimal.Parse(row.Monto.Replace('.', ','));
                                            debito = debito + " " + row.DocumentoComercial + ";" + row.Monto + ";" + row.FechaEmision + ";" + row.FechaVencimiento + ";" + row.Referencia + ";" + "FACTURA" + " | ";
                                            break;

                                        default:
                                            break;
                                    }
                                }

                                ListaConsulta.Add(row);
                            }
                            catch
                            {
                                //crear log de error y guardar en base de datos
                            }
                        }
                        else
                        {
                            _win = false;
                        }
                    }
                    if (_win)
                    {
                        Total = Credito - Debito;
                        EstadoCuenta.ArchivoLecturaPolar = item.Id;
                        EstadoCuenta.MontoCredito = Credito;
                        EstadoCuenta.MontoDebito = Debito;
                        EstadoCuenta.Total = Total;
                        EstadoCuenta.TotalArchivo = Total * -1;
                        EstadoCuenta.DetalleCredito = credito;
                        EstadoCuenta.DetalleDebito = debito;
                        EstadoCuenta.FechaLectura = DateTime.Now;
                        EstadoCuenta.CodigoComercio = comercio;
                        EstadoCuenta.TipoDocumento = tipodocumento;
                        EstadoCuenta.NumeroDocumento = numerodocumento;
                        EstadoCuenta.FechaVencimiento = fechavencimiento;
                        EstadoCuenta.FechaPago = fechapago;


                        if (EstadoCuenta.Total > 0)
                        {
                            EstadoCuenta.Estatus = 2;
                        }
                        else
                        {
                            EstadoCuenta.Estatus = 1;
                        }
                        EstadoCuentaREPO.AddEntity(EstadoCuenta);

                    }




                }
                EstadoCuentaREPO.SaveChanges();
                ListaEstado = EstadoCuentaREPO.GetAllRecords().Where(u => u.ArchivoLecturaPolar == item.Id && u.Estatus == 1).ToList();
                if (!vencido)
                {
                    Console.WriteLine("cobros:" + file.Name + " \r\n");
                    if (ListaEstado.Count > 0)
                        COB_GenerarCobroBanesco(ListaEstado, asociado);
                    else
                    {
                        string line = "Sin Cobros";
                        string RUTAERRORCOB = ConfigurationManager.AppSettings["rutaErrorCOB"].ToString();
                        string ruta = RUTAERRORCOB + "BANPAG" + referencia + ".txt";
                        System.IO.File.WriteAllText(ruta, line);
                    }
                }

                //texto = texto + "moviendo archivo:" + file.Name + "\r\n";
                string final = RUTABACKUPCOB + file.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                string[] _lines2 = System.IO.File.ReadAllLines(file.FullName);
                System.IO.File.WriteAllLines(final, _lines2);
                file.Delete();
            }
            return "todo ok";
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

        public bool Test()
        {

            for (int i = 0; i < 1000; i++)
            {
                Random _Random = new Random();
                string ram = _Random.Next(0, 99999).ToString();
                Console.WriteLine(ram.ToString().PadLeft(6, '0') + "\r\n");
                System.Threading.Thread.Sleep(1);


            }
            Console.ReadLine();
            return true;
        }
        public bool ERRORCOB_GenerarPOLAR(List<CP_ArchivoItem> _items, string codigo)
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;
            string RUTAERRORCOB = ConfigurationManager.AppSettings["rutaErrorCOB"].ToString();
            if (_items.Count > 0)
            {
                List<string> _cobros = new List<string>();
                int i = 0;
                foreach (var cobro in _items)
                {
                    if (i <= (_items.Count() - 1))
                    {
                        string item = cobro.CodigoError.ToString().PadLeft(3, '0').ToString() + "|" +
                            cobro.CodigoCliente + "|" +
                            cobro.DocumentoComercial + "|" +
                            cobro.Referencia + "|" +
                            cobro.CodigoDepartamento + "|" +
                            cobro.Monto + "|" +
                            cobro.FechaEmision + "|" +
                            cobro.FechaVencimiento + "|" +
                            cobro.TipoDocumento + "|" +
                            cobro.MontoIVA + "|" + (cobro.OBS == "" ? "X" : cobro.OBS) + "|" +
                            cobro.DescripcionError;

                        _cobros.Add(item);
                    }
                    i++;
                }

                if (_cobros.Count() > 0)
                {
                    string[] lines = { };

                    foreach (var _item in _cobros)
                    {
                        Array.Resize(ref lines, lines.Length + 1);
                        lines[lines.Length - 1] = _item;
                    }
                    string ruta = RUTAERRORCOB + "BANERRORCOB" + codigo + ".txt";
                    System.IO.File.WriteAllLines(ruta, lines);
                }
                else
                {

                    string line = "Sin Errores";
                    string ruta = RUTAERRORCOB + "BANERRORCOB" + codigo + ".txt";
                    System.IO.File.WriteAllText(ruta, line);

                }
                return true;
            }
            else
            {
                string line = "Sin Cobros";
                string ruta = RUTAERRORCOB + "BANPAG" + codigo + ".txt";
                System.IO.File.WriteAllText(ruta, line);
                return true;
            }
        }

        public bool COB_GenerarCobroBanesco(List<CP_ArchivoEstadoCuenta> Cobros, string _asociado)
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;
            Thread.CurrentThread.CurrentCulture = __newCulture;
            string RUTACOBRO = ConfigurationManager.AppSettings["rutaCobroBanesco"].ToString();
            List<CP_Archivo> Archivos = new List<CP_Archivo>();
            //int i = 1;
            foreach (var cobro in Cobros)
            {
                //System.Threading.Thread.Sleep(250);
                CP_Archivo item = new CP_Archivo();

                item.FechaLectura = DateTime.Now;
                item.Registro = 1;
                item.ReferenciaOrigen = "";
                item.ReferenciaArchivoBanco = "";
                item.FechaCreacion = DateTime.Now;
                item.FechaLectura = DateTime.Now;
                item.Descripcion = "Lectura Archivo Porlar COB";
                item.Tipo = 3;
                item.EsRespuesta = false;
                item.ContenidoRespuesta = "";
                item.Descripcion = "Archivo Cobro Enviado Banesco";
                item.Tipo = 2;
                //string comercio = Cobros.First().AE_Avance.Id;
                //string rif = Cobros.First().AE_Avance.RifCommerce;
                //string fechaarchivo = DateTime.Now.AddDays(0).ToString("ddMMyy.ssff");
                //string id = Cobros.FirstOrDefault().Id.ToString();
                //if (id.Length > 4)
                //{
                //    id = id.Substring((id.Length - 4), 4);
                //}
                //else if (id.Length < 4)
                //{
                //    id = id.PadLeft(4, '0');
                //}
                string __NumeroDocumento = ChangeString(cobro.NumeroDocumento.ToString());
                int _length = __NumeroDocumento.Length >= 8 ? 8 : __NumeroDocumento.Length;
                string recibo = __NumeroDocumento.Substring(__NumeroDocumento.Length - _length, _length).PadLeft(8, '0');
                string reciboX = __NumeroDocumento.Substring(__NumeroDocumento.Length - 3, 3).PadLeft(3, '0');
               
                
                string asociado = _asociado;
                string __newrandom = string.Empty;
                try
                {
                    __newrandom = Convert.ToInt64(Convert.ToDecimal(cobro.NumeroDocumento) * Convert.ToDecimal(cobro.CodigoComercio) / (Convert.ToDecimal(cobro.Id))).ToString();
                    
                    __newrandom = Regex.Match(__newrandom, @"(.{7})\s*$").ToString().PadLeft(7, '0');
                    

                }
                catch (Exception)
                {
                    Random _Random = new Random(cobro.Id);
                    string ram = _Random.Next(0, 999).ToString();
                    __newrandom = DateTime.Now.AddDays(0).ToString("ddss") + ram.PadLeft(3, '0');

                }

                string numeroorden = __newrandom + reciboX.PadLeft(3, '0') + asociado.Substring(asociado.Length - 2, 2);
                string _fecha = DateTime.Now.AddDays(0).ToString("yyyyMMdd");
                //numeroorden = numeroorden;
                //datos fijos
                string registro = "00";
                //string idtransaccion = "2020040801";Codigo
                //
                //asociado = "540207829";
                string _rif = "";
                string ordenante = "";
                string _numerocuenta = "";
                //
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
                int k = 0;
                //decimal total = 0;
                List<string> _cobros = new List<string>();

                CP_INI beneficiario = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == cobro.CodigoComercio && u.Estatus == 2).FirstOrDefault();
                string _cuenta = "";
                string _nombrecomercial = "";
                string _rifc = "";
                if (beneficiario != null && beneficiario.Id > 0)
                {
                    _cuenta = beneficiario.CuentaBancaria;
                    _nombrecomercial = beneficiario.Nombre;
                    _rifc = beneficiario.RifCliente.PadLeft(9, '0');
                }

                string tipo = "03";
                //string __NumeroDocumento = ChangeString(cobro.NumeroDocumento.ToString());
                //int _length = __NumeroDocumento.Length >= 8 ? 8 : __NumeroDocumento.Length;
                //string recibo = __NumeroDocumento.Substring(__NumeroDocumento.Length - _length, _length).PadLeft(8, '0');
                decimal _cambio = Math.Round(cobro.TotalArchivo, 2);
                _cambio = _cambio * 100;
                total = total + _cambio;
                string montoacobrar = _cambio.ToString().Split(',')[0];
                string moneda = "VES";
                string numerocuenta = _cuenta;
                string swift = "UNIOVECA";
                //string _rif = Cobros.FirstOrDefault().AE_Avance.Commerce.Rif;              

                //string nombre = _nombrecomercial.Replace(".", " ").Replace(",", " ").Replace("Ñ", "N").ToUpper().TrimEnd();
                string nombre = Regex.Replace(_nombrecomercial, @"[^A-Za-z0-9- ]+", "");
                string libre = "423";
                string contrato = _rifc;
                string fechavencimiento = "       ";
                string debito = tipo + recibo.PadRight(30)
                    + montoacobrar.PadLeft(15, '0') + moneda + numerocuenta.PadRight(30)
                    + swift.PadRight(11) + _rifc.PadRight(17) + nombre.Truncate(34).PadRight(35)
                    + libre + contrato.Truncate(35).PadRight(35) + fechavencimiento;
                _cobros.Add(debito);



                //registro credito
                string _tipo2 = "02";
                int __length = __NumeroDocumento.Length >= 8 ? 8 : __NumeroDocumento.Length;
                string _recibo = __NumeroDocumento.Substring(__NumeroDocumento.Length - __length, __length).PadLeft(8, '0');

                //Cobros.First().Id.ToString().PadLeft(8, '0');



                if (asociado == "540132787")
                {
                    _rif = "J000301255";
                    ordenante = "EFE C A";
                    _numerocuenta = "01340850598503004455";
                    //_numerocuenta = "01340850578503004652";
                }
                else if (asociado == "205903844")
                {
                    _rif = "J301370139";
                    ordenante = "PEPSI C A";
                    _numerocuenta = "01340850598503004195";
                    //_numerocuenta = "01340850588503004268";
                }
                else if (asociado == "540133497")
                {
                    _rif = "J000413126";
                    ordenante = "ALIMENTOS POLAR C A";
                    _numerocuenta = "01340375913751013514";
                    //_numerocuenta = "01340850568503004482";

                }
                else if (asociado == "540130908")
                {
                    _rif = "J000063729";
                    ordenante = "CERVECERIA C A";
                    _numerocuenta = "01340850598503004357";
                    //_numerocuenta = "01340375993751007271";
                }


                //string _rif = "J401878105";
                //string ordenante = "TECNOLOGIA INSTAPAGO C A";
                //string _numerocuenta = "01340031810311158627";
                //decimal cambio = Math.Round(Cobros.Sum(y => y.Monto), 2);
                //cambio = cambio * 100;
                string _montoabono = total.ToString().Split(',')[0];
                string _moneda = "VES";
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
                string debitos = "1";
                string montototal = total.ToString().Split(',')[0];
                string totales = _tipo + totalcreditos.PadLeft(15, '0') + debitos.PadLeft(15, '0') + montototal.PadLeft(15, '0');
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = totales;


                // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
                // and then closes the file.  You do NOT need to call Flush() or Close().
                //string ruta = RUTACOBRO + "IXDP" + i.ToString().PadLeft(5, '0') + "." + asociado + "." + fechaarchivo + i++;
                string ruta = RUTACOBRO + "IXDP" + GlobalCounter.ToString().PadLeft(5, '0');
                System.IO.File.WriteAllLines(ruta, lines);
                item.Contenido = registrodecontrol + "|" + encabezado + "|" + credito;
                item.ReferenciaArchivoBanco = numeroorden;
                item.Nombre = "IXDP" + GlobalCounter.ToString().PadLeft(5, '0') + ".txt";
                item.Ruta = ruta;

                CP_ArchivoREPO.AddEntity(item);
                CP_ArchivoREPO.SaveChanges();
                cobro.ArchivoLecturaPolar = item.Id;
                GlobalCounter++;
            }

            EstadoCuentaREPO.SaveChanges();
            return true;
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

            Program p = new Program();

            StringBuilder Logs = new StringBuilder();
            //p.Test();

            Logs.Append("Iniciamos busqueda en ftp\r\n");
            //string result = p.COB_DownloadAndMove(); // Calling method
           // Logs.Append(result);
            Console.WriteLine("Finalizada la Descarga \r\n");
            Console.WriteLine("Procesamos Lectura \r\n");
            string result2 = p.COB_LecturaPOLAR(); // Calling method
            Logs.Append(result2);
            Console.WriteLine("Fin procesamiento \r\n");

            Console.WriteLine("Subiendo errores COB y PAG sin cobros \r\n");
           // string result3 = p.COBERROR_UploadAndMove();

            Console.WriteLine("Subiendo cobros \r\n");
          //  string result4 = p.COBRO_UploadAndMove();

            string RUTALOGS = ConfigurationManager.AppSettings["rutaLogs"].ToString() + "LOGS" + DateTime.Now.ToString("dd-MM-yy-ss-mm") + ".txt";
            System.IO.File.WriteAllText(RUTALOGS, Logs.ToString());

            //Console.ReadLine();




        }

    }

    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }


}
