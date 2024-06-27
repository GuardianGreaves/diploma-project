using DiplomLibrary;
using DiplomLibrary.DataBase;
using DiplomLibrary.Export;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using MahApps.Metro.IconPacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static DiplomOboskalov.Pages.PageTrauma;

namespace DiplomOboskalov.Pages
{
    public partial class PageGroups : Page
    {
        private readonly StudentsDataLoader studentsDataLoader = new StudentsDataLoader();

        public ObservableCollection<Student> AllStudents
        {
            get { return allStudents; }
        }

        private readonly ObservableCollection<Student> allStudents = new ObservableCollection<Student>();

        // Коллекция студентов, отображаемых на текущей странице
        private readonly ObservableCollection<Student> displayedStudents;




        public ObservableCollection<Group> AllGroups
        {
            get { return allGroup; }
        }

        // Список групп, доступных для фильтрации
        public List<Group> Groups { get; set; }

        // Событие, срабатывающее при запросе на открытие окна добавления студента
        public event EventHandler OpenAddGroupRequested;

        // Коллекция всех студентов из базы данных
        private readonly ObservableCollection<Group> allGroup = new ObservableCollection<Group>();

        // Коллекция студентов, отображаемых на текущей странице
        private readonly ObservableCollection<Group> displayedGroups;

        // Общее количество записей в базе данных
        private int totalRecords;

        // Счетчик кликов для фильтрации
        private int countclick = 0;


        public PageGroups()
        {
            // Инициализация коллекций и компонентов страницы
            allGroup = new ObservableCollection<Group>();
            displayedGroups = new ObservableCollection<Group>();
            Groups = new List<Group>();
            allStudents = new ObservableCollection<Student>();
            displayedStudents = new ObservableCollection<Student>();

            InitializeComponent();
            FillStudentsDataGrid();
            LoadStudents();
            FillGroupsDataGrid();
            LoadGroups();
            GroupsDataGrid.SelectedIndex = 0;
        }

