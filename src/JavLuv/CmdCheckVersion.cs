using Common;
using MovieInfo;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace JavLuv
{

    public class Release
    {
        #region Properties

        public string url { get; set; }
        public string assets_url { get; set; }
        public string upload_url { get; set; }
        public string html_url { get; set; }

        public string tag_name { get; set; }

        public int id { get; set; }
        public string tarball_url { get; set; }
        public string zipball_rul { get; set; }
        public string body { get; set; }

        #endregion
    }

    public class CmdCheckVersion : IAsyncCommand
    {
        #region Constructors

        public CmdCheckVersion()
        {
            IsNewVersionAvailable = false;
        }

        #endregion

        #region Events

        public event EventHandler FinishedVersionCheck;

        #endregion

        #region Properties

        public bool IsNewVersionAvailable { get; private set; }
        public Release LatestRelease { get; private set; }
        public SemanticVersion LatestVersion { get; private set; }

        #endregion

        #region Public Functions

        public void Execute()
        {
            // Check for early out conditions
            if (Application.Current == null || Application.Current.Dispatcher == null)
                return;

            var task = GetLatestReleaseAsync();
            task.Wait();

            if (LatestRelease != null)
            {
                LatestVersion = new SemanticVersion(LatestRelease.tag_name);
                int compare = LatestVersion.CompareTo(SemanticVersion.Current);
                if (compare > 0)
                {
                    Logger.WriteInfo("There is a new version of JavLuv available.");
                    IsNewVersionAvailable = true;
                }
                else if (compare == 0)
                {
                    Logger.WriteInfo("JavLuv is up to date.");
                }
                else
                {
                    Logger.WriteInfo("JavLuv is currently ahead of the latest release.");
                }
            }

            // Invoke notification that we're done checking against the latest version
            if (Application.Current == null || Application.Current.Dispatcher == null)
                return;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                FinishedVersionCheck?.Invoke(this, new EventArgs());
            }));           
        }

        #endregion

        #region Private Functions

        private async Task GetLatestReleaseAsync()
        {
            string siteURL = "https://api.github.com/repos/JavLuv/JavLuv/releases/latest";
            try
            {
                Logger.WriteInfo("Checking GitHub for latest version at: " + siteURL);

                // Get latest release info from Github
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.Add("User-Agent", "JavLuv");
                var streamTask = client.GetStreamAsync(siteURL);
                LatestRelease = await System.Text.Json.JsonSerializer.DeserializeAsync<Release>(await streamTask);
            }
            catch (Exception ex)
            {
                Logger.WriteError("Issue getting latest release infor from: " + siteURL, ex);
            }
        }

        #endregion
    }
}
