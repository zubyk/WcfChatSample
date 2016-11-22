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

namespace WcfChatSample.Client.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Data.DataSource.Current.PropertyChanged += Current_PropertyChanged;
            Commands.CommandManager.MessagePosted += CommandManager_MessagePosted;
        }

        private void Current_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "IsConnected":
                    messageBox.Text = null;
                    break;
            }
        }

        private void CommandManager_MessagePosted(object sender, bool e)
        {
            if (e)
            {
                messageBox.Text = null;
            }
        }

        private void messageBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Commands.CommandManager.FireCanExecuteChanged(Commands.CommandManager.PostMessage);
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var item = sender as FrameworkElement;

            if (item != null)
            {
                item.BringIntoView();
            }
        }
    }
}
