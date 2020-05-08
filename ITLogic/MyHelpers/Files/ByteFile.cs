using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyHelpers.Files
{
    public class ByteFile
    {
        public string nombre { get; set; }
        public string extencion { get; set; }
        public byte[] archivo { get; set; }

        public ByteFile(string nombre,string extencion, byte[] archivo)
        {
            this.nombre = nombre;
            this.extencion = extencion;
            this.archivo = archivo;
        }
    }
}
