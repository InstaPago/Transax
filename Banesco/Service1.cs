using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using InstaTransfer.BLL;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.Globalization;
using System.Threading;
using InstaTransfer.DataAccess;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet;

namespace Banesco
{
    public partial class Service1 : ServiceBase
    {

        private System.Timers.Timer TimerLecturaArchivoSalidaBanesco;

        private System.Timers.Timer TimerDownloadAll;

        private string LastRun;
        public Service1()
        {
            this.ServiceName = "CobroMasivoBanescoTransax";
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("es-VE");
            this.TimerLecturaArchivoSalidaBanesco = new System.Timers.Timer(75000D);  // 30000 milliseconds = 30 seconds
            this.TimerLecturaArchivoSalidaBanesco.AutoReset = true;
            this.TimerLecturaArchivoSalidaBanesco.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_LecturaArchivoSalidaBanesco);
            this.TimerLecturaArchivoSalidaBanesco.Start();


            
            this.TimerDownloadAll = new System.Timers.Timer(75000D);  // 30000 milliseconds = 30 seconds
            this.TimerDownloadAll.AutoReset = true;
            this.TimerDownloadAll.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_DownloadAll);
            this.TimerDownloadAll.Start();
        }

        private void timer_LecturaArchivoSalidaBanesco(object sender, System.Timers.ElapsedEventArgs e)
        {
            LecturaArchivoSalidaBanesco(); // my separate static method for do work
    
        }

        private void timer_DownloadAll(object sender, System.Timers.ElapsedEventArgs e)
        {
            DownloadAll(); // my separate static method for do work

        }

        public bool DownloadAll()
        {
            string texto = "";
            try
            {
                var host = "10.148.174.215";
                var port = 5522;
                var username = "instapag";
                var password = "540144017";
                var remoteDirectory = "/OUT";
                var backupDirectory = "/OUTbackup/";

                //string remoteDirectory = "/RemotePath/";
                string localDirectory = @"C:\Apps\Transax\Repo\RespuestaBanesco\";
                using (var sftp = new SftpClient(host, port, username, password))
                {

                    sftp.Connect();
                    texto = "conecto con el sftp | ";
                    if (sftp.IsConnected)
                    {
                        //Debug.WriteLine("I'm connected to the client");
                        if (sftp.Exists(sftp.WorkingDirectory + remoteDirectory))
                        {
                            texto = texto + "directorio existe" + sftp.WorkingDirectory + remoteDirectory + " | ";
                            //sftp.ChangeDirectory(sftp.WorkingDirectory + remoteDirectory);
                            //texto = texto + "cambie directorio" + sftp.WorkingDirectory + remoteDirectory + "| ";
                            texto = texto + "busco archivos en" + sftp.WorkingDirectory + remoteDirectory + " | ";
                            var files = sftp.ListDirectory(sftp.WorkingDirectory + remoteDirectory);
                            texto = texto + "busque archivos " + sftp.WorkingDirectory + remoteDirectory + "  -> " + files.Count().ToString() + " | ";
                            foreach (var file in files)
                            {
                                texto = texto + "encontre y recorro " + file.Name + " | ";
                                string remoteFileName = file.Name;
                                if ((!file.Name.StartsWith(".")) && ((file.LastWriteTime.Date == DateTime.Today)))
                                {
                                    texto = texto + "encontre y recorro " + localDirectory + remoteFileName + " | ";
                                    using (Stream file1 = System.IO.File.OpenWrite(localDirectory + remoteFileName))
                                    {
                                        texto = texto + "intento descargar | ";
                                        sftp.DownloadFile(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName, file1);
                                        texto = texto + "lo descargue | ";
                                        var inFile = sftp.Get(sftp.WorkingDirectory + remoteDirectory + "/" + remoteFileName);
                                        texto = texto + "lo muevo " + sftp.WorkingDirectory + backupDirectory + " | ";
                                        inFile.MoveTo(sftp.WorkingDirectory + backupDirectory + remoteFileName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //texto = texto + e.Message + "  " + e.StackTrace;
                return false;
            }
            return true;
        }

        public bool LecturaArchivoSalidaBanesco()
        {
            InstaTransfer.BLL.Concrete.URepository<CP_Archivo> ArchivoREPO = new InstaTransfer.BLL.Concrete.URepository<CP_Archivo>();
            DirectoryInfo d = new DirectoryInfo(@"C:\Apps\Transax\Repo\RespuestaBanesco\");
            //Assuming Test is your Folder
            string rutafinal = @"C:\Apps\Transax\Repo\RespuestaBanescoBackUp\";
            FileInfo[] Files = d.GetFiles("*"); //Getting Text files
            //List<Estructura> Lista = new List<Estructura>();
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
                            //EstructuraSalidaBanescoEncabezado Registro = new EstructuraSalidaBanescoEncabezado();
                            //Registro.Trading = line.Substring(0, 14).ToString().TrimEnd();
                            //Registro.Filler = line.Substring(14, 15).ToString().TrimEnd();
                            //string TipoRegistro = line.Substring(16, 18).ToString().TrimEnd();
                            //Registro.__NumeroReferenciaRespuesta = line.Substring(19, 34).ToString().TrimEnd();
                            string __NumeroReferenciaRespuesta = line.Substring(19, 34).ToString().TrimEnd();
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

                            getCP = ArchivoREPO.GetAllRecords().Where(u => u.ReferenciaArchivoBanco == __NumeroReferenciaRespuesta).FirstOrDefault();

                        }
                        else if (tipo == "02")
                        {
                            //EstructuraSalidaBanescoDetalle Registro = new EstructuraSalidaBanescoDetalle();
                            //Registro.Trading = line.Substring(0, 14).ToString().TrimEnd();
                            //Registro.Filler = line.Substring(14, 15).ToString().TrimEnd();
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

                    file.MoveTo(rutafinal + file.Name + ".txt");
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

                    file.MoveTo(rutafinal + file.Name + ".txt");
                }


            }
            return true;
        }

      
    }
}
