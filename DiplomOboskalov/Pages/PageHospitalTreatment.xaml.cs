using DiplomLibrary;
using System;
using System.Collections.Generic;
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
using static DiplomOboskalov.Pages.PagePassesForIllness;

namespace DiplomOboskalov.Pages
{
    public partial class PageHospitalTreatment : Page
    {
        public PageHospitalTreatment()
        {
            ListHospitalTratment = new List<HospitalTratmentInfo>();
            ListStudents = new List<StudentInfo>();

            InitializeComponent();

            LoadStudents();
            LoadHospitalTratment();

            ComboBoxStudent.SelectedIndex = 0;
        }
        public List<HospitalTratmentInfo> ListHospitalTratment { get; set; }
        public List<StudentInfo> ListStudents { get; set; }

        private int totalRecords;
        private int countclick = 0;


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------


        private void LoadHospitalTratment()
        {
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                HospitalTratmentDataGrid.ItemsSource = null;

                // Запрос к базе данных для получения данных о группах
                string query = "SELECT HT.ID_HOSPITAL_TREATMENT, HT.Description, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO, MO.ID_MEDICAL_ORGANIZATION, HT.Date " +
                               "FROM HOSPITAL_TREATMENT HT " +
                               "INNER JOIN [USER] U ON HT.ID_USER = U.ID_USER " +
                               "INNER JOIN [MEDICAL_ORGANIZATION] MO ON HT.ID_MEDICAL_ORGANIZATION = MO.ID_MEDICAL_ORGANIZATION " +
                               "WHERE 1=1 ";

                DataTable dt = connectionDb.ExecuteSql(query);

                // Очищаем предыдущие данные в коллекции групп
                ListHospitalTratment.Clear();

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in dt.Rows)
                {
                    ListHospitalTratment.Add(new HospitalTratmentInfo
                    {
                        ID_PASSES_FOR_ILLNESS = row["ID_HOSPITAL_TREATMENT"].ToString(),
                        ID_USER = row["FIO"].ToString(),
                        Description = row["Description"].ToString(),
                        Med = row["ID_MEDICAL_ORGANIZATION"].ToString(),
                        Date = row["Date"].ToString()
                    });
                }
                totalRecords = ListHospitalTratment.Count;

