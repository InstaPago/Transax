using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.IO;
using InstaTransfer.BLL;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.Globalization;
using System.Threading;
using InstaTransfer.DataAccess;

namespace CobroDiario
{
    public partial class Service1 : ServiceBase
    {

        private System.Timers.Timer timerSolicitarArchivoAnalisis;
        private System.Timers.Timer timerProcesarArchivoAnalisis;
        private System.Timers.Timer timerGenerarSolicitudAPI;
        private System.Timers.Timer timerCrearSolicitud;
        private string LastRun;
        public Service1()
        {
            this.ServiceName = "CobroDiario";
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("es-VE");
            this.timerSolicitarArchivoAnalisis = new System.Timers.Timer(75000D);  // 30000 milliseconds = 30 seconds
            this.timerSolicitarArchivoAnalisis.AutoReset = true;
            this.timerSolicitarArchivoAnalisis.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_SolicitarArchivoAnalisis);
            this.timerSolicitarArchivoAnalisis.Start();

            this.timerProcesarArchivoAnalisis = new System.Timers.Timer(300000D);  // 30000 milliseconds = 30 seconds
            this.timerProcesarArchivoAnalisis.AutoReset = true;
            this.timerProcesarArchivoAnalisis.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_ProcesarArchivoAnalisis);
            this.timerProcesarArchivoAnalisis.Start();

