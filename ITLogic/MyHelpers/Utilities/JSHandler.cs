using System;
using System.Net;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Web.Caching;
using System.Text;
using System.Collections.Generic;
using Utils.Web.JS;
using Utils.Web.HttpHandlers;

namespace MyHelpers.Utilities
{
    /// <summary>
    /// Handles javascript files. 
    /// Allows for many js files to be combinded into a single request.
    /// Gives js files an expire response header
    /// minifies js files to save on bandwidth
    /// </summary>
    public class JSHandler : CombineHandlerBase
    {
        protected override string HandlerPrefix
        {
            get { return "Scripts"; }
        }

        protected override string HandlerContentType
        {
            get { return "text/javascript"; }
        }

        protected override string HandlerAllowedFileType
        {
            get { return ".js"; }
        }

        protected override string HandlerFileSeperator
        {
            get { return "\r\n;\r\n"; }
        }

        protected override string MinifyFile(string fileContents)
        {
            Utils.Web.JS.JavascriptMinifier jsm = new Utils.Web.JS.JavascriptMinifier();
            return jsm.MinifyString(fileContents);
        }

        //make sure the file is not minified or packed already
        protected override bool CanMinifyFile(string filePath)
        {
            return !filePath.EndsWith(".min.js", StringComparison.InvariantCultureIgnoreCase) &&
                !filePath.EndsWith(".pack.js", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
