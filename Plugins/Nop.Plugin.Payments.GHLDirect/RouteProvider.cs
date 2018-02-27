using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.GHLDirect
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //PDT
            routes.MapRoute("Plugin.Payments.GHLDirect.PDTHandler",
                 "Plugins/PaymentGHLDirect/PDTHandler",
                 new { controller = "PaymentGHLDirect", action = "PDTHandler" },
                 new[] { "Nop.Plugin.Payments.GHLDirect.Controllers" }
            );

            //Cancel
            routes.MapRoute("Plugin.Payments.GHLDirect.CancelOrder",
                 "Plugins/PaymentGHLDirect/CancelOrder",
                 new { controller = "PaymentGHLDirect", action = "CancelOrder" },
                 new[] { "Nop.Plugin.Payments.GHLDirect.Controllers" }
            );

            //Redirect to payment processing
            routes.MapRoute("Plugin.Payments.GHLDirect.RedirectToGHLDirect",
                 "Plugins/PaymentGHLDirect/RedirectToGHLDirect/{orderGuid}",
                 new { controller = "PaymentGHLDirect", action = "RedirectToGHLDirect" },
                 new { orderGuid = new GuidConstraint(false) },
                 new[] { "Nop.Plugin.Payments.GHLDirect.Controllers" });
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
