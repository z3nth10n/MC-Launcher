using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace z3nth10n_Launcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;

            if (!Program.OfflineMode || Program.IsLinux)
                pictureBox1.Load("http://localhost/z3nth10n/logo.php");
            else
            {
                Font ff = null;

                string f = Path.Combine(Program.LocalPATH, "Minecraft.otf");

                if (Program.PreviousChk(f))
                    File.WriteAllBytes(f, Properties.Resources.MBold);

                PrivateFontCollection pfc = new PrivateFontCollection();
                //string f = Program.URLToLocalFile("https://github.com/z3nth10n/MC-Launcher/raw/master/z3nth10n%20Launcher/Resources/Minecraft.ttf"); //"https://github.com/z3nth10n/MC-Launcher/raw/master/z3nth10n%20Launcher/Resources/MBold.otf");

                pfc.AddFontFile(f); //Aun asi no funciona, solo en WIN

                ff = new Font(pfc.Families[0], 30);

                pictureBox1.Image = Program.DrawText("Minecraft Launcher", ff, Color.FromArgb(255, 127, 127, 127), Color.Transparent);
            }

            lblNotifications.Text = "";
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}