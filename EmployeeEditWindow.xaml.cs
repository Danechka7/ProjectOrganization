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
        private static readonly Regex AddressRegex =
            new Regex(@"^[\p{L}\p{N}\s\.,\-]+$", RegexOptions.Compiled);

        private static readonly Regex HouseRegex =
            new Regex(@"^[\p{L}\p{N}\s/]+$", RegexOptions.Compiled);
        private static readonly Regex StreetHasLetterRegex =
           new Regex(@"\p{L}", RegexOptions.Compiled);

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

            LoginPanel.Visibility = Visibility.Collapsed;
            PasswordPanel.Visibility = Visibility.Collapsed;

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

        private void Address_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !AddressRegex.IsMatch(e.Text);
        }

        private void House_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !HouseRegex.IsMatch(e.Text);
        }

        private void Address_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!AddressRegex.IsMatch(text)) e.CancelCommand();
            }
            else e.CancelCommand();
        }

        private void House_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!HouseRegex.IsMatch(text)) e.CancelCommand();
            }
            else e.CancelCommand();
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
            string street = TxtStreet.Text.Trim();
            string house = TxtHouse.Text.Trim();
            if (string.IsNullOrWhiteSpace(TxtLastName.Text) ||
                string.IsNullOrWhiteSpace(TxtFirstName.Text) ||
                string.IsNullOrWhiteSpace(CmbPosition.Text) ||
                string.IsNullOrWhiteSpace(CmbGender.Text) ||
                string.IsNullOrWhiteSpace(street) ||
                string.IsNullOrWhiteSpace(house) ||
                DpBirthDate.SelectedDate == null)
            {
                MessageBox.Show("Заполните все обязательные поля (включая адрес и дом)!");
                return;
            }
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Text.Trim();

            if (_editingEmployee == null)
            {
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Для нового сотрудника обязательно задать логин и пароль!");
                    return;
                }
            }

            if (!AddressRegex.IsMatch(street))
            {
                MessageBox.Show("В поле «Улица» допускаются только буквы, цифры, пробел, точка, запятая и дефис!");
                TxtStreet.Focus();
                return;
            }
            if (!StreetHasLetterRegex.IsMatch(street))
            {
                MessageBox.Show("Название улицы должно содержать хотя бы одну букву (не может состоять только из цифр)!");
                TxtStreet.Focus();
                return;
            }

            if (!HouseRegex.IsMatch(house))
            {
                MessageBox.Show("В поле «Дом» допускаются только буквы, цифры, пробел и символ «/»!");
                TxtHouse.Focus();
                return;
            }

            string fullAddress = $"{street} {house}";

            try
            {
                using (var db = new ProjectOrganizationDBEntities())
                {
                    if (_editingEmployee == null)
                    {
                        bool loginExists = db.Employees.Any(x =>
                            x.Login != null && x.Login.ToLower() == login.ToLower());
                        if (loginExists)
                        {
                            MessageBox.Show("Такой логин уже используется! Введите уникальный.");
                            return;
                        }

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