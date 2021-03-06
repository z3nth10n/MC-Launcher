﻿//**************************************************************//
// Class FileDownloader v1.0.2 - April 2009                     //
// By De Dauw Jeroen - jeroendedauw@gmail.com                   //
//**************************************************************//
// Copyright 2009 - BN+ Discussions                             //
// http://code.bn2vs.com                                        //
//**************************************************************//

// This code is avaible at
// > BN+ Discussions: http://code.bn2vs.com/viewtopic.php?t=153
// > The Code Project: http://www.codeproject.com/KB/cs/BackgroundFileDownloader.aspx

// VB.Net implementation avaible at
// > BN+ Discussions: http://code.bn2vs.com/viewtopic.php?t=150
// > The Code Project: http://www.codeproject.com/KB/vb/FileDownloader.aspx

// Dutch support can be found here: http://www.helpmij.nl/forum/showthread.php?t=416568

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace LauncherAPI
{
    #region FileDownloader

    /// <summary>Class for downloading files in the background that supports info about their progress, the total progress, cancellation, pausing, and resuming. The downloads will run on a separate thread so you don't have to worry about multihreading yourself. </summary>
    /// <remarks>Class FileDownloader v1.0.2, by De Dauw Jeroen - April 2009</remarks>
    public class FileDownloader : IDisposable
    {
        #region Nested Types

        /// <summary>Simple struct for managing file info</summary>
        public struct FileInfo
        {
            /// <summary>The complete path of the file (directory + filename)</summary>
            public string FilePath { get; set; }

            /// <summary>The name of the file</summary>
            public string Name
            {
                get
                {
                    return Path.GetFileName(FilePath);
                }
            }

            public string Url { get; set; }

            /// <summary>Create a new instance of FileInfo</summary>
            /// <param name="path">The complete path of the file (directory + filename)</param>
            public FileInfo(string path, string url)
            {
                FilePath = path;
                Url = url;
            }
        }

        /// <summary>Holder for events that are triggered in the background worker but need to be fired in the main thread</summary>
        private enum Event
        {
            CalculationFileSizesStarted,

            FileSizesCalculationComplete,
            DeletingFilesAfterCancel,

            FileDownloadAttempting,
            FileDownloadStarted,
            FileDownloadStopped,
            FileDownloadSucceeded,

            ProgressChanged
        }

        /// <summary>Holder for the action that needs to be invoked</summary>
        private enum InvokeType
        {
            EventRaiser,
            FileDownloadFailedRaiser,
            CalculatingFileNrRaiser
        }

        #endregion Nested Types

        #region Events

        /// <summary>Occurs when the file downloading has started</summary>
        public event EventHandler Started;

        /// <summary>Occurs when the file downloading has been paused</summary>
        public event EventHandler Paused;

        /// <summary>Occurs when the file downloading has been resumed</summary>
        public event EventHandler Resumed;

        /// <summary>Occurs when the user has requested to cancel the downloads</summary>
        public event EventHandler CancelRequested;

        /// <summary>Occurs when the user has requested to cancel the downloads and the cleanup of the downloaded files has started</summary>
        public event EventHandler DeletingFilesAfterCancel;

        /// <summary>Occurs when the file downloading has been canceled by the user</summary>
        public event EventHandler Canceled;

        /// <summary>Occurs when the file downloading has been completed (without canceling it)</summary>
        public event EventHandler Completed;

        /// <summary>Occurs when the file downloading has been stopped by either cancellation or completion</summary>
        public event EventHandler Stopped;

        /// <summary>Occurs when the busy state of the FileDownloader has changed</summary>
        public event EventHandler IsBusyChanged;

        /// <summary>Occurs when the pause state of the FileDownloader has changed</summary>
        public event EventHandler IsPausedChanged;

        /// <summary>Occurs when the either the busy or pause state of the FileDownloader have changed</summary>
        public event EventHandler StateChanged;

        /// <summary>Occurs when the calculation of the file sizes has started</summary>
        public event EventHandler CalculationFileSizesStarted;

        /// <summary>Occurs when the calculation of the file sizes has started</summary>
        public event CalculatingFileSizeEventHandler CalculatingFileSize;

        /// <summary>Occurs when the calculation of the file sizes has been completed</summary>
        public event EventHandler FileSizesCalculationComplete;

        /// <summary>Occurs when the FileDownloader attempts to get a web response to download the file</summary>
        public event EventHandler FileDownloadAttempting;

        /// <summary>Occurs when a file download has started</summary>
        public event EventHandler FileDownloadStarted;

        /// <summary>Occurs when a file download has stopped</summary>
        public event EventHandler FileDownloadStopped;

        /// <summary>Occurs when a file download has been completed successfully</summary>
        public event EventHandler FileDownloadSucceeded;

        /// <summary>Occurs when a file download has been completed unsuccessfully</summary>
        public event FailEventHandler FileDownloadFailed;

        /// <summary>Occurs every time a block of data has been downloaded</summary>
        public event EventHandler ProgressChanged;

        #endregion Events

        #region Fields

        // Default amount of decimals
        private const int default_decimals = 2;

        // Delegates
        public delegate void FailEventHandler(object sender, Exception ex);

        public delegate void CalculatingFileSizeEventHandler(object sender, int fileNr);

        // The download worker
        private readonly BackgroundWorker bgwDownloader = new BackgroundWorker();

        // Preferences
        private bool m_supportsProgress;

        private int m_packageSize, m_stopWatchCycles;

        // State
        private readonly bool m_disposed = false;

        private bool m_busy, m_paused, m_canceled;
        private long m_currentFileProgress, m_totalProgress, m_currentFileSize;
        private int m_currentSpeed, m_fileNr;

        // Data

        private List<FileInfo> m_files = new List<FileInfo>();
        private long m_totalSize;

        #endregion Fields

        #region Constructors

        /// <summary>Create a new instance of a FileDownloader</summary>
        public FileDownloader()
        {
            initizalize(false);
        }

        /// <summary>Create a new instance of a FileDownloader</summary>
        /// <param name="supportsProgress">Optional. bool. Should the FileDownloader support total progress statistics?</param>
        public FileDownloader(bool supportsProgress)
        {
            initizalize(supportsProgress);
        }

        private void initizalize(bool supportsProgress)
        {
            // Set the bgw properties
            bgwDownloader.WorkerReportsProgress = true;
            bgwDownloader.WorkerSupportsCancellation = true;
            bgwDownloader.DoWork += new DoWorkEventHandler(bgwDownloader_DoWork);
            bgwDownloader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwDownloader_RunWorkerCompleted);
            bgwDownloader.ProgressChanged += new ProgressChangedEventHandler(bgwDownloader_ProgressChanged);

            // Set the default class preferences
            SupportsProgress = supportsProgress;
            PackageSize = 4096;
            StopWatchCyclesAmount = 5;
            DeleteCompletedFilesAfterCancel = true;
        }

        #endregion Constructors

        #region Public methods

        public void Start()
        {
            IsBusy = true;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void Stop()
        {
            IsBusy = false;
        }

        public void Stop(bool deleteCompletedFiles)
        {
            DeleteCompletedFilesAfterCancel = deleteCompletedFiles;
            Stop();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region Size formatting functions

        /// <summary>Format an amount of bytes to a more readible notation with binary notation symbols</summary>
        /// <param name="size">Required. long. The raw amount of bytes</param>
        public static string FormatSizeBinary(long size)
        {
            return FormatSizeBinary(size, default_decimals);
        }

        /// <summary>Format an amount of bytes to a more readible notation with binary notation symbols</summary>
        /// <param name="size">Required. long. The raw amount of bytes</param>
        /// <param name="decimals">Optional. int. The amount of decimals you want to have displayed in the notation</param>
        public static string FormatSizeBinary(long size, int decimals)
        {
            // By De Dauw Jeroen - April 2009 - jeroen_dedauw@yahoo.com
            string[] sizes = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };
            double formattedSize = size;
            int sizeIndex = 0;
            while (formattedSize >= 1024 && sizeIndex < sizes.Length)
            {
                formattedSize /= 1024;
                sizeIndex += 1;
            }
            return formattedSize.ToString("0." + new string('0', decimals)) + sizes[sizeIndex];
        }

        /// <summary>Format an amount of bytes to a more readible notation with decimal notation symbols</summary>
        /// <param name="size">Required. long. The raw amount of bytes</param>
        public static string FormatSizeDecimal(long size)
        {
            return FormatSizeDecimal(size, default_decimals);
        }

        /// <summary>Format an amount of bytes to a more readible notation with decimal notation symbols</summary>
        /// <param name="size">Required. long. The raw amount of bytes</param>
        /// <param name="decimals">Optional. int. The amount of decimals you want to have displayed in the notation</param>
        public static string FormatSizeDecimal(long size, int decimals)
        {
            // By De Dauw Jeroen - April 2009 - jeroen_dedauw@yahoo.com
            string[] sizes = { "B", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double formattedSize = size;
            int sizeIndex = 0;
            while (formattedSize >= 1000 && sizeIndex < sizes.Length)
            {
                formattedSize /= 1000;
                sizeIndex += 1;
            }
            return Math.Round(formattedSize, decimals) + sizes[sizeIndex];
        }

        #endregion Size formatting functions

        #endregion Public methods

        #region Protected methods

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects)
                    bgwDownloader.Dispose();
                }
                // Free your own state (unmanaged objects)
                // Set large fields to null
                Files = null;
            }
        }

        #endregion Protected methods

        #region Private methods

        private void bgwDownloader_DoWork(object sender, DoWorkEventArgs e)
        {
            int fileNr = 0;

            if (SupportsProgress) { calculateFilesSize(); }

            while (fileNr < Files.Count && !bgwDownloader.CancellationPending)
            {
                m_fileNr = fileNr;

                FileInfo file = Files[fileNr];
                string dir = Path.GetDirectoryName(file.FilePath);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                downloadFile(fileNr);

                if (bgwDownloader.CancellationPending)
                {
                    fireEventFromBgw(Event.DeletingFilesAfterCancel);
                    cleanUpFiles(DeleteCompletedFilesAfterCancel ? 0 : m_fileNr, DeleteCompletedFilesAfterCancel ? m_fileNr + 1 : 1);
                }
                else
                {
                    fileNr += 1;
                }
            }
        }

        private void calculateFilesSize()
        {
            fireEventFromBgw(Event.CalculationFileSizesStarted);
            m_totalSize = 0;

            for (int fileNr = 0; fileNr < Files.Count; fileNr++)
            {
                bgwDownloader.ReportProgress((int)InvokeType.CalculatingFileNrRaiser, fileNr + 1);
                try
                {
                    HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(Files[fileNr].FilePath);
                    HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
                    m_totalSize += webResp.ContentLength;
                    webResp.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was a problem calculating file size!!");
                    Console.WriteLine(ex);
                }
            }
            fireEventFromBgw(Event.FileSizesCalculationComplete);
        }

        private void fireEventFromBgw(Event eventName)
        {
            bgwDownloader.ReportProgress((int)InvokeType.EventRaiser, eventName);
        }

        private void downloadFile(int fileNr)
        {
            m_currentFileSize = 0;
            fireEventFromBgw(Event.FileDownloadAttempting);

            FileInfo file = Files[fileNr];
            long size = 0;

            Byte[] readBytes = new Byte[PackageSize];
            int currentPackageSize;
            System.Diagnostics.Stopwatch speedTimer = new System.Diagnostics.Stopwatch();
            int readings = 0;
            Exception exc = null;

            FileStream writer = new FileStream(file.FilePath, FileMode.Create);

            HttpWebRequest webReq;
            HttpWebResponse webResp = null;

            try
            {
                webReq = (HttpWebRequest)WebRequest.Create(Files[fileNr].Url);
                webResp = (HttpWebResponse)webReq.GetResponse();

                size = webResp.ContentLength;
            }
            catch (Exception ex)
            { exc = ex; }

            m_currentFileSize = size;
            fireEventFromBgw(Event.FileDownloadStarted);

            if (exc != null)
            {
                bgwDownloader.ReportProgress((int)InvokeType.FileDownloadFailedRaiser, exc);
            }
            else
            {
                m_currentFileProgress = 0;
                while (m_currentFileProgress < size && !bgwDownloader.CancellationPending)
                {
                    while (IsPaused) { System.Threading.Thread.Sleep(100); }

                    speedTimer.Start();

                    currentPackageSize = webResp != null ? webResp.GetResponseStream().Read(readBytes, 0, PackageSize) : 0;

                    m_currentFileProgress += currentPackageSize;
                    m_totalProgress += currentPackageSize;
                    fireEventFromBgw(Event.ProgressChanged);

                    writer.Write(readBytes, 0, currentPackageSize);
                    readings += 1;

                    if (readings >= StopWatchCyclesAmount)
                    {
                        m_currentSpeed = (int)(PackageSize * StopWatchCyclesAmount * 1000 / (speedTimer.ElapsedMilliseconds + 1));
                        speedTimer.Reset();
                        readings = 0;
                    }
                }

                speedTimer.Stop();
                writer.Close();
                if (webResp != null) webResp.Close();
                if (!bgwDownloader.CancellationPending) { fireEventFromBgw(Event.FileDownloadSucceeded); }
            }
            fireEventFromBgw(Event.FileDownloadStopped);
        }

        private void cleanUpFiles(int start, int length)
        {
            int last = length < 0 ? Files.Count - 1 : start + length - 1;

            for (int fileNr = start; fileNr <= last; fileNr++)
                if (File.Exists(Files[fileNr].FilePath)) { File.Delete(Files[fileNr].FilePath); }
        }

        private void bgwDownloader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_paused = false;
            m_busy = false;

            if (HasBeenCanceled)
            {
                Canceled?.Invoke(this, new EventArgs());
            }
            else
            {
                Completed?.Invoke(this, new EventArgs());
            }

            Stopped?.Invoke(this, new EventArgs());
            IsBusyChanged?.Invoke(this, new EventArgs());
            StateChanged?.Invoke(this, new EventArgs());
        }

        private void bgwDownloader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch ((InvokeType)e.ProgressPercentage)
            {
                case InvokeType.EventRaiser:
                    switch ((Event)e.UserState)
                    {
                        case Event.CalculationFileSizesStarted:
                            CalculationFileSizesStarted?.Invoke(this, new EventArgs());
                            break;

                        case Event.FileSizesCalculationComplete:
                            FileSizesCalculationComplete?.Invoke(this, new EventArgs());
                            break;

                        case Event.DeletingFilesAfterCancel:
                            DeletingFilesAfterCancel?.Invoke(this, new EventArgs());
                            break;

                        case Event.FileDownloadAttempting:
                            FileDownloadAttempting?.Invoke(this, new EventArgs());
                            break;

                        case Event.FileDownloadStarted:
                            FileDownloadStarted?.Invoke(this, new EventArgs());
                            break;

                        case Event.FileDownloadStopped:
                            FileDownloadStopped?.Invoke(this, new EventArgs());
                            break;

                        case Event.FileDownloadSucceeded:
                            FileDownloadSucceeded?.Invoke(this, new EventArgs());
                            break;

                        case Event.ProgressChanged:
                            ProgressChanged?.Invoke(this, new EventArgs());
                            break;
                    }
                    break;

                case InvokeType.FileDownloadFailedRaiser:
                    FileDownloadFailed?.Invoke(this, (Exception)e.UserState);
                    break;

                case InvokeType.CalculatingFileNrRaiser:
                    CalculatingFileSize?.Invoke(this, (int)e.UserState);
                    break;
            }
        }

        #endregion Private methods

        #region Properties

        /// <summary>Gets or sets the list of files to download</summary>
        public List<FileInfo> Files
        {
            get { return m_files; }
            set
            {
                if (IsBusy)
                {
                    throw new InvalidOperationException("You can not change the file list during the download");
                }
                else
                {
                    if (Files != null) m_files = value;
                }
            }
        }

        /// <summary>Gets or sets if the FileDownloader should support total progress statistics. Note that when enabled, the FileDownloader will have to get the size of each file before starting to download them, which can delay the operation.</summary>
        public bool SupportsProgress
        {
            get { return m_supportsProgress; }
            set
            {
                if (IsBusy)
                {
                    throw new InvalidOperationException("You can not change the SupportsProgress property during the download");
                }
                else
                {
                    m_supportsProgress = value;
                }
            }
        }

        /// <summary>Gets or sets if when the download process is cancelled the complete downloads should be deleted</summary>
        public bool DeleteCompletedFilesAfterCancel { get; set; }

        /// <summary>Gets or sets the size of the blocks that will be downloaded</summary>
        public int PackageSize
        {
            get { return m_packageSize; }
            set
            {
                if (value > 0)
                {
                    m_packageSize = value;
                }
                else
                {
                    throw new InvalidOperationException("The PackageSize needs to be greather then 0");
                }
            }
        }

        /// <summary>Gets or sets the amount of blocks that need to be downloaded before the progress speed is re-calculated. Note: setting this to a low value might decrease the accuracy</summary>
        public int StopWatchCyclesAmount
        {
            get { return m_stopWatchCycles; }
            set
            {
                if (value > 0)
                {
                    m_stopWatchCycles = value;
                }
                else
                {
                    throw new InvalidOperationException("The StopWatchCyclesAmount needs to be greather then 0");
                }
            }
        }

        /// <summary>Gets or sets the busy state of the FileDownloader</summary>
        public bool IsBusy
        {
            get { return m_busy; }
            set
            {
                if (IsBusy != value)
                {
                    m_busy = value;
                    m_canceled = !value;
                    if (IsBusy)
                    {
                        m_totalProgress = 0;
                        bgwDownloader.RunWorkerAsync();

                        Started?.Invoke(this, new EventArgs());
                        IsBusyChanged?.Invoke(this, new EventArgs());
                        StateChanged?.Invoke(this, new EventArgs());
                    }
                    else
                    {
                        m_paused = false;
                        bgwDownloader.CancelAsync();
                        CancelRequested?.Invoke(this, new EventArgs());
                        StateChanged?.Invoke(this, new EventArgs());
                    }
                }
            }
        }

        /// <summary>Gets or sets the pause state of the FileDownloader</summary>
        public bool IsPaused
        {
            get { return m_paused; }
            set
            {
                if (IsBusy)
                {
                    if (IsPaused != value)
                    {
                        m_paused = value;
                        if (IsPaused)
                        {
                            Paused?.Invoke(this, new EventArgs());
                        }
                        else
                        {
                            Resumed?.Invoke(this, new EventArgs());
                        }
                        IsPausedChanged?.Invoke(this, new EventArgs());
                        StateChanged?.Invoke(this, new EventArgs());
                    }
                }
                else
                {
                    throw new InvalidOperationException("You can not change the IsPaused property when the FileDownloader is not busy");
                }
            }
        }

        /// <summary>Gets if the FileDownloader can start</summary>
        public bool CanStart
        {
            get { return !IsBusy; }
        }

        /// <summary>Gets if the FileDownloader can pause</summary>
        public bool CanPause
        {
            get { return IsBusy && !IsPaused && !bgwDownloader.CancellationPending; }
        }

        /// <summary>Gets if the FileDownloader can resume</summary>
        public bool CanResume
        {
            get { return IsBusy && IsPaused && !bgwDownloader.CancellationPending; }
        }

        /// <summary>Gets if the FileDownloader can stop</summary>
        public bool CanStop
        {
            get { return IsBusy && !bgwDownloader.CancellationPending; }
        }

        /// <summary>Gets the total size of all files together. Only avaible when the FileDownloader suports progress</summary>
        public long TotalSize
        {
            get
            {
                if (SupportsProgress)
                {
                    return m_totalSize;
                }
                else
                {
                    throw new InvalidOperationException("This FileDownloader that it doesn't support progress. Modify SupportsProgress to state that it does support progress to get the total size.");
                }
            }
        }

        /// <summary>Gets the total amount of bytes downloaded</summary>
        public long TotalProgress
        {
            get { return m_totalProgress; }
        }

        /// <summary>Gets the amount of bytes downloaded of the current file</summary>
        public long CurrentFileProgress
        {
            get { return m_currentFileProgress; }
        }

        /// <summary>Gets the total download percentage. Only avaible when the FileDownloader suports progress</summary>
        public double TotalPercentage()
        {
            return TotalPercentage(default_decimals);
        }

        /// <summary>Gets the total download percentage. Only avaible when the FileDownloader suports progress</summary>
        public double TotalPercentage(int decimals)
        {
            if (SupportsProgress)
            {
                return Math.Round((double)TotalProgress / TotalSize * 100, decimals);
            }
            else
            {
                throw new InvalidOperationException("This FileDownloader that it doesn't support progress. Modify SupportsProgress to state that it does support progress.");
            }
        }

        /// <summary>Gets the percentage of the current file progress</summary>
        public double CurrentFilePercentage()
        {
            return CurrentFilePercentage(default_decimals);
        }

        /// <summary>Gets the percentage of the current file progress</summary>
        public double CurrentFilePercentage(int decimals)
        {
            return Math.Round((double)CurrentFileProgress / CurrentFileSize * 100, decimals);
        }

        /// <summary>Gets the current download speed in bytes</summary>
        public int DownloadSpeed
        {
            get { return m_currentSpeed; }
        }

        /// <summary>Gets the FileInfo object representing the current file</summary>
        public FileInfo CurrentFile
        {
            get { return Files[m_fileNr]; }
        }

        /// <summary>Gets the size of the current file in bytes</summary>
        public long CurrentFileSize
        {
            get { return m_currentFileSize; }
        }

        /// <summary>Gets if the last download was canceled by the user</summary>
        public bool HasBeenCanceled
        {
            get { return m_canceled; }
        }

        #endregion Properties
    }

    #endregion FileDownloader
}