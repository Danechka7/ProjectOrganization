using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectOrganization
{
    public partial class OrganizationsPage : Page
    {
        private string _role;

        public OrganizationsPage(string role)
        {
            InitializeComponent();
            _role = role;
            if (_role == "Employee") BtnAdd.Visibility = Visibility.Collapsed;
            LoadCities();
            UpdateList();
        }

        private void LoadCities()
        {
            var db = new ProjectOrganizationDBEntities();
            var cities = db.Organizations.Select(x => x.City).Distinct().ToList();
            cities.Insert(0, "Все города");
            CmbFilterCity.ItemsSource = cities;
            CmbFilterCity.SelectedIndex = 0;
        }

        private void UpdateList()
        {
            if (OrganizationListView == null) return;

            var db = new ProjectOrganizationDBEntities();
            var list = db.Organizations.ToList();

            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                list = list.Where(x => x.OrgName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            if (CmbFilterCity.SelectedIndex > 0)
                list = list.Where(x => x.City == CmbFilterCity.SelectedItem.ToString()).ToList();

            OrganizationListView.ItemsSource = list;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void CmbFilterCity_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee") return;
            var win = new OrganizationEditWindow();
            if (win.ShowDialog() == true) { LoadCities(); UpdateList(); }
        }

        private void OrganizationListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role == "Employee" || OrganizationListView.SelectedItem == null) return;
            var org = (Organizations)OrganizationListView.SelectedItem;
            var win = new OrganizationEditWindow(org);
            if (win.ShowDialog() == true) { LoadCities(); UpdateList(); }
        }
    }
}