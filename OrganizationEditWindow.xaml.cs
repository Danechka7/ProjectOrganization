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
            if (string.IsNullOrWhiteSpace(TxtOrgName.Text) || string.IsNullOrWhiteSpace(TxtActivityType.Text) || string.IsNullOrWhiteSpace(TxtCountry.Text) || string.IsNullOrWhiteSpace(TxtCity.Text) || string.IsNullOrWhiteSpace(TxtAddress.Text) || string.IsNullOrWhiteSpace(TxtDirectorName.Text))
            {
                MessageBox.Show("Заполните ВСЕ поля организации!");
                return;
            }
            try
            {
                var db = new ProjectOrganizationDBEntities();
                var allOrgs = db.Organizations.ToList();
                bool isNameExists = false;
                foreach (var org in allOrgs)
                {
                    if (org.OrgName.Trim().ToLower() == TxtOrgName.Text.Trim().ToLower())
                    {
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
                if (_editingOrg == null)
                {
                    db.Organizations.Add(new Organizations
                    {
                        OrgName = TxtOrgName.Text.Trim(),
                        ActivityType = TxtActivityType.Text.Trim(),
                        Country = TxtCountry.Text.Trim(),
                        City = TxtCity.Text.Trim(),
                        Address = TxtAddress.Text.Trim(),
                        DirectorName = TxtDirectorName.Text.Trim()
                    });
                }
                else
                {
                    var org = db.Organizations.FirstOrDefault(x => x.OrganizationID == _editingOrg.OrganizationID);
                    if (org != null)
                    {
                        org.OrgName = TxtOrgName.Text.Trim();
                        org.ActivityType = TxtActivityType.Text.Trim();
                        org.Country = TxtCountry.Text.Trim();
                        org.City = TxtCity.Text.Trim();
                        org.Address = TxtAddress.Text.Trim();
                        org.DirectorName = TxtDirectorName.Text.Trim();
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