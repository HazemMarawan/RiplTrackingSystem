using Newtonsoft.Json;
using RiplTrackingSystem.Models;
using RiplTrackingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RiplTrackingSystem.Helpers;

namespace RiplTrackingSystem.Controllers
{
    public class AccountController : Controller
    {
        DBContext db = new DBContext();

        // GET: Account
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(User user)
        {

            User currentUser = db.users.Where(s => s.user_name == user.user_name && s.password == user.password).FirstOrDefault();
            string serializedRequest = JsonConvert.SerializeObject(user);
            if (currentUser != null)
            {
                if (currentUser.active != 1)
                {
                    Helpers.Helpers.SystemLogger(currentUser.id, "Login", String.Empty, serializedRequest, "Tried To Login But Account Not Active");
                    ViewBag.errorMsg = "This User isnot Active.";
                }
                else
                {
                    Helpers.Helpers.SystemLogger(currentUser.id, "Login", String.Empty, serializedRequest, "Successfull Login");

                    Session["user"] = currentUser;
                    Session["user_name"] = currentUser.user_name;
                    if (String.IsNullOrEmpty(currentUser.image))
                        Session["image"] = "~/Content/assets/img/90x90.jpg";
                    else
                        Session["image"] = currentUser.image;

                    List<PermissionViewModel> userPermission = (from u in db.users
                                                                join userRole in db.userRoles on u.id equals userRole.user_id
                                                                join role in db.roles on userRole.role_id equals role.id
                                                                join rolePermission in db.rolePermissions on role.id equals rolePermission.role_id
                                                                join permission in db.permissions on rolePermission.permission_id equals permission.id
                                                                //group permission.name by permission.id into p
                                                                select new PermissionViewModel
                                                                {
                                                                    id = u.id,
                                                                    name = permission.name

                                                                }).Where(u => u.id == currentUser.id).ToList();

                    Session["user_permission"] = userPermission;
                    return RedirectToAction("Index", "Home");
                }

            }
            else
            {
                Helpers.Helpers.SystemLogger(null, "Login", String.Empty, serializedRequest, "Invalid Username Or Password");
                ViewBag.errorMsg = "Invalid Username Or Password";
            }

            return View();
        }
        public ActionResult Logout()
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Logout", String.Empty, String.Empty, "Successfull Logout");

            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}