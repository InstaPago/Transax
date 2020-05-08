using System;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Configuration;
using InstaTransfer.ITResources.Scraper;
using InstaTransfer.ITLogic.Log;
using System.Reflection;

namespace InstaTransfer.ITLogic.Security
{
    /// <summary>
    /// Clase que maneja todos los metodos de encriptacion para InstaTransfer
    /// </summary>
    public class ITSecurity
    {
        static byte[] entropy = Encoding.Unicode.GetBytes("Salt Is Not A Password");

        /// <summary>
        /// Desencripta las credenciales del usuario
        /// </summary>
        /// <param name="encryptedData">Credencial a desencriptar</param>
        /// <returns>Credencial desencriptada</returns>
        public static string DecryptUserCredentials(string encryptedData)
        {
            string _insecureString = ToInsecureString(ITSecurity.DecryptString(encryptedData));
            return _insecureString;
        }

        /// <summary>
        /// Encrita las credenciales del usuario
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns>Credencial encriptada</returns>
        public static string EncryptUserCredentials(string rawData)
        {
            string _encryptedData = EncryptString(ToSecureString(rawData));
            return _encryptedData;
        }

        /// <summary>
        /// Obtiene las credenciales de manera segura desde el App.config
        /// </summary>
        /// <param name="key">Tipo de credencial a obtener desde el App.config</param>
        /// <returns>Credencial desencriptada en formato string</returns>
        public static string RetrieveSecureCredentials(string key)
        {
            string input = ConfigurationManager.AppSettings[key];
            string _insecureString = ITSecurity.ToInsecureString(ITSecurity.DecryptString(input));
            return _insecureString;
        }

        /// <summary>
        /// Almacena las credenciales de forma segura en el App.config
        /// </summary>
        /// <param name="input">Credencial a almacenar de forma segura</param>
        /// <param name="type">Tipo de credencial</param>
        public static void StoreSecureCredentials(string input, string key)
        {
            UpdateSetting(key, ITSecurity.EncryptString(ITSecurity.ToSecureString(input)));
        }

        /// <summary>
        /// Crea o actualiza los valores en el App.config
        /// </summary>
        /// <param name="key">Elemento key del App.config</param>
        /// <param name="value">Valor asociado al key del App.config</param>
        public static void UpdateSetting(string key, string value)
        {
            //Abre el archivo de configuracion de la aplicacion actual
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try
            {
                //Actualiza el valor del key pasado por parametros
                configuration.AppSettings.Settings[key].Value = value;
            }
            catch (NullReferenceException)
            {
                //Si el key no existe, lo crea con el nombre y valor pasado por parametros
                configuration.AppSettings.Settings.Add(key, value);
                Logger.WriteSuccessLog("Creando key en app.config", MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            finally
            {
                //Guarda los cambios realizados
                configuration.Save(ConfigurationSaveMode.Minimal);
                //Refresco los cambios al archivo
                ConfigurationManager.RefreshSection(ScraperResources.SectonAppSettings);
            }
        }

        #region Encryption

        //Todo(Security): Permitir desencriptacion desde otra pc, servidor o usuario

        public static string EncryptString(SecureString input)
        {
            byte[] encryptedData = ProtectedData.Protect(
                Encoding.Unicode.GetBytes(ToInsecureString(input)),
                entropy,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    DataProtectionScope.CurrentUser);
                return ToSecureString(Encoding.Unicode.GetString(decryptedData));
            }
            catch (CryptographicException e)
            {
                Logger.WriteWarningLog(e.Message, MethodBase.GetCurrentMethod().DeclaringType.Name);
                throw;
            }
            catch (Exception e)
            {
                Logger.WriteWarningLog(e.Message, MethodBase.GetCurrentMethod().DeclaringType.Name);
                return new SecureString();
            }
        }

        public static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = Marshal.PtrToStringBSTR(ptr);
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        #endregion

    }

}
