using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectOrganization
{
    public partial class WorksPage : Page
    {
        private string _role;

        private class WorkRowView
        {
            public int WorkID { get; set; }
            public int ContractID { get; set; }
            public string ContractNumberDisplay { get; set; }
            public object StartDate { get; set; }
            public string DepartmentNameDisplay { get; set; }
            public string EmployeeNameDisplay { get; set; }
            public string WorkDescription { get; set; }
            public ProjectWorks SourceWork { get; set; }
        }

        public WorksPage(string role)
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
            if (WorkListView == null) return;

            using (var db = new ProjectOrganizationDBEntities())
            {
                var contractsMap = db.Contracts.ToDictionary(c => c.ContractID, c => c.ContractNumber);
                var departmentsMap = db.Departments.ToDictionary(d => d.DepartmentID, d => d.DepartmentName);
                var employeesMap = db.Employees.ToDictionary(e => e.EmployeeID, e => e.LastName);
                var works = db.ProjectWorks.ToList();

                List<WorkRowView> list = works.Select(w =>
                {
                    int contractId = Convert.ToInt32(w.ContractID);
                    int deptId = Convert.ToInt32(w.DepartmentID);
                    int? empId = w.ResponsibleEmployeeID;

                    return new WorkRowView
                    {
                        WorkID = w.WorkID,
                        ContractID = contractId,
                        ContractNumberDisplay = contractsMap.ContainsKey(contractId)
                            ? contractsMap[contractId]
                            : "—",
                        StartDate = w.StartDate,
                        DepartmentNameDisplay = departmentsMap.ContainsKey(deptId)
                            ? departmentsMap[deptId]
                            : "—",
                        EmployeeNameDisplay = (empId.HasValue && employeesMap.ContainsKey(empId.Value))
                            ? employeesMap[empId.Value]
                            : "—",
                        WorkDescription = w.WorkDescription,
                        SourceWork = w
                    };
                }).ToList();

                if (int.TryParse(TBoxSearch.Text, out int searchId))
                    list = list.Where(x => x.ContractID == searchId).ToList();

                if (CmbSort.SelectedIndex == 1) list = list.OrderByDescending(x => x.StartDate?.ToString()).ToList();
                if (CmbSort.SelectedIndex == 2) list = list.OrderBy(x => x.ContractID).ToList();

                WorkListView.ItemsSource = list;
            }
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();

        private void OnlyNumbers_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]+");
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Manager")
            {
                MessageBox.Show("Добавление работ доступно только менеджеру.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var win = new WorkEditWindow();
            if (win.ShowDialog() == true) UpdateList();
        }

        private void WorkListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role != "Manager" || WorkListView.SelectedItem == null) return;
            var row = (WorkRowView)WorkListView.SelectedItem;
            var win = new WorkEditWindow(row.SourceWork);
            if (win.ShowDialog() == true) UpdateList();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Manager")
            {
                MessageBox.Show("Удаление работ доступно только менеджеру.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (WorkListView.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите работу в списке (кликните по строке).",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var row = (WorkRowView)WorkListView.SelectedItem;
            var work = row.SourceWork;

            if (MessageBox.Show($"Удалить работу №{work.WorkID}?\n\nДействие необратимо.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new ProjectOrganizationDBEntities())
                {
                    var workToDelete = db.ProjectWorks.FirstOrDefault(x => x.WorkID == work.WorkID);
                    if (workToDelete == null)
                    {
                        MessageBox.Show("Работа не найдена в базе.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        UpdateList();
                        return;
                    }

                    db.ProjectWorks.Remove(workToDelete);
                    db.SaveChanges();
                }

                MessageBox.Show("Работа удалена.", "Готово",
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