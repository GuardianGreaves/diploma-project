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
using System.Diagnostics.SymbolStore;

namespace DiplomOboskalov.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageAddOrChangeCheckVaccine.xaml
    /// </summary>
    public partial class PageAddOrChangeCheckVaccine : Page
    {
        public PageAddOrChangeCheckVaccine()
        {
            ListStudents = new List<StudentInfo>();
            InitializeComponent();
            LoadStudents();
            ComboBoxPermission.Items.Add("Разрешено");
            ComboBoxPermission.Items.Add("Не разрешено");
        }
        public List<StudentInfo> ListStudents { get; set; }

        public bool AddOrChange = false;

        public int ID;
        public string USER;
        public int ID_USER;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void get(int _ID_CHECK_VACCINE, string _USER, string _Date, string _BodyTemp, string _ResultInspecion, string _Vaccine, string _Permission, string _StateAfterThirtyMin)
        {
            USER = _USER;
            ID = _ID_CHECK_VACCINE;
            ComboBoxStudentFIO.Text = _USER;
            DatePickerDate.Text = _Date;
            TextBoxBodyTemp.Text = _BodyTemp;
            TextBoxResultInspecion.Text = _ResultInspecion;
            TextBoxVaccine.Text = _Vaccine;
            ComboBoxPermission.Text = _Permission;
            TextBoxStateAfterThirtyMin.Text = _StateAfterThirtyMin;
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
                            string sql = "INSERT INTO [dbo].[CHECK_VACCINE] (ID_USER, Date, BodyTemp, ResultInspecion, Vaccine, Permission, StateAfterThirtyMin ) " +
                                         "VALUES (@ID_USER, @Date, @BodyTemp, @ResultInspecion, @Vaccine, @Permission, @StateAfterThirtyMin) ";

                            // Формируем условия фильтрации
                            List<SqlParameter> parameters = new List<SqlParameter>();

                            // Добавляем условия для текстового поиска
                            parameters.Add(new SqlParameter("@ID_USER", ID_USER));
                            parameters.Add(new SqlParameter("@Date", ParseAndFormatDate(DatePickerDate.Text)));
                            parameters.Add(new SqlParameter("@BodyTemp", TextBoxBodyTemp.Text));
                            parameters.Add(new SqlParameter("@ResultInspecion", TextBoxResultInspecion.Text));
                            parameters.Add(new SqlParameter("@Vaccine", TextBoxVaccine.Text));
                            parameters.Add(new SqlParameter("@Permission", ComboBoxPermission.Text));
                            parameters.Add(new SqlParameter("@StateAfterThirtyMin", TextBoxStateAfterThirtyMin.Text));

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

                            var sql = "UPDATE [CHECK_VACCINE] SET " +
                                        "ID_USER = @ID_USER, " +
                                        "Date = @Date, " +
                                        "BodyTemp = @BodyTemp, " +
                                        "ResultInspecion = @ResultInspecion, " +
                                        "Vaccine = @Vaccine, " +
                                        "Permission = @Permission, " +
                                        "StateAfterThirtyMin = @StateAfterThirtyMin " +
                                        "WHERE ID_CHECK_VACCINE = @ID_CHECK_VACCINE";

                            parameters.Add(new SqlParameter("@ID_USER", ID_USER));
                            parameters.Add(new SqlParameter("@Date", ParseAndFormatDate(DatePickerDate.Text)));
                            parameters.Add(new SqlParameter("@BodyTemp", TextBoxBodyTemp.Text));
                            parameters.Add(new SqlParameter("@ResultInspecion", TextBoxResultInspecion.Text));
                            parameters.Add(new SqlParameter("@Vaccine", TextBoxVaccine.Text));
                            parameters.Add(new SqlParameter("@Permission", ComboBoxPermission.Text));
                            parameters.Add(new SqlParameter("@StateAfterThirtyMin", TextBoxStateAfterThirtyMin.Text));

                            parameters.Add(new SqlParameter("@ID_CHECK_VACCINE", ID));

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

            PageCheckVaccine page = new PageCheckVaccine();
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PageCheckVaccine page = new PageCheckVaccine();
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