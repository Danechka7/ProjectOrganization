using System;
using System.Linq;
using System.Windows;

namespace ProjectOrganization
{
    public partial class AuthWindow : Window
    {
        public static Employees CurrentEmployee { get; set; }

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            try
            {
                using (var db = new ProjectOrganizationDBEntities())
                {
                    var emp = db.Employees.FirstOrDefault(x =>
                        x.Login != null &&
                        x.Login.ToLower() == login.ToLower() &&
                        x.Password == password);

                    if (emp == null)
                    {
                        MessageBox.Show("Неверный логин или пароль!");
                        return;
                    }

                    CurrentEmployee = emp;

                    string role = "Employee";
                    if (!string.IsNullOrWhiteSpace(emp.Position))
                    {
                        string pos = emp.Position.ToLower();
                        if (pos.Contains("админ")) role = "Admin";
                        else if (pos.Contains("начальник") || pos.Contains("руковод") || pos.Contains("менедж")) role = "Manager";
                    }
                    string fio = $"{emp.LastName} {emp.FirstName} {emp.Patronymic}".Trim();
                    if (string.IsNullOrWhiteSpace(fio)) fio = emp.Login;

                    new MainWindow(role, fio).Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при входе: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}