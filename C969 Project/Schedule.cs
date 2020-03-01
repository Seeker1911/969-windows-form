using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Appointment_Tracker
{
    public partial class Schedule : Form
    {
        public static List<KeyValuePair<string, object>> UserList;

        public Schedule()
        {
            InitializeComponent();
            UserDisplay();
        }
        public void SetUserList(List<KeyValuePair<string, object>> list)
        {

            UserList = list;

        }

        public static List<KeyValuePair<string, object>> GetUserList()
        {

            return UserList;
        }


        public void UserDisplay()
        {
            MySqlConnection conn = new MySqlConnection(DBHelper.getDataString());

            try
            {
                string query = "select  userId, userName as 'Display' from user;";
                MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                conn.Open();
                DataSet ds = new DataSet();
                da.Fill(ds, "User");
                consultantComboBox.DisplayMember = "Display";
                consultantComboBox.ValueMember = "userId";
                consultantComboBox.DataSource = ds.Tables["User"];
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"error: " + ex);
            }
        }

        private void ReportButton_Click(object sender, EventArgs e)
        {
            DataRowView drv = consultantComboBox.SelectedItem as DataRowView;
            int id = Convert.ToInt32(consultantComboBox.SelectedValue);

            DataTable dtRecord = DBHelper.schedule(id.ToString());
            dataGridView.DataSource = dtRecord;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.Owner.Show();
        }
    }
}
