using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AkuCalcDesktop
{
    public partial class AkuSettings : Form
    {
        public AkuSettings()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Store the user input in variables
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string endpoint = txtEndpoint.Text;

            // Create or update the configuration file
            using (StreamWriter writer = new StreamWriter("config.txt"))
            {
                writer.WriteLine("Username=" + username);
                writer.WriteLine("Password=" + password);
                writer.WriteLine("Endpoint=" + endpoint);
            }

            MessageBox.Show("Configuration saved successfully.");

        }

        private void Settings_Load(object sender, EventArgs e)
        {

        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
