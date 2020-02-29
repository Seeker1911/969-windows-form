using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Appointment_Tracker
{
    public partial class Appointment : Form
    {
        public Appointment()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.Owner.Show();
        }

        private void ReportButton_Click(object sender, EventArgs e)
        {
            DataRowView drv = typeComboBox.SelectedItem as DataRowView;
            try
            {
                string item = typeComboBox.SelectedItem.ToString();
                IDictionary<string, object> dictionary = DBHelper.reportAppoint(item);
                janResult.Text = dictionary["January"].ToString();
                febResult.Text = dictionary["February"].ToString();
                marResult.Text = dictionary["March"].ToString();
                aprResult.Text = dictionary["April"].ToString();
                mayResult.Text = dictionary["May"].ToString();
                junResult.Text = dictionary["June"].ToString();
                julResult.Text = dictionary["July"].ToString();
                augResult.Text = dictionary["August"].ToString();
                septResult.Text = dictionary["September"].ToString();
                octResult.Text = dictionary["October"].ToString();
                novResult.Text = dictionary["November"].ToString();
                decResult.Text = dictionary["December"].ToString();
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }

        }
    }
}
