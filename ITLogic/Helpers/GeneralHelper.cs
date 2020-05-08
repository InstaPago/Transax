using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions;
using InstaTransfer.ITExceptions.Service.Reconciliator;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Scraper;
using InstaTransfer.ITResources.Service.Reconciliator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InstaTransfer.ITLogic.Helpers
{
    public static class GeneralHelper
    {
        #region Variables

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        #endregion

        #region Methods

        #region Extensions

        /// <summary>
        /// Extension para comparar si el string contiene un valor especifico con parametros adicionales
        /// </summary>
        /// <param name="source">String original</param>
        /// <param name="value">String a buscar</param>
        /// <param name="comp">Reglas de comparacion del string</param>
        /// <returns>True - Contiene el valor. False - No contiene el valor.</returns>
        public static bool Contains(this string source, string value, StringComparison comp)
        {
            return source.IndexOf(value, comp) >= 0;
        }

        /// <summary>
        /// Extension para comparar si el string esta contenido entre varios valores
        /// </summary>
        /// <param name="source">String original</param>
        /// <param name="values">Valores string a comparar</param>
        /// <returns>True - Contiene algun valor. False - No contiene ningun valor.</returns>
        public static bool Contains(this string source, params string[] values)
        {
            return values.Any(v => source.Contains(v, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Obtiene los caracteres especificados a la derecha
        /// </summary>
        /// <param name="value">Valor del string</param>
        /// <param name="length">Numero de caracteres a obtener</param>
        /// <returns></returns>
        public static string Right(this string value, int length)
        {
            //Check if the value is valid
            if (string.IsNullOrEmpty(value))
            {
                //Set valid empty string as string could be null
                value = string.Empty;
            }
            else if (value.Length > length)
            {
                //Make the string no longer than the max length
                value = value.Substring(value.Length - length, length);
            }

            //Return the string
            return value;
        }

        #endregion

        #region Scraper

        /// <summary>
        /// Levanta el scraper de un banco especifico
        /// </summary>
        /// <param name="filePath">Ruta del ejecutable del scraper</param>
        /// <param name="bankID">Id del banco</param>

        public static void StartScraper(string filePath, string bankID)
        {
            Process scraper = new Process();
            scraper.StartInfo.FileName = filePath;
            scraper.StartInfo.Arguments = bankID;
            scraper.Start();
        }

        /// <summary>
        /// Levanta el scraper
        /// </summary>
        /// <param name="filePath">Ruta del ejecutable del scraper</param>
        public static void StartScraper(string filePath)
        {
            Process scraper = new Process();
            scraper.StartInfo.FileName = filePath;
            scraper.Start();
        }

        /// <summary>
        /// Corre el scraper para un usuario y banco especifico
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="bankID">Id del banco</param>
        /// <param name="fileName">Ruta del ejecutable</param>
        public static void StartScraper(string username, string bankID, string fileName)
        {
            try
            {
                Process scraper = new Process();
                scraper.StartInfo.FileName = fileName;
                scraper.StartInfo.Arguments = username + " " + bankID;
                scraper.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Updater
        /// <summary>
        /// Corre el actualizador de todos los estados de cuenta sin procesar
        /// </summary>
        public static void StartUpdater(string fileName)
        {
            try
            {
                Process updater = new Process();
                updater.StartInfo.FileName = fileName;
                updater.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Network

        /// <summary>
        /// Verifica si hay conexion a internet
        /// </summary>
        /// <returns>True - Hay conexion. False - No hay conexion.</returns>
        public static bool IsConnectedToInternet()
        {
            int Desc;
            bool conState = InternetGetConnectedState(out Desc, 0);
            bool isNetworkAvailable = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();


            if (conState && isNetworkAvailable)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica si una url esta en linea
        /// </summary>
        /// <param name="url"></param>
        /// <returns>True - La url esta en linea. False - La url no esta en linea.</returns>
        public static bool IsUrlOnline(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Head;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            bool pageExists = response.StatusCode == HttpStatusCode.OK;

            return true;
        }

        #endregion

        #region Config

        /// <summary>
        /// Retorna el archivo de configuracion desde la ruta especifica
        /// </summary>
        /// <param name="path">Ruta del archivo de configuracion</param>
        /// <returns>El archivo de configuracion</returns>
        public static Configuration GetAppConfig(string path)
        {
            ConfigurationFileMap fileMap = new ConfigurationFileMap(path);
            Configuration configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);

            return configuration;
        }

        /// <summary>
        /// Retorna el valor del key asociado en el app.config de una ruta especifica
        /// </summary>
        /// <param name="path">Ruta del app.config</param>
        /// <param name="key">Key del valor a retornar</param>
        /// <returns>Valor del key especificado</returns>
        public static string GetAppSettingValue(string path, string key)
        {
            ConfigurationFileMap fileMap = new ConfigurationFileMap(path);
            Configuration configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
            string settingValue = configuration.AppSettings.Settings[key].Value;

            return settingValue;
        }

        /// <summary>
        /// Retorna el valor del key asociado en el app.config
        /// </summary>
        /// <param name="key">Key del app.config</param>
        /// <returns>Valor del key especificado</returns>
        public static string GetAppSettingValue(string key)
        {
            string settingValue = ConfigurationManager.AppSettings[key];
            return settingValue;
        }

        #endregion

        #region DataAccess

        /// <summary>
        /// Obtiene todos los usuarios activos del sistema
        /// </summary>
        /// <returns>Lista de <see cref="UUser"/> activos</returns>
        public static List<UUser> GetAllActiveUmbrellaUsers()
        {
            Command commandGetAllUsers;
            List<UUser> umbrellaUsers = new List<UUser>();

            try
            {
                commandGetAllUsers = CommandFactory.GetCommandGetAllUsers();
                commandGetAllUsers.Execute();
                //Guardamos el resultado del comando como una lista de usuarios
                umbrellaUsers = (List<UUser>)commandGetAllUsers.Receiver;
            }
            catch (Exception e)
            {
                ITLogic.Log.Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            //Retornamos la lista de usuarios
            return umbrellaUsers;
        }


        #endregion

        #region Conversion

        /// <summary>
        /// Retorna el id del banco como un string
        /// </summary>
        /// <param name="bankId">Enum del id del banco</param>
        /// <returns>Representación en string del <see cref="Bank"/></returns>
        public static string GetBankIdString(Bank bankId)
        {
            string _bankIdString = ((int)bankId).ToString("0000");
            return _bankIdString;
        }

        /// <summary>
        /// Retorna el id del banco como un string desde el nombre de un banco especificado
        /// </summary>
        /// <param name="bankName">El nombre del banco</param>
        /// <returns>Representación en string del id del banco especificado/></returns>
        public static string GetBankIdString(string bankName)
        {
            string _bankIdString = ((int)Enum
                                        .Parse(typeof(Bank), bankName))
                                        .ToString("0000");
            return _bankIdString;
        }

        /// <summary>
        /// Obtiene el enum <see cref="Bank"/> asociado al Id o nombre especificado
        /// </summary>
        /// <param name="bank">Id o nombre del banco</param>
        /// <returns>Enum de tipo <see cref="Bank"/></returns>
        public static Bank GetBankEnum(string bank)
        {
            Bank _bank = (Bank)Enum.Parse(typeof(Bank), bank);
            return _bank;
        }

        #endregion

        /// <summary>
        /// Muestra un popup con el mensaje de error levantado
        /// </summary>
        /// <param name="message">Mensaje de error personalizado</param>
        public static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, GeneralErrors.SystemErrorMessage, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Detiene inmediatamente el proceso con el nombre especificado
        /// </summary>
        /// <param name="processName">El nombre del proceso</param>
        public static void KillProcess(string processName)
        {
            foreach (Process proc in Process.GetProcessesByName(processName))
            {
                proc.Kill();
            }
        }

        /// <summary>
        /// Construye la ruta de almacenamiento del EDC y el directorio si no existe.
        /// </summary>
        /// <param name="entryValues">Los parametros de entrada de la aplicacion.</param>
        /// <returns>La ruta de almacenamiento del EDC.</returns>
        /// <exception cref="IOException">Error en la ruta de la red</exception>
        public static string BuildFilePathString(string backupPath, string[] userData)
        {
            try
            {
                //Directorio del proyecto.
                //Fecha y hora actual de descarga en formato compatible para archivos.
                string now = DateTime.Now.ToString(ScraperResources.DateFormat);
                //Construimos la ruta y el nombre del archivo
                string dBackslash = ScraperResources.DoubleBackslash.Replace(@"\\", @"\");

                string pathBuilder = string.Concat(backupPath
                    + dBackslash + userData[0]
                    + dBackslash + userData[1]
                    + dBackslash + userData[2]);
                //Construimos la ruta del directorio para su creacion
                string dirPath = string.Concat(backupPath
                    + dBackslash + userData[0]
                    + dBackslash + userData[1]
                    + dBackslash);
                //Creamos el directorio si no existe
                DirectoryInfo dir = Directory.CreateDirectory(dirPath);
                //Creamos array a devolver

                return pathBuilder;
            }
            catch (IOException e)
            {
                throw new ITException
                    (
                        GeneralErrors.IOExceptionCode,
                        typeof(GeneralErrors).GetType().Name,
                        MethodBase.GetCurrentMethod().Name,
                        GeneralErrors.IOExceptionMessage,
                        e
                    );
            }
            catch (Exception)
            {
                throw;
            }

        }

        #endregion
    }




}
