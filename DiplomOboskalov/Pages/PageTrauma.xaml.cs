using DiplomLibrary;
using DiplomLibrary.DataBase;
using DiplomLibrary.Export;
using MahApps.Metro.IconPacks;
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
    /// Логика взаимодействия для PageTrauma.xaml
    /// </summary>
    public partial class PageTrauma : Page
    {
        DataTable dt;
        public ObservableCollection<Trauma> AllTraumas
        {
            get { return allTraumas; }
        }

        private readonly ObservableCollection<Trauma> allTraumas = new ObservableCollection<Trauma>();

        // Коллекция студентов, отображаемых на текущей странице
        private readonly ObservableCollection<Trauma> displayedTraumas;




        public ObservableCollection<Student> AllStudents
        {
            get { return allStudents; }
        }

        // Список групп, доступных для фильтрации
        public List<Student> Students { get; set; }
        public List<Trauma> Traumas { get; set; }


        // Событие, срабатывающее при запросе на открытие окна добавления студента
        public event EventHandler OpenAddTraumaRequested;

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


        public PageTrauma()
        {
            // Инициализация коллекций и компонентов страницы
            allStudents = new ObservableCollection<Student>();
            displayedStudents = new ObservableCollection<Student>();
            Students = new List<Student>();

            allTraumas = new ObservableCollection<Trauma>();
            displayedTraumas = new ObservableCollection<Trauma>();
            Traumas = new List<Trauma>();

            InitializeComponent();
            FillTraumasDataGrid();
            LoadStudents();
            LoadTrauma();
            UpdatePageButtons();

            ComboBoxStudent.SelectedIndex = 0;
        }
        private void LoadTrauma()
        {
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                TraumaDataGrid.ItemsSource = null;

                // Формируем условия фильтрации
                List<string> conditions = new List<string>();
                List<SqlParameter> parameters = new List<SqlParameter>();

                string txtfilter = TextBoxFilterSearchTrauma.Text;
                if (!string.IsNullOrEmpty(txtfilter))
                {
                    string searchCondition = "T.Description LIKE @Filter ";
                    conditions.Add(searchCondition);
                    parameters.Add(new SqlParameter("@Filter", "%" + txtfilter + "%"));
                }

                // Запрос к базе данных для получения данных о группах
                string query = "SELECT T.ID_TRAUMA, T.Description, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                               "FROM TRAUMA T " +
                               "INNER JOIN [USER] U ON T.ID_USER = U.ID_USER " +
                               "WHERE 1=1 ";

                foreach (string condition in conditions)
                {
                    query += " AND " + condition;
                }

                dt = connectionDb.ExecuteSqlParameters(query, parameters.ToArray());

                // Очищаем предыдущие данные в коллекции групп
                Traumas.Clear();

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in dt.Rows)
                {
                    Traumas.Add(new Trauma
                    {
                        ID = row["ID_TRAUMA"].ToString(),
                        NameTrauma = row["Description"].ToString(),
                        Students = row["FIO"].ToString()
                    });
                }
                totalRecords = allStudents.Count;
                totalPages = (int)Math.Ceiling((double)totalRecords / recordsPerPage);
                FillTraumasDataGrid();

            }
        }

        private void LoadStudents()
        {
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                TraumaDataGrid.ItemsSource = null;

                // Формируем условия фильтрации
                List<string> conditions = new List<string>();
                List<SqlParameter> parameters = new List<SqlParameter>();

                string txtfilter = TextBoxFilterSearchTrauma.Text;
                if (!string.IsNullOrEmpty(txtfilter))
                {
                    string searchCondition = "U.Surname LIKE @Filter OR U.Name LIKE @Filter OR U.Patronymic LIKE @Filter ";
                    conditions.Add(searchCondition);
                    parameters.Add(new SqlParameter("@Filter", "%" + txtfilter + "%"));
                }

                // Составляем запрос с учетом всех условий
                string query = "SELECT T.ID_TRAUMA, T.Description, U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.Hostel, U.ID_GROUP, U.DateEnrollment " +
                               "FROM TRAUMA T " +
                               "INNER JOIN [USER] U ON T.ID_USER = U.ID_USER " + // Пробел добавлен здесь
                               "WHERE 1=1 ";

                foreach (string condition in conditions)
                {
                    query += " AND " + condition;
                }

                // Выполняем SQL-запрос с параметрами
                dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

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
                        DateEnrollment = ParseAndFormatDate(row["DateEnrollment"]),
                    });
                    // Удалите ItemsSource
                    ComboBoxStudent.ItemsSource = null;

                    // Добавьте элементы через Items
                    ComboBoxStudent.Items.Clear();
                    ComboBoxStudent.Items.Add("Все студенты");

                    foreach (var studentName in allStudents.Select(s => s.FirstName + ' ' + s.MiddleName + ' ' + s.LastName).Distinct())
                    {
                        ComboBoxStudent.Items.Add(studentName);
                    }
                }

                // Обновляем общее количество записей и количество страниц
                totalRecords = allStudents.Count;
                totalPages = (int)Math.Ceiling((double)totalRecords / recordsPerPage);
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

        private void FillTraumasDataGrid()
        {
            // Очищаем коллекцию перед загрузкой новых данных
            displayedTraumas.Clear();

            // Определите начальный и конечный индексы для отображения записей на текущей странице
            startIndex = (currentPage - 1) * recordsPerPage;
            endIndex = startIndex + recordsPerPage;

            for (int i = startIndex; i < endIndex && i < Traumas.Count; i++)
            {
                displayedTraumas.Add(Traumas[i]);
            }
            TraumaDataGrid.ItemsSource = displayedTraumas;
        }

        private void ButtonPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            LoadPreviousPage();
        }

        private void ButtonNextPage_Click(object sender, RoutedEventArgs e)
        {
            LoadNextPage();
        }

        private void ButtonAddStudent_Click(object sender, RoutedEventArgs e)
        {
            // Создаем форму для добавления нового студента
            PageAddOrChangeTrauma pageAddOrChangeTrauma = new PageAddOrChangeTrauma
            {
                AddOrChange = false
            };
            NavigationService.Navigate(pageAddOrChangeTrauma);
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

        public partial class Trauma
        {
            public string ID { get; set; }
            public string NameTrauma { get; set; }
            public string Students { get; set; }
        }

        private void StudentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBoxStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxStudent.SelectedItem == "Все студенты")
            {
                LoadTrauma();
            }
            else
            {
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    // Очищаем текущее содержимое DataGrid
                    TraumaDataGrid.ItemsSource = null;

                    // Формируем условия фильтрации
                    List<string> conditions = new List<string>();
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    string selectedStudent = Convert.ToString(ComboBoxStudent.SelectedItem);
                    // Фильтрация по группе
                    string groupCondition = "U.Surname + ' ' + U.Name + ' ' + U.Patronymic = @GroupFilter";
                    conditions.Add(groupCondition);
                    parameters.Add(new SqlParameter("@GroupFilter", selectedStudent));

                    // Составляем запрос с учетом всех условий
                    string query = "SELECT T.ID_TRAUMA, T.Description, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                                   "FROM TRAUMA T " +
                                   "INNER JOIN [USER] U ON T.ID_USER = U.ID_USER " +
                                   "WHERE 1=1 ";

                    foreach (string condition in conditions)
                    {
                        query += " AND " + condition;
                    }

                    // Выполняем SQL-запрос с параметрами
                    dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

                    // Заполняем коллекцию студентов данными из запроса
                    // Очищаем предыдущие данные в коллекции групп
                    Traumas.Clear();

                    // Добавляем данные о группах в коллекцию Groups
                    foreach (DataRow row in dt.Rows)
                    {
                        Traumas.Add(new Trauma
                        {
                            ID = row["ID_TRAUMA"].ToString(),
                            NameTrauma = row["Description"].ToString(),
                            Students = row["FIO"].ToString()
                        });
                    }
                    totalRecords = Traumas.Count;
                    FillTraumasDataGrid();
                }

            }
        }

        private void TextBoxFilterSearchTrauma_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxFilterSearchTrauma.Text == "")
            {
                TextBlockSearchTrauma.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockSearchTrauma.Visibility = Visibility.Collapsed;

            }

            LoadTrauma();
        }

        // Обработчик события для кнопки "Удалить запись"
        private void ButtonDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (TraumaDataGrid.SelectedItem is Trauma selectedTrauma)
            {
                // Удаляем выбранного студента из базы данных
                DeleteStudent(selectedTrauma.ID);
                // Обновляем отображение после удаления
                //RefreshDataGrid();
            }
        }
        
        private void DeleteStudent(string studentID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            try
            {
                using (ConnectionDataBase connectionDb = new ConnectionDataBase())
                {
                    // Используйте параметры в запросе
                    var sql = "DELETE FROM [TRAUMA] WHERE ID_TRAUMA = @ID_TRAUMA";
                    // Подготовка команды SQL
                    parameters.Add(new SqlParameter("@ID_TRAUMA", studentID));
                    // Выполнение SQL-запроса
                    connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());
                    MessageBox.Show("Студент успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении студента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadTrauma();
        }

        // Обработчик события для кнопки "Редактировать запись"
        private void ButtonEditRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (TraumaDataGrid.SelectedItem is Trauma selectedTrauma)
            {
                // Создаем экземпляр формы AddStudent
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    PageAddOrChangeTrauma pageAddOrChangeTrauma = new PageAddOrChangeTrauma();

                    // Передаем данные выбранного студента в форму редактирования
                    pageAddOrChangeTrauma.studentget(
                        int.Parse(selectedTrauma.ID), selectedTrauma.NameTrauma, selectedTrauma.Students
                    // Передайте остальные данные студента
                    );

                    // Переходим к форме редактирования студента
                    pageAddOrChangeTrauma.AddOrChange = true;
                    NavigationService.Navigate(pageAddOrChangeTrauma);
                }
            }
        }

        string queryTrauma = "SELECT U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS Студент, T.Description AS Описание " +
                             "FROM TRAUMA T " +
                             "INNER JOIN [USER] U ON T.ID_USER = U.ID_USER";

        private void ButtonExportDock_Click(object sender, RoutedEventArgs e)
        {
            ExportWord exportWord = new ExportWord();
            exportWord.ExportToWord(dt);

        }

        private void ButtonExportExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportExcel exportExcel = new ExportExcel();
            exportExcel.ExportToExcel(dt);

        }
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

        // Обработчик события для кнопки "Страница"
        private void ButtonPage_Click(object sender, RoutedEventArgs e)
        {
            // Обработка нажатия на кнопку страницы и загрузка соответствующей страницы
            if (sender is Button clickedButton)
            {
                if (int.TryParse(clickedButton.Content.ToString(), out int selectedPage))
                {
                    currentPage = selectedPage;
                    FillTraumasDataGrid();
                    UpdatePageButtons();
                }
            }
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
            FillTraumasDataGrid();
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
            FillTraumasDataGrid();
            UpdatePageButtons();
        }

    }
}
