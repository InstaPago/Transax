using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.Scraper;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITLogic.Security;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Scraper;
using InstaTransfer.ITResources.Scraper.ScraperBanesco2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace InstaTransfer.ITLogic.Commands.Scraper.Banesco2
{
    public class CommandBanesco2ScraperLogin : Command
    {
        public CommandBanesco2ScraperLogin(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            UUser currentUser = (UUser)Receiver;
            try
            {
                Dictionary<string, object> parameter = (Dictionary<string, object>)Parameter;
                WebBrowser browser = (WebBrowser)parameter[ScraperResources.DictionaryKeyBrowser];
                string username = ITSecurity.DecryptUserCredentials(currentUser.Username);
                string password = ITSecurity.DecryptUserCredentials(currentUser.Password);
                HtmlDocument doc = browser.Document;
                // Obtengo el documento desde el frame de los elementos del login
                var fdoc = doc.Window.Frames[ScraperBanesco2Resources.HtmlIDFrameLogin].Document;


                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new UserLoginException(GeneralErrors.UserLoginExceptionMessage);
                }

                #region Obsolete
                ////Ingreso las credenciales desencriptadas en los campos de Usuario y Clave de la pagina web
                //fdoc.GetElementById(ScraperBanesco2Resources.HtmlIDLoginUsuario)
                //    .SetAttribute(ScraperResources.HtmlAttributeValue, username);
                //fdoc.GetElementById(ScraperBanesco2Resources.HtmlIDLoginClave)
                //    .SetAttribute(ScraperResources.HtmlAttributeValue, password);
                ////Click al boton de aceptar de la pagina web
                //fdoc.GetElementById(ScraperBanesco2Resources.HtmlIDLoginAceptar)
                //    .InvokeMember(ScraperResources.HtmlMemberClick);
                #endregion

            }
            catch (NullReferenceException e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
    }
}
