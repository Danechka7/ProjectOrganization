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
            string password = TxtPassword.Password.Trim();

            if (login == "" || password == "")
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            try
            {
                // 1. Берем строку подключения из твоего рабочего контекста EF
                string efConnStr = new ProjectOrganizationDBEntities().Database.Connection.ConnectionString;

                // 2. Вытаскиваем из нее только имя сервера
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(efConnStr);
                string serverName = builder.DataSource;

                // 3. Формируем строку подключения с введенными логином и паролем
                string testConnStr = $"Data Source={serverName};Initial Catalog=ProjectOrganizationDB;User ID={login};Password={password};";

                // 4. Пытаемся подключиться к базе
                using (SqlConnection conn = new SqlConnection(testConnStr))
                {
                    conn.Open(); // Если логин/пароль неверные - вылетит ошибка 18456

                    // Если мы здесь, значит логин и пароль ПРАВИЛЬНЫЕ!

                    // Определяем роль
                    string role = "Employee";
                    if (login.StartsWith("Admin")) role = "Admin";
                    else if (login.StartsWith("Manager")) role = "Manager";

                    // Открываем главное окно и закрываем окно авторизации
                    new MainWindow(role).Show();
                    this.Close();
                }
            }
            catch (SqlException ex)
            {
                // Ошибка 18456 - это "Login failed" (неверный логин или пароль)
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