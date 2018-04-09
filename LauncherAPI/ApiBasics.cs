﻿using MimeTypes;
using System;
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
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
                System.Threading.Thread.Sleep(20);
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

        public static string DownloadFile(string path, string url, Action<long, long, string, long> dlProgressChanged = null, Action dlCompleted = null, bool overwrite = false)
        {
            if (PreviousChk(path) || overwrite)
            {
                Console.WriteLine("Downloading '{0}' from '{1}', please wait...", Path.GetFileName(path), url.CleverSubstring());
                Thread th = new Thread(() =>
                {
                    using (WebClient wc = new WebClient())
                    {
                        if (dlProgressChanged != null) wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => { Wc_DownloadProgressChanged(sender, e, dlProgressChanged, Path.GetFileName(path)); });
                        if (dlCompleted != null) wc.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => { Wc_DownloadFileCompleted(sender, e, dlCompleted); });
                        wc.DownloadFileAsync(new Uri(url), path);
                    }
                });
                th.Start();
            }
            else
                Console.WriteLine("File '{0}' already exists! Skipping...", Path.GetFileName(path));

            return path;
        }

        private static DateTime lastUpdate;
        private static long lastBytes = 0;

        private static void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, Action<long, long, string, long> dlProgressChanged, string file)
        {
            if (lastBytes == 0)
            {
                lastUpdate = DateTime.Now;
                lastBytes = e.BytesReceived;
                return;
            }

            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - lastUpdate;
            long bytesChange = e.BytesReceived - lastBytes,
                 bytesPerSecond = bytesChange / timeSpan.Seconds;

            dlProgressChanged(e.BytesReceived, e.TotalBytesToReceive, file, bytesPerSecond);

            lastBytes = e.BytesReceived;
            lastUpdate = now;
        }

        private static void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e, Action dlCompleted)
        {
            dlCompleted();
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
    }
}