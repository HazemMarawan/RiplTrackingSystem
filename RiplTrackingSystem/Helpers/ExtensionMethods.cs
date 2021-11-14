using RiplTrackingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Helpers
{
    public static class ExtensionMethods
    {
        public static bool checkIfContains(this String str,List<PermissionViewModel> permissions)
        {
            foreach(PermissionViewModel permission in permissions)
            {
                if (permission.name.Contains(str))
                    return true;
            }
            return false;
        }
    }
}