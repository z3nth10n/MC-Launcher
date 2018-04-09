using LauncherAPI;
using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

//using Props = z3nth10n_Launcher.Properties.Settings;

namespace z3nth10n_Launcher
{
    public partial class Form1 : Form
    {
        private static string minecraftJAR
        {
            get
            { //minecraft.jar or any version
                return Path.Combine(ApiBasics.AssemblyFolderPATH, "minecraft.jar");
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SM.Username))
                txtUsername.Text = SM.Username;

            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;

            bool _off = !ApiBasics.OfflineMode || ApiBasics.GetSO() == OS.Linux;
            if (_off)
                try
                {
                    pictureBox1.Load(ApiLauncher.GetLogoStr());
                    pictureBox2.Load(ApiLauncher.GetLogoStr("Updating Minecraft"));
                }
                catch
                {
                    _off = false;
                }

            Font arial = new Font("Arial", 15);

            if (!_off)
            {
                //string f = Path.Combine(ApiBasics.LocalPATH, "Minecraft.otf");

                /*if (ApiBasics.PreviousChk(f))
                    File.WriteAllBytes(f, Properties.Resources.MBold);

                PrivateFontCollection pfc = new PrivateFontCollection();

                pfc.AddFontFile(f);*/ //Aun asi no funciona, solo en WIN

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

            ApiLauncher.dlProgressChanged = (bytesIn, totalBytes, fileName, bytesSec) =>
            {
                Invoke((MethodInvoker)delegate
                {
                    double percentage = (double)bytesIn / totalBytes * 100d;
                    lblProgress.Text = string.Format("Downloading packages\nRetrieving: {0} ({1}%) @ {2} KB/sec", fileName, (int)percentage, (bytesSec / 1024d).ToString("F2"));
                    progressBar1.Value = (int)percentage;
                });
            };

            ApiLauncher.dlCompleted = () =>
            {
                Invoke((MethodInvoker)delegate
                { //WIP ... necesita algunos reajustes
                    lblProgress.Text = "Download completed!!";
                    Console.WriteLine("Download completed!!");
                });
            };
        }

        //Funcs

        private static bool CheckValidJar()
        {
            if (!File.Exists(minecraftJAR)) return false;
            else if (!ApiBasics.AssemblyFolderPATH.Contains("bin") || !ApiBasics.AssemblyFolderPATH.Contains("versions")) return false; //Tengo que comprobar la version de la carpeta "versions"

            return ApiLauncher.IsValidJAR(minecraftJAR);
        }

        private DateTime lastTime = DateTime.Now;

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Exit();
        }

        private void button1_Click_1(object sender, EventArgs e)
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
                    //Controls.Remove(pnlMain);
                    string str = ApiLauncher.DownloadLibraries();
                    Console.WriteLine(string.IsNullOrEmpty(str) ? "Update completed succesfully! Now we have to run Minecraft at desired position and resolution... (WIP)" : str);
                }

                lastTime = DateTime.Now;
            }
        }
    }
}