using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using OfficeOpenXml;

namespace DiplomOboskalov.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageExcel.xaml
    /// </summary>
    public partial class PageExcel : Page
    {
        public Action<object, EventArgs> OpenExcelRequested { get; internal set; }

        public PageExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            InitializeComponent();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            TextBlockMessage.Visibility = Visibility.Collapsed;
            LoadData();
        }
        private void LoadData()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*",
                Title = "Выберите файл Excel"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string excelFilePath = openFileDialog.FileName;

                try
                {
                    using (ExcelPackage package = new ExcelPackage(new System.IO.FileInfo(excelFilePath)))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        DataTable dataTable = new DataTable();

                        foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                        {
                            dataTable.Columns.Add(firstRowCell.Text);
                        }

                        for (int rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                        {
                            var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                            var newRow = dataTable.Rows.Add();
                            foreach (var cell in row)
                            {
                                newRow[cell.Start.Column - 1] = cell.Text;
                            }
                        }

                        CsvDataGrid.ItemsSource = dataTable.DefaultView;

                        // Опционально: Создание столбцов в DataGrid динамически
                        CsvDataGrid.Columns.Clear();
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            CsvDataGrid.Columns.Add(new DataGridTextColumn
                            {
                                Header = column.ColumnName,
                                Binding = new System.Windows.Data.Binding(column.ColumnName)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ButtonCloseFile_Click(object sender, RoutedEventArgs e)
        {
            TextBlockMessage.Visibility = Visibility.Visible;
            CsvDataGrid.ItemsSource = null;
            CsvDataGrid.Columns.Clear();
            TextBlocHolder.Text = "Просмотр Excel файлов";
        }
    }
}
