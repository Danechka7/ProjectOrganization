using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectOrganization
{
    public partial class EmployeeEditWindow : Window
    {
        private Employees _editingEmployee = null;

        public EmployeeEditWindow()
        {
            InitializeComponent();
            TxtTitle.Text = "Добавление сотрудника";
            LoadData();
        }

        public EmployeeEditWindow(Employees emp) : this()
        {
            _editingEmployee = emp;
            TxtTitle.Text = "Редактирование сотрудника";

            TxtLastName.Text = emp.LastName;
            TxtFirstName.Text = emp.FirstName;
            TxtPatronymic.Text = emp.Patronymic;
            CmbPosition.Text = emp.Position;

            // Заполняем пол
            CmbGender.Text = emp.Gender;

            CmbDepartment.SelectedValue = emp.DepartmentID;
            DpBirthDate.SelectedDate = emp.BirthDate;

            // Адрес
            if (!string.IsNullOrWhiteSpace(emp.Address))
            {
                int lastSpace = emp.Address.LastIndexOf(' ');
                if (lastSpace > 0)
                {
                    TxtStreet.Text = emp.Address.Substring(0, lastSpace);
                    TxtHouse.Text = emp.Address.Substring(lastSpace + 1);
                }
                else
                {
                    TxtStreet.Text = emp.Address;
                }
            }
        }

        private void LoadData()
        {
            var db = new ProjectOrganizationDBEntities();
            CmbDepartment.ItemsSource = db.Departments.ToList();

            var positions = db.Employees.Select(x => x.Position).Distinct().ToList();
            if (positions.Count == 0)
            {
                positions.Add("Начальник отдела");
                positions.Add("Инженер");
                positions.Add("Техник");
            }
            CmbPosition.ItemsSource = positions;
        }

        // Ограничение только для цифр (для дома)
        private void OnlyNumbers_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]+");
        }

        private void DpBirthDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DpBirthDate.SelectedDate.HasValue)
            {
                DateTime minDate = DateTime.Today.AddYears(-18);
                if (DpBirthDate.SelectedDate.Value > minDate)
                {
                    MessageBox.Show("Сотрудник должен быть старше 18 лет!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DpBirthDate.SelectedDate = null;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Добавили проверку пола
            if (string.IsNullOrWhiteSpace(TxtLastName.Text) ||
                string.IsNullOrWhiteSpace(TxtFirstName.Text) ||
                string.IsNullOrWhiteSpace(CmbPosition.Text) ||
                string.IsNullOrWhiteSpace(CmbGender.Text) ||
                DpBirthDate.SelectedDate == null)
            {
                MessageBox.Show("Заполните обязательные поля (Фамилия, Имя, Пол, Должность, Дата рождения)!");
                return;
            }

            string fullAddress = $"{TxtStreet.Text.Trim()} {TxtHouse.Text.Trim()}".Trim();
            if (fullAddress == "") fullAddress = null;

            try
            {
                var db = new ProjectOrganizationDBEntities();

                if (_editingEmployee == null)
                {
                    var newEmp = new Employees
                    {
                        LastName = TxtLastName.Text,
                        FirstName = TxtFirstName.Text,
                        Patronymic = TxtPatronymic.Text,
                        Position = CmbPosition.Text,
                        Gender = CmbGender.Text, // СОХРАНЯЕМ ПОЛ
                        BirthDate = DpBirthDate.SelectedDate.Value,
                        DepartmentID = CmbDepartment.SelectedValue as int?,
                        Address = fullAddress
                    };
                    db.Employees.Add(newEmp);
                }
                else
                {
                    var emp = db.Employees.FirstOrDefault(x => x.EmployeeID == _editingEmployee.EmployeeID);
                    if (emp != null)
                    {
                        emp.LastName = TxtLastName.Text;
                        emp.FirstName = TxtFirstName.Text;
                        emp.Patronymic = TxtPatronymic.Text;
                        emp.Position = CmbPosition.Text;
                        emp.Gender = CmbGender.Text; // СОХРАНЯЕМ ПОЛ
                        emp.BirthDate = DpBirthDate.SelectedDate.Value;
                        emp.DepartmentID = CmbDepartment.SelectedValue as int?;
                        emp.Address = fullAddress;
                    }
                }

                db.SaveChanges();
                DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show("Ошибка сохранения: " + errorMsg);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}