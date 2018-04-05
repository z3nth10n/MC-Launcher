using System;
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
            if (Program.IsRunningOnMono())
            {
                PrivateFontCollection pfc = new PrivateFontCollection();
                pfc.AddFontFile(Program.URLToLocalFile("https://github.com/z3nth10n/MC-Launcher/raw/master/z3nth10n%20Launcher/Resources/MBold.otf"));

                label1.Font = new Font(pfc.Families[0], 30);
            }
            else
            {
                MemoryFonts.AddMemoryFont(Properties.Resources.MBold);

                label1.Font = MemoryFonts.GetFont(0, 30);
            }
        }
    }
}