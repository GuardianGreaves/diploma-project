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
using System.Collections.ObjectModel;
using System.Collections;

namespace DiplomOboskalov.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageAddOrChangeTrauma.xaml
    /// </summary>
    public partial class PageAddOrChangeTrauma : Page
    {
        public PageAddOrChangeTrauma()
        {
            allStudents = new ObservableCollection<Student>();
            InitializeComponent();
            LoadStudents();
        }

        public event EventHandler OpenAddMedicalOrganizationRequested;
        public int ID;
        public int ID_Student;
        public bool AddOrChange = false;

        public void studentget(int id, string trauma, string student)
        {
            ID = id;
            TextBoxTrauma.Text = trauma;
            ComboBoxStudentFIO.Text = student;
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            string trauma = TextBoxTrauma.Text;
            string student = ComboBoxStudentFIO.Text;

            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                if (!string.IsNullOrEmpty(student))
                {
                    // Запрос к базе данных для получения ID_GROUP по названию группы
                    string query_groupname = $"SELECT U.ID_USER, U.Surname + ' ' + U.Name + ' ' + U.Patronymic FROM [USER] U WHERE U.Surname + ' ' + U.Name + ' ' + U.Patronymic = '{student}'";

                    // Выполнение запроса
                    DataTable result = connectionDataBase.ExecuteSql(query_groupname);

                    // Проверка, что запрос вернул результат
                    if (result.Rows.Count > 0)
                    {
                        // Получение значения ID_GROUP из первой строки результата
                        ID_Student = Convert.ToInt32(result.Rows[0]["ID_USER"]);

                        // Используйте idGroup по вашему усмотрению
                    }
                    else
                    {
                        MessageBox.Show($"Группа с названием {student} не найдена в базе данных.");
                    }
                }
            }


            try
            {
                using (ConnectionDataBase connectionDb = new ConnectionDataBase())
                {
                    if (AddOrChange == false)
                    {
                        string sql = "INSERT INTO [dbo].[TRAUMA] (ID_USER, Description) " +
                                     "VALUES (@ID_USER, @Description) ";

                        // Формируем условия фильтрации
                        List<SqlParameter> parameters = new List<SqlParameter>();

                        // Добавляем условия для текстового поиска
                        parameters.Add(new SqlParameter("@Description", trauma));
                        parameters.Add(new SqlParameter("@ID_USER", ID_Student));

                        DataTable result = connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());


                        MessageBox.Show("Данные успешно добавлены в базу данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        try
                        {
                            List<SqlParameter> parameters = new List<SqlParameter>();

                            // Используйте параметры в запросе
                            var sql = "UPDATE [TRAUMA] SET " +
                                        "ID_USER = @ID_USER, " +
                                        "Description = @Description " +
                                        "WHERE ID_TRAUMA = @ID_TRAUMA";

                            parameters.Add(new SqlParameter("@ID_USER", ID_Student));
                            parameters.Add(new SqlParameter("@Description", trauma));
                            parameters.Add(new SqlParameter("@ID_TRAUMA", ID));

                            DataTable dt = connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());

                            MessageBox.Show("Данные успешно изменены в базе данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при изменении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            PageTrauma pageTrauma = new PageTrauma();
            NavigationService.Navigate(pageTrauma);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PageTrauma pageTrauma = new PageTrauma();
            NavigationService.Navigate(pageTrauma);
        }

        private readonly ObservableCollection<Student> allStudents = new ObservableCollection<Student>();

        private void LoadStudents()
        {
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Формируем условия фильтрации
                List<string> conditions = new List<string>();
                List<SqlParameter> parameters = new List<SqlParameter>();

                // Составляем запрос с учетом всех условий
                string query = "SELECT U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.Hostel, U.ID_GROUP, U.DateEnrollment " +
                               "FROM [USER] U ";

                // Выполняем SQL-запрос с параметрами
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
                    });
                    // Удалите ItemsSource
                    ComboBoxStudentFIO.ItemsSource = null;

                    // Добавьте элементы через Items
                    ComboBoxStudentFIO.Items.Clear();

                    foreach (var studentName in allStudents.Select(s => s.FirstName + ' ' + s.MiddleName + ' ' + s.LastName).Distinct())
                    {
                        ComboBoxStudentFIO.Items.Add(studentName);
                    }
                }
            }
        }
        public partial class Student
        {
            public string ID { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
        }

    }
}
