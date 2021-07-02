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

namespace ProcesoCLI
{
    public class Program
    {

        public string CLIERROR_UploadAndMove()
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;

            string texto = "";
            var host = "10.148.174.215";
            var port = 5522;
            var username = "instapag";
            var password = "540144017";
            //var workingdirectory = ConfigurationManager.AppSettings["rutaFtpOut"].ToString();
            var workingdirectory = "/OUTtoPolar";
            // path for file you want to upload 
            string RUTAERRORCLI = ConfigurationManager.AppSettings["rutaErrorCLIR"].ToString();
            string RUTABACKUPCLI = ConfigurationManager.AppSettings["rutaBackUpCLI"].ToString();
            InstaTransfer.BLL.Concrete.URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
            DirectoryInfo d = new DirectoryInfo(RUTAERRORCLI);
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            Console.WriteLine("procesando:" + Files.Count() + " archivos \r\n");
            texto = texto + "se encontraron:" + Files.Count() + "archivos \r\n";
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("BANERRORCLI"))
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

        public string CLI_DownloadAndMove()
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
                string RUTALECTURACLI = ConfigurationManager.AppSettings["rutaLecturaCLI"].ToString();
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

                                if ((!file.Name.StartsWith(".")) && file.Name.Contains("BANCLI"))
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

        public string CLI_LecturaPolar()
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;

