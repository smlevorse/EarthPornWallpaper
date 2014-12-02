using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;

namespace EarthPornWallpaper
{
    class Program
    {
        public const string appPath = "C:\\Users\\Sean\\git\\EarthPornWallpaper\\EarthPornWallpaper\\EarthPornWallpaper";
        static void Main(string[] args)
        {
            WebClient client = new WebClient();
            string htmlLoc = appPath + "earthporn.html";
            string earthpornHtml;
            string entry;
            int entryLoc;
            int imageEnd;
            int imageLoc;
            string URL;

            //Download the most recent Earthporn top website
            try
            {
                client.DownloadFile("http://www.reddit.com/r/earthporn/top/?sort=top&t=day", htmlLoc);
            }
            catch (WebException e)
            {
                Console.WriteLine("An error occured when downloading r/earthporn\n" + e.Message);
            }

            //read the html file
            earthpornHtml = File.ReadAllText(htmlLoc);
            
            /* look for the top post
             * it should be in a div that contains a span:
             * <span class="rank">
             */
            entryLoc = earthpornHtml.IndexOf("<span class=\"rank\">5");
            /* look for the start of the a tag that contains
             * the link to the image:
             * <a class="thumbnail may-blank " href="IMAGEURL
             */
            entryLoc = earthpornHtml.IndexOf("<a class=\"thumbnail may-blank \"", entryLoc);
            imageEnd = earthpornHtml.IndexOf(">", entryLoc) - 2;
            entry = earthpornHtml.Substring(entryLoc, imageEnd - entryLoc);
            
            //break the url out of the entry
            imageLoc = entry.IndexOf("href") + 6;
            URL = entry.Substring(imageLoc);
            //URL = "https://farm8.staticflickr.com/7520/15732565938_3618efdc32_k.jpg";
            Console.WriteLine("Image downloaded from: " + URL);

            try
            {
                SetWallpaper(DownloadImage(URL));
            }
            catch (WebException e)
            {
                Console.WriteLine("An error occured when downloading the image\n" + e.Message);
            }
            
        }

        /// <summary>
        /// Downloads the image and stores it to a file
        /// </summary>
        /// <returns>The path of the image</returns>
        public static string DownloadImage(string url) 
        {
            string path = appPath + "\\Images\\DesktopTest3.jpg";
            WebClient client = new WebClient();

            client.DownloadFile(url, path);

            return path;
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
