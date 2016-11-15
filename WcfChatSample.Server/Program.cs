using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WcfChatSample.Service;

namespace WcfChatSample.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Log(Assembly.GetExecutingAssembly().GetName().Name + " v." + Assembly.GetExecutingAssembly().GetName().Version + " started");

            ChatService.Initialize(null);
            ChatService.LogMessage += service_LogMessage;

            using (var host = new ServiceHost(typeof(ChatService)))
            {
                host.Open();

                Log(String.Format("Chat server open at: {0}", String.Join(", ", host.BaseAddresses.Select(u => u.ToString()).ToArray())));
                Log("Press any key to terminate server...");
                Console.ReadLine();
            }
        }

        static void service_LogMessage(object sender, string msg)
        {
            Log(msg);
        }

        static void Log(string message)
        {
            Console.WriteLine("[{0}] {1}", DateTime.Now, message);
        }
    }
}
