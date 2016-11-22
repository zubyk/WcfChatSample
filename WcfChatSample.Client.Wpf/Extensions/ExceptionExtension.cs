using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace WcfChatSample.Client.Wpf.Extensions
{
    internal static class ExceptionExtension
    {
        private static string _productName = null;

        private static string ProductName
        {
            get
            {
                try
                {
                    if (_productName == null)
                        _productName = Application.Current.MainWindow.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true).OfType<AssemblyProductAttribute>().First().Product;

                    return _productName;
                }
                catch
                {
                    return "Error";
                }
            }
        }
        
        internal static void ShowMessage(this Exception e, string source = null)
        {
            Exception inner = e;

            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }

            MessageBox.Show(String.Format("{0}:{1}{2}", source != null ? source.TrimStart("Error ".ToArray()) + " error" : "Error", Environment.NewLine, e.Message),
                ProductName,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        internal static void ShowMessage(this AggregateException e, string source = null)
        {
            var sb = new StringBuilder();

            foreach (var err in e.InnerExceptions)
            {
                sb.AppendLine(err.Message);
            }

            MessageBox.Show(String.Format("Error{0}:{1}{2}", source != null ? " " + source.TrimStart("Error ".ToArray()) : "", Environment.NewLine, sb.ToString()),
                ProductName,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        internal static string GetAllMessages(this Exception e, bool full = false)
        {
            var sb = new StringBuilder();

            do
            {
                sb.AppendLine(full ? e.ToString() : e.Message);
                e = e.InnerException;
            } 
            while (e != null);
           
            return sb.ToString();

        }

        internal static string GetAllMessages(this AggregateException e, bool full = false)
        {
            var sb = new StringBuilder();
            var first = true;

            foreach (var err in e.InnerExceptions)
            {
                var inner = err;
                do
                {
                    sb.AppendLine(full ? inner.ToString() : inner.Message);
                    inner = inner.InnerException;
                }
                while (inner != null);

                if (first)
                {
                    first = false;
                    sb.AppendLine(new String('-', 20));
                }
            }

            return sb.ToString();
        }
    }
}
