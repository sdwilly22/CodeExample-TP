using AutoMapper;
using BEAR.Authorization;
using BEAR.BusinessLogic.Awards;
using BEAR.BusinessLogic.Awards.DTO;
using BEAR.DAL._Context;
using BEAR.Legacy.Interfaces;
using BEAR.Legacy.Models.User;
using BEAR.Web.Filters;
using BEAR.Web.Models.Awards;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BEAR.Web.Controllers
{
    [Authorize]
    public class AwardController : Controller
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Route("~/Workload/Awards")]
        public ActionResult BrowseAwards()
        {
            log.Debug("BrowseAwards: Entering");

            log.Debug("BrowseAwards: Exiting");
            return View();
        }

        [Route("~/Award/{awardNumber}")]
        public ActionResult AwardDetails(string awardNumber)
        {
            log.Debug("AwardDetails: Entering");

            log.Debug("AwardDetails: Exiting");
            return View();
        }

        [Route("~/Workload/TransferRequests")]
        [AllowAccess(Roles = "GroupLead,CanApproveAllAwardTransfers")]
        public ActionResult TransferRequests()
        {
            log.Debug("TransferRequests: Entering");

            log.Debug("TransferRequests: Exiting");
            return View();
        }

        [Route("~/Award/{awardNumber}/Closeout")]
        public ActionResult Closeout(string awardNumber)
        {
            log.Debug("Closeout: Entering");

            log.Debug("Closeout: Exiting");
            return View();
        }
    }
}