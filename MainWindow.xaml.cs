using System.Windows;

namespace ProjectOrganization
{
    public partial class MainWindow : Window
    {
        private string _role;

        public MainWindow(string role)
        {
            InitializeComponent();
            _role = role;
            string loginName = "Гость";
            if (role == "Admin") loginName = "Администратор";
            else if (role == "Manager") loginName = "Менеджер";
            else if (role == "Employee") loginName = "Сотрудник";
            TxtUserInfo.Text = $"Вы вошли как:\n{loginName}";
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