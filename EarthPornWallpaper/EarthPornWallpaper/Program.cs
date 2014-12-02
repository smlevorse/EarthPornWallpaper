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
            string path;
            string title;
            List<string> paths = new List<string>();
            int entryLoc;
            int imageEnd;
            int imageLoc;
            string URL;
            int picCount = 0;
            int postCount = 1;
            bool success;
            int titleLoc;
            int titleEnd;

            //Download the most recent Earthporn top website
            try
            {
                client.DownloadFile("http://www.reddit.com/r/earthporn/top/?sort=top&t=day", htmlLoc);
            }
            catch (WebException e)
            {
                Console.WriteLine("An error occured when downloading r/earthporn\n" + e.Message);
            }

            //in a loop, download the 5 top posts right now
            while (picCount < 5 && postCount < 20)
            {
                //read the html file
                earthpornHtml = File.ReadAllText(htmlLoc);

                /* look for the top post
                 * it should be in a div that contains a span:
                 * <span class="rank">
                 */
                entryLoc = earthpornHtml.IndexOf("<span class=\"rank\">" + postCount);
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

                Console.WriteLine("Post {0} URL: " + URL, postCount);

                /* Get the title of the post, it should be wrapped in a 
                 * <a> tag, but as the content instead of an attribute
                 * <a class="title may-blank href="IMAGEURL" tabindex="1">
                 *      ::before
                 *      "POSTTITLE"
                 * </a>
                 */

                titleLoc = earthpornHtml.IndexOf("<a class=", imageEnd);
                titleEnd = earthpornHtml.IndexOf("</a>", titleLoc);
                title = earthpornHtml.Substring(titleLoc, titleEnd - titleLoc);
                titleLoc = title.IndexOf(">") + 1;
                title = title.Substring(titleLoc);
                Console.WriteLine("Post {0} Title: " + title, postCount);

                //try to download
                try
                {
                    path = DownloadImage(URL, postCount);
                    paths.Add(path);
                    success = true;
                }
                catch (WebException e)
                {
                    Console.WriteLine("An error occured when downloading the image\n" + e.Message);
                    success = false;
                }
                catch (FormatException f)
                {
                    Console.WriteLine("The file found was not an image");
                    success = false;
                }

                postCount++;
                if (success)
                    picCount++;
            }

            //set the walpaper if there is an appropriate path
            if(!paths[1].Equals(""))
                SetWallpaper(paths[1]);

        }

        /// <summary>
        /// Downloads the image and stores it to a file
        /// </summary>
        /// <returns>The path of the image</returns>
        public static string DownloadImage(string url, int post) 
        {
            //get the format
            string format = url.Substring(url.Length - 4);
            while (!format.StartsWith("."))
            {
                if (format.Length > 0)
                    format = format.Substring(1);
                else
                    throw new FormatException("URL is not a file");
            }

            //before we downlaod, check that the file is an image so that we don't acidentally download a virus
            if (!IsImageFormat(format))
                throw new FormatException("The file type was not an image");

            //prepare to download
            string path = appPath + "\\Images\\Desktop" + post + format;
            WebClient client = new WebClient();

            //download the image
            client.DownloadFile(url, path);
            return path;
        }


        /// <summary>
        /// Takes the file type and determines if it is an image
        /// </summary>
        /// <param name="fileExtension">The filetype to analize</param>
        /// <returns>If the given filetype is an image</returns>
        public static bool IsImageFormat(string fileExtension)
        {
            fileExtension = fileExtension.ToUpper();
            if (   fileExtension.Equals(".JPG") || fileExtension.Equals(".JPEG") || fileExtension.Equals(".JIF") || fileExtension.Equals(".JFIF")
                || fileExtension.Equals(".TIF") || fileExtension.Equals(".TIFF")
                || fileExtension.Equals(".GIF") || fileExtension.Equals(".PNG")
                || fileExtension.Equals(".JP2") || fileExtension.Equals(".JPX") || fileExtension.Equals(".J2K") || fileExtension.Equals(".J2C")
                || fileExtension.Equals(".BMP"))
                return true;
            return false;
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
