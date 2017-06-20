using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using HelloPbiEmbedded_3rdParty.Models;

namespace HelloPbiEmbedded_3rdParty.Controllers {
  public class ReportsController : Controller {

    public async Task<ActionResult> Index(string ReportId) {
      var viewModel = await PbiEmbeddedManager.GetReports(ReportId);
      return View(viewModel);
    }

  }
}