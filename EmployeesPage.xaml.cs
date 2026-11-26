using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectOrganization
{
    public partial class EmployeesPage : Page
    {
        private string _role;

        private class EmployeeRowView
        {
            public int EmployeeID { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string Patronymic { get; set; }
            public string Position { get; set; }
            public int? DepartmentID { get; set; }
            public string DepartmentNameDisplay { get; set; }
            public string Login { get; set; }
            public string Gender { get; set; }
            public int Age { get; set; }
            public string Address { get; set; }
            public Employees SourceEmployee { get; set; }
        }

        public EmployeesPage(string role)
        {
            InitializeComponent();
            _role = role;
            if (_role != "Manager")
            {
                BtnAdd.Visibility = Visibility.Collapsed;
                BtnDelete.Visibility = Visibility.Collapsed;
                TxtLoginHeader.Visibility = Visibility.Collapsed;
                LoginHeaderCol.Width = new GridLength(0);
            }

            LoadDepartments();
            UpdateList();
        }

        private void LoadDepartments()
        {
            using (var db = new ProjectOrganizationDBEntities())
            {
                var deps = db.Departments.ToList();
                deps.Insert(0, new Departments { DepartmentID = 0, DepartmentName = "Все отделы" });
                CmbFilterDept.ItemsSource = deps;
                CmbFilterDept.SelectedIndex = 0;
            }
        }

        private void UpdateList()
        {
            if (EmployeeListView == null) return;

            using (var db = new ProjectOrganizationDBEntities())
            {
                var departmentsMap = db.Departments.ToDictionary(d => (int?)d.DepartmentID, d => d.DepartmentName);
                var employees = db.Employees.ToList();

                List<EmployeeRowView> list = employees.Select(emp =>
                {
                    int? deptId = emp.DepartmentID;
                    string deptName = (deptId.HasValue && departmentsMap.ContainsKey(deptId.Value))
                        ? departmentsMap[deptId.Value]
                        : "—";

                    return new EmployeeRowView
                    {
                        EmployeeID = emp.EmployeeID,
                        LastName = emp.LastName,
                        FirstName = emp.FirstName,
                        Patronymic = emp.Patronymic,
                        Position = emp.Position,
                        DepartmentID = deptId,
                        DepartmentNameDisplay = deptName,
                        Login = emp.Login,
                        Gender = emp.Gender,
                        Age = emp.Age ?? 0,
                        Address = emp.Address,
                        SourceEmployee = emp
                    };
                }).ToList();

                if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                    list = list.Where(x => x.LastName != null &&
                                           x.LastName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

                if (CmbFilterDept.SelectedIndex > 0 && CmbFilterDept.SelectedValue != null)
                {
                    int selectedDeptId = Convert.ToInt32(CmbFilterDept.SelectedValue);
                    list = list.Where(x => x.DepartmentID == selectedDeptId).ToList();
                }

                if (CmbSort.SelectedIndex == 1) list = list.OrderBy(x => x.LastName).ToList();
                if (CmbSort.SelectedIndex == 2) list = list.OrderByDescending(x => x.LastName).ToList();
                if (CmbSort.SelectedIndex == 3) list = list.OrderByDescending(x => x.Age).ToList();

                EmployeeListView.ItemsSource = list;
            }
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void CmbFilterDept_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Manager")
            {
                MessageBox.Show("Добавление сотрудников доступно только менеджеру.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var editWindow = new EmployeeEditWindow();
            if (editWindow.ShowDialog() == true) UpdateList();
        }

        private void EmployeeListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role != "Manager" || EmployeeListView.SelectedItem == null) return;
            var row = (EmployeeRowView)EmployeeListView.SelectedItem;
            var editWindow = new EmployeeEditWindow(row.SourceEmployee);
            if (editWindow.ShowDialog() == true) UpdateList();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Manager")
            {
                MessageBox.Show("Удаление сотрудников доступно только менеджеру.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EmployeeListView.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите сотрудника в списке (кликните по строке).",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var row = (EmployeeRowView)EmployeeListView.SelectedItem;
            var emp = row.SourceEmployee;

            string fio = $"{emp.LastName} {emp.FirstName} {emp.Patronymic}".Trim();
            if (MessageBox.Show($"Удалить сотрудника «{fio}»?\n\nДействие необратимо.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new ProjectOrganizationDBEntities())
                {
                    bool isHead = db.Departments.Any(x => x.HeadID == emp.EmployeeID);
                    bool hasContracts = db.Contracts.Any(x => x.ResponsibleEmployeeID == emp.EmployeeID);
                    bool hasWorks = db.ProjectWorks.Any(x => x.ResponsibleEmployeeID == emp.EmployeeID);

                    if (isHead || hasContracts || hasWorks)
                    {
                        var reasons = new List<string>();
                        if (isHead) reasons.Add("является начальником отдела");
                        if (hasContracts) reasons.Add("указан ответственным по договору");
                        if (hasWorks) reasons.Add("указан ответственным по работе");

                        MessageBox.Show(
                            "Невозможно удалить сотрудника:\n• " + string.Join("\n• ", reasons),
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var empToDelete = db.Employees.FirstOrDefault(x => x.EmployeeID == emp.EmployeeID);
                    if (empToDelete == null)
                    {
                        MessageBox.Show("Сотрудник не найден в базе.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        UpdateList();
                        return;
                    }

                    db.Employees.Remove(empToDelete);
                    db.SaveChanges();
                }

                MessageBox.Show("Сотрудник удалён.", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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