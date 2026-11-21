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
            if (_role == "Employee") return;
            var win = new WorkEditWindow();
            if (win.ShowDialog() == true) UpdateList();
        }

        private void WorkListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role == "Employee" || WorkListView.SelectedItem == null) return;
            var row = (WorkRowView)WorkListView.SelectedItem;
            var win = new WorkEditWindow(row.SourceWork);
            if (win.ShowDialog() == true) UpdateList();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;
            if (WorkListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите работу!");
                return;
            }
            var row = (WorkRowView)WorkListView.SelectedItem;
            var work = row.SourceWork;
            try
            {
                if (MessageBox.Show($"Удалить работу №{work.WorkID}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
                using (var db = new ProjectOrganizationDBEntities())
                {
                    var workToDelete = db.ProjectWorks.FirstOrDefault(x => x.WorkID == work.WorkID);
                    if (workToDelete != null)
                    {
                        db.ProjectWorks.Remove(workToDelete);
                        db.SaveChanges();
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