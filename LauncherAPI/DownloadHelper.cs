using System;
using System.IO;
using System.Net;

namespace LauncherAPI
{
    public static class DownloadHelper
    {
        public readonly static FileDownloader downloader = new FileDownloader();

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
    }
}