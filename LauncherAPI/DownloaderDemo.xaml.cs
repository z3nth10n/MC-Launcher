//**************************************************************//
// FileDownloader Demo                                          //
// By De Dauw Jeroen - jeroendedauw@gmail.com                   //
//**************************************************************//
// Copyright 2009 - BN+ Discussions                             //
// http://code.bn2vs.com                                        //
//**************************************************************//

// This code is avaible at
// > BN+ Discussions: http://code.bn2vs.com/viewtopic.php?t=153
// > The Code Project: http://www.codeproject.com/KB/cs/BackgroundFileDownloader.aspx

// Dutch support can be found here: http://www.helpmij.nl/forum/showthread.php?t=416568

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FileDownloaderApp
{
    /// <summary>Interaction logic for DownloaderDemo.xaml</summary>
    public partial class DownloaderDemo : Window
    {
        // Creating a new instance of a FileDownloader
        private FileDownloader downloader = new FileDownloader();

        public DownloaderDemo()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close,
                new ExecutedRoutedEventHandler(delegate(object sender, ExecutedRoutedEventArgs args) { this.Close(); })));

            downloader.StateChanged += new EventHandler(downloader_StateChanged);
            downloader.CalculatingFileSize += new FileDownloader.CalculatingFileSizeEventHandler(downloader_CalculationFileSize);
            downloader.ProgressChanged += new EventHandler(downloader_ProgressChanged);
            downloader.FileDownloadAttempting += new EventHandler(downloader_FileDownloadAttempting);
            downloader.FileDownloadStarted += new EventHandler(downloader_FileDownloadStarted);
            downloader.Completed += new EventHandler(downloader_Completed);
            downloader.CancelRequested += new EventHandler(downloader_CancelRequested);
            downloader.DeletingFilesAfterCancel += new EventHandler(downloader_DeletingFilesAfterCancel);
            downloader.Canceled += new EventHandler(downloader_Canceled);

        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Height -= 30;
        }

        // A simple implementation of setting the directory path, adding files from a textbox and starting the download
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Set the path to the local directory where the files will be downloaded to
                downloader.LocalDirectory = openFolderDialog.SelectedPath;

                // Clear the current list of files (in case it's not the first download)
                downloader.Files.Clear();

                // Get the contents of the rich text box
                string rtbContents = new TextRange(rtbPaths.Document.ContentStart, rtbPaths.Document.ContentEnd).Text;
                foreach (string line in rtbContents.Split('\n'))
                {
                    String trimmedLine = line.Trim(' ', '\r');
                    if (trimmedLine.Length > 0)
                    {
                        // If the line is not empty, assume it's a valid url and add it to the files list
                        // Note: You could check if the url is valid before adding it, and probably should do this is a real application
                        downloader.Files.Add(new FileDownloader.FileInfo(trimmedLine));
                    }
                }

                // Start the downloader
                downloader.Start();
            }
        }
       
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            // Pause the downloader
            downloader.Pause();
        }
        
        private void btnResume_Click(object sender, RoutedEventArgs e)
        {
            // Resume the downloader
            downloader.Resume();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            // Stop the downloader
            // Note: This will not be instantantanious - the current requests need to be closed down, and the downloaded files need to be deleted
            downloader.Stop();
        }

        // This event is fired every time the paused or busy state is changed, and used here to set the controls of the interface
        // This makes it enuivalent to a void handling both downloader.IsBusyChanged and downloader.IsPausedChanged
        private void downloader_StateChanged(object sender, EventArgs e)
        {
            // Setting the buttons
            btnStart.IsEnabled = downloader.CanStart;
            btnStop.IsEnabled = downloader.CanStop;
            btnPause.IsEnabled = downloader.CanPause;
            btnResume.IsEnabled = downloader.CanResume;

            // Enabling or disabling the setting controls
            rtbPaths.IsReadOnly = downloader.IsBusy;
            cbUseProgress.IsEnabled = !downloader.IsBusy;
        }

        // Show the progress of file size calculation
        // Note that these events will only occur when the total file size is calculated in advance, in other words when the SupportsProgress is set to true
        private void downloader_CalculationFileSize(object sender, Int32 fileNr)
        {
            lblStatus.Content = String.Format("Calculating file sizes - file {0} of {1}", fileNr, downloader.Files.Count);
        }

        // Occurs every time of block of data has been downloaded, and can be used to display the progress with
        // Note that you can also create a timer, and display the progress every certain interval
        // Also note that the progress properties return a size in bytes, which is not really user friendly to display
        //      The FileDownloader class provides static functions to format these byte amounts to a more readible format, either in binary or decimal notation 
        private void downloader_ProgressChanged(object sender, EventArgs e)
        {
            pBarFileProgress.Value = downloader.CurrentFilePercentage();
            lblFileProgress.Content = String.Format("Downloaded {0} of {1} ({2}%)", FileDownloader.FormatSizeBinary(downloader.CurrentFileProgress), FileDownloader.FormatSizeBinary(downloader.CurrentFileSize), downloader.CurrentFilePercentage()) + String.Format(" - {0}/s", FileDownloader.FormatSizeBinary(downloader.DownloadSpeed));
           
            if (downloader.SupportsProgress)
            {
                pBarTotalProgress.Value = downloader.TotalPercentage();
                lblTotalProgress.Content = String.Format("Downloaded {0} of {1} ({2}%)", FileDownloader.FormatSizeBinary(downloader.TotalProgress), FileDownloader.FormatSizeBinary(downloader.TotalSize), downloader.TotalPercentage());
            }
        }

        // This will be shown when the request for the file is made, before the download starts (or fails)
        private void downloader_FileDownloadAttempting(object sender, EventArgs e)
        {
            lblStatus.Content = String.Format("Preparing {0}", downloader.CurrentFile.Path);
        }

        // Display of the file info after the download started
        private void downloader_FileDownloadStarted(object sender, EventArgs e)
        {
            lblStatus.Content = String.Format("Downloading {0}", downloader.CurrentFile.Path);
            lblFileSize.Content = String.Format("File size: {0}", FileDownloader.FormatSizeBinary(downloader.CurrentFileSize));
            lblSavingTo.Content = String.Format("Saving to {0}\\{1}", downloader.LocalDirectory, downloader.CurrentFile.Name);
        }

        // Display of a completion message, showing the amount of files that has been downloaded.
        // Note, this does not hold into account any possible failed file downloads
        private void downloader_Completed(object sender, EventArgs e)
        {
            lblStatus.Content = String.Format("Download complete, downloaded {0} files.", downloader.Files.Count);
        }

        // Show a message that the downloads are being canceled - all files downloaded will be deleted and the current ones will be aborted
        private void downloader_CancelRequested(object sender, EventArgs e)
        {
            lblStatus.Content = "Canceling downloads...";
        }

        // Show a message that the downloads are being canceled - all files downloaded will be deleted and the current ones will be aborted
        private void downloader_DeletingFilesAfterCancel(object sender, EventArgs e)
        {
            lblStatus.Content = "Canceling downloads - deleting files...";
        }

        // Show a message saying the downloads have been canceled
        private void downloader_Canceled(object sender, EventArgs e)
        {
            lblStatus.Content = "Download(s) canceled";
            pBarFileProgress.Value = 0;
            pBarTotalProgress.Value = 0;
            lblFileProgress.Content = "-";
            lblTotalProgress.Content = "-";
            lblFileSize.Content = "-";
            lblSavingTo.Content = "-";
        }

        // Setting the SupportsProgress property - if set to false, no total progress data will be avaible!
        private void cbUseProgress_Checked(object sender, RoutedEventArgs e)
        {
            downloader.SupportsProgress = (Boolean)cbUseProgress.IsChecked; 
        }

        // Setting the DeleteCompletedFilesAfterCancel property - indicates if the completed files should be deleted after cancellation
        private void cbDeleteCompletedFiles_Checked(object sender, RoutedEventArgs e)
        {
            downloader.DeleteCompletedFilesAfterCancel = (Boolean)cbDeleteCompletedFiles.IsChecked;
        }

        // Close the window when the close button is hit
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



    }
}


