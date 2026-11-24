using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectOrganization
{
    public partial class DepartmentsPage : Page
    {
        private string _role;

        private class DepartmentRowView
        {
            public int DepartmentID { get; set; }
            public string DepartmentName { get; set; }
            public object Floor { get; set; }
            public string Phone { get; set; }
            public string HeadNameDisplay { get; set; }
            public Departments SourceDepartment { get; set; }
        }

        public DepartmentsPage(string role)
        {
            InitializeComponent();
            _role = role;

            // Manager: полный доступ. Employee: только просмотр.
            if (_role != "Manager")
            {
                BtnAdd.Visibility = Visibility.Collapsed;
                BtnDelete.Visibility = Visibility.Collapsed;
            }

            UpdateList();
        }

        private void UpdateList()
        {
            if (DepartmentListView == null) return;

            using (var db = new ProjectOrganizationDBEntities())
            {
                var employeesMap = db.Employees.ToDictionary(e => e.EmployeeID, e => e.LastName);
                var departments = db.Departments.ToList();

                List<DepartmentRowView> list = departments.Select(dep =>
                {
                    int? headId = dep.HeadID;
                    string headName = (headId.HasValue && employeesMap.ContainsKey(headId.Value))
                        ? employeesMap[headId.Value]
                        : "—";

                    return new DepartmentRowView
                    {
                        DepartmentID = dep.DepartmentID,
                        DepartmentName = dep.DepartmentName,
                        Floor = dep.Floor,
                        Phone = dep.Phone,
                        HeadNameDisplay = headName,
                        SourceDepartment = dep
                    };
                }).ToList();

                if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                    list = list.Where(x => x.DepartmentName != null &&
                                           x.DepartmentName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

                if (CmbSort.SelectedIndex == 1) list = list.OrderBy(x => x.DepartmentName).ToList();
                if (CmbSort.SelectedIndex == 2)
                    list = list.OrderBy(x => x.Floor == null ? int.MaxValue : Convert.ToInt32(x.Floor)).ToList();

                DepartmentListView.ItemsSource = list;
            }
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Manager")
            {
                MessageBox.Show("Добавление отделов доступно только менеджеру.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var win = new DepartmentEditWindow();
            if (win.ShowDialog() == true) UpdateList();
        }

        private void DepartmentListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role != "Manager" || DepartmentListView.SelectedItem == null) return;
            var row = (DepartmentRowView)DepartmentListView.SelectedItem;
            var win = new DepartmentEditWindow(row.SourceDepartment);
            if (win.ShowDialog() == true) UpdateList();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Manager")
            {
                MessageBox.Show("Удаление отделов доступно только менеджеру.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DepartmentListView.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите отдел в списке (кликните по строке).",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var row = (DepartmentRowView)DepartmentListView.SelectedItem;
            var dep = row.SourceDepartment;

            if (MessageBox.Show($"Удалить отдел «{dep.DepartmentName}»?\n\nДействие необратимо.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new ProjectOrganizationDBEntities())
                {
                    bool hasEmployees = db.Employees.Any(x => x.DepartmentID == dep.DepartmentID);
                    bool hasWorks = db.ProjectWorks.Any(x => x.DepartmentID == dep.DepartmentID);

                    if (hasEmployees || hasWorks)
                    {
                        var reasons = new List<string>();
                        if (hasEmployees) reasons.Add("в отделе есть сотрудники");
                        if (hasWorks) reasons.Add("в отделе есть работы");

                        MessageBox.Show(
                            "Невозможно удалить отдел:\n• " + string.Join("\n• ", reasons),
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var depToDelete = db.Departments.FirstOrDefault(x => x.DepartmentID == dep.DepartmentID);
                    if (depToDelete == null)
                    {
                        MessageBox.Show("Отдел не найден в базе.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        UpdateList();
                        return;
                    }

                    db.Departments.Remove(depToDelete);
                    db.SaveChanges();
                }

                MessageBox.Show("Отдел удалён.", "Готово",
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