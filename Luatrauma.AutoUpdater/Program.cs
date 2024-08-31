using System.Diagnostics;

namespace Luatrauma.AutoUpdater
{
    internal class Program
    {
        public static string[] Args;

        static void Main(string[] args)
        {
            string tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "Luatrauma.AutoUpdater.Temp");
            Directory.CreateDirectory(tempFolder);

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Logger.Log("Unhandled exception: " + e.ExceptionObject);
            };

            Args = args;

            Task task = Start();
            task.Wait();
        }

        public async static Task Start()
        {
            await Updater.Update();

            if (Args.Length > 0)
            {
                Logger.Log("Starting " + string.Join(" ", Args));

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
