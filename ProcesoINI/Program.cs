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

namespace ProcesoCOB
{
    public class Program
    {
        URepository<CP_Archivo> ArchivoREPO = new URepository<CP_Archivo>();
        URepository<CP_ArchivoEstadoCuenta> EstadoCuentaREPO = new URepository<CP_ArchivoEstadoCuenta>();
        URepository<CP_Archivo> CP_ArchivoREPO = new URepository<CP_Archivo>();
        URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
        URepository<CP_ArchivoItem> CP_ArchivoItemRepo = new URepository<CP_ArchivoItem>();


        public string INI_DownloadAndMove()
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
                string RUTALECTURACLI = ConfigurationManager.AppSettings["rutaLecturaINI"].ToString();
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

                                if (file.Name.Contains("BANINI"))
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

        public bool INI_LecturaPolar()
        {
            string rutainipolar = ConfigurationManager.AppSettings["rutaLecturaINIR"].ToString();
            //string rutainipolar = 
            DirectoryInfo d = new DirectoryInfo(rutainipolar);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*"); //Getting Text files

            foreach (FileInfo file in Files)
            {
                List<EstructuraINI> Lista = new List<EstructuraINI>();
                List<CP_INI> ListaINI = new List<CP_INI>();
                CP_Archivo item = new CP_Archivo();
                string rifreferencia = "";
                string nombrearchivo = "";

                if (file.Name.Contains("BANINI0001"))
                {
                    //ALIMENTOS POLAR  J000413126 - IBS :540133497
                    rifreferencia = "J000413126";
                    nombrearchivo = "BANERRORINI0001";

                }
                else if (file.Name.Contains("BANINI0002"))
                {
                    //CERVECERIA POLAR J000063729 - IBS:540130908
                    rifreferencia = "J000063729";
                    nombrearchivo = "BANERRORINI0002";
                }
                else if (file.Name.Contains("BANINI0600"))
                {
                    //PRODUCTOS EFE J000301255  - IBS: 540132787 
                    rifreferencia = "J000301255";
                    nombrearchivo = "BANERRORINI0600";
                }
                else if (file.Name.Contains("BANINI0100"))
                {
                    //PEPSI COLA J301370139 -IBS: 205903844 
                    rifreferencia = "J301370139";
                    nombrearchivo = "BANERRORINI0100";
                }



                string lineas = "";
                string[] lines = System.IO.File.ReadAllLines(file.FullName);

                // Display the file contents by using a foreach loop.
                //System.Console.WriteLine("Contents of WriteLines2.txt = ");
                foreach (string line in lines)
                {
                    lineas = lineas + line + "<br />";
                    // Use a tab to indent each line of the file.
                    string sep = "\t";
                    ////string[] splitContent = content.Split(sep.ToCharArray());
                    string[] valores = line.Split(sep.ToCharArray()).ToArray();

                    EstructuraINI _registro = new EstructuraINI();
                    _registro.Fecha = valores[0];
                    _registro.RifEmpresa = valores[1];
                    _registro.Departamento = valores[2];
                    _registro.CodigoCliente = valores[3];
                    _registro.RifCliente = valores[4];
                    _registro.CuentaBancaria = valores[5];
                    _registro.Nombre = valores[6];
                    int idestatus = 1;
                    if (_registro.CodigoCliente == "")
                    {
                        idestatus = 3;
                        _registro.CodigoError = "005";
                        _registro.ErrorDescripcion = "Rif Incorrecto";
                        _registro.RifCliente = "0";

                    }
                    else if (_registro.CodigoCliente == "")
                    {
                        idestatus = 3;
                        _registro.CodigoError = "005";
                        _registro.ErrorDescripcion = "Codigo Cliente Incorrecto";
                        _registro.RifCliente = "0";
                    }
                    else
                    {
                        var __item = Lista.Where(u => u.RifCliente == _registro.RifCliente
                        //&& u.CuentaBancaria == _registro.CuentaBancaria 
                        && u.CodigoCliente == _registro.CodigoCliente).ToList();

                        var result = CP_IniRepo.GetAllRecords().Where(u => u.RifCliente == _registro.RifCliente
                        //&& u.CuentaBancaria == _registro.CuentaBancaria
                        && u.Estatus == 2
                        && u.CodigoCliente == _registro.CodigoCliente
                        && _registro.Departamento == u.Departamento).FirstOrDefault();

                        if (_registro.CuentaBancaria.Length != 20)
                        {
                            idestatus = 3;
                            _registro.CodigoError = "004";
                            _registro.ErrorDescripcion = "Error en cuenta";
                        }
                        else if (__item != null && __item.Count > 0)
                        {

                            idestatus = 3;
                            _registro.CodigoError = "002";
                            _registro.ErrorDescripcion = "Cliente Duplicado";
                            foreach (var elemento in __item)
                            {
                                elemento.CodigoError = "002";
                                elemento.ErrorDescripcion = "Cliente Duplicado";
                                var ele = ListaINI.Where(u => u.RifCliente == elemento.RifCliente && u.CuentaBancaria == elemento.CuentaBancaria && u.CodigoCliente == elemento.CodigoCliente).ToList();
                                foreach (var elementodos in ele)
                                {
                                    elementodos.Estatus = 3;
                                    elementodos.CodigoError = "002";
                                    elementodos.DescripcionError = "Cliente Duplicado";

                                }
                            }
                        }
                        else if (result != null && result.Id > 0)
                        {
                            idestatus = 3;
                            _registro.CodigoError = "006";
                            _registro.ErrorDescripcion = "Cliente con carga previa";
                        }
                        else
                        {
                            _registro.CodigoError = "";
                            _registro.ErrorDescripcion = "";
                        }
                    }

                    Lista.Add(_registro);
                    CP_INI _Ini = new CP_INI();
                    _Ini.CodigoError = _registro.CodigoError;
                    _Ini.DescripcionError = _registro.ErrorDescripcion;
                    _Ini.Fecha = valores[0];
                    _Ini.RifEmpresa = valores[1];
                    _Ini.Departamento = valores[2];
                    _Ini.CodigoCliente = valores[3];
                    _Ini.ActivoCLI = false;
                    if (valores[4].Length > 2)
                    {
                        string _tipodocumento = valores[4].Substring(0, 1).ToString();
                        string _documento = valores[4].Substring(1, (valores[4].Length - 1)).PadLeft(9, '0');
                        _Ini.RifCliente = _tipodocumento + _documento;
                    }
                    else
                    {
                        _Ini.CodigoError = "003";
                        _Ini.DescripcionError = "Error rif no valido";
                    }
                    //_Ini.RifCliente = valores[4];
                    _Ini.CuentaBancaria = valores[5];
                    _Ini.Nombre = valores[6];

                    _Ini.Estatus = idestatus;
                    _Ini.FechaCreacion = DateTime.Now;
                    _Ini.FechaActualizacion = DateTime.Now;
                    if (_Ini.RifCliente == null || _Ini.RifCliente == "")
                        _Ini.RifCliente = "-";
                    if (_Ini.CodigoCliente == null || _Ini.CodigoCliente == "")
                        _Ini.CodigoCliente = "-";
                    ListaINI.Add(_Ini);


                }
                string idarchivo = "";
                bool sinres = false;
                if (Lista.Where(u => u.CodigoError == "").Count() > 0)
                {
                    INI_GenerarAfiliaBanesco(Lista.Where(u => u.CodigoError == "").ToList(), rifreferencia, out idarchivo);
                }
                else
                {

                    sinres = true;

                }
                //CP_Beneficiario.SaveChanges();
                item.Nombre = file.Name;
                item.Ruta = file.FullName;
                item.Registro = 0;
                item.ReferenciaOrigen = "";
                item.ReferenciaArchivoBanco = idarchivo;
                item.FechaCreacion = DateTime.Now;
                item.FechaLectura = DateTime.Now;
                item.Contenido = lineas;
                item.Descripcion = "Lectura Archivo Porlar INI";
                item.Tipo = 1;
                item.EsRespuesta = false;
                item.ContenidoRespuesta = "";
                ArchivoREPO.AddEntity(item);
                ArchivoREPO.SaveChanges();

                foreach (var items in ListaINI)
                {
                    items.IdOrigen = item.Id;
                    CP_IniRepo.AddEntity(items);

                }
                CP_IniRepo.SaveChanges();

                if (sinres)
                {
                    INI_GeneraRespuestaPolar(ListaINI, nombrearchivo);
                }

            }

            foreach (var ele in Files)
            {
                string rutaIniBackUp = ConfigurationManager.AppSettings["rutaIniBackUp"].ToString();
                string final = rutaIniBackUp + ele.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                string[] _lines = System.IO.File.ReadAllLines(ele.FullName);
                System.IO.File.WriteAllLines(final, _lines);
                //texto = texto + "borrando :" + ele.Name + "\r\n";
                Console.WriteLine("borrando \r\n");
                ele.Delete();
            }
            return true;
        }