            this.timerGenerarSolicitudAPI = new System.Timers.Timer(45000D);  // 30000 milliseconds = 30 seconds
            this.timerGenerarSolicitudAPI.AutoReset = true;
            this.timerGenerarSolicitudAPI.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_GenerarSolicitudAPI);
            this.timerGenerarSolicitudAPI.Start();


            this.timerCrearSolicitud = new System.Timers.Timer(60000D);  // 30000 milliseconds = 30 seconds
            this.timerCrearSolicitud.AutoReset = true;
            this.timerCrearSolicitud.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_CrearSolicitud);
            this.timerCrearSolicitud.Start();
        }

        private void timer_SolicitarArchivoAnalisis(object sender, System.Timers.ElapsedEventArgs e)
        {
            SolicitarArchivoAnalisis(); // my separate static method for do work
            SolicitarArchivoCobroDiario();
        }

        //private void timer_SolicitarArchivoCobroDiario(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    SolicitarArchivoCobroDiario(); // my separate static method for do work
        //}

        private void timer_ProcesarArchivoAnalisis(object sender, System.Timers.ElapsedEventArgs e)
        {
            _ProcesarArchivoAnalisis();
            _ProcesarArchivosCobroDiario();
            // my separate static method for do work
        }

        private void timer_GenerarSolicitudAPI(object sender, System.Timers.ElapsedEventArgs e)
        {
            CrearSolicitudAnalisisAPI(); // my separate static method for do work
            CrearSolicitudCobroDiarioAPI();
        }

        private void timer_CrearSolicitud(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour == 5 && DateTime.Now.Minute == 1 && DateTime.Now.ToString("ddMMyymm") != LastRun)
            {
                LastRun = DateTime.Now.ToString("ddMMyymm");
                _CrearSolicitud();
            }
        }

        public void SolicitarArchivoAnalisis()
        {
            InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi> AESolicitudApiREPO = new InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi>();
            List<AE_SolicitudApi> Pendientes = AESolicitudApiREPO.GetAllRecords(u => u.Activa && u.Analisis && u.Procesado && u.IdBankStatementRequest != null).ToList();
            foreach (var item in Pendientes)
            {
                RevisarSolicitudAnalisisAPI(item);
                AESolicitudApiREPO.SaveChanges();
            }
            //return true;
        }

        public void SolicitarArchivoCobroDiario()
        {
            InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi> AESolicitudApiREPO = new InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi>();
            List<AE_SolicitudApi> Pendientes = AESolicitudApiREPO.GetAllRecords(u => u.Activa && !u.Analisis && u.Procesado && u.IdBankStatementRequest != null).ToList();
            foreach (var item in Pendientes)
            {
                RevisarSolicitudAPI(item);
                AESolicitudApiREPO.SaveChanges();
            }
            //return true;
        }

        public void RevisarSolicitudAnalisisAPI()
        {
            _ProcesarArchivoAnalisis();

        }


        protected override void OnStop()
        {
        }
        #region SOLICITUD_API

        public bool CrearSolicitudAnalisisAPI()
        {
            InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi> AESolicitudApiREPO = new InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi>();
            InstaTransfer.BLL.Concrete.URepository<AE_UsuarioBanco> AE_UsuarioBancoREPO = new InstaTransfer.BLL.Concrete.URepository<AE_UsuarioBanco>();
            InstaTransfer.BLL.Concrete.URepository<AE_Avance> AEavanceREPO = new InstaTransfer.BLL.Concrete.URepository<AE_Avance>();
            List<InstaTransfer.DataAccess.AE_SolicitudApi> Listasolicitudes = AESolicitudApiREPO.GetAllRecords().Where(u => u.Procesado == false && u.Activa && u.IdBankStatementRequest == null).ToList();
            string Key = LoginApiScraper();
            foreach (var item in Listasolicitudes)
            {
                try
                {

                    StringBuilder MyString = new StringBuilder("");
                    int idusuariobanco = int.Parse(item.Id.Split('_')[1]);
                    InstaTransfer.DataAccess.AE_UsuarioBanco usuariobanco = AE_UsuarioBancoREPO.GetEntity(idusuariobanco);
                    MyString.Append("startdate=" + item.FechaInicio.ToString("dd/MM/yy") + "&");
                    MyString.Append("enddate=" + item.FechaFin.ToString("dd/MM/yy") + "&");
                    MyString.Append("idbank=" + usuariobanco.AccountNumber.Substring(0, 4).ToString() + "&");
                    MyString.Append("umbrellauser[accountNumber]=" + usuariobanco.AccountNumber + "&");
                    MyString.Append("umbrellauser[username]=" + usuariobanco.Username.Trim() + "&");
                    MyString.Append("umbrellauser[password]=" + usuariobanco.Password.Trim() + "&");
                    MyString.Append("umbrellauser[rif]=" + usuariobanco.RifCommerce);
                    string query = MyString.ToString();
                    //string url = "http://scrapper.transax.tech/bankstatement/create";
                    //string url = "http://scrapper.transax.tech/account/login";
                    string url = ConfigurationManager.AppSettings["ScraperCreate"].ToString();
                    byte[] queryStream = Encoding.UTF8.GetBytes(query);
                    if (item.Intentos == 0)
                    {
                        //actualizamos el valor de la solicitud
                        item.FechaPrimerRequest = DateTime.Now;
                    }
                    item.Request = MyString.ToString();
                    item.Intentos = item.Intentos + 1;
                    item.FechaUltimoRequest = DateTime.Now;

                    //Llamo al API con el url que contiene los parámetros
                    WebRequest req = WebRequest.Create(url) as HttpWebRequest;
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.ContentLength = queryStream.Length;
                    req.Headers.Add(HttpRequestHeader.Authorization, Key);

                    Stream reqStream = req.GetRequestStream();
                    reqStream.Write(queryStream, 0, queryStream.Length);
                    reqStream.Close();

                    //response
                    HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                    Stream dataStream = resp.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    //if (resp.StatusCode == HttpStatusCode.OK)
                    //{
                    //JavaScriptSerializer serializer = new JavaScriptSerializer();
                    CreateModelResponse _createmodel = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(CreateModelResponse)) as CreateModelResponse;

                    item.Response = responseFromServer.ToString();
                    item.Procesado = true;
                    //item.Intentos = item.Intentos + 1;
                    item.Message = String.Join(",", _createmodel.message);
                    if (bool.Parse(_createmodel.success))
                    {
                        item.IdBankStatementRequest = Guid.Parse(_createmodel.idbankstatementrequest);
                        item.Reprocesar = false;
                    }
                    else
                    {
                        item.Reprocesar = true;
                    }

                    AESolicitudApiREPO.SaveChanges();
                    //}
                    //else
                    //{
                    //    AESolicitudApiREPO.SaveChanges();
                    //    return false;
                    //}
                }
                catch (Exception e)
                {

                    item.Reprocesar = true;
                    item.Error = e.Message;
                    AESolicitudApiREPO.SaveChanges();

                }
            }
            return true;

        }

        public bool CrearSolicitudCobroDiarioAPI()
        {
            string Key = LoginApiScraper();
            InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi> AESolicitudApiREPO = new InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi>();
            InstaTransfer.BLL.Concrete.URepository<AE_Avance> AEavanceREPO = new InstaTransfer.BLL.Concrete.URepository<AE_Avance>();
            List<InstaTransfer.DataAccess.AE_SolicitudApi> Listasolicitudes = AESolicitudApiREPO.GetAllRecords().Where(u => u.Procesado == false && u.Activa).ToList();
            foreach (var item in Listasolicitudes)
            {
                try
                {

                    StringBuilder MyString = new StringBuilder("");
                    int idavance = int.Parse(item.Id.Split('_')[4]);
                    InstaTransfer.DataAccess.AE_Avance Avance = AEavanceREPO.GetEntity(idavance);
                    MyString.Append("startdate=" + item.FechaInicio.ToString("dd/MM/yy") + "&");
                    MyString.Append("enddate=" + item.FechaFin.ToString("dd/MM/yy") + "&");
                    MyString.Append("idbank=" + Avance.NumeroCuenta.Substring(0, 4).ToString() + "&");
                    MyString.Append("umbrellauser[accountNumber]=" + Avance.NumeroCuenta + "&");
                    MyString.Append("umbrellauser[username]=" + Avance.Usuario.Trim() + "&");
                    MyString.Append("umbrellauser[password]=" + Avance.Clave.Trim() + "&");
                    MyString.Append("umbrellauser[rif]=" + Avance.RifCommerce);
                    string query = MyString.ToString();
                    //string url = "http://scrapper.transax.tech/bankstatement/create";
                    string url = ConfigurationManager.AppSettings["ScraperCreate"].ToString();
                    byte[] queryStream = Encoding.UTF8.GetBytes(query);

                    //actualizamos el valor de la solicitud
                    if (item.Intentos == 0)
                    {
                        item.FechaPrimerRequest = DateTime.Now;
                    }
                    item.Request = MyString.ToString();
                    item.FechaUltimoRequest = DateTime.Now;
                    //Llamo al API con el url que contiene los parámetros
                    WebRequest req = WebRequest.Create(url) as HttpWebRequest;
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.ContentLength = queryStream.Length;
                    req.Headers.Add(HttpRequestHeader.Authorization, Key);
                    Stream reqStream = req.GetRequestStream();
                    reqStream.Write(queryStream, 0, queryStream.Length);
                    reqStream.Close();

                    //response
                    HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                    Stream dataStream = resp.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    //if (resp.StatusCode == HttpStatusCode.OK)
                    //{
                    //JavaScriptSerializer serializer = new JavaScriptSerializer();
                    CreateModelResponse _createmodel = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(CreateModelResponse)) as CreateModelResponse;

                    item.Response = responseFromServer.ToString();
                    item.Procesado = true;
                    item.Intentos = item.Intentos + 1;
                    item.Message = String.Join(",", _createmodel.message);
                    if (bool.Parse(_createmodel.success))
                    {
                        item.IdBankStatementRequest = Guid.Parse(_createmodel.idbankstatementrequest);
                        item.Reprocesar = false;
                    }
                    else
                    {
                        item.Reprocesar = true;
                    }
                    //}
                    //else
                    //{
                    //    return false;
                    //}
                    AESolicitudApiREPO.SaveChanges();
                }
                catch (Exception e)
                {

                    item.Reprocesar = true;
                    item.Error = e.Message;
                    AESolicitudApiREPO.SaveChanges();

                }
            }
            return true;

        }

        public string LoginApiScraper()
        {
            try
            {
                StringBuilder MyString = new StringBuilder("");

                MyString.Append("email=" + ConfigurationManager.AppSettings["ScraperUser"].ToString() + "&");
                MyString.Append("password=" + ConfigurationManager.AppSettings["ScraperPass"].ToString());
                string query = MyString.ToString();
                //string url = "http://scrapper.transax.tech/account/login";
                string url = ConfigurationManager.AppSettings["ScraperLogin"].ToString();
                byte[] queryStream = Encoding.UTF8.GetBytes(query);
                //Llamo al API con el url que contiene los parámetros
                WebRequest req = WebRequest.Create(url) as HttpWebRequest;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = queryStream.Length;

                Stream reqStream = req.GetRequestStream();
                reqStream.Write(queryStream, 0, queryStream.Length);
                reqStream.Close();
                //response
                HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                Stream dataStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                LoginModelResponse _loginmodel = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(LoginModelResponse)) as LoginModelResponse;

                return _loginmodel.token.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public bool RevisarSolicitudAnalisisAPI(InstaTransfer.DataAccess.AE_SolicitudApi item)
        {
            string Key = LoginApiScraper();
            InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi> AESolicitudApiREPO = new InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi>();
            try
            {

                StringBuilder MyString = new StringBuilder("");
                MyString.Append("id=" + item.IdBankStatementRequest);
                string query = MyString.ToString();
                // string url = "http://scrapper.transax.tech/bankstatement/get";
                string url = ConfigurationManager.AppSettings["ScraperGet"].ToString();
                byte[] queryStream = Encoding.UTF8.GetBytes(query);
                //Llamo al API con el url que contiene los parámetros
                WebRequest req = WebRequest.Create(url) as HttpWebRequest;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = queryStream.Length;
                req.Headers.Add(HttpRequestHeader.Authorization, Key);
                Stream reqStream = req.GetRequestStream();
                reqStream.Write(queryStream, 0, queryStream.Length);
                reqStream.Close();
                item.FechaUltimoRequest = DateTime.Now;
                //response
                HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                Stream dataStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                //if (resp.StatusCode == HttpStatusCode.OK)
                //{
                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                GetModelResponse _createmodel = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetModelResponse)) as GetModelResponse;
                item.FechaResponse = DateTime.Now;
                item.UltimoEstatus = _createmodel.idstatus;
                //item.Response = responseFromServer.ToString();
                item.Message = String.Join(",", _createmodel.message);
                if (_createmodel.idstatus == 1 || _createmodel.idstatus == 4 || _createmodel.idstatus == 7 || _createmodel.idstatus == 11 || _createmodel.idstatus == 12)
                {

                    item.FechaUltimoRequest = DateTime.Now;
                    //item.Response = responseFromServer.ToString();

                    // AESolicitudApiREPO.SaveChanges();
                }
                else if (_createmodel.idstatus == 2)
                {
                    //H4sIAAAAAAAEAHNLTc5IVAAB66DUtNSi1LzkzEQFa5fU4uSizILkzMOb8xTIAta++Xkl+dilghNzUrBLgaV5uQBLySuWlgAAAA==
                    var binaryData = Convert.FromBase64String(_createmodel.file.ToString());
                    byte[] finalbytes = InstaTransfer.ITLogic.Helpers.ApiHelper.Decompress(binaryData);
                    System.IO.File.WriteAllBytes(ConfigurationManager.AppSettings["RutaAnalisisPendiente"].ToString() + item.Id + ".xls", finalbytes);
                    //item.UltimoEstatus = _createmodel.idstatus;
                    item.Activa = false;
                    item.Procesado = true;
                    //AESolicitudApiREPO.SaveChanges();

                }
                else if (_createmodel.idstatus == 3 || _createmodel.idstatus == 5 || _createmodel.idstatus == 6 || _createmodel.idstatus == 8 || _createmodel.idstatus == 9 || _createmodel.idstatus == 10)
                {
                    //item.UltimoEstatus = _createmodel.idstatus;
                    item.Activa = false;
                    item.Procesado = true;
                    // AESolicitudApiREPO.SaveChanges();

                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public bool RevisarSolicitudAPI(InstaTransfer.DataAccess.AE_SolicitudApi item)
        {
            string Key = LoginApiScraper();
            //InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi> AESolicitudApiREPO = new InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi>();
            try
            {
                item.FechaUltimoRequest = DateTime.Now;
                StringBuilder MyString = new StringBuilder("");
                MyString.Append("id=" + item.IdBankStatementRequest);
                string query = MyString.ToString();
                // string url = "http://scrapper.transax.tech/bankstatement/get";
                string url = ConfigurationManager.AppSettings["ScraperGet"].ToString();
                byte[] queryStream = Encoding.UTF8.GetBytes(query);
                //Llamo al API con el url que contiene los parámetros
                WebRequest req = WebRequest.Create(url) as HttpWebRequest;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = queryStream.Length;
                req.Headers.Add(HttpRequestHeader.Authorization, Key);
                Stream reqStream = req.GetRequestStream();
                reqStream.Write(queryStream, 0, queryStream.Length);
                reqStream.Close();
                //response
                HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                Stream dataStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                //if (resp.StatusCode == HttpStatusCode.OK)
                //{
                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                GetModelResponse _createmodel = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetModelResponse)) as GetModelResponse;
                item.FechaResponse = DateTime.Now;
                //item.Response = responseFromServer.ToString();
                item.Message = String.Join(",", _createmodel.message);
                if (_createmodel.idstatus == 1 || _createmodel.idstatus == 4 || _createmodel.idstatus == 7)
                {
                    item.UltimoEstatus = _createmodel.idstatus;
                    item.FechaUltimoRequest = DateTime.Now;

                }
                else if (_createmodel.idstatus == 2)
                {
                    //H4sIAAAAAAAEAHNLTc5IVAAB66DUtNSi1LzkzEQFa5fU4uSizILkzMOb8xTIAta++Xkl+dilghNzUrBLgaV5uQBLySuWlgAAAA==
                    var binaryData = Convert.FromBase64String(_createmodel.file.ToString());
                    byte[] finalbytes = InstaTransfer.ITLogic.Helpers.ApiHelper.Decompress(binaryData);
                    System.IO.File.WriteAllBytes(ConfigurationManager.AppSettings["RutaCobroDiarioPendiente"].ToString() + item.Id + ".xls", finalbytes);
                    item.UltimoEstatus = _createmodel.idstatus;
                    item.Activa = false;
                    item.Procesado = true;
                    //AESolicitudApiREPO.SaveChanges();

                }
                else if (_createmodel.idstatus == 3 || _createmodel.idstatus == 5 || _createmodel.idstatus == 6 || _createmodel.idstatus == 7 || _createmodel.idstatus == 8 || _createmodel.idstatus == 9)
                {
                    item.UltimoEstatus = _createmodel.idstatus;
                    item.Activa = false;
                    item.Procesado = true;
                }

                return true;
            }
            catch
            {
                return false;
            }

        }

        #endregion

        #region METODOS_PRINCIPALES

        public bool _CrearSolicitud()
        {
            InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi> AESolicitudApiREPO = new InstaTransfer.BLL.Concrete.URepository<AE_SolicitudApi>();
            InstaTransfer.BLL.Concrete.URepository<AE_Avance> AEAvanceREPO = new InstaTransfer.BLL.Concrete.URepository<AE_Avance>();
            List<InstaTransfer.DataAccess.AE_Avance> avances = AEAvanceREPO.GetAllRecords().Where(u => u.IdEstatus == 1 && DateTime.Now >= u.FechaInicioCobro).ToList();
            foreach (var item in avances)
            {
                try
                {
                    InstaTransfer.DataAccess.AE_SolicitudApi nueva = new AE_SolicitudApi();
                    nueva.RifCommerce = item.RifCommerce;
                    nueva.FechaInicio = DateTime.Now;
                    nueva.FechaFin = DateTime.Now;
                    nueva.Procesado = false;
                    nueva.Reprocesar = false;
                    nueva.Intentos = 0;
                    nueva.Id = item.RifCommerce + "_" + DateTime.Now.ToString("dd-MM-yy") + "_" + DateTime.Now.ToString("ddMMyyhhss") + "_CD_" + item.Id;
                    //nueva.Id = item.RifCommerce + "_" + DateTime.Now.ToString("ddMMyy") + "_" + item.Id;
                    nueva.Activa = true;
                    nueva.Analisis = false;
                    AESolicitudApiREPO.AddEntity(nueva);


                }
                catch { }
            }
            AESolicitudApiREPO.SaveChanges();
            //LastRun = DateTime.Now;
            return true;
        }

        public bool _ProcesarArchivoAnalisis()
        {
            InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta> AEmovimientosREPO = new InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta>();
            //string _filename = DateTime.Now.ToString("hh_mm_ss") + file.FileName;
            List<string> _texto = new List<string>();
            List<InstaTransfer.DataAccess.AE_MovimientosCuenta> ListaMovimientos = new List<AE_MovimientosCuenta>();
            // bool win = SaveFile(file, _filename);
            DirectoryInfo d = new DirectoryInfo(ConfigurationManager.AppSettings["RutaGETAnalisisPendiente"].ToString());//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.xls"); //Getting Text files
                                                    //FileInfo[] Files2 = d.GetFiles("*.xlsx"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                Application xlApp = new Application();
                try
                {


                    if (!file.FullName.Contains('~'))
                    {
                        Workbook xlWorkbook = xlApp.Workbooks.Open(file.FullName);
                        _Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                        Range xlRange = xlWorksheet.UsedRange;

                        int rowCount = xlRange.Rows.Count;
                        int colCount = 4;
                        string _rif = file.Name.Substring(0, 10);
                        bool error = false;
                        for (int i = 1; i <= rowCount; i++)
                        {
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
                                    InstaTransfer.DataAccess.AE_MovimientosCuenta _Movimiento = new AE_MovimientosCuenta();
                                    Thread.CurrentThread.CurrentCulture = new CultureInfo("es-VE");
                                    var arreglo = item.Split(';');
                                    if (ValidateRow(arreglo[2]))
                                    {
                                        if (BuscarRegistro(arreglo[2].ToString(), arreglo[1].ToString(), arreglo[0], _rif))
                                        {
                                            //double d = double.Parse(arreglo[0]);
                                            //string fechaformato = arreglo[0].Split('/')[1] + "/" + arreglo[0].Split('/')[0] + "/" + arreglo[0].Split('/')[2];
                                            DateTime conv = DateTime.Parse(arreglo[0]);
                                            _Movimiento.Fecha = conv;
                                            _Movimiento.Referencia = arreglo[1].ToString();
                                            _Movimiento.Descripcion = arreglo[2].ToString();

                                            _Movimiento.Monto = decimal.Parse(arreglo[3].Replace('+', ' ').Trim().ToString());
                                            _Movimiento.RifCommerce = _rif;
                                            _Movimiento.FechaRegistro = DateTime.Now;
                                            _Movimiento.Activo = true;
                                            AEmovimientosREPO.AddEntity(_Movimiento);
                                        }
                                    }
                                    else
                                    {
                                        //if (BuscarRegistro(arreglo[2].ToString(), arreglo[1].ToString(), arreglo[0], _rif))
                                        //{
                                        //double d = double.Parse(arreglo[0]);
                                        //string fechaformato = arreglo[0].Split('/')[1] + "/" + arreglo[0].Split('/')[0] + "/" + arreglo[0].Split('/')[2];
                                        DateTime conv = DateTime.Parse(arreglo[0]);
                                        _Movimiento.Fecha = conv;
                                        _Movimiento.Referencia = arreglo[1].ToString();
                                        _Movimiento.Descripcion = arreglo[2].ToString();
                                        _Movimiento.Monto = 0;
                                        _Movimiento.RifCommerce = _rif;
                                        _Movimiento.FechaRegistro = DateTime.Now;
                                        _Movimiento.Activo = true;
                                        AEmovimientosREPO.AddEntity(_Movimiento);
                                        //}
                                    }
                                }
                            }
                        }
                        xlApp.Workbooks.Close();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                        AEmovimientosREPO.SaveChanges();
                        System.IO.File.Move(file.FullName, ConfigurationManager.AppSettings["RutaAnalisisProcesado"].ToString() + file.Name);
                        file.Delete();

                        //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp.Workbooks);

                    }
                }
                catch (Exception e)
                {
                    xlApp.Workbooks.Close();
                    //xlApp.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp.Workbooks);
                    //return false;
                }

            }

            return true;
        }

        public bool _ProcesarArchivosCobroDiario()
        {

            //InstaTransfer.BLL.Concrete.Repository._connectionString = "";
            //InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta> AEmovimientosREPO = new InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta>();
            InstaTransfer.BLL.Concrete.URepository<AE_Propuesta> AE_PropuestaREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_Propuesta>();
            InstaTransfer.BLL.Concrete.URepository<AE_Avance> AE_AvanceREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_Avance>();
            InstaTransfer.BLL.Concrete.URepository<AE_EstadoCuenta> AE_EstadoCuentaREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_EstadoCuenta>();
            InstaTransfer.BLL.Concrete.URepository<AE_MovimientosDebito> AE_MovimientosDebitoREPO = new InstaTransfer.BLL.Concrete.URepository<InstaTransfer.DataAccess.AE_MovimientosDebito>();
            InstaTransfer.BLL.Concrete.URepository<AE_Dolar> AE_DolarREPO = new InstaTransfer.BLL.Concrete.URepository<AE_Dolar>();

            InstaTransfer.DataAccess.AE_Dolar Dolar = AE_DolarREPO.GetAllRecords().OrderByDescending(u => u.FechaValor).FirstOrDefault();
            //C: \Users\carmelo\Desktop\Archivos\Pendientes
            DirectoryInfo d = new DirectoryInfo(ConfigurationManager.AppSettings["RutaGETCobroDiarioPendiente"].ToString());//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.xls"); //Getting Text files
                                                    //FileInfo[] Files2 = d.GetFiles("*.xlsx"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                decimal cobrando = 0;
                decimal cobrado = 0;
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
                    int _idavance = 0;
                    if (file.Name.Contains("."))
                        _idavance = int.Parse(file.Name.Split('.')[0].Split('_')[4]);
                    else
                        _idavance = int.Parse(file.Name.Split('_')[4]);

                    Listavance = AE_AvanceREPO.GetAllRecords().Where(u => u.RifCommerce == _rif && u.IdEstatus == 1 && u.Id == _idavance).ToList();

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
                                            //item = item + xlRange.Cells[i, j].Value2.ToString() + "~";
                                            item = item + xlRange.Cells[i, j].Value2.ToString() + ";";
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
                                        Thread.CurrentThread.CurrentCulture = new CultureInfo("es-VE");
                                        var arreglo = item.Split(';');
                                        if (ValidateRow(arreglo[2]))
                                        {
                                            if (BuscarRegistroDebito(arreglo[2].ToString(), arreglo[1].ToString(), arreglo[0], _rif))
                                            {
                                                //double _d = double.Parse(arreglo[0]);
                                                DateTime conv;
                                                try
                                                {
                                                    //double _d = double.Parse(arreglo[0]);
                                                    conv = DateTime.Parse(arreglo[0]);
                                                }
                                                catch
                                                {
                                                    double _d = double.Parse(arreglo[0]);
                                                    conv = DateTime.FromOADate(_d);
                                                }
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
                                                try
                                                {
                                                    string monto = arreglo[3].Replace('+', ' ').Trim().ToString();
                                                    if (monto.Contains(".") && monto.Contains(","))
                                                    {
                                                        string monto2 = monto.Replace(',', ' ').Trim();
                                                        string monto3 = monto2.Replace('.', ' ').Trim();
                                                        string monto4 = monto3.Replace(" ", string.Empty).Trim();
                                                        _Movimiento.Monto = decimal.Parse(monto4) / 100;
                                                    }
                                                    else if (monto.Contains("."))
                                                    {
                                                        //string monto2 = monto.Replace(',', ' ').Trim();
                                                        string monto3 = monto.Replace('.', ' ').Trim();
                                                        string monto4 = monto3.Replace(" ", string.Empty).Trim();
                                                        _Movimiento.Monto = decimal.Parse(monto4) / 100;
                                                    }
                                                    else if (monto.Contains(","))
                                                    {
                                                        string monto3 = monto.Replace(',', ' ').Trim();
                                                        string monto4 = monto3.Replace(" ", string.Empty).Trim();
                                                        _Movimiento.Monto = decimal.Parse(monto4) / 100;
                                                    }
                                                    else
                                                    {
                                                        _Movimiento.Monto = decimal.Parse(monto);
                                                    }



                                                }
                                                catch {

                                                   _Movimiento.Monto = 10;
                                                }
                                                //_Movimiento.RifCommerce = _rif;
                                                _Movimiento.FechaRegistro = DateTime.Now;
                                                _Movimiento.Activo = true;
                                                AE_MovimientosDebitoREPO.AddEntity(_Movimiento);
                                                Lista.Add(_Movimiento);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        xlApp.Workbooks.Close();
                                        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                                    }

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

                        //decimal saldofinal = _ultimo.SaldoFinal;
                        decimal pagos = estadocuentalist.Where(u => !u.Abono).Sum(u => u.Monto);
                        decimal saldofinal = (_avance.Reembolso - pagos) * Dolar.Tasa;
                        List<int> distinctLote = Lista.Select(p => p.Lote).Distinct().ToList();

                        if (saldofinal > 0)
                        {
                            foreach (var item in distinctLote)
                            {
                                List<InstaTransfer.DataAccess.AE_MovimientosDebito> _newlistbylote = Lista.Where(u => u.Lote == item).ToList();
                                decimal monto = _newlistbylote.Sum(u => u.Monto);
                                decimal debocobra = (monto * _avance.Porcentaje) / 100;
                                AE_EstadoCuenta estadocuenta = new AE_EstadoCuenta();
                                estadocuenta.MontoBase = monto;
                                estadocuenta.Abono = false;
                                estadocuenta.Estatus = 1;
                                estadocuenta.FechaOperacion = _newlistbylote.FirstOrDefault().Fecha;
                                estadocuenta.FechaRegistro = DateTime.Now;
                                estadocuenta.IdAvance = _avance.Id;
                                estadocuenta.Lote = item;
                                estadocuenta.Efectivo = false;
                                estadocuenta.EfectivoCambiado = false;
                                //estadocuenta.MontoBase = monto;
                                cobrando = cobrando + debocobra;
                                if (AE_EstadoCuentaREPO.GetAllRecords().Where(u => u.IdAvance == estadocuenta.IdAvance && u.Lote == estadocuenta.Lote && u.Monto == estadocuenta.Monto && u.MontoBase == estadocuenta.MontoBase).ToList().Count > 0)
                                {


                                }
                                else
                                {
                                    if (debocobra > saldofinal)
                                    {
                                        cobrado = cobrado + saldofinal;
                                        estadocuenta.MontoBs = saldofinal;
                                        estadocuenta.Monto = saldofinal / Dolar.Tasa;
                                        estadocuenta.Tasa = Dolar.Tasa;
                                        estadocuenta.SaldoFinal = 0;
                                        estadocuenta.SaldoInicial = saldofinal;
                                        saldofinal = saldofinal - saldofinal;
                                        _avance.IdEstatus = 2;
                                        AE_AvanceREPO.SaveChanges();
                                        Cobros.Add(estadocuenta);
                                        AE_EstadoCuentaREPO.AddEntity(estadocuenta);
                                        AE_EstadoCuentaREPO.SaveChanges();
                                        foreach (var imte2 in Lista.Where(u => u.Lote == item))
                                        {
                                            imte2.IdAE_EstadoCuenta = estadocuenta.Id;
                                        }
                                        break;

                                    }
                                    else if (cobrando > _avance.MaximoCobro)
                                    {
                                        decimal calculomonto = 0;
                                        if (cobrado == 0)
                                        {
                                            calculomonto = _avance.MaximoCobro;
                                        }
                                        else {
                                            calculomonto = _avance.MaximoCobro - cobrado;
                                        }
                                        cobrado = cobrado + calculomonto;
                                        estadocuenta.MontoBs = calculomonto;
                                        estadocuenta.Monto = calculomonto / Dolar.Tasa;
                                        estadocuenta.Tasa = Dolar.Tasa;
                                        estadocuenta.SaldoFinal = saldofinal - calculomonto;
                                        estadocuenta.SaldoInicial = saldofinal;
                                        saldofinal = saldofinal - calculomonto;
                                        Cobros.Add(estadocuenta);
                                        AE_EstadoCuentaREPO.AddEntity(estadocuenta);
                                        AE_EstadoCuentaREPO.SaveChanges();
                                        foreach (var imte2 in Lista.Where(u => u.Lote == item))
                                        {
                                            imte2.IdAE_EstadoCuenta = estadocuenta.Id;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        cobrado = cobrado + debocobra;
                                        estadocuenta.MontoBs = debocobra;
                                        estadocuenta.Monto = debocobra / Dolar.Tasa;
                                        estadocuenta.Tasa = Dolar.Tasa;
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

                        }
                        else {
                            _avance.IdEstatus = 2;
                            AE_AvanceREPO.SaveChanges();
                            return true;
                        }
                    }
                    if (Cobros.Count > 0)
                    {
                        bool win = false;
                        try
                        {
                            win = _GenerarArchivo(Cobros);
                        }
                        catch {
                            string ruta = ConfigurationManager.AppSettings["RutaCargoCuenta"].ToString() + "mamalo.txt";
                        }
                        if (win)
                        {
                            try
                            {
                                xlApp.Workbooks.Close();
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                            }
                            catch { }
                            AE_MovimientosDebitoREPO.SaveChanges();
                            try
                            {
                                System.IO.File.Move(file.FullName, ConfigurationManager.AppSettings["RutaCobroDiarioProcesado"].ToString() + file.Name);
                                file.Delete();
                            }
                            catch { };
                        }
                        else
                        {
                            try
                            {
                                //xlApp.Workbooks.Close();
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                            }
                            catch { }
                            try
                            {
                                System.IO.File.Move(file.FullName, ConfigurationManager.AppSettings["RutaCobroDiarioFalla"].ToString() + file.Name);
                                file.Delete();
                                xlApp.Workbooks.Close();
                            }
                            catch { }


                        }
                    }
                    else {
                        System.IO.File.Move(file.FullName, ConfigurationManager.AppSettings["RutaCobroDiarioFalla"].ToString() + file.Name);
                        file.Delete();
                    }
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp.Workbooks);
                }
                else
                {
                    try
                    {
                        xlApp.Workbooks.Close();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                    }
                    catch { }
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp.Workbooks);
                    try
                    {
                        System.IO.File.Move(file.FullName, ConfigurationManager.AppSettings["RutaCobroDiarioFalla"].ToString() + file.Name);
                        file.Delete();
                    }
                    catch { }
                    //AE_EstadoCuenta estadocuenta = new AE_EstadoCuenta();
                    //estadocuenta.Abono = false;
                    //estadocuenta.Estatus = 1;
                    //estadocuenta.FechaOperacion = DateTime.Parse(file.Name.Split('_')[1]);
                    //estadocuenta.FechaRegistro = DateTime.Now;
                    //estadocuenta.IdAvance = _avance.Id;
                    //estadocuenta.Lote = 0;
                    //estadocuenta.MontoBase = 0;
                    //AE_EstadoCuentaREPO.AddEntity(estadocuenta);
                    //AE_EstadoCuentaREPO.SaveChanges();
                }


                try
                {
                    //xlApp.Workbooks.Close();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                }
                catch { }
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp.Workbooks);
            }

            return true;
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

        #endregion

        #region METODOS_PRIVADOS

        public string getBetween(string strSource, string strStart, string strEnd)
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
            InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta> AEmovimientosREPO = new InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta>();
            String referecnias = ConfigurationManager.AppSettings["Referencia"].ToString();
            var lista = referecnias.Split(',');
            foreach (var item in lista)
            {
                if (item == "TT")
                {
                    if (Descripcion.Contains("TT "))
                    {
                        return true;
                    }
                }
                else
                {
                    if (Descripcion.Contains(item))
                    {
                        //if (Descripcion.Contains("L."))
                        return true;
                    }
                }
            }
            return false;

        }

        public bool BuscarRegistro(string Descripcion, string Referencia, string fecha, string _rif)
        {
            InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta> AEmovimientosREPO = new InstaTransfer.BLL.Concrete.URepository<AE_MovimientosCuenta>();
            //double d = double.Parse(fecha);
            //DateTime _fecha = DateTime.FromOADate(d);
            //string fechaformato = fecha.Split('/')[1] + "/" + fecha.Split('/')[0] + "/" + fecha.Split('/')[2];
            DateTime _fecha;
            try
            {

                _fecha = DateTime.Parse(fecha);
            }
            catch {
                double d = double.Parse(fecha);
                _fecha = DateTime.FromOADate(d);
            }
            List<InstaTransfer.DataAccess.AE_MovimientosCuenta> _movimientos = AEmovimientosREPO.GetAllRecords().Where(u => u.RifCommerce == _rif && u.Activo && u.Fecha == _fecha && u.Descripcion == Descripcion && u.Referencia == Referencia).ToList();
            if (_movimientos.Count() > 0)
            {
                return false;
            }
            return true;

        }

        public bool BuscarRegistroDebito(string Descripcion, string Referencia, string fecha, string _rif)
        {
            InstaTransfer.BLL.Concrete.URepository<AE_MovimientosDebito> AEmovimientosREPO = new InstaTransfer.BLL.Concrete.URepository<AE_MovimientosDebito>();
            //double d = double.Parse(fecha);
            //DateTime _fecha = DateTime.FromOADate(d);
            DateTime _fecha;
            try
            {

                _fecha = DateTime.Parse(fecha);
            }
            catch
            {
                double d = double.Parse(fecha);
                _fecha = DateTime.FromOADate(d);
            }
            List<InstaTransfer.DataAccess.AE_MovimientosDebito> _movimientos = AEmovimientosREPO.GetAllRecords().Where(u => u.Descripcion == Descripcion && u.Referencia == Referencia && u.Fecha == _fecha && u.Activo).ToList();
            if (_movimientos.Count() > 0)
            {
                return false;
            }
            return true;

        }

        #endregion

        #region CLASES
         
        public class LoginModelResponse
        {
            public string success { get; set; }

            public List<string> message { get; set; }
            public string token { get; set; }

        }

        public class CreateModelResponse
        {
            public string success { get; set; }

            public List<string> message { get; set; }
            public string idbankstatementrequest { get; set; }

        }

        public class GetModelResponse
        {
            public bool success { get; set; }
            public List<string> message { get; set; }
            public int idstatus { get; set; }
            public string status { get; set; }
            public object file { get; set; }

            //public int idstatus { get; set; }
        }

        #endregion
    }
}
