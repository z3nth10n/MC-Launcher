using MimeTypes;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Security.Cryptography;

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

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static string URLToLocalFile(string url)
        {
            string fold = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "z3nth10n", "Launcher"),
                   fil = "";

            if (!Directory.Exists(fold))
                Directory.CreateDirectory(fold);

            using (WebClient wc = new WebClient())
            {
                byte[] by = wc.DownloadData(url),
                       mychecksum = null;
                string[] arr = Directory.GetFiles(fold);

                fil = Path.Combine(fold, string.Format("file{0}{1}",
                                            arr.Length,
                                            MimeTypeMap.GetExtension(GetContentType(url))));
                
                File.WriteAllBytes(fil, by);

                //Remove repeated files
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(fil))
                        mychecksum = md5.ComputeHash(stream);

                    foreach(string ff in arr)
                    using (var stream = File.OpenRead(ff))
                        if (md5.ComputeHash(stream) == mychecksum) 
                        {
                            File.Delete(fil);
                            fil = ff;
                            break;
                        }
                }
            }

            if (string.IsNullOrEmpty(fil))
                throw new Exception("Coudn't retrieve file name.");

            return fil;
        }

        private static string GetContentType(string url)
        {
            string contentType = "";

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request != null)
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if (response != null)
                    contentType = response.ContentType;
            }

            return contentType;
        }
    }
}