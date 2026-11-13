using System;
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
        public ContractsPage(string role)
        {
            InitializeComponent();
            _role = role;
            BtnAdd.Visibility = Visibility.Visible;
            if (_role == "Employee" || _role == "Manager")
            {
                BtnDelete.Visibility = Visibility.Collapsed;
            }
            UpdateList();
        }
        private void UpdateList()
        {
            if (ContractListView == null) return;
            var db = new ProjectOrganizationDBEntities();
            var list = db.Contracts.ToList();
            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                list = list.Where(x => x.ContractNumber.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();
            if (decimal.TryParse(TxtMinAmount.Text, out decimal minAmount))
                list = list.Where(x => x.ContractAmount >= minAmount).ToList();
            if (CmbSort.SelectedIndex == 1) list = list.OrderByDescending(x => x.ContractDate).ToList();
            if (CmbSort.SelectedIndex == 2) list = list.OrderByDescending(x => x.ContractAmount).ToList();
            ContractListView.ItemsSource = list;
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
            var win = new ContractEditWindow();
            if (win.ShowDialog() == true) UpdateList();
        }
        private void ContractListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ContractListView.SelectedItem == null) return;
            var con = (Contracts)ContractListView.SelectedItem;
            var win = new ContractEditWindow(con);
            if (win.ShowDialog() == true) UpdateList();
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;
            if (ContractListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите договор!");
                return;
            }
            var con = (Contracts)ContractListView.SelectedItem;
            try
            {
                var db = new ProjectOrganizationDBEntities();
                bool hasWorks = db.ProjectWorks.Any(x => x.ContractID == con.ContractID);
                if (hasWorks)
                {
                    MessageBox.Show("Невозможно удалить! По договору есть работы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (MessageBox.Show($"Удалить договор №{con.ContractNumber}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
                var conToDelete = db.Contracts.FirstOrDefault(x => x.ContractID == con.ContractID);
                if (conToDelete != null)
                {
                    db.Contracts.Remove(conToDelete);
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