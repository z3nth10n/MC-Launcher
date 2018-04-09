using LauncherAPI;
using System;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace z3nth10n_Launcher
{
    public static class Program
    {
        private static Timer timer;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            //Chk conn
            timer = new Timer(ApiBasics.ChkConn, null, 0, 1000 * 5 * 60);
        }

        public static void Exit()
        {
            if (timer != null) timer.Dispose();
        }
    }
}