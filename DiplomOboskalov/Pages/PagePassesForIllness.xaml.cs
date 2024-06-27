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
using static DiplomOboskalov.Pages.PagePastInfectiousDiases;

namespace DiplomOboskalov.Pages
{
    /// <summary>
    /// Логика взаимодействия для PagePassesForIllness.xaml
    /// </summary>
    public partial class PagePassesForIllness : Page
    {
        public PagePassesForIllness()
        {
            ListPassesForIllness = new List<PassesForIllnessInfo>();
            ListStudents = new List<StudentInfo>();

            InitializeComponent();

            LoadStudents();
            LoadPassesForIllness();

            ComboBoxStudent.SelectedIndex = 0;
        }
        public List<PassesForIllnessInfo> ListPassesForIllness { get; set; }
        public List<StudentInfo> ListStudents { get; set; }

        private int totalRecords;
        private int countclick = 0;


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------


        private void LoadPassesForIllness()
        {
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                PassesForIllnessDataGrid.ItemsSource = null;

                // Запрос к базе данных для получения данных о группах
                string query = "SELECT PFI.ID_PASSES_FOR_ILLNESS, PFI.Date, PFI.Diagnosis, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                               "FROM PASSES_FOR_ILLNESS PFI " +
                               "INNER JOIN [USER] U ON PFI.ID_USER = U.ID_USER " +
                               "WHERE 1=1 ";

                DataTable dt = connectionDb.ExecuteSql(query);

                // Очищаем предыдущие данные в коллекции групп
                ListPassesForIllness.Clear();

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in dt.Rows)
                {
                    ListPassesForIllness.Add(new PassesForIllnessInfo
                    {
                        ID_PASSES_FOR_ILLNESS = row["ID_PASSES_FOR_ILLNESS"].ToString(),
                        ID_USER = row["FIO"].ToString(),
                        Date = ParseAndFormatDate(row["Date"].ToString()),
                        Diagnosis = row["Diagnosis"].ToString()
                    });
                }
                totalRecords = ListPassesForIllness.Count;

                PassesForIllnessDataGrid.ItemsSource = ListPassesForIllness;
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
            PageAddOrChangePassesForIllness page = new PageAddOrChangePassesForIllness
            {
                AddOrChange = false
            };
            NavigationService.Navigate(page);
        }

        // Обработчик события для кнопки "Редактировать запись"
        private void ButtonEditRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (PassesForIllnessDataGrid.SelectedItem is PassesForIllnessInfo selected)
            {
                // Создаем экземпляр формы AddStudent
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    PageAddOrChangePassesForIllness page = new PageAddOrChangePassesForIllness();

                    // Передаем данные выбранного студента в форму редактирования
                    page.get(
                        int.Parse(selected.ID_PASSES_FOR_ILLNESS), selected.ID_USER, Convert.ToDateTime(selected.Date), selected.Diagnosis);

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
                    var sql = "DELETE FROM [PASSES_FOR_ILLNESS] WHERE ID_PASSES_FOR_ILLNESS = @ID_PASSES_FOR_ILLNESS";
                    // Подготовка команды SQL
                    parameters.Add(new SqlParameter("@ID_PASSES_FOR_ILLNESS", _ID));
                    // Выполнение SQL-запроса
                    connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());
                    MessageBox.Show("Студент успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadStudents();
                    LoadPassesForIllness();
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
            if (PassesForIllnessDataGrid.SelectedItem is PassesForIllnessInfo selected)
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
                LoadPassesForIllness();
            }
            else
            {
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    PassesForIllnessDataGrid.ItemsSource = null;

                    List<string> conditions = new List<string>();
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    string selectedStudent = Convert.ToString(ComboBoxStudent.SelectedItem);
                    string groupCondition = "U.Surname + ' ' + U.Name + ' ' + U.Patronymic = @GroupFilter";
                    conditions.Add(groupCondition);
                    parameters.Add(new SqlParameter("@GroupFilter", selectedStudent));

                    string query = "SELECT PFI.ID_PASSES_FOR_ILLNESS, PFI.Date, PFI.Diagnosis, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                                   "FROM PASSES_FOR_ILLNESS PFI " +
                                   "INNER JOIN [USER] U ON PFI.ID_USER = U.ID_USER " +
                                   "WHERE 1=1 ";


                    foreach (string condition in conditions)
                    {
                        query += " AND " + condition;
                    }

                    DataTable dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

                    ListPassesForIllness.Clear();

                    foreach (DataRow row in dt.Rows)
                    {
                        ListPassesForIllness.Add(new PassesForIllnessInfo
                        {
                            ID_PASSES_FOR_ILLNESS = row["ID_PASSES_FOR_ILLNESS"].ToString(),
                            ID_USER = row["FIO"].ToString(),
                            Date = ParseAndFormatDate(row["Date"].ToString()),
                            Diagnosis = row["Diagnosis"].ToString()
                        });
                    }
                    totalRecords = ListPassesForIllness.Count;
                }
                PassesForIllnessDataGrid.ItemsSource = ListPassesForIllness;
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

        public partial class PassesForIllnessInfo
        {
            public string ID_PASSES_FOR_ILLNESS { get; set; }
            public string ID_USER { get; set; }
            public string Date { get; set; }
            public string Diagnosis { get; set; }
        }

    }
}