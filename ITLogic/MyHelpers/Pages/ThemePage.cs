using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace MyHelpers.Pages
{
    public class ThemePage : System.Web.UI.Page
    {
        protected void Page_PreInit(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                String QueryString = Request.QueryString["IdSeccion"];
                int IdSeccion = 1;
                try
                {
                    IdSeccion = Convert.ToInt32(QueryString);
                    switch (IdSeccion)
                    {
                        case 1:
                            Session.Add("MyTheme", "Home");
                            break;
                        case 2:
                            Session.Add("MyTheme", "Musica");
                            break;
                        case 3:
                            Session.Add("MyTheme", "Deportes");
                            break;
                        case 4:
                            Session.Add("MyTheme", "Teatro");
                            break;
                        case 5:
                            Session.Add("MyTheme", "Especiales");
                            break;
                        default:
                            Session.Add("MyTheme", "Home");
                            break;
                    }
                    Page.Theme = ((string)Session["MyTheme"]);
                }
                catch (FormatException) { }


            }
        }
    }

}