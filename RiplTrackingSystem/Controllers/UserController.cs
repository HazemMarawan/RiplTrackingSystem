using Newtonsoft.Json;
using RiplTrackingSystem.Auth;
using RiplTrackingSystem.Helpers;
using RiplTrackingSystem.Models;
using RiplTrackingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RiplTrackingSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class UserController : Controller
    {
        DBContext db = new DBContext();
        // GET: User
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("access_user"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Access User", String.Empty, String.Empty , "Tring to Access User but has no Permission");

                return RedirectToAction("Error404", "Error");
            }

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
                var userData = (from user in db.users
                                //join userRole in db.userRoles on user.id equals userRole.user_id
                                //join role in db.roles on userRole.role_id equals role.id
                                select new UserViewModel
                                {
                                    id = user.id,
                                    user_name = user.user_name,
                                    full_name = user.full_name,
                                    password = user.password,
                                    type = user.type,
                                    phone1 = user.phone1,
                                    phone2 = user.phone2,
                                    location_id = user.location_id,
                                    imagePath = user.image,
                                    address1 = user.address1,
                                    address2 = user.address2,
                                    birthDate = user.birthDate,
                                    code = user.code,
                                    email = user.email,
                                    gender = user.gender,
                                    active = user.active,
                                    roles = (from role in db.roles
                                             join userRole in db.userRoles on role.id equals userRole.role_id
                                             select new RoleViewModel
                                             {
                                                 id = role.id,
                                                user_id = userRole.user_id,
                                                name = role.name,
                                                active = role.active
                                             }).Where(r => r.user_id == user.id && r.active == 1).ToList()
                                }).Where(s=>s.active == 1);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    userData = userData.Where(m => m.full_name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                     m.email.ToLower().Contains(searchValue.ToLower()) || m.user_name.ToLower().Contains(searchValue.ToLower()));
                }

                //total number of rows count     
                var displayResult = userData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = userData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }

            ViewBag.roles = db.roles.Select(s => new { s.id, s.name }).ToList();
            ViewBag.locations = db.locations.Select(s => new { s.id, s.name }).ToList();
            return View();
        }
        [HttpPost]
        public JsonResult saveUser(UserViewModel userVM)
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("edit_user"))
            {
                if(userVM.id == 0)
                    Helpers.Helpers.SystemLogger(currentUser.id, "Save User", String.Empty, JsonConvert.SerializeObject(userVM), "Tring to Save User but has not permission");
                else
                    Helpers.Helpers.SystemLogger(currentUser.id, "Edit User", JsonConvert.SerializeObject(db.users.Find(userVM.id)), JsonConvert.SerializeObject(userVM), "Tring to Edit User but has not permission");

                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }

            if (userVM.id == 0)
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Save User", String.Empty, JsonConvert.SerializeObject(userVM), "Successfull Save User");

                User user = AutoMapper.Mapper.Map<UserViewModel, User>(userVM);

                user.created_at = DateTime.Now;
                user.updated_at = DateTime.Now;
                //user.created_by = Session["id"].ToString().ToInt();
                //user.type = (int)UserTypes.Staff;

                if(userVM.image != null)
                { 
                    Guid guid = Guid.NewGuid();
                    var InputFileName = Path.GetFileName(userVM.image.FileName);
                    var ServerSavePath = Path.Combine(Server.MapPath("~/images/profile/") + guid.ToString() + "_Profile" + Path.GetExtension(userVM.image.FileName));
                    userVM.image.SaveAs(ServerSavePath);
                    user.image = "/images/profile/" + guid.ToString() + "_Profile" + Path.GetExtension(userVM.image.FileName);
                }
                db.users.Add(user);
                db.SaveChanges();

                foreach(int roleID in userVM.role_ids)
                {
                    UserRole userRole = new UserRole();
                    userRole.user_id = user.id;
                    userRole.role_id = roleID;
                    userRole.created_at = DateTime.Now;
                    userRole.updated_at = DateTime.Now;
                    db.userRoles.Add(userRole);
                    db.SaveChanges();

                }

            }
            else
            {

                User oldUser = db.users.Find(userVM.id);
                Helpers.Helpers.SystemLogger(currentUser.id, "Edit User", JsonConvert.SerializeObject(oldUser), JsonConvert.SerializeObject(userVM), "Successfull Edit User");

                oldUser.full_name = userVM.full_name;
                oldUser.user_name = userVM.user_name;
                oldUser.password = userVM.password;
                oldUser.code = userVM.code;
                oldUser.email = userVM.email;
                oldUser.phone1 = userVM.phone1;
                oldUser.phone2 = userVM.phone2;
                oldUser.address1 = userVM.address1;
                oldUser.address2 = userVM.address2;
                oldUser.birthDate = userVM.birthDate;
                oldUser.gender = userVM.gender;
                oldUser.active = userVM.active;
                if (userVM.image != null)
                {
                    Guid guid = Guid.NewGuid();
                    var InputFileName = Path.GetFileName(userVM.image.FileName);
                    var ServerSavePath = Path.Combine(Server.MapPath("~/images/profile/") + guid.ToString() + "_Profile" + Path.GetExtension(userVM.image.FileName));
                    userVM.image.SaveAs(ServerSavePath);
                    oldUser.image = "/images/profile/" + guid.ToString() + "_Profile" + Path.GetExtension(userVM.image.FileName);
                }
                oldUser.updated_at = DateTime.Now;
                db.Entry(oldUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                db.userRoles.Where(ur => ur.user_id == oldUser.id).ToList().ForEach(ur => db.userRoles.Remove(ur));
                db.SaveChanges();

                foreach (int roleID in userVM.role_ids)
                {
                    UserRole userRole = new UserRole();
                    userRole.user_id = oldUser.id;
                    userRole.role_id = roleID;
                    userRole.created_at = DateTime.Now;
                    userRole.updated_at = DateTime.Now;
                    db.userRoles.Add(userRole);
                    db.SaveChanges();

                }
            }
            
            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
           
        }

        [HttpGet]
        public JsonResult deleteUser(int id)
        {
            User currentUser = Session["user"] as User;

            if (!can.hasPermission("delete_user"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Delete User", String.Empty, JsonConvert.SerializeObject(db.users.Find(id)), "Tring to Delete User but has not permission");

                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
            //List<UserRole> userRoles  = db.userRoles.Where(rp => rp.user_id == id).ToList();
            //userRoles.ForEach(ur => db.userRoles.Remove(ur));
            //db.SaveChanges();

            User deleteUser = db.users.Find(id);
            deleteUser.active = 0;
            db.Entry(deleteUser).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();
            Helpers.Helpers.SystemLogger(currentUser.id, "Delete User", String.Empty, JsonConvert.SerializeObject(db.users.Find(id)), "Successfull Delete User");

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult checkUsernameAvailability(string user_name, int id =0)
        {
            if(id != 0)
            {
                var oldUsername = db.users.Find(id).user_name;
                if(oldUsername == user_name)
                    return Json(new { message = "valid username", is_valid = true }, JsonRequestBehavior.AllowGet);

            }
            var checkAvailabilty = db.users.Any(s => s.user_name == user_name);
            if (checkAvailabilty)
            {
                return Json(new { message = "username already taken", is_valid = false }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { message = "valid username", is_valid = true }, JsonRequestBehavior.AllowGet);
        }
    }
}