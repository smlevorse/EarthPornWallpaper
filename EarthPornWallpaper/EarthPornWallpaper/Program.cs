using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace EarthPornWallpaper
{
    class Program
    {
        static void Main(string[] args)
        {
            SetWallpaper("C:\\Users\\Sean\\git\\EarthPornWallpaper\\EarthPornWallpaper\\EarthPornWallpaper\\DesktopTest.jpg");
        }

        /***********************************************
         * Code pulled from http://tinyurl.com/q7vd47e *
         ***********************************************/
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(
            UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);
 
        private static readonly UInt32 SPI_SETDESKWALLPAPER  = 0x14;
        private static readonly UInt32 SPIF_UPDATEINIFILE    = 0x01;
        private static readonly UInt32 SPIF_SENDWININICHANGE = 0x02;
 
        //Added static modifier to copied code
        public static void SetWallpaper(String path)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        /***********************************************
         *               End Copied Code               *
         ***********************************************/
    }
}
