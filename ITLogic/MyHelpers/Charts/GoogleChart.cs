using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace MyHelpers.Charts
{
    public class chartshape
    {
        public String name { get; set; }
        public String type { get; set; }
        public List<int> coords { get; set; }
        //public String title { get; set; }
        public chartshape()
        {
            this.coords = new List<int>();
        }
    }

    public class chartshapes
    {
        public List<chartshape> chartshape { get; set; }
        public chartshapes()
        {
            chartshape = new List<chartshape>();
        }
    }

    public class GoogleChart
    {
        public String Url { get; set; }
        public chartshapes Map { get; set; }
        public Boolean HasMap { get; set; }
        public List<int> BarDataInt { get; set; }
        public List<Decimal> BarDataDecimal { get; set; }
        public String UnitX { get; set; }
        public String UnitY { get; set; }

        public void BarChart(List<Decimal> DataX, List<Decimal> DataY, String BackgroundColorHex,
            String BarColorHex, String TextColorHex, int Width, int Height, String Title,
            Boolean UsesMap, String UnitX, String UnitY)
        {
            Decimal __minX, __maxX;
            Decimal __minY, __maxY;
            HasMap = UsesMap;
            __minX = Math.Round(DataX.Min(), 0);
            __maxX = Math.Round(DataX.Max(), 0);
            __minY = Math.Round(DataY.Min(), 0);
            __maxY = Math.Round(DataY.Max(), 0);
            BarDataDecimal = DataY.Where(u => !u.Equals(0)).ToList();
            this.UnitX = UnitX;
            this.UnitY = UnitY;

            StringBuilder __builder = new StringBuilder();
            __builder.Append("http://chart.apis.google.com/chart?");
            __builder.Append("chf=bg,s," + BackgroundColorHex + "&"); // background color
            __builder.Append("chxl=2:|" + UnitX + "|3:|" + UnitY + "&"); // legend
            __builder.Append("chxr=0," + __minY.ToString() + "," + __maxY.ToString() + "|1," + __minX.ToString() + "," + __maxX.ToString() + "&"); // axis data
            __builder.Append("chxs=0," + TextColorHex + ",10.5,-0.5,l," + TextColorHex
                + "|1," + TextColorHex + ",11.5,0,l," + TextColorHex
                + "|2," + BarColorHex + ",11.5,0,l," + BarColorHex
                + "|3," + BarColorHex + ",11.5,0,l," + BarColorHex + "&"); // text colors
            __builder.Append("chxt=y,x,x,y&chbh=a&chs=" + Width.ToString() + "x" + Height.ToString() + "&cht=bvg&"); // width and height
            __builder.Append("chco=" + BarColorHex + "&"); // bars color
            __builder.Append("chds=" + __minY.ToString() + "," + __maxY.ToString() + "&"); // ?

            // data
            String __data = String.Empty;
            foreach (var __item in DataY)
                __data += Math.Round(__item, 0).ToString().Replace(",", ".") + ",";
            __data = __data.Remove(__data.LastIndexOf(','));
            __builder.Append("chd=t:" + __data + "&"); // data
            __builder.Append("chg=0,-1,0,0&chtt=" + Title + "&chts=" + TextColorHex + ",12.5"); // title and grid

            if (UsesMap)
            {
                WebRequest __request = WebRequest.Create(__builder.ToString() + "&chof=json");
                WebResponse __response = __request.GetResponse();
                StreamReader __reader = new StreamReader(__response.GetResponseStream());
                String __json = __reader.ReadToEnd();
                __reader.Close();
                __response.Close();

                System.Web.Script.Serialization.JavaScriptSerializer __jsonSerializer =
                    new System.Web.Script.Serialization.JavaScriptSerializer();
                this.Map = (chartshapes)__jsonSerializer.Deserialize<chartshapes>(__json);
            }

            this.Url = __builder.ToString();
        }

        public void BarChart(List<int> DataX, List<int> DataY, String BackgroundColorHex,
            String BarColorHex, String TextColorHex, int Width, int Height, String Title,
            Boolean UsesMap, String UnitX, String UnitY)
        {
            int __minX, __maxX;
            int __minY, __maxY;
            HasMap = UsesMap;
            __minX = DataX.Min();
            __maxX = DataX.Max();
            __minY = DataY.Min();
            __maxY = DataY.Max();
            BarDataInt = DataY.Where(u => !u.Equals(0)).ToList();
            this.UnitX = UnitX;
            this.UnitY = UnitY;

            StringBuilder __builder = new StringBuilder();
            __builder.Append("http://chart.apis.google.com/chart?");
            __builder.Append("chf=bg,s," + BackgroundColorHex + "&"); // background color
            __builder.Append("chxl=2:|" + UnitX + "|3:|" + UnitY + "&"); // legend
            __builder.Append("chxr=0," + __minY.ToString() + "," + __maxY.ToString() + "|1," + __minX.ToString() + "," + __maxX.ToString() + "&"); // axis data
            __builder.Append("chxs=0," + TextColorHex + ",10.5,-0.5,l," + TextColorHex
                + "|1," + TextColorHex + ",11.5,0,l," + TextColorHex
                + "|2," + BarColorHex + ",11.5,0,l," + BarColorHex
                + "|3," + BarColorHex + ",11.5,0,l," + BarColorHex + "&"); // text colors
            __builder.Append("chxt=y,x,x,y&chbh=a&chs=" + Width.ToString() + "x" + Height.ToString() + "&cht=bvg&"); // width and height
            __builder.Append("chco=" + BarColorHex + "&"); // bars color
            __builder.Append("chds=" + __minY.ToString() + "," + __maxY.ToString() + "&"); // ?

            // data
            String __data = String.Empty;
            foreach (var __item in DataY)
                __data += __item.ToString() + ",";
            __data = __data.Remove(__data.LastIndexOf(','));
            __builder.Append("chd=t:" + __data + "&"); // data
            __builder.Append("chg=0,-1,0,0&chtt=" + Title + "&chts=" + TextColorHex + ",12.5"); // title and grid

            if (UsesMap)
            {
                WebRequest __request = WebRequest.Create(__builder.ToString() + "&chof=json");
                WebResponse __response = __request.GetResponse();
                StreamReader __reader = new StreamReader(__response.GetResponseStream());
                String __json = __reader.ReadToEnd();
                __reader.Close();
                __response.Close();

                System.Web.Script.Serialization.JavaScriptSerializer __jsonSerializer =
                    new System.Web.Script.Serialization.JavaScriptSerializer();
                this.Map = (chartshapes)__jsonSerializer.Deserialize<chartshapes>(__json);
            }

            this.Url = __builder.ToString();
        }

        public void LineChart(List<Decimal> DataX, List<Decimal> DataY, String BackgroundColorHex,
            String BarColorHex, String TextColorHex, int Width, int Height, String Title,
            String UnitX, String UnitY)
        {
            Decimal __minX, __maxX;
            Decimal __minY, __maxY;
            HasMap = false;
            __minX = Math.Round(DataX.Min(), 0);
            __maxX = Math.Round(DataX.Max(), 0);
            __minY = Math.Round(DataY.Min(), 0);
            __maxY = Math.Round(DataY.Max(), 0);
            BarDataDecimal = DataY.Where(u => !u.Equals(0)).ToList();
            this.UnitX = UnitX;
            this.UnitY = UnitY;

            StringBuilder __builder = new StringBuilder();
            __builder.Append("http://chart.apis.google.com/chart?");
            __builder.Append("chf=bg,s," + BackgroundColorHex + "&"); // background color
            __builder.Append("chxl=2:|" + UnitX + "|3:|" + UnitY + "&"); // legend
            __builder.Append("chxr=0," + __minY.ToString() + "," + __maxY.ToString() + "|1," + __minX.ToString() + "," + __maxX.ToString() + "&"); // axis data
            __builder.Append("chxs=0," + TextColorHex + ",10.5,-0.5,l," + TextColorHex
                + "|1," + BackgroundColorHex + ",11.5,0,l," + BackgroundColorHex
                + "|2," + BarColorHex + ",11.5,0,l," + BarColorHex
                + "|3," + BarColorHex + ",11.5,0,l," + BarColorHex + "&"); // text colors
            __builder.Append("chxt=y,x,x,y&chbh=a&chs=" + Width.ToString() + "x" + Height.ToString() + "&cht=lc&"); // width and height
            __builder.Append("chco=" + BarColorHex + "&"); // bars color
            __builder.Append("chds=" + __minY.ToString() + "," + __maxY.ToString() + "&"); // ?

            // data
            String __data = String.Empty;
            foreach (var __item in DataY)
                __data += Math.Round(__item, 0).ToString().Replace(",", ".") + ",";
            __data = __data.Remove(__data.LastIndexOf(','));
            __builder.Append("chd=t:" + __data + "&"); // data
            __builder.Append("chg=0,-1,0,0&chtt=" + Title + "&chts=" + TextColorHex + ",12.5"); // title and grid

            this.Url = __builder.ToString();
        }

        public void PieChart(List<int> Data, List<String> Leyenda, String BackgroundColorHex,
            String MainColorsHex, String TextColorHex, int Width, int Height, String Title,
            String Unit)
        {
            int __minX, __maxX;
            HasMap = false;
            __minX = 0;
            __maxX = Data.Max();
            this.UnitX = UnitX;
            this.UnitY = UnitY;

            StringBuilder __builder = new StringBuilder();
            __builder.Append("http://chart.apis.google.com/chart?");
            __builder.Append("chf=bg,s," + BackgroundColorHex.Replace("#", "") + "&"); // background color
            __builder.Append("cht=p&"); // chart type
            __builder.Append("chco=" + MainColorsHex + "&");
            __builder.Append("chs=" + Width.ToString() + "x" + Height.ToString() + "&");
            __builder.Append("chds=" + __minX.ToString() + "," + __maxX.ToString() + "&"); // minimo y maximo

            String __data = String.Empty;
            String __leyenda = String.Empty;
            String __numeros = String.Empty;
            foreach (var __item in Data)
            {
                __data += __item.ToString().Replace(",", ".") + ",";
                __numeros += __item.ToString().Replace(",", ".") + "|";
            }
            foreach (var __item in Leyenda)
                __leyenda += __item.ToString().Replace(",", ".") + "|";

            __numeros = __numeros.Remove(__numeros.LastIndexOf("|"));
            __data = __data.Remove(__data.LastIndexOf(','));
            __leyenda = __leyenda.Remove(__leyenda.LastIndexOf("|"));

            __builder.Append("chd=t:" + __data + "&"); // data
            __builder.Append("chl=" + __numeros + "&");
            __builder.Append("chdl=" + __leyenda + "&");
            __builder.Append("chtt=" + Title);

            this.Url = __builder.ToString();
        }
    }
}
