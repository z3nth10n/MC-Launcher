using LauncherAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using DL = LauncherAPI.DownloadHelper;

namespace z3nth10n_Launcher
{
    public partial class Form1 : Form
    {
        private bool cmpRev = false;

        private bool completedRevision
        {
            get
            {
                return cmpRev;
            }
            set
            {
                cmpRev = value;
                if (cmpRev)
                    RunMinecraft();
            }
        }

        private DateTime lastTime;

        private Image updatingLogo, launcherLogo;

        private static string minecraftJAR
        {
            get
            {
                string defPath = Path.Combine(ApiBasics.AssemblyFolderPATH, "minecraft.jar");
                try
                {
                    IEnumerable<FileInfo> obj = ApiLauncher.GetValidJars();
                    return obj == null ? defPath : obj.ElementAt(0).FullName;
                }
                catch
                {
                    return defPath;
                }
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

            Console.WriteLine("Java Path: " + ApiLauncher.GetJavaInstallationPath());

            lastTime = default(DateTime);
        }

        private void Downloader_Completed(object sender, EventArgs e)
        {
            foreach (var kv in dateTime)
                Console.WriteLine("Key: {0}: Value: {1}", kv.Key, kv.Value);

            lblProgress.Text = "Download completed!!";

            RunMinecraft();
        }

        private void RunMinecraft()
        {
            if (completedRevision)
            {
                //Execute here Minecraft... WIP (resolution and position)
                Console.WriteLine("Running minecraft!");

                using (Process p = consoleControl1.StartProcess(ApiLauncher.GenerateLaunchProccess(txtUsername.Text, minecraftJAR)))
                {
                    using (StreamReader sr = p.StandardOutput)
                    {
                        while (!sr.EndOfStream)
                        {
                            string output = sr.ReadLine();
                            if (!string.IsNullOrEmpty(output))
                                consoleControl1.WriteOutput(output, Color.White);
                        }
                    }
                }
            }
        }

        private Dictionary<string, int> dateTime = new Dictionary<string, int>();

        private void Downloader_ProgressChanged(object sender, EventArgs e)
        {
            string fileName = DL.downloader.CurrentFile.Name;

            if (!dateTime.ContainsKey(fileName))
                dateTime.Add(fileName, 0);

            if (dateTime[fileName] % (DL.downloader.CurrentFileSize > 1024 * 200 ? 40 : 5) == 0)
            {
                double pp = DL.downloader.CurrentFilePercentage(),
                       percentage = pp == double.PositiveInfinity ? 100 : pp;

                long fileSize = DL.downloader.CurrentFileSize == 0 ? DL.downloader.CurrentFileProgress : DL.downloader.CurrentFileSize;

                progressBar1.Value = Convert.ToInt32(percentage);
                lblProgress.Text = string.Format("Downloading packages => {0}\nRetrieving => {3} // {4} ({1}%) @ {2}", DL.downloader.CurrentFile.Name, percentage.ToString("0.00"), string.Format("{0}/s", FileDownloader.FormatSizeBinary(DL.downloader.DownloadSpeed)), FileDownloader.FormatSizeBinary(DL.downloader.CurrentFileProgress), FileDownloader.FormatSizeBinary(fileSize));
            }

            ++dateTime[fileName];
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SM.Username))
                txtUsername.Text = SM.Username;

            if (SM.Nicks != null && SM.Nicks.Count > 0)
            {
                AutoCompleteStringCollection collection = new AutoCompleteStringCollection();
                collection.AddRange(SM.Nicks.Cast<string>().ToArray());
                txtUsername.AutoCompleteCustomSource = collection;
                txtUsername.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtUsername.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            }

            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;

            //WIP ... el hit lo haré en 2 metodos y llamaré a otro PHP, lo digo para hacer una clase con dichos 2 metodos, y uno general para hacer una request
            bool _off = ApiBasics.GetSO() == OS.Linux; //No compruebo si es offline mode, esto es restritivo de Linux, haya o no haya internet, si no hay internet ya se decidirá que hacer
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

            Font arial = new Font("Arial", 15),
                 arialBig = new Font("Arial", 30, FontStyle.Bold);

            if (!_off)
            {
                bool isLinux = ApiBasics.GetSO() == OS.Linux;

                Font mBold = null,
                     mRegular = null;

                if (!isLinux)
                {
                    MemoryFonts.AddMemoryFont(Properties.Resources.MBold);

                    mBold = MemoryFonts.GetFont(0, 30);

                    MemoryFonts.AddMemoryFont(Properties.Resources.MRegular);

                    mRegular = MemoryFonts.GetFont(0, 15);
                }

                //Assign font
                updatingLogo = ApiBasics.DrawText("Updating Minecraft", isLinux ? arialBig : mBold, Color.FromArgb(255, 127, 127, 127), Color.Transparent);
                launcherLogo = ApiBasics.DrawText("Minecraft Launcher", isLinux ? arialBig : mBold, Color.FromArgb(255, 127, 127, 127), Color.Transparent);
                lblProgress.Font = isLinux ? arial : mRegular;

                pictureBox1.Image = updatingLogo;
                pictureBox2.Image = launcherLogo;
            }
            else
                lblProgress.Font = arial;

            lblNotifications.Text = CheckValidJar();
        }

        //Funcs

        private static string CheckValidJar()
        {
            if (!File.Exists(minecraftJAR)) return string.Format("File {0} doesn't exist!!", minecraftJAR);
            else if (!ApiBasics.AssemblyFolderPATH.Split(ApiBasics.FolderDelimiter).Contains("bin") && !ApiBasics.AssemblyFolderPATH.Split(ApiBasics.FolderDelimiter).Contains("versions"))
                return string.Format("Assembly isn't inside bin or version folder!"); //W-IP ... Tengo que comprobar la version de la carpeta "versions", ya me da igual, porq ahora este launcher se descarga el jar en la carpeta BIN (WIP) porque no tengo esto implementado al 100%

            return !ApiLauncher.IsValidJAR(minecraftJAR) ? string.Format("You have to put this executable inside of a valid Minecraft folder, next to {0} file.", Path.GetFileName(minecraftJAR)) : "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((DateTime.Now - lastTime).TotalMilliseconds > 2000 || lastTime == default(DateTime))
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
                    pictureBox2.Image = updatingLogo;
                    lblProgress.Text = "Retrieving files...";

                    SM.Username = txtUsername.Text;
                    SM.AddNick(txtUsername.Text);

                    pnlMain.SendToBack();
                    pnlMain.Visible = false;

                    //tabControl1.SendToBack();
                    //tabControl1.Visible = false;

                    string str = ApiLauncher.DownloadLibraries();

                    if (str == "nothing")
                        Downloader_Completed(null, null);

                    completedRevision = string.IsNullOrEmpty(str) || str == "nothing";
                    Console.WriteLine(completedRevision ? "Update completed succesfully! Now we have to run Minecraft..." : str);
                }

                lastTime = DateTime.Now;
            }
        }
    }
}