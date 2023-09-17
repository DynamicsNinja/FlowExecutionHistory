using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fic.XTB.FlowExecutionHistory.Models;
using McTools.Xrm.Connection;
using Newtonsoft.Json.Linq;

namespace Fic.XTB.FlowExecutionHistory.Helpers
{
    public class BrowserLoader
    {
        private static string GetChromeProfileName(string folder)
        {
            JObject jo;
            using (var reader = new StreamReader(Path.Combine(folder, "Preferences")))
            {
                jo = JObject.Parse(reader.ReadToEnd());
            }

            var profileName = ((JValue)((JObject)jo["profile"])?["profile_name"])?.Value?.ToString();
            var name = ((JValue)((JObject)jo["profile"])?["name"])?.Value?.ToString();

            return name;
        }

        public static List<Browser> GetBrowsers()
        {
            var browsers = new List<Browser>
            {
                new Browser
                {
                    Name = "Edge",
                    Profiles = GetBrowserProfiles(BrowserEnum.Edge),
                    Executable = "msedge.exe",
                    Type = BrowserEnum.Edge
                },
                new Browser
                {
                    Name = "Chrome",
                    Profiles = GetBrowserProfiles(BrowserEnum.Chrome),
                    Executable = "chrome.exe",
                    Type = BrowserEnum.Chrome
                },
                new Browser
                {
                    Name = "Firefox",
                    Profiles = GetBrowserProfiles(BrowserEnum.Firefox),
                    Executable = "firefox.exe",
                    Type = BrowserEnum.Firefox
                },
            };

            return browsers.Where(b => b.Profiles.Count > 0).ToList();
        }

        private static List<BrowserProfile> GetBrowserProfiles(BrowserEnum browserType)
        {
            var browserProfiles = new List<BrowserProfile>();

            try
            {
                switch (browserType)
                {
                    case BrowserEnum.Chrome:
                    case BrowserEnum.Edge:
                        var path = browserType == BrowserEnum.Chrome ? "Google\\Chrome\\User Data" : "Microsoft\\Edge\\User Data";
                        var folderPaths = Directory
                            .GetDirectories(
                                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                    path), "Profile *")
                            .OrderBy(p => p);

                        foreach (var folder in folderPaths)
                        {
                            var profileName = !Directory.Exists($@"{folder}\Managed Extension Settings")
                                ? folder.EndsWith("Profile 1")
                                    ? "Personal"
                                    : GetChromeProfileName(folder)
                                : "Work Profile";

                            browserProfiles.Add(new BrowserProfile { Name = profileName, Path = Path.GetFileName(folder) });
                        }

                        var defaultFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path, "Default");
                        browserProfiles.Add(new BrowserProfile { Name = GetChromeProfileName(defaultFolder), Path = Path.GetFileName(defaultFolder) });

                        break;

                    case BrowserEnum.Firefox:
                        foreach (var folder in Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mozilla\\Firefox\\Profiles")))
                        {
                            browserProfiles.Add(new BrowserProfile { Name = folder.Split('.')[1], Path = Path.GetFileName(folder).Split('.')[1] });
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                // Browser not installed
            }

            return browserProfiles.OrderBy(bp => bp.Name).ToList();
        }
    }
}