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
            PrivateFontCollection pfc = new PrivateFontCollection();

            /*byte[] fontBytes = Properties.Resources.Minecraft_Regular;
            IntPtr fontData = Marshal.AllocCoTaskMem(fontBytes.Length);

            Marshal.Copy(fontBytes, 0, fontData, fontBytes.Length);
            pfc.AddMemoryFont(fontData, fontBytes.Length);
            Marshal.FreeCoTaskMem(fontData);

            Console.WriteLine(pfc.Families.Length);*/

            pfc.AddFontFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MBold.otf"));

            label1.Font = new Font(pfc.Families[0], 30);
            //label3.Font = new Font(pfc.Families[0], 15);
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }
    }
}