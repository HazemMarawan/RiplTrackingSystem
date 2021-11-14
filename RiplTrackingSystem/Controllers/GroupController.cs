using Newtonsoft.Json;
using RiplTrackingSystem.Auth;
using RiplTrackingSystem.Helpers;
using RiplTrackingSystem.Models;
using RiplTrackingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RiplTrackingSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class GroupController : Controller
    {
        // GET: Group
        DBContext db = new DBContext();
        // GET: Default
        public ActionResult Index()
        {
            if (!can.hasPermission("access_group"))
                return RedirectToAction("Error404", "Error");

            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var from_date = Request.Form.GetValues("columns[0][search][value]")[0];
                var to_date = Request.Form.GetValues("columns[1][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var permissionGroupsData = db.permisisonGroups.Select(s => new PermissionGroupViewModel
                {
                    id = s.id,
                    name = s.name,
                    description = s.description,
                    active = s.active,
                    permissions = db.permissions.Where(p => p.permission_group_id == s.id).Select(p => new PermissionViewModel
                    {
                        id=p.id,
                        name=p.name,
                        nice_name=p.nice_name,
                        description=p.description,
                        active = p.active
                    }).Where(p=>p.active == 1).ToList()
                }).Where(s=>s.active == 1);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    permissionGroupsData = permissionGroupsData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                     m.description.ToLower().Contains(searchValue.ToLower()));
                }

                //total number of rows count     
                var displayResult = permissionGroupsData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = permissionGroupsData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }

            return View();
        }
        [HttpPost]
        public JsonResult savePermissionGroup(PermissionGroupViewModel PermissionGroupVM)
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("edit_group"))
            {
                if(PermissionGroupVM.id == 0)
                    Helpers.Helpers.SystemLogger(currentUser.id, "Add Permission Group", String.Empty, JsonConvert.SerializeObject(PermissionGroupVM), "Tring to Add Permission Group but has no permission");
                else
                    Helpers.Helpers.SystemLogger(currentUser.id, "Edit Permission Group", JsonConvert.SerializeObject(db.permisisonGroups.Find(PermissionGroupVM.id)), JsonConvert.SerializeObject(PermissionGroupVM), "Tring to Edit Permission Group but has no permission");

                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }

            if (PermissionGroupVM.id == 0)
            {
                PermissionGroup permissionGroup = AutoMapper.Mapper.Map<PermissionGroupViewModel, PermissionGroup>(PermissionGroupVM);

                permissionGroup.updated_at = DateTime.Now;
                permissionGroup.created_at = DateTime.Now;
                permissionGroup.active = 1;
                db.permisisonGroups.Add(permissionGroup);
                Helpers.Helpers.SystemLogger(currentUser.id, "Add Permission Group", String.Empty, JsonConvert.SerializeObject(PermissionGroupVM), "Successfull Add Permisison Group");

            }
            else
            {

                PermissionGroup oldPermissionGroup = db.permisisonGroups.Find(PermissionGroupVM.id);
                Helpers.Helpers.SystemLogger(currentUser.id, "Add Permission Group",JsonConvert.SerializeObject(oldPermissionGroup), JsonConvert.SerializeObject(PermissionGroupVM), "Successfull Add Permisison Group");

                oldPermissionGroup.name = PermissionGroupVM.name;
                oldPermissionGroup.description = PermissionGroupVM.description;
                oldPermissionGroup.updated_at = DateTime.Now;

                db.Entry(oldPermissionGroup).State = System.Data.Entity.EntityState.Modified;
            }
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deletePermissionGroup(int id)
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("delete_group"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Delete Permission Group", String.Empty, JsonConvert.SerializeObject(db.permisisonGroups.Find(id)), "Tring to Delete Permisison Group but has no permission");

                return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
            }

            //List<Permission> groupPermissions = db.permissions.Where(s => s.permission_group_id == id).ToList();
            //foreach(Permission permission in groupPermissions)
            //{
            //    db.rolePermissions.Where(s => s.permission_id == permission.id).ToList().ForEach(p => db.rolePermissions.Remove(p));
            //    db.SaveChanges();
            //}

            //db.permissions.Where(s => s.permission_group_id == id).ToList().ForEach(s=>db.permissions.Remove(s));
            //db.SaveChanges();

            PermissionGroup deletePermissionGroup = db.permisisonGroups.Find(id);
            //db.permisisonGroups.Remove(deletePermissionGroup);
            deletePermissionGroup.active = 0;
            db.Entry(deletePermissionGroup).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}