using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Appointment_Tracker
{
    public partial class Login : Form
    {

        public string error = "The username and password entered are incorrect.";
        public string exit = "Are you sure ?";
        public Login()
        {
            InitializeComponent();
            //Supports English or French(LCID == 1036);
            CheckLanguage(CultureInfo.CurrentUICulture.LCID);
        }

        private void CheckLanguage(int LCID)
        {
            if (LCID == 1036)
            {
                this.Text = "S'identifier";
                infoLabel.Text = "entrez les informations de connexion";
                usernameLabel.Text = "Nom d'utilisateur ";
                passwordLabel.Text = "mot de passe";
                loginButton.Text = "s'identifier";
                cancelButton.Text = "annuler";
                error = "nom ou mot de passe incorrect.";
                exit = "etes-vous sur de vouloir quitter?";
            }
        }


        private void loginButton_Click(object sender, EventArgs e)
        {
            if (DBHelper.userCheck(userTextbox.Text, passTextbox.Text) == 1)
            {
                Logger.signIn(DBHelper.getCurrentUserName());
                Form dash = new Dashboard();
                Logger.reminder();
                dash.Show();
                this.Hide();
            }
            else
            {
               MessageBox.Show(error);
            }
            

        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(exit, this.Text, MessageBoxButtons.YesNo);
            if (confirm == DialogResult.Yes)
            {
                Close();
            }
        }
    }
}
