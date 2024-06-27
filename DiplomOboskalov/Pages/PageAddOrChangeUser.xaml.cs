using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using DiplomLibrary;
using Microsoft.Win32;
using static DiplomOboskalov.Pages.PageAddOrChangeUser;

namespace DiplomOboskalov.Pages
{
    public partial class PageAddOrChangeUser : Page
    {
        public PageAddOrChangeUser()
        {
            Groups = new List<GroupItem>();
            Roles = new List<Role>();
            InitializeComponent();
            LoadRole();
            LoadGroups();
        }

        string fileName;
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string photosSubdirectory = "Image";

        private string selectedPhotoPath;
        public int ID;
        public string SelectedGroup { get; set; }
        public List<GroupItem> Groups { get; set; }
        public List<Role> Roles { get; set; }

        public bool AddOrChange = false;

        public void studentget(int id, string lastName, string firstName, string middleName, DateTime birthDate, string address, string dormitory, string idGroup, DateTime enrollmentDate, string selectedPhotoPath, string idRole, string login, string password)
        {
            ID = id;
            TextBoxLastName.Text = lastName;
            TextBoxFirstName.Text = firstName;
            TextBoxMiddleName.Text = middleName;
            DatePickerBirthDate.SelectedDate = birthDate;
            TextBoxAddress.Text = address;
            TextBoxDormitory.Text = dormitory;

            // Проверка на null для selectedPhotoPath
            if (selectedPhotoPath != null)
            {
                string imagePath = Path.Combine(baseDirectory, photosSubdirectory, selectedPhotoPath);

                // Проверка существования файла
                if (File.Exists(imagePath))
                {
                    BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));
                    ImageStudent.Source = bitmapImage;
                }
            }

