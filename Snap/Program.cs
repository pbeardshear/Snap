using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Snap
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        // private static Form form = new SnapForm();

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SnapForm());
        }
    }
}
