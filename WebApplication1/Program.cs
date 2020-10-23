using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using MCDBackend;
using MCDBackend.McsClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApplication1.Data;

namespace WebApplication1
{
    public class MainClass
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(MainClass));
        private static string _inputFilename = "BackendConfig.xml";

        private static DataManager dataManager;

        public delegate void McdClientDelegate(JsonClientRequest request);

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static void Main(string[] args)
        {
            // Instantiate the delegate.
             McdClientDelegate mcdClientHandler = DelegateMethod;

             // Read command line arguments (input filename)
             if (args.Count() == 1 && !string.IsNullOrEmpty(args[0]))
             {
                 _inputFilename = args[0];
             }

             dataManager = new DataManager();
             dataManager.Initialize(_inputFilename);


             // Start frontend server (listening to frontend requests)
             //McdClientListener myServer = new McdClientListener(8090, mcdClientHandler);
             //log.Info("ClientListener is running on this port: " + myServer.Port.ToString());

             dataManager.StartTimers();

             CreateHostBuilder(args).Build().Run();

             // Keep backend running (till quit command is given)
             ConsoleKeyInfo name;
             log.Info("Press 'Q' to quit");
             do
             {
                 name = Console.ReadKey();
             } while (!name.KeyChar.Equals('Q') && !name.KeyChar.Equals('q'));
             log.Info("\n'" + name.KeyChar + "' pressed, stopping application.");

             dataManager.Dispose();
             //myServer.Stop();
        }

        // Create a method for a delegate.
        public static void DelegateMethod(JsonClientRequest request)
        {
            Console.WriteLine("Delegate method called with message: " + request.ToString());
            foreach (JsonVariables variable in request.registervars)
            {
                dataManager.getVariableObject(variable.path)?.AddClient(request.address, request.port);
            }
        }
    }
}
