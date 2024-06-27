using DiplomLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiplomOboskalov.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageAddOrChangeHospitalTreatment.xaml
    /// </summary>
    public partial class PageAddOrChangeHospitalTreatment : Page
    {
        public PageAddOrChangeHospitalTreatment()
        {
            ListMedicalOrganization = new List<MedicalOrganizationInfo>();
            ListStudents = new List<StudentInfo>();
            InitializeComponent();
            LoadStudents();
            LoadMedicalOrganizations();
        }

        public List<StudentInfo> ListStudents { get; set; }
        public List<MedicalOrganizationInfo> ListMedicalOrganization { get; set; }

        public bool AddOrChange = false;

        public string MEDICAL_ORGANIZATION;
        public int ID_MEDICAL_ORGANIZATION;

        public int ID;
        public string USER;
        public int ID_USER;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void get(int _ID_PAST_INFECTIOUS_DIASES, string _USER, string _Description, string _ID_MEDICAL_ORGANIZATION, DateTime _Year)
        {
            USER = _USER;
            ID = _ID_PAST_INFECTIOUS_DIASES;
            ComboBoxStudentFIO.Text = _USER;
            TextBoxDescription.Text = _Description;
            ComboBoxMedicalOrganization.Text = _ID_MEDICAL_ORGANIZATION;
            DatePickerYear.Text = _Year.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ConnectionDataBase connectionDb = new ConnectionDataBase())
                {

                    // Поиск ID_USER
                    string query1 = $"SELECT ID_USER FROM [USER] WHERE Surname + ' ' + Name + ' ' + Patronymic = '{ComboBoxStudentFIO.Text}'";

                    DataTable result = connectionDb.ExecuteSql(query1);

                    if (result.Rows.Count > 0)
                    {
                        ID_USER = Convert.ToInt32(result.Rows[0]["ID_USER"]);
                    }
                    else
                    {
                        MessageBox.Show($"'{ComboBoxStudentFIO.Text}' не найдено в базе данных.");
                    }

                    string query_groupname2 = $"SELECT ID_MEDICAL_ORGANIZATION FROM [MEDICAL_ORGANIZATION] WHERE Name = '{ComboBoxMedicalOrganization.Text}'";

                    // Выполнение запроса
                    DataTable result2 = connectionDb.ExecuteSql(query_groupname2);

                    // Проверка, что запрос вернул результат
                    if (result2.Rows.Count > 0)
                    {
                        ID_MEDICAL_ORGANIZATION = Convert.ToInt32(result2.Rows[0]["ID_MEDICAL_ORGANIZATION"]);
                    }
                    else
                    {
                        MessageBox.Show($"Группа с названием {MEDICAL_ORGANIZATION} не найдена в базе данных.");
                    }



                    // Добавление
                    try
                    {
                        if (AddOrChange == false)
                        {
                            string sql = "INSERT INTO [dbo].[HOSPITAL_TREATMENT] (ID_USER, Description, ID_MEDICAL_ORGANIZATION, Date) " +
                                         "VALUES (@ID_USER, @Description, @ID_MEDICAL_ORGANIZATION, @Date) ";

                            // Формируем условия фильтрации
                            List<SqlParameter> parameters = new List<SqlParameter>();

                            // Добавляем условия для текстового поиска
                            parameters.Add(new SqlParameter("@ID_USER", ID_USER));
                            parameters.Add(new SqlParameter("@Description", TextBoxDescription.Text));
                            parameters.Add(new SqlParameter("@ID_MEDICAL_ORGANIZATION", ID_MEDICAL_ORGANIZATION));
                            parameters.Add(new SqlParameter("@Date", ParseAndFormatDate(DatePickerYear.Text)));


                            DataTable result3 = connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());

                            if (result3.Rows.Count > 0)
                            {
                                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }


                    // Редактирование
                    try
                    {

                        if (AddOrChange == true)
                        {
                            List<SqlParameter> parameters = new List<SqlParameter>();

                            var sql = "UPDATE [HOSPITAL_TREATMENT] SET " +
                                        "ID_USER = @ID_USER, " +
                                        "Description = @Description " +
                                        "ID_MEDICAL_ORGANIZATION = @ID_MEDICAL_ORGANIZATION, " +
                                        "Year = @Year " +
                                        "WHERE ID_HOSPITAL_TREATMENT = @ID_HOSPITAL_TREATMENT";

                            parameters.Add(new SqlParameter("@ID_USER", ID_USER));
                            parameters.Add(new SqlParameter("@Description", TextBoxDescription.Text));
                            parameters.Add(new SqlParameter("@ID_MEDICAL_ORGANIZATION", ID_MEDICAL_ORGANIZATION));
                            parameters.Add(new SqlParameter("@Year", ParseAndFormatDate(DatePickerYear.Text)));

                            parameters.Add(new SqlParameter("@ID_HOSPITAL_TREATMENT", ID));

                            DataTable result4 = connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());

                            if (result4.Rows.Count > 0)
                            {

                                MessageBox.Show("Данные успешно изменены в базе данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при изменении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            PageHospitalTreatment page = new PageHospitalTreatment();
            NavigationService.Navigate(page);
        }

        public void LoadMedicalOrganizations()
        {
            ListMedicalOrganization.Clear();
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Запрос к базе данных для получения данных о группах
                string query = "SELECT ID_MEDICAL_ORGANIZATION, Name, Address FROM [MEDICAL_ORGANIZATION]";
                DataTable result = connectionDb.ExecuteSql(query);

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in result.Rows)
                {
                    ListMedicalOrganization.Add(new MedicalOrganizationInfo
                    {
                        ID = row["ID_MEDICAL_ORGANIZATION"].ToString(),
                        Name = row["Name"].ToString(),
                        Address = row["Address"].ToString()
                    });
                    // Удалите ItemsSource
                    ComboBoxMedicalOrganization.ItemsSource = null;

                    // Добавьте элементы через Items
                    ComboBoxMedicalOrganization.Items.Clear();

                    foreach (var studentName in ListMedicalOrganization.Select(s => s.Name).Distinct())
                    {
                        ComboBoxMedicalOrganization.Items.Add(studentName);
                    }

                }

            }
        }
        private string ParseAndFormatDate(object dateObject)
        {
            if (DateTime.TryParse(dateObject.ToString(), out DateTime date))
            {
                return date.ToString("dd.MM.yyyy"); // Используйте нужный вам формат
            }
            else
            {
                return string.Empty; // В случае ошибки парсинга можно вернуть пустую строку или другое значение по умолчанию
            }
        }
        private void LoadStudents()
        {
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                string query = "SELECT U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.Hostel, U.ID_GROUP, U.DateEnrollment " +
                               "FROM [USER] U ";

                DataTable dt = connectionDataBase.ExecuteSql(query);

                ListStudents.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    ListStudents.Add(new StudentInfo
                    {
                        ID = row["ID_USER"].ToString(),
                        FirstName = row["Surname"].ToString(),
                        MiddleName = row["Name"].ToString(),
                        LastName = row["Patronymic"].ToString(),
                    });

                    ComboBoxStudentFIO.ItemsSource = null;
                    ComboBoxStudentFIO.Items.Clear();
                    foreach (var studentName in ListStudents.Select(s => s.FirstName + ' ' + s.MiddleName + ' ' + s.LastName).Distinct())
                    {
                        ComboBoxStudentFIO.Items.Add(studentName);
                    }

                }
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PageHospitalTreatment page = new PageHospitalTreatment();
            NavigationService.Navigate(page);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------


        public partial class StudentInfo
        {
            public string ID { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string Group { get; set; }
            public string DateBirth { get; set; }
            public string Hostel { get; set; }
            public string DateEnrollment { get; set; }
        }
        public partial class MedicalOrganizationInfo
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
        }

    }
}