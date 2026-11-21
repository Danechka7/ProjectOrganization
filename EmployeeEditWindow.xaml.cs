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
            CmbGender.Text = emp.Gender;
            CmbDepartment.SelectedValue = emp.DepartmentID;
            DpBirthDate.SelectedDate = emp.BirthDate;
            TxtLogin.Text = emp.Login;
            TxtPassword.Text = emp.Password;

            if (!string.IsNullOrWhiteSpace(emp.Address))
            {
                int lastSpace = emp.Address.LastIndexOf(' ');
                if (lastSpace > 0)
                {
                    TxtStreet.Text = emp.Address.Substring(0, lastSpace);
                    TxtHouse.Text = emp.Address.Substring(lastSpace + 1);
                }
                else TxtStreet.Text = emp.Address;
            }
        }

        private void LoadData()
        {
            using (var db = new ProjectOrganizationDBEntities())
            {
                CmbDepartment.ItemsSource = db.Departments.ToList();
                var positions = db.Employees.Select(x => x.Position).Where(x => x != null).Distinct().ToList();
                if (positions.Count == 0)
                {
                    positions.Add("Начальник отдела");
                    positions.Add("Инженер");
                    positions.Add("Техник");
                }
                CmbPosition.ItemsSource = positions;
            }
        }

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
                    MessageBox.Show("Сотрудник должен быть старше 18 лет!");
                    DpBirthDate.SelectedDate = null;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(TxtLastName.Text) ||
                string.IsNullOrWhiteSpace(TxtFirstName.Text) ||
                string.IsNullOrWhiteSpace(CmbPosition.Text) ||
                string.IsNullOrWhiteSpace(CmbGender.Text) ||
                DpBirthDate.SelectedDate == null ||
                string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните обязательные поля (включая логин и пароль)!");
                return;
            }

            string fullAddress = $"{TxtStreet.Text.Trim()} {TxtHouse.Text.Trim()}".Trim();
            if (fullAddress == "") fullAddress = null;

            try
            {
                using (var db = new ProjectOrganizationDBEntities())
                {
                    int? editingId = _editingEmployee?.EmployeeID;
                    bool loginExists = db.Employees.Any(x =>
                        x.Login != null &&
                        x.Login.ToLower() == login.ToLower() &&
                        (!editingId.HasValue || x.EmployeeID != editingId.Value));

                    if (loginExists)
                    {
                        MessageBox.Show("Такой логин уже используется другим сотрудником! Введите уникальный логин.");
                        return;
                    }

                    if (_editingEmployee == null)
                    {
                        db.Employees.Add(new Employees
                        {
                            LastName = TxtLastName.Text.Trim(),
                            FirstName = TxtFirstName.Text.Trim(),
                            Patronymic = string.IsNullOrWhiteSpace(TxtPatronymic.Text) ? null : TxtPatronymic.Text.Trim(),
                            Position = CmbPosition.Text.Trim(),
                            Gender = CmbGender.Text.Trim(),
                            BirthDate = DpBirthDate.SelectedDate.Value,
                            DepartmentID = CmbDepartment.SelectedValue as int?,
                            Address = fullAddress,
                            Login = login,
                            Password = password
                        });
                    }
                    else
                    {
                        var emp = db.Employees.FirstOrDefault(x => x.EmployeeID == _editingEmployee.EmployeeID);
                        if (emp != null)
                        {
                            emp.LastName = TxtLastName.Text.Trim();
                            emp.FirstName = TxtFirstName.Text.Trim();
                            emp.Patronymic = string.IsNullOrWhiteSpace(TxtPatronymic.Text) ? null : TxtPatronymic.Text.Trim();
                            emp.Position = CmbPosition.Text.Trim();
                            emp.Gender = CmbGender.Text.Trim();
                            emp.BirthDate = DpBirthDate.SelectedDate.Value;
                            emp.DepartmentID = CmbDepartment.SelectedValue as int?;
                            emp.Address = fullAddress;
                            emp.Login = login;
                            emp.Password = password;
                        }
                    }

                    db.SaveChanges();
                    DialogResult = true;
                    this.Close();
                }
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