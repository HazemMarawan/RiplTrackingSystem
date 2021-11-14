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
    public class RoleController : Controller
    {
        DBContext db = new DBContext();
        // GET: Default
        public ActionResult Index()
        {
            if (!can.hasPermission("access_role"))
                return RedirectToAction("Error404", "Error");

            //tessst
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
                var permissionData = db.roles.Select(s => new RoleViewModel
                {
                    id = s.id,
                    name = s.name,
                    description = s.description,
                    permissions = (from permission in db.permissions
                                   join rolePermission in db.rolePermissions on permission.id equals rolePermission.permission_id
                                   select new PermissionViewModel
                                   {
                                       id = permission.id,
                                       name = permission.name,
                                       role_id = rolePermission.role_id
                                   }).Where(rp => rp.role_id == s.id).ToList()
                });

                //Search    
                //if (!string.IsNullOrEmpty(searchValue))
                //{
                //    permissionData = permissionData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                //     m.description.ToLower().Contains(searchValue.ToLower()) || m.nice_name.ToLower().Contains(searchValue.ToLower()));
                //}

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
            return View();
        }

        public ActionResult Show()
        {
            User currentUser = Session["user"] as User;

            if (!can.hasPermission("access_role"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Access Roles", String.Empty, String.Empty, "Tring to Access Roles but has no permission");

                return RedirectToAction("Error404", "Error");
            }

            ViewBag.Roles = db.roles.Select(r => new { r.id, r.name }).ToList();
            List<PermissionGroupViewModel> permissionData = db.permisisonGroups.Select(s => new PermissionGroupViewModel
            {
                id = s.id,
                name = s.name,
                description = s.description,
                active = s.active,
                permissions = db.permissions.Where(p => p.permission_group_id == s.id).Select(p => new PermissionViewModel
                {
                    id = p.id,
                    name = p.name,
                    nice_name = p.nice_name,
                    description = p.description,
                    active = p.active
                }).Where(p=>p.active == 1).ToList()
            }).Where(s => s.active == 1).ToList();

            return View(permissionData);
        }
     
        public JsonResult saveRole(RoleViewModel RoleVM)
        {
            User currentUser = Session["user"] as User;

            if (!can.hasPermission("edit_role"))
            {
                if(RoleVM.id == 0)
                    Helpers.Helpers.SystemLogger(currentUser.id, "Add Role", String.Empty, JsonConvert.SerializeObject(RoleVM), "Trining to Add Role but has no permission");
                else
                    Helpers.Helpers.SystemLogger(currentUser.id, "Edit Role", JsonConvert.SerializeObject(db.roles.Find(RoleVM.id)), JsonConvert.SerializeObject(RoleVM), "Trining to Edit Role but has no permission");
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }

            if (RoleVM.id == 0)
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Add Role", String.Empty, JsonConvert.SerializeObject(RoleVM), "Successfull Add Role");

                Role role = AutoMapper.Mapper.Map<RoleViewModel, Role>(RoleVM);

                role.updated_at = DateTime.Now;
                role.created_at = DateTime.Now;
                role.active = 1;
                db.roles.Add(role);
                db.SaveChanges();
                return Json(new { message = "done", role = role,
                    icon = "success",
                    title = "Done",
                    text = "Role Added Successfully."
                }, JsonRequestBehavior.AllowGet);
                }
            else
            {
                List<RolePermission> deletedRolePermission = db.rolePermissions.Where(rp => rp.role_id == RoleVM.id).ToList();
                deletedRolePermission.ForEach(rp => db.rolePermissions.Remove(rp));
                db.SaveChanges();
                if (RoleVM.permissionIDs != null)
                {
                    List<string> permissions = RoleVM.permissionIDs.Split(',').ToList();

                    foreach (var permission in permissions)
                    {
                        int permissionID = int.Parse(permission);
                        RolePermission oldRolePermission = db.rolePermissions.Where(rp => rp.role_id == RoleVM.id && rp.permission_id == permissionID).FirstOrDefault();

                        RolePermission rolePermission = new RolePermission();
                        rolePermission.role_id = RoleVM.id;
                        rolePermission.permission_id = int.Parse(permission);
                        rolePermission.updated_at = DateTime.Now;
                        rolePermission.created_at = DateTime.Now;
                        db.rolePermissions.Add(rolePermission);
                        db.SaveChanges();

                    }
                }

            }

            return Json(new { message = "done",
                icon = "success",
                title = "Done",
                text = "Role Updated Successfully."
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult permissionList(int id)
        {
            List<int?> permissionIDs = db.rolePermissions.Where(rp => rp.role_id == id).Select(rp => rp.permission_id).ToList(); 
            return Json(new { message = "done", permissionIDs = permissionIDs }, JsonRequestBehavior.AllowGet);
        }    
        [HttpGet]
        public JsonResult deleteRole(int id)
        {
            User currentUser = Session["user"] as User;;
            if (!can.hasPermission("delete_role"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Delete Role", String.Empty, JsonConvert.SerializeObject(db.roles.Find(id)), "Trining to Delete Role but has no permission");

                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }

            db.userRoles.Where(s => s.role_id == id).ToList().ForEach(s => db.userRoles.Remove(s));
            db.SaveChanges();

            db.rolePermissions.Where(rp => rp.role_id == id).ToList().ForEach(rp => db.rolePermissions.Remove(rp));
            db.SaveChanges();

            Role deleteRole = db.roles.Find(id);
            deleteRole.active = 0;
            db.Entry(deleteRole).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();

            Helpers.Helpers.SystemLogger(currentUser.id, "Delete Role", String.Empty, JsonConvert.SerializeObject(deleteRole), "Successfull Delete Role");

            return Json(new { message = "done", role = id,
                icon = "success",
                title = "Done",
                text = "Role Deleted Successfully."
            }, JsonRequestBehavior.AllowGet);
        }
    }
}