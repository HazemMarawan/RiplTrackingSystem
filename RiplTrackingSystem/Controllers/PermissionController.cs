using Newtonsoft.Json;
using RiplTrackingSystem.Auth;
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
    public class PermissionController : Controller
    {
        DBContext db = new DBContext();
        // GET: Default
        public ActionResult Index()
        {
            if (!can.hasPermission("access_permission"))
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
                var permissionData = (from permission in db.permissions
                                      join permissiongroup in db.permisisonGroups on permission.permission_group_id equals permissiongroup.id

                                      select new PermissionViewModel
                                      {
                                          id = permission.id,
                                          name = permission.name,
                                          nice_name = permission.nice_name,
                                          description = permission.description,
                                          permission_group_id = permissiongroup.id,
                                          permission_group = permissiongroup.name
                                      });

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    permissionData = permissionData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                     m.description.ToLower().Contains(searchValue.ToLower()) || m.nice_name.ToLower().Contains(searchValue.ToLower()));
                }

                //total number of rows count     
                var displayResult = permissionData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = permissionData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.permissionGroups = db.permisisonGroups.Select(s => new { s.id, s.name }).ToList();
            return View();
        }
        [HttpPost]
        public JsonResult savePermission(PermissionViewModel PermissionVM)
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("edit_permission"))
            {
                if (PermissionVM.id == 0)
                    Helpers.Helpers.SystemLogger(currentUser.id, "Add Permission", String.Empty, JsonConvert.SerializeObject(PermissionVM), "Tring to Add Permission but has not permission");
                else
                    Helpers.Helpers.SystemLogger(currentUser.id, "Edit Permission", JsonConvert.SerializeObject(db.permissions.Find(PermissionVM.id)), JsonConvert.SerializeObject(PermissionVM), "Tring to Edit Permission but has not permission");
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }

            if (PermissionVM.id == 0)
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Add Permission", String.Empty, JsonConvert.SerializeObject(PermissionVM), "Successfull Add Permission");

                Permission permission = AutoMapper.Mapper.Map<PermissionViewModel, Permission>(PermissionVM);

                permission.updated_at = DateTime.Now;
                permission.created_at = DateTime.Now;
                permission.active = 1;
                db.permissions.Add(permission);
            }
            else
            {
                Permission oldPermission = db.permissions.Find(PermissionVM.id);
                Helpers.Helpers.SystemLogger(currentUser.id, "Edit Permission", JsonConvert.SerializeObject(oldPermission), JsonConvert.SerializeObject(PermissionVM), "Tring to Edit Permission but has not permission");

                oldPermission.name = PermissionVM.name;
                oldPermission.description = PermissionVM.description;
                oldPermission.nice_name = PermissionVM.nice_name;
                oldPermission.updated_at = DateTime.Now;

                db.Entry(oldPermission).State = System.Data.Entity.EntityState.Modified;
            }
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deletePermission(int id)
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("delete_permission"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Delete Permission", String.Empty, JsonConvert.SerializeObject(db.permissions.Find(id)), "Tring to Delete Permission but has not permission");
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }

            Helpers.Helpers.SystemLogger(currentUser.id, "Delete Permission", String.Empty, JsonConvert.SerializeObject(db.permissions.Find(id)), "Successfull Delete Permission");

            Permission deletedPermission = db.permissions.Find(id);
            deletedPermission.active = 0;
            db.Entry(deletedPermission).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}