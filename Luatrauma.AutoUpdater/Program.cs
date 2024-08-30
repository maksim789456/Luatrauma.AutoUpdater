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

            if (Args.Length > 0)
            {
                Console.WriteLine("Starting " + string.Join(" ", Args));

                var info = new ProcessStartInfo
                {
                    FileName = Args[0],
                    Arguments = string.Join(" ", Args.Skip(1)),
                    WorkingDirectory = Path.GetDirectoryName(Args[0]),
                };

                Process.Start(info);
            }
        }
    }
}
