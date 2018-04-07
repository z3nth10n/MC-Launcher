using ICSharpCode.SharpZipLib.Zip;
using MimeTypes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace LauncherAPI
{
    public enum OS { Windows, Linux, OSx, Other }

    public static class API
    {
        private static bool _off, chk;

        public static void ForEach<T>(
this IEnumerable<T> source,
Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static void ForEachStop<T>(
this IEnumerable<T> source,
Func<T, bool> action)
        {
            foreach (T element in source)
                if (action(element))
                    break;
        }

        public static bool Between(this int val, int min, int max, bool exclusive = false)
        {
            if (!exclusive)
                return val >= min && val <= max;
            else
                return val > min && val < max;
        }

        public static bool Between(this int val, long min, long max, bool exclusive = false)
        {
            if (!exclusive)
                return val >= min && val <= max;
            else
                return val > min && val < max;
        }

        public static bool Between(this long val, long min, long max, bool exclusive = false)
        {
            if (!exclusive)
                return val >= min && val <= max;
            else
                return val > min && val < max;
        }

        public static OS GetSO()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return OS.Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OS.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OS.OSx;
            else
                return OS.Other;
        }

        public static object ReadJAR(string path, Func<ZipFile, ZipEntry, bool, object> jarAction, Func<ZipEntry, bool> func = null)
        {
            object v = null;
            using (var zip = new ZipInputStream(File.OpenRead(path)))
            {
                using (ZipFile zipfile = new ZipFile(path))
                {
                    ZipEntry item;
                    while ((item = zip.GetNextEntry()) != null)
                    {
                        if (func == null)
                            func = (i) => !i.IsDirectory && i.Name == "net/minecraft/client/main/Main.class";

                        v = jarAction(zipfile, item, func(item));

                        if (v == null)
                            continue;

                        switch (v.GetType().Name.ToLower())
                        {
                            case "boolean":
                                if ((bool)v)
                                    return true;
                                break;

                            case "string":
                                if (!string.IsNullOrEmpty((string)v))
                                    return v;
                                break;

                            default:
                                Console.WriteLine("Unrecognized type: {0}", v.GetType().Name.ToLower());
                                break;
                        }
                    }
                }
            }

            return v;
        }

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

        public static string AssemblyPATH
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
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
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool PreviousChk(string path)
        {
            bool isf = Directory.Exists(path) && File.GetAttributes(path).HasFlag(FileAttributes.Directory);
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
                throw new Exception("Coudn't retrieve file name.");

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

        public static void DownloadFile(string path, string url, bool overwrite = false)
        {
            if (PreviousChk(path) || overwrite)
            {
                Console.WriteLine("Downloading '{0}' from '{1}', please wait...", Path.GetFileName(path), url.Substring(0, 100));
                using (WebClient wc = new WebClient())
                    File.WriteAllBytes(path, wc.DownloadData(url));
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("File '{0}' already exists! Skipping...", Path.GetFileName(path));
                Console.WriteLine();
            }
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
    }
}