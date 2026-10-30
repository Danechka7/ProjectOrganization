using System.Windows;

namespace ProjectOrganization
{
    public partial class QuickOrgWindow : Window
    {
        // Сюда мы положим созданную организацию
        public Organizations NewOrganization { get; private set; }

        public QuickOrgWindow()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Введите название организации!");
                return;
            }

            // Создаем объект в памяти (в базу пока не пишем!)
            NewOrganization = new Organizations
            {
                OrgName = TxtName.Text,
                ActivityType = TxtActivityType.Text,
                Country = TxtCountry.Text,
                City = TxtCity.Text,
                Address = TxtAddress.Text,
                DirectorName = TxtDirector.Text
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