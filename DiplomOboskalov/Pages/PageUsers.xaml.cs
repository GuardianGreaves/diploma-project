using DiplomLibrary;
using DiplomLibrary.DataBase;
using DiplomLibrary.Export;
using MahApps.Metro.IconPacks;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static DiplomOboskalov.Pages.PageOperations;

namespace DiplomOboskalov.Pages
{
    public partial class PageUsers : Page
    {
        public List<User> ListUsers { get; set; }

        private int totalRecords;
        private int countclick = 0;

        public PageUsers()
        {
            ListUsers = new List<User>();

            InitializeComponent();

            LoadUser();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------


        private void LoadUser()
        {
            // Создаем экземпляр ConnectionDataBase для взаимодействия с базой данных
            using (ConnectionDataBase connectionDb = new ConnectionDataBase())
            {
                // Очищаем текущее содержимое DataGrid
                StudentsDataGrid.ItemsSource = null;

                // Запрос к базе данных для получения данных о группах
                string query = "SELECT U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.HomeAddress, U.Hostel, U.ID_GROUP, U.DateEnrollment, G.GroupName, U.Color, L.Login, L.Password, R.NameRole " +
                             "FROM [dbo].[USER] U " +
                             "INNER JOIN [GROUP] G ON U.ID_GROUP = G.ID_GROUP " +
                             "INNER JOIN [LOGIN_PASSWORD] L ON U.ID_USER = L.ID_USER " +
                             "INNER JOIN [USER_ROLE] R ON U.ID_USER_ROLE = R.ID_USER_ROLE";

                DataTable dt = connectionDb.ExecuteSql(query);

                // Очищаем предыдущие данные в коллекции групп
                ListUsers.Clear();

                // Добавляем данные о группах в коллекцию Groups
                foreach (DataRow row in dt.Rows)
                {
                    string enteredLogin = row["Login"].ToString();
                    string enteredPassword = row["Password"].ToString();
                    Decrypt decryptClass = new Decrypt();
                    var decryptLogin = decryptClass.decrypt(enteredLogin);
                    var decryptPassword = decryptClass.decrypt(enteredPassword);

                    ListUsers.Add(new User
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
                        Role = row["NameRole"].ToString(),
                        Login = decryptLogin,
                        Password = decryptPassword
                    }) ;
                }
                totalRecords = ListUsers.Count;

                StudentsDataGrid.ItemsSource = ListUsers;
            }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------


        private void ButtonAddStudent_Click(object sender, RoutedEventArgs e)
        {
            // Создаем форму для добавления нового студента
            //PageAddOrChangeSanitarySpaTreatment page = new PageAddOrChangeSanitarySpaTreatment
            //{
            //    AddOrChange = false
            //};
            //NavigationService.Navigate(page);
        }

        // Обработчик события для кнопки "Редактировать запись"
        private void ButtonEditRow_Click(object sender, RoutedEventArgs e)
        {
            //// Получаем выбранного студента из DataGrid
            //if (OperationsDataGrid.SelectedItem is OperationsInfo selected)
            //{
            //    // Создаем экземпляр формы AddStudent
            //    using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            //    {
            //        PageAddOrChangeSanitarySpaTreatment page = new PageAddOrChangeSanitarySpaTreatment();

            //        // Передаем данные выбранного студента в форму редактирования
            //        page.get(
            //            int.Parse(selected.ID_SANITARY_SPA_TREATMENT), selected.ID_USER, selected.Description);

            //        // Переходим к форме редактирования студента
            //        page.AddOrChange = true;
            //        NavigationService.Navigate(page);
            //    }
            //}
        }

        private void DeleteRow(string studentID)
        {
            try
            {
                using (var connection = new SqlConnection())
                {
                    connection.Open();
                    // Используйте параметры в запросе
                    var sql = "DELETE FROM [USER] WHERE ID_USER = @UserID";
                    // Подготовка команды SQL
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", studentID);
                        // Выполнение SQL-запроса
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Пользователь успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Пользователь с указанным ID не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadUser();
        }


        // Обработчик события для кнопки "Удалить запись"
        private void ButtonDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного студента из DataGrid
            if (StudentsDataGrid.SelectedItem is User selected)
            {
                // Удаляем выбранного студента из базы данных
                DeleteRow(selected.ID);
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


        public partial class User
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
            public string Role { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }

        }

        private void ButtonAddStudent_Click_1(object sender, RoutedEventArgs e)
        {
            // Создаем форму для добавления нового студента
            PageAddOrChangeUser page = new PageAddOrChangeUser
            {
                AddOrChange = false
            };
            NavigationService.Navigate(page);
        }
    }
}