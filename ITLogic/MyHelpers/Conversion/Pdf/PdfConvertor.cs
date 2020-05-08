using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace MyHelpers.Conversion.Pdf
{
    public class PdfConvertor
    {
        /// <summary>
        /// Convierte un string a un archivo pdf, retornando el archivo como un byteArray. La carpeta wkhtmltopdf 
        /// contiene el programa que convierte a pdf y los dlls que necesita. Esta  carpeta debe ser guardada en la aplicación que invoca esta función                
        /// </summary>
        /// <param name="contenido"></param>
        /// <param name="programDirectory">La dirección fisica en disco de la carpeta wkhtmltopdf
        /// para convertir el string a pdf.</param>
        /// <returns></returns>
        public byte[] FromStringToByteArray(string contenido,string programDirectory)
        {            
            var fileName = " - ";
            var wkhtmlDir = programDirectory;
            var wkhtml = programDirectory + "wkhtmltopdf.exe";

            //var wkhtmlDir = "C:\\Program Files (x86)\\wkhtmltopdf\\";
            //var wkhtml = "C:\\Program Files (x86)\\wkhtmltopdf\\wkhtmltopdf.exe";

            var url = wkhtmlDir + "temp" + Guid.NewGuid().ToString() + ".html";            
            using (StreamWriter outfile = 
            new StreamWriter(url,false,Encoding.Default))
            {                
                outfile.Write(contenido);
            }

            var p = new Process();

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = wkhtml;
            p.StartInfo.WorkingDirectory = wkhtmlDir;

            string switches = "";
            switches += "--print-media-type ";
            switches += "--margin-top 10mm --margin-bottom 10mm --margin-right 10mm --margin-left 10mm ";
            switches += "--page-size Letter ";            
            p.StartInfo.Arguments = switches + " " + url + " " + fileName;
            p.Start();

            //read output
            byte[] buffer = new byte[32768];
            byte[] file;
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    int read = p.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);

                    if (read <= 0)
                    {
                        break;
                    }
                    ms.Write(buffer, 0, read);
                }
                file = ms.ToArray();
            }

            // wait or exit
            p.WaitForExit(60000);

            // read the exit code, close process
            int returnCode = p.ExitCode;
            p.Close();            
            File.Delete(url);
            return file;            
        }
    }
}
