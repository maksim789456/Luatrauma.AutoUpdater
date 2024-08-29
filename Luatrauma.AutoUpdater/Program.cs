using System.Diagnostics;

namespace Luatrauma.AutoUpdater
{
    internal class Program
    {
        public static string[] Args;

        static void Main(string[] args)
        {
            Args = args;

            Task task = Start();
            task.Wait();
        }

        public async static Task Start()
        {
            await Updater.Update();

            Console.WriteLine("Update completed.");

            if (Args.Length > 1)
            {
                Console.WriteLine("Starting " + string.Join(" ", Args));
                Process process = Process.Start(Args[0], Args.Skip(1).ToArray());
                await process.WaitForExitAsync();
            }
        }
    }
}
