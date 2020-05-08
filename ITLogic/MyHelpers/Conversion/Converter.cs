using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.ComponentModel;
using System.Data;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MyHelpers.Conversion
{
    public class Converter
    {
        public Converter()
        {
        }

        /// <summary>
        /// formatea un tipo de dato "Decimal" a "String" con la palabra "Bs" al final
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static String ToBs(Decimal D)
        {
            D = Math.Round(D, 2);
            String ret = D.ToString() + " Bs";
            return ret;
        }

        public static String ToDollar(Decimal D, String culture)
        {

            String ret;
            if (culture.Contains("en"))
                ret = D.ToString().Remove(D.ToString().LastIndexOf(".") + 3) + " $";
            else
                ret = D.ToString().Remove(D.ToString().LastIndexOf(",") + 3) + " $";
            return ret;
        }

        public static Decimal FromBsToDollar(Decimal D, String culture)
        {
            Decimal convertionRate;
            if (culture.Contains("en"))
                convertionRate = Convert.ToDecimal(System.Configuration.ConfigurationManager.AppSettings["ConvertionRateEng"]);
            else
                convertionRate = Convert.ToDecimal(System.Configuration.ConfigurationManager.AppSettings["ConvertionRateEsp"]);

            Decimal ret = 0;

            ret = D / convertionRate;
            return ret;
        }
        public static DataTable ConvertTo<T>(IList<T> lst)
        {
            //create DataTable Structure
            DataTable tbl = CreateTable<T>();
            Type entType = typeof(T);

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entType);
            //get the list item and add into the list
            foreach (T item in lst)
            {
                DataRow row = tbl.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    if (!prop.PropertyType.Name.Contains("Nullable"))
                        row[prop.Name] = prop.GetValue(item);
                }
                tbl.Rows.Add(row);
            }

            return tbl;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name=”T”>Custome Class</typeparam>
        /// <returns></returns>
        public static DataTable CreateTable<T>()
        {
            //T –> ClassName
            Type entType = typeof(T);
            //set the datatable name as class name
            DataTable tbl = new DataTable(entType.Name);
            String __header = String.Empty;
            //get the property list
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entType);
            foreach (PropertyDescriptor prop in properties)
            {   
                //add property as column
                if (!prop.PropertyType.Name.Contains("Nullable"))
                    tbl.Columns.Add(prop.Name, prop.PropertyType);
            }
            return tbl;
        }

        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

    }
}
