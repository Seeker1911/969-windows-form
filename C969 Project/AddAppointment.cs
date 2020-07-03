using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace Appointment_Tracker
{

    public partial class AddAppointment : Form
    {
        public string dataString = DBHelper.getDataString();
        public AddAppointment()
        {
            InitializeComponent();
            GetCustomerInfo();
            endDTP.Value = endDTP.Value.AddMinutes(30);

        }

        public void GetCustomerInfo()
        {
            MySqlConnection conn = new MySqlConnection(dataString);

            try
            {
                string query = "SELECT customerId, customerName as Display FROM customer;";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                conn.Open();
                DataSet data = new DataSet();
                adapter.Fill(data, "Customer");
                custComboBox.DisplayMember = "Display";
                custComboBox.ValueMember = "customerId";
                custComboBox.DataSource = data.Tables["Customer"];
            }
            catch (Exception ex)
            {
                // write exception info to log or anything else
                MessageBox.Show("error: " + ex);
            }
        }

        private void CreateCustomerButton(object sender, EventArgs e)
        {
            bool pass = validateInput();
            if (pass)
            {
                if (custComboBox.SelectedItem != null)
                {
                    DataRowView drv = custComboBox.SelectedItem as DataRowView;
                    int custID = Convert.ToInt32(custComboBox.SelectedValue);
                    DateTime start = startDTP.Value.ToUniversalTime();
                    DateTime end = endDTP.Value.ToUniversalTime();
                    //Validations
                    int validated = appointmentValidator(start, end);

                    switch (validated)
                    {
                        case 1:
                            MessageBox.Show(@"This appointment does not fall within business hours.");
                            break;
                        case 2:
                            MessageBox.Show("This appointment Overlaps with another appointment.");
                            break;
                        case 3:
                            MessageBox.Show("The appointments starts after the end time.");
                            break;
                        case 4:
                            MessageBox.Show("This appointment start and end date are not on the same date.");
                            break;
                        default:
                            DBHelper.createAppointment(custID, titleTextbox.Text, descriptionTextbox.Text, locationTextbox.Text, contactTextbox.Text, typeCombobox.SelectedItem.ToString(), start, end);
                            this.Owner.Show();
                            this.Close();
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("A Customer must be selected!");
                }
            }

        }

        public int appointmentValidator(DateTime start, DateTime end)
        {
            DateTime localStart = start.ToLocalTime();
            DateTime localEnd = end.ToLocalTime();
            DateTime businessStart = DateTime.Today.AddHours(8);
            DateTime businessEnd = DateTime.Today.AddHours(19);

            //outside allowed hours
            if (localStart.TimeOfDay < businessStart.TimeOfDay || localEnd.TimeOfDay > businessEnd.TimeOfDay)
            {
                return 1;
            }
            //overlap
            if (DBHelper.overlap(start, end) != 0)
            {
                return 2;
            }
            //start after end
            if (localStart.TimeOfDay > localEnd.TimeOfDay)
            {
                return 3;
            }
            //Not same day
            if (localStart.ToShortDateString() != localEnd.ToShortDateString())
            {
                return 4;
            }
            //pass 
            return 0;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Owner.Show();
            this.Close();
        }

        private bool validateInput()
        {

            if (emptyCheck() == false)
            {
                MessageBox.Show(@"Please fill out all Information fields.");
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(contactTextbox.Text, "[^a-zA-Z]+$"))
            {
                showError(contactLabel.Text);
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(descriptionTextbox.Text, "[^a-zA-Z][.!]+$"))
            {
                showError(descriptionLabel.Text);
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(locationTextbox.Text, "[^a-zA-Z]+$"))
            {
                showError(locationLabel.Text);
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(titleTextbox.Text, "[^a-zA-Z]+$"))
            {
                showError(titleLabel.Text);
                return false;
            }
            if (typeCombobox.SelectedIndex == -1)
            {
                showError(typeLabel.Text);
                return false;
            }

            return true;
        }

        private void showError(string item)
        {
            MessageBox.Show("Please enter a valid information for " + item);

        }
        private bool emptyCheck()
        {
            foreach (Control con in this.Controls)
            {
                if (con is TextBox textBox)
                {
                    if (textBox.Text == string.Empty)
                    {
                        return false;
                    }
                }
                if (con is ComboBox) {
                    ComboBox combo = con as ComboBox;
                    if (combo.SelectedIndex == -1) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
