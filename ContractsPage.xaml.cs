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

            // Скрываем кнопки для Сотрудника и Менеджера
            if (_role == "Employee" || _role == "Manager")
            {
                BtnAdd.Visibility = Visibility.Collapsed;
                BtnDelete.Visibility = Visibility.Collapsed;
            }

            UpdateList();
        }

        private void UpdateList()
        {
            if (ContractListView == null) return;

            var db = new ProjectOrganizationDBEntities();
            var list = db.Contracts.ToList();

            // Поиск
            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                list = list.Where(x => x.ContractNumber.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            // Фильтр по минимальной сумме
            if (decimal.TryParse(TxtMinAmount.Text, out decimal minAmount))
                list = list.Where(x => x.ContractAmount >= minAmount).ToList();

            // Сортировка
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

        // ДОБАВЛЕНИЕ
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;
            var win = new ContractEditWindow();
            if (win.ShowDialog() == true) UpdateList();
        }

        // РЕДАКТИРОВАНИЕ (двойной клик)
        private void ContractListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role == "Employee" || ContractListView.SelectedItem == null) return;
            var con = (Contracts)ContractListView.SelectedItem;
            var win = new ContractEditWindow(con);
            if (win.ShowDialog() == true) UpdateList();
        }

        // УДАЛЕНИЕ ДОГОВОРА
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;

            if (ContractListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите договор для удаления!");
                return;
            }

            var con = (Contracts)ContractListView.SelectedItem;

            try
            {
                var db = new ProjectOrganizationDBEntities();

                // Проверка: есть ли работы по этому договору
                bool hasWorks = db.ProjectWorks.Any(x => x.ContractID == con.ContractID);
                if (hasWorks)
                {
                    MessageBox.Show("Невозможно удалить договор, так как по нему уже есть проектные работы! Сначала удалите работы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Подтверждение
                if (MessageBox.Show($"Вы уверены, что хотите удалить договор №{con.ContractNumber}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                // Удаление
                var conToDelete = db.Contracts.FirstOrDefault(x => x.ContractID == con.ContractID);
                if (conToDelete != null)
                {
                    db.Contracts.Remove(conToDelete);
                    db.SaveChanges();
                    MessageBox.Show("Договор успешно удален.");
                    UpdateList();
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show("Ошибка удаления: " + msg);
            }
        }
    }
}