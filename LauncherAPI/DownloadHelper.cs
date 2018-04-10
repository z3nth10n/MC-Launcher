using System;
using System.IO;
using System.Net;

namespace LauncherAPI
{
    public static class DownloadHelper
    {
        public static FileDownloader downloader = new FileDownloader();

        public static void DownloadFile(string path, string url)
        {
            downloader.Files.Add(new FileDownloader.FileInfo(path, url));
        }

        public static string DownloadSyncFile(string path, string url, bool overwrite = false)
        {
            if (ApiBasics.PreviousChk(path) || overwrite)
            {
                Console.WriteLine("Downloading '{0}' from '{1}', please wait...", Path.GetFileName(path), url.CleverSubstring());
                try
                {
                    using (WebClient wc = new WebClient())
                        wc.DownloadFile(new Uri(url), path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was a problem downloading {0}...", url);
                    Console.WriteLine(ex);
                }
            }
            else
                Console.WriteLine("File '{0}' already exists! Skipping... (Path: {1})", Path.GetFileName(path), path);

            return path;
        }

        /*public static Action<long, long, KeyValuePair<string, string>, long> dlProgressChanged { get; set; }
        public static Func<int, bool> dlCompleted { get; set; }

        public static bool isCompleted
        {
            get
            { //Esto va a traer problemas ... WIP
                return indexDl + 1 == dlArr.Count;
            }
        }

        public static int dlCount
        {
            get
            {
                return dlArr.Count;
            }
        }

        private static List<KeyValuePair<string, string>> dlArr = new List<KeyValuePair<string, string>>();
        private static DateTime lastUpdate;
        private static long lastBytes = 0;
        private static int indexDl = 1; // WIP ... esto tb traera problemas
        private static ManualResetEvent mre = new ManualResetEvent(false);

        public static void DownloadFile(string path, string url)
        {
            Console.WriteLine("Adding file '{0}'...", Path.GetFileName(path));
            dlArr.Add(new KeyValuePair<string, string>(url, path));
        }

        public static void DownloadArr(bool overwrite)
        {
            DownloadFile(dlArr.AsEnumerable(), dlProgressChanged, dlCompleted, overwrite);
        }

        public static string DownloadSyncFile(string path, string url, bool overwrite = false)
        {
            if (ApiBasics.PreviousChk(path) || overwrite)
            {
                Console.WriteLine("Downloading '{0}' from '{1}', please wait...", Path.GetFileName(path), url.CleverSubstring());
                try
                {
                    using (WebClient wc = new WebClient())
                        wc.DownloadFile(new Uri(url), path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was a problem downloading {0}...", url);
                    Console.WriteLine(ex);
                }
            }
            else
                Console.WriteLine("File '{0}' already exists! Skipping... (Path: {1})", Path.GetFileName(path), path);

            return path;
        }

        public static string DownloadFile(string path, string url, Action<long, long, KeyValuePair<string, string>, long> dlProgressChanged = null, Func<int, bool> dlCompleted = null, bool overwrite = false)
        {
            List<KeyValuePair<string, string>> arr = new List<KeyValuePair<string, string>>();
            arr.Add(new KeyValuePair<string, string>(url, path));
            DownloadFile(arr.AsEnumerable(), dlProgressChanged, dlCompleted, overwrite);
            return path;
        }

        public static void DownloadFile(IEnumerable<KeyValuePair<string, string>> urlPath, Action<long, long, KeyValuePair<string, string>, long> dlProgressChanged = null, Func<int, bool> dlCompleted = null, bool overwrite = false)
        {
            Thread th = new Thread(() =>
            {
                using (WebClient wc = new WebClient())
                {
                    AsyncCompletedEventHandler aCEH = null;

                    DownloadProgressChangedEventHandler dlProgressChangedEventHadler = new DownloadProgressChangedEventHandler((sender, e) => { Wc_DownloadProgressChanged(sender, e, dlProgressChanged, urlPath); });
                    AsyncCompletedEventHandler asyncCompletedEventHandler = new AsyncCompletedEventHandler((sender, e) => { Wc_DownloadFileCompleted(sender, e, dlCompleted, dlProgressChangedEventHadler, aCEH, indexDl); });

                    aCEH = asyncCompletedEventHandler;

                    if (dlProgressChanged != null) wc.DownloadProgressChanged += dlProgressChangedEventHadler;
                    if (dlCompleted != null) wc.DownloadFileCompleted += asyncCompletedEventHandler;

                    if (urlPath.Count() == 1)
                    {
                        string url = urlPath.ElementAt(0).Key, path = urlPath.ElementAt(0).Value;
                        Console.WriteLine("Downloading '{0}' from '{1}', please wait...", Path.GetFileName(path), url.CleverSubstring());

                        wc.DownloadExtFile(path, url, overwrite);

                        mre.WaitOne();

                        dlArr.Remove(urlPath.ElementAt(0));

                        mre.Reset();
                    }
                    else
                    {
                        foreach (var kvURLP in urlPath)
                        {
                            if (wc.DownloadExtFile(kvURLP.Value, kvURLP.Key, overwrite))
                            {
                                Console.WriteLine("aaaa");
                                mre.WaitOne();
                                Console.WriteLine("bbbb");
                            }

                            //dlArr.Remove(urlPath.ElementAt(indexDl));
                            ++indexDl;
                        }
                    }
                }
            });

            //Reset indexes...
            indexDl = 1;
            th.Start();
        }

        private static bool DownloadExtFile(this WebClient wc, string path, string url, bool overwrite)
        {
            if (ApiBasics.PreviousChk(path) || overwrite)
            {
                wc.DownloadFileAsync(new Uri(url), path);
                return true;
            }
            else
            {
                Console.WriteLine("File '{0}' already exists! Skipping... (Path: {1})", Path.GetFileName(path), path);
                return false;
            }
        }

        private static void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, Action<long, long, KeyValuePair<string, string>, long> dlProgressChanged, IEnumerable<KeyValuePair<string, string>> file)
        {
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - lastUpdate;

            long bytesChange = e.BytesReceived - lastBytes,
                 bytesPerSecond = 0;

            if (lastBytes == 0)
            {
                lastUpdate = DateTime.Now;
                lastBytes = e.BytesReceived;
            }
            else
            {
                lastBytes = e.BytesReceived;
                lastUpdate = now;
            }

            try
            {
                bytesPerSecond = bytesChange / timeSpan.Seconds;
            }
            catch
            {
                bytesPerSecond = 0;
            }

            dlProgressChanged(e.BytesReceived, e.TotalBytesToReceive, file.ElementAt(indexDl - 1), bytesPerSecond);
        }

        private static void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e, Func<int, bool> dlCompleted, DownloadProgressChangedEventHandler dlProgressChangedEventHadler, AsyncCompletedEventHandler asyncCompletedEventHandler, int index)
        {
            if (dlCompleted(index))
            {
                WebClient wc = ((WebClient)sender);

                wc.DownloadProgressChanged -= dlProgressChangedEventHadler;
                wc.DownloadFileCompleted -= asyncCompletedEventHandler;

                dlArr = new List<KeyValuePair<string, string>>();

                mre.Reset();
            }
            else
                mre.Set();
        }*/
    }
}