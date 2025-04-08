using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using System.Windows.Threading;
using Rul.Entities;
using Rul.Services;

namespace Rul.Pages
{
    /// <summary>
    /// Логика взаимодействия для Autho.xaml
    /// </summary>
    public partial class Autho : Page
    {
        private DispatcherTimer timer;
        private int remainingTime;
        int click;

        public Autho()
        {
            InitializeComponent();
            CreateTimer();
            click = 0;
        }

        private void CreateTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void btnEnterGuests_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(null, null);
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            click += 1;
            string login = txtbLogin.Text.Trim();
            string password = pswbPassword.Password.Trim();

            var context = TradeEntities.GetContext();

            var user = context.User.Where(x => x.UserLogin == login && x.UserPassword == password).FirstOrDefault();

            if (click == 1)
            {
                if (user != null)
                {
                    txtbLogin.Clear();
                    pswbPassword.Clear();
                    LoadPage(user, user.UserRole.ToString());
                }
                else
                {
                    MessageBox.Show("Вы ввели логин или пароль неверно!");
                    GenerateCapctcha();

                    pswbPassword.Clear();

                    tblCaptcha.Visibility = Visibility.Visible;
                    tblCaptcha.Text = CaptchaGenerator.GenerateCaptchaText(6);
                }
            }
            else if (click > 1)
            {
                if (click == 3)
                {
                    BlockControls();

                    remainingTime = 10;
                    txtbTimer.Visibility = Visibility.Visible;
                    timer.Start();
                }

                if (user != null && tbCaptcha.Text == tblCaptcha.Text)
                {
                    txtbLogin.Clear();
                    pswbPassword.Clear();
                    tblCaptcha.Text = "Text";
                    tbCaptcha.Text = "";
                    tbCaptcha.Visibility = Visibility.Hidden;
                    tblCaptcha.Visibility = Visibility.Hidden;
                    LoadPage(user, user.UserRole.ToString());
                }
                else
                {

                    tblCaptcha.Text = CaptchaGenerator.GenerateCaptchaText(6);
                    tbCaptcha.Text = "";
                    MessageBox.Show("Пройдите капчу заново!");
                }
            }
        }

        private void LoadPage(User user, string role)
        {
            click = 0;
            switch (role)
            {
                case "1":
                    NavigationService.Navigate(new Admin(user));
                    break;
                case "2":
                    NavigationService.Navigate(new Client(user));
                    break;
                case "3":
                    NavigationService.Navigate(new Client(user));
                    break;
                default:
                    NavigationService.Navigate(new Client(user));
                    break;
            }
        }

        private void GenerateCapctcha()
        {
            tbCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Visibility = Visibility.Visible;

            string capctchaText = CaptchaGenerator.GenerateCaptchaText(6);
            tblCaptcha.Text = capctchaText;
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;
        }

        private void BlockControls()
        {
            txtbLogin.IsEnabled = false;
            pswbPassword.IsEnabled = false;
            tbCaptcha.IsEnabled = false;
            btnEnterGuests.IsEnabled = false;
            btnEnter.IsEnabled = false;
        }

        private void UnlockControls()
        {
            txtbLogin.IsEnabled = true;
            pswbPassword.IsEnabled = true;
            tbCaptcha.IsEnabled = true;
            btnEnterGuests.IsEnabled = true;
            btnEnter.IsEnabled = true;
            txtbLogin.Clear();
            pswbPassword.Clear();
            tblCaptcha.Text = "Text";
            tbCaptcha.Text = "";
            tbCaptcha.Visibility = Visibility.Hidden;
            tblCaptcha.Visibility = Visibility.Hidden;
            click = 0;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            if (remainingTime <= 0)
            {
                timer.Stop();
                UnlockControls();
                txtbTimer.Visibility = Visibility.Hidden;
                return;
            }

            txtbTimer.Text = $"Оставшееся время: {remainingTime} секунд";
        }
    }
}
