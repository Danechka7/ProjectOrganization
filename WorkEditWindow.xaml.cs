using System;
using System.Linq;
using System.Windows;

namespace ProjectOrganization
{
    public partial class WorkEditWindow : Window
    {
        private ProjectWorks _editingWork = null;
        private int? _preselectedContractId = null;

        public WorkEditWindow()
        {
            InitializeComponent();
            TxtTitle.Text = "Добавление работы";
            LoadData();
        }

        // Вызов из ContractEditWindow
        public WorkEditWindow(int contractId) : this()
        {
            _preselectedContractId = contractId;
            CmbContract.SelectedValue = contractId;
            CmbContract.IsEnabled = false; // Блокируем смену договора
        }

        public WorkEditWindow(ProjectWorks work) : this()
        {
            _editingWork = work;
            TxtTitle.Text = "Редактирование работы";
            CmbContract.SelectedValue = work.ContractID;
            CmbDepartment.SelectedValue = work.DepartmentID;
            CmbEmployee.SelectedValue = work.ResponsibleEmployeeID;
            TxtDescription.Text = work.WorkDescription;

            if (work.StartDate != null) DpStartDate.SelectedDate = work.StartDate;
            if (work.EndDate != null) DpEndDate.SelectedDate = work.EndDate;
        }

        private void LoadData()
        {
            using (var db = new ProjectOrganizationDBEntities())
            {
                CmbContract.ItemsSource = db.Contracts.ToList();
                CmbDepartment.ItemsSource = db.Departments.ToList();
                CmbEmployee.ItemsSource = db.Employees.ToList();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CmbContract.SelectedValue == null ||
                CmbDepartment.SelectedValue == null ||
                CmbEmployee.SelectedValue == null ||
                string.IsNullOrWhiteSpace(TxtDescription.Text) ||
                DpStartDate.SelectedDate == null ||
                DpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Заполните все обязательные поля, даты начала и окончания!");
                return;
            }

            DateTime start = DpStartDate.SelectedDate.Value.Date;
            DateTime end = DpEndDate.SelectedDate.Value.Date;

            if (end < start)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала!");
                return;
            }

            int contractId = (int)CmbContract.SelectedValue;
            int deptId = (int)CmbDepartment.SelectedValue;
            int empId = (int)CmbEmployee.SelectedValue;

            try
            {
                using (var db = new ProjectOrganizationDBEntities())
                {
                    // >>> ПРОВЕРКА НА ПЕРЕСЕЧЕНИЕ СТРОГО ПО ОДНОМУ ДОГОВОРУ <<<
                    int? editingWorkId = _editingWork?.WorkID;
                    bool isOverlapping = db.ProjectWorks.Any(w =>
                        w.ContractID == contractId && // Только для этого договора
                        (!editingWorkId.HasValue || w.WorkID != editingWorkId.Value) &&
                        w.StartDate <= end && w.EndDate >= start);

                    if (isOverlapping)
                    {
                        MessageBox.Show("По этому договору уже есть работа на выбранные даты (пересечение периодов)!",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (_editingWork == null)
                    {
                        db.ProjectWorks.Add(new ProjectWorks
                        {
                            ContractID = contractId,
                            DepartmentID = deptId,
                            ResponsibleEmployeeID = empId,
                            WorkDescription = TxtDescription.Text.Trim(),
                            StartDate = start,
                            EndDate = end
                        });
                    }
                    else
                    {
                        var work = db.ProjectWorks.FirstOrDefault(x => x.WorkID == _editingWork.WorkID);
                        if (work != null)
                        {
                            work.ContractID = contractId;
                            work.DepartmentID = deptId;
                            work.ResponsibleEmployeeID = empId;
                            work.WorkDescription = TxtDescription.Text.Trim();
                            work.StartDate = start;
                            work.EndDate = end;
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