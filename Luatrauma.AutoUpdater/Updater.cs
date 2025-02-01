using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Luatrauma.AutoUpdater.Github;

namespace Luatrauma.AutoUpdater
{
    static class Updater
    {
        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private static readonly Dictionary<PlatformID, string> PatchFilenames = new()
        {
            { PlatformID.Win32NT, "luacsforbarotrauma_patch_windows_client.zip" },
            { PlatformID.Unix, "luacsforbarotrauma_patch_linux_client.zip" },
            { PlatformID.MacOSX, "luacsforbarotrauma_patch_mac_client.zip" },
        };

        private static readonly string LuaCsForBarotraumaRepoReleaseInfoUrl =
            "https://api.github.com/repos/evilfactory/LuaCsForBarotrauma/releases/latest";

        public static async Task Update()
        {
            Logger.Log("Starting update...");

            var patchFilename = PatchFilenames[Environment.OSVersion.Platform];
            if (patchFilename == null)
            {
                Logger.Log("Unsupported operating system.");
                return;
            }

            string patchUrl;
            string patchId;
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Luatrauma.AutoUpdater/1.0");
                var jsonStr = await client.GetStringAsync(LuaCsForBarotraumaRepoReleaseInfoUrl);
                var release = JsonSerializer.Deserialize<Release>(jsonStr);
                if (release == null)
                {
                    Logger.Log("Failed to download patch release info");
                    return;
                }

                var asset = release.Assets.FirstOrDefault(x => x.Name == patchFilename);
                patchUrl = asset!.BrowserDownloadUrl;
                patchId = asset!.Id.ToString();
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to download patch release info: {e.Message}");
                return;
            }

            if (File.Exists(".luatrauma_version"))
            {
                var current = await File.ReadAllTextAsync(".luatrauma_version");
                if (current == patchId)
                {
                    Logger.Log("Update don't need");
                    return;
                }
            }

            string tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "Luatrauma.AutoUpdater.Temp");
            string patchZip = Path.Combine(tempFolder, "patch.zip");
            string extractionFolder = Path.Combine(tempFolder, "Extracted");

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
                if (Directory.Exists(extractionFolder))
                {
                    Directory.Delete(extractionFolder, true);
                }

                Directory.CreateDirectory(extractionFolder);

                ZipFile.ExtractToDirectory(patchZip, extractionFolder, true);
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to extract patch zip: {e.Message}");
                return;
            }

            Logger.Log($"Extracted patch zip to {Directory.GetCurrentDirectory()}");

            Logger.Log("Applying patch...");

            // Verify that the dll version is the same as the current one
            string currentDll = Path.Combine(Directory.GetCurrentDirectory(), "Barotrauma.dll");
            string newDll = Path.Combine(extractionFolder, "Barotrauma.dll");

            if (!File.Exists(currentDll))
            {
                Logger.Log("Failed to find the current Barotrauma.dll", ConsoleColor.Red);
                return;
            }

            if (!File.Exists(newDll))
            {
                Logger.Log("Failed to find the new Barotrauma.dll", ConsoleColor.Red);
                return;
            }

            // Grab the version of the current dll
            var currentVersion = FileVersionInfo.GetVersionInfo(currentDll);
            var newVersion = FileVersionInfo.GetVersionInfo(newDll);

            if (currentVersion == null || newVersion == null)
            {
                Logger.Log("Failed to get version info for the dlls");
                return;
            }

            if (currentVersion.FileVersion == newVersion.FileVersion)
            {
                Logger.Log($"The patch is compatible with the current game version {newVersion.FileVersion}.");
            }
            else
            {
                Logger.Log(
                    $"The patch is not compatible with the current game version {currentVersion.FileVersion} -> {newVersion.FileVersion}, aborting.");

                Logger.Log(
                    "Theres no patch available for the current game version, the game will be launched without the patch, if there was a new game update please wait until a new patch is released.",
                    ConsoleColor.Yellow);

                await Task.Delay(8000);

                return;
            }

            CopyFilesRecursively(extractionFolder, Directory.GetCurrentDirectory());

            Logger.Log($"Patch {patchId} applied.");

            if (File.Exists("luacsversion.txt")) // Workshop stuff, get rid of it so it doesn't interfere
            {
                File.Delete("luacsversion.txt");
            }

            await File.WriteAllTextAsync(".luatrauma_version", patchId);
            Logger.Log("Update completed.");
        }
    }
}