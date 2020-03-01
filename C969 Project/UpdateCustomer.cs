using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Appointment_Tracker
{
    public partial class UpdateCustomer : Form
    {
        public static List<KeyValuePair<string, object>> CustomerList;

        public UpdateCustomer()
        {
            InitializeComponent();
            CustomerForm();
        }

        public void setCustomerList(List<KeyValuePair<string, object>> list)
        {
            CustomerList = list;
        }

        public static List<KeyValuePair<string, object>> getCustomerList()
        {
            return CustomerList;
        }



        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Owner.Show();
            this.Hide();
        }

        public void CustomerForm()
        {
            MySqlConnection conn = new MySqlConnection(DBHelper.getDataString());

            try
            {
                string query = "SELECT customerId, customerName as Display FROM customer;";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                conn.Open();
                DataSet data = new DataSet();
                adapter.Fill(data, "Customer");
                custComboBox.DisplayMember = "Display";
                custComboBox.ValueMember = "customerID";
                custComboBox.DataSource = data.Tables["Customer"];

            }
            catch (Exception ex)
            {
                MessageBox.Show(@"error: " + ex);
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            Enabling(false);
            //Lambda used for function to clear fields
            Action<Control.ControlCollection> clearForm = null;

            clearForm = (controls) =>
            {
                foreach (Control option in controls)
                    if (option is TextBox)
                        (option as TextBox).Clear();
                    else if (option is RadioButton)
                        (option as RadioButton).Checked = false;
                    else
                        clearForm(option.Controls);
            };

            clearForm(Controls);
        }


        private void SearchButton_Click(object sender, EventArgs e)
        {
           
            DataRowView drv = custComboBox.SelectedItem as DataRowView;
            int id = Convert.ToInt32(custComboBox.SelectedValue);
            var customerList = DBHelper.searchCustomer(id);
            setCustomerList(customerList);
            if (customerList != null)
            {
                Enabling(true);
                FillFields(customerList);
            }
        }

        private void Enabling(bool status)
        {
            nameTextbox.Enabled = status;
            phoneTextbox.Enabled = status;
            addressTextbox.Enabled = status;
            cityTextbox.Enabled = status;
            zipTextbox.Enabled = status;
            countryTextbox.Enabled = status;
            yesRadio.Enabled = status;
            noRadio.Enabled = status;
            updateButton.Enabled = status;
        }

        private void FillFields(List<KeyValuePair<string, object>> custList)
        {
            // Lambda used to set text values from kvp
            nameTextbox.Text = custList.First(kvp => kvp.Key == "customerName").Value.ToString();
            phoneTextbox.Text = custList.First(kvp => kvp.Key == "phone").Value.ToString();
            addressTextbox.Text = custList.First(kvp => kvp.Key == "address").Value.ToString();
            cityTextbox.Text = custList.First(kvp => kvp.Key == "city").Value.ToString();
            zipTextbox.Text = custList.First(kvp => kvp.Key == "postalCode").Value.ToString();
            countryTextbox.Text = custList.First(kvp => kvp.Key == "country").Value.ToString();
            if (Convert.ToInt32(custList.First(kvp => kvp.Key == "active").Value) == 1)
            {
                yesRadio.Checked = true;
            }
            else
            {
                noRadio.Checked = true;
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            DialogResult areYouSure = MessageBox.Show(@"Are you sure you want to update this customer?", "", MessageBoxButtons.YesNo);
            if (areYouSure == DialogResult.Yes)
            {
                try
                {
                    //Convert the list to dictionary
                    var list = getCustomerList();
                    IDictionary<string, object> dictionary = list.ToDictionary(pair => pair.Key, pair => pair.Value);
                    dictionary["customerName"] = nameTextbox.Text;
                    dictionary["phone"] = phoneTextbox.Text;
                    dictionary["address"] = addressTextbox.Text;
                    dictionary["city"] = cityTextbox.Text;
                    dictionary["postalCode"] = zipTextbox.Text;
                    dictionary["country"] = countryTextbox.Text;
                    dictionary["active"] = yesRadio.Checked ? 1 : 0;

                    DBHelper.updateCustomer(dictionary);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    clearButton_Click(null, new EventArgs());
                    MessageBox.Show(@"Customer Record Updated");

                    this.Owner.Show();
                    this.Close();
                }

            }
        }
    }
}
