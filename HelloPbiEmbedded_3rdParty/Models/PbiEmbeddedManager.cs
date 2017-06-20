using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace HelloPbiEmbedded_3rdParty.Models {

    public class EmbedConfiguration {
    public string Id { get; set; }
    public string EmbedUrl { get; set; }
    public EmbedToken EmbedToken { get; set; }
    public int MinutesToExpiration {
      get {
        var minutesToExpiration = EmbedToken.Expiration.Value - DateTime.UtcNow;
        return minutesToExpiration.Minutes;
      }
    }
    public string ErrorMessage { get; internal set; }
  }

  public class PbiEmbeddedManager {

    // Note that aadAuthorizationEndpoint is unneeded in 3rd party embedding - just a direct call to token endpoint
    static string aadAuthorizationEndpoint = "https://login.windows.net/common/oauth2/authorize";

    static string aadTokenEndpoint = "https://login.windows.net/common/oauth2/token";
    static string resourceUri = "https://analysis.windows.net/powerbi/api";
    static string urlPowerBiRestApiRoot = "https://api.powerbi.com/";

    static string clientId = ConfigurationManager.AppSettings["clientId"];
    static string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
    static string redirectUri = ConfigurationManager.AppSettings["redirectUri"];

    static string pbiUserName = ConfigurationManager.AppSettings["pbiUserName"];
    static string pbiUserPassword = ConfigurationManager.AppSettings["pbiUserPassword"];

    static string appWorkspaceId = ConfigurationManager.AppSettings["appWorkspaceId"];

    static string GetAccessTokenForNativeClient() {
      AuthenticationContext authContext = new AuthenticationContext(aadAuthorizationEndpoint);
      var userCredentials = new UserPasswordCredential(pbiUserName, pbiUserPassword);
      string token = authContext.AcquireTokenAsync(resourceUri, clientId, userCredentials).Result.AccessToken;
      return token;
    }

    static string GetAccessToken() {

      var client = new HttpClient();
      string urlTokenEndpoint = aadTokenEndpoint;
      client.BaseAddress = new Uri(urlTokenEndpoint);

      var content = new FormUrlEncodedContent(new[] {
        new KeyValuePair<string, string>("resource", resourceUri),
        new KeyValuePair<string, string>("client_id", clientId),
        new KeyValuePair<string, string>("client_secret", clientSecret),
        new KeyValuePair<string, string>("grant_type", "password"),
        new KeyValuePair<string, string>("username", pbiUserName),
        new KeyValuePair<string, string>("password", pbiUserPassword)
      });

      var responseText = client.PostAsync(urlTokenEndpoint, content).Result.Content.ReadAsStringAsync().Result;
      JsonWebToken jwt = JsonWebToken.Deserialize(responseText);
      return jwt.access_token;
    }

    static PowerBIClient GetPowerBiClient() {
      var tokenCredentials = new TokenCredentials(GetAccessToken(), "Bearer");
      return new PowerBIClient(new Uri(urlPowerBiRestApiRoot), tokenCredentials);
    }

    public static async Task<HomeViewModel> GetHomeView() {
      var client = GetPowerBiClient();
      var workspaces = (await client.Groups.GetGroupsAsync()).Value;
      var workspace = workspaces.Where(ws => ws.Id == appWorkspaceId).FirstOrDefault();
      var viewModel = new HomeViewModel {
        WorkspaceName = workspace.Name,
        WorkspaceId = workspace.Id
      };
      return viewModel;
    }

    public static async Task<ReportsViewModel> GetReports(string reportId) {

      var client = GetPowerBiClient();
      var reports = (await client.Reports.GetReportsInGroupAsync(appWorkspaceId)).Value;

      var viewModel = new ReportsViewModel {
        Reports = reports.ToList()
      };

      if (!string.IsNullOrEmpty(reportId)) {
        Report report = reports.Where(r => r.Id == reportId).First();
        var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
        var token = client.Reports.GenerateTokenInGroupAsync(appWorkspaceId, report.Id, generateTokenRequestParameters).Result;

        var embedConfig = new EmbedConfiguration() {
          EmbedToken = token,
          EmbedUrl = report.EmbedUrl,
          Id = report.Id
        };


        viewModel.CurrentReport = new ReportViewModel {
          Report = report,
          EmbedConfig = embedConfig
        };

      }

      return viewModel;
    }

    public static async Task<DashboardsViewModel> GetDashboards(string dashboardId) {

      var client = GetPowerBiClient();
      var dashboards = (await client.Dashboards.GetDashboardsInGroupAsync(appWorkspaceId)).Value;

      var viewModel = new DashboardsViewModel {
        Dashboards = dashboards.ToList()
      };

      if (!string.IsNullOrEmpty(dashboardId)) {
        Dashboard dashboard = dashboards.Where(d => d.Id == dashboardId).First();
        var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
        var token = client.Dashboards.GenerateTokenInGroupAsync(appWorkspaceId, dashboard.Id, generateTokenRequestParameters).Result;

        var embedConfig = new EmbedConfiguration() {
          EmbedToken = token,
          EmbedUrl = dashboard.EmbedUrl,
          Id = dashboard.Id
        };


        viewModel.CurrentDashboard = new DashboardViewModel {
          Dashboard = dashboard,
          EmbedConfig = embedConfig
        };

      }

      return viewModel;
    }


  }
}