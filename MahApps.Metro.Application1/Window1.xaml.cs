using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;


namespace MahApps.Metro.Application1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : MetroWindow
    {

        public bool not_re = false;

        public Window1()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            chng_form.Visibility = Visibility.Hidden;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            if (not_re)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (pass.Password == "shoelace")
            {
                login_form.Visibility = Visibility.Hidden;
                chng_form.Visibility = Visibility.Visible;
                err.Visibility = Visibility.Collapsed;
            }
            else
            {
                err.Visibility = Visibility.Visible;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Application1.Properties.Settings.Default.subnet = subnet.Text;
            Application1.Properties.Settings.Default.mac = mac.Text;
            Properties.Settings.Default.Save();
            await this.ShowMessageAsync("Settings", "Settings saved.");
            show_restart();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            show_restart();
        }

        private async void show_restart()
        {
            if (not_re)
            {
                await this.ShowMessageAsync("Restart", "The application needs to be restarted to reflect the changes. Press OK to continue.");
                System.Windows.Forms.Application.Restart();
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}
