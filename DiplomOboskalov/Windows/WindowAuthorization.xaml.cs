using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using DiplomOboskalov.Windows;
using System.Data;
using DiplomLibrary;
using System;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace OboskalovDiplom.Windows
{
    public partial class WindowAuthorization : Window
    {
        private ObservableCollection<UserLoginPassword> LoginPassword;

        int CountSignInFailed = 0;

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        public WindowAuthorization()
        {
            LoginPassword = new ObservableCollection<UserLoginPassword>();
            InitializeComponent();
            LoadStudentsData();
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            Storyboard fadeInStoryboard = (Storyboard)this.Resources["fadeOutAnimation"];
            fadeInStoryboard.Begin();
            Messageregistr.Margin = new Thickness(175, 330, 175, -240);

            if (!string.IsNullOrEmpty(TextBlockLogin.Text) && TextBoxLogin.Text.Length > 0)
            {
                TextBlockLogin.Visibility = Visibility.Hidden;
            }
            else
            {
                TextBlockLogin.Visibility = Visibility.Visible;
            }
        }

        string FIO;
        string role;

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string enteredLogin = TextBoxLogin.Text;
                string enteredPassword = TextBoxPassword.Password;

                Encrypt encryptClass = new Encrypt();
                var encryptLogin = encryptClass.encrypt(enteredLogin);
                var encryptPassword = encryptClass.encrypt(enteredPassword);

                if (Messageregistr.Opacity == 1)
                {
                    Storyboard fadeInStoryboard = (Storyboard)this.Resources["fadeOutAnimation"];
                    fadeInStoryboard.Begin();
                    Messageregistr.Margin = new Thickness(175, 330, 175, -240);
                }
                DataTable dt = null;
                string query = null;
                var user = LoginPassword.FirstOrDefault(u => u.Login == encryptLogin && u.Password == encryptPassword);
                DiplomLibrary.ConnectionDataBase dbHelper = new DiplomLibrary.ConnectionDataBase();
                if (user == null)
                {
                    CountSignInFailed++;

                    if (CountSignInFailed >= 3)
                    {
                        HandleFailedSignInWithCaptcha();
                    }
                    else
                    {
                        HandleFailedSignInWithoutCaptcha();
                    }
                }
                else
                {
                    query = $"SELECT U.ID_USER_ROLE, UR.NameRole, U.Surname + ' ' + U.Name AS FIO FROM [dbo].[USER] U INNER JOIN USER_ROLE UR ON U.ID_USER_ROLE = UR.ID_USER_ROLE WHERE U.ID_USER = '{user.ID_USER}'";
                    dt = dbHelper.ExecuteSql(query);
                    LoginPassword.Clear();
                    foreach (DataRow row in dt.Rows)
                    {
                        FIO = row["FIO"].ToString();
                        role = row["NameRole"].ToString();
                    }

                    if (user != null)
                    {
                        // Вход выполнен успешно
                        WindowMain mainPage = new WindowMain(FIO, role);
                        mainPage.Show();
                        Close();
                    }
                    else
                    {
                        CountSignInFailed++;

                        if (CountSignInFailed >= 3)
                        {
                            HandleFailedSignInWithCaptcha();
                        }
                        else
                        {
                            HandleFailedSignInWithoutCaptcha();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    

        private void HandleFailedSignInWithCaptcha()
        {
            CapthaGenerate();
            TextBlockHello.Visibility = Visibility.Collapsed;
            CaptchaConfirm.IsEnabled = true;
            TextBoxCaptha.IsEnabled = true;

            TextBoxLogin.IsEnabled = false;
            TextBoxPassword.IsEnabled = false;
            SignIn.IsEnabled = false;

            Storyboard fadeInStoryboardCaptcha = (Storyboard)this.Resources["AnimationCaptchaIn"];
            fadeInStoryboardCaptcha.Begin();

            CountSignInFailed = 0;
        }

        private void HandleFailedSignInWithoutCaptcha()
        {
            Messageregistr.Margin = new Thickness(175,315,155,5);
            ButtonCloseMessage.IsEnabled = true;

            TextBlockRegTitle.Text = "Ошибка входа!";
            TextBlockReg.Text = "Вы ввели неверный логин или пароль";

            Storyboard fadeInStoryboard = (Storyboard)this.Resources["fadeInAnimation"];
            fadeInStoryboard.Begin();
        }

        private void LoadStudentsData()
        {
            DiplomLibrary.ConnectionDataBase dbHelper = new DiplomLibrary.ConnectionDataBase();
            string query = "SELECT ID_USER, Password, Login " +
                           "FROM [dbo].[LOGIN_PASSWORD]";


            DataTable dt = dbHelper.ExecuteSql(query);
            LoginPassword.Clear();
            foreach (DataRow row in dt.Rows)
            {
                LoginPassword.Add(new UserLoginPassword
                {
                    ID_USER = row["ID_USER"].ToString(),
                    Login = row["Login"].ToString(),
                    Password = row["Password"].ToString(),
                });
            }
        }

        private void TextBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Storyboard fadeInStoryboard = (Storyboard)this.Resources["fadeOutAnimation"];
            fadeInStoryboard.Begin();
            Messageregistr.Margin = new Thickness(175, 330, 175, -240);

            if (!string.IsNullOrEmpty(TextBlockPassword.Text) && TextBoxPassword.Password.Length > 0)
            {
                TextBlockPassword.Visibility = Visibility.Hidden;
            }
            else
            {
                TextBlockPassword.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonCloseMessage_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMessage.IsEnabled = false;

            Storyboard fadeInStoryboard = (Storyboard)this.Resources["fadeOutAnimation"];
            fadeInStoryboard.Begin();

            TextBoxLogin.IsEnabled = true;
            TextBoxPassword.IsEnabled = true;
            SignIn.IsEnabled = true;

            Messageregistr.Margin = new Thickness(175, 330, 175, -240);
        }


        public partial class UserLoginPassword
        {
            public string ID_USER { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }


        string CaptchaText;
        void CapthaGenerate()
        {
            Captcha captchaGenerator = new Captcha(120, 36);
            CaptchaText = captchaGenerator.CaptchaText;
            BitmapImage _captchaImage = captchaGenerator.GenerateCaptchaBitmapImage();
            captchaImage.Source = _captchaImage;
        }

        private void TextBoxCaptha_TextChanged(object sender, TextChangedEventArgs e)
        {
            SolidColorBrush redBrush = new SolidColorBrush(Colors.Gray);
            CapchaTextBox.BorderBrush = redBrush;
        }

        private void CaptchaConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (CaptchaText == TextBoxCaptha.Text)
            {

                CaptchaConfirm.IsEnabled = false;
                TextBoxCaptha.IsEnabled = false;

                Storyboard fadeInStoryboard = (Storyboard)this.Resources["AnimationCaptchaOut"];
                fadeInStoryboard.Begin();

                TextBlockHello.Visibility = Visibility.Visible;

                TextBoxLogin.IsEnabled = true;
                TextBoxPassword.IsEnabled = true;
                SignIn.IsEnabled = true;

                TextBoxCaptha.Clear();
            }
            else
            {
                SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
                CapchaTextBox.BorderBrush = redBrush;
                TextBloxCapchaText.Text = "Вы непарвильно ввели капчу, введите капчу снова.";
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CapthaGenerate();
        }

        private void TextBoxLogin_GotFocus(object sender, RoutedEventArgs e)
        {
        }
    }
}
