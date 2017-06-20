using System;
using System.Collections.Generic;
using Microsoft.PowerBI.Api.V2.Models;

namespace HelloPbiEmbedded_3rdParty.Models {

  public class HomeViewModel {
    public string WorkspaceName;
    public string WorkspaceId;
  }

  public class ReportViewModel {
    public Report Report { get; set; }
    public EmbedConfiguration EmbedConfig { get; set; }
  }

  public class ReportsViewModel {
    public List<Report> Reports { get; set; }
    public ReportViewModel CurrentReport { get; set; }
  }

  public class DashboardViewModel {
    public Dashboard Dashboard { get; set; }
    public EmbedConfiguration EmbedConfig { get; set; }
  }

  public class DashboardsViewModel {
    public List<Dashboard> Dashboards { get; set; }
    public DashboardViewModel CurrentDashboard { get; set; }
  }

}