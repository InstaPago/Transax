using System;
using System.Threading;
using System.Globalization;
using System.Web;

namespace MyHelpers.Pages
{
    /// <summary>
    /// Esta clase es la base de todas las demas paginas. Se usa para agregar codigo que tendran las paginas en comun para manejar 
    /// el final de la session entre otras cosas
    /// </summary>
    public class BasePage : System.Web.UI.Page
    {
        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public BasePage()
        {
        }

        /// <summary>
        /// INICIALIZA LA CULTURA DE LA PAGINA
        /// </summary>
        protected override void InitializeCulture()        
        {               
            
            base.InitializeCulture();
            if (!Page.IsPostBack)
            {

                CultureInfo selectedCulture = new CultureInfo("es-VE");

                // To save permanent information about user language selection, we will use cookies.
                HttpCookie cookie = Request.Cookies.Get("lang");

                // Setting up a cookie to expire in a custom-defined time frame (also defined in web.config).
                DateTime cookieExpiration = DateTime.Now.AddDays(5);

                // Now, we will check for explicit query string language selection.
                // This way we enable users to change language using url variables

                if (Session["MyCulture"] != null)
                {
                    selectedCulture = new CultureInfo(Session["MyCulture"].ToString());

                    // We will also write a cookie to remember our selection.
                    cookie = new HttpCookie("lang", selectedCulture.Name) { Expires = cookieExpiration };
                    Response.Cookies.Add(cookie);
                }
                // If no explicit selection is found, use the one saved in a cookie.
                else if (cookie != null)
                {
                    selectedCulture = new CultureInfo(cookie.Value);
                }
                // If for any reason both methods fail, fall to default settings.
                else
                {
                    // Just write a cookie to save default option.
                    cookie = new HttpCookie("lang", selectedCulture.Name) { Expires = cookieExpiration };
                    Response.Cookies.Add(cookie);

                }

                // Apply selected language to Page culture.

                selectedCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
                selectedCulture.DateTimeFormat.ShortTimePattern = String.Empty;
                Thread.CurrentThread.CurrentUICulture = selectedCulture;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(selectedCulture.Name);                
            }
        }


        /// <summary>
        /// LOAD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Page_Load(object sender, EventArgs e)
        {
            
        }
    }
}
