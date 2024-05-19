namespace MangoStore_API.Logging
{
    public class Logging : ILogging
    {
        public void Log(string message, string type)
        {
            if (type == "error")
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss.fff tt")} ERROR - " + message);
                Console.BackgroundColor = ConsoleColor.Black;
            }

            if (type == "warning")
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss.fff tt")} WARNING - " + message);
                Console.BackgroundColor = ConsoleColor.Black;
            }

            if (type == "info")
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine($"{DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss.fff tt")} INFO - " + message);
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }
    }
}
