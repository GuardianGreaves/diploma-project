using DiplomLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
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
    /// Логика взаимодействия для PageAddOrChangeSurveys.xaml
    /// </summary>
    public partial class PageAddOrChangeSurveys : Page
    {
        public PageAddOrChangeSurveys()
        {
            allStudents = new ObservableCollection<Student>();
            allMedicalOrganization = new ObservableCollection<MedicalOrganization>();
            InitializeComponent();
            LoadStudents();
            LoadMedicalOrganizations();
        }

        public int ID;
        public bool AddOrChange = false;
        public string ID_USER;
        public string ID_MEDICAL_ORGANIZATION;

        private readonly ObservableCollection<Student> allStudents = new ObservableCollection<Student>();
        public partial class Student
        {
            public string ID { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
        }
        public void studentget(int _ID_SURVEYS, string _ID_USER, string _Year, string _Survey, string _SurveyResult, string _ID_MEDICAL_ORGANIZATION)
        {
            ID_MEDICAL_ORGANIZATION = _ID_MEDICAL_ORGANIZATION;
            ID_USER = _ID_USER;
            ID = _ID_SURVEYS;
            TextBoxSurveys.Text = _Survey;
            TextBoxSurveyResult.Text = _SurveyResult;
            ComboBoxMedicalOrganization.Text = _ID_MEDICAL_ORGANIZATION;
            ComboBoxStudentFIO.Text = _ID_USER;
            TextBoxDate.Text = _Year;
            // Создаем экземпляр ConnectionDataBase
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Запрос к базе данных для получения ID_GROUP по названию группы
                string query_groupname = $"SELECT ID_USER FROM [USER] WHERE Surname + ' ' + Name + ' ' + Patronymic = '{_ID_USER}'";

                // Выполнение запроса
                DataTable result = connectionDataBase.ExecuteSql(query_groupname);

                // Проверка, что запрос вернул результат
                if (result.Rows.Count > 0)
                {
                    //ComboBoxStudentFIO.Text = Convert.ToInt32(result.Rows[0]["ID_USER"]) ;
                }
                else
                {
                    MessageBox.Show($"Группа с названием {_ID_USER} не найдена в базе данных.");
                }

                // Запрос к базе данных для получения ID_GROUP по названию группы
                string query_groupname2 = $"SELECT ID_MEDICAL_ORGANIZATION FROM [MEDICAL_ORGANIZATION] WHERE Name= '{_ID_MEDICAL_ORGANIZATION}'";

                // Выполнение запроса
                DataTable result2 = connectionDataBase.ExecuteSql(query_groupname2);

                // Проверка, что запрос вернул результат
                if (result2.Rows.Count > 0)
                {
                    // Получение значения ID_GROUP из первой строки результата
                    //ComboBoxMedicalOrganization.SelectedIndex = Convert.ToInt32(result2.Rows[0]["ID_MEDICAL_ORGANIZATION"]);
                }
                else
                {
                    MessageBox.Show($"Группа с названием {_ID_USER} не найдена в базе данных.");
                }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string _ID_MEDICAL_ORGANIZATION2 = ComboBoxMedicalOrganization.Text;
            string _ID_USER2 = ComboBoxStudentFIO.Text;

            // Создаем экземпляр ConnectionDataBase
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Запрос к базе данных для получения ID_GROUP по названию группы
                string query_groupname = $"SELECT ID_USER FROM [USER] WHERE Surname + ' ' + Name + ' ' + Patronymic = '{_ID_USER2}'";

                // Выполнение запроса
                DataTable result = connectionDataBase.ExecuteSql(query_groupname);

                // Проверка, что запрос вернул результат
                if (result.Rows.Count > 0)
                {
                    ID_USER = result.Rows[0]["ID_USER"].ToString();
                }
                else
                {
                    MessageBox.Show($"Группа с названием {_ID_USER2} не найдена в базе данных.");
                }

                // Запрос к базе данных для получения ID_GROUP по названию группы
                string query_groupname2 = $"SELECT ID_MEDICAL_ORGANIZATION FROM [MEDICAL_ORGANIZATION] WHERE Name= '{_ID_MEDICAL_ORGANIZATION2}'";

                // Выполнение запроса
                DataTable result2 = connectionDataBase.ExecuteSql(query_groupname2);

                // Проверка, что запрос вернул результат
                if (result2.Rows.Count > 0)
                {
                    // Получение значения ID_GROUP из первой строки результата
                    ID_MEDICAL_ORGANIZATION = result2.Rows[0]["ID_MEDICAL_ORGANIZATION"].ToString();
                }
                else
                {
                    MessageBox.Show($"Группа с названием {_ID_MEDICAL_ORGANIZATION2} не найдена в базе данных.");
                }

            }


            try
            {
                using (ConnectionDataBase connectionDb = new ConnectionDataBase())
                {
                    // ... (код до получения idGroup)
                    string Survey = TextBoxSurveys.Text;
                    string SurveyResult = TextBoxSurveyResult.Text;
                    string Year = TextBoxDate.Text;

                    if (AddOrChange == false)
                    {
                        string sql = "INSERT INTO [dbo].[SURVEYS] (ID_USER, ID_MEDICAL_ORGANIZATION, Year, Survey, SurveyResult) " +
                                     "VALUES (@ID_USER, @ID_MEDICAL_ORGANIZATION, @Year, @Survey, @SurveyResult) ";

                        // Формируем условия фильтрации
                        List<SqlParameter> parameters = new List<SqlParameter>();

                        // Добавляем условия для текстового поиска
                        parameters.Add(new SqlParameter("@ID_USER", ID_USER));
                        parameters.Add(new SqlParameter("@ID_MEDICAL_ORGANIZATION", ID_MEDICAL_ORGANIZATION));
                        parameters.Add(new SqlParameter("@Year", Year));
                        parameters.Add(new SqlParameter("@Survey", Survey));
                        parameters.Add(new SqlParameter("@SurveyResult", SurveyResult));

                        DataTable result = connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());

                        MessageBox.Show("Данные успешно добавлены в базу данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        try
                        {
                            List<SqlParameter> parameters = new List<SqlParameter>();

                            // Используйте параметры в запросе
                            var sql = "UPDATE [SURVEYS] SET " +
                                        "ID_USER = @ID_USER, " +
                                        "ID_MEDICAL_ORGANIZATION = @ID_MEDICAL_ORGANIZATION, " +
                                        "Year = @Year, " +
                                        "Survey = @Survey, " +
                                        "SurveyResult = @SurveyResult " +
                                        "WHERE ID_SURVEYS = @ID_SURVEYS";

                            parameters.Add(new SqlParameter("@ID_USER", ID_USER));
                            parameters.Add(new SqlParameter("@ID_MEDICAL_ORGANIZATION", ID_MEDICAL_ORGANIZATION));
                            parameters.Add(new SqlParameter("@Year", Year));
                            parameters.Add(new SqlParameter("@Survey", Survey));
                            parameters.Add(new SqlParameter("@SurveyResult", SurveyResult));
                            parameters.Add(new SqlParameter("@ID_SURVEYS", ID));

                            DataTable dt = connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());

                            MessageBox.Show("Данные успешно изменены в базе данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при изменении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            PageSurveys pageSurveys = new PageSurveys();
            NavigationService.Navigate(pageSurveys);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PageSurveys pageSurveys = new PageSurveys();
            NavigationService.Navigate(pageSurveys);
        }

        private readonly ObservableCollection<MedicalOrganization> allMedicalOrganization = new ObservableCollection<MedicalOrganization>();

        public void LoadMedicalOrganizations()
        {
            allMedicalOrganization.Clear();
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Запрос к базе данных для получения данных о группах
                string query = "SELECT ID_MEDICAL_ORGANIZATION, Name, Address FROM [MEDICAL_ORGANIZATION]";
                DataTable result = connectionDb.ExecuteSql(query);

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in result.Rows)
                {
                    allMedicalOrganization.Add(new MedicalOrganization
                    {
                        ID = row["ID_MEDICAL_ORGANIZATION"].ToString(),
                        Name = row["Name"].ToString(),
                        Address = row["Address"].ToString()
                    });
                    // Удалите ItemsSource
                    ComboBoxMedicalOrganization.ItemsSource = null;

                    // Добавьте элементы через Items
                    ComboBoxMedicalOrganization.Items.Clear();

                    foreach (var studentName in allMedicalOrganization.Select(s => s.Name).Distinct())
                    {
                        ComboBoxMedicalOrganization.Items.Add(studentName);
                    }

                }

            }
        }
        public partial class MedicalOrganization
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
        }

        public partial class Surveys
        {
            public string ID_SURVEYS { get; set; }
            public string ID_USER { get; set; }
            public string Year { get; set; }
            public string Survey { get; set; }
            public string SurveyResult { get; set; }
            public string ID_MEDICAL_ORGANIZATION { get; set; }

        }

        private void LoadStudents()
        {
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Формируем условия фильтрации
                List<string> conditions = new List<string>();
                List<SqlParameter> parameters = new List<SqlParameter>();

                // Составляем запрос с учетом всех условий
                string query = "SELECT U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.Hostel, U.ID_GROUP, U.DateEnrollment " +
                               "FROM [USER] U ";

                // Выполняем SQL-запрос с параметрами
                DataTable dt = connectionDataBase.ExecuteSql(query);

                // Заполняем коллекцию студентов данными из запроса
                allStudents.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    allStudents.Add(new Student
                    {
                        ID = row["ID_USER"].ToString(),
                        FirstName = row["Surname"].ToString(),
                        MiddleName = row["Name"].ToString(),
                        LastName = row["Patronymic"].ToString(),
                    });
                    // Удалите ItemsSource
                    ComboBoxStudentFIO.ItemsSource = null;

                    // Добавьте элементы через Items
                    ComboBoxStudentFIO.Items.Clear();

                    foreach (var studentName in allStudents.Select(s => s.FirstName + ' ' + s.MiddleName + ' ' + s.LastName).Distinct())
                    {
                        ComboBoxStudentFIO.Items.Add(studentName);
                    }
                }
            }
        }

    }
}
