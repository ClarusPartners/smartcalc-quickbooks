using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AkuCalcConfiguration
{
    public partial class AkuCalc : Form
    {
        public AkuCalc()
        {
            InitializeComponent();
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Store the user input in variables
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string endpoint = txtEndpoint.Text;
            string taxpayer = txtTaxpayer.Text;
            var dir = Application.StartupPath;
            // Create or update the configuration file
            using (StreamWriter writer = new StreamWriter(dir + @"\config.txt"))
            {
                writer.WriteLine("Username=" + username);
                writer.WriteLine("Password=" + password);
                writer.WriteLine("Endpoint=" + endpoint);
                writer.WriteLine("Taxpayer=" + taxpayer);

            }

            MessageBox.Show("Configuration saved successfully.");
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("https://claruspartners.com/contact-us/");
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void AkuCalc_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string endpoint = txtEndpoint.Text;
            string taxpayer = txtTaxpayer.Text;
            var dir = Application.StartupPath;
            var configPath = Application.StartupPath + @"\config.txt";
            // Create or update the configuration file
            using (StreamWriter writer = new StreamWriter(configPath))
            {
                writer.WriteLine("Username=" + "qbapi");
                writer.WriteLine("Password=" + "f15df058bee598625a2762554488d903");
                writer.WriteLine("Endpoint=" + @"http://mswsmartcalc.suchimsapps.com/");
                writer.WriteLine("Taxpayer=" + "MSW Consulting LLC");

            }

            MessageBox.Show("Configuration saved successfully.");

        }

        private void txtEndpoint_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
