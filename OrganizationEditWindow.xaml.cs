using System;
using System.Linq;
using System.Windows;

namespace ProjectOrganization
{
    public partial class OrganizationEditWindow : Window
    {
        private Organizations _editingOrg = null;

        public OrganizationEditWindow()
        {
            InitializeComponent();
            TxtTitle.Text = "Добавление организации";
        }

        public OrganizationEditWindow(Organizations org) : this()
        {
            _editingOrg = org;
            TxtTitle.Text = "Редактирование организации";

            TxtOrgName.Text = org.OrgName;
            TxtActivityType.Text = org.ActivityType;
            TxtCountry.Text = org.Country;
            TxtCity.Text = org.City;
            TxtAddress.Text = org.Address;
            TxtDirectorName.Text = org.DirectorName;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(TxtOrgName.Text))
            {
                MessageBox.Show("Введите название организации!");
                return;
            }

            try
            {
                var db = new ProjectOrganizationDBEntities();

                // 2. Проверка на дубликат названия
                var allOrgs = db.Organizations.ToList();
                bool isNameExists = false;

                foreach (var org in allOrgs)
                {
                    if (org.OrgName.Trim().ToLower() == TxtOrgName.Text.Trim().ToLower())
                    {
                        // Если редактируем, но нашли другую организацию с таким же именем
                        if (_editingOrg == null || org.OrganizationID != _editingOrg.OrganizationID)
                        {
                            isNameExists = true;
                            break;
                        }
                    }
                }

                if (isNameExists)
                {
                    MessageBox.Show("Организация с таким названием уже существует!");
                    return;
                }

                // 3. Сохранение
                if (_editingOrg == null)
                {
                    var newOrg = new Organizations
                    {
                        OrgName = TxtOrgName.Text.Trim(),
                        ActivityType = TxtActivityType.Text,
                        Country = TxtCountry.Text,
                        City = TxtCity.Text,
                        Address = TxtAddress.Text,
                        DirectorName = TxtDirectorName.Text
                    };
                    db.Organizations.Add(newOrg);
                }
                else
                {
                    var org = db.Organizations.FirstOrDefault(x => x.OrganizationID == _editingOrg.OrganizationID);
                    if (org != null)
                    {
                        org.OrgName = TxtOrgName.Text.Trim();
                        org.ActivityType = TxtActivityType.Text;
                        org.Country = TxtCountry.Text;
                        org.City = TxtCity.Text;
                        org.Address = TxtAddress.Text;
                        org.DirectorName = TxtDirectorName.Text;
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