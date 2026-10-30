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

            var busyHeadIds = db.Departments
                .Where(d => d.HeadID != 0)
                .Select(d => d.HeadID)
                .ToList();
            var availableEmployees = allEmployees
                .Where(emp => !busyHeadIds.Contains(emp.EmployeeID) ||
                              (currentDepartmentId != null && db.Departments.FirstOrDefault(d => d.DepartmentID == currentDepartmentId).HeadID == emp.EmployeeID))
                .ToList();

            CmbHead.ItemsSource = availableEmployees;
            CmbHead.DisplayMemberPath = "LastName";
            CmbHead.SelectedValuePath = "EmployeeID";
        }

        // Этаж - только цифры
        private void TxtFloor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]+");
        }

        // Телефон - только цифры
        private void TxtPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]+");
        }

        // Автоматическая маска: 12-34
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
            if (string.IsNullOrWhiteSpace(TxtName.Text) ||
                string.IsNullOrWhiteSpace(TxtPhone.Text) ||
                CmbHead.SelectedValue == null)
            {
                MessageBox.Show("Заполните обязательные поля (Название, Телефон, Начальник)!");
                return;
            }

            if (TxtPhone.Text.Length != 5)
            {
                MessageBox.Show("Введите телефон полностью в формате: 12-34");
                return;
            }

            // Проверка этажа
            int? floor = null;
            if (!string.IsNullOrWhiteSpace(TxtFloor.Text))
            {
                int parsedFloor = int.Parse(TxtFloor.Text);
                if (parsedFloor < 0 || parsedFloor > 10)
                {
                    MessageBox.Show("Этаж должен быть от 0 до 10!");
                    return;
                }
                floor = parsedFloor;
            }

            try
            {
                var db = new ProjectOrganizationDBEntities();

                // 1. Проверка на дубликат телефона (через foreach, как делали в сотрудниках)
                var allDepartments = db.Departments.ToList();
                bool isPhoneExists = false;

                foreach (var dept in allDepartments)
                {
                    if (dept.Phone == TxtPhone.Text)
                    {
                        if (_editingDept == null || dept.DepartmentID != _editingDept.DepartmentID)
                        {
                            isPhoneExists = true;
                            break;
                        }
                    }
                }

                if (isPhoneExists)
                {
                    MessageBox.Show("Отдел с таким номером телефона уже существует! Введите другой.");
                    return;
                }

                // 2. Проверка на то, что начальник не занят другим отделом
                bool isHeadExists = false;
                foreach (var dept in allDepartments)
                {
                    if (dept.HeadID == (int)CmbHead.SelectedValue)
                    {
                        if (_editingDept == null || dept.DepartmentID != _editingDept.DepartmentID)
                        {
                            isHeadExists = true;
                            break;
                        }
                    }
                }

                if (isHeadExists)
                {
                    MessageBox.Show("Этот сотрудник уже является начальником другого отдела!");
                    return;
                }

                // 3. Сохранение
                if (_editingDept == null)
                {
                    var newDep = new Departments
                    {
                        DepartmentName = TxtName.Text,
                        Floor = floor,
                        Phone = TxtPhone.Text,
                        HeadID = (int)CmbHead.SelectedValue
                    };
                    db.Departments.Add(newDep);
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
                // Показываем внутреннюю ошибку
                string msg = ex.Message;
                if (ex.InnerException != null) msg += "\n" + ex.InnerException.Message;
                MessageBox.Show("Ошибка: " + msg);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}