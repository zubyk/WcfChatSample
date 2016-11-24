using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using WcfChatSample.Service;

namespace WcfChatSample.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Log(Assembly.GetExecutingAssembly().GetName().Name + " v." + Assembly.GetExecutingAssembly().GetName().Version + " started\n\r");

            Log("Initialize DBProvider...");
            var db = new DB.SqliteDbProvider();

            var adminUsr = ConfigurationManager.AppSettings["AdminUsername"];
            var adminPass = ConfigurationManager.AppSettings["AdminPassword"];
            
            if (!String.IsNullOrWhiteSpace(adminUsr) && !String.IsNullOrWhiteSpace(adminPass))
            {
                Log("Set admin account [{0}/{1}]...\n\r", adminUsr, adminPass);
                db.SetAdmin(adminUsr, adminPass);
            }
            else
            {
                Log("No correct admin credentials found in config file. Admin account don`t created\n\r");
            }

            Log("Initialize ChatService...\n\r");
            ChatService.LogMessage += service_LogMessage;
            ChatService.Initialize(db);
            
            using (var host = new ServiceHost(typeof(ChatService)))
            {
                host.Open();

                Log(String.Format("Chat server started at {0}", String.Join(", ", host.BaseAddresses.Select(u => u.ToString()).ToArray())));
                Log("Press any key to terminate server...\n\r");
                Console.ReadLine();
            }
        }

        private static void service_LogMessage(object sender, string msg)
        {
            Log(msg);
        }

        private static void Log(string message)
        {
            Console.WriteLine("[{0}] {1}", DateTime.Now, message);
        }

        private static void Log(string message, params object[] args)
        {
            Log(String.Format(message, args));
        }
    }
}
