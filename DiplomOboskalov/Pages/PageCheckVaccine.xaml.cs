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
using static DiplomOboskalov.Pages.PageSanitarySpaTreatment;

namespace DiplomOboskalov.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageCheckVaccine.xaml
    /// </summary>
    public partial class PageCheckVaccine : Page
    {
        public PageCheckVaccine()
        {
            ListCheckVaccine = new List<CheckVaccineInfo>();
            ListStudents = new List<StudentInfo>();

            InitializeComponent();

            LoadStudents();
            LoadCheckVaccine();

            ComboBoxStudent.SelectedIndex = 0;
        }
        public List<CheckVaccineInfo> ListCheckVaccine { get; set; }
        public List<StudentInfo> ListStudents { get; set; }

        private int totalRecords;
        private int countclick = 0;


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------


        private void LoadCheckVaccine()
        {
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                CheckVaccineDataGrid.ItemsSource = null;

                // Запрос к базе данных для получения данных о группах
                string query = "SELECT CV.ID_CHECK_VACCINE, CV.Date, CV.BodyTemp, CV.ResultInspecion, CV.Vaccine, CV.Permission, CV.StateAfterThirtyMin, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                               "FROM CHECK_VACCINE CV " +
                               "INNER JOIN [USER] U ON CV.ID_USER = U.ID_USER " +
                               "WHERE 1=1 ";

                DataTable dt = connectionDb.ExecuteSql(query);

                // Очищаем предыдущие данные в коллекции групп
                ListCheckVaccine.Clear();

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in dt.Rows)
                {
                    ListCheckVaccine.Add(new CheckVaccineInfo
                    {
                        ID_CHECK_VACCINE = row["ID_CHECK_VACCINE"].ToString(),
                        ID_USER = row["FIO"].ToString(),
                        Date = ParseAndFormatDate(row["Date"].ToString()),
                        BodyTemp = row["BodyTemp"].ToString(),
                        ResultInspecion = row["ResultInspecion"].ToString(),
                        Vaccine = row["Vaccine"].ToString(),
                        Permission = row["Permission"].ToString(),
                        StateAfterThirtyMin = row["StateAfterThirtyMin"].ToString()
                    });
                }
                totalRecords = ListCheckVaccine.Count;

                CheckVaccineDataGrid.ItemsSource = ListCheckVaccine;
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
            PageAddOrChangeCheckVaccine page = new PageAddOrChangeCheckVaccine
            {
                AddOrChange = false
            };
            NavigationService.Navigate(page);
        }

        // Обработчик события для кнопки "Редактировать запись"
        private void ButtonEditRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (CheckVaccineDataGrid.SelectedItem is CheckVaccineInfo selected)
            {
                // Создаем экземпляр формы AddStudent
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    PageAddOrChangeCheckVaccine page = new PageAddOrChangeCheckVaccine();

                    // Передаем данные выбранного студента в форму редактирования
                    page.get(
                        int.Parse(selected.ID_CHECK_VACCINE), selected.ID_USER, selected.Date, selected.BodyTemp, selected.ResultInspecion, selected.Vaccine, selected.Permission, selected.StateAfterThirtyMin);

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
                    var sql = "DELETE FROM [CHECK_VACCINE] WHERE ID_CHECK_VACCINE = @ID_CHECK_VACCINE";
                    // Подготовка команды SQL
                    parameters.Add(new SqlParameter("@ID_CHECK_VACCINE", _ID));
                    // Выполнение SQL-запроса
                    connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());
                    MessageBox.Show("Студент успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadStudents();
                    LoadCheckVaccine();
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
            if (CheckVaccineDataGrid.SelectedItem is CheckVaccineInfo selected)
            {
                // Удаляем выбранного студента из базы данных
                DeleteRow(selected.ID_CHECK_VACCINE);
                // Обновляем отображение после удаления
                //RefreshDataGrid();
            }
        }

        private void ComboBoxStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxStudent.SelectedItem == "Все студенты")
            {
                LoadCheckVaccine();
            }
            else
            {
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    CheckVaccineDataGrid.ItemsSource = null;

                    List<string> conditions = new List<string>();
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    string selectedStudent = Convert.ToString(ComboBoxStudent.SelectedItem);
                    string groupCondition = "U.Surname + ' ' + U.Name + ' ' + U.Patronymic = @GroupFilter";
                    conditions.Add(groupCondition);
                    parameters.Add(new SqlParameter("@GroupFilter", selectedStudent));

                    string query = "SELECT CV.ID_CHECK_VACCINE, CV.Date, CV.BodyTemp, CV.ResultInspecion, CV.Vaccine, CV.Permission, CV.StateAfterThirtyMin, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                                   "FROM CHECK_VACCINE CV " +
                                   "INNER JOIN [USER] U ON CV.ID_USER = U.ID_USER " +
                                   "WHERE 1=1 ";


                    foreach (string condition in conditions)
                    {
                        query += " AND " + condition;
                    }

                    DataTable dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

                    ListCheckVaccine.Clear();

                    foreach (DataRow row in dt.Rows)
                    {
                        ListCheckVaccine.Add(new CheckVaccineInfo
                        {
                            ID_CHECK_VACCINE = row["ID_CHECK_VACCINE"].ToString(),
                            ID_USER = row["FIO"].ToString(),
                            Date = ParseAndFormatDate(row["Date"].ToString()),
                            BodyTemp = row["BodyTemp"].ToString(),
                            ResultInspecion = row["ResultInspecion"].ToString(),
                            Vaccine = row["Vaccine"].ToString(),
                            Permission = row["Permission"].ToString(),
                            StateAfterThirtyMin = row["StateAfterThirtyMin"].ToString()
                        });
                    }
                    totalRecords = ListCheckVaccine.Count;
                }
                CheckVaccineDataGrid.ItemsSource = ListCheckVaccine;
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

        public partial class CheckVaccineInfo
        {
            public string ID_CHECK_VACCINE { get; set; }
            public string ID_USER { get; set; }
            public string Date { get; set; }
            public string BodyTemp { get; set; }
            public string ResultInspecion { get; set; }
            public string Vaccine { get; set; }
            public string Permission { get; set; }
            public string StateAfterThirtyMin { get; set; }
        }

    }
}