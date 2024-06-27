using DiplomLibrary;
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
    /// Логика взаимодействия для PagePreventiveVaccine.xaml
    /// </summary>
    public partial class PagePreventiveVaccine : Page
    {
        DataTable dt;

        public ObservableCollection<PreventieVaccine> AllPreventieVaccines
        {
            get { return allPreventieVaccines; }
        }

        private readonly ObservableCollection<PreventieVaccine> allPreventieVaccines = new ObservableCollection<PreventieVaccine>();

        // Коллекция студентов, отображаемых на текущей странице
        private readonly ObservableCollection<PreventieVaccine> displayedPreventieVaccines;




        public ObservableCollection<Student> AllStudents
        {
            get { return allStudents; }
        }

        // Список групп, доступных для фильтрации
        public List<Student> Students { get; set; }
        public List<PreventieVaccine> PreventieVaccines { get; set; }


        // Событие, срабатывающее при запросе на открытие окна добавления студента
        public event EventHandler OpenAddTraumaRequested;

        // Коллекция всех студентов из базы данных
        private readonly ObservableCollection<Student> allStudents = new ObservableCollection<Student>();

        // Коллекция студентов, отображаемых на текущей странице
        private readonly ObservableCollection<Student> displayedStudents;

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

        public PagePreventiveVaccine()
        {
            allStudents = new ObservableCollection<Student>();
            displayedStudents = new ObservableCollection<Student>();
            Students = new List<Student>();

            allPreventieVaccines = new ObservableCollection<PreventieVaccine>();
            displayedPreventieVaccines = new ObservableCollection<PreventieVaccine>();
            PreventieVaccines = new List<PreventieVaccine>();

            InitializeComponent();
            LoadStudents();
            LoadPreventieVaccine();
            UpdatePageButtons();

            ComboBoxStudent.SelectedIndex = 0;
        }
        private void LoadPreventieVaccine()
        {
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                PreventiveDataGrid.ItemsSource = null;

                // Формируем условия фильтрации
                //List<string> conditions = new List<string>();
                //List<SqlParameter> parameters = new List<SqlParameter>();

                // Запрос к базе данных для получения данных о группах
                string query = "SELECT PV.ID_PREVENTIVE_VACCINE, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO, MO.Name AS Med, MO.ID_MEDICAL_ORGANIZATION, PV.Vaccine, PV.Date " +
                               "FROM PREVENTIVE_VACCINE PV " +
                               "INNER JOIN [USER] U ON PV.ID_USER = U.ID_USER " +
                               "INNER JOIN [MEDICAL_ORGANIZATION] MO ON PV.ID_MEDICAL_ORGANIZATION = MO.ID_MEDICAL_ORGANIZATION " +
                               "WHERE 1=1 ";

                //foreach (string condition in conditions)
                //{
                //    query += " AND " + condition;
                //}

                //DataTable dt = connectionDb.ExecuteSqlParameters(query, parameters.ToArray());
                DataTable dt = connectionDb.ExecuteSql(query);

                // Очищаем предыдущие данные в коллекции групп
                PreventieVaccines.Clear();

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in dt.Rows)
                {
                    PreventieVaccines.Add(new PreventieVaccine
                    {
                        ID_PREVENTIVE_VACCINE = row["ID_PREVENTIVE_VACCINE"].ToString(),
                        USER = row["FIO"].ToString(),
                        MEDICAL_ORGANIZATION = row["Med"].ToString(),
                        Vaccine = row["Vaccine"].ToString(),
                        Date = ParseAndFormatDate(row["Date"])
                    });
                }
                totalRecords = PreventieVaccines.Count;
                totalPages = (int)Math.Ceiling((double)totalRecords / recordsPerPage);
                FillPreventieVaccineDataGrid();

            }
        }

        private void LoadStudents()
        {
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                PreventiveDataGrid.ItemsSource = null;

                // Формируем условия фильтрации
                //List<string> conditions = new List<string>();
                //List<SqlParameter> parameters = new List<SqlParameter>();

                //string txtfilter = TextBoxFilterSearchTrauma.Text;
                //if (!string.IsNullOrEmpty(txtfilter))
                //{
                //    string searchCondition = "U.Surname LIKE @Filter OR U.Name LIKE @Filter OR U.Patronymic LIKE @Filter ";
                //    conditions.Add(searchCondition);
                //    parameters.Add(new SqlParameter("@Filter", "%" + txtfilter + "%"));
                //}

                // Составляем запрос с учетом всех условий
                string query = "SELECT PV.ID_PREVENTIVE_VACCINE, U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.Hostel, U.ID_GROUP, U.DateEnrollment, MO.Name AS Med, PV.Vaccine, PV.Date " +
                               "FROM PREVENTIVE_VACCINE PV " +
                               "INNER JOIN [USER] U ON PV.ID_USER = U.ID_USER " +
                               "INNER JOIN [MEDICAL_ORGANIZATION] MO ON PV.ID_MEDICAL_ORGANIZATION = MO.ID_MEDICAL_ORGANIZATION " +
                               "WHERE 1=1 ";


                //foreach (string condition in conditions)
                //{
                //    query += " AND " + condition;
                //}

                // Выполняем SQL-запрос с параметрами
                //DataTable dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());
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
                        DateBirth = ParseAndFormatDate(row["DateBirth"]),
                        Hostel = row["Hostel"].ToString(),
                        DateEnrollment = ParseAndFormatDate(row["DateEnrollment"]),
                    });
                    //Удалите ItemsSource
                    ComboBoxStudent.ItemsSource = null;

                    // Добавьте элементы через Items
                    ComboBoxStudent.Items.Clear();
                    ComboBoxStudent.Items.Add("Все студенты");

                    foreach (var studentName in allStudents.Select(s => s.FirstName + ' ' + s.MiddleName + ' ' + s.LastName).Distinct())
                    {
                        ComboBoxStudent.Items.Add(studentName);
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

        private void FillPreventieVaccineDataGrid()
        {
            // Очищаем коллекцию перед загрузкой новых данных
            displayedPreventieVaccines.Clear();


            // Выводим только записи для текущей страницы
            for (int i = 0; i < PreventieVaccines.Count; i++)
            {
                displayedPreventieVaccines.Add(PreventieVaccines[i]);
            }
            PreventiveDataGrid.ItemsSource = displayedPreventieVaccines;
        }

        private void ButtonAddStudent_Click(object sender, RoutedEventArgs e)
        {
            // Создаем форму для добавления нового студента
            PageAddOrChangePreventiveVaccine pageAddOrChangePreventiveVaccine = new PageAddOrChangePreventiveVaccine
            {
                AddOrChange = false
            };
            NavigationService.Navigate(pageAddOrChangePreventiveVaccine);
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

        public partial class PreventieVaccine
        {
            public string ID_PREVENTIVE_VACCINE { get; set; }
            public string MEDICAL_ORGANIZATION { get; set; }
            public string USER { get; set; }
            public string Vaccine { get; set; }
            public string Date { get; set; }

        }
        // Обработчик события для кнопки "Редактировать запись"
        private void ButtonEditRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (PreventiveDataGrid.SelectedItem is PreventieVaccine selectedPreventieVaccine)
            {
                // Создаем экземпляр формы AddStudent
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    PageAddOrChangePreventiveVaccine pageAddOrChangePreventiveVaccine = new PageAddOrChangePreventiveVaccine();

                    // Передаем данные выбранного студента в форму редактирования
                    pageAddOrChangePreventiveVaccine.studentget(
                        int.Parse(selectedPreventieVaccine.ID_PREVENTIVE_VACCINE), selectedPreventieVaccine.USER, selectedPreventieVaccine.MEDICAL_ORGANIZATION, selectedPreventieVaccine.Vaccine, Convert.ToDateTime(selectedPreventieVaccine.Date)
                    // Передайте остальные данные студента
                    );

                    // Переходим к форме редактирования студента
                    pageAddOrChangePreventiveVaccine.AddOrChange = true;
                    NavigationService.Navigate(pageAddOrChangePreventiveVaccine);
                }
            }
        }

        private void StudentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
                    var sql = "DELETE FROM [PREVENTIVE_VACCINE] WHERE ID_PREVENTIVE_VACCINE = @ID_PREVENTIVE_VACCINE";
                    // Подготовка команды SQL
                    parameters.Add(new SqlParameter("@ID_PREVENTIVE_VACCINE", studentID));
                    // Выполнение SQL-запроса
                    connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());
                    MessageBox.Show("Студент успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении студента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadPreventieVaccine();
        }

        // Обработчик события для кнопки "Удалить запись"
        private void ButtonDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (PreventiveDataGrid.SelectedItem is PreventieVaccine selectedPreventieVaccine)
            {
                // Удаляем выбранного студента из базы данных
                DeleteStudent(selectedPreventieVaccine.ID_PREVENTIVE_VACCINE);
                // Обновляем отображение после удаления
                //RefreshDataGrid();
            }
        }

        private void ComboBoxStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxStudent.SelectedItem == "Все студенты")
            {
                LoadPreventieVaccine();
            }
            else
            {
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    // Очищаем текущее содержимое DataGrid
                    PreventiveDataGrid.ItemsSource = null;

                    // Формируем условия фильтрации
                    List<string> conditions = new List<string>();
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    string selectedStudent = Convert.ToString(ComboBoxStudent.SelectedItem);
                    // Фильтрация по группе
                    string groupCondition = "U.Surname + ' ' + U.Name + ' ' + U.Patronymic = @GroupFilter";
                    conditions.Add(groupCondition);
                    parameters.Add(new SqlParameter("@GroupFilter", selectedStudent));

                    // Составляем запрос с учетом всех условий
                    string query = "SELECT PV.ID_PREVENTIVE_VACCINE, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO, MO.ID_MEDICAL_ORGANIZATION, U.DateBirth, U.Hostel, U.ID_GROUP, U.DateEnrollment, MO.Name AS Med, PV.Vaccine, PV.Date " +
                                   "FROM PREVENTIVE_VACCINE PV " +
                                   "INNER JOIN [USER] U ON PV.ID_USER = U.ID_USER " +
                                   "INNER JOIN[MEDICAL_ORGANIZATION] MO ON PV.ID_MEDICAL_ORGANIZATION = MO.ID_MEDICAL_ORGANIZATION " +
                                   "WHERE 1=1 ";


                    foreach (string condition in conditions)
                    {
                        query += " AND " + condition;
                    }

                    // Выполняем SQL-запрос с параметрами
                    DataTable dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

                    // Заполняем коллекцию студентов данными из запроса
                    // Очищаем предыдущие данные в коллекции групп
                    PreventieVaccines.Clear();

                    // Добавляем данные о группах в коллекцию Groups
                    foreach (DataRow row in dt.Rows)
                    {
                        PreventieVaccines.Add(new PreventieVaccine
                        {
                            ID_PREVENTIVE_VACCINE = row["ID_PREVENTIVE_VACCINE"].ToString(),
                            USER = row["FIO"].ToString(),
                            MEDICAL_ORGANIZATION = row["Med"].ToString(),
                            Vaccine = row["Vaccine"].ToString(),
                            Date = ParseAndFormatDate(row["Date"])
                        });
                    }
                    totalRecords = PreventieVaccines.Count;
                    FillPreventieVaccineDataGrid();
                }

            }
        }

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

        private void ButtonPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            LoadPreviousPage();
        }

        private void ButtonNextPage_Click(object sender, RoutedEventArgs e)
        {
            LoadNextPage();
        }
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
            LoadPreventieVaccine();
            UpdatePageButtons();
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
                    LoadPreventieVaccine();
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
            LoadPreventieVaccine();
            UpdatePageButtons();
        }
    }
}
