using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Luatrauma.AutoUpdater
{
    static class Updater
    {
        public const string TempFolder = "Luatrauma.AutoUpdater.Temp";

        public async static Task Update()
        {
            string patchUrl = null;
            if (OperatingSystem.IsWindows())
            {
                patchUrl = "https://github.com/evilfactory/LuaCsForBarotrauma/releases/download/latest/luacsforbarotrauma_patch_windows_client.zip";
            }
            else if (OperatingSystem.IsLinux())
            {
                patchUrl = "https://github.com/evilfactory/LuaCsForBarotrauma/releases/download/latest/luacsforbarotrauma_patch_linux_client.zip";
            }
            else if (OperatingSystem.IsMacOS())
            {
                patchUrl = "https://github.com/evilfactory/LuaCsForBarotrauma/releases/download/latest/luacsforbarotrauma_patch_mac_client.zip";
            }

            if (patchUrl == null)
            {
                Console.WriteLine("Unsupported operating system.");
                return;
            }

            string tempFolder = Path.Combine(Directory.GetCurrentDirectory(), TempFolder);
            string patchZip = Path.Combine(tempFolder, "patch.zip");

            Directory.CreateDirectory(tempFolder);

            Console.WriteLine("Downloading patch zip from {0}", patchUrl);

            try
            {
                using var client = new HttpClient();

                byte[] fileBytes = await client.GetByteArrayAsync(patchUrl);

                await File.WriteAllBytesAsync(patchZip, fileBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to download patch zip: {0}", e.Message);
                return;
            }

            Console.WriteLine("Downloaded patch zip to {0}", patchZip);

            try
            {
                ZipFile.ExtractToDirectory(patchZip, Directory.GetCurrentDirectory(), true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to extract patch zip: {0}", e.Message);
                return;
            }

            Console.WriteLine("Extracted patch zip to {0}", Directory.GetCurrentDirectory());
        }
    }
}
