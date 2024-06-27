using DiplomLibrary;
using DiplomLibrary.DataBase;
using DiplomLibrary.Export;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DiplomOboskalov.Pages
{
    public partial class PageStudents : Page
    {
        string queryStudent = "SELECT U.ID_USER, U.Surname, U.Name, U.Patronymic, FORMAT(U.DateBirth, 'dd.MM.yyyy') as DateBirth, U.HomeAddress, U.Hostel, U.ID_GROUP, FORMAT(U.DateEnrollment, 'dd.MM.yyyy') as DateEnrollment, G.GroupName, U.Color " +
               "FROM [dbo].[USER] U " +
               "INNER JOIN [GROUP] G ON U.ID_GROUP = G.ID_GROUP";

        private readonly StudentsDataLoader studentsDataLoader = new StudentsDataLoader();

        public ObservableCollection<Student> AllStudents
        {
            get { return allStudents; }
        }

        // Список групп, доступных для фильтрации
        public List<GroupItem> Groups { get; set; }

        // Список общежитий, доступных для фильтрации
        public List<HostelItem> Hostels { get; set; }

        // Выбранная группа для фильтрации
        public GroupItem SelectedGroup { get; set; }

        // Выбранное общежитие для фильтрации
        public HostelItem SelectedHostel { get; set; }

        // Событие, срабатывающее при запросе на открытие окна добавления студента
        public event EventHandler OpenAddStudentRequested;

        // Коллекция всех студентов из базы данных
        private readonly ObservableCollection<Student> allStudents = new ObservableCollection<Student>();

        // Коллекция студентов, отображаемых на текущей странице
        private readonly ObservableCollection<Student> displayedStudents;

        // Количество записей на одной странице
        private readonly int recordsPerPage = 11;

        // Текущая страница
        private int currentPage = 1;

        // Общее количество записей в базе данных
        private int totalRecords;

        // Общее количество страниц
        private int totalPages;

        // Индексы для отображения записей на текущей странице
        private int startIndex;
        private int endIndex;

        // Счетчик кликов для фильтрации
        private int countclick = 0;

        // Конструктор класса Students
        public PageStudents()
        {
            // Инициализация коллекций и компонентов страницы
            allStudents = new ObservableCollection<Student>();
            displayedStudents = new ObservableCollection<Student>();
            Groups = new List<GroupItem>();
            Hostels = new List<HostelItem>();
            InitializeComponent();

            // Загрузка данных о студентах, заполнение DataGrid и загрузка списков групп и общежитий
            FillStudentsDataGrid();
            LoadStudents();
            LoadGroups();
            LoadHostels();
        }

        // Load
        // Загрузка групп из базы данных и обновление списка групп для фильтрации
        private void LoadGroups()
        {
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
                    Groups.Add(new GroupItem
                    {
                        Group = row["GroupName"].ToString(),
                        GroupID = row["ID_GROUP"].ToString(),
                        Value = row["NumberPeople"].ToString()
                    });
                }

                // Устанавливаем ItemsSource для ComboBoxGroupFiltr для отображения групп в пользовательском интерфейсе
                ComboBoxGroupFiltr.ItemsSource = Groups;
            }
        }

        // Загрузка уникальных значений общежитий из базы данных (исключая NULL) и обновление списка общежитий для фильтрации
        private void LoadHostels()
        {
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Запрос к базе данных для получения уникальных значений общежитий (исключая NULL)
                string query = "SELECT DISTINCT Hostel FROM [USER] WHERE Hostel IS NOT NULL";
                DataTable result = connectionDb.ExecuteSql(query);

                // Очищаем предыдущие данные в коллекции общежитий
                Hostels.Clear();

                // Добавляем уникальные значения общежитий в коллекцию Hostels
                foreach (DataRow row in result.Rows)
                {
                    // Проверка на NULL перед добавлением в коллекцию
                    if (row["Hostel"] != DBNull.Value)
                    {
                        Hostels.Add(new HostelItem
                        {
                            Hostel = row["Hostel"].ToString()
                        });
                    }
                }

                // Устанавливаем ItemsSource для ComboBoxHostelFiltr для отображения общежитий в пользовательском интерфейсе
                ComboBoxHostelFiltr.ItemsSource = Hostels;
            }
        }

        // Загрузка данных о студентах из базы данных и обновление DataGrid и пагинации
        private void LoadStudents()
        {
            // Очищаем коллекцию перед загрузкой новых данных
            allStudents.Clear();

            // Загружаем студентов из базы данных
            List<Student> students = studentsDataLoader.GetStudents();

            // Добавляем студентов в ObservableCollection
            foreach (var student in students)
            {
                allStudents.Add(student);
            }

            totalRecords = allStudents.Count;
            totalPages = (int)Math.Ceiling((double)totalRecords / recordsPerPage);

            FillStudentsDataGrid();
            UpdatePageButtons();
        }




        // Загрузка предыдущей страницы данных
        private void LoadPreviousPage()
        {
            // Уменьшение текущего номера страницы на 1
            currentPage--;

            // Проверка на выход за пределы минимального номера страницы
            if (currentPage < 1)
            {
                currentPage = 1;
            }

            // Обновление DataGrid и элементов пагинации
            FillStudentsDataGrid();
            UpdatePageButtons();
        }

        // Загрузка следующей страницы данных
        private void LoadNextPage()
        {
            // Увеличение текущего номера страницы на 1
            currentPage++;

            // Проверка на выход за пределы максимального номера страницы
            if (currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            // Обновление DataGrid и элементов пагинации
            FillStudentsDataGrid();
            UpdatePageButtons();
        }




        // Fill
        // Заполнение DataGrid отображаемыми записями на текущей странице
        private void FillStudentsDataGrid()
        {
            // Очищаем коллекцию перед загрузкой новых данных
            displayedStudents.Clear();

            // Определите начальный и конечный индексы для отображения записей на текущей странице
            startIndex = (currentPage - 1) * recordsPerPage;
            endIndex = startIndex + recordsPerPage;

            // Используем using для автоматического вызова Dispose()
            using (StudentsDataLoader studentsDataLoader = new StudentsDataLoader())
            {
                // Выводим только записи для текущей страницы
                for (int i = startIndex; i < endIndex && i < allStudents.Count; i++)
                {
                    displayedStudents.Add(allStudents[i]);
                }
                StudentsDataGrid.ItemsSource = displayedStudents;
            }
        }




        // Button
        // Обработчик события для кнопки "Фильтр"
        private void ButtonFilterAll_Click(object sender, RoutedEventArgs e)
        {
            // Переключение значения countclick между 0 и 1 при каждом нажатии
            countclick = (countclick == 0) ? 1 : 0;

            if (countclick == 1)
            {
                // Показываем всплывающий элемент PopupGridFilterAll с анимацией появления
                PopupGridFilterAll.Visibility = Visibility.Visible;
                Storyboard fadeInStoryboard = (Storyboard)this.Resources["fadeInAnimation"];
                fadeInStoryboard.Begin();
            }
            else
            {
                // Скрываем всплывающий элемент PopupGridFilterAll с анимацией исчезновения
                PopupGridFilterAll.Visibility = Visibility.Collapsed;
                Storyboard fadeInStoryboard = (Storyboard)this.Resources["fadeOutAnimation"];
                fadeInStoryboard.Begin();
            }
        }

        // Обработчик события для кнопки "Поиск" в окне фильтрации
        private void ButtonFilterAllSearch_Click(object sender, RoutedEventArgs e)
        {
            // Применяем выбранные фильтры
            ApplyFilters();
        }

        // Обработчик события для кнопки "Сброс" фильтров
        private void ButtonRestore_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем выбранные фильтры и загружаем все студенты
            ComboBoxAgeFiltr.SelectedItem = null;
            ComboBoxGroupFiltr.SelectedItem = null;
            ComboBoxHostelFiltr.SelectedItem = null;
            LoadStudents();
            FillStudentsDataGrid();
        }

        // Обработчик события для кнопки "Следующая страница"
        private void ButtonNextPage_Click(object sender, RoutedEventArgs e)
        {
            // Загружаем следующую страницу с записями
            LoadNextPage();
        }

        // Обработчик события для кнопки "Предыдущая страница"
        private void ButtonPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            // Загружаем предыдущую страницу с записями
            LoadPreviousPage();
        }

        // Обработчик события для кнопки "Редактировать запись"
        private void ButtonEditRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (StudentsDataGrid.SelectedItem is Student selectedStudent)
            {
                // Создаем экземпляр формы AddStudent
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    PageAddOrChangeStudents addStudent = new PageAddOrChangeStudents();

                    // Передаем данные выбранного студента в форму редактирования
                    addStudent.studentget(
                        int.Parse(selectedStudent.ID), selectedStudent.FirstName,
                        selectedStudent.MiddleName, selectedStudent.LastName,
                        Convert.ToDateTime(selectedStudent.DateBirth), selectedStudent.Address,
                        selectedStudent.Hostel, Convert.ToString(selectedStudent.GroupIDID),
                        Convert.ToDateTime(selectedStudent.DateEnrollment),
                        selectedStudent.ImageName
                    // Передайте остальные данные студента
                    );

                    // Переходим к форме редактирования студента
                    addStudent.AddOrChange = true;
                    NavigationService.Navigate(addStudent);
                }
            }
        }

        // Обработчик события для кнопки "Добавить студента"
        private void ButtonAddStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем форму для добавления нового студента
                PageAddOrChangeStudents addStudent = new PageAddOrChangeStudents
                {
                    AddOrChange = false
                };

                // Вызываем событие, указывающее на запрос открытия формы добавления студента
                OpenAddStudentRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик события для кнопки "Удалить запись"
        private void ButtonDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранного студента из DataGrid
                if (StudentsDataGrid.SelectedItem is Student selectedStudent)
                {
                    // Удаляем выбранного студента из базы данных
                    DeleteStudent(selectedStudent.ID);
                    // Обновляем отображение после удаления
                    //RefreshDataGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик события для кнопки "Страница"
        private void ButtonPage_Click(object sender, RoutedEventArgs e)
        {
            // Обработка нажатия на кнопку страницы и загрузка соответствующей страницы
            if (sender is Button clickedButton)
            {
                if (int.TryParse(clickedButton.Content.ToString(), out int selectedPage))
                {
                    currentPage = selectedPage;
                    FillStudentsDataGrid();
                    UpdatePageButtons();
                }
            }
        }




        // TextBox
        // Обработчик изменения текста в поле поиска
        private void TextBoxFilterSearchStudent_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Обработка изменения текста в TextBox для фильтрации студентов
            if (TextBoxFilterSearchStudent.Text == "")
            {
                TextBlockSearchStudent.Text = "Поиск ...";
                FillStudentsDataGrid();
                LoadStudents();
                //studentsDataLoader.GetStudents();
                FillStudentsDataGrid();
            }
            else
            {
                TextBlockSearchStudent.Text = "";
                ApplyFilters();
            }
        }




        // Other
        // Применение выбранных фильтров
        public void ApplyFilters()
        {
            // Создаем экземпляр ConnectionDataBase
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                StudentsDataGrid.ItemsSource = null;

                // Формируем условия фильтрации
                List<string> conditions = new List<string>();
                List<SqlParameter> parameters = new List<SqlParameter>();

                if (ComboBoxAgeFiltr.SelectedItem is ComboBoxItem selectedAge)
                {
                    // Фильтрация по возрасту
                    string ageFilter = selectedAge.Content.ToString();
                    string ageCondition = (ageFilter == "Старше 18") ? "DATEDIFF(YEAR, U.DateBirth, GETDATE()) > 18" :
                                          (ageFilter == "Младше 18") ? "DATEDIFF(YEAR, U.DateBirth, GETDATE()) < 18" :
                                                                        ""; // Добавьте дополнительные условия, если необходимо
                    conditions.Add(ageCondition);
                }

                if (ComboBoxHostelFiltr.SelectedItem is HostelItem selectedHostel)
                {
                    // Фильтрация по общежитию
                    string hostelCondition = "U.Hostel = @HostelFilter";
                    conditions.Add(hostelCondition);
                    parameters.Add(new SqlParameter("@HostelFilter", selectedHostel.Hostel));
                }

                if (ComboBoxGroupFiltr.SelectedItem is GroupItem selectedGroup)
                {
                    // Фильтрация по группе
                    string groupCondition = "U.ID_GROUP = @GroupFilter";
                    conditions.Add(groupCondition);
                    parameters.Add(new SqlParameter("@GroupFilter", Convert.ToInt32(selectedGroup.GroupID)));
                }

                // Добавляем условия для текстового поиска
                string txtfilter = TextBoxFilterSearchStudent.Text;
                if (!string.IsNullOrEmpty(txtfilter))
                {
                    string searchCondition = "U.Surname LIKE @Filter OR U.Name LIKE @Filter OR U.Patronymic LIKE @Filter";
                    conditions.Add(searchCondition);
                    parameters.Add(new SqlParameter("@Filter", "%" + txtfilter + "%"));
                }

                // Составляем запрос с учетом всех условий
                string query = "SELECT U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.HomeAddress, U.Hostel, U.ID_GROUP, U.DateEnrollment, G.GroupName, U.Color " +
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
                        OneSimvol = NameInitialExtractor.ExtractInitial(row["Name"].ToString()),
                        DateBirth = ParseAndFormatDate(row["DateBirth"]),
                        Address = row["HomeAddress"].ToString(),
                        Hostel = row["Hostel"].ToString(),
                        Group = row["GroupName"].ToString(),
                        DateEnrollment = ParseAndFormatDate(row["DateEnrollment"]),
                        BgColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(row["Color"].ToString())),
                        GroupIDID = row["ID_GROUP"].ToString()
                    });
                }

                // Обновляем общее количество записей и количество страниц
                totalRecords = allStudents.Count;
                totalPages = (int)Math.Ceiling((double)totalRecords / recordsPerPage);

                // Заполняем DataGrid новыми данными и обновляем кнопки страниц
                FillStudentsDataGrid();
                UpdatePageButtons();
            }
        }

        // Парсинг и форматирование даты
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

        // Обновление кнопок страниц
        private void UpdatePageButtons()
        {
            NumberPage.Child = null; // Очистить существующие элементы

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); // Кнопка "Назад"
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Динамические кнопки страниц
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); // Кнопка "Вперед"

            // Создать кнопку "Назад"
            Button buttonPreviousPage = new Button
            {
                Content = "Назад",
                Style = FindResource("ButtonPageNavigation") as Style,
            };
            buttonPreviousPage.Click += ButtonPreviousPage_Click;

            // Добавить иконку к кнопке "Назад"
            PackIconMaterial iconPrevious = new PackIconMaterial
            {
                Kind = PackIconMaterialKind.ChevronLeft,
                Style = FindResource("IconButtonPageNavigation") as Style
            };
            buttonPreviousPage.Content = iconPrevious;

            Grid.SetColumn(buttonPreviousPage, 0);
            grid.Children.Add(buttonPreviousPage);

            // Динамические кнопки страниц
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            Grid.SetColumn(stackPanel, 1);

            // Определить диапазон отображаемых кнопок страниц
            int startPage = Math.Max(1, currentPage - 3);
            int endPage = Math.Min(totalPages, startPage + 7); // Ограничьте количество отображаемых кнопок

            // Создать кнопки для каждой страницы в диапазоне
            for (int i = startPage; i <= endPage; i++)
            {
                Button pageButton = new Button
                {
                    Content = i.ToString(),
                    Style = FindResource("ButtonPageNavigation") as Style
                };

                // Выделить текущую страницу
                if (i == currentPage)
                {
                    pageButton.Background = new SolidColorBrush(Color.FromRgb(13, 67, 202));
                    pageButton.Foreground = new SolidColorBrush(Colors.White);
                }

                // Привязать обработчик события к кнопке страницы
                pageButton.Click += ButtonPage_Click;
                stackPanel.Children.Add(pageButton);
            }
            grid.Children.Add(stackPanel);

            // Создать кнопку "Вперед"
            Button buttonNextPage = new Button
            {
                Content = "Вперед",
                Style = FindResource("ButtonPageNavigation") as Style,
            };
            buttonNextPage.Click += ButtonNextPage_Click;

            // Добавить иконку к кнопке "Вперед"
            PackIconMaterial iconNext = new PackIconMaterial
            {
                Kind = PackIconMaterialKind.ChevronRight,
                Style = FindResource("IconButtonPageNavigation") as Style
            };
            buttonNextPage.Content = iconNext;

            Grid.SetColumn(buttonNextPage, 2);
            grid.Children.Add(buttonNextPage);

            NumberPage.Child = grid;
        }

        // Удаление студента по ID
        private void DeleteStudent(string studentID)
        {
            try
            {
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    // Используйте параметры в запросе
                    var sql = $"DELETE FROM [USER] WHERE ID_USER = '{studentID}'";
                    // Подготовка команды SQL
                    DataTable result = connectionDataBase.ExecuteSql(sql);

                    MessageBox.Show("Студент успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении студента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            FillStudentsDataGrid();
        }

        private void ButtonExportDock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExportWord exportWord = new ExportWord();
                exportWord.export(queryStudent, "f");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExportExcel exportExcel = new ExportExcel();
                exportExcel.export(queryStudent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }




    // Classes
    public class NameInitialExtractor
    {
        public static string ExtractInitial(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                // Извлекаем первую букву и приводим ее к верхнему регистру
                return name[0].ToString().ToUpper();
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public class GroupItem
    {
        public string Group { get; set; }
        public string GroupID { get; set; }
        public string Value { get; set; }
    }
    public class HostelItem
    {
        public string Hostel { get; set; }
    }
    public partial class Student
    {
        public string OneSimvol { get; set; }
        public string ID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string DateBirth { get; set; }
        public string Address { get; set; }
        public string Hostel { get; set; }
        public string Group { get; set; }
        public string DateEnrollment { get; set; }
        public Brush BgColor { get; set; }
        public string ImageName { get; set; }
        public string GroupIDID { get; set; }
    }
}


//Есть защита от SQL-инъекций