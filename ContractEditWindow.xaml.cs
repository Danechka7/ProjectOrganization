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
            if (con.ContractDate != null) DpContractDate.SelectedDate = con.ContractDate;
        }
        private void GenerateContractNumber()
        {
            if (_editingContract != null) return;

            try
            {
                var db = new ProjectOrganizationDBEntities();
                int currentYear = DateTime.Today.Year;
                var lastContract = db.Contracts.OrderByDescending(x => x.ContractID).FirstOrDefault();

                int nextNumber = 1;

                if (lastContract != null && !string.IsNullOrWhiteSpace(lastContract.ContractNumber))
                {
                    string lastNum = lastContract.ContractNumber;
                    int lastDash = lastNum.LastIndexOf('-');

                    if (lastDash > 0 && lastDash < lastNum.Length - 1)
                    {
                        string numberPart = lastNum.Substring(lastDash + 1);
                        if (int.TryParse(numberPart, out int num))
                        {
                            nextNumber = num + 1;
                        }
                    }
                }
                TxtNumber.Text = $"Д-{currentYear}-{nextNumber:D3}";
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
            if (string.IsNullOrWhiteSpace(TxtNumber.Text) || string.IsNullOrWhiteSpace(TxtAmount.Text) || CmbOrganization.SelectedItem == null || CmbEmployee.SelectedValue == null || DpContractDate.SelectedDate == null)
            {
                MessageBox.Show("Заполните все обязательные поля!");
                return;
            }
            DateTime selectedDate = DpContractDate.SelectedDate.Value.Date;
            if (_editingContract == null)
            {
                if (selectedDate != DateTime.Today)
                {
                    MessageBox.Show("Для нового договора дата должна быть сегодняшней!");
                    return;
                }
            }
            else if (_editingContract.ContractDate != null)
            {
                int daysPassed = (DateTime.Today - _editingContract.ContractDate.Date).Days;
                if (daysPassed > 7 && selectedDate != _editingContract.ContractDate.Date)
                {
                    MessageBox.Show("Договор создан более 7 дней назад. Менять дату запрещено!");
                    return;
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
                decimal amount = decimal.Parse(TxtAmount.Text.Replace('.', ','));
                if (_editingContract == null)
                {
                    db.Contracts.Add(new Contracts
                    {
                        ContractNumber = TxtNumber.Text,
                        ContractDate = selectedDate,
                        ContractAmount = amount,
                        OrganizationID = orgForContract.OrganizationID,
                        ResponsibleEmployeeID = (int)CmbEmployee.SelectedValue
                    });
                }
                else
                {
                    var con = db.Contracts.FirstOrDefault(x => x.ContractID == _editingContract.ContractID);
                    if (con != null)
                    {
                        con.ContractNumber = TxtNumber.Text;
                        con.ContractDate = selectedDate;
                        con.ContractAmount = amount;
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
                MessageBox.Show("Ошибка: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}