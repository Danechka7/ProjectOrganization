using System;
using System.Linq;
using System.Windows;
namespace ProjectOrganization
{
    public partial class WorkEditWindow : Window
    {
        private ProjectWorks _editingWork = null;
        public WorkEditWindow()
        {
            InitializeComponent();
            TxtTitle.Text = "Добавление работы";
            LoadData();
        }
        public WorkEditWindow(ProjectWorks work) : this()
        {
            _editingWork = work;
            TxtTitle.Text = "Редактирование работы";
            CmbContract.SelectedValue = work.ContractID;
            CmbDepartment.SelectedValue = work.DepartmentID;
            CmbEmployee.SelectedValue = work.ResponsibleEmployeeID;
        }
        private void LoadData()
        {
            var db = new ProjectOrganizationDBEntities();
            CmbContract.ItemsSource = db.Contracts.ToList();
            CmbDepartment.ItemsSource = db.Departments.ToList();
            CmbEmployee.ItemsSource = db.Employees.ToList();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CmbContract.SelectedValue == null || CmbDepartment.SelectedValue == null || CmbEmployee.SelectedValue == null)
            {
                MessageBox.Show("Заполните все обязательные поля!");
                return;
            }
            try
            {
                var db = new ProjectOrganizationDBEntities();
                if (_editingWork == null)
                {
                    db.ProjectWorks.Add(new ProjectWorks
                    {
                        ContractID = (int)CmbContract.SelectedValue,
                        DepartmentID = (int)CmbDepartment.SelectedValue,
                        ResponsibleEmployeeID = (int)CmbEmployee.SelectedValue,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now
                    });
                }
                else
                {
                    var work = db.ProjectWorks.FirstOrDefault(x => x.WorkID == _editingWork.WorkID);
                    if (work != null)
                    {
                        work.ContractID = (int)CmbContract.SelectedValue;
                        work.DepartmentID = (int)CmbDepartment.SelectedValue;
                        work.ResponsibleEmployeeID = (int)CmbEmployee.SelectedValue;
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