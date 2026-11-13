using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace ProjectOrganization
{
    public partial class DepartmentEditWindow : Window
    {
        private Departments _editingDept = null;
        public DepartmentEditWindow()
        {
            InitializeComponent();
            TxtTitle.Text = "Добавление отдела";
            LoadEmployees(null);
        }
        public DepartmentEditWindow(Departments dep) : this()
        {
            _editingDept = dep;
            TxtTitle.Text = "Редактирование отдела";
            TxtName.Text = dep.DepartmentName;
            TxtFloor.Text = dep.Floor.ToString();
            TxtPhone.Text = dep.Phone;
            LoadEmployees(dep.DepartmentID);
            CmbHead.SelectedValue = dep.HeadID;
        }
        private void LoadEmployees(int? currentDepartmentId)
        {
            var db = new ProjectOrganizationDBEntities();
            var allEmployees = db.Employees.ToList();
            var busyHeadIds = db.Departments.Where(d => d.HeadID != 0).Select(d => d.HeadID).ToList();
            var available = allEmployees.Where(emp => !busyHeadIds.Contains(emp.EmployeeID) || (currentDepartmentId != null && db.Departments.FirstOrDefault(d => d.DepartmentID == currentDepartmentId).HeadID == emp.EmployeeID)).ToList();
            CmbHead.ItemsSource = available;
        }
        private void OnlyNumbers_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]+");
        }
        private void TxtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            string text = tb.Text.Replace("-", "");
            if (text.Length > 4) text = text.Substring(0, 4);
            if (text.Length > 2) text = text.Insert(2, "-");
            tb.Text = text;
            tb.CaretIndex = tb.Text.Length;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) || string.IsNullOrWhiteSpace(TxtPhone.Text) || CmbHead.SelectedValue == null)
            {
                MessageBox.Show("Заполните обязательные поля!");
                return;
            }
            if (TxtPhone.Text.Length != 5)
            {
                MessageBox.Show("Телефон должен быть в формате 12-34");
                return;
            }
            int? floor = null;
            if (!string.IsNullOrWhiteSpace(TxtFloor.Text))
            {
                int parsedFloor = int.Parse(TxtFloor.Text);
                if (parsedFloor < 0 || parsedFloor > 10)
                {
                    MessageBox.Show("Этаж от 0 до 10!");
                    return;
                }
                floor = parsedFloor;
            }
            try
            {
                var db = new ProjectOrganizationDBEntities();
                var allDepartments = db.Departments.ToList();
                if (allDepartments.Any(x => x.Phone == TxtPhone.Text && (_editingDept == null || x.DepartmentID != _editingDept.DepartmentID)))
                {
                    MessageBox.Show("Телефон уже существует!");
                    return;
                }
                if (allDepartments.Any(x => x.HeadID == (int)CmbHead.SelectedValue && (_editingDept == null || x.DepartmentID != _editingDept.DepartmentID)))
                {
                    MessageBox.Show("Этот сотрудник уже начальник другого отдела!");
                    return;
                }
                if (_editingDept == null)
                {
                    db.Departments.Add(new Departments { DepartmentName = TxtName.Text, Floor = floor, Phone = TxtPhone.Text, HeadID = (int)CmbHead.SelectedValue });
                }
                else
                {
                    var dep = db.Departments.FirstOrDefault(x => x.DepartmentID == _editingDept.DepartmentID);
                    if (dep != null)
                    {
                        dep.DepartmentName = TxtName.Text;
                        dep.Floor = floor;
                        dep.Phone = TxtPhone.Text;
                        dep.HeadID = (int)CmbHead.SelectedValue;
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