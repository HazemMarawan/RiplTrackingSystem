using RiplTrackingSystem.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RiplTrackingSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class RolePermissionController : Controller
    {
        // GET: RolePermission
        public ActionResult Index()
        {
            return View();
        }
    }
}