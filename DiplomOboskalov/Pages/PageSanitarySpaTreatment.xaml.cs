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
using static DiplomOboskalov.Pages.PageHospitalTreatment;
using DiplomLibrary.Export;
using MahApps.Metro.IconPacks;

namespace DiplomOboskalov.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageSanitarySpaTreatment.xaml
    /// </summary>
    public partial class PageSanitarySpaTreatment : Page
    {
        DataTable dt;

        public PageSanitarySpaTreatment()
        {
            ListSanitarySpaTreatment = new List<SanitarySpaTreatmentInfo>();
            ListStudents = new List<StudentInfo>();

            InitializeComponent();

            LoadStudents();
            LoadSanitarySpaTreatment();
            UpdatePageButtons();

            ComboBoxStudent.SelectedIndex = 0;
        }
        public List<SanitarySpaTreatmentInfo> ListSanitarySpaTreatment { get; set; }
        public List<StudentInfo> ListStudents { get; set; }

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


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------


        private void LoadSanitarySpaTreatment()
        {
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                SanitarySpaTreatmentDataGrid.ItemsSource = null;

                // Запрос к базе данных для получения данных о группах
                string query = "SELECT SST.ID_SANITARY_SPA_TREATMENT, SST.Description, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                               "FROM SANITARY_SPA_TREATMENT SST " +
                               "INNER JOIN [USER] U ON SST.ID_USER = U.ID_USER " +
                               "WHERE 1=1 ";

                dt = connectionDb.ExecuteSql(query);

                // Очищаем предыдущие данные в коллекции групп
                ListSanitarySpaTreatment.Clear();

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in dt.Rows)
                {
                    ListSanitarySpaTreatment.Add(new SanitarySpaTreatmentInfo
                    {
                        ID_SANITARY_SPA_TREATMENT = row["ID_SANITARY_SPA_TREATMENT"].ToString(),
                        ID_USER = row["FIO"].ToString(),
                        Description = row["Description"].ToString()
                    });
                }
                totalRecords = ListSanitarySpaTreatment.Count;
                totalPages = (int)Math.Ceiling((double)totalRecords / recordsPerPage);

                SanitarySpaTreatmentDataGrid.ItemsSource = ListSanitarySpaTreatment;
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

                dt = connectionDataBase.ExecuteSql(query);

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
            // Получаем выбранного студента из DataGrid
            if (SanitarySpaTreatmentDataGrid.SelectedItem is SanitarySpaTreatmentInfo selected)
            {
                // Создаем экземпляр формы AddStudent
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    PageAddOrChangeSanitarySpaTreatment page = new PageAddOrChangeSanitarySpaTreatment();

                    // Передаем данные выбранного студента в форму редактирования
                    page.get(
                        int.Parse(selected.ID_SANITARY_SPA_TREATMENT), selected.ID_USER, selected.Description);

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
                    var sql = "DELETE FROM [SANITARY_SPA_TREATMENT] WHERE ID_SANITARY_SPA_TREATMENT = @ID_SANITARY_SPA_TREATMENT";
                    // Подготовка команды SQL
                    parameters.Add(new SqlParameter("@ID_SANITARY_SPA_TREATMENT", _ID));
                    // Выполнение SQL-запроса
                    connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());
                    MessageBox.Show("Студент успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadStudents();
                    LoadSanitarySpaTreatment();
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
            if (SanitarySpaTreatmentDataGrid.SelectedItem is SanitarySpaTreatmentInfo selected)
            {
                // Удаляем выбранного студента из базы данных
                DeleteRow(selected.ID_SANITARY_SPA_TREATMENT);
                // Обновляем отображение после удаления
                //RefreshDataGrid();
            }
        }

        private void ComboBoxStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxStudent.SelectedItem == "Все студенты")
            {
                LoadSanitarySpaTreatment();
            }
            else
            {
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    SanitarySpaTreatmentDataGrid.ItemsSource = null;

                    List<string> conditions = new List<string>();
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    string selectedStudent = Convert.ToString(ComboBoxStudent.SelectedItem);
                    string groupCondition = "U.Surname + ' ' + U.Name + ' ' + U.Patronymic = @GroupFilter";
                    conditions.Add(groupCondition);
                    parameters.Add(new SqlParameter("@GroupFilter", selectedStudent));

                    string query = "SELECT SST.ID_SANITARY_SPA_TREATMENT, SST.Description, U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic AS FIO " +
                                   "FROM SANITARY_SPA_TREATMENT SST " +
                                   "INNER JOIN [USER] U ON SST.ID_USER = U.ID_USER " +
                                   "WHERE 1=1 ";


                    foreach (string condition in conditions)
                    {
                        query += " AND " + condition;
                    }

                    dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

                    ListSanitarySpaTreatment.Clear();

                    foreach (DataRow row in dt.Rows)
                    {
                        ListSanitarySpaTreatment.Add(new SanitarySpaTreatmentInfo
                        {
                            ID_SANITARY_SPA_TREATMENT = row["ID_SANITARY_SPA_TREATMENT"].ToString(),
                            ID_USER = row["FIO"].ToString(),
                            Description = row["Description"].ToString()
                        });
                    }
                    totalRecords = ListSanitarySpaTreatment.Count;
                    totalPages = (int)Math.Ceiling((double)totalRecords / recordsPerPage);
                }
                SanitarySpaTreatmentDataGrid.ItemsSource = ListSanitarySpaTreatment;
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
            LoadSanitarySpaTreatment();
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
                    LoadSanitarySpaTreatment();
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
            LoadSanitarySpaTreatment();
            UpdatePageButtons();
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

        public partial class SanitarySpaTreatmentInfo
        {
            public string ID_SANITARY_SPA_TREATMENT { get; set; }
            public string ID_USER { get; set; }
            public string Description { get; set; }
        }

    }
}