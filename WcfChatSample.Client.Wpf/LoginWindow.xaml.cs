using System;
using System.Windows;
using System.Windows.Controls;
using WcfChatSample.Client.Wpf.Data;

namespace WcfChatSample.Client.Wpf
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private bool ValidateLogin()
        {
            return loginButton.IsEnabled = !String.IsNullOrWhiteSpace(loginBox.Text) && passwordBox.SecurePassword.Length > 0;
        }

        private void loginBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateLogin();
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidateLogin();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateLogin())
            {
                Close();
                Commands.CommandManager.Connect.Execute(new UserLogin(loginBox.Text, passwordBox.SecurePassword.Copy()));
            }
        }
    }
}
