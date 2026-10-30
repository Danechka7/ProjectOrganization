using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectOrganization
{
    public partial class EmployeesPage : Page
    {
        private string _role;

        public EmployeesPage(string role)
        {
            InitializeComponent();
            _role = role;

            if (_role == "Employee" || _role == "Manager")
            {
                BtnAdd.Visibility = Visibility.Collapsed;
                BtnDelete.Visibility = Visibility.Collapsed;
            }

            LoadDepartments();
            UpdateList();
        }

        private void LoadDepartments()
        {
            var db = new ProjectOrganizationDBEntities();
            var deps = db.Departments.ToList();
            deps.Insert(0, new Departments { DepartmentID = 0, DepartmentName = "Все отделы" });
            CmbFilterDept.ItemsSource = deps;
            CmbFilterDept.SelectedIndex = 0;
        }

        private void UpdateList()
        {
            if (EmployeeListView == null) return;

            var db = new ProjectOrganizationDBEntities();
            var list = db.Employees.ToList();

            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                list = list.Where(x => x.LastName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            if (CmbFilterDept.SelectedIndex > 0)
                list = list.Where(x => x.DepartmentID == (int)CmbFilterDept.SelectedValue).ToList();

            if (CmbSort.SelectedIndex == 1) list = list.OrderBy(x => x.LastName).ToList();
            if (CmbSort.SelectedIndex == 2) list = list.OrderByDescending(x => x.LastName).ToList();
            if (CmbSort.SelectedIndex == 3) list = list.OrderByDescending(x => x.Age).ToList();

            EmployeeListView.ItemsSource = list;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void CmbFilterDept_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;
            var editWindow = new EmployeeEditWindow();
            if (editWindow.ShowDialog() == true) UpdateList();
        }

        private void EmployeeListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role == "Employee" || EmployeeListView.SelectedItem == null) return;
            var emp = (Employees)EmployeeListView.SelectedItem;
            var editWindow = new EmployeeEditWindow(emp);
            if (editWindow.ShowDialog() == true) UpdateList();
        }

        // УДАЛЕНИЕ СОТРУДНИКА
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;

            if (EmployeeListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника для удаления!");
                return;
            }

            var emp = (Employees)EmployeeListView.SelectedItem;

            try
            {
                var db = new ProjectOrganizationDBEntities();

                // Проверка связей: является ли он начальником отдела, ответственным за договор или работу
                bool isHead = db.Departments.Any(x => x.HeadID == emp.EmployeeID);
                bool hasContracts = db.Contracts.Any(x => x.ResponsibleEmployeeID == emp.EmployeeID);
                bool hasWorks = db.ProjectWorks.Any(x => x.ResponsibleEmployeeID == emp.EmployeeID);

                if (isHead || hasContracts || hasWorks)
                {
                    MessageBox.Show("Невозможно удалить сотрудника, так как он является начальником отдела или ответственным по договорам/работам! Сначала переназначьте его.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show($"Удалить сотрудника {emp.LastName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                var empToDelete = db.Employees.FirstOrDefault(x => x.EmployeeID == emp.EmployeeID);
                if (empToDelete != null)
                {
                    db.Employees.Remove(empToDelete);
                    db.SaveChanges();
                    MessageBox.Show("Сотрудник удален.");
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