            if (AddOrChange == true)
            {
                if (!string.IsNullOrEmpty(idGroup))
                {
                    // Создаем экземпляр ConnectionDataBase
                    using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                    {
                        // Запрос к базе данных для получения ID_GROUP по названию группы
                        string query_groupname = $"SELECT GroupName FROM [GROUP] WHERE ID_GROUP = '{idGroup}'";

                        // Выполнение запроса
                        DataTable result = connectionDataBase.ExecuteSql(query_groupname);

                        // Проверка, что запрос вернул результат
                        if (result.Rows.Count > 0)
                        {
                            // Получение значения ID_GROUP из первой строки результата
                            ComboBoxGroup.Text = result.Rows[0]["GroupName"].ToString();
                        }
                        else
                        {
                            MessageBox.Show($"Группа с названием {idGroup} не найдена в базе данных.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите группу из ComboBox.");
                }

                if (!string.IsNullOrEmpty(idRole))
                {
                    // Создаем экземпляр ConnectionDataBase
                    using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                    {
                        // Запрос к базе данных для получения ID_GROUP по названию группы
                        string query_groupname = $"SELECT NameRole FROM [USER_ROLE] WHERE ID_USER_ROLE = '{idRole}'";

                        // Выполнение запроса
                        DataTable result = connectionDataBase.ExecuteSql(query_groupname);

                        // Проверка, что запрос вернул результат
                        if (result.Rows.Count > 0)
                        {
                            // Получение значения ID_GROUP из первой строки результата
                            ComboBoxRole.Text = result.Rows[0]["NameRole"].ToString();
                        }
                        else
                        {
                            MessageBox.Show($"Роль с наименованием {idRole} не найдена в базе данных.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите роль из ComboBox.");
                }

            }
            else
            {
                // Создаем экземпляр ConnectionDataBase
                using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
                {
                    // Запрос к базе данных для получения ID_GROUP по названию группы
                    string query_groupname = $"SELECT NameRole FROM [USER_ROLE]";

                    // Выполнение запроса
                    DataTable result = connectionDataBase.ExecuteSql(query_groupname);

                    // Проверка, что запрос вернул результат
                    if (result.Rows.Count > 0)
                    {
                        // Получение значения ID_GROUP из первой строки результата
                        ComboBoxRole.Text = result.Rows[0]["NameRole"].ToString();
                    }
                }
            }

            // Создаем экземпляр ConnectionDataBase
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                // Запрос к базе данных для получения ID_GROUP по названию группы
                string query_groupname = $"SELECT GroupName FROM [GROUP]";

                // Выполнение запроса
                DataTable result = connectionDataBase.ExecuteSql(query_groupname);

                // Проверка, что запрос вернул результат
                if (result.Rows.Count > 0)
                {
                    // Получение значения ID_GROUP из первой строки результата
                    ComboBoxGroup.Text = result.Rows[0]["GroupName"].ToString();
                }
            }
            DatePickerEnrollmentDate.SelectedDate = enrollmentDate;
        }

        public class GroupItem
        {
            public string Group { get; set; }
            // Добавьте дополнительные свойства, если необходимо
        }
        public class Role
        {
            public string NameRole { get; set; }
            // Добавьте дополнительные свойства, если необходимо
        }

        private void LoadRole()
        {
            // Создаем экземпляр DatabaseManager
            using (ConnectionDataBase databaseManager = new ConnectionDataBase())
            {
                // Получаем группы из базы данных и добавляем их в коллекцию Groups
                string query = "SELECT NameRole FROM [USER_ROLE]";
                DataTable result = databaseManager.ExecuteSql(query);

                foreach (DataRow row in result.Rows)
                {
                    Roles.Add(new Role { NameRole = row["NameRole"].ToString() });
                }

                // Устанавливаем ItemsSource для ComboBoxGroup
                ComboBoxRole.ItemsSource = Roles;
            }
        }

        private void LoadGroups()
        {
            // Создаем экземпляр DatabaseManager
            using (ConnectionDataBase databaseManager = new ConnectionDataBase())
            {
                // Получаем группы из базы данных и добавляем их в коллекцию Groups
                string query = "SELECT ID_GROUP, GroupName FROM [GROUP]";
                DataTable result = databaseManager.ExecuteSql(query);

                foreach (DataRow row in result.Rows)
                {
                    Groups.Add(new GroupItem { Group = row["GroupName"].ToString() });
                }

                // Устанавливаем ItemsSource для ComboBoxGroup
                ComboBoxGroup.ItemsSource = Groups;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PageStudents pageStudents = new PageStudents();
            NavigationService.Navigate(pageStudents);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connectionDataBase = new ConnectionDataBase())
                {
                    // ... (код до получения idGroup)
                    string lastName = TextBoxLastName.Text;
                    string firstName = TextBoxFirstName.Text;
                    string middleName = TextBoxMiddleName.Text;
                    DateTime birthDate = (DatePickerBirthDate.SelectedDate ?? DateTime.MinValue).Date;
                    string address = TextBoxAddress.Text;
                    string login = TextBoxLogin.Text;
                    string password = TextBoxPassword.Text;
                    string dormitory = TextBoxDormitory.Text;
                    DateTime enrollmentDate = (DatePickerEnrollmentDate.SelectedDate ?? DateTime.MinValue).Date;
                    string idGroup = "0";
                    string idRole = "0";

                    string imageName = null;
                    string imagePath = Path.Combine(baseDirectory, photosSubdirectory, selectedPhotoPath);
                    // Проверка существования файла
                    if (File.Exists(imagePath))
                    {
                        imageName = fileName;
                    }

                    string groupName = ((GroupItem)ComboBoxGroup.SelectedItem)?.Group;
                    string nameRole = ((Role)ComboBoxRole.SelectedItem)?.NameRole;

                    // Проверка на null и пустую строку
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        // Запрос к базе данных для получения ID_GROUP по названию группы
                        string query_groupname = $"SELECT ID_GROUP FROM [GROUP] WHERE GroupName = '{groupName}'";

                        // Выполнение запроса
                        DataTable result = connectionDataBase.ExecuteSql(query_groupname);

                        // Проверка, что запрос вернул результат
                        if (result.Rows.Count > 0)
                        {
                            // Получение значения ID_GROUP из первой строки результата
                            idGroup = result.Rows[0]["ID_GROUP"].ToString();

                            // Используйте idGroup по вашему усмотрению
                        }
                        else
                        {
                            MessageBox.Show($"Группа с названием {groupName} не найдена в базе данных.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Выберите группу из ComboBox.");
                    }

                    if (!string.IsNullOrEmpty(nameRole))
                    {
                        // Запрос к базе данных для получения ID_GROUP по названию группы
                        string query_groupname = $"SELECT ID_USER_ROLE FROM [USER_ROLE] WHERE NameRole = '{nameRole}'";

                        // Выполнение запроса
                        DataTable result = connectionDataBase.ExecuteSql(query_groupname);

                        // Проверка, что запрос вернул результат
                        if (result.Rows.Count > 0)
                        {
                            // Получение значения ID_GROUP из первой строки результата
                            idRole = result.Rows[0]["ID_USER_ROLE"].ToString();

                            // Используйте idGroup по вашему усмотрению
                        }
                        else
                        {
                            MessageBox.Show($"Группа с названием {nameRole} не найдена в базе данных.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Выберите группу из ComboBox.");
                    }

                    var idUser = 0;
                    if (AddOrChange == false)
                    {
                        // Используйте параметры в запросе
                        var sql = "INSERT INTO [USER] (ID_USER_ROLE, Surname, Name, Patronymic, DateBirth, HomeAddress, Hostel, ID_GROUP, DateEnrollment, ImageName, Color) " +
                                  "VALUES (@UserRole, @Surname, @Name, @Patronymic, @DateBirth, @HomeAddress, @Hostel, @IDGroup, @DateEnrollment, @ImageName, @Color); " +
                                  "SELECT SCOPE_IDENTITY()";

                        // Подготовка команды SQL
                        using (var command = new SqlCommand(sql, connectionDataBase.GetSqlConnection()))
                        {
                            command.Parameters.AddWithValue("@UserRole", idRole);
                            command.Parameters.AddWithValue("@Surname", lastName);
                            command.Parameters.AddWithValue("@Name", firstName);
                            command.Parameters.AddWithValue("@Patronymic", middleName);
                            command.Parameters.AddWithValue("@DateBirth", birthDate);
                            command.Parameters.AddWithValue("@HomeAddress", address);
                            command.Parameters.AddWithValue("@Hostel", dormitory);
                            command.Parameters.AddWithValue("@IDGroup", idGroup);
                            command.Parameters.AddWithValue("@DateEnrollment", enrollmentDate);
                            command.Parameters.AddWithValue("@ImageName", imageName);

                            Random random = new Random();
                            string colorCode = "#" + random.Next(0x1000000).ToString("X6");
                            command.Parameters.AddWithValue("@Color", colorCode);

                            connectionDataBase.GetSqlConnection().Open();

                            //command.ExecuteNonQuery();

                            object result = command.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                idUser = Convert.ToInt32(result);
                            }
                            else
                            {
                                // Обработка ошибки: запрос не возвратил ни одной строки
                            }

                            // Выполнение SQL-запроса
                            // Открытие подключения

                            // Используйте параметры в запросе
                            sql = "INSERT INTO [LOGIN_PASSWORD] (ID_USER, Login, Password) " +
                                      "VALUES (@ID_USER, @Login, @Password)";

                            MessageBox.Show("Данные успешно добавлены в базу данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        // Подготовка команды SQL
                        using (var command2 = new SqlCommand(sql, connectionDataBase.GetSqlConnection()))
                        {
                            command2.Parameters.AddWithValue("@ID_USER", idUser);
                            command2.Parameters.AddWithValue("@Login", lastName);
                            command2.Parameters.AddWithValue("@Password", firstName);

                            // Выполнение SQL-запроса на добавление данных
                            command2.ExecuteNonQuery();

                        }

                    }
                    else
                    {
                        try
                        {
                            using (var connectionDataBase2 = new ConnectionDataBase())
                            {
                                // Используйте параметры в запросе
                                var sql = "UPDATE [USER] SET " +
                                          "Surname = @Surname, " +
                                          "Name = @Name, " +
                                          "Patronymic = @Patronymic, " +
                                          "DateBirth = @DateBirth, " +
                                          "HomeAddress = @HomeAddress, " +
                                          "Hostel = @Hostel, " +
                                          "ID_GROUP = @IDGroup, " +
                                          "DateEnrollment = @DateEnrollment, " +
                                          "ImageName = @ImageName " +
                                          "WHERE ID_USER = @UserID; " +
                                          "SELECT ID_USER FROM [USER] WHERE ID_USER = @UserID";

                                // Подготовка команды SQL
                                using (var command = new SqlCommand(sql, connectionDataBase2.GetSqlConnection()))
                                {
                                    command.Parameters.AddWithValue("@UserID", ID); // ID пользователя, которого вы хотите изменить
                                    command.Parameters.AddWithValue("@Surname", lastName);
                                    command.Parameters.AddWithValue("@Name", firstName);
                                    command.Parameters.AddWithValue("@Patronymic", middleName);
                                    command.Parameters.AddWithValue("@DateBirth", birthDate);
                                    command.Parameters.AddWithValue("@HomeAddress", address);
                                    command.Parameters.AddWithValue("@Hostel", dormitory);
                                    command.Parameters.AddWithValue("@IDGroup", idGroup);
                                    command.Parameters.AddWithValue("@DateEnrollment", enrollmentDate);
                                    command.Parameters.AddWithValue("@ImageName", imageName);

                                    idUser = (int)command.ExecuteScalar();

                                    using (var command2 = new SqlCommand("UPDATE [LOGIN_PASSWORD] SET " +
                                                                      "Login = @Login, " +
                                                                      "Password = @Password " +
                                                                      "WHERE ID_USER = @ID_USER", connectionDataBase2.GetSqlConnection()))
                                    {
                                        command2.Parameters.AddWithValue("@ID_USER", idUser);
                                        command2.Parameters.AddWithValue("@Login", login);
                                        command2.Parameters.AddWithValue("@Password", password);
                                        command2.ExecuteNonQuery();
                                    }
                                }

                                MessageBox.Show("Данные успешно изменены в базе данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
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
            PageStudents pageStudents = new PageStudents();
            NavigationService.Navigate(pageStudents);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg; *.png; *.bmp)|*.jpg; *.png; *.bmp|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedPhotoPath = openFileDialog.FileName;

                // Specify the destination folder where you want to copy the selected photo
                string destinationFolder = Path.Combine(baseDirectory, photosSubdirectory);

                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                try
                {
                    // Get the file name from the selected photo path
                    fileName = Path.GetFileName(selectedPhotoPath);

                    // Construct the destination path by combining the destination folder and file name
                    string destinationPath = Path.Combine(destinationFolder, fileName);

                    // Copy the file to the destination folder
                    File.Copy(selectedPhotoPath, destinationPath, true);

                    // Устанавливаем Source для Image
                    ImageStudent.Source = new BitmapImage(new Uri(destinationPath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при копировании или загрузке изображения: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
