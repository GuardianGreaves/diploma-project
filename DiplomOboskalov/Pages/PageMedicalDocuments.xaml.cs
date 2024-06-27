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
    /// Логика взаимодействия для PageMedicalDocuments.xaml
    /// </summary>
    public partial class PageMedicalDocuments : Page
    {
        public event EventHandler OpenTraumaRequested;
        public event EventHandler OpenPreventiveVaccineRequested;

        public PageMedicalDocuments()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Вызываем событие, указывающее на запрос открытия формы добавления студента
            OpenTraumaRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenPreventiveVaccineRequested?.Invoke(this, EventArgs.Empty);
        }

        private void PackIconMaterial_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PageSurveys pageSurveys = new PageSurveys();
            NavigationService.Navigate(pageSurveys);
        }

        private void PackIconMaterial_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            PagePastInfectiousDiases page = new PagePastInfectiousDiases();
            NavigationService.Navigate(page);
        }

        private void PackIconMaterial_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            PagePassesForIllness page = new PagePassesForIllness();
            NavigationService.Navigate(page);
        }

        private void PackIconMaterial_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            PageHospitalTreatment page = new PageHospitalTreatment();
            NavigationService.Navigate(page);
        }

        private void PackIconMaterial_MouseDown_4(object sender, MouseButtonEventArgs e)
        {
            PageSanitarySpaTreatment page = new PageSanitarySpaTreatment();
            NavigationService.Navigate(page);
        }

        private void PackIconMaterial_MouseDown_5(object sender, MouseButtonEventArgs e)
        {
            PageCheckVaccine page = new PageCheckVaccine();
            NavigationService.Navigate(page);
        }

        private void PackIconMaterial_MouseDown_6(object sender, MouseButtonEventArgs e)
        {
            PageDataMedicalCheckups page = new PageDataMedicalCheckups();
            NavigationService.Navigate(page);
        }

        private void PackIconMaterial_MouseDown_7(object sender, MouseButtonEventArgs e)
        {
            PageOperations page = new PageOperations();
            NavigationService.Navigate(page);
        }
    }
}