        private void LoadGroups()
        {
            AllGroups.Clear();
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Запрос к базе данных для получения данных о группах
                string query = "SELECT ID_GROUP, GroupName, NumberPeople FROM [GROUP]";
                DataTable result = connectionDb.ExecuteSql(query);

                // Очищаем предыдущие данные в коллекции групп
                Groups.Clear();

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in result.Rows)
                {
                    allGroup.Add(new Group
                    {
                        GroupName = row["GroupName"].ToString(),
                        GroupID = row["ID_GROUP"].ToString(),
                        NumberPeople = row["NumberPeople"].ToString()
                    });
                }
                totalRecords = allGroup.Count;
                FillGroupsDataGrid();

            }
        }
        private void FillGroupsDataGrid()
        {
            // Очищаем коллекцию перед загрузкой новых данных
            displayedGroups.Clear();


            // Выводим только записи для текущей страницы
            for (int i = 0; i < allGroup.Count; i++)
            {
                displayedGroups.Add(allGroup[i]);
            }
            GroupsDataGrid.ItemsSource = displayedGroups;
        }

        private void LoadStudents()
        {
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                StudentsDataGrid.ItemsSource = null;

                // Формируем условия фильтрации
                List<string> conditions = new List<string>();
                List<SqlParameter> parameters = new List<SqlParameter>();

                if (GroupsDataGrid.SelectedItem is Group selectedGroup)
                {
                    // Фильтрация по группе
                    string groupCondition = "U.ID_GROUP = @GroupFilter";
                    conditions.Add(groupCondition);
                    parameters.Add(new SqlParameter("@GroupFilter", Convert.ToInt32(selectedGroup.GroupID)));
                }

                // Составляем запрос с учетом всех условий
                string query = "SELECT U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.Hostel, U.ID_GROUP, U.DateEnrollment, G.GroupName " +
                               "FROM [dbo].[USER] U " +
                               "INNER JOIN [GROUP] G ON U.ID_GROUP = G.ID_GROUP " +
                               "WHERE 1=1";

                foreach (string condition in conditions)
                {
                    query += " AND " + condition;
                }

                // Выполняем SQL-запрос с параметрами
                DataTable dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

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
                        DateBirth = ParseAndFormatDate(row["DateBirth"]),
                        Hostel = row["Hostel"].ToString(),
                        Group = row["GroupName"].ToString(),
                        DateEnrollment = ParseAndFormatDate(row["DateEnrollment"]),
                    });
                }

                // Обновляем общее количество записей и количество страниц
                totalRecords = allStudents.Count;

                // Заполняем DataGrid новыми данными и обновляем кнопки страниц
                FillStudentsDataGrid();
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

        private void FillStudentsDataGrid()
        {
            // Очищаем коллекцию перед загрузкой новых данных
            displayedStudents.Clear();


            // Используем using для автоматического вызова Dispose()
            using (StudentsDataLoader studentsDataLoader = new StudentsDataLoader())
            {
                // Выводим только записи для текущей страницы
                for (int i = 0; i < allStudents.Count; i++)
                {
                    displayedStudents.Add(allStudents[i]);
                }
                StudentsDataGrid.ItemsSource = displayedStudents;
            }
        }

        private void ButtonPage_Click(object sender, RoutedEventArgs e)
        {
            // Обработка нажатия на кнопку страницы и загрузка соответствующей страницы
            if (sender is Button clickedButton)
            {
                if (int.TryParse(clickedButton.Content.ToString(), out int selectedPage))
                {
                    FillGroupsDataGrid();
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
                    var sql = "DELETE FROM [GROUP] WHERE ID_GROUP = @ID_GROUP";
                    // Подготовка команды SQL
                    parameters.Add(new SqlParameter("@ID_GROUP", studentID));
                    // Выполнение SQL-запроса
                    connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());
                    MessageBox.Show("Студент успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении студента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadGroups();
        }

        // Обработчик события для кнопки "Удалить запись"
        private void ButtonDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (GroupsDataGrid.SelectedItem is Group selectedGroup)
            {
                // Удаляем выбранного студента из базы данных
                DeleteStudent(selectedGroup.GroupID);
                // Обновляем отображение после удаления
                //RefreshDataGrid();
            }
        }

        private void ButtonPreviousPage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonNextPage_Click(object sender, RoutedEventArgs e)
        {

        }

        public event EventHandler OpenAddGroupsRequested;

        private void ButtonAddStudent_Click(object sender, RoutedEventArgs e)
        {
            // Создаем форму для добавления нового студента
            PageAddOrChangeGroups addOrChangeGroups = new PageAddOrChangeGroups
            {
                AddOrChange = false
            };

            OpenAddGroupRequested?.Invoke(this, EventArgs.Empty);

        }

        private void GroupsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Очищаем коллекцию перед загрузкой новых данных
            allStudents.Clear();
            if (GroupsDataGrid.SelectedItem is Group selectedGroup)
            {
                textGroup.Text = "Студенты " + selectedGroup.GroupName;
            }

            // Загружаем студентов для выбранной группы
            LoadStudents();
        }

        public partial class Student
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

        private void StudentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBoxFilterSearchStudent_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Обработка изменения текста в TextBox для фильтрации студентов
            if (TextBoxFilterSearchGroup.Text == "")
            {
                TextBlockSearchStudent.Text = "Поиск по группе...";
                LoadGroups();
            }
            else
            {
                TextBlockSearchStudent.Text = "";
                ApplyFilters();
            }
        }
        public partial class Group
        {
            public string GroupID { get; set; }
            public string GroupName { get; set; }
            public string NumberPeople { get; set; }
        }

        // Обработчик события для кнопки "Редактировать запись"
        private void ButtonEditRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (GroupsDataGrid.SelectedItem is Group selectedGroup)
            {
                // Создаем экземпляр формы AddStudent
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    PageAddOrChangeGroups pageAddOrChangeGroups = new PageAddOrChangeGroups();

                    // Передаем данные выбранного студента в форму редактирования
                    pageAddOrChangeGroups.studentget(
                        int.Parse(selectedGroup.GroupID), selectedGroup.GroupName, Convert.ToInt32(selectedGroup.NumberPeople)
                    // Передайте остальные данные студента
                    );

                    // Переходим к форме редактирования студента
                    pageAddOrChangeGroups.AddOrChange = true;
                    NavigationService.Navigate(pageAddOrChangeGroups);
                }
            }
        }

        public void ApplyFilters()
        {
            // Создаем экземпляр ConnectionDataBase
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                GroupsDataGrid.ItemsSource = null;

                // Формируем условия фильтрации
                List<string> conditions = new List<string>();
                List<SqlParameter> parameters = new List<SqlParameter>();

                // Добавляем условия для текстового поиска
                string txtfilter = TextBoxFilterSearchGroup.Text;
                if (!string.IsNullOrEmpty(txtfilter))
                {
                    string searchCondition = "GroupName LIKE @Filter ";
                    conditions.Add(searchCondition);
                    parameters.Add(new SqlParameter("@Filter", "%" + txtfilter + "%"));
                }

                // Составляем запрос с учетом всех условий
                string query = "SELECT ID_GROUP, GroupName, NumberPeople " +
                               "FROM [dbo].[GROUP] " +
                               "WHERE 1=1";

                foreach (string condition in conditions)
                {
                    query += " AND " + condition;
                }

                // Выполняем SQL-запрос с параметрами
                DataTable dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

                // Заполняем коллекцию студентов данными из запроса
                allGroup.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    allGroup.Add(new Group
                    {
                        GroupID = row["ID_GROUP"].ToString(),
                        GroupName = row["GroupName"].ToString(),
                        NumberPeople = row["NumberPeople"].ToString(),
                    });
                }

                // Обновляем общее количество записей и количество страниц
                totalRecords = allGroup.Count;

                // Заполняем DataGrid новыми данными и обновляем кнопки страниц
                FillGroupsDataGrid();
            }
        }

        string queryGroup = "SELECT ID_GROUP AS [Иденификационный номер], GroupName AS Группа, NumberPeople AS [Кол-во человек]" +
               "FROM [dbo].[GROUP] " +
               "WHERE 1=1";

        private void ButtonExportDock_Click(object sender, RoutedEventArgs e)
        {
            ExportWord exportWord = new ExportWord();
            exportWord.export(queryGroup, "f");
        }

        private void ButtonExportExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportExcel exportExcel = new ExportExcel();
            exportExcel.export(queryGroup);
        }
    }
}
