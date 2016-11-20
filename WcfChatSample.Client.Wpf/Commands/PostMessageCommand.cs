using System;
using System.Windows;
using System.Windows.Input;

namespace WcfChatSample.Client.Wpf.Commands
{
    partial class CommandManager
    {
        private static ICommand _PostMessageCommand = new PostMessageCommand();

        public static ICommand PostMessage
        {
            get
            {
                return _PostMessageCommand;
            }
        }

        private class PostMessageCommand : ChatCommandBase
        {
            public override bool CanExecute(object parameter)
            {
                return parameter is String && base.CanExecute(parameter) && Data.DataSource.Current.IsConnected;
            }
            
            public override void Execute(object parameter)
            {
                var msg = parameter as String;

                if (msg != null)
                {
                    Data.DataSource.Current.Post(msg);
                }
            }
        }
    }
}
