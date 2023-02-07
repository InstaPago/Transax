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


namespace ProcesoFINPAGO
{
    public class Program
    {



        public string COBRO_DownloadAndMove()
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
                var remoteDirectory = "/OUT";
                var backupDirectory = "/OUTbackup/";
                string RUTALECTURAOUT = ConfigurationManager.AppSettings["rutaDescargaOUT"].ToString();
                //string remoteDirectory = "/RemotePath/";
                string localDirectory = RUTALECTURAOUT;
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
                            texto = texto + "busco archivos cobro en" + sftp.WorkingDirectory + remoteDirectory + " \r\n ";
                            var files = sftp.ListDirectory(sftp.WorkingDirectory + remoteDirectory);
                            //texto = texto + "busque archivos " + sftp.WorkingDirectory + remoteDirectory + "  -> " + files.Count().ToString() + " | ";
                            foreach (var file in files)
                            {

                                if ((!file.Name.StartsWith(".")) && (file.Name.Contains("210374095_O0002") || (file.Name.Contains("210374095_O0004"))))
                                {

                                    if (file.Name.Contains("210374095_O0004"))
                                    {
                                        string remoteFileName = file.Name;
                                        try
                                        {
                                            //using (Stream file1 = System.IO.File.OpenWrite(localDirectory + remoteFileName))
                                            //{
                                            //sftp.DownloadFile(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName, file1);
                                            //texto = texto + "lo descargue \r\n";
                                            var inFile = sftp.Get(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName);
                                            //texto = texto + "lo movi \r\n" + sftp.WorkingDirectory + backupDirectory + " | ";
                                            inFile.MoveTo(sftp.WorkingDirectory + backupDirectory + remoteFileName);
                                            //}
                                        }
                                        catch (Exception e)
                                        {
                                            texto = texto + "intento descargar y fallo : " + e.Message + " \r\n ";
                                        }

                                    }
                                    else
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
                                                inFile.MoveTo(sftp.WorkingDirectory + backupDirectory + remoteFileName);
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

        /// <summary>
        /// Este metodo pertenece para cosas de POLAR deberia migrarlo algun servicio de POLAR
        /// </summary>
        /// <returns></returns>
        public string COBRO_DownloadAndMovePOLAR()
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
                //var remoteDirectory = "/OUT";
                var remoteDirectory = "/filezip_Resp";
                //var backupDirectory = "/OUTbackup/";
                var backupDirectory = "/filezip_bkp/";
                string RUTALECTURAOUT = ConfigurationManager.AppSettings["rutaDescargaOUTPOLAR"].ToString();
                //string remoteDirectory = "/RemotePath/";
                string localDirectory = RUTALECTURAOUT;
                using (var sftp = new SftpClient(host, port, username, password))
                {
                    sftp.Connect();
                    texto = "conecto con el sftp \r\n ";
                    Console.WriteLine("conetando");
                    if (sftp.IsConnected)
                    {
                        //Debug.WriteLine("I'm connected to the client");
                        if (sftp.Exists(sftp.WorkingDirectory + remoteDirectory))
                        {
                            Console.WriteLine("directorio existe");
                            texto = texto + "directorio existe" + sftp.WorkingDirectory + remoteDirectory + " \r\n ";
                            //Console.WriteLine(texto);
                            //sftp.ChangeDirectory(sftp.WorkingDirectory + remoteDirectory);
                            //texto = texto + "cambie directorio" + sftp.WorkingDirectory + remoteDirectory + "| ";
                            texto = texto + "busco archivos cobro en" + sftp.WorkingDirectory + remoteDirectory + " \r\n ";
                         
                            var files = sftp.ListDirectory(sftp.WorkingDirectory + remoteDirectory);
                            //texto = texto + "busque archivos " + sftp.WorkingDirectory + remoteDirectory + "  -> " + files.Count().ToString() + " | ";
                            foreach (var file in files)
                            {
                                Console.WriteLine(file.Name);
                                if (file.Name.Contains("BANSTA_IP")  || file.Name.Contains(".CSV"))
                                {

                                    //if (file.Name.Contains("_O0004"))
                                    //{
                                    //    string remoteFileName = file.Name;
                                    //    try
                                    //    {
                                    //        //using (Stream file1 = System.IO.File.OpenWrite(localDirectory + remoteFileName))
                                    //        //{
                                    //        //sftp.DownloadFile(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName, file1);
                                    //        //texto = texto + "lo descargue \r\n";
                                    //        var inFile = sftp.Get(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName);
                                    //        //texto = texto + "lo movi \r\n" + sftp.WorkingDirectory + backupDirectory + " | ";
                                    //        inFile.MoveTo(sftp.WorkingDirectory + backupDirectory + remoteFileName);
                                    //        //}
                                    //    }
                                    //    catch (Exception e)
                                    //    {
                                    //        texto = texto + "intento descargar y fallo : " + e.Message + " \r\n ";
                                    //    }

                                    //}
                                    //else
                                    //{
                                    texto = texto + "encontre: " + file.Name + "\r\n";
                                    string remoteFileName = file.Name;
                                    Console.WriteLine("econtre:"+ file.Name);
                                    //texto = texto + "encontre y recorro " + localDirectory + remoteFileName + " | ";
                                    try
                                    {
                                        using (Stream file1 = System.IO.File.OpenWrite(localDirectory + DateTime.Now.ToString("yyyyMMdd") + remoteFileName ))
                                        {
                                            Console.WriteLine(sftp.WorkingDirectory);
                                            Console.WriteLine(remoteDirectory + "/" + DateTime.Now.ToString("yyyyMMdd") + remoteFileName);
                                            Console.WriteLine(sftp.WorkingDirectory + remoteDirectory + "/" + DateTime.Now.ToString("yyyyMMdd") + remoteFileName);
                                            Console.WriteLine(file1);
                                            sftp.DownloadFile(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName, file1);
                                            texto = texto + "lo descargue \r\n";
                                            Console.WriteLine("Descargando");

                                            var inFile = sftp.Get(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName);
                                            texto = texto + "lo movi \r\n" + sftp.WorkingDirectory + backupDirectory + " | ";
                                            //Console.WriteLine("Moviendo - " + sftp.WorkingDirectory + remoteDirectory + "/Backup/BANSTA_IP_" + DateTime.Now.ToString("yyyyMMdd") + ".CSV");
                                            //inFile.MoveTo(sftp.WorkingDirectory + remoteDirectory + "/Backup/BANSTA_IP_" + DateTime.Now.ToString("yyyyMMdd") + ".CSV");
                                            
                                            inFile.Delete();
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message);
                                        Console.WriteLine(e.StackTrace);
                                        texto = texto + "intento descargar y fallo : " + e.Message + " \r\n ";
                                    }
                                    //}

                                    

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
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return texto;
            }
            //Console.WriteLine(texto);
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
            string RUTACOBRO = ConfigurationManager.AppSettings["rutacobrolocalR"].ToString();
            string RUTABACKUPCOB = ConfigurationManager.AppSettings["rutacobrolocalBackUp"].ToString();
            InstaTransfer.BLL.Concrete.URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
            DirectoryInfo d = new DirectoryInfo(RUTACOBRO);
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            Console.WriteLine("procesando:" + Files.Count() + " archivos \r\n");
            texto = texto + "se encontraron:" + Files.Count() + "archivos \r\n";
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("I0005.210374095"))
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




        public string PROCESARCOBRO()
        {
            URepository<CP_Archivo> ArchivoREPO = new URepository<CP_Archivo>();
            DirectoryInfo d = new DirectoryInfo(@"C:\Apps\Transax\Repo\RespuestaBanesco\");
            //Assuming Test is your Folder
            string rutafinal = @"C:\Apps\Transax\Repo\RespuestaBanescoBackUp\";
            FileInfo[] Files = d.GetFiles("*"); //Getting Text files
            List<EstructuraCOB> Lista = new List<EstructuraCOB>();
            CP_Archivo item = new CP_Archivo();
            CP_Archivo getCP = new CP_Archivo();
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("0002"))
                {
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
                            Registro.Trading = line.Substring(0, 14).ToString().TrimEnd();
                            Registro.Filler = line.Substring(14, 15).ToString().TrimEnd();
                            Registro.TipoRegistro = line.Substring(16, 18).ToString().TrimEnd();
                            Registro.__NumeroReferenciaRespuesta = line.Substring(19, 34).ToString().TrimEnd();
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
                            var _list = ArchivoREPO.GetAllRecords().Where(u => u.ReferenciaArchivoBanco == Registro.__NumeroReferenciaRespuesta).ToList();

                            if (_list.Count > 0)
                            {
                                getCP = _list.FirstOrDefault();
                            }
                            else
                            {

                                //getCP.ContenidoRespuesta = lineas;
                                //getCP.EsRespuesta = true;
                                //ArchivoREPO.SaveChanges();


                                string _final = rutafinal + file.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                                string[] _lines3 = System.IO.File.ReadAllLines(file.FullName);
                                System.IO.File.WriteAllLines(_final, _lines3);
                                file.Delete();

                            }



                        }
                        else if (tipo == "02")
                        {
                            //EstructuraSalidaBanescoDetalle Registro = new EstructuraSalidaBanescoDetalle();
                            //Registro.Trading = line.Substring(0, 14).ToString().TrimEnd();
                            //Registro.Filler = line.Substring(1y bajando la respuesta exploto 4, 15).ToString().TrimEnd();
                            //Registro.TipoRegistro = line.Substring(16, 17).ToString().TrimEnd();
                            //Registro.Indicador = line.Substring(17, 18).ToString().TrimEnd();
                            //Registro.NumeroReferencia = line.Substring(19, 53).ToString().TrimEnd();
                            //Registro.Fecha = line.Substring(54, 61).ToString().TrimEnd();
                            //Registro.Monto = line.Substring(62, 76).ToString().TrimEnd();
                            //Registro.Moneda = line.Substring(77, 79).ToString().TrimEnd();
                            //Registro.Rif = line.Substring(80, 96).ToString().TrimEnd();
                            //Registro.NumeroCuenta = line.Substring(97, 131).ToString().TrimEnd();
                            //Registro.BancoBeneficiario = line.Substring(132, 142).ToString().TrimEnd();
                            //Registro.BancoBeneficiarioDescripcion = line.Substring(143, 212).ToString().TrimEnd();
                            //Registro.CodigoAgencia = line.Substring(213, 215).ToString().TrimEnd();
                            //Registro.NombreBeneficiario = line.Substring(216, 285).ToString().TrimEnd();
                            //Registro.NumeroCliente = line.Substring(286, 320).ToString().TrimEnd();
                            //Registro.FechaVencimiento = line.Substring(321, 326).ToString().TrimEnd();
                            //Registro.NumeroSecuenciaArchivo = line.Substring(327, 332).ToString().TrimEnd();

                            //string _line = Registro.NumeroCuenta + "|1|";

                            //Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                            //linesArchivo[linesArchivo.Length - 1] = _line;
                        }
                        else if (tipo == "03")
                        {
                            //EstructuraSalidaBanescoEstatus Registro = new EstructuraSalidaBanescoEstatus();
                            //Registro.Trading = line.Substring(0, 14).ToString().TrimEnd();
                            //Registro.Filler = line.Substring(14, 15).ToString().TrimEnd();
                            //Registro.TipoRegistro = line.Substring(16, 17).ToString().TrimEnd();
                            //Registro._CodigoEstatus = line.Substring(19, 21).ToString().TrimEnd();
                            //Registro._Descripcion = line.Substring(22, 91).ToString().TrimEnd();

                            //string _line = Registro._CodigoEstatus + "|1|";

                            //Array.Resize(ref linesArchivo, linesArchivo.Length + 1);
                            //linesArchivo[linesArchivo.Length - 1] = _line;
                        }
                        else if (tipo == "04")
                        {

                        }

                        //}
                        //i++;
                    }

                    getCP.FechaLectura = DateTime.Now;
                    getCP.ContenidoRespuesta = lineas;
                    getCP.EsRespuesta = true;

                    ArchivoREPO.SaveChanges();


                    string final = rutafinal + file.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                    string[] _lines2 = System.IO.File.ReadAllLines(file.FullName);
                    System.IO.File.WriteAllLines(final, _lines2);
                    file.Delete();
                    //file.MoveTo(rutafinal + file.Name + ".txt");
                    //item.Contenido = lineas;
                    //item.Descripcion = "[FINPAGO] RESPUESTA Cargo cuenta masivo.";
                    //item.Tipo = 1;
                    //item.IdEmpresa = 1;
                    //item.IdReferencia = Guid.NewGuid();
                    //item.

                    //ArchivoREPO.AddEntity(item);
                }
                else
                {
                    string final = rutafinal + file.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                    string[] _lines2 = System.IO.File.ReadAllLines(file.FullName);
                    System.IO.File.WriteAllLines(final, _lines2);
                    file.Delete();
                    //file.MoveTo(rutafinal + file.Name + ".txt");
                }


            }

            return "true";
        }



        static void Main(string[] args)
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;

            Program p = new Program();
            StringBuilder Logs = new StringBuilder();
   
            Logs.Append("Iniciamos busqueda en Sftp\r\n");

            p.COBRO_DownloadAndMovePOLAR();
            //string result = p.COBRO_UploadAndMove(); // Calling method
            //Logs.Append(result);
            //Console.WriteLine("Subimos Archivoss \r\n");
            //Console.WriteLine("Buscamos si hay descargas descargas \r\n");
            //string result2 = p.COBRO_DownloadAndMove(); // Calling method
            //Logs.Append(result2);
            Console.WriteLine("Fin descarga \r\n");

            //Console.WriteLine("Procesamos archivos locales \r\n");
            //string result3 = p.PROCESARCOBRO();



            //Console.ReadLine();

            //string RUTALOGS = ConfigurationManager.AppSettings["rutaLogs"].ToString() + "LOGS" + DateTime.Now.ToString("dd-MM-yy-ss-mm") + ".txt";
            //System.IO.File.WriteAllText(RUTALOGS, Logs.ToString());
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
