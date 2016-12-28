using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDataKzBot
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Bot Start");
                if (Helper.LoadSettings())
                {
                    BodyBot.StartBotAsync().Wait();
                }
                Console.WriteLine("Bot Stop");
            }
            catch (Exception error)
            {
                Console.WriteLine("Error: " + error.Message);
                LogService.SendException(error, Comment: "Main - Преложение остановлено.");
            }

            Console.WriteLine("\r\nНажмите Enter для закрытия окна...");
            Console.ReadLine();
        }
    }
}
