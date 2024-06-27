using DiplomLibrary;
using DiplomLibrary.Export;
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

namespace DiplomOboskalov.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageReports.xaml
    /// </summary>
    public partial class PageReports : Page
    {
        ConnectionDataBase connectionDb = new ConnectionDataBase();
        public int ID_USER;
        private readonly ObservableCollection<Student> allStudents = new ObservableCollection<Student>();
        public partial class Student
        {
            public string ID { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string Group { get; set; }
            public string Hostel { get; set; }
        }

        public PageReports(string _srudent)
        {
            allStudents = new ObservableCollection<Student>();
            InitializeComponent();
            if (_srudent != null)
            {
                ComboBoxStudent.Items.Add(_srudent);
                ComboBoxStudent.Text = _srudent;
            }
            else
            {
                LoadStudents();
            }
            ComboBoxStudent.SelectedIndex = 0;
        }
        private void LoadStudents()
        {
            using (ConnectionDataBase connectionDataBase = new ConnectionDataBase())
            {
                string query = "SELECT U.ID_USER, U.Surname, U.Name, U.Patronymic, U.DateBirth, U.HomeAddress, U.Hostel, U.ID_GROUP, U.DateEnrollment, G.GroupName, U.Color " +
                               "FROM [dbo].[USER] U " +
                               "INNER JOIN [GROUP] G ON U.ID_GROUP = G.ID_GROUP";

                DataTable dt = connectionDataBase.ExecuteSql(query);

                allStudents.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    allStudents.Add(new Student
                    {
                        ID = row["ID_USER"].ToString(),
                        FirstName = row["Surname"].ToString(),
                        MiddleName = row["Name"].ToString(),
                        LastName = row["Patronymic"].ToString(),
                        Hostel = row["Hostel"].ToString(),
                    });
                    // Удалите ItemsSource
                    ComboBoxStudent.ItemsSource = null;

                    // Добавьте элементы через Items
                    ComboBoxStudent.Items.Clear();

                    foreach (var studentName in allStudents.Select(s => s.FirstName + ' ' + s.MiddleName + ' ' + s.LastName).Distinct())
                    {
                        ComboBoxStudent.Items.Add(studentName);
                    }
                }
            }
        }

        private void userId()
        {
            string query1 = $"SELECT ID_USER FROM [USER] WHERE Surname + ' ' + Name + ' ' + Patronymic = '{ComboBoxStudent.Text}'";

            DataTable result = connectionDb.ExecuteSql(query1);

            if (result.Rows.Count > 0)
            {
                ID_USER = Convert.ToInt32(result.Rows[0]["ID_USER"]);
            }
            else
            {
                MessageBox.Show($"'{ComboBoxStudent.Text}' не найдено в базе данных.");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            userId();

            // Получаем выбранное значение из ComboBox
            string comboBoxValue = ComboBoxStudent.SelectedItem?.ToString();

            // Проверяем, выбрано ли значение в ComboBox
            if (comboBoxValue != null)
            {
                // Создаем запрос с использованием выбранного значения из ComboBox
                string query = $"SELECT Vaccine AS Вакцина, CONVERT(varchar, Date, 104) AS Дата FROM PREVENTIVE_VACCINE WHERE ID_USER = '{ID_USER}'";

                // Создаем экземпляр класса для экспорта данных в Word
                ExportWord exportWord = new ExportWord();

                // Экспортируем данные в Word, используя запрос
                exportWord.export(query, ComboBoxStudent.Text);
            }
            else
            {
                // Если значение в ComboBox не выбрано, выводим сообщение об ошибке или предпринимаем другие действия
                MessageBox.Show("Ошибка.");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            userId();

            // Получаем выбранное значение из ComboBox
            string comboBoxValue = ComboBoxStudent.SelectedItem?.ToString();

            // Проверяем, выбрано ли значение в ComboBox
            if (comboBoxValue != null)
            {
                // Создаем запрос с использованием выбранного значения из ComboBox
                string query = $"select Survey AS Обследование, SurveyResult AS Результат, Year AS Год from SURVEYS WHERE ID_USER = '{ID_USER}'";

                // Создаем экземпляр класса для экспорта данных в Word
                ExportWord exportWord = new ExportWord();

                // Экспортируем данные в Word, используя запрос
                exportWord.export(query, ComboBoxStudent.Text);
            }
            else
            {
                // Если значение в ComboBox не выбрано, выводим сообщение об ошибке или предпринимаем другие действия
                MessageBox.Show("Ошибка.");
            }

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            userId();

            // Получаем выбранное значение из ComboBox
            string comboBoxValue = ComboBoxStudent.SelectedItem?.ToString();

            // Проверяем, выбрано ли значение в ComboBox
            if (comboBoxValue != null)
            {
                // Создаем запрос с использованием выбранного значения из ComboBox
                string query = $"select Description AS Обследование from PAST_INFECTIOUS_DIASES WHERE ID_USER = '{ID_USER}'";

                // Создаем экземпляр класса для экспорта данных в Word
                ExportWord exportWord = new ExportWord();

                // Экспортируем данные в Word, используя запрос
                exportWord.exportStroka(query, ComboBoxStudent.Text);
            }
            else
            {
                // Если значение в ComboBox не выбрано, выводим сообщение об ошибке или предпринимаем другие действия
                MessageBox.Show("Ошибка.");
            }

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            userId();

            // Получаем выбранное значение из ComboBox
            string comboBoxValue = ComboBoxStudent.SelectedItem?.ToString();

            // Проверяем, выбрано ли значение в ComboBox
            if (comboBoxValue != null)
            {
                // Создаем запрос с использованием выбранного значения из ComboBox
                string query = $"select Survey AS Обследование, SurveyResult AS [Результат обследования], Year AS Год from DATA_MEDICAL_CHECKUPS WHERE ID_USER = '{ID_USER}'";

                // Создаем экземпляр класса для экспорта данных в Word
                ExportWord exportWord = new ExportWord();

                // Экспортируем данные в Word, используя запрос
                exportWord.export(query, ComboBoxStudent.Text);
            }
            else
            {
                // Если значение в ComboBox не выбрано, выводим сообщение об ошибке или предпринимаем другие действия
                MessageBox.Show("Ошибка.");
            }

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            userId();

            // Получаем выбранное значение из ComboBox
            string comboBoxValue = ComboBoxStudent.SelectedItem?.ToString();

            // Проверяем, выбрано ли значение в ComboBox
            if (comboBoxValue != null)
            {
                // Создаем запрос с использованием выбранного значения из ComboBox
                string query = $"select Diagnosis AS Диагноз, Date AS [Дата] from PASSES_FOR_ILLNESS WHERE ID_USER = '{ID_USER}'";

                // Создаем экземпляр класса для экспорта данных в Word
                ExportWord exportWord = new ExportWord();

                // Экспортируем данные в Word, используя запрос
                exportWord.export(query, ComboBoxStudent.Text);
            }
            else
            {
                // Если значение в ComboBox не выбрано, выводим сообщение об ошибке или предпринимаем другие действия
                MessageBox.Show("Ошибка.");
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            userId();

            // Получаем выбранное значение из ComboBox
            string comboBoxValue = ComboBoxStudent.SelectedItem?.ToString();

            // Проверяем, выбрано ли значение в ComboBox
            if (comboBoxValue != null)
            {
                // Создаем запрос с использованием выбранного значения из ComboBox
                string query = $"select Description AS Описание from HOSPITAL_TREATMENT WHERE ID_USER = '{ID_USER}'";

                // Создаем экземпляр класса для экспорта данных в Word
                ExportWord exportWord = new ExportWord();

                // Экспортируем данные в Word, используя запрос
                exportWord.export(query, ComboBoxStudent.Text);
            }
            else
            {
                // Если значение в ComboBox не выбрано, выводим сообщение об ошибке или предпринимаем другие действия
                MessageBox.Show("Ошибка.");
            }

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            userId();

            // Получаем выбранное значение из ComboBox
            string comboBoxValue = ComboBoxStudent.SelectedItem?.ToString();

            // Проверяем, выбрано ли значение в ComboBox
            if (comboBoxValue != null)
            {
                // Создаем запрос с использованием выбранного значения из ComboBox
                string query = $"select Description AS Описание from SANITARY_SPA_TREATMENT WHERE ID_USER = '{ID_USER}'";

                // Создаем экземпляр класса для экспорта данных в Word
                ExportWord exportWord = new ExportWord();

                // Экспортируем данные в Word, используя запрос
                exportWord.export(query, ComboBoxStudent.Text);
            }
            else
            {
                // Если значение в ComboBox не выбрано, выводим сообщение об ошибке или предпринимаем другие действия
                MessageBox.Show("Ошибка.");
            }

        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            userId();

            // Получаем выбранное значение из ComboBox
            string comboBoxValue = ComboBoxStudent.SelectedItem?.ToString();

            // Проверяем, выбрано ли значение в ComboBox
            if (comboBoxValue != null)
            {
                // Создаем запрос с использованием выбранного значения из ComboBox
                string query = $"select Description AS Описание from TRAUMA WHERE ID_USER = '{ID_USER}'";

                // Создаем экземпляр класса для экспорта данных в Word
                ExportWord exportWord = new ExportWord();

                // Экспортируем данные в Word, используя запрос
                exportWord.export(query, ComboBoxStudent.Text);
            }
            else
            {
                // Если значение в ComboBox не выбрано, выводим сообщение об ошибке или предпринимаем другие действия
                MessageBox.Show("Ошибка.");
            }

        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            userId();

            // Получаем выбранное значение из ComboBox
            string comboBoxValue = ComboBoxStudent.SelectedItem?.ToString();

            // Проверяем, выбрано ли значение в ComboBox
            if (comboBoxValue != null)
            {
                // Создаем запрос с использованием выбранного значения из ComboBox
                string query = $"select Operations AS Операция from OPERATIONS WHERE ID_USER = '{ID_USER}'";

                // Создаем экземпляр класса для экспорта данных в Word
                ExportWord exportWord = new ExportWord();

                // Экспортируем данные в Word, используя запрос
                exportWord.export(query, ComboBoxStudent.Text);
            }
            else
            {
                // Если значение в ComboBox не выбрано, выводим сообщение об ошибке или предпринимаем другие действия
                MessageBox.Show("Ошибка.");
            }

        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            userId();

            // Получаем выбранное значение из ComboBox
            string comboBoxValue = ComboBoxStudent.SelectedItem?.ToString();

            // Проверяем, выбрано ли значение в ComboBox
            if (comboBoxValue != null)
            {
                // Создаем запрос с использованием выбранного значения из ComboBox
                string query = $"select Date AS Дата, BodyTemp AS [Температура тела], Vaccine AS Вакцина, Permission AS Разршение, StateAfterThirtyMin AS [Состояние через 30 мин.] from CHECK_VACCINE WHERE ID_USER = '{ID_USER}'";

                // Создаем экземпляр класса для экспорта данных в Word
                ExportWord exportWord = new ExportWord();

                // Экспортируем данные в Word, используя запрос
                exportWord.export(query, ComboBoxStudent.Text);
            }
            else
            {
                // Если значение в ComboBox не выбрано, выводим сообщение об ошибке или предпринимаем другие действия
                MessageBox.Show("Ошибка.");
            }

        }
    }
}
