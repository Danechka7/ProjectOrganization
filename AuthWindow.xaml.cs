using System;
using System.Windows;
using System.Data.SqlClient;

namespace ProjectOrganization
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Text.Trim();

            if (login == "" || password == "")
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            try
            {
                string efConnStr = new ProjectOrganizationDBEntities().Database.Connection.ConnectionString;
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(efConnStr);
                string serverName = builder.DataSource;
                string testConnStr = $"Data Source={serverName};Initial Catalog=ProjectOrganizationDB;User ID={login};Password={password};";
                using (SqlConnection conn = new SqlConnection(testConnStr))
                {
                    conn.Open();

                    string role = "Employee";
                    if (login.StartsWith("Admin")) role = "Admin";
                    else if (login.StartsWith("Manager")) role = "Manager";

                    new MainWindow(role).Show();
                    this.Close();
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 18456)
                {
                    MessageBox.Show("Неверный логин или пароль!");
                }
                else
                {
                    MessageBox.Show("Ошибка SQL Server: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Системная ошибка: " + ex.Message);
            }
        }
    }
}