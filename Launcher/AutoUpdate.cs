namespace Launcher
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net;

    public static class AutoUpdate
    {
        private static string upgrade_url = "https://api.github.com/repos/zaafar/GameOverlay/releases/latest";
        private static string version_file_name = "VERSION.txt";
        private static string release_file_name = "release.zip";
        private static JObject get_latest_version_info()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(upgrade_url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Method = "GET";
            httpWebRequest.UserAgent = "curl/7.83.0";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using var streamReader = new StreamReader(httpResponse.GetResponseStream());
            var jsonObject = JsonConvert.DeserializeObject<JObject>(streamReader.ReadToEnd());
            return jsonObject;
        }

        private static string extract_download_url(JObject info)
        {
            var asserts = info["assets"];
            foreach (var assert in asserts)
            {
                if (assert["name"].ToString() == release_file_name)
                {
                    return assert["browser_download_url"].ToString();
                }
            }

            return string.Empty;
        }

        public static void UpgradeGameHelper(string gameHelperDir)
        {
            var versionFile = Path.Combine(gameHelperDir, version_file_name);
            if (!File.Exists(versionFile))
            {
                Console.WriteLine($"{versionFile} is missing, skipping upgrade process.");
                return;
            }

            var currentVersion = File.ReadAllText(versionFile).Trim();
            var info = get_latest_version_info();
            var latestVersion = string.Empty;
            if (info["tag_name"] != null)
            {
                latestVersion = info["tag_name"].ToString();
            }
            else
            {
                Console.WriteLine($"Failed to upgrade because I couldn't find tag in {info}.");
                return;
            }

            var downloadUrl = extract_download_url(info);
            if (string.IsNullOrEmpty(downloadUrl))
            {
                Console.WriteLine($"Upgrade failed because I couldn't find {release_file_name} in {info}.");
                return;
            }

            if (currentVersion != latestVersion)
            {
                Console.WriteLine($"Your version is {currentVersion}. Latest version is {latestVersion}, downloading now...");
                using var client = new WebClient();
                try
                {
                    client.DownloadFile(downloadUrl, release_file_name);
                    ZipFile.ExtractToDirectory(release_file_name, gameHelperDir, true);
                }
                finally
                {
                    if (File.Exists(release_file_name))
                    {
                        File.Delete(release_file_name);
                    }
                }

            }
        }
    }
}
