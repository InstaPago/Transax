using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MyHelpers.Files
{
    /// <summary>
    /// clase estatica para descarga de archivos utilizando WebClient
    /// </summary>
    public static class FileDownloader
    {
        /// <summary>
        /// metodo para descarga de archivos utilizando WebClient
        /// </summary>
        /// <param name="url">el url del archivo</param>
        /// <returns>el archivo</returns>
        public static byte[] DownloadFile(string url)
        {
            WebClient _webClient = new WebClient();
            return _webClient.DownloadData(url);
        }
    }
}
