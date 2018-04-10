using LauncherAPI;
using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using DL = LauncherAPI.DownloadHelper;

namespace z3nth10n_Launcher
{
    public partial class Form1 : Form
    {
        private bool completedRevision = false;

        private static string minecraftJAR
        {
            get
            { //minecraft.jar or any version ... WIP
                return Path.Combine(ApiBasics.AssemblyFolderPATH, "minecraft.jar");
            }
        }

        public Form1()
        {
            InitializeComponent();

            /*DL.downloader.StateChanged += Downloader_StateChanged;
            DL.downloader.CalculatingFileSize += Downloader_CalculatingFileSize;
            DL.downloader.FileDownloadAttempting += Downloader_FileDownloadAttempting;
            DL.downloader.FileDownloadStarted += Downloader_FileDownloadStarted;
            DL.downloader.CancelRequested += Downloader_CancelRequested;
            DL.downloader.DeletingFilesAfterCancel += Downloader_DeletingFilesAfterCancel;
            DL.downloader.Canceled += Downloader_Canceled;*/

            DL.downloader.ProgressChanged += Downloader_ProgressChanged;
            DL.downloader.Completed += Downloader_Completed;
        }

        private void Downloader_Completed(object sender, EventArgs e)
        {
            lblProgress.Text = "Download completed!!";
            if (completedRevision)
            {
                //Execute here Minecraft... WIP
            }
        }

        private void Downloader_ProgressChanged(object sender, EventArgs e)
        {
            try
            {
                progressBar1.Value = Convert.ToInt32(DL.downloader.CurrentFilePercentage());
                lblProgress.Text = string.Format("Downloading packages\nRetrieving: {0} ({1}%) @ {2}", DL.downloader.CurrentFile.Name, FileDownloader.FormatSizeBinary(DL.downloader.CurrentFileProgress), string.Format("{0}/s", FileDownloader.FormatSizeBinary(DL.downloader.DownloadSpeed)));
            }
            catch
            { }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SM.Username))
                txtUsername.Text = SM.Username;

            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;

            //WIP ... el hit lo haré en 2 metodos y llamaré a otro PHP, lo digo para hacer una clase con dichos 2 metodos, y uno general para hacer una request
            bool _off = !ApiBasics.OfflineMode || ApiBasics.GetSO() == OS.Linux; //WIP ... No deberia abusar de recursos en Windows haciendo que el propio usuario pueda generar la imagen
            if (_off)
                try
                {
                    pictureBox1.Load(ApiLauncher.GetLogoStr());
                    pictureBox2.Load(ApiLauncher.GetLogoStr("Updating Minecraft"));
                }
                catch
                {
                    Console.WriteLine("Coudn't generate logos!!");
                    _off = false;
                }

            Font arial = new Font("Arial", 15);

            if (!_off)
            {
                MemoryFonts.AddMemoryFont(Properties.Resources.MBold);

                Font mBold = MemoryFonts.GetFont(0, 30),
                     mRegular = null;

                MemoryFonts.AddMemoryFont(Properties.Resources.MRegular);

                mRegular = MemoryFonts.GetFont(0, 15);

                //Assign font
                pictureBox1.Image = ApiBasics.DrawText("Minecraft Launcher", mBold, Color.FromArgb(255, 127, 127, 127), Color.Transparent);
                pictureBox2.Image = ApiBasics.DrawText("Updating Minecraft", mBold, Color.FromArgb(255, 127, 127, 127), Color.Transparent);
                lblProgress.Font = ApiBasics.GetSO() == OS.Linux ? arial : mRegular;
            }
            else
                lblProgress.Font = arial;

            lblNotifications.Text = CheckValidJar() ? "" : "You have to put this executable inside of a valid Minecraft folder, next to minecraft.jar file."; //WIP ... esto siempre aparece
        }

        //Funcs

        private static bool CheckValidJar()
        {
            if (!File.Exists(minecraftJAR)) return false;
            else if (!ApiBasics.AssemblyFolderPATH.Contains("bin") || !ApiBasics.AssemblyFolderPATH.Contains("versions")) return false; //Tengo que comprobar la version de la carpeta "versions"

            return ApiLauncher.IsValidJAR(minecraftJAR);
        }

        private DateTime lastTime = DateTime.Now;

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((DateTime.Now - lastTime).TotalMilliseconds > 2000)
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    lblNotifications.Text = "You have to specify an username!!";
                    ApiBasics.Shake(this);

                    using (Stream s = Properties.Resources.sound101)
                        (new SoundPlayer(s)).Play();
                }
                else
                {
                    SM.Username = txtUsername.Text;
                    pnlMain.SendToBack();
                    pnlMain.Visible = false;

                    string str = ApiLauncher.DownloadLibraries();
                    completedRevision = string.IsNullOrEmpty(str);
                    Console.WriteLine(completedRevision ? "Update completed succesfully! Now we have to run Minecraft at desired position and resolution... (WIP)" : str);
                }

                lastTime = DateTime.Now;
            }
        }
    }
}