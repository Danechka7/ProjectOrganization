using System.Windows;
using System.Windows.Media;

namespace ProjectOrganization
{
    public partial class MainWindow : Window
    {
        private string _role;
        private string _employeeName;

        public MainWindow(string role, string employeeName = "")
        {
            InitializeComponent();
            _role = NormalizeRole(role);   // >>> ИЗМЕНЕНИЕ
            _employeeName = employeeName;

            string displayName = !string.IsNullOrWhiteSpace(_employeeName)
                ? _employeeName.Trim()
                : GetRoleFallbackName(_role);

            TxtUserInfo.Text = $"Вы вошли как:\n{displayName}";

            if (_role == "Admin")
            {
                UserInfoBorder.Background = Brushes.Yellow;
                TxtUserInfo.Foreground = Brushes.Black;
            }
            else
            {
                UserInfoBorder.Background = Brushes.Transparent;
                TxtUserInfo.Foreground = Brushes.White;
            }
        }

        // >>> ИЗМЕНЕНИЕ: защита от регистра <<<
        private string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return "Employee";
            switch (role.Trim().ToLower())
            {
                case "admin": return "Admin";
                case "manager": return "Manager";
                case "employee": return "Employee";
                default: return "Employee";
            }
        }

        private string GetRoleFallbackName(string role)
        {
            if (role == "Admin") return "Администратор";
            if (role == "Manager") return "Менеджер";
            if (role == "Employee") return "Сотрудник";
            return "Гость";
        }

        private void BtnEmployees_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new EmployeesPage(_role));
        private void BtnDepartments_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new DepartmentsPage(_role));
        private void BtnOrganizations_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new OrganizationsPage(_role));
        private void BtnContracts_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ContractsPage(_role));
        private void BtnWorks_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new WorksPage(_role));

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            new AuthWindow().Show();
            this.Close();
        }
    }
}