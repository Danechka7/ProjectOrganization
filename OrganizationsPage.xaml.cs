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
            if (_role != "Manager")
            {
                BtnAdd.Visibility = Visibility.Collapsed;
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
                    list = list.Where(x => x.OrgName != null &&
                                           x.OrgName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

                if (CmbFilterCity.SelectedIndex > 0 && CmbFilterCity.SelectedItem != null)
                    list = list.Where(x => x.City == CmbFilterCity.SelectedItem.ToString()).ToList();

                OrganizationListView.ItemsSource = list;
            }
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void CmbFilterCity_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Manager")
            {
                MessageBox.Show("Добавление организаций доступно только менеджеру.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var win = new OrganizationEditWindow();
            if (win.ShowDialog() == true)
            {
                LoadCities();
                UpdateList();
            }
        }

        private void OrganizationListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role != "Manager" || OrganizationListView.SelectedItem == null) return;
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
            if (_role != "Manager")
            {
                MessageBox.Show("Удаление организаций доступно только менеджеру.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (OrganizationListView.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите организацию в списке (кликните по строке).",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var org = (Organizations)OrganizationListView.SelectedItem;

            if (MessageBox.Show($"Удалить организацию «{org.OrgName}»?\n\nДействие необратимо.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new ProjectOrganizationDBEntities())
                {
                    bool hasContracts = db.Contracts.Any(x => x.OrganizationID == org.OrganizationID);
                    if (hasContracts)
                    {
                        MessageBox.Show("Невозможно удалить! С организацией заключены договоры.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var orgToDelete = db.Organizations.FirstOrDefault(x => x.OrganizationID == org.OrganizationID);
                    if (orgToDelete == null)
                    {
                        MessageBox.Show("Организация не найдена в базе.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        UpdateList();
                        return;
                    }

                    db.Organizations.Remove(orgToDelete);
                    db.SaveChanges();
                }

                MessageBox.Show("Организация удалена.", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCities();
                UpdateList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + (ex.InnerException?.Message ?? ex.Message),
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}