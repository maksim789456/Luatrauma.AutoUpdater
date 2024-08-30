using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luatrauma.AutoUpdater
{
    internal class Logger
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);

            File.AppendAllText("Luatrauma.AutoUpdater.Temp/log.txt", message + Environment.NewLine);
        }
    }
}
