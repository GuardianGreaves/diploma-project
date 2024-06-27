using DiplomLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DiplomOboskalov.Pages.PagePastInfectiousDiases;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace DiplomOboskalov.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageAddOrChangePastInfectiousDiases.xaml
    /// </summary>
    public partial class PageAddOrChangePastInfectiousDiases : Page
    {
        public PageAddOrChangePastInfectiousDiases()
        {
            ListStudents = new List<StudentInfo>();
            InitializeComponent();
            LoadStudents();
        }

        public List<StudentInfo> ListStudents { get; set; }

        public bool AddOrChange = false;

        public int ID;
        public string USER;
        public int ID_USER;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void get(int _ID_PAST_INFECTIOUS_DIASES, string _USER, DateTime _Date, string _Description)
        {
            USER = _USER;
            ID = _ID_PAST_INFECTIOUS_DIASES;
            ComboBoxStudentFIO.Text = _USER;
            TextBoxDescription.Text = _Description;
            DatePickerDate.Text = _Date.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ConnectionDataBase connectionDb = new ConnectionDataBase())
                {

                    // Поиск ID_USER
                    string query1 = $"SELECT ID_USER FROM [USER] WHERE Surname + ' ' + Name + ' ' + Patronymic = '{ComboBoxStudentFIO.Text}'";

                    DataTable result = connectionDb.ExecuteSql(query1);

                    if (result.Rows.Count > 0)
                    {
                        ID_USER = Convert.ToInt32(result.Rows[0]["ID_USER"]);
                    }
                    else
                    {
                        MessageBox.Show($"'{ComboBoxStudentFIO.Text}' не найдено в базе данных.");
                    }


                    // Добавление
                    try
                    {
                        if (AddOrChange == false)
                        {
                            string sql = "INSERT INTO [dbo].[PAST_INFECTIOUS_DIASES] (ID_USER, Date, Description) " +
                                         "VALUES (@ID_USER, @Date, @Description) ";

                            // Формируем условия фильтрации
                            List<SqlParameter> parameters = new List<SqlParameter>();

                            // Добавляем условия для текстового поиска
                            parameters.Add(new SqlParameter("@ID_USER", ID_USER));
                            parameters.Add(new SqlParameter("@Date", DatePickerDate.Text));
                            parameters.Add(new SqlParameter("@Description", TextBoxDescription.Text));

                            DataTable result2 = connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());
                            
                            if (result2.Rows.Count > 0)
                            {
                                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }


                    // Редактирование
                    try
                    {

                        if (AddOrChange == true)
                        {
                            List<SqlParameter> parameters = new List<SqlParameter>();

                            var sql = "UPDATE [PAST_INFECTIOUS_DIASES] SET " +
                                        "ID_USER = @ID_USER, " +
                                        "Date = @Date, " +
                                        "Description = @Description " +
                                        "WHERE ID_PAST_INFECTIOUS_DIASES = @ID_PAST_INFECTIOUS_DIASES";

                            parameters.Add(new SqlParameter("@ID_USER", ID_USER));
                            parameters.Add(new SqlParameter("@Date", DatePickerDate.Text));
                            parameters.Add(new SqlParameter("@Description", TextBoxDescription.Text));

                            parameters.Add(new SqlParameter("@ID_PAST_INFECTIOUS_DIASES", ID));

                            DataTable result3 = connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());

                            if (result3.Rows.Count > 0)
                            {

                                MessageBox.Show("Данные успешно изменены в базе данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при изменении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            PagePastInfectiousDiases page = new PagePastInfectiousDiases();
            NavigationService.Navigate(page);
        }

        private void LoadStudents()
        {
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                string query = "SELECT U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.Hostel, U.ID_GROUP, U.DateEnrollment " +
                               "FROM [USER] U ";

                DataTable dt = connectionDataBase.ExecuteSql(query);

                ListStudents.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    ListStudents.Add(new StudentInfo
                    {
                        ID = row["ID_USER"].ToString(),
                        FirstName = row["Surname"].ToString(),
                        MiddleName = row["Name"].ToString(),
                        LastName = row["Patronymic"].ToString(),
                    });

                    ComboBoxStudentFIO.ItemsSource = null;
                    ComboBoxStudentFIO.Items.Clear();
                    foreach (var studentName in ListStudents.Select(s => s.FirstName + ' ' + s.MiddleName + ' ' + s.LastName).Distinct())
                    {
                        ComboBoxStudentFIO.Items.Add(studentName);
                    }

                }
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PagePastInfectiousDiases page = new PagePastInfectiousDiases();
            NavigationService.Navigate(page);
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
    }
}
