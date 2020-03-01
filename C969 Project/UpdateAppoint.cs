using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Appointment_Tracker
{
    public partial class UpdateAppoint : Form
    {
        public static List<KeyValuePair<string, object>> AppointList;
        public UpdateAppoint()
        {
            InitializeComponent();
            fillAppointment();
        }

        public void setAppointmentList(List<KeyValuePair<string, object>> list)
        {

            AppointList = list;

        }

        public static List<KeyValuePair<string, object>> getAppointList()
        {

            return AppointList;
        }


        private void SearchButton_Click(object sender, EventArgs e)
        {
            //Grabs ID
            DataRowView drv = appointComboBox.SelectedItem as DataRowView;
            int id = Convert.ToInt32(appointComboBox.SelectedValue);

            var appointmentList = DBHelper.lookupAppointment(id);
            setAppointmentList(appointmentList);


            if (appointmentList != null)
            {
                setEnableStatus(true);
                fillFields(appointmentList);
                fillCust(Convert.ToInt32(appointmentList.First(kvp => kvp.Key == "customerId").Value.ToString()));
            }

        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            //Locks fields
            setEnableStatus(false);
            //Lamba used for function to clear fields
            Action<Control.ControlCollection> clearIT = null;

            clearIT = (controls) =>
            {
                foreach (Control option in controls)
                    if (option is TextBox)
                        (option as TextBox).Clear();
                    else if (option is RadioButton)
                        (option as RadioButton).Checked = false;
                    else if (option is DateTimePicker)
                        (option as DateTimePicker).Value = DateTime.Now;
                    else if (option is ComboBox)
                        (option as ComboBox).SelectedIndex = -1;
                    else
                        clearIT(option.Controls);
            };

            clearIT(Controls);
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            bool pass = Validator();
            if (pass)
            {
                DialogResult youSure = MessageBox.Show("Are you sure you want to update this Appointment?", "", MessageBoxButtons.YesNo);
                if (youSure == DialogResult.Yes)
                {
                    try
                    {
                        //Grab List & convert
                        var list = getAppointList();
                        IDictionary<string, object> dictionary = list.ToDictionary(pair => pair.Key, pair => pair.Value);
                        //replace values for the keys in the form         
                        dictionary["appointmentId"] = AppointList.First(kvp => kvp.Key == "appointmentId").Value;
                        dictionary["customerId"] = customerComboBox.SelectedValue;
                        dictionary["title"] = titleTextbox.Text;
                        dictionary["description"] = descriptionTextbox.Text;
                        dictionary["location"] = locationTextbox.Text;
                        dictionary["contact"] = contactTextbox.Text;
                        dictionary["type"] = typeCombobox.SelectedItem.ToString();
                        dictionary["start"] = startDTP.Value;
                        dictionary["end"] = endDTP.Value;
                        dictionary["url"] = customerComboBox.SelectedValue;

                        //Pass the updated IDictionary to dbhelper to update the database
                        DBHelper.updateAppointment(dictionary);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(@"error: " + ex);
                    }
                    finally
                    {
                        ClearButton_Click(null, new EventArgs());
                        MessageBox.Show("Customer Record Updated");
                        this.Owner.Show();
                        this.Close();
                    }
                }
            }
        }

            private void CancelButton_Click(object sender, EventArgs e)
            {
                this.Owner.Show();
                this.Close();
            }

            public void fillAppointment()
            {
                MySqlConnection conn = new MySqlConnection(DBHelper.getDataString());

                try
                {
                    string query = "select appointmentId, concat('apt. ID: ' , appointmentId, (select  concat(' -- Customer: ', customerName) from customer where appointment.customerId = customer.customerId))  as Display from appointment where end > now();";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    conn.Open();
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, "Appointment");
                    appointComboBox.DisplayMember = "Display";
                    appointComboBox.ValueMember = "appointmentId";
                    appointComboBox.DataSource = dataSet.Tables["Appointment"];
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"error: " + ex);
                }
            }

            public void fillCust(int custID)
            {
                MySqlConnection conn = new MySqlConnection(DBHelper.getDataString());

                try
                {
                    string query = $"SELECT c.customerId, concat('apt. ID: ', a.appointmentId, ' : ', c.customerName) as Display " +
                                   $"FROM customer c inner join appointment a on c.customerId = a.customerId;";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    conn.Open();
                    DataSet dataset = new DataSet();
                    adapter.Fill(dataset, "Customer");
                    customerComboBox.DisplayMember = "Display";
                    customerComboBox.ValueMember = "customerID";
                    customerComboBox.DataSource = dataset.Tables["Customer"];
                    DataRowView drv = customerComboBox.SelectedItem as DataRowView;
                    customerComboBox.SelectedValue = custID;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"error: " + ex);
                }
            }

            private void setEnableStatus(bool status)
            {
                customerComboBox.Enabled = status;
                titleTextbox.Enabled = status;
                descriptionTextbox.Enabled = status;
                locationTextbox.Enabled = status;
                contactTextbox.Enabled = status;
                typeCombobox.Enabled = status;
                startDTP.Enabled = status;
                endDTP.Enabled = status;
                updateButton.Enabled = status;
            }

            private void fillFields(List<KeyValuePair<string, object>> AppointList)
            {
                // Lambda used to set text values
                titleTextbox.Text = AppointList.First(kvp => kvp.Key == "title").Value.ToString();
                descriptionTextbox.Text = AppointList.First(kvp => kvp.Key == "description").Value.ToString();
                locationTextbox.Text = AppointList.First(kvp => kvp.Key == "location").Value.ToString();
                contactTextbox.Text = AppointList.First(kvp => kvp.Key == "contact").Value.ToString();
                typeCombobox.SelectedIndex = typeCombobox.FindStringExact(AppointList.First(kvp => kvp.Key == "type").Value.ToString());
                string start = AppointList.First(kvp => kvp.Key == "start").Value.ToString();
                string end = AppointList.First(kvp => kvp.Key == "end").Value.ToString();
                startDTP.Value = Convert.ToDateTime(start).ToLocalTime();
                endDTP.Value = Convert.ToDateTime(end).ToLocalTime();

            }

        private bool Validator()
        {

            if (checkIfEmpty())
            {
                MessageBox.Show(@"Please complete all Appointment Information fields.");
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(titleTextbox.Text, "[^a-zA-Z0-9]+$"))
            {
                displayError(titleLabel.Text);
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(descriptionTextbox.Text, "[^a-zA-Z]+$"))
            {
                displayError(descriptionLabel.Text);
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(locationTextbox.Text, "[^a-zA-Z]+$"))
            {
                displayError(locationLabel.Text);
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(contactTextbox.Text, "[^a-zA-Z]+$"))
            {
                displayError(contactLabel.Text);
                return false;
            }
            if (typeCombobox.SelectedIndex == -1)
            {
                displayError(typeLabel.Text);
                return false;
            }
            if (customerComboBox.SelectedIndex == -1)
            {
                displayError(customerLabel.Text);
                return false;
            }

            return true;
        }

        private void displayError(string item)
        {
            MessageBox.Show(@"Please enter a valid information for " + item);

        }
        private bool checkIfEmpty()
        {
            foreach (Control con in this.Controls)
            {
                if (con is TextBox)
                {
                    TextBox textBox = con as TextBox;
                    if (textBox.Text == string.Empty)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    }
