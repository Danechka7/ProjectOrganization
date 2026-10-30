using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectOrganization
{
    public partial class ContractEditWindow : Window
    {
        private Contracts _editingContract = null;
        private List<Organizations> _organizations;

        public ContractEditWindow()
        {
            InitializeComponent();
            TxtTitle.Text = "Добавление договора";
            DpContractDate.SelectedDate = DateTime.Today;
            LoadData();
            GenerateContractNumber();
        }

        public ContractEditWindow(Contracts con) : this()
        {
            _editingContract = con;
            TxtTitle.Text = "Редактирование договора";

            TxtNumber.Text = con.ContractNumber;
            TxtAmount.Text = con.ContractAmount.ToString();
            CmbOrganization.SelectedValue = con.OrganizationID;
            CmbEmployee.SelectedValue = con.ResponsibleEmployeeID;

            if (con.ContractDate != null)
            {
                DpContractDate.SelectedDate = con.ContractDate;

                // Блокируем изменение даты, только если прошло больше 7 дней
                int daysPassed = (DateTime.Today - con.ContractDate.Date).Days;
                if (daysPassed > 7)
                {
                    DpContractDate.IsEnabled = false;
                    DpContractDate.ToolTip = "Дата не может быть изменена, так как договору больше 7 дней.";
                }
            }
        }

        private void GenerateContractNumber()
        {
            if (_editingContract != null) return;

            try
            {
                var db = new ProjectOrganizationDBEntities();

                // Берем ТЕКУЩИЙ системный год
                int currentYear = DateTime.Today.Year;

                // Ищем договор, в номере которого есть текущий год (например, "2026")
                // Мы ищем по строке, а не по дате, чтобы не зависеть от тестовых данных
                string yearPattern = currentYear.ToString();

                var lastContract = db.Contracts
                    .Where(x => x.ContractNumber.Contains(yearPattern))
                    .OrderByDescending(x => x.ContractID)
                    .FirstOrDefault();

                string newNumber = $"Д-{currentYear}-001";

                if (lastContract != null)
                {
                    string lastNum = lastContract.ContractNumber;
                    int lastDash = lastNum.LastIndexOf('-');

                    if (lastDash > 0)
                    {
                        string prefix = lastNum.Substring(0, lastDash + 1);
                        string numberPart = lastNum.Substring(lastDash + 1);

                        if (int.TryParse(numberPart, out int num))
                        {
                            newNumber = $"Д-{currentYear}-" + (num + 1).ToString("D3");
                        }
                    }
                }

                TxtNumber.Text = newNumber;
            }
            catch
            {
                TxtNumber.Text = $"Д-{DateTime.Today.Year}-001";
            }
        }

        private void LoadData()
        {
            var db = new ProjectOrganizationDBEntities();

            _organizations = db.Organizations.ToList();
            CmbOrganization.ItemsSource = _organizations;
            CmbEmployee.ItemsSource = db.Employees.ToList();
        }

        private void OnlyNumbers_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            string pattern = (tb.Name == "TxtAmount") ? @"[0-9.,]+" : "[0-9]+";
            e.Handled = !Regex.IsMatch(e.Text, pattern);
        }

        private void BtnAddNewOrg_Click(object sender, RoutedEventArgs e)
        {
            var quickWindow = new QuickOrgWindow();
            if (quickWindow.ShowDialog() == true)
            {
                _organizations.Add(quickWindow.NewOrganization);
                CmbOrganization.ItemsSource = null;
                CmbOrganization.ItemsSource = _organizations;
                CmbOrganization.SelectedItem = quickWindow.NewOrganization;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNumber.Text) ||
                CmbOrganization.SelectedItem == null ||
                CmbEmployee.SelectedValue == null)
            {
                MessageBox.Show("Заполните обязательные поля (Номер, Организация, Сотрудник)!");
                return;
            }

            if (DpContractDate.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату договора!");
                return;
            }

            // --- ПРОВЕРКА ДАТЫ ---
            DateTime selectedDate = DpContractDate.SelectedDate.Value.Date;
            DateTime today = DateTime.Today;

            if (_editingContract == null)
            {
                // Для нового договора дата должна быть строго сегодняшней
                if (selectedDate != today)
                {
                    MessageBox.Show("Для нового договора дата должна быть строго сегодняшней (" + today.ToString("dd.MM.yyyy") + ")!");
                    return;
                }
            }
            else
            {
                // Для редактирования проверяем, сколько прошло времени с даты старого договора
                if (_editingContract.ContractDate != null)
                {
                    DateTime oldDate = _editingContract.ContractDate.Date;
                    int daysPassed = (today - oldDate).Days;

                    // Если прошло больше 7 дней и дату пытаются изменить
                    if (daysPassed > 7 && selectedDate != oldDate)
                    {
                        MessageBox.Show($"Договор был создан {oldDate.ToString("dd.MM.yyyy")} (более 7 дней назад). Изменять дату запрещено!");
                        return;
                    }

                    // Если прошло меньше 7 дней - разрешаем менять
                }
            }

            try
            {
                var db = new ProjectOrganizationDBEntities();

                var selectedOrg = (Organizations)CmbOrganization.SelectedItem;
                Organizations orgForContract;

                if (selectedOrg.OrganizationID == 0)
                {
                    orgForContract = new Organizations
                    {
                        OrgName = selectedOrg.OrgName,
                        ActivityType = selectedOrg.ActivityType,
                        Country = selectedOrg.Country,
                        City = selectedOrg.City,
                        Address = selectedOrg.Address,
                        DirectorName = selectedOrg.DirectorName
                    };
                    db.Organizations.Add(orgForContract);
                    db.SaveChanges();
                }
                else
                {
                    orgForContract = db.Organizations.FirstOrDefault(x => x.OrganizationID == selectedOrg.OrganizationID);
                }

                if (_editingContract == null)
                {
                    var newCon = new Contracts
                    {
                        ContractNumber = TxtNumber.Text,
                        ContractDate = selectedDate,
                        ContractAmount = string.IsNullOrWhiteSpace(TxtAmount.Text) ? 0 : decimal.Parse(TxtAmount.Text.Replace('.', ',')),
                        OrganizationID = orgForContract.OrganizationID,
                        ResponsibleEmployeeID = (int)CmbEmployee.SelectedValue
                    };
                    db.Contracts.Add(newCon);
                }
                else
                {
                    var con = db.Contracts.FirstOrDefault(x => x.ContractID == _editingContract.ContractID);
                    if (con != null)
                    {
                        con.ContractNumber = TxtNumber.Text;
                        con.ContractDate = selectedDate; // Ставим ту, которую выбрал пользователь (или старую)
                        con.ContractAmount = string.IsNullOrWhiteSpace(TxtAmount.Text) ? 0 : decimal.Parse(TxtAmount.Text.Replace('.', ','));
                        con.OrganizationID = orgForContract.OrganizationID;
                        con.ResponsibleEmployeeID = (int)CmbEmployee.SelectedValue;
                    }
                }

                db.SaveChanges();
                DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show("Ошибка сохранения: " + msg);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}