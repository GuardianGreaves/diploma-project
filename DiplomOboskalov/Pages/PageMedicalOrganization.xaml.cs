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
using static DiplomOboskalov.Pages.PageMedicalOrganization;

namespace DiplomOboskalov.Pages
{
    public partial class PageMedicalOrganization : Page
    {
        private readonly ObservableCollection<MedicalOrganization> allMedicalOrganization = new ObservableCollection<MedicalOrganization>();

        // Коллекция студентов, отображаемых на текущей странице
        private readonly ObservableCollection<MedicalOrganization> displayedMedicalOrganizations;

        public ObservableCollection<MedicalOrganization> AllMedicalOrganization
        {
            get { return allMedicalOrganization; }
        }

        // Список групп, доступных для фильтрации
        public List<MedicalOrganization> MedicalOrganizations { get; set; }

        // Событие, срабатывающее при запросе на открытие окна добавления студента
        public event EventHandler OpenAddMedicalOrganizationRequested;

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

        public PageMedicalOrganization()
        {
            // Инициализация коллекций и компонентов страницы
            allMedicalOrganization = new ObservableCollection<MedicalOrganization>();
            displayedMedicalOrganizations = new ObservableCollection<MedicalOrganization>();
            MedicalOrganizations = new List<MedicalOrganization>();

            InitializeComponent();
            // Загрузка данных о студентах, заполнение DataGrid и загрузка списков групп и общежитий
            FillMedicalOrganizationsDataGrid();
            LoadMedicalOrganizations();
        }

