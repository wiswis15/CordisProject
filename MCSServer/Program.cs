using System;
using System.Linq;

namespace MCSServer
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            int port = 80;
            if (args.Any())
            {
                if (int.TryParse(args[0], out int newPort))
                {
                    port = newPort;
                }
            }

            // Create server with a given port
            SimpleHTTPServer myServer = new SimpleHTTPServer(port);
            Console.WriteLine("MCSServer is running on this port: " + myServer.Port.ToString());

            ConsoleKeyInfo name;
            do
            {
                Console.WriteLine("MCSServer: Press 'Q' to quit");
                name = Console.ReadKey();
            } while (!name.KeyChar.Equals('Q') && !name.KeyChar.Equals('q'));
            Console.WriteLine("\nMCSServer: '{0}' pressed, stopping server.", name.KeyChar);

            // Stop server
            myServer.Stop();
        }
    }
}
