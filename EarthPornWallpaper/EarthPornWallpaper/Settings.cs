using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace EarthPornWallpaper
{
    public partial class Settings : Form
    {
        //fields
        private static string subreddit;
        private static Queue<string[]> posts;   //{path, title, user, URL, aspectRatio, imageURL}
        private static string url;
        private static string imageURL;

        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            subreddit = "http://www.reddit.com/r/earthporn/top/?sort=top&t=day";
            posts = new Queue<string[]>();
            butNext_Click(sender, e);
            
            this.Hide();
        }

        //hide the form instead of closing it
        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
        
        //reopen the form
        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void cmbInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            string interval = cmbInterval.Text;
            int milliseconds;
            switch (interval)
            { 
                /* Possible intervals
                 * 5 Minutes
                 * 10 Minutes
                 * 15 Minutes
                 * 30 Minutes
                 * 1 Hour
                 * 2 Hours
                 * 3 Hours
                 * 4 Hours
                 * 6 Hours
                 * 12 Hours
                 * 1 Day
                 */
                case "5 Minutes":
                    milliseconds = 300000;
                    break;
                case "10 Minutes":
                    milliseconds = 600000;
                    break;
                case "30 Minutes":
                    milliseconds = 1800000;
                    break;
                case "1 Hour":
                    milliseconds = 3600000;
                    break;
                case "2 Hours":
                    milliseconds = 7200000;
                    break;
                case "3 Hours":
                    milliseconds = 10800000;
                    break;
                case "4 Hours":
                    milliseconds = 14400000;
                    break;
                case "6 Hours":
                    milliseconds = 21600000;
                    break;
                case "12 Hours":
                    milliseconds = 43200000;
                    break;
                case "1 Day":
                    milliseconds = 86400000;
                    break;
                default:
                    milliseconds = 30000;
                    break;
            }
            timer.Interval = milliseconds;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            butNext_Click(sender, e);
        }

        private void butNext_Click(object sender, EventArgs e)
        {
            //if there are no more images to draw from, download the top posts again
            if (posts.Count <= 0)
            {
                DownloadTopPosts();
            }

            //set the walpaper if there is an appropriate path
            if (posts.Count > 0 && posts.Peek().Length == 6)
            {
                string[] data = posts.Dequeue();    //{path, title, user, URL, aspectRatio, imageURL}
                SetWallpaper(data[0]);
                picBoxCurrent.ImageLocation = data[0];
                lblTitle.Text = "Title: " + data[1];
                lblUser.Text = "User: " + data[2];
                url = data[3];
                lblAspectRatio.Text = "Aspect Ratio: " + data[4];
                imageURL = data[5];
            }
            
        }


        private void butDownload_Click(object sender, EventArgs e)
        {
            
            //Open folder dialogue to allow the user to select where they want to save the image:
            string format = GetFormat(imageURL);
            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = format;
            saveFileDialog.OverwritePrompt = true;
            DialogResult result = saveFileDialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                try
                {
                    DownloadImage(imageURL, saveFileDialog.FileName);
                }
                catch (WebException web)
                { 
                    Console.WriteLine("Web Exception: " + web.Message);
                    Console.WriteLine("URL: " + imageURL);
                    Console.WriteLine("Path: " + saveFileDialog.FileName);
                }
            }
        }

        public static void DownloadTopPosts()
        {
            WebClient client = new WebClient();
            string htmlLoc = Program.appPath + "earthporn.html";
            string earthpornHtml;
            string entry;
            string path;
            string title;
            int entryLoc;
            int imageEnd;
            int imageLoc;
            string URL;
            int picCount = 0;
            int postCount = 1;
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
            int authorLoc;
            int authorEnd;
            string author;
            int commentLoc;
            int commentEnd;
            string commentURL;
            int numScreens = Screen.AllScreens.Length;  //If there are multiple screen, it will download panoramas and use them. 
            
            //Download the most recent Earthporn top website
            try
            {
                client.DownloadFile(subreddit, htmlLoc);
            }
            catch (WebException e)
            {
                Console.WriteLine("An error occured when downloading r/earthporn\n" + e.Message);
            }


            //in a loop, download the 12 top posts right now
            while (picCount < 12 && postCount < 24)
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
                Console.WriteLine("Post " + postCount + " Title: " + title);
                
                /* Get the user who submitted the post. It should be wrapped in
                 * an <a> tag, but after the string " by ":
                 * " by "
                 * <a href="Uhttp://www.retti.com/user/USERNAME"
                 */
                authorLoc = earthpornHtml.IndexOf(" by ", titleEnd);
                authorLoc = earthpornHtml.IndexOf("http://www.reddit.com/user/", authorLoc) + 27;
                authorEnd = earthpornHtml.IndexOf("\"", authorLoc);
                author = earthpornHtml.Substring(authorLoc, authorEnd - authorLoc);

                /* Get the url to the comments page. It should be wrapped in an <a>
                 * tag nested in the "flat-list buttons" ul as part of the "first" <li>
                 * <li class="first">
                 *      <a href="COMMENTURL">
                 */
                commentLoc = earthpornHtml.IndexOf("<li class=\"first\">", titleEnd);
                commentLoc = earthpornHtml.IndexOf("href=", commentLoc) + 6;
                commentEnd = earthpornHtml.IndexOf("\"", commentLoc);
                commentURL = earthpornHtml.Substring(commentLoc, commentEnd - commentLoc);

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

                if (ratio >= 1.25 && ratio <= 1.85 * numScreens * numScreens)   //numScreens^2 because panoramas can often be 4:1 ratios or more. 
                {
                    //try to download
                    try
                    {
                            
                        path = DownloadImage(URL, postCount);
                        string[] data = { path, title, author, commentURL, aspectRatio, URL};
                        posts.Enqueue(data);
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
                                string[] data = { path, title, author, commentURL, aspectRatio, URL };
                                posts.Enqueue(data);
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
                            catch (Exception all)
                            {
                                Console.WriteLine("Other error occurred when downloading:\r\n" + all.Message);
                                success = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("The file found was not an image");
                            success = false;
                        }
                    }
                    catch (Exception all)
                    {
                        Console.WriteLine("Other error occurred when downloading:\r\n" + all.Message);
                        success = false;
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
        }

        private void linkPostURL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkPostURL.LinkVisited = true;
            System.Diagnostics.Process.Start(url);
        }

        private void chkStartUp_CheckedChanged(object sender, EventArgs e)
        {
            SetStartup();
        }

        private void chkRunning_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRunning.Checked)
            {
                timer.Enabled = true;
                chkRunning.Text = "Stop";
            }
            else
            {
                timer.Enabled = false;
                chkRunning.Text = "Continue";
            }
        }

        /// <summary>
        /// Downloads the image and stores it to a file
        /// </summary>
        /// <returns>The path of the image</returns>
        public static string DownloadImage(string url, int post)
        {
            //get the format
            string format = GetFormat(url);

            //before we downlaod, check that the file is an image so that we don't acidentally download a virus
            if (!IsImageFormat(format))
                throw new FormatException("The file type was not an image");

            //prepare to download
            string path = Program.appPath + "\\Images\\Desktop" + post + format;
            WebClient client = new WebClient();

            //download the image
            client.DownloadFile(url, path);
            return path;
        }

        public static string GetFormat(string file)
        {
            //get the format
            string format = file.Substring(file.Length - 4);
            while (!format.StartsWith("."))
            {
                if (format.Length > 0)
                    format = format.Substring(1);
                else
                    throw new FormatException("URL is not a file");
            }

            return format;
        }

        public static string DownloadImage(string url, string path)
        {
            //get the format
            string format = GetFormat(url);

            //before we downlaod, check that the file is an image so that we don't acidentally download a virus
            if (!IsImageFormat(format))
                throw new FormatException("The file type was not an image");

            //prepare to download
            WebClient client = new WebClient();

            //download the image
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
            string htmlLoc = Program.appPath + "Imgur.html";
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
            if (fileExtension.Equals(".JPG") || fileExtension.Equals(".JPEG") || fileExtension.Equals(".JIF") || fileExtension.Equals(".JFIF")
                || fileExtension.Equals(".TIF") || fileExtension.Equals(".TIFF")
                || fileExtension.Equals(".GIF") || fileExtension.Equals(".PNG")
                || fileExtension.Equals(".JP2") || fileExtension.Equals(".JPX") || fileExtension.Equals(".J2K") || fileExtension.Equals(".J2C")
                || fileExtension.Equals(".BMP"))
                return true;
            return false;
        }

        /// <summary>
        /// Sets the application to run on start or not run on startup
        /// </summary>
        private void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (chkStartUp.Checked)
                rk.SetValue("EarthPornDesktopBackground", Application.ExecutablePath.ToString());
            else
                rk.DeleteValue("EarthPornDesktopBackground",false);            

        }

        /***********************************************
         * Code pulled from http://tinyurl.com/q7vd47e *
         ***********************************************/
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(
            UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);

        private static readonly UInt32 SPI_SETDESKWALLPAPER = 0x14;
        private static readonly UInt32 SPIF_UPDATEINIFILE = 0x01;
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