            string texto = "";
            string RUTALECTURACLI = ConfigurationManager.AppSettings["rutaLecturaCLIR"].ToString();
            string RUTRAERRORCLI = ConfigurationManager.AppSettings["rutaErrorCLI"].ToString();
            string RUTABACKUPCLI = ConfigurationManager.AppSettings["rutaBackUpCLI"].ToString();
            InstaTransfer.BLL.Concrete.URepository<CP_INI> CP_IniRepo = new URepository<CP_INI>();
            DirectoryInfo d = new DirectoryInfo(RUTALECTURACLI);
            FileInfo[] Files = d.GetFiles(); //Getting Text files
            string nombrearchivo = "";
            texto = texto + "se encontraron:" + Files.Count() + "archivos \r\n";
            Console.WriteLine(texto);
            foreach (FileInfo file in Files)
            {
                texto = texto + "procesando:" + file.Name + "\r\n";
                Console.WriteLine("procesando:" + file.Name + "\r\n");
                if (file.Name.Contains("CLI0001"))
                {
                    //ALIMENTOS POLAR  J000413126 - IBS :540133497
                    nombrearchivo = "BANERRORCLI0001";
                }
                else if (file.Name.Contains("CLI0002"))
                {
                    //CERVECERIA POLAR J000063729 - IBS:540130908
                    nombrearchivo = "BANERRORCLI0002";
                }
                else if (file.Name.Contains("CLI0600"))
                {
                    //PRODUCTOS EFE J000301255  - IBS: 540132787 
                    nombrearchivo = "BANERRORCLI0600";
                }
                else if (file.Name.Contains("CLI0100"))
                {
                    //PEPSI COLA J301370139 -IBS: 205903844 
                    nombrearchivo = "BANERRORCLI0100";
                }

                List<string> errores = new List<string>();
                bool vencido = false;
                string[] lines = System.IO.File.ReadAllLines(file.FullName);
                int ultimalinea = lines.Count();
                var contenidoultimalinea = lines[ultimalinea - 1];
                string fechaarchivo = contenidoultimalinea.Substring(0, 4) + "/" + contenidoultimalinea.Substring(4, 2) + "/" + contenidoultimalinea.Substring(6, 2);
                if (DateTime.Now > DateTime.Parse(fechaarchivo).AddHours(48))
                {
                    vencido = true;

                }
                if (!vencido)
                {
                    //var engine = new FileHelperEngine<_EstructuraCLI>();
                    //var result = engine.ReadFile(file.FullName);
                    int j = 0;
                    foreach (var _item in lines)
                    {
                        if (j != (ultimalinea - 1))
                        {
                            var elemento = _item.Split('|');
                            if (elemento.Count() < 13)
                            {
                                string lineafinal = _item + "|0002|Formato Errado";
                                errores.Add(lineafinal);
                            }
                            else
                            {

                                CP_INI beneficiario = CP_IniRepo.GetAllRecords().Where(u => u.CodigoCliente == elemento[0] && u.Departamento == elemento[12] && u.Estatus == 2).FirstOrDefault();
                                if (beneficiario != null && beneficiario.Id > 0)
                                {
                                    beneficiario.ActivoCLI = elemento[10] == "0" ? false : true;
                                    beneficiario.FechaVencimiento = elemento[16] == "1" ? true : false;
                                    CP_IniRepo.SaveChanges();
                                }
                                else
                                {
                                    string lineafinal = _item + "|0001|Cliente sin carga Incial INI";
                                    errores.Add(lineafinal);
                                }

                            }
                        }
                        j++;
                    }

                    if (errores.Count > 0)
                    {
                        string[] _lines = { };
                        foreach (var _item in errores)
                        {
                            Array.Resize(ref _lines, _lines.Length + 1);
                            _lines[_lines.Length - 1] = _item;
                        }
                        string ruta = RUTRAERRORCLI + nombrearchivo + ".txt";
                        System.IO.File.WriteAllLines(ruta, _lines);

                    }
                    else
                    {

                        string line = "Sin Errores";
                        string ruta = RUTRAERRORCLI + nombrearchivo + ".txt";
                        System.IO.File.WriteAllText(ruta, line);

                    }
                }
                else
                {

                    int j = 0;
                    foreach (var _item in lines)
                    {
                        var elemento = _item.Split('|');
                        if (j != (ultimalinea - 1))
                        {

                            string lineafinal = _item + "|0001|Error Documento Fecha";
                            errores.Add(lineafinal);

                        }
                        j++;
                    }

                    texto = texto + "creando ERRORCLI:" + file.Name + "\r\n";
                    if (errores.Count > 0)
                    {
                        string[] _lines = { };
                        foreach (var _item in errores)
                        {
                            Array.Resize(ref _lines, _lines.Length + 1);
                            _lines[_lines.Length - 1] = _item;
                        }
                        string ruta = RUTRAERRORCLI + nombrearchivo + ".txt";
                        System.IO.File.WriteAllLines(ruta, _lines);

                    }
                    else
                    {
                        string line = "Sin Errores";
                        string ruta = RUTRAERRORCLI + nombrearchivo + ".txt";
                        System.IO.File.WriteAllText(ruta, line);
                    }
                }

                texto = texto + "moviendo archivo:" + file.Name + "\r\n";
                string final = RUTABACKUPCLI + file.Name + DateTime.Now.ToString("dd-MM-yy-mm-ss");
                string[] _lines2 = System.IO.File.ReadAllLines(file.FullName);
                System.IO.File.WriteAllLines(final, _lines2);
                file.Delete();

            }
            return texto;
        }

        static void Main(string[] args)
        {
            CultureInfo __newCulture;
            __newCulture = new CultureInfo("es-VE");
            Thread.CurrentThread.CurrentUICulture = __newCulture;

            Program p = new Program();
            StringBuilder Logs = new StringBuilder();
            Logs.Append("Iniciamos busqueda en ftp\r\n");
            string result = p.CLI_DownloadAndMove(); // Calling method
            Logs.Append(result);
            Console.WriteLine("Finalizada la Descarga \r\n");
            Console.WriteLine("Procesamos Lectura \r\n");
            string result2 = p.CLI_LecturaPolar(); // Calling method
            Logs.Append(result2);
            Console.WriteLine("Fin procesamiento \r\n");

            Console.WriteLine("Subiendo \r\n");
            string result3 =  p.CLIERROR_UploadAndMove();

            string RUTALOGS = ConfigurationManager.AppSettings["rutaLogs"].ToString() + "LOGS" + DateTime.Now.ToString("dd-MM-yy-ss-mm") + ".txt";
            System.IO.File.WriteAllText(RUTALOGS, Logs.ToString());
        }

    }

}
