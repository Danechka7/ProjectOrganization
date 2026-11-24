using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectOrganization
{
    public partial class ContractsPage : Page
    {
        private string _role;

        private class ContractRowView
        {
            public int ContractID { get; set; }
            public string ContractNumber { get; set; }
            public object ContractDate { get; set; }
            public decimal ContractAmount { get; set; }
            public int? ResponsibleEmployeeID { get; set; }
            public string EmployeeNameDisplay { get; set; }
            public string OrgNameDisplay { get; set; }
            public Contracts SourceContract { get; set; }
        }

        public ContractsPage(string role)
        {
            InitializeComponent();
            _role = role;

            // Добавлять могут ВСЕ (Manager + Employee)
            BtnAdd.Visibility = Visibility.Visible;

            // Удалять — только Manager
            if (_role != "Manager")
            {
                BtnDelete.Visibility = Visibility.Collapsed;
            }

            UpdateList();
        }

        private void UpdateList()
        {
            if (ContractListView == null) return;

            using (var db = new ProjectOrganizationDBEntities())
            {
                var orgsMap = db.Organizations.ToDictionary(o => o.OrganizationID, o => o.OrgName);
                var empsMap = db.Employees.ToDictionary(e => e.EmployeeID, e => e.LastName);
                var contracts = db.Contracts.ToList();

                List<ContractRowView> list = contracts.Select(c =>
                {
                    int? empId = c.ResponsibleEmployeeID;
                    int orgId = Convert.ToInt32(c.OrganizationID);
                    decimal amount = Convert.ToDecimal(c.ContractAmount);

                    return new ContractRowView
                    {
                        ContractID = c.ContractID,
                        ContractNumber = c.ContractNumber,
                        ContractDate = c.ContractDate,
                        ContractAmount = amount,
                        ResponsibleEmployeeID = empId,
                        EmployeeNameDisplay = (empId.HasValue && empsMap.ContainsKey(empId.Value))
                            ? empsMap[empId.Value]
                            : "—",
                        OrgNameDisplay = orgsMap.ContainsKey(orgId)
                            ? orgsMap[orgId]
                            : "—",
                        SourceContract = c
                    };
                }).ToList();

                if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                    list = list.Where(x => x.ContractNumber != null &&
                                           x.ContractNumber.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

                if (decimal.TryParse(TxtMinAmount.Text, out decimal minAmount))
                    list = list.Where(x => x.ContractAmount >= minAmount).ToList();

                if (CmbSort.SelectedIndex == 1) list = list.OrderByDescending(x => x.ContractDate?.ToString()).ToList();
                if (CmbSort.SelectedIndex == 2) list = list.OrderByDescending(x => x.ContractAmount).ToList();

                ContractListView.ItemsSource = list;
            }
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void TxtMinAmount_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();

        private void OnlyNumbers_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]+");
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Добавлять могут все — проверка роли не нужна
            var win = new ContractEditWindow();
            if (win.ShowDialog() == true) UpdateList();
        }

        private void ContractListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Редактирование — только Manager
            if (_role != "Manager" || ContractListView.SelectedItem == null) return;
            var row = (ContractRowView)ContractListView.SelectedItem;
            var win = new ContractEditWindow(row.SourceContract);
            if (win.ShowDialog() == true) UpdateList();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Manager")
            {
                MessageBox.Show("Удаление договоров доступно только менеджеру.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ContractListView.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите договор в списке (кликните по строке).",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var row = (ContractRowView)ContractListView.SelectedItem;
            var con = row.SourceContract;

            if (MessageBox.Show($"Удалить договор №{con.ContractNumber}?\n\nДействие необратимо.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new ProjectOrganizationDBEntities())
                {
                    bool hasWorks = db.ProjectWorks.Any(x => x.ContractID == con.ContractID);
                    if (hasWorks)
                    {
                        MessageBox.Show("Невозможно удалить! По договору есть работы.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var conToDelete = db.Contracts.FirstOrDefault(x => x.ContractID == con.ContractID);
                    if (conToDelete == null)
                    {
                        MessageBox.Show("Договор не найден в базе.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        UpdateList();
                        return;
                    }

                    db.Contracts.Remove(conToDelete);
                    db.SaveChanges();
                }

                MessageBox.Show("Договор удалён.", "Готово",
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