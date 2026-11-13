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
            if (_role == "Employee")
            {
                BtnAdd.Visibility = Visibility.Collapsed;
                BtnDelete.Visibility = Visibility.Collapsed;
            }
            else if (_role == "Manager")
            {
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
            if (_role == "Employee") return;
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
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;
            if (DepartmentListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите отдел!");
                return;
            }
            var dep = (Departments)DepartmentListView.SelectedItem;
            try
            {
                var db = new ProjectOrganizationDBEntities();
                bool hasEmployees = db.Employees.Any(x => x.DepartmentID == dep.DepartmentID);
                bool hasWorks = db.ProjectWorks.Any(x => x.DepartmentID == dep.DepartmentID);
                if (hasEmployees || hasWorks)
                {
                    MessageBox.Show("Невозможно удалить! В отделе есть сотрудники или работы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (MessageBox.Show($"Удалить отдел {dep.DepartmentName}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
                var depToDelete = db.Departments.FirstOrDefault(x => x.DepartmentID == dep.DepartmentID);
                if (depToDelete != null)
                {
                    db.Departments.Remove(depToDelete);
                    db.SaveChanges();
                    UpdateList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}