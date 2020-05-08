using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.Scraper;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITLogic.Security;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Scraper;
using InstaTransfer.ITResources.Scraper.ScraperProvincial;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace InstaTransfer.ITLogic.Commands.Scraper.Provincial
{
    public class CommandProvincialScraperLogin : Command
    {
        public CommandProvincialScraperLogin(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            UUser currentUser = (UUser)Receiver;
            Dictionary<string, object> parameter = (Dictionary<string, object>)Parameter;
            WebBrowser browser = (WebBrowser)parameter[ScraperResources.DictionaryKeyBrowser];
            string username = ITSecurity.DecryptUserCredentials(currentUser.Username);
            string password = ITSecurity.DecryptUserCredentials(currentUser.Password);
            string rif = currentUser.IdUSocialReason.Remove(0, 1);
            HtmlDocument doc = browser.Document;

            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new UserLoginException(GeneralErrors.UserLoginExceptionMessage);
                }

                //Obtengo el Usuario, Contraseña, y Rif de manera segura desde el App.config y lo ingreso a los campos
                //Usuario
                HtmlElement tbUser = doc.GetElementById(ScraperProvincialResources.HtmlIDLoginUsuario);
                tbUser.SetAttribute(ScraperProvincialResources.HtmlAttributeValue, username);
                tbUser.RaiseEvent(ScraperProvincialResources.HtmlOnchangeEvent);
                //Contraseña
                doc.GetElementById(ScraperProvincialResources.HtmlIDLoginClave)
                    .SetAttribute(ScraperProvincialResources.HtmlAttributeValue, password);
                //Tipo de documento
                HtmlElement select = doc.GetElementById(ScraperProvincialResources.HtmlIDTipoDoc);
                select.Children[1]
                    .SetAttribute(ScraperProvincialResources.HtmlSelectedValue, ScraperProvincialResources.HtmlSelectedValue);
                select.RaiseEvent(ScraperProvincialResources.HtmlOnchangeEvent);
                //Numero de Rif
                HtmlElement rifNum = doc.GetElementById(ScraperProvincialResources.HtmlIDRifNum);
                rifNum.SetAttribute(ScraperProvincialResources.HtmlAttributeValue, rif.Substring(0, 8));
                rifNum.RaiseEvent(ScraperProvincialResources.HtmlOnchangeEvent);
                //Numero de verificacion del RIF
                HtmlElement rifVer = doc.GetElementById(ScraperProvincialResources.HtmlIDRifID);
                rifVer.SetAttribute(ScraperProvincialResources.HtmlAttributeValue, rif[8].ToString());
                rifVer.RaiseEvent(ScraperProvincialResources.HtmlOnchangeEvent);

                //Click al boton de aceptar de la pagina web
                doc.GetElementById(ScraperProvincialResources.HtmlIDLoginEntrar)
                    .InvokeMember(ScraperResources.HtmlMemberClick);
            }
            catch (NullReferenceException e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
    }
}