                HospitalTratmentDataGrid.ItemsSource = ListHospitalTratment;
            }
        }

        private void LoadStudents()
        {
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Составляем запрос с учетом всех условий
                string query = "SELECT U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.HomeAddress, U.Hostel, U.ID_GROUP, U.DateEnrollment, G.GroupName, U.Color " +
                               "FROM [dbo].[USER] U " +
                               "INNER JOIN [GROUP] G ON U.ID_GROUP = G.ID_GROUP " +
                               "WHERE 1=1";

                DataTable dt = connectionDataBase.ExecuteSql(query);

                // Заполняем коллекцию студентов данными из запроса
                ListStudents.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    ListStudents.Add(new StudentInfo
                    {
                        ID = row["ID_USER"].ToString(),
                        FirstName = row["Surname"].ToString(),
                        MiddleName = row["Name"].ToString(),
                        LastName = row["Patronymic"].ToString(),
                        DateBirth = ParseAndFormatDate(row["DateBirth"]),
                        Hostel = row["Hostel"].ToString(),
                        DateEnrollment = ParseAndFormatDate(row["DateEnrollment"]),
                    });

                    //Удалите ItemsSource
                    ComboBoxStudent.ItemsSource = null;

                    // Добавьте элементы через Items
                    ComboBoxStudent.Items.Clear();
                    ComboBoxStudent.Items.Add("Все студенты");

                    foreach (var studentName in ListStudents.Select(s => s.FirstName + ' ' + s.MiddleName + ' ' + s.LastName).Distinct())
                    {
                        ComboBoxStudent.Items.Add(studentName);
                    }
                }

            }

        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------


        private void ButtonAddStudent_Click(object sender, RoutedEventArgs e)
        {
            // Создаем форму для добавления нового студента
            PageAddOrChangeHospitalTreatment page = new PageAddOrChangeHospitalTreatment
            {
                AddOrChange = false
            };
            NavigationService.Navigate(page);
        }

        // Обработчик события для кнопки "Редактировать запись"
        private void ButtonEditRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (HospitalTratmentDataGrid.SelectedItem is HospitalTratmentInfo selected)
            {
                // Создаем экземпляр формы AddStudent
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    PageAddOrChangeHospitalTreatment page = new PageAddOrChangeHospitalTreatment();

                    // Передаем данные выбранного студента в форму редактирования
                    page.get(
                        int.Parse(selected.ID_PASSES_FOR_ILLNESS), selected.ID_USER, selected.Description, selected.Med, Convert.ToDateTime(selected.Date));

                    // Переходим к форме редактирования студента
                    page.AddOrChange = true;
                    NavigationService.Navigate(page);
                }
            }
        }

        // Удаление студента по ID
        private void DeleteRow(string _ID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            try
            {
                using (ConnectionDataBase connectionDb = new ConnectionDataBase())
                {
                    // Используйте параметры в запросе
                    var sql = "DELETE FROM [HOSPITAL_TREATMENT] WHERE ID_HOSPITAL_TREATMENT = @ID_HOSPITAL_TREATMENT";
                    // Подготовка команды SQL
                    parameters.Add(new SqlParameter("@ID_HOSPITAL_TREATMENT", _ID));
                    // Выполнение SQL-запроса
                    connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());
                    MessageBox.Show("Студент успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadStudents();
                    LoadHospitalTratment();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении студента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик события для кнопки "Удалить запись"
        private void ButtonDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (HospitalTratmentDataGrid.SelectedItem is HospitalTratmentInfo selected)
            {
                // Удаляем выбранного студента из базы данных
                DeleteRow(selected.ID_PASSES_FOR_ILLNESS);
                // Обновляем отображение после удаления
                //RefreshDataGrid();
            }
        }

        private void ComboBoxStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxStudent.SelectedItem == "Все студенты")
            {
                LoadHospitalTratment();
            }
            else
            {
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    HospitalTratmentDataGrid.ItemsSource = null;

                    List<string> conditions = new List<string>();
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    string selectedStudent = Convert.ToString(ComboBoxStudent.SelectedItem);
                    string groupCondition = "U.Surname + ' ' + U.Name + ' ' + U.Patronymic = @GroupFilter";
                    conditions.Add(groupCondition);
                    parameters.Add(new SqlParameter("@GroupFilter", selectedStudent));

                    string query = "SELECT HT.ID_HOSPITAL_TREATMENT, HT.Description, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                                   "FROM HOSPITAL_TREATMENT HT " +
                                   "INNER JOIN [USER] U ON HT.ID_USER = U.ID_USER " +
                                   "WHERE 1=1 ";


                    foreach (string condition in conditions)
                    {
                        query += " AND " + condition;
                    }

                    DataTable dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

                    ListHospitalTratment.Clear();

                    foreach (DataRow row in dt.Rows)
                    {
                        ListHospitalTratment.Add(new HospitalTratmentInfo
                        {
                            ID_PASSES_FOR_ILLNESS = row["ID_HOSPITAL_TREATMENT"].ToString(),
                            ID_USER = row["FIO"].ToString(),
                            Description = row["Description"].ToString()
                        });
                    }
                    totalRecords = ListHospitalTratment.Count;
                }
                HospitalTratmentDataGrid.ItemsSource = ListHospitalTratment;
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

        public partial class HospitalTratmentInfo
        {
            public string ID_PASSES_FOR_ILLNESS { get; set; }
            public string ID_USER { get; set; }
            public string Description { get; set; }
            public string Med { get; set; }
            public string Date { get; set; }
        }

    }
}