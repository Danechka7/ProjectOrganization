using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectOrganization
{
    public partial class DepartmentsPage : Page
    {
        private string _role;

        public DepartmentsPage(string role)
        {
            InitializeComponent();
            _role = role;

            if (_role == "Employee" || _role == "Manager")
            {
                BtnAdd.Visibility = Visibility.Collapsed;
                BtnDelete.Visibility = Visibility.Collapsed;
            }

            UpdateList();
        }

        private void UpdateList()
        {
            if (DepartmentListView == null) return;

            var db = new ProjectOrganizationDBEntities();
            var list = db.Departments.ToList();

            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                list = list.Where(x => x.DepartmentName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            if (CmbSort.SelectedIndex == 1) list = list.OrderBy(x => x.DepartmentName).ToList();
            if (CmbSort.SelectedIndex == 2) list = list.OrderBy(x => x.Floor).ToList();

            DepartmentListView.ItemsSource = list;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;
            var win = new DepartmentEditWindow();
            if (win.ShowDialog() == true) UpdateList();
        }

        private void DepartmentListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role == "Employee" || DepartmentListView.SelectedItem == null) return;
            var dep = (Departments)DepartmentListView.SelectedItem;
            var win = new DepartmentEditWindow(dep);
            if (win.ShowDialog() == true) UpdateList();
        }

        // УДАЛЕНИЕ ОТДЕЛА
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;

            if (DepartmentListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите отдел для удаления!");
                return;
            }

            var dep = (Departments)DepartmentListView.SelectedItem;

            try
            {
                var db = new ProjectOrganizationDBEntities();

                // Проверка: есть ли сотрудники или работы в этом отделе
                bool hasEmployees = db.Employees.Any(x => x.DepartmentID == dep.DepartmentID);
                bool hasWorks = db.ProjectWorks.Any(x => x.DepartmentID == dep.DepartmentID);

                if (hasEmployees || hasWorks)
                {
                    MessageBox.Show("Невозможно удалить отдел, так как в нем есть сотрудники или проектные работы! Сначала переведите их в другой отдел или удалите.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show($"Удалить отдел {dep.DepartmentName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                var depToDelete = db.Departments.FirstOrDefault(x => x.DepartmentID == dep.DepartmentID);
                if (depToDelete != null)
                {
                    db.Departments.Remove(depToDelete);
                    db.SaveChanges();
                    MessageBox.Show("Отдел удален.");
                    UpdateList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }
    }
}