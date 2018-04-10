using MimeTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LauncherAPI
{
    public static class ApiBasics
    {
        private static bool _off, chk;

        public static string Base64PATH
        {
            get
            {
                byte[] arr = Encoding.UTF8.GetBytes(Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(LocalPATH, "Base64");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, Convert.ToBase64String(arr)) + ".json";

                return path;
            }
        }

        public static string LocalPATH
        {
            get
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "z3nth10n", "Launcher");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public static string AssemblyFolderPATH
        {
            get
            {
                //Console.WriteLine("xxx: " + Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "natives"));
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        private static bool? _IsConsole;

        public static bool IsConsole
        {
            get
            {
                if (_IsConsole == null)
                {
                    _IsConsole = true;
                    try { int window_height = Console.WindowHeight; }
                    catch { _IsConsole = false; }
                }
                return _IsConsole.Value;
            }
        }

        public static bool OfflineMode
        {
            get
            {
                if (!chk)
                {
                    _off = !CheckForInternetConnection();
                    chk = true;
                }
                return _off;
            }
        }

        public static void ChkConn(object objState)
        {
            if (chk) chk = false;
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool PreviousChk(string path)
        {
            bool isf = Directory.Exists(path) && IsDirectory(path);
            string fold = isf ? path : Path.GetDirectoryName(path);
            if (!Directory.Exists(fold))
                Directory.CreateDirectory(fold);

            if (!isf && File.Exists(path))
                return false;

            return true;
        }

        public static void UrlToLocalFile(string url, string path)
        {
            if (!PreviousChk(path))
                return;

            using (WebClient wc = new WebClient())
                wc.DownloadFile(url, path);
        }

        public static string URLToLocalFile(string url)
        {
            string fil = "";

            using (WebClient wc = new WebClient())
            {
                byte[] by = wc.DownloadData(url);
                string[] arr = Directory.GetFiles(LocalPATH);

                fil = Path.Combine(LocalPATH, string.Format("file{0}{1}",
                                            arr.Length,
                                            MimeTypeMap.GetExtension(GetContentType(url))));

                if (arr.Any(x => File.ReadAllBytes(x) != by))
                    File.WriteAllBytes(fil, by);
            }

            if (string.IsNullOrEmpty(fil))
                Console.WriteLine("Coudn't retrieve file name.");

            return fil;
        }

        public static string GetContentType(string url)
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

        public static Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }

        public static void Shake(Form form)
        {
            var original = form.Location;
            var rnd = new Random();
            const int shake_amplitude = 10;
            for (int i = 0; i < 10; i++)
            {
                form.Location = new Point(original.X + rnd.Next(-shake_amplitude, shake_amplitude), original.Y + rnd.Next(-shake_amplitude, shake_amplitude));
                Thread.Sleep(20);
            }
            form.Location = original;
        }

        public static void SoundBytes(byte[] arr)
        {
            using (MemoryStream ms = new MemoryStream(arr))
            {
                SoundPlayer simpleSound = new SoundPlayer(ms);
                simpleSound.Play();
            }
        }

        public static string DownloadFile(string path, string url, Action<long, long, KeyValuePair<string, string>, long> dlProgressChanged = null, Action dlCompleted = null, bool overwrite = false)
        {
            List<KeyValuePair<string, string>> arr = new List<KeyValuePair<string, string>>();
            arr.Add(new KeyValuePair<string, string>(url, path));
            DownloadFile(arr.AsEnumerable(), dlProgressChanged, dlCompleted, overwrite);
            return path;
        }

        private static int indexDl = 0;

        public static void DownloadFile(IEnumerable<KeyValuePair<string, string>> urlPath, Action<long, long, KeyValuePair<string, string>, long> dlProgressChanged = null, Action dlCompleted = null, bool overwrite = false)
        {
            Thread th = new Thread(async () =>
            {
                using (WebClient wc = new WebClient())
                {
                    AsyncCompletedEventHandler aCEH = null;

                    DownloadProgressChangedEventHandler dlProgressChangedEventHadler = new DownloadProgressChangedEventHandler((sender, e) => { Wc_DownloadProgressChanged(sender, e, dlProgressChanged, urlPath); });
                    AsyncCompletedEventHandler asyncCompletedEventHandler = new AsyncCompletedEventHandler((sender, e) => { Wc_DownloadFileCompleted(sender, e, dlCompleted, dlProgressChangedEventHadler, aCEH); });

                    aCEH = asyncCompletedEventHandler;

                    if (dlProgressChanged != null) wc.DownloadProgressChanged += dlProgressChangedEventHadler;
                    if (dlCompleted != null) wc.DownloadFileCompleted += asyncCompletedEventHandler;

                    if (urlPath.Count() == 1)
                    {
                        string url = urlPath.ElementAt(0).Key, path = urlPath.ElementAt(0).Value;
                        Console.WriteLine("Downloading '{0}' from '{1}', please wait...", Path.GetFileName(path), url.CleverSubstring());
                        await wc.DownloadExtFile(path, url, overwrite);
                    }
                    else
                    {
                        foreach (var kvURLP in urlPath)
                        {
                            await wc.DownloadExtFile(kvURLP.Value, kvURLP.Key, overwrite);
                            ++indexDl;
                        }
                    }
                }
            });

            //Reset indexes...
            indexDl = 0;
            th.Start();
        }

        private async static Task DownloadExtFile(this WebClient wc, string path, string url, bool overwrite)
        {
            if (PreviousChk(path) || overwrite)
                await new Task(() => wc.DownloadFileAsync(new Uri(url), path));
            else
                Console.WriteLine("File '{0}' already exists! Skipping...", Path.GetFileName(path));
        }

        private static DateTime lastUpdate;
        private static long lastBytes = 0;

        private static void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, Action<long, long, KeyValuePair<string, string>, long> dlProgressChanged, IEnumerable<KeyValuePair<string, string>> file)
        {
            if (lastBytes == 0)
            {
                lastUpdate = DateTime.Now;
                lastBytes = e.BytesReceived;
            }

            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - lastUpdate;
            long bytesChange = e.BytesReceived - lastBytes,
                 bytesPerSecond = 0;

            try
            {
                bytesPerSecond = bytesChange / timeSpan.Seconds;
            }
            catch { }

            dlProgressChanged(e.BytesReceived, e.TotalBytesToReceive, file.ElementAt(indexDl), bytesPerSecond);

            lastBytes = e.BytesReceived;
            lastUpdate = now;
        }

        private static void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e, Action dlCompleted, DownloadProgressChangedEventHandler dlProgressChangedEventHadler, AsyncCompletedEventHandler asyncCompletedEventHandler)
        {
            dlCompleted();

            WebClient wc = ((WebClient)sender);

            wc.DownloadProgressChanged -= dlProgressChangedEventHadler;
            wc.DownloadFileCompleted -= asyncCompletedEventHandler;
        }

        public static void WriteLineStop(string val = "")
        {
            WriteLineStop(val, null);
        }

        public static void WriteLineStop(string val, params object[] objs)
        {
            Console.WriteLine(val, objs);
            Console.Read();
        }

        public static void WriteStop(string val = "")
        {
            WriteStop(val, null);
        }

        public static void WriteStop(string val, params object[] objs)
        {
            Console.Write(val, objs);
            Console.Read();
        }

        public static string GetUpperFolders(this string cpath, int levels = 1)
        {
            if (!IsDirectory(cpath)) cpath = Path.GetDirectoryName(cpath);

            for (int i = 0; i < levels; ++i)
                cpath = Path.GetDirectoryName(cpath);

            return cpath;
        }

        public static bool IsDirectory(string path)
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

        public static string CleverSubstring(this string str, int limit = 50)
        {
            return str.Length >= limit ? str.Substring(0, limit / 2) + "..." + str.Substring(str.Length - limit / 2 - 1) : str;
        }

        public static OS GetSO()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return OS.Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OS.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return OS.OSx;
            else
                return OS.Other;
        }

        public static bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    //Returns TRUE if the Status code == 200
                    return (response.StatusCode == HttpStatusCode.OK);
                }
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        public static string CleverBackslashes(this string path)
        {
            return GetSO() != OS.Windows ? path : path.Replace('/', '\\');
        }

        public static ulong GetTotalMemoryInBytes()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
        }

        public static List<string> DirSearch(string sDir)
        {
            List<string> files = new List<string>();

            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                        files.Add(f);
                    files.AddRange(DirSearch(d));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return files;
        }
    }
}