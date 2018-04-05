﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Font ff = null;

            if (Program.IsLinux)
            {
                PrivateFontCollection pfc = new PrivateFontCollection();
                string f = Program.URLToLocalFile("https://github.com/z3nth10n/MC-Launcher/raw/master/z3nth10n%20Launcher/Resources/MBold.otf");
                Console.WriteLine(f);
                pfc.AddFontFile(f);

                ff = new Font(pfc.Families[0], 30);
                label1.Font = ff;
            }
            else
            {
                MemoryFonts.AddMemoryFont(Properties.Resources.MBold);

                ff = MemoryFonts.GetFont(0, 30);

                label1.Font = ff;
            }

            pictureBox1.Image = Program.DrawText("Hola", ff, Color.Red, Color.Transparent);
        }
    }
}