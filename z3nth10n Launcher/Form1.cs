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
                return Path.Combine(APIBasics.AssemblyFolderPATH, "minecraft.jar");
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;

            bool _off = !APIBasics.OfflineMode || APIBasics.GetSO() == OS.Linux;
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

                string f = Path.Combine(APIBasics.LocalPATH, "Minecraft.otf");

                if (APIBasics.PreviousChk(f))
                    File.WriteAllBytes(f, Properties.Resources.MBold);

                PrivateFontCollection pfc = new PrivateFontCollection();
                //string f = Program.URLToLocalFile("https://github.com/z3nth10n/MC-Launcher/raw/master/z3nth10n%20Launcher/Resources/Minecraft.ttf"); //"https://github.com/z3nth10n/MC-Launcher/raw/master/z3nth10n%20Launcher/Resources/MBold.otf");

                pfc.AddFontFile(f); //Aun asi no funciona, solo en WIN

                ff = new Font(pfc.Families[0], 30);

                pictureBox1.Image = APIBasics.DrawText("Minecraft Launcher", ff, Color.FromArgb(255, 127, 127, 127), Color.Transparent);
            }

            lblNotifications.Text = CheckValidJar() ? "" : "You have to put this executable inside of a valid Minecraft folder, next to minecraft.jar file.";
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        //Funcs

        private static bool CheckValidJar()
        {
            if (!File.Exists(minecraftJAR)) return false;
            else if (!APIBasics.AssemblyFolderPATH.Contains("bin") || !APIBasics.AssemblyFolderPATH.Contains("versions")) return false; //Tengo que comprobar la version de la carpeta "versions"

            return APILauncher.IsValidJAR(minecraftJAR);
        }

        private DateTime lastTime = DateTime.Now;

        private void button1_Click(object sender, EventArgs e)
        {
            if ((DateTime.Now - lastTime).TotalMilliseconds > 2000)
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    lblNotifications.Text = "You have to specify an username!!";
                    APIBasics.Shake(this);

                    using (Stream s = Properties.Resources.sound101)
                        (new SoundPlayer(s)).Play();
                }

                lastTime = DateTime.Now;
            }
        }
    }
}