using System;
using WcfChatSample.Client.Wpf.Extensions;

namespace WcfChatSample.Client.Wpf.Data
{
    partial class DataSource
    {
        private static DataSource self = null;

        public static bool FailedOnStart { get; private set; }

        public static DataSource GetInstance()
        {
            return Current;
        }

        public static DataSource Current
        {
            get
            {               
                if (App.Current != null && self == null && !FailedOnStart)
                {
                    try
                    {
                        self = new DataSource();
                    }
                    catch (Exception e)
                    {
                        e.ShowMessage("DataSource create");

                        FailedOnStart = true;
                        self = null;

                        App.Current.Shutdown();
                    }
                }

                return self;
            }
        }
    }
}
