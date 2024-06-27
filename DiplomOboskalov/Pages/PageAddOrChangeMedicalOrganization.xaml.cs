using DiplomLibrary;
using DiplomLibrary.DataBase;
using DiplomOboskalov.Windows;
using System;
using System.Collections.Generic;
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
using static DiplomOboskalov.Pages.PageTrauma;

namespace DiplomOboskalov.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageAddOrChangeMedicalOrganization.xaml
    /// </summary>
    public partial class PageAddOrChangeMedicalOrganization : Page
    {
        public event EventHandler OpenAddMedicalOrganizationRequested;
        public int ID;
        public bool AddOrChange = false;



        public PageAddOrChangeMedicalOrganization()
        {
            InitializeComponent();
        }

        public void studentget(int id, string medicalOrganizationName, string medicalOrganizationAddress)
        {
            ID = id;
            TextBoxAddress.Text = medicalOrganizationAddress.ToString();
            TextBoxNameMedicalOrganization.Text = medicalOrganizationName.ToString();
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ConnectionDataBase connectionDb = new ConnectionDataBase())
                {
                    // ... (код до получения idGroup)
                    string _Name = TextBoxNameMedicalOrganization.Text;
                    string _Address = TextBoxAddress.Text;
                    if (AddOrChange == false)
                    {
                        string sql = "INSERT INTO [dbo].[MEDICAL_ORGANIZATION] (Name, Address) " +
                                     "VALUES (@Name, @Address) ";

                        // Формируем условия фильтрации
                        List<SqlParameter> parameters = new List<SqlParameter>();

                        // Добавляем условия для текстового поиска
                        parameters.Add(new SqlParameter("@Name", _Name));
                        parameters.Add(new SqlParameter("@Address", _Address));

                        DataTable result = connectionDb.ExecuteSqlParameters(sql, parameters.ToArray());


                        MessageBox.Show("Данные успешно добавлены в базу данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        try
                        {
                            List<SqlParameter> parameters = new List<SqlParameter>();

                            // Используйте параметры в запросе
                            var sql = "UPDATE [MEDICAL_ORGANIZATION] SET " +
                                        "Name = @_Name, " +
                                        "Address = @_Address " +
                                        "WHERE ID_MEDICAL_ORGANIZATION = @_ID_MEDICAL_ORGANIZATION";

                            parameters.Add(new SqlParameter("@_Name", _Name));
                            parameters.Add(new SqlParameter("@_Address", _Address));
                            parameters.Add(new SqlParameter("@_ID_MEDICAL_ORGANIZATION", ID));

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
            PageMedicalOrganization pageMedicalOrganization = new PageMedicalOrganization();
            NavigationService.Navigate(pageMedicalOrganization);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PageMedicalOrganization pageMedicalOrganization = new PageMedicalOrganization();
            NavigationService.Navigate(pageMedicalOrganization);
        }
    }
}
