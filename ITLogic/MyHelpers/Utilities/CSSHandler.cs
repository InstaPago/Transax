using System;
using System.Net;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Web.Caching;
using System.Text;
using Utils.Web.HttpHandlers;
using Utils.Web;

namespace MyHelpers.Utilities
{
    public class CSSHandler : CombineHandlerBase
    {
        protected override string HandlerPrefix
        {
            get { return "CSS"; }
        }

        protected override string HandlerContentType
        {
            get { return "text/css"; }
        }

        protected override string HandlerAllowedFileType
        {
            get { return ".css"; }
        }

        protected override string MinifyFile(string fileContents)
        {
            var cssmin = new Utils.Web.CSS.CSSMinify();
            return cssmin.Minify(fileContents);
        }

        protected override string ProcessFileContents(string contents, string file)
        {
            string relativePath = VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.GetDirectory(URL.StripQueryString(file)));

            return FixImageURLs(contents, relativePath);
        }

        private string _strRelativePath;
        private string FixImageURLs(string body, string relativePath)
        {
            _strRelativePath = relativePath;
            Regex r = new Regex("url\\([\"']?(?<1>.*?)['\"]?\\)", RegexOptions.Multiline);
            return r.Replace(body, new MatchEvaluator(FixImageURL));
        }

        private string FixImageURL(Match m)
        {
            string imagePath = m.Groups[1].Value;
            if (!VirtualPathUtility.IsAbsolute(imagePath) &&
                !VirtualPathUtility.IsAppRelative(imagePath))
            {
                imagePath = VirtualPathUtility.ToAbsolute(VirtualPathUtility.Combine(_strRelativePath, imagePath));
            }
            return "url(" + imagePath + ")";
        }
    }
}
