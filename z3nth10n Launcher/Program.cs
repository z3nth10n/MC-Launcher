using MimeTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace z3nth10n_Launcher
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public static string URLToLocalFile(string url)
        {
            string fold = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
                   fil = Path.Combine(fold, string.Format("file{0}.{1}", Directory.GetFiles(fold).Length, GetURLExtension(url)));
            if (!Directory.Exists(fold)) Directory.CreateDirectory(fold);
            using (WebClient wc = new WebClient())
                wc.DownloadFile(url, fil);
            return fil;
        }

        public static string GetURLExtension(string url)
        {
            using (WebClient client = new WebClient())
            {
                string data = client.DownloadString(url),
                       contentType = client.Headers["Content-Type"];
                return MimeTypeMap.GetExtension(contentType);
            }
        }
    }
}