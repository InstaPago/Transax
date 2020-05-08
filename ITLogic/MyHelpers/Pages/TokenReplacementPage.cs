using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.UI;

namespace MyHelpers.Pages
{
    public abstract class TokenReplacementPage :  System.Web.UI.Page
    {
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            // mecanismo propio para guardar el page output
            StringBuilder pageSource = new StringBuilder();
            StringWriter sw = new StringWriter(pageSource);
            HtmlTextWriter htmlWriter = new HtmlTextWriter(sw);
            base.Render(htmlWriter);

            // replacements
            RunGlobalReplacements(pageSource);
            RunPageReplacements(pageSource);

            // output replacements
            writer.Write(pageSource.ToString());            
        }

        /// <summary>
        /// reemplazos globales a todas las paginas que hereden de esta pagina
        /// </summary>
        /// <param name="pageSource"></param>
        protected void RunGlobalReplacements(StringBuilder pageSource)
        {
               
        }

        /// <summary>
        /// reemplazos particulares de las paginas que hereden de esta
        /// </summary>
        /// <param name="pageSource"></param>
        protected virtual void RunPageReplacements(StringBuilder pageSource) { }
    }
}
