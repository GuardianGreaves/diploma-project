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
    /// Логика взаимодействия для PagePastInfectiousDiases.xaml
    /// </summary>
    public partial class PagePastInfectiousDiases : Page
    {
        public PagePastInfectiousDiases()
        {
            ListPastInfectiousDiases = new List<PastInfectiousDiasesInfo>();

            ListStudents = new List<StudentInfo>();

            InitializeComponent(); 
            
            LoadStudents();
            LoadPastInfectiousDiases();

            ComboBoxStudent.SelectedIndex = 0;
        }

        public List<PastInfectiousDiasesInfo> ListPastInfectiousDiases { get; set; }
        public List<StudentInfo> ListStudents { get; set; }

        private int totalRecords;
        private int countclick = 0;


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------


        private void LoadPastInfectiousDiases()
        {
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                PastInfectiousDiasesDataGrid.ItemsSource = null;

                // Запрос к базе данных для получения данных о группах
                string query = "SELECT PID.ID_PAST_INFECTIOUS_DIASES, PID.Date, PID.Description, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                               "FROM PAST_INFECTIOUS_DIASES PID " +
                               "INNER JOIN [USER] U ON PID.ID_USER = U.ID_USER " +
                               "WHERE 1=1 ";

                DataTable dt = connectionDb.ExecuteSql(query);

                // Очищаем предыдущие данные в коллекции групп
                ListPastInfectiousDiases.Clear();

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in dt.Rows)
                {
                    ListPastInfectiousDiases.Add(new PastInfectiousDiasesInfo
                    {
                        ID_PAST_INFECTIOUS_DIASES = row["ID_PAST_INFECTIOUS_DIASES"].ToString(),
                        ID_USER = row["FIO"].ToString(),
                        Date = ParseAndFormatDate(row["Date"].ToString()),
                        Description = row["Description"].ToString()
                    });
                }
                totalRecords = ListPastInfectiousDiases.Count;

                PastInfectiousDiasesDataGrid.ItemsSource = ListPastInfectiousDiases;
            }
        }

        private void LoadStudents()
        {
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Составляем запрос с учетом всех условий
                string query = "SELECT PID.ID_PAST_INFECTIOUS_DIASES, PID.Date, PID.Description, U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.Hostel, U.DateEnrollment " +
                               "FROM PAST_INFECTIOUS_DIASES PID " +
                               "INNER JOIN [USER] U ON PID.ID_USER = U.ID_USER " +
                               "WHERE 1=1 ";

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
            PageAddOrChangePastInfectiousDiases page = new PageAddOrChangePastInfectiousDiases
            {
                AddOrChange = false
            };
            NavigationService.Navigate(page);
        }

        // Обработчик события для кнопки "Редактировать запись"
        private void ButtonEditRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (PastInfectiousDiasesDataGrid.SelectedItem is PastInfectiousDiasesInfo selectedPreventieVaccine)
            {
                // Создаем экземпляр формы AddStudent
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    PageAddOrChangePastInfectiousDiases page = new PageAddOrChangePastInfectiousDiases();

                    // Передаем данные выбранного студента в форму редактирования
                    page.get(
                        int.Parse(selectedPreventieVaccine.ID_PAST_INFECTIOUS_DIASES), selectedPreventieVaccine.ID_USER, Convert.ToDateTime(selectedPreventieVaccine.Date), selectedPreventieVaccine.Description);

                    // Переходим к форме редактирования студента
                    page.AddOrChange = true;
                    NavigationService.Navigate(page);
                }
            }
        }

        // Удаление студента по ID
        private void DeleteStudent(string studentID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            try
            {
                using (ConnectionDataBase connectionDb = new ConnectionDataBase())
                {
                    // Используйте параметры в запросе
                    var sql = "DELETE FROM [PAST_INFECTIOUS_DIASES] WHERE ID_PAST_INFECTIOUS_DIASES = @ID_PAST_INFECTIOUS_DIASES";
                    // Подготовка команды SQL
                    parameters.Add(new SqlParameter("@ID_PAST_INFECTIOUS_DIASES", studentID));
                    // Выполнение SQL-запроса
                    connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());
                    MessageBox.Show("Студент успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении студента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadPastInfectiousDiases();
            LoadStudents();
        }

        // Обработчик события для кнопки "Удалить запись"
        private void ButtonDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (PastInfectiousDiasesDataGrid.SelectedItem is PastInfectiousDiasesInfo selectedPreventieVaccine)
            {
                // Удаляем выбранного студента из базы данных
                DeleteStudent(selectedPreventieVaccine.ID_PAST_INFECTIOUS_DIASES);
                // Обновляем отображение после удаления
                //RefreshDataGrid();
            }
        }

        private void ComboBoxStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxStudent.SelectedItem == "Все студенты")
            {
                LoadPastInfectiousDiases();
            }
            else
            {
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    // Очищаем текущее содержимое DataGrid
                    PastInfectiousDiasesDataGrid.ItemsSource = null;

                    // Формируем условия фильтрации
                    List<string> conditions = new List<string>();
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    string selectedStudent = Convert.ToString(ComboBoxStudent.SelectedItem);
                    // Фильтрация по группе
                    string groupCondition = "U.Surname + ' ' + U.Name + ' ' + U.Patronymic = @GroupFilter";
                    conditions.Add(groupCondition);
                    parameters.Add(new SqlParameter("@GroupFilter", selectedStudent));

                    // Составляем запрос с учетом всех условий
                    string query = "SELECT PID.ID_PAST_INFECTIOUS_DIASES, PID.Date, PID.Description, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                                   "FROM PAST_INFECTIOUS_DIASES PID " +
                                   "INNER JOIN [USER] U ON PID.ID_USER = U.ID_USER " +
                                   "WHERE 1=1 ";


                    foreach (string condition in conditions)
                    {
                        query += " AND " + condition;
                    }

                    // Выполняем SQL-запрос с параметрами
                    DataTable dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

                    // Заполняем коллекцию студентов данными из запроса
                    // Очищаем предыдущие данные в коллекции групп
                    ListPastInfectiousDiases.Clear();

                    // Добавляем данные о группах в коллекцию Groups
                    foreach (DataRow row in dt.Rows)
                    {
                        ListPastInfectiousDiases.Add(new PastInfectiousDiasesInfo
                        {
                            ID_PAST_INFECTIOUS_DIASES = row["ID_PAST_INFECTIOUS_DIASES"].ToString(),
                            ID_USER = row["FIO"].ToString(),
                            Date = ParseAndFormatDate(row["Date"].ToString()),
                            Description = row["Description"].ToString()
                        });
                    }
                    totalRecords = ListPastInfectiousDiases.Count;
                }
                PastInfectiousDiasesDataGrid.ItemsSource = ListPastInfectiousDiases;
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

        public partial class PastInfectiousDiasesInfo
        {
            public string ID_PAST_INFECTIOUS_DIASES { get; set; }
            public string ID_USER { get; set; }
            public string Date { get; set; }
            public string Description { get; set; }
        }

    }
}