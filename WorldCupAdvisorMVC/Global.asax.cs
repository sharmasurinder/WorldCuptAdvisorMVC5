using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WorldCupAdvisorMVC.Models;

namespace WorldCupAdvisorMVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

            if (roleManager.RoleExists(Common.Roles.Administrator) == false)
            {
                roleManager.Create(new IdentityRole() { Id = 1.ToString(), Name = (Common.Roles.Administrator) });
            }
            if (roleManager.RoleExists(Common.Roles.User) == false)
            {
                roleManager.Create(new IdentityRole() { Id = 2.ToString(), Name = (Common.Roles.User) });
            }

            //MembershipConfig.RegisterMembership();
        }
    }
}
