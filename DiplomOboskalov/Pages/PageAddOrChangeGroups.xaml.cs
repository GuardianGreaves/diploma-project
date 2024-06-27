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
    /// Логика взаимодействия для PageAddOrChangeGroups.xaml
    /// </summary>
    public partial class PageAddOrChangeGroups : Page
    {
        public event EventHandler OpenAddStudentRequested;
        public int ID;
        public bool AddOrChange = false;

        public PageAddOrChangeGroups()
        {
            InitializeComponent();
        }
        public void studentget(int id, string groupName, int countPeople)
        {
            ID = id;
            TextBoxNameGoup.Text = groupName;
            TextBoxCountPeople.Text = countPeople.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connectionDataBase = new ConnectionDataBase())
                {
                    // ... (код до получения idGroup)
                    string groupName = TextBoxNameGoup.Text;
                    int countPeople = Convert.ToInt32(TextBoxCountPeople.Text);
                    if (AddOrChange == false)
                    {
                        // Используйте параметры в запросе
                        var sql = "INSERT INTO [GROUP] (GroupName, NumberPeople) " +
                                  "VALUES (@_GroupName, @_NumberPeople)";
                        // Формируем условия фильтрации
                        List<SqlParameter> parameters = new List<SqlParameter>();

                        // Добавляем условия для текстового поиска
                        parameters.Add(new SqlParameter("@_GroupName", groupName));
                        parameters.Add(new SqlParameter("@_NumberPeople", countPeople));

                        DataTable result = connectionDataBase.ExecuteSqlParameters(sql, parameters.ToArray());

                        MessageBox.Show("Данные успешно добавлены в базу данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        try
                        {
                            List<SqlParameter> parameters = new List<SqlParameter>();

                            // Используйте параметры в запросе
                            var sql = "UPDATE [GROUP] SET " +
                                      "GroupName = @_GroupName, " +
                                      "NumberPeople = @_NumberPeople " +
                                      "WHERE ID_GROUP = @_ID_GROUP";

                            parameters.Add(new SqlParameter("@_GroupName", groupName));
                            parameters.Add(new SqlParameter("@_NumberPeople", countPeople));
                            parameters.Add(new SqlParameter("@_ID_GROUP", ID));

                            DataTable dt = connectionDataBase.ExecuteSqlParameters(sql, parameters.ToArray());

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
            PageGroups pageGroups = new PageGroups();
            NavigationService.Navigate(pageGroups);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PageGroups pageGroups = new PageGroups();
            NavigationService.Navigate(pageGroups);
        }
    }
}
