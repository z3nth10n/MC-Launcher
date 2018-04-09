using LauncherAPI;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace z3nth10n_Launcher
{
    public partial class Form1 : Form
    {
        private static string minecraftJAR
        {
            get
            {
                return Path.Combine(ApiBasics.AssemblyFolderPATH, "minecraft.jar");
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;

            bool _off = !ApiBasics.OfflineMode || ApiBasics.GetSO() == OS.Linux;
            if (_off)
                try
                {
                    pictureBox1.Load("http://localhost/z3nth10n-PHP/logo.php");
                }
                catch
                {
                    _off = false;
                }

            if (!_off)
            {
                Font ff = null;

                string f = Path.Combine(ApiBasics.LocalPATH, "Minecraft.otf");

                if (ApiBasics.PreviousChk(f))
                    File.WriteAllBytes(f, Properties.Resources.MBold);

                PrivateFontCollection pfc = new PrivateFontCollection();

                pfc.AddFontFile(f); //Aun asi no funciona, solo en WIN

                ff = new Font(pfc.Families[0], 30);

                pictureBox1.Image = ApiBasics.DrawText("Minecraft Launcher", ff, Color.FromArgb(255, 127, 127, 127), Color.Transparent);
            }

            lblNotifications.Text = CheckValidJar() ? "" : "You have to put this executable inside of a valid Minecraft folder, next to minecraft.jar file.";
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
            if ((DateTime.Now - lastTime).TotalMilliseconds > 2000)
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    lblNotifications.Text = "You have to specify an username!!";
                    ApiBasics.Shake(this);

                    using (Stream s = Properties.Resources.sound101)
                        (new SoundPlayer(s)).Play();
                }

                lastTime = DateTime.Now;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Exit();
        }
    }
}