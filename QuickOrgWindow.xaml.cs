using System;
using System.Windows;
namespace ProjectOrganization
{
    public partial class QuickOrgWindow : Window
    {
        public Organizations NewOrganization { get; private set; }
        public QuickOrgWindow()
        {
            InitializeComponent();
        }
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) || string.IsNullOrWhiteSpace(TxtActivityType.Text) || string.IsNullOrWhiteSpace(TxtCountry.Text) || string.IsNullOrWhiteSpace(TxtCity.Text) || string.IsNullOrWhiteSpace(TxtAddress.Text) || string.IsNullOrWhiteSpace(TxtDirector.Text))
            {
                MessageBox.Show("Заполните ВСЕ поля организации!");
                return;
            }
            NewOrganization = new Organizations
            {
                OrgName = TxtName.Text.Trim(),
                ActivityType = TxtActivityType.Text.Trim(),
                Country = TxtCountry.Text.Trim(),
                City = TxtCity.Text.Trim(),
                Address = TxtAddress.Text.Trim(),
                DirectorName = TxtDirector.Text.Trim()
            };
            DialogResult = true;
            this.Close();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}