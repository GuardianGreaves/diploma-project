using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для PageCsvFile.xaml
    /// </summary>
    public partial class PageCsvFile : Page
    {
        public Action<object, EventArgs> OpenSCVRequested { get; internal set; }

        public PageCsvFile()
        {
            InitializeComponent();
        }

        private void LoadData()
        {
            // Вызывать диалог открытия файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Выберите CSV файл"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Получить путь к выбранному файлу
                string csvFilePath = openFileDialog.FileName;

                // Получить имя файла
                string fileName = System.IO.Path.GetFileName(csvFilePath);
                TextBlocHolder.Text = "Просмотр CSV файла - " + fileName;

                // Чтение данных из CSV файла с использованием разделителя ;
                List<dynamic> records = new List<dynamic>();
                string[] headers;

                using (var parser = new TextFieldParser(csvFilePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(";");

                    // Прочитать заголовки
                    headers = parser.ReadFields();

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        var record = new System.Dynamic.ExpandoObject();
                        var recordDict = record as IDictionary<string, object>;

                        for (int i = 0; i < fields.Length; i++)
                        {
                            // Используем заголовки для именования полей
                            recordDict[headers[i]] = fields[i];
                        }

                        records.Add(record);
                    }
                }

                // Очистка существующих колонок в DataGrid
                CsvDataGrid.Columns.Clear();

                // Привязка данных к DataGrid
                CsvDataGrid.ItemsSource = records;

                // Создание столбцов в DataGrid динамически
                if (headers != null && headers.Length > 0)
                {
                    foreach (var columnHeader in headers)
                    {
                        CsvDataGrid.Columns.Add(new DataGridTextColumn
                        {
                            Header = columnHeader,
                            Binding = new System.Windows.Data.Binding(columnHeader)
                        });
                    }
                }
            }
        }

        private void ButtonAddStudent_Click(object sender, RoutedEventArgs e)
        {
            TextBlockMessage.Visibility = Visibility.Collapsed;
            LoadData();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            TextBlockMessage.Visibility = Visibility.Visible;
            CsvDataGrid.ItemsSource = null;
            CsvDataGrid.Columns.Clear();
            TextBlocHolder.Text = "Просмотр CSV файлов";
        }
    }
}
