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
using SharpShell;

namespace EarthPornWallpaper
{
    class Program
    {
        public static string appPath = "C:\\Users\\Sean\\git\\EarthPornWallpaper\\EarthPornWallpaper\\EarthPornWallpaper";
        static void Main(string[] args)
        {
            //get appPath
           // appPath = AppDomain.CurrentDomain.BaseDirectory;
            //Console.Write(appPath);

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Settings());

            Settings ui = new Settings();
            ApplicationContext applicationContext = new ApplicationContext();
            applicationContext.MainForm = ui;
            Application.Run(applicationContext);

        }
    }

}
