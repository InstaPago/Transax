using Fiddler;
using InstaTransfer.ITLogic.Security;
using InstaTransfer.ITResources.Scraper;
using Microsoft.Win32;
using System;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace InstaTransfer.ITLogic.Helpers
{
    public class FiddlerHelper
    {
        /// <summary>
        /// Levanta la aplicacion de fiddler en el puerto por defecto.
        /// </summary>
        public static void StartFiddler(int port)
        {
            //Empezamos el engine de FiddlerCore
            if (!FiddlerApplication.IsStarted())
                FiddlerApplication.Startup(port, true, true);
            //Añadimos el proxy
            if (!FiddlerApplication.IsSystemProxy())
                FiddlerApplication.oProxy.Attach();
        }

        /// <summary>
        /// Cierra la sesion de fiddler
        /// </summary>
        public static void StopFiddler()
        {
            //Removemos el proxy de fiddler si esta corriendo
            try
            {
                if (FiddlerApplication.IsSystemProxy())
                    FiddlerApplication.oProxy.Detach();
                FiddlerApplication.Shutdown();
                ITLogic.Log.Logger.WriteSuccessLog("Fiddler detenido exitosamente", MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (Exception e)
            {
                ITLogic.Log.Logger.WriteErrorLog("Error al detener fiddler: " + e.Message, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }


        }

        //Risk (InstallCertificate): La primera vez que se instala, levanta un dialogo.
        /// <summary>
        /// Instala el certificado de fiddler si ya no existe.
        /// </summary>
        /// <returns>Si fue exitosa la instalacion</returns>
        public static bool InstallCertificate()
        {
            if (!CertMaker.rootCertExists())
            {
                if (!CertMaker.createRootCert())
                    return false;

                if (!CertMaker.trustRootCert())
                    return false;

                // guarda el valor del certificado de Fiddler en el App.config

                string cert = CertMaker.GetRootCertificate().GetRawCertDataString();
                string pk = CertMaker.GetRootCertificate().GetPublicKeyString();

                try
                {
                    ITSecurity.StoreSecureCredentials(cert, ScraperResources.SettingsFiddlerCert);
                    ITSecurity.StoreSecureCredentials(pk, ScraperResources.SettingsFiddlerPK);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (!CertMaker.rootCertIsTrusted())
                if (!CertMaker.trustRootCert())
                    return false;

            return true;
        }

        /// <summary>
        /// Desinstala el certificado actual de fiddler
        /// </summary>
        /// <returns>Si fue exitosa la desinstalacion</returns>
        public static bool UninstallCertificate()
        {
            if (CertMaker.rootCertExists())
            {
                if (!CertMaker.removeFiddlerGeneratedCerts(true))
                    return false;
            }
            // guarda el valor del certificado de Fiddler en el App.config
            ITSecurity.UpdateSetting(ScraperResources.SettingsFiddlerCert, null);
            ITSecurity.UpdateSetting(ScraperResources.SettingsFiddlerPK, null);
            return true;
        }

        /// <summary>
        /// Verifica si el proxy actual esta habilitado.
        /// Si lo esta, lo reinicia y deshabilita.
        /// </summary>
        /// <param name="port">Puerto donde corre el proxy</param>
        public static void CheckProxy(int port)
        {
            // Abrimos la llave del registro que contiene el estado del proxy
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            if (registryKey != null)
            {
                // Verificamos el status del proxy
                int proxyStatus = (int)registryKey.GetValue("ProxyEnable");

                // Si el proxy esta habilitado reiniciamos Fiddler
                if (proxyStatus == 1)
                {
                    StartFiddler(port);
                    StopFiddler();
                }
                // Cerramos la llave del registro
                registryKey.Close();
            }
        }
    }
}