        public void LoadMedicalOrganizations()
        {
            AllMedicalOrganization.Clear();
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Запрос к базе данных для получения данных о группах
                string query = "SELECT ID_MEDICAL_ORGANIZATION, Name, Address FROM [MEDICAL_ORGANIZATION]";
                DataTable result = connectionDb.ExecuteSql(query);

                // Очищаем предыдущие данные в коллекции групп
                MedicalOrganizations.Clear();

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in result.Rows)
                {
                    allMedicalOrganization.Add(new MedicalOrganization
                    {
                        ID = row["ID_MEDICAL_ORGANIZATION"].ToString(),
                        Name = row["Name"].ToString(),
                        Address = row["Address"].ToString()
                    });
                }
                totalRecords = allMedicalOrganization.Count;
                totalPages = (int)Math.Ceiling((double)totalRecords / recordsPerPage);
                FillMedicalOrganizationsDataGrid();

            }
        }

        private void FillMedicalOrganizationsDataGrid()
        {
            // Очищаем коллекцию перед загрузкой новых данных
            displayedMedicalOrganizations.Clear();

            // Определите начальный и конечный индексы для отображения записей на текущей странице
            startIndex = (currentPage - 1) * recordsPerPage;
            endIndex = startIndex + recordsPerPage;

            // Выводим только записи для текущей страницы
            for (int i = 0; i < allMedicalOrganization.Count; i++)
            {
                displayedMedicalOrganizations.Add(allMedicalOrganization[i]);
            }
            MedicalOrganizationsDataGrid.ItemsSource = displayedMedicalOrganizations;
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
            FillMedicalOrganizationsDataGrid();
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
            FillMedicalOrganizationsDataGrid();
            UpdatePageButtons();
        }




        // Fill
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
            if (MedicalOrganizationsDataGrid.SelectedItem is MedicalOrganization selectedStudent)
            {
                // Создаем экземпляр формы AddStudent
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    PageAddOrChangeMedicalOrganization addStudent = new PageAddOrChangeMedicalOrganization();

                    // Передаем данные выбранного студента в форму редактирования
                    addStudent.studentget(
                        int.Parse(selectedStudent.ID),
                        selectedStudent.Name,
                        selectedStudent.Address
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
            // Создаем форму для добавления нового студента
            PageAddOrChangeMedicalOrganization addOrChangeMedicalOrganization = new PageAddOrChangeMedicalOrganization
            {
                AddOrChange = false
            };

            // Вызываем событие, указывающее на запрос открытия формы добавления студента
            OpenAddMedicalOrganizationRequested?.Invoke(this, EventArgs.Empty);
        }


        // Обработчик события для кнопки "Удалить запись"
        private void ButtonDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Запрос к базе данных для получения данных о группах
                string query = "DELETE FROM [MEDICAL_ORGANIZATION] WHERE 1=1 ";

                // Формируем условия фильтрации
                List<string> conditions = new List<string>();
                List<SqlParameter> parameters = new List<SqlParameter>();

                // Добавляем условия для текстового поиска
                if (MedicalOrganizationsDataGrid.SelectedItem is MedicalOrganization selectedStudent)
                {
                    string searchCondition = "ID_MEDICAL_ORGANIZATION = @ID_MEDICAL_ORGANIZATION ";
                    conditions.Add(searchCondition);
                    parameters.Add(new SqlParameter("@ID_MEDICAL_ORGANIZATION", selectedStudent.ID));
                }

                foreach (string condition in conditions)
                {
                    query += " AND " + condition;
                }

                DataTable result = connectionDb.ExecuteSqlParameters(query, parameters.ToArray());

                LoadMedicalOrganizations();
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
                    FillMedicalOrganizationsDataGrid();
                    UpdatePageButtons();
                }
            }
        } 
        




        // TextBox
        // Обработчик изменения текста в поле поиска
        private void TextBoxFilterSearchStudent_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Обработка изменения текста в TextBox для фильтрации студентов
            if (TextBoxFilterSearchMedicalOrganizations.Text == "")
            {
                TextBlockSearchMedicalOrganizations.Text = "Поиск ...";
                FillMedicalOrganizationsDataGrid();
                LoadMedicalOrganizations();
                //studentsDataLoader.GetStudents();
                FillMedicalOrganizationsDataGrid();
            }
            else
            {
                TextBlockSearchMedicalOrganizations.Text = "";
                ApplyFilters();
            }
        }

        public void ApplyFilters()
        {
            // Создаем экземпляр ConnectionDataBase
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                MedicalOrganizationsDataGrid.ItemsSource = null;

                // Формируем условия фильтрации
                List<string> conditions = new List<string>();
                List<SqlParameter> parameters = new List<SqlParameter>();

                // Добавляем условия для текстового поиска
                string txtfilter = TextBoxFilterSearchMedicalOrganizations.Text;
                if (!string.IsNullOrEmpty(txtfilter))
                {
                    string searchCondition = "Name LIKE @Filter ";
                    conditions.Add(searchCondition);
                    parameters.Add(new SqlParameter("@Filter", "%" + txtfilter + "%"));
                }

                // Составляем запрос с учетом всех условий
                string query = "SELECT ID_MEDICAL_ORGANIZATION, Name, Address " +
                               "FROM [dbo].[MEDICAL_ORGANIZATION] " +
                               "WHERE 1=1";

                foreach (string condition in conditions)
                {
                    query += " AND " + condition;
                }

                // Выполняем SQL-запрос с параметрами
                DataTable dt = connectionDataBase.ExecuteSqlParameters(query, parameters.ToArray());

                // Заполняем коллекцию студентов данными из запроса
                allMedicalOrganization.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    allMedicalOrganization.Add(new MedicalOrganization
                    {
                        ID = row["ID_MEDICAL_ORGANIZATION"].ToString(),
                        Name = row["Name"].ToString(),
                        Address = row["Address"].ToString(),
                    });
                }

                // Обновляем общее количество записей и количество страниц
                totalRecords = allMedicalOrganization.Count;

                // Заполняем DataGrid новыми данными и обновляем кнопки страниц
                FillMedicalOrganizationsDataGrid();
            }
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

        private void RecordForm_RecordAdded(object sender, EventArgs e)
        {
            // При вызове события RecordAdded обновляем данные в DataGrid
            LoadMedicalOrganizations();
        }

        public partial class MedicalOrganization
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
        }

        string queryMedicalOrganization = "SELECT ID_MEDICAL_ORGANIZATION AS [Идентификационый номер], Name AS [Наименование], Address AS [Адрес] FROM [MEDICAL_ORGANIZATION]";

        private void ButtonExportDock_Click(object sender, RoutedEventArgs e)
        {
            ExportWord exportWord = new ExportWord();
            exportWord.export(queryMedicalOrganization, "f");

        }

        private void ButtonExportExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportExcel exportExcel = new ExportExcel();
            exportExcel.export(queryMedicalOrganization);

        }
    }
}
