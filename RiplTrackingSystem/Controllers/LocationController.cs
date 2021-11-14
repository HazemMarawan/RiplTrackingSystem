using RiplTrackingSystem.Auth;
using RiplTrackingSystem.Models;
using RiplTrackingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RiplTrackingSystem.Enums;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.Data;
using OfficeOpenXml.Style;
using RiplTrackingSystem.Helpers;
using Newtonsoft.Json;

namespace RiplTrackingSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class LocationController : Controller
    {
        DBContext db = new DBContext();
        // GET: Location
        public ActionResult Index()
        {

            User currentUser = Session["user"] as User;

            if (!can.hasPermission("access_location"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Access Locations", String.Empty, String.Empty, "Tring to Access Locations but has no permission");
                return RedirectToAction("Error404", "Error");
            }
          
            ViewBag.isRipl = currentUser.location_id == null ? true : false;
            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var from_date = Request.Form.GetValues("columns[0][search][value]")[0];
                var to_date = Request.Form.GetValues("columns[1][search][value]")[0];
                var location_id = Request.Form.GetValues("columns[2][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var locationsData = (from location in db.locations
                                     select new CompanyDetail
                                     {
                                         id = location.id,
                                         name = location.name,
                                         description = location.description,
                                         address = location.address,
                                         phone = location.phone,
                                         parent_id = location.parent_id,
                                         type = location.type,
                                         canSendPluck = location.can_send_pluck == null ? 0 : location.can_send_pluck,
                                         created_at = location.created_at,
                                         displayedType = (location.type == 1) ? "Company" : (location.type == 2) ? "Factory" : (location.type == 3) ? "Store" : "Distributor",
                                         displayedParent = (location.parent_id != null) ? db.locations.Where(l => l.id == location.parent_id).FirstOrDefault().name : "",
                                         actual_number_of_assets = db.companyAssetsRent.Where(s=>s.to_location == location.id && s.status == (int)AssetStatus.Received && s.due_date >= DateTime.Now).ToList().Count(),
                                         number_of_assets = db.rentOrders.Where(s=>s.location_id == location.id && s.due_date >= DateTime.Now).ToList().Count() != 0 ? db.rentOrders.Where(s => s.location_id == location.id && s.due_date >= DateTime.Now).Select(s=>s.assetes_count).Sum():0
                                     }).Where(l => l.type == (int)LocationType.Company);

                //Search    
                if (!string.IsNullOrWhiteSpace(location_id))
                {
                    var loc = int.Parse(location_id);
                    locationsData = locationsData.Where(s => s.id == loc);
                }
                if (!string.IsNullOrEmpty(from_date))
                {
                    if (Convert.ToDateTime(from_date) != DateTime.MinValue)
                    {
                        DateTime from = Convert.ToDateTime(from_date);
                        locationsData = locationsData.Where(s => s.created_at >= from);
                    }
                }
                if (!string.IsNullOrEmpty(from_date))
                {
                    if (Convert.ToDateTime(to_date) != DateTime.MinValue)
                    {
                        DateTime to = Convert.ToDateTime(to_date);
                        locationsData = locationsData.Where(s => s.created_at <= to);
                    }
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    locationsData = locationsData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                     m.description.ToLower().Contains(searchValue.ToLower()) || m.description.ToLower().Contains(searchValue.ToLower()) || m.address.ToLower().Contains(searchValue.ToLower()) || m.phone.ToLower().Contains(searchValue.ToLower()));
                }

                if (currentUser.location_id != null)
                {
                    RiplTrackingSystem.Models.Location userLocation = db.locations.Find(currentUser.location_id);
                    if (userLocation.type == (int)LocationType.Company)
                        locationsData = locationsData.Where(m => m.id == userLocation.id);
                    else
                        locationsData = locationsData.Where(m => m.id == -1);
                }
                //total number of rows count     
                var displayResult = locationsData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = locationsData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.roles = db.roles.Select(s => new { s.id, s.name }).ToList();
            ViewBag.locations = db.locations.Where(l => l.parent_id == null).Select(s => new { s.id, s.name }).ToList();
            return View();
        }
        [HttpPost]
        public JsonResult saveLocation(locationViewModel LocationVM)
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("edit_location"))
            {
                if(LocationVM.id == 0)
                    Helpers.Helpers.SystemLogger(currentUser.id, "Add Location", String.Empty, JsonConvert.SerializeObject(LocationVM), "Tring to Add Location but has no permission");
                else
                    Helpers.Helpers.SystemLogger(currentUser.id, "Edit Location", JsonConvert.SerializeObject(db.locations.Find(LocationVM.id)), JsonConvert.SerializeObject(LocationVM), "Tring to Edit Location but has no permission");

                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }

            if (LocationVM.id == 0)
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Add Location", String.Empty, JsonConvert.SerializeObject(LocationVM), "Successfull Add Location");

                RiplTrackingSystem.Models.Location location = AutoMapper.Mapper.Map<locationViewModel, RiplTrackingSystem.Models.Location>(LocationVM);

                location.updated_at = DateTime.Now;
                location.created_at = DateTime.Now;
                location.active = 1;
                db.locations.Add(location);
                db.SaveChanges();

                if (location.type == (int)LocationType.Company)
                    location.company_id = location.id;
                else
                    location.company_id = db.locations.Find(location.parent_id).company_id;

                db.Entry(location).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return Json(new { message = "done", location = location }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                RiplTrackingSystem.Models.Location oldLocation = db.locations.Find(LocationVM.id);
                Helpers.Helpers.SystemLogger(currentUser.id, "Edit Location", JsonConvert.SerializeObject(oldLocation), JsonConvert.SerializeObject(LocationVM), "Successfull Edit Location");

                oldLocation.name = LocationVM.name;
                oldLocation.description = LocationVM.description;
                oldLocation.address = LocationVM.address;
                oldLocation.phone = LocationVM.phone;
                oldLocation.parent_id = LocationVM.parent_id;
                oldLocation.type = LocationVM.type;
                oldLocation.can_send_pluck = LocationVM.can_send_pluck;
                oldLocation.updated_at = DateTime.Now;

                db.Entry(oldLocation).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { message = "done", location = oldLocation }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult deleteLocation(int id)
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("delete_location"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Delete Location", String.Empty, JsonConvert.SerializeObject(db.locations.Find(id)), "Tring to Delete Location but has no permission");

                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }

            //db.locations.Where(l => l.parent_id == id).ToList().ForEach(l => db.locations.Remove(l));
            //db.SaveChanges();

            RiplTrackingSystem.Models.Location deleteLocation = db.locations.Find(id);
            deleteLocation.active = 0;
            db.Entry(deleteLocation).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();



            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowTree(int id)
        {
            locationViewModel rootLocation = db.locations.Where(l => l.id == id).Select(l => new locationViewModel
            {
                id = l.id,
                name = l.name,
                description = l.description,
                address = l.address,
                parent_id = l.parent_id,
                type = l.type,
            }).FirstOrDefault();
            List<locationViewModel> allPaths = new List<locationViewModel>();

            List<locationViewModel> locationTree =
                                                        (from l in db.locations
                                                         join pl in db.locations on l.parent_id equals pl.id
                                                         select new locationViewModel
                                                         {
                                                             id = l.id,
                                                             name = l.name,
                                                             description = l.description,
                                                             address = l.address,
                                                             parent_id = l.parent_id,
                                                             type = l.type,
                                                             displayedParent = pl.name

                                                         }).ToList();
            allPaths.Add(rootLocation);
            allPaths.AddRange(locationTree);
            return View(allPaths);
        }

        public ActionResult Show(int id)
        {
            CompanyDetail company = db.locations.Where(l => l.id == id).Select(s => new CompanyDetail
            {
                id = s.id,
                name = s.name,
                phone = s.phone,
                type = s.type,
                address = s.address,
                description = s.description,
            }).FirstOrDefault();

            RiplTrackingSystem.Models.Location currentLocation = db.locations.Find(id);
            List<SiteMapPath> paths = new List<SiteMapPath>();
            SiteMapPath currentPath = new SiteMapPath();
            currentPath.id = currentLocation.id;
            currentPath.name = currentLocation.name;
            paths.Add(currentPath);

            if(currentLocation.parent_id != null)
            {
                while(true)
                {
                    currentLocation = db.locations.Find(currentLocation.parent_id);
                    currentPath = new SiteMapPath();
                    currentPath.id = currentLocation.id;
                    currentPath.name = currentLocation.name;
                    paths.Add(currentPath);

                    if (currentLocation.parent_id == null)
                        break;
                }
            }

            paths.Reverse();

            ViewBag.roles = db.roles.Select(s => new { s.id, s.name }).ToList();
            ViewBag.locations = db.locations.Select(s => new { s.id, s.name }).ToList();
            ViewBag.companyID = id;
            ViewBag.Paths = paths;
            return View(company);

        }

        public ActionResult factories(int location_id)
        {
            bool isCompany = db.locations.Find(location_id).type == (int)LocationType.Company ? true : false;
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
            var from_date = Request.Form.GetValues("columns[0][search][value]")[0];
            var to_date = Request.Form.GetValues("columns[1][search][value]")[0];
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Getting all data    
            var factories = db.locations.Where(f => f.type == (int)LocationType.Factory)
            .Select(f => new locationViewModel
            {
                id = f.id,
                name = f.name,
                phone = f.phone,
                company_id = f.company_id,
                address = f.address,
                parent_id = f.parent_id,
                displayedParent = (f.parent_id != null) ? db.locations.Where(l => l.id == f.parent_id).FirstOrDefault().name : "",
                type = f.type,
                description = f.description,
                number_of_assets = db.companyAssetsRent.Where(s => s.to_location == f.id && s.due_date >= DateTime.Now).ToList().Count(),
                actual_number_of_assets = db.companyAssetsRent.Where(s => s.to_location == f.id && s.due_date >= DateTime.Now && s.status == (int)AssetStatus.Received).ToList().Count()
            });

            if (isCompany)
                factories = factories.Where(f => f.company_id == location_id);
            else
                factories = factories.Where(f => f.parent_id == location_id);
            //total number of rows count     
            var displayResult = factories.OrderByDescending(u => u.id).Skip(skip)
                 .Take(pageSize).ToList();
            var totalRecords = factories.Count();

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = displayResult

            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult factoryUsers(int location_id)
        {
            bool isCompany = db.locations.Find(location_id).type == (int)LocationType.Company ? true : false;
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
            var from_date = Request.Form.GetValues("columns[0][search][value]")[0];
            var to_date = Request.Form.GetValues("columns[1][search][value]")[0];
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Getting all data    
            var factoryUsers = (from location in db.locations
                                join user in db.users on location.id equals user.location_id
                                select new UserViewModel
                                {
                                    location_type = location.type,
                                    company_id = location.company_id,
                                    id = user.id,
                                    parent_id = location.parent_id,
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
                                    stringGender = user.gender == 1 ? "Male" : "Female",
                                    active = user.active,
                                    stringActive = user.active == 1 ? "Active" : "In Active",
                                    roles = (from role in db.roles
                                             join userRole in db.userRoles on role.id equals userRole.role_id
                                             select new RoleViewModel
                                             {
                                                 id = role.id,
                                                 user_id = userRole.user_id,
                                                 name = role.name
                                             }).Where(r => r.user_id == user.id).ToList()
                                }).Where(fu => fu.location_type == (int)LocationType.Factory);
            if (isCompany)
                factoryUsers = factoryUsers.Where(f => f.company_id == location_id);
            else
                factoryUsers = factoryUsers.Where(f => f.parent_id == location_id);

            //total number of rows count     
            var displayResult = factoryUsers.OrderByDescending(u => u.id).Skip(skip)
                 .Take(pageSize).ToList();
            var totalRecords = factoryUsers.Count();

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = displayResult

            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult stores(int location_id)
        {
            bool isCompany = db.locations.Find(location_id).type == (int)LocationType.Company ? true : false;

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
            var from_date = Request.Form.GetValues("columns[0][search][value]")[0];
            var to_date = Request.Form.GetValues("columns[1][search][value]")[0];
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Getting all data    
            var stores = db.locations.Where(f => f.type == (int)LocationType.Store)
            .Select(f => new locationViewModel
            {
                id = f.id,
                name = f.name,
                phone = f.phone,
                company_id = f.company_id,
                address = f.address,
                parent_id = f.parent_id,
                type = f.type,
                displayedParent = (f.parent_id != null) ? db.locations.Where(l => l.id == f.parent_id).FirstOrDefault().name : "",
                description = f.description,
                number_of_assets = db.companyAssetsRent.Where(s => s.to_location == f.id && s.due_date >= DateTime.Now).ToList().Count(),
                actual_number_of_assets = db.companyAssetsRent.Where(s => s.to_location == f.id && s.due_date >= DateTime.Now && s.status == (int)AssetStatus.Received).ToList().Count()
            });

            if (isCompany)
                stores = stores.Where(f => f.company_id == location_id);
            else
                stores = stores.Where(f => f.parent_id == location_id);

            //total number of rows count     
            var displayResult = stores.OrderByDescending(u => u.id).Skip(skip)
                 .Take(pageSize).ToList();
            var totalRecords = stores.Count();

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = displayResult

            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult storeUsers(int location_id)
        {
            bool isCompany = db.locations.Find(location_id).type == (int)LocationType.Company ? true : false;
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
            var from_date = Request.Form.GetValues("columns[0][search][value]")[0];
            var to_date = Request.Form.GetValues("columns[1][search][value]")[0];
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Getting all data    
            var storeUsers = (from location in db.locations
                              join user in db.users on location.id equals user.location_id
                              select new UserViewModel
                              {
                                  location_type = location.type,
                                  company_id = location.company_id,
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
                                  parent_id = location.parent_id,
                                  stringGender = user.gender == 1 ? "Male" : "Female",
                                  active = user.active,
                                  stringActive = user.active == 1 ? "Active" : "In Active",
                                  roles = (from role in db.roles
                                           join userRole in db.userRoles on role.id equals userRole.role_id
                                           select new RoleViewModel
                                           {
                                               id = role.id,
                                               user_id = userRole.user_id,
                                               name = role.name
                                           }).Where(r => r.user_id == user.id).ToList()
                              }).Where(fu => fu.location_type == (int)LocationType.Store);

            if (isCompany)
                storeUsers = storeUsers.Where(f => f.company_id == location_id);
            else
                storeUsers = storeUsers.Where(f => f.parent_id == location_id);

            //total number of rows count     
            var displayResult = storeUsers.OrderByDescending(u => u.id).Skip(skip)
                 .Take(pageSize).ToList();
            var totalRecords = storeUsers.Count();

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = displayResult

            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult distributors(int location_id)
        {
            bool isCompany = db.locations.Find(location_id).type == (int)LocationType.Company ? true : false;
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
            var from_date = Request.Form.GetValues("columns[0][search][value]")[0];
            var to_date = Request.Form.GetValues("columns[1][search][value]")[0];
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Getting all data    
            var distributors = db.locations.Where(f => f.type == (int)LocationType.Distributor)
            .Select(f => new locationViewModel
            {
                id = f.id,
                name = f.name,
                phone = f.phone,
                company_id = f.company_id,
                address = f.address,
                parent_id = f.parent_id,
                type = f.type,
                displayedParent = (f.parent_id != null) ? db.locations.Where(l => l.id == f.parent_id).FirstOrDefault().name : "",
                description = f.description,
                number_of_assets = db.companyAssetsRent.Where(s=>s.to_location == f.id && s.due_date >= DateTime.Now).ToList().Count(),
                actual_number_of_assets = db.companyAssetsRent.Where(s => s.to_location == f.id && s.due_date >= DateTime.Now && s.status == (int) AssetStatus.Received).ToList().Count()
            });

            if (isCompany)
                distributors = distributors.Where(f => f.company_id == location_id);
            else
                distributors = distributors.Where(f => f.parent_id == location_id);

            //total number of rows count     
            var displayResult = distributors.OrderByDescending(u => u.id).Skip(skip)
                 .Take(pageSize).ToList();
            var totalRecords = distributors.Count();

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = displayResult

            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult distributorUsers(int location_id)
        {
            bool isCompany = db.locations.Find(location_id).type == (int)LocationType.Company ? true : false;

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
            var from_date = Request.Form.GetValues("columns[0][search][value]")[0];
            var to_date = Request.Form.GetValues("columns[1][search][value]")[0];
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Getting all data    
            var distributorUsers = (from location in db.locations
                                    join user in db.users on location.id equals user.location_id
                                    select new UserViewModel
                                    {
                                        location_type = location.type,
                                        company_id = location.company_id,
                                        parent_id = location.parent_id,
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
                                        stringGender = user.gender == 1 ? "Male" : "Female",
                                        active = user.active,
                                        stringActive = user.active == 1 ? "Active" : "In Active",
                                        roles = (from role in db.roles
                                                 join userRole in db.userRoles on role.id equals userRole.role_id
                                                 select new RoleViewModel
                                                 {
                                                     id = role.id,
                                                     user_id = userRole.user_id,
                                                     name = role.name
                                                 }).Where(r => r.user_id == user.id).ToList()
                                    }).Where(fu => fu.company_id == location_id && fu.location_type == (int)LocationType.Distributor);
            if (isCompany)
                distributorUsers = distributorUsers.Where(f => f.company_id == location_id);
            else
                distributorUsers = distributorUsers.Where(f => f.parent_id == location_id);

            //total number of rows count     
            var displayResult = distributorUsers.OrderByDescending(u => u.id).Skip(skip)
                 .Take(pageSize).ToList();
            var totalRecords = distributorUsers.Count();

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = displayResult

            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult companyUsers(int location_id)
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
            var companyUsers = (from location in db.locations
                                join user in db.users on location.id equals user.location_id
                                select new UserViewModel
                                {
                                    location_type = location.type,
                                    company_id = location.company_id,
                                    parent_id = location.parent_id,
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
                                                 name = role.name
                                             }).Where(r => r.user_id == user.id).ToList(),
                                    stringLocation = location.name
                                }).Where(u => u.company_id == location_id).ToList();

            //total number of rows count     
            var displayResult = companyUsers.OrderByDescending(u => u.id).Skip(skip)
                 .Take(pageSize).ToList();
            var totalRecords = companyUsers.Count();

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = displayResult

            }, JsonRequestBehavior.AllowGet);
        }

        public void LocationsReport()
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Locations", String.Empty, String.Empty, "Successfull Export Locations");

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Orders");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells["A1:H1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:H1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "ID";
            Sheet.Cells["B1"].Value = "Name";
            Sheet.Cells["C1"].Value = "Phone";
            Sheet.Cells["D1"].Value = "Assets Count";
            Sheet.Cells["E1"].Value = "Factories";
            Sheet.Cells["F1"].Value = "Stores";
            Sheet.Cells["G1"].Value = "Distributors";
            Sheet.Cells["H1"].Value = "Created at";

            var locationsData = (from location in db.locations
                                 select new CompanyDetail
                                 {
                                     id = location.id,
                                     name = location.name,
                                     description = location.description,
                                     address = location.address,
                                     phone = location.phone,
                                     parent_id = location.parent_id,
                                     stringCreatedAt = location.created_at.ToString(),
                                     type = location.type,
                                     stores = db.locations.Where(s=>s.company_id == location.id &&  s.type == (int)LocationType.Store).Select(s=>new locationViewModel { 
                                        name = s.name

                                     }).ToList(),
                                     factories = db.locations.Where(s => s.company_id == location.id && s.type == (int)LocationType.Factory).Select(s => new locationViewModel
                                     {
                                         name = s.name

                                     }).ToList(),
                                     distributors = db.locations.Where(s => s.company_id == location.id && s.type == (int)LocationType.Distributor).Select(s => new locationViewModel
                                     {
                                         name = s.name

                                     }).ToList(),
                                     displayedType = (location.type == 1) ? "Company" : (location.type == 2) ? "Factory" : (location.type == 3) ? "Store" : "Distributor",
                                     displayedParent = (location.parent_id != null) ? db.locations.Where(l => l.id == location.parent_id).FirstOrDefault().name : "",
                                     actual_number_of_assets = db.companyAssetsRent.Where(s => s.to_location == location.id && s.status == (int)AssetStatus.Received && s.due_date >= DateTime.Now).ToList().Count(),
                                     number_of_assets = db.rentOrders.Where(s => s.location_id == location.id && s.due_date >= DateTime.Now).ToList().Count() != 0 ? db.rentOrders.Where(s => s.location_id == location.id && s.due_date >= DateTime.Now).Select(s => s.assetes_count).Sum() : 0
                                 }).Where(l => l.type == (int)LocationType.Company);

            int row = 2;
            foreach (var item in locationsData)
            {
                string locaitonFactories = String.Empty;
                string locaitonStores = String.Empty;
                string locaitonDistributors = String.Empty;

                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.name;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.phone;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.number_of_assets;

                if(item.factories.Count() != 0)
                {
                    foreach(var factory in item.factories)
                    {
                        locaitonFactories += factory.name + ",";
                    }
                    locaitonFactories = locaitonFactories.Remove(locaitonFactories.Length - 1);
                }
                Sheet.Cells[string.Format("E{0}", row)].Value = locaitonFactories;

                if (item.stores.Count() != 0)
                {
                    foreach (var store in item.stores)
                    {
                        locaitonStores += store.name + ",";
                    }
                    locaitonStores = locaitonStores.Remove(locaitonStores.Length - 1);
                }
                Sheet.Cells[string.Format("F{0}", row)].Value = locaitonStores;

                if (item.distributors.Count() != 0)
                {
                    foreach (var distributor in item.distributors)
                    {
                        locaitonDistributors += distributor.name + ",";
                    }
                    locaitonDistributors = locaitonDistributors.Remove(locaitonDistributors.Length - 1);
                }
                Sheet.Cells[string.Format("G{0}", row)].Value = locaitonDistributors;
                Sheet.Cells[string.Format("H{0}", row)].Value = item.stringCreatedAt;

                row++;
            }
            
            row++;
            colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
            text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Font.Color.SetColor(text);

            Sheet.Cells[string.Format("A{0}", row)].Value = "Total";
            Sheet.Cells[string.Format("B{0}", row)].Value = locationsData.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        public void LocationsWithHierarchyReport()
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Locations With Hierarchy", String.Empty, String.Empty, "Successfull Export Locations With Hierarchy");

            ExcelPackage Ep = new ExcelPackage();

            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Orders");
            Sheet.Cells["A1"].Value = "Type";
            Sheet.Cells["B1"].Value = "Name";
            Sheet.Cells["C1"].Value = "Phone";
            Sheet.Cells["D1"].Value = "Assets Count";
            Sheet.Cells["E1"].Value = "Created at";

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells["A1:E1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:E1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:E1"].Style.Font.Color.SetColor(text);

            var locationsData = (from location in db.locations

                                 select new CompanyDetail
                                 {
                                     id = location.id,
                                     name = location.name,
                                     description = location.description,
                                     address = location.address,
                                     phone = location.phone,
                                     parent_id = location.parent_id,
                                     stringCreatedAt = location.created_at.ToString(),
                                     factories = db.locations.Where(s => s.company_id == location.id && s.type == (int)LocationType.Factory).Select(s => new locationViewModel
                                     {
                                         id = s.id,
                                         name = s.name

                                     }).ToList(),
                                     type = location.type,
                                     displayedType = (location.type == 1) ? "Company" : (location.type == 2) ? "Factory" : (location.type == 3) ? "Store" : "Distributor",
                                     displayedParent = (location.parent_id != null) ? db.locations.Where(l => l.id == location.parent_id).FirstOrDefault().name : "",
                                     actual_number_of_assets = db.companyAssetsRent.Where(s => s.to_location == location.id && s.status == (int)AssetStatus.Received && s.due_date >= DateTime.Now).ToList().Count(),
                                     number_of_assets = db.rentOrders.Where(s => s.location_id == location.id && s.due_date >= DateTime.Now).ToList().Count() != 0 ? db.rentOrders.Where(s => s.location_id == location.id && s.due_date >= DateTime.Now).Select(s => s.assetes_count).Sum() : 0
                                 }).Where(l => l.type == (int)LocationType.Company);

            int row = 2;
            foreach (var item in locationsData)
            {
                colFromHex = System.Drawing.ColorTranslator.FromHtml("#d4d3e3");
                Sheet.Cells[string.Format("A{0}:E{1}", row,row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("A{0}:E{1}", row,row)].Style.Fill.BackgroundColor.SetColor(colFromHex);

                Sheet.Cells[string.Format("A{0}", row)].Value = "Company";
                Sheet.Cells[string.Format("B{0}", row)].Value = item.name;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.phone;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.number_of_assets;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.stringCreatedAt;

                row++;

                if (item.factories.Count() != 0)
                {
                    foreach (var factory in item.factories)
                    {
                        colFromHex = System.Drawing.ColorTranslator.FromHtml("#a7a7b5");
                        Sheet.Cells[string.Format("A{0}:E{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        Sheet.Cells[string.Format("A{0}:E{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);

                        Sheet.Cells[string.Format("A{0}", row)].Value = "Factory";

                        Sheet.Cells[string.Format("B{0}", row)].Value = factory.name;
                        Sheet.Cells[string.Format("C{0}", row)].Value = item.phone;
                        Sheet.Cells[string.Format("D{0}", row)].Value = item.number_of_assets;
                        Sheet.Cells[string.Format("E{0}", row)].Value = item.stringCreatedAt;
                        row++;
                   

                        List<locationViewModel> factoryStores = db.locations.Where(s => s.parent_id == factory.id && s.type == (int)LocationType.Store).Select(s => new locationViewModel
                        {
                            id = s.id,
                            name = s.name

                        }).ToList();

                        if(factoryStores.Count() != 0)
                        {
                            
                            foreach (locationViewModel factoryStore in factoryStores)
                            {
                                colFromHex = System.Drawing.ColorTranslator.FromHtml("#868691");
                                Sheet.Cells[string.Format("A{0}:E{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                Sheet.Cells[string.Format("A{0}:E{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);


                                Sheet.Cells[string.Format("A{0}", row)].Value = "Store";
                                Sheet.Cells[string.Format("B{0}", row)].Value = factoryStore.name;
                                Sheet.Cells[string.Format("C{0}", row)].Value = item.phone;
                                Sheet.Cells[string.Format("D{0}", row)].Value = item.number_of_assets;
                                Sheet.Cells[string.Format("E{0}", row)].Value = item.stringCreatedAt;
                                row++;
                                List<locationViewModel> storeDistributors = db.locations.Where(s => s.parent_id == factoryStore.id && s.type == (int)LocationType.Distributor).Select(s => new locationViewModel
                                {
                                    id = s.id,
                                    name = s.name

                                }).ToList();

                                if (storeDistributors.Count() != 0)
                                {
                                    colFromHex = System.Drawing.ColorTranslator.FromHtml("#676773");
                                    Sheet.Cells[string.Format("A{0}:E{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    Sheet.Cells[string.Format("A{0}:E{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);

                                    Sheet.Cells[string.Format("A{0}", row)].Value = "Distributor";

                                    foreach(locationViewModel storeDistributor in storeDistributors)
                                    {
                                        Sheet.Cells[string.Format("B{0}", row)].Value = storeDistributor.name;
                                        Sheet.Cells[string.Format("C{0}", row)].Value = item.phone;
                                        Sheet.Cells[string.Format("D{0}", row)].Value = item.number_of_assets;
                                        Sheet.Cells[string.Format("E{0}", row)].Value = item.stringCreatedAt;
                                        row++;
                                    }
                                }
                                row++;



                            }
                            row++;
                        }
                    }
                }
                
                colFromHex = System.Drawing.ColorTranslator.FromHtml("#484854");
                Sheet.Cells[string.Format("A{0}:E{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("A{0}:E{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);

                row++;
            }
            
            colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells[string.Format("A{0}:B{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[string.Format("A{0}:B{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
            text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells[string.Format("A{0}:B{1}", row, row)].Style.Font.Color.SetColor(text);

            Sheet.Cells[string.Format("A{0}", row)].Value = "Total";
            Sheet.Cells[string.Format("B{0}", row)].Value = locationsData.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
    }
}