        public string INIERROR_UploadAndMove()
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
            string RUTAERRORCLI = ConfigurationManager.AppSettings["rutaErrorIniPolar"].ToString();
            string RUTABACKUPCLI = ConfigurationManager.AppSettings["rutaIniBackUp"].ToString();
            InstaTransfer.BLL.Concrete.URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
            DirectoryInfo d = new DirectoryInfo(RUTAERRORCLI);
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            Console.WriteLine("procesando:" + Files.Count() + " archivos \r\n");
            texto = texto + "se encontraron:" + Files.Count() + "archivos \r\n";
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("BANERRORINI"))
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

        public string AFILIA_UploadAndMove()
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
            string RUTACOBRO = ConfigurationManager.AppSettings["rutaSalidaAfilia"].ToString();
            string RUTABACKUPCOB = ConfigurationManager.AppSettings["rutaIniBackUp"].ToString();
            InstaTransfer.BLL.Concrete.URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
            DirectoryInfo d = new DirectoryInfo(RUTACOBRO);
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            Console.WriteLine("procesando:" + Files.Count() + " archivos \r\n");
            texto = texto + "se encontraron:" + Files.Count() + "archivos \r\n";
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("afilia."))
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
            //foreach (var ele in Files)
            //{
            //    string rutaIniBackUp = ConfigurationManager.AppSettings["rutaIniBackUp"].ToString();
            //    string final = rutaIniBackUp + ele.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
            //    string[] _lines = System.IO.File.ReadAllLines(ele.FullName);
            //    System.IO.File.WriteAllLines(final, _lines);
            //    //texto = texto + "borrando :" + ele.Name + "\r\n";
            //    Console.WriteLine("borrando \r\n");
            //    ele.Delete();
            //}
            return texto;
        }

        public string AFILIA_DownloadAndMovePOLAR()
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
                string RUTALECTURAOUT = ConfigurationManager.AppSettings["rutaLecturaRespuestaBanesco"].ToString();
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

                                if (!file.Name.StartsWith(".")
                                && file.Name.Contains("540133497RAFIL")
                                || file.Name.Contains("540130908RAFIL")
                                || file.Name.Contains("540132787RAFIL")
                                || file.Name.Contains("205903844RAFIL"))
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
        /// METODO 2 GENERA ARCHIVO AFILIA.TXT PARA BANESCO Y LO SUBE AL SFTP
        /// </summary>
        /// <param name="Cobros"></param>
        /// <param name="idarchivo"></param>
        /// <returns></returns>
        public bool INI_GenerarAfiliaBanesco(List<EstructuraINI> Cobros, string rifreferencia, out string idarchivo)
        {
            int cantidadmovimientos = Cobros.Count();
            string fechaarchivo = DateTime.Now.AddDays(0).ToString("ddMMyy.hhmm");

            string _fecha = DateTime.Now.AddDays(0).ToString("yyyyMMdd");
            //datos fijos
            string registro = "00";

            //BUSCAR RIF BASE DE DATOS
            string rif = rifreferencia;
            string Filler = "                          ";
            string Referencia = DateTime.Now.AddDays(0).ToString("ddMMyyyyhhmm");
            idarchivo = Referencia;
            string documento = "AFILIA";
            string registrodecontrol = registro + rif + Filler + Referencia.PadRight(30) + documento;

            int k = 0;

            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros)
            {

                string _art = cobro.RifCliente.Substring(1, (cobro.RifCliente.Length - 1));
                string _tipodocumento = cobro.RifCliente.Substring(0, 1).ToString();
                string _documento = _art.PadLeft(10, '0');
                string digito = "0";
                string tipocuenta = "01";
                string numerocuenta = cobro.CuentaBancaria;
                string nombre = cobro.Nombre.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                string _referencia = cobro.CodigoCliente.ToString().PadLeft(8, '0');
                string debito = _tipodocumento + _documento + digito + tipocuenta + numerocuenta.PadLeft(35, '0') + nombre.PadRight(59) + _referencia;
                _cobros.Add(debito);

            }

            string[] lines = { registrodecontrol };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().

            string salidaafilia = ConfigurationManager.AppSettings["rutaSalidaAfilia"].ToString();
            string ruta = salidaafilia + "afilia." + rifreferencia + ".txt";
            System.IO.File.WriteAllLines(ruta, lines);


            return true;
        }

        /// <summary>
        /// METODO 3 LEE SALIDA ARCHIVO AFILIA BANESCO
        /// </summary>
        /// <returns></returns>
        public bool INI_LecturaRespuestaBanesco()
        {
            string rutaLecturaRespuestaBanesco = ConfigurationManager.AppSettings["rutaLecturaRespuestaBanesco"].ToString();
            string rutaIniBackUp = ConfigurationManager.AppSettings["rutaIniBackUp"].ToString();
            DirectoryInfo d = new DirectoryInfo(rutaLecturaRespuestaBanesco);
            //Assuming Test is your Folder
            string rutafinal = rutaIniBackUp;

            FileInfo[] Files = d.GetFiles("*"); //Getting Text files

            foreach (FileInfo file in Files)
            {
                string nombrearchivo = "";
                CP_Archivo item = new CP_Archivo();
                CP_Archivo getCP = new CP_Archivo();
                if (file.Name.Contains("RAFIL"))
                {
                    if (file.Name.Contains("540133497RAFIL"))
                    {
                        //ALIMENTOS POLAR  J000413126 - IBS :540133497
                        nombrearchivo = "BANERRORINI0001";

                    }
                    else if (file.Name.Contains("540130908RAFIL"))
                    {
                        //CERVECERIA POLAR J000063729 - IBS:540130908
                        nombrearchivo = "BANERRORINI0002";
                    }
                    else if (file.Name.Contains("540132787RAFIL"))
                    {
                        //PRODUCTOS EFE J000301255  - IBS: 540132787 
                        nombrearchivo = "BANERRORINI0600";
                    }
                    else if (file.Name.Contains("205903844RAFIL"))
                    {
                        //PEPSI COLA J301370139 -IBS: 205903844 
                        nombrearchivo = "BANERRORINI0100";
                    }
                    //item.Nombre = file.Name;
                    //item.Ruta = file.FullName;
                    //item.FechaLectura = DateTime.Now;
                    string lineas = "";
                    int i = 1;
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
                    List<CP_INI> ListCP = new List<CP_INI>();
                    foreach (string line in lines)
                    {

                        lineas = lineas + line + "<br />";
                        if (i == 1)
                        {
                            string __NumeroReferenciaRespuesta = line.Substring(37, 14).ToString().TrimEnd();
                            getCP = ArchivoREPO.GetAllRecords().Where(u => u.ReferenciaArchivoBanco == __NumeroReferenciaRespuesta).FirstOrDefault();
                            if (getCP != null && getCP.Id > 0)
                                ListCP = CP_IniRepo.GetAllRecords().Where(u => u.IdOrigen == getCP.Id).ToList();
                        }
                        else
                        {
                            var identificacion = line.Substring(0, 1).ToString() + line.Substring(2, 9).ToString();
                            var respuesta = line.Substring(11, 1).ToString().TrimEnd();
                            var tipocuenta = line.Substring(12, 2).ToString().TrimEnd();
                            var cuenta = line.Substring(14, 35).ToString().TrimStart(new Char[] { '0' });
                            cuenta = "0" + cuenta;
                            var nombre = line.Substring(49, 59).ToString().TrimEnd();
                            var result = ListCP.Where(u => u.RifCliente.Trim() == identificacion.Trim() && u.CuentaBancaria.Trim() == cuenta.Trim() && u.Estatus == 1).FirstOrDefault();
                            if (result == null)
                            {
                                var _identificacion = line.Substring(0, 1).ToString() + line.Substring(1, 9).ToString();
                                result = ListCP.Where(u => u.RifCliente.Trim() == _identificacion.Trim() && u.CuentaBancaria.Trim() == cuenta.Trim() && u.Estatus == 1).FirstOrDefault();
                            }
                            if (result != null && result.Id > 0)
                            {
                                if (respuesta == "1")
                                {
                                    result.CodigoError = "001";
                                    result.DescripcionError = "Correcto";
                                    result.Estatus = 2;

                                }
                                else if (respuesta == "3")
                                {
                                    result.CodigoError = "005";
                                    result.DescripcionError = "Cuenta no pertenece al RIF";
                                    result.Estatus = 3;
                                }
                                else if (respuesta == "2")
                                {
                                    result.CodigoError = "003";
                                    result.DescripcionError = "Cuenta bancaria no corresponde";
                                    result.Estatus = 3;
                                }
                                else if (respuesta == "4")
                                {
                                    result.CodigoError = "004";
                                    result.DescripcionError = "Cuenta conjunta";
                                    result.Estatus = 3;
                                }

                                CP_IniRepo.SaveChanges();
                            }

                        }
                        i++;
                    }


                    INI_GeneraRespuestaPolar(ListCP.Where(u => u.CodigoError != "001").ToList(), nombrearchivo);


                    //file.MoveTo(rutafinal + file.Name + ".txt");
                    foreach (var ele in Files)
                    {
                        string final = rutaIniBackUp + ele.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                        string[] _lines = System.IO.File.ReadAllLines(ele.FullName);
                        System.IO.File.WriteAllLines(final, _lines);
                        //texto = texto + "borrando :" + ele.Name + "\r\n";
                        Console.WriteLine("borrando \r\n");
                        ele.Delete();
                    }


                }
                else
                {

                    foreach (var ele in Files)
                    {
                        string final = rutaIniBackUp + ele.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                        string[] _lines = System.IO.File.ReadAllLines(ele.FullName);
                        System.IO.File.WriteAllLines(final, _lines);
                        //texto = texto + "borrando :" + ele.Name + "\r\n";
                        Console.WriteLine("borrando \r\n");
                        ele.Delete();
                    }
                }


            }
            return true;
        }

        /// <summary>
        /// METODO 4  CREA ARCHIVO PARA POLAR REPSUESTA FIN DEL CICLO
        /// </summary>
        /// <param name="Cobros"></param>
        /// <returns></returns>
        public bool INI_GeneraRespuestaPolar(List<CP_INI> Cobros, string nombrearchivo)
        {
            int k = 0;
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            string sep = "\t";
            foreach (var cobro in Cobros)
            {

                string fecha = cobro.Fecha + sep;
                string Rifempresa = cobro.RifEmpresa + sep;
                string Departamento = cobro.Departamento + sep;
                string Cliente = cobro.CodigoCliente + sep;
                string RifCliente = cobro.RifCliente + sep;
                string Cuenta = cobro.CuentaBancaria + sep;
                string NombreCliente = cobro.Nombre + sep;
                string cod = cobro.CodigoError + sep;
                string error = cobro.DescripcionError;
                string debito = fecha + Rifempresa + Departamento + Cliente + RifCliente + Cuenta + NombreCliente + cod + error;
                _cobros.Add(debito);

            }
            if (_cobros.Count > 0)
            {
                string[] lines = { };
                foreach (var _item in _cobros)
                {
                    Array.Resize(ref lines, lines.Length + 1);
                    lines[lines.Length - 1] = _item;
                }

                // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
                // and then closes the file.  You do NOT need to call Flush() or Close().

                string rutainierror = ConfigurationManager.AppSettings["rutaErrorIniPolar"].ToString();
                string ruta = rutainierror + nombrearchivo;
                System.IO.File.WriteAllLines(ruta, lines);
            }
            else
            {
                string rutainierror = ConfigurationManager.AppSettings["rutaErrorIniPolar"].ToString();
                string line = "Sin Error";
                // WriteAllLines creates a file, wregistrodecontrolrites a collection of strings to the file,
                // and then closes the file.  You do NOT need to call Flush() or Close().
                string ruta = rutainierror + nombrearchivo;
                System.IO.File.WriteAllText(ruta, line);

            }


            return true;
        }


        static void Main(string[] args)
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");

            Thread.CurrentThread.CurrentUICulture = __newCulture;

            Program p = new Program();
            StringBuilder Logs = new StringBuilder();
            Logs.Append("Iniciamos busqueda en ftp\r\n");
            string result = p.INI_DownloadAndMove(); // Calling method
            Logs.Append(result);
            Console.WriteLine("Finalizada la Descarga \r\n");


            Console.WriteLine("Procesamos Lectura \r\n");
            bool result2 = p.INI_LecturaPolar(); // Calling method
            Logs.Append(result2.ToString());
            Console.WriteLine("Fin procesamiento \r\n");

            Console.WriteLine("Subiendo errores INI \r\n");
            string result3 = p.INIERROR_UploadAndMove();

            Console.WriteLine("Subiendo AFILIACION \r\n");
            string result4 = p.AFILIA_UploadAndMove();


            p.AFILIA_DownloadAndMovePOLAR();


            p.INI_LecturaRespuestaBanesco();


            p.INIERROR_UploadAndMove();


            string RUTALOGS = ConfigurationManager.AppSettings["rutaLogs"].ToString() + "LOGS" + DateTime.Now.ToString("dd-MM-yy-ss-mm") + ".txt";
            System.IO.File.WriteAllText(RUTALOGS, Logs.ToString());

            //Console.ReadLine();




        }


        public class EstructuraINI
        {
            public string Fecha { get; set; }
            public string RifEmpresa { get; set; }
            public string Departamento { get; set; }
            public string CodigoCliente { get; set; }
            public string RifCliente { get; set; }
            public string CuentaBancaria { get; set; }
            public string Nombre { get; set; }
            public string CodigoError { get; set; }

            public string ErrorDescripcion { get; set; }

        }

    }

}
