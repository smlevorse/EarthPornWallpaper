using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using SharpShell.SharpContextMenu;
using SharpShell;
using System.Runtime.InteropServices;
using SharpShell.Attributes;


namespace EarthPornWallpaper
{
    [ComVisible(true)]
    class RightClickMenu : SharpContextMenu
    {

        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            //create menu
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem download = new ToolStripMenuItem("Download Currect Background");
            ToolStripMenuItem next = new ToolStripMenuItem("Next Background");

            //hook up methods to events
            download.Click += (sender, args) => DownloadBackground();
            next.Click += (sender, args) => Next();

            //attatch menu events to menu
            menu.Items.Add(download);
            menu.Items.Add(next);

            return menu;
        }

        /// <summary>
        /// Downloads the current background
        /// </summary>
        public void DownloadBackground()
        {
 
        }

        /// <summary>
        /// Skips to next background
        /// </summary>
        public void Next()
        { 
            
        }
    }
}
