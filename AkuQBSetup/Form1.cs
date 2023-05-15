using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AkuQBSetup
{
    public partial class Setup : Form
    {
        public Setup()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dir = Application.StartupPath;
            Process.Start(dir + @"\SmartCalc.exe", "-unregserver");

        }

        private void Register_Click(object sender, EventArgs e)
        {
            var dir = Application.StartupPath;
            Process.Start(dir + @"\SmartCalc.exe", "-regserver");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var dir = Application.StartupPath;
            Process.Start(dir + @"\SmartCalc.exe", "-u \"AkuCalc\"") ;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dir = Application.StartupPath;
            Process.Start(dir + @"\SmartCalc.exe", "-d");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var dir = Application.StartupPath;
            Process.Start(dir + @"\AkuCalcConfiguration.exe");
        }
    }
}
