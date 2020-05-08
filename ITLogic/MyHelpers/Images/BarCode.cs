using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MyHelpers.Images
{
    public class BarCode
    {
        public static Image GetBarcode(string text,int weight)
        {
            return GenCode128.Code128Rendering.MakeBarcodeImage(text, weight, true);
        }
    }
}
