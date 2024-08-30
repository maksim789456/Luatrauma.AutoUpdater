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
                Logger.Log("Unsupported operating system.");
                return;
            }

            string tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "Luatrauma.AutoUpdater.Temp");
            string patchZip = Path.Combine(tempFolder, "patch.zip");

            Directory.CreateDirectory(tempFolder);

            Logger.Log($"Downloading patch zip from {patchUrl}");

            try
            {
                using var client = new HttpClient();

                byte[] fileBytes = await client.GetByteArrayAsync(patchUrl);

                await File.WriteAllBytesAsync(patchZip, fileBytes);
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to download patch zip: {e.Message}");
                return;
            }

            Logger.Log($"Downloaded patch zip to {patchZip}");

            try
            {
                ZipFile.ExtractToDirectory(patchZip, Directory.GetCurrentDirectory(), true);
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to extract patch zip: {e.Message}");
                return;
            }

            Logger.Log($"Extracted patch zip to {Directory.GetCurrentDirectory()}");
        }
    }
}
