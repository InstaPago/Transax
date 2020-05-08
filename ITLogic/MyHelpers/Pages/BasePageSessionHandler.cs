using System;
using System.Threading;
using System.Globalization;
using System.Web.Security;

namespace MyHelpers.Pages
{
    /// <summary>
    /// Esta clase es la base de todas las demas paginas. Se usa para agregar codigo que tendran las paginas en comun para manejar 
    /// el final de la session entre otras cosas
    /// </summary>
    public class BasePageSessionHandler : System.Web.UI.Page
    {
        public BasePageSessionHandler()
        {
        }

        /// <summary>
        /// GLOBALIZATION
        /// </summary>
        protected override void InitializeCulture()
        {
            //retrieve culture information from session
            if (Session["MyCulture"] != null)
            {
                string culture = Convert.ToString(Session["MyCulture"]);                
             
                //set culture to current thread
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            }
            
            base.InitializeCulture();
        }

        /// <summary>
        /// LOAD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Page_Load(object sender, EventArgs e)
        {
            // Añadimos la pagina actual para que quede como un URL de referencia
            //Session.Add("URLRedirect", Request.Url.AbsoluteUri);

            // Inicializamos el idioma por defect de la pagina web
            InitializeCulture();
            
            // Verificamos el estado de la sesion del usuario.
            HandleSession();




        }

        /// <summary>
        /// SESSION HANDLING
        /// </summary>
        private void HandleSession()
        {
            //Refresh the expiration on the user's authentication ticket
            try
            {
                FormsAuthenticationTicket authTicket = ((FormsIdentity)this.Page.User.Identity).Ticket;
                if (authTicket.Expired)
                {
                    Response.Redirect("~/Default.aspx?action=Warning", true);
                }

                authTicket = FormsAuthentication.RenewTicketIfOld(authTicket);

                //Validate the session - in your case, you may want to check for the
                //existence of a certain session variable
                if (Session["UserId"] == null)
                {
                    FormsAuthentication.SignOut();
                    // llama a la exception
                    Response.Redirect("~/Default.aspx?action=Warning", true);

                }
            }
            catch (Exception)
            {
                Response.Redirect("~/Default.aspx?action=Warning", true);
            }
        }
    }
}
