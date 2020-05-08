using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.Scraper;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITLogic.Security;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Scraper;
using InstaTransfer.ITResources.Scraper.ScraperBanesco;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace InstaTransfer.ITLogic.Commands.Scraper.Banesco
{
    public class CommandBanescoScraperLogin : Command
    {
        public CommandBanescoScraperLogin(Object receiver) : base(receiver) { }

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

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new UserLoginException(GeneralErrors.UserLoginExceptionMessage);
                }

                //Ingreso las credenciales desencriptadas en los campos de Usuario y Clave de la pagina web
                doc.GetElementById(ScraperBanescoResources.HtmlIDLoginUsuario)
                    .SetAttribute(ScraperResources.HtmlAttributeValue, username);
                doc.GetElementById(ScraperBanescoResources.HtmlIDLoginClave)
                    .SetAttribute(ScraperResources.HtmlAttributeValue, password);
                //Click al boton de aceptar de la pagina web
                doc.GetElementById(ScraperBanescoResources.HtmlIDLoginAceptar)
                    .InvokeMember(ScraperResources.HtmlMemberClick);
            }
            catch (NullReferenceException e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
    }
}
