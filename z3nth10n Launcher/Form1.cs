using Ionic.Zip;
using System;
using System.Collections.Generic;
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

            bool _off = !Program.OfflineMode || Program.IsLinux;
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

                string f = Path.Combine(Program.LocalPATH, "Minecraft.otf");

                if (Program.PreviousChk(f))
                    File.WriteAllBytes(f, Properties.Resources.MBold);

                PrivateFontCollection pfc = new PrivateFontCollection();
                //string f = Program.URLToLocalFile("https://github.com/z3nth10n/MC-Launcher/raw/master/z3nth10n%20Launcher/Resources/Minecraft.ttf"); //"https://github.com/z3nth10n/MC-Launcher/raw/master/z3nth10n%20Launcher/Resources/MBold.otf");

                pfc.AddFontFile(f); //Aun asi no funciona, solo en WIN

                ff = new Font(pfc.Families[0], 30);

                pictureBox1.Image = Program.DrawText("Minecraft Launcher", ff, Color.FromArgb(255, 127, 127, 127), Color.Transparent);
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
            string minecraftJAR = Path.Combine(Program.AssemblyPATH, "minecraft.jar");

            if (!File.Exists(minecraftJAR)) return false;

            using (FileStream fs = new FileStream(minecraftJAR, FileMode.Open))
            {
                //List<String> classNames = new List<string>();
                ZipInputStream zip = new ZipInputStream(fs);
                for (ZipEntry entry = zip.GetNextEntry(); entry != null; entry = zip.GetNextEntry())
                    if (!entry.IsDirectory && entry.FileName.EndsWith(".class"))
                    {
                        Console.WriteLine(entry.FileName);
                    }
            }

            return true;
        }
    }
}