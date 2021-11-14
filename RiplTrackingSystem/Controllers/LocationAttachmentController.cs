using Newtonsoft.Json;
using RiplTrackingSystem.Auth;
using RiplTrackingSystem.Enums;
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
    public class LocationAttachmentController : Controller
    {
        DBContext db = new DBContext();
        // GET: Default
        public ActionResult Index()
        {
            //if (!can.hasPermission("access_permission"))
            //    return RedirectToAction("Error404", "Error");

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
                var locaitonAttachments = (from locationAttachment in db.locationAttachments
                                           join location in db.locations on locationAttachment.location_id equals location.id
                                           join user in db.users on locationAttachment.created_by equals user.id
                                           select new LocationAttachmentViewModel
                                           {
                                               id = locationAttachment.id,
                                               company_name = location.name,
                                               path = locationAttachment.path,
                                               description = locationAttachment.description,
                                               string_created_by = user.full_name,
                                               string_created_at = locationAttachment.created_at.ToString(),
                                               location_id = location.id,
                                               active = locationAttachment.active
                                           }).Where(l=>l.active == 1);
                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    locaitonAttachments = locaitonAttachments.Where(m => m.company_name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                     m.description.ToLower().Contains(searchValue.ToLower()));
                }

                //total number of rows count     
                var displayResult = locaitonAttachments.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = locaitonAttachments.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.companies = db.locations.Where(s => s.parent_id == null && s.type == (int)LocationType.Company).Select(s => new { s.id, s.name }).ToList();
            return View();
        }
        [HttpPost]
        public JsonResult saveLocationAttachment(LocationAttachmentViewModel locationAttachmentVM)
        {
            User currentUser = Session["user"] as User;
            //if (!can.hasPermission("edit_permission"))
            //{
            //    if (PermissionVM.id == 0)
            //        Helpers.Helpers.SystemLogger(currentUser.id, "Add Permission", String.Empty, JsonConvert.SerializeObject(PermissionVM), "Tring to Add Permission but has not permission");
            //    else
            //        Helpers.Helpers.SystemLogger(currentUser.id, "Edit Permission", JsonConvert.SerializeObject(db.permissions.Find(PermissionVM.id)), JsonConvert.SerializeObject(PermissionVM), "Tring to Edit Permission but has not permission");
            //    return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            //}

            if (locationAttachmentVM.id == 0)
            {
                //Helpers.Helpers.SystemLogger(currentUser.id, "Add Permission", String.Empty, JsonConvert.SerializeObject(PermissionVM), "Successfull Add Permission");

                LocationAttachment locationAttachment = AutoMapper.Mapper.Map<LocationAttachmentViewModel, LocationAttachment>(locationAttachmentVM);

                locationAttachment.updated_at = DateTime.Now;
                locationAttachment.created_at = DateTime.Now;
                locationAttachment.created_by = currentUser.id;
                locationAttachment.active = 1;
                if (locationAttachmentVM.attachment_file != null)
                {
                    Guid guid = Guid.NewGuid();
                    var InputFileName = Path.GetFileName(locationAttachmentVM.attachment_file.FileName);
                    var ServerSavePath = Path.Combine(Server.MapPath("~/UploadedFiles/LocationAttachments/") + guid.ToString() + "LocationAttachments" + Path.GetExtension(locationAttachmentVM.attachment_file.FileName));
                    locationAttachmentVM.attachment_file.SaveAs(ServerSavePath);
                    locationAttachment.path = "/UploadedFiles/LocationAttachments/" + guid.ToString() + "LocationAttachments" + Path.GetExtension(locationAttachmentVM.attachment_file.FileName);

                }

                db.locationAttachments.Add(locationAttachment);
            }
            else
            {
                LocationAttachment oldlocationAttachment = db.locationAttachments.Find(locationAttachmentVM.id);
                // Helpers.Helpers.SystemLogger(currentUser.id, "Edit Permission", JsonConvert.SerializeObject(oldPermission), JsonConvert.SerializeObject(PermissionVM), "Tring to Edit Permission but has not permission");

                oldlocationAttachment.location_id = locationAttachmentVM.location_id;
                oldlocationAttachment.description = locationAttachmentVM.description;
                oldlocationAttachment.updated_at = DateTime.Now;

                if (locationAttachmentVM.attachment_file != null)
                {
                    Guid guid = Guid.NewGuid();
                    var InputFileName = Path.GetFileName(locationAttachmentVM.attachment_file.FileName);
                    var ServerSavePath = Path.Combine(Server.MapPath("~/UploadedFiles/LocationAttachments/") + guid.ToString() + "LocationAttachments" + Path.GetExtension(locationAttachmentVM.attachment_file.FileName));
                    locationAttachmentVM.attachment_file.SaveAs(ServerSavePath);
                    oldlocationAttachment.path = "/UploadedFiles/LocationAttachments/" + guid.ToString() + "LocationAttachments" + Path.GetExtension(locationAttachmentVM.attachment_file.FileName);

                }


                db.Entry(oldlocationAttachment).State = System.Data.Entity.EntityState.Modified;
            }
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteLocationAttachment(int id)
        {
            //User currentUser = Session["user"] as User;
            //if (!can.hasPermission("delete_permission"))
            //{
            //    Helpers.Helpers.SystemLogger(currentUser.id, "Delete Permission", String.Empty, JsonConvert.SerializeObject(db.permissions.Find(id)), "Tring to Delete Permission but has not permission");
            //    return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            //}

            //Helpers.Helpers.SystemLogger(currentUser.id, "Delete Permission", String.Empty, JsonConvert.SerializeObject(db.permissions.Find(id)), "Successfull Delete Permission");

            LocationAttachment locationAttachment = db.locationAttachments.Find(id);
            locationAttachment.active = 0;
            db.Entry(locationAttachment).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}