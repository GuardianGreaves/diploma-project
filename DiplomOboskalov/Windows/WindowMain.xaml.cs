using DiplomOboskalov.Pages;
using LiveChartsCore.Measure;
using Microsoft.Office.Interop.Word;
using OboskalovDiplom.Windows;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Navigation;

namespace DiplomOboskalov.Windows
{
    public partial class WindowMain : System.Windows.Window
    {
        string student;
        string role;
        public WindowMain(string _FIO, string _role)
        {
            try
            {
                InitializeComponent();
                TitleFIO.Text = _FIO;
                student = _FIO;
                TitleJob.Text = _role;
                role = _role;

                if (_role == "Студент")
                {
                    ButtonStudents.Visibility = Visibility.Collapsed;
                    ButtonGroups.Visibility = Visibility.Collapsed;
                    ButtonOrganization.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ButtonStudents.Visibility = Visibility.Visible;
                    ButtonGroups.Visibility = Visibility.Visible;
                    ButtonOrganization.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private bool isMenuCollapsed = false;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isMenuCollapsed)
            {
                ColumnMenu.Width = new GridLength(236);
                Storyboard expandStoryboard = (Storyboard)this.Resources["ReverseAnimationMenuLeft"];
                expandStoryboard.Begin();
                IconMenu.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.ChevronLeft;
            }
            else
            {
                ColumnMenu.Width = new GridLength(80);
                Storyboard collapseStoryboard = (Storyboard)this.Resources["AnimationMenuLeft"];
                collapseStoryboard.Begin();
                IconMenu.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.ChevronRight;
            }

            // Переключаем состояние меню
            isMenuCollapsed = !isMenuCollapsed;

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            PageStudents studentsPage = new PageStudents();
            studentsPage.OpenAddStudentRequested += StudentsPage_OpenAddStudentRequested;
            mainFrame.Navigate(studentsPage);
        }

        private void StudentsPage_OpenAddStudentRequested(object sender, EventArgs e)
        {
            mainFrame.Navigate(new PageAddOrChangeStudents());
        }

        private void StudentsPage_OpenAddGroupRequested(object sender, EventArgs e)
        {
            mainFrame.Navigate(new PageAddOrChangeGroups());
        }
        public void StudentsPage_OpenAddMedicalOrganizationRequested(object sender, EventArgs e)
        {
            mainFrame.Navigate(new PageAddOrChangeMedicalOrganization());
        }

        private WindowState _previousWindowState = WindowState.Normal;
        private double _previousMaxHeight = SystemParameters.WorkArea.Height;
        private bool _isFullScreen = false;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!IsFullScreen())
            {
                // Сохраняем текущие значения WindowState и MaxHeight
                _previousWindowState = WindowState;
                _previousMaxHeight = MaxHeight;

                // Переходим в полноэкранный режим
                effect.BlurRadius = 0;
                BorderView.CornerRadius = new CornerRadius(30, 0, 0, 30);
                mainBorder.CornerRadius = new CornerRadius(0);
                mainBorder.Margin = new Thickness(0);

                WindowState = WindowState.Normal; // Чтобы изменение размера работало корректно
                MaxHeight = SystemParameters.WorkArea.Height;
                WindowState = WindowState.Maximized;

                _isFullScreen = true;
            }
            else
            {
                // Возвращаемся к предыдущему состоянию окна
                effect.BlurRadius = 10;
                BorderView.CornerRadius = new CornerRadius(30);
                mainBorder.Margin = new Thickness(10);
                mainBorder.CornerRadius = new CornerRadius(30);

                WindowState = _previousWindowState;
                MaxHeight = _previousMaxHeight;

                _isFullScreen = false;
            }
        }

        private bool IsFullScreen()
        {
            return _isFullScreen;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CSVPage_OpenSCVRequested(object sender, EventArgs e)
        {
            //mainFrame.Navigate(new ReadWriteCsvFile());
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            PageCsvFile pageCsvFile = new PageCsvFile();
            mainFrame.Navigate(pageCsvFile);
        }

        private void ChartPage_OpenChartRequested(object sender, EventArgs e)
        {
            //mainFrame.Navigate(new Chart());
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            PageChart pageChart = new PageChart();
            mainFrame.Navigate(pageChart);
        }
        private void ReadWriteExcelFilePage_OpenExcelRequested(object sender, EventArgs e)
        {
            //mainFrame.Navigate(new ReadWriteExcelFilePage());
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            PageExcel pageExcel = new PageExcel();
            mainFrame.Navigate(pageExcel);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            PageGroups pageGroups = new PageGroups();
            pageGroups.OpenAddGroupRequested += StudentsPage_OpenAddGroupRequested;
            mainFrame.Navigate(pageGroups);
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            PageMedicalOrganization pageMedicalOrganization = new PageMedicalOrganization();
            pageMedicalOrganization.OpenAddMedicalOrganizationRequested += StudentsPage_OpenAddMedicalOrganizationRequested;
            mainFrame.Navigate(pageMedicalOrganization);
        }

        public void x()
        {
            PageMedicalOrganization pageMedicalOrganization = new PageMedicalOrganization();
            mainFrame.Navigate(pageMedicalOrganization);
        }

        private void PageMedicalOrganization_OpenPageMedicalOrganization(object sender, EventArgs e)
        {
            mainFrame.Navigate(new PageMedicalOrganization());
        }



        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            PageMedicalDocuments pageMedicalDocuments = new PageMedicalDocuments();
            mainFrame.Navigate(pageMedicalDocuments);
            pageMedicalDocuments.OpenTraumaRequested += PageMedicalDocuments_OpenPageTrauma;
            pageMedicalDocuments.OpenPreventiveVaccineRequested += PageMedicalDocuments_OpenPagePreventiveVaccine;
        }

        private void PageMedicalDocuments_OpenPageTrauma(object sender, EventArgs e)
        {
            mainFrame.Navigate(new PageTrauma());
        }
        private void PageMedicalDocuments_OpenPagePreventiveVaccine(object sender, EventArgs e)
        {
            mainFrame.Navigate(new PagePreventiveVaccine());
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if (role == "Студент")
                mainFrame.Navigate(new PageReports(student));
            else
                mainFrame.Navigate(new PageReports(null));
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ButtonUser_Click(object sender, RoutedEventArgs e)
        {
            PageUsers page = new PageUsers();
            mainFrame.Navigate(page);
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            WindowAuthorization windowAuthorization = new WindowAuthorization();
            windowAuthorization.Show();
            Close();
        }
    }
}
