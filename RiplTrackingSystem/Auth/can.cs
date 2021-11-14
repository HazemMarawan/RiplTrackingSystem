using RiplTrackingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RiplTrackingSystem.Models;
namespace RiplTrackingSystem.Auth
{
    public class can
    {
        public static DBContext db = new DBContext();
        public static bool hasPermission(string permission)
        {
            bool hasPermission = false;
            List<PermissionViewModel> permissionVM = HttpContext.Current.Session["user_permission"] as List<PermissionViewModel>;
            foreach (var permissionObj in permissionVM)
            {
                if (permission == permissionObj.name)
                {
                    hasPermission = true;
                    break;
                }
            }
            return hasPermission;
        }

        public static void fillSession()
        {
            User currentUser = can.db.users.Find(1);
            HttpContext.Current.Session["user"] = currentUser;
            HttpContext.Current.Session["user_name"] = currentUser.user_name;
            if (String.IsNullOrEmpty(currentUser.image))
                HttpContext.Current.Session["image"] = "~/Content/assets/img/90x90.jpg";
            else
                HttpContext.Current.Session["image"] = currentUser.image;

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

            HttpContext.Current.Session["user_permission"] = userPermission;
        }
    }
}