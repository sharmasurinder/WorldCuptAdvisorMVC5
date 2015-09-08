using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldCupAdvisorMVC.Common
{
    public class Roles
    {
        public const string Administrator = "Administrator";
        public const string User = "User";

        public static IEnumerable<string> GetRoles()
        {
            return new string[] { WorldCupAdvisorMVC.Common.Roles.Administrator,WorldCupAdvisorMVC.Common.Roles.User  };
        }
    }
}
