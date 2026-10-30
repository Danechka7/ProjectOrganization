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
        }

        // Навигация по страницам
        private void BtnEmployees_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new EmployeesPage(_role));
        private void BtnDepartments_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new DepartmentsPage(_role));
        private void BtnOrganizations_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new OrganizationsPage(_role));
        private void BtnContracts_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ContractsPage(_role));
        private void BtnWorks_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new WorksPage(_role));

        // Выход из аккаунта
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            new AuthWindow().Show();
            this.Close();
        }
    }
}