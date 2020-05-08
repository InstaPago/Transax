using System;
using System.Runtime.InteropServices;

namespace InstaTransfer.ScraperPresenter
{
    internal static class SNativeMethods
    {
        //External implementation for AddFontMemResourceEx
        [DllImport("gdi32.dll")]
        //Adds the font resource from a memory image to the system.
        internal static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);
    }
}
