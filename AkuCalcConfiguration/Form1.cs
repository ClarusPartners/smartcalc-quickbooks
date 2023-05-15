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
            var dir = Application.StartupPath;
            // Create or update the configuration file
            using (StreamWriter writer = new StreamWriter(dir + @"\config.txt"))
            {
                writer.WriteLine("Username=" + username);
                writer.WriteLine("Password=" + password);
                writer.WriteLine("Endpoint=" + endpoint);
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
    }
}
