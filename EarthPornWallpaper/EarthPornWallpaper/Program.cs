using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using System.Threading;

namespace EarthPornWallpaper
{
    class Program
    {
        public const string appPath = "C:\\Users\\Sean\\git\\EarthPornWallpaper\\EarthPornWallpaper\\EarthPornWallpaper";
        static void Main(string[] args)
        {
            //constants
            const int minutes = 30;
            string subreddit = "http://www.reddit.com/r/earthporn/top/?sort=top&t=day";

            WebClient client = new WebClient();
            string htmlLoc = appPath + "earthporn.html";
            string earthpornHtml;
            string entry;
            string path;
            string title;
            Queue<string> paths = new Queue<string>();
            int entryLoc;
            int imageEnd;
            int imageLoc;
            string URL;
            int picCount = 0;
            int postCount = 17;
            bool success;
            int titleLoc;
            int titleEnd;
            string aspectRatio;
            string height;
            string width;
            double ratio;
            int aspectStart;
            int aspectEnd;
            int x;              //the location of the X in the aspect ratio
            bool foundRatio;
            double h, w;
            
            while (true)
            {
                //Download the most recent Earthporn top website
                try
                {
                    client.DownloadFile(subreddit, htmlLoc);
                }
                catch (WebException e)
                {
                    Console.WriteLine("An error occured when downloading r/earthporn\n" + e.Message);
                }

                //in a loop, download the 5 top posts right now
                while (picCount < 5 && postCount < 24)
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

                    //Check for aspect ratio between 1.25 and 1.85
                    //1.25 is the earliest TV screens, 1.85 is the cinema standard
                    x = 0;
                    aspectStart = 0;
                    foundRatio = false;
                    aspectRatio = "";
                    ratio = 1.5;        //this will set the default within the range, so if we can't determine the ratio, it won't throw out a perfectly useful image
                    for (int i = 0; i < title.Length - 1; i++)
                    {
                        if (!foundRatio)
                        {
                            aspectStart = title.IndexOf('[', i);
                            if (aspectStart >= 0)
                            {
                                aspectEnd = title.IndexOf(']', aspectStart);
                                if (aspectEnd >= 0)
                                {
                                    aspectRatio = title.Substring(aspectStart + 1, aspectEnd - aspectStart - 1).ToLower();
                                    x = aspectRatio.IndexOf("x");
                                    if (x >= 0)
                                        foundRatio = true;
                                }
                            }
                        }
                    }

                    if (foundRatio)
                    {
                        aspectStart = 0;
                        width = aspectRatio.Substring(0, x);
                        height = aspectRatio.Substring(x + 1);
                        if (double.TryParse(height, out  h) && double.TryParse(width, out w))
                        {
                            ratio = w / h;
                        }
                    }

                    if (ratio >= 1.25 && ratio <= 1.85)
                    {
                        //try to download
                        try
                        {
                            Console.WriteLine(URL);
                            path = DownloadImage(URL, postCount);
                            paths.Enqueue(path);
                            Console.WriteLine("Downloaded post " + postCount);
                            success = true;
                        }
                        catch (WebException e)
                        {
                            Console.WriteLine("An error occured when downloading the image\n" + e.Message);
                            success = false;
                        }
                        catch (FormatException f)
                        {
                            if (f.Message.Equals("URL is not a file") && URL.ToUpper().Contains("IMGUR"))
                            {
                                try
                                {
                                    path = ImgurWorkAround(URL, postCount);
                                    paths.Enqueue(path);
                                    Console.WriteLine("Used Imgur Workaround: " + URL);
                                    success = true;
                                }
                                catch (WebException g)
                                {
                                    Console.WriteLine("An error occured when downloading the image\n" + g.Message);
                                    Console.WriteLine(g.Response);
                                    success = false;
                                }
                                catch (FormatException l)
                                {
                                    Console.WriteLine("The file found was not an image\r\n" + l.Message);
                                    success = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine("The file found was not an image");
                                success = false;
                            }
                        }
                    }
                    else
                    {
                        success = false;
                    }
                    postCount++;
                    if (success)
                        picCount++;
                }

                while (paths.Count > 0)
                {
                    //set the walpaper if there is an appropriate path
                    if (!paths.Peek().Equals(""))
                        SetWallpaper(paths.Dequeue());
                    Thread.Sleep(60000 * minutes);
                }
            }

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
            Console.WriteLine(url);
            client.DownloadFile(url, path);
            return path;
        }

        /// <summary>
        /// Takes a URL for an image hosted on Imgur and downloads it
        /// </summary>
        /// <param name="url">The URL on Imgur</param>
        /// <param name="post">The post number</param>
        /// <returns>The path to where the image is saved</returns>
        public static string ImgurWorkAround(string url, int post) 
        {
            WebClient client = new WebClient();
            string htmlLoc = appPath + "Imgur.html";
            string imgurHtml;
            int entryLoc;
            int imageEnd;
            int imageLoc;
            string URL;

            //download the file
            try
            {
                client.DownloadFile(url, htmlLoc);
            }
            catch (WebException e)
            {
                Console.WriteLine("An error occured when downloading Imgur\n" + e.Message);
            }

            //read the html file
            imgurHtml = File.ReadAllText(htmlLoc);

            /* Look for the location of the image. It should be in a div
             * of class "image textbox"
             * <div class="image textbox">
             *      <a href="//IMAGEURL">
             */
            entryLoc = imgurHtml.IndexOf("<div class=\"image textbox\">") + 30;
            imageEnd = imgurHtml.IndexOf(">", entryLoc) - 14;
            imageLoc = imgurHtml.IndexOf("//", entryLoc) + 2;
            URL = "http://" + imgurHtml.Substring(imageLoc, imageEnd - imageLoc);

            
            return DownloadImage(URL, post);
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
