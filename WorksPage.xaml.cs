using System;
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

        public WorksPage(string role)
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
            if (WorkListView == null) return;

            var db = new ProjectOrganizationDBEntities();
            var list = db.ProjectWorks.ToList();

            if (int.TryParse(TBoxSearch.Text, out int searchId))
                list = list.Where(x => x.ContractID == searchId).ToList();

            if (CmbSort.SelectedIndex == 1) list = list.OrderByDescending(x => x.StartDate).ToList();
            if (CmbSort.SelectedIndex == 2) list = list.OrderBy(x => x.ContractID).ToList();

            WorkListView.ItemsSource = list;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();

        private void OnlyNumbers_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]+");
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;
            var win = new WorkEditWindow();
            if (win.ShowDialog() == true) UpdateList();
        }

        private void WorkListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_role == "Employee" || WorkListView.SelectedItem == null) return;
            var work = (ProjectWorks)WorkListView.SelectedItem;
            var win = new WorkEditWindow(work);
            if (win.ShowDialog() == true) UpdateList();
        }

        // УДАЛЕНИЕ РАБОТЫ
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Employee" || _role == "Manager") return;

            if (WorkListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите работу для удаления!");
                return;
            }

            var work = (ProjectWorks)WorkListView.SelectedItem;

            try
            {
                if (MessageBox.Show($"Удалить работу №{work.WorkID}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                var db = new ProjectOrganizationDBEntities();
                var workToDelete = db.ProjectWorks.FirstOrDefault(x => x.WorkID == work.WorkID);
                if (workToDelete != null)
                {
                    db.ProjectWorks.Remove(workToDelete);
                    db.SaveChanges();
                    MessageBox.Show("Работа удалена.");
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