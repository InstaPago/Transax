using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data.Linq;

namespace MyHelpers.Utilities
{
    public static class FileHelper
    {
        public static Binary FileToLinqBinary(HttpPostedFileBase file)
        {
            StreamReader streamReader = new StreamReader(file.InputStream);
            byte[] buffer = new byte[file.ContentLength];
            file.InputStream.Read(buffer, 0, file.ContentLength);
            return new System.Data.Linq.Binary(buffer);
        }

        public static byte[] LinqBinaryToFileContents(System.Data.Linq.Binary file)
        {
            byte[] fileData = file.ToArray();
            return fileData;
        }

        internal static byte[] FileToBuffer(HttpPostedFileBase fileBase)
        {
            StreamReader streamReader = new StreamReader(fileBase.InputStream);
            byte[] buffer = new byte[fileBase.ContentLength];
            fileBase.InputStream.Read(buffer, 0, fileBase.ContentLength);
            return buffer;
        }
    }
}