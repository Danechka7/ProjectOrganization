using System;
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
            if (_role == "Employee")
            {
                BtnAdd.Visibility = Visibility.Collapsed;
                BtnDelete.Visibility = Visibility.Collapsed;
            }
            else if (_role == "Manager")
            {
                BtnDelete.Visibility = Visibility.Collapsed;
            }
            LoadCities();
            UpdateList();
        }

        private void LoadCities()
        {
            using (var db = new ProjectOrganizationDBEntities())
            {
                var cities = db.Organizations.Select(x => x.City).Where(x => x != null).Distinct().ToList();
                cities.Insert(0, "Все города");
                CmbFilterCity.ItemsSource = cities;
                CmbFilterCity.SelectedIndex = 0;
            }
        }

        private void UpdateList()
        {
            if (OrganizationListView == null) return;
            using (var db = new ProjectOrganizationDBEntities())
            {
                var list = db.Organizations.ToList();
                if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                    list = list.Where(x => x.OrgName != null && x.OrgName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();
                if (CmbFilterCity.SelectedIndex > 0 && CmbFilterCity.SelectedItem != null)
                    list = list.Where(x => x.City == CmbFilterCity.SelectedItem.ToString()).ToList();
                OrganizationListView.ItemsSource = list;
            }
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void CmbFilterCity_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee") return;
            var win = new OrganizationEditWindow();
            if (win.ShowDialog() == true)
            {
                LoadCities();
                UpdateList();
            }
        }

        private void OrganizationListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role == "Employee" || OrganizationListView.SelectedItem == null) return;
            var org = (Organizations)OrganizationListView.SelectedItem;
            var win = new OrganizationEditWindow(org);
            if (win.ShowDialog() == true)
            {
                LoadCities();
                UpdateList();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;
            if (OrganizationListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите организацию!");
                return;
            }
            var org = (Organizations)OrganizationListView.SelectedItem;
            try
            {
                using (var db = new ProjectOrganizationDBEntities())
                {
                    bool hasContracts = db.Contracts.Any(x => x.OrganizationID == org.OrganizationID);
                    if (hasContracts)
                    {
                        MessageBox.Show("Невозможно удалить! С организацией заключены договоры.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (MessageBox.Show($"Удалить организацию {org.OrgName}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
                    var orgToDelete = db.Organizations.FirstOrDefault(x => x.OrganizationID == org.OrganizationID);
                    if (orgToDelete != null)
                    {
                        db.Organizations.Remove(orgToDelete);
                        db.SaveChanges();
                        LoadCities();
                        UpdateList();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}