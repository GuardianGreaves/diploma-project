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

namespace DiplomOboskalov.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageOperations.xaml
    /// </summary>
    public partial class PageOperations : Page
    {
        public PageOperations()
        {
            ListOperations = new List<OperationsInfo>();
            ListStudents = new List<StudentInfo>();

            InitializeComponent();

            LoadStudents();
            LoadOperations();

            ComboBoxStudent.SelectedIndex = 0;
        }
        public List<OperationsInfo> ListOperations { get; set; }
        public List<StudentInfo> ListStudents { get; set; }

        private int totalRecords;
        private int countclick = 0;


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------


        private void LoadOperations()
        {
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                OperationsDataGrid.ItemsSource = null;

                // Запрос к базе данных для получения данных о группах
                string query = "SELECT O.ID_OPERATIONS, O.Operations, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                               "FROM OPERATIONS O " +
                               "INNER JOIN [USER] U ON O.ID_USER = U.ID_USER " +
                               "WHERE 1=1 ";

                DataTable dt = connectionDb.ExecuteSql(query);

                // Очищаем предыдущие данные в коллекции групп
                ListOperations.Clear();

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in dt.Rows)
                {
                    ListOperations.Add(new OperationsInfo
                    {
                        ID_OPERATIONS = row["ID_OPERATIONS"].ToString(),
                        ID_USER = row["FIO"].ToString(),
                        Operations = row["Operations"].ToString()
                    });
                }
                totalRecords = ListOperations.Count;

                OperationsDataGrid.ItemsSource = ListOperations;
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
            PageAddOrChangeSanitarySpaTreatment page = new PageAddOrChangeSanitarySpaTreatment
            {
                AddOrChange = false
            };
            NavigationService.Navigate(page);
        }

        // Обработчик события для кнопки "Редактировать запись"
        private void ButtonEditRow_Click(object sender, RoutedEventArgs e)
        {
            //// Получаем выбранного студента из DataGrid
            //if (OperationsDataGrid.SelectedItem is OperationsInfo selected)
            //{
            //    // Создаем экземпляр формы AddStudent
            //    using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            //    {
            //        PageAddOrChangeSanitarySpaTreatment page = new PageAddOrChangeSanitarySpaTreatment();

            //        // Передаем данные выбранного студента в форму редактирования
            //        page.get(
            //            int.Parse(selected.ID_SANITARY_SPA_TREATMENT), selected.ID_USER, selected.Description);

            //        // Переходим к форме редактирования студента
            //        page.AddOrChange = true;
            //        NavigationService.Navigate(page);
            //    }
            //}
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
                    var sql = "DELETE FROM [OPERATIONS] WHERE ID_OPERATIONS = @ID_OPERATIONS ";
                    // Подготовка команды SQL
                    parameters.Add(new SqlParameter("@ID_OPERATIONS ", _ID));
                    // Выполнение SQL-запроса
                    connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());
                    MessageBox.Show("Студент успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadStudents();
                    LoadOperations();
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
            if (OperationsDataGrid.SelectedItem is OperationsInfo selected)
            {
                // Удаляем выбранного студента из базы данных
                DeleteRow(selected.ID_OPERATIONS);
                // Обновляем отображение после удаления
                //RefreshDataGrid();
            }
        }

        private void ComboBoxStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxStudent.SelectedItem == "Все студенты")
            {
                LoadOperations();
            }
            else
            {
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    OperationsDataGrid.ItemsSource = null;

                    List<string> conditions = new List<string>();
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    string selectedStudent = Convert.ToString(ComboBoxStudent.SelectedItem);
                    string groupCondition = "U.Surname + ' ' + U.Name + ' ' + U.Patronymic = @GroupFilter";
                    conditions.Add(groupCondition);
                    parameters.Add(new SqlParameter("@GroupFilter", selectedStudent));

                    string query = "SELECT O.ID_OPERATIONS, O.Operations, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                                   "FROM OPERATIONS O " +
                                   "INNER JOIN [USER] U ON O.ID_USER = U.ID_USER " +
                                   "WHERE 1=1 ";

                    foreach (string condition in conditions)
                    {
                        query += " AND " + condition;
                    }

                    DataTable dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

                    ListOperations.Clear();

                    foreach (DataRow row in dt.Rows)
                    {
                        ListOperations.Add(new OperationsInfo
                        {
                            ID_OPERATIONS = row["ID_OPERATIONS"].ToString(),
                            ID_USER = row["FIO"].ToString(),
                            Operations = row["Operations"].ToString()
                        });
                    }
                    totalRecords = ListOperations.Count;
                }
                OperationsDataGrid.ItemsSource = ListOperations;
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

        public partial class OperationsInfo
        {
            public string ID_OPERATIONS { get; set; }
            public string ID_USER { get; set; }
            public string Operations { get; set; }
        }

    }
}