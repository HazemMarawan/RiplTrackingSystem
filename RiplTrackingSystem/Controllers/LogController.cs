using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RiplTrackingSystem.Auth;
using RiplTrackingSystem.Models;
using RiplTrackingSystem.ViewModel;
using RiplTrackingSystem.Enums;
using log4net;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Newtonsoft.Json;
namespace RiplTrackingSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class LogController : Controller
    {
        DBContext db = new DBContext();
        // GET: Log
        public ActionResult Index()
        {
            if (!can.hasPermission("access_log"))
                return RedirectToAction("Error404", "Error");

            User currrentUser = Session["user"] as User;
          
            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var location_id = Request.Form.GetValues("columns[0][search][value]")[0];
                var from_date = Request.Form.GetValues("columns[1][search][value]")[0];
                var to_date = Request.Form.GetValues("columns[2][search][value]")[0];
                var user_id = Request.Form.GetValues("columns[3][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var requestsData = (from log in db.logs
                                    join u in db.users on log.user_id equals u.id into ps
                                    from u in ps.DefaultIfEmpty()
                                    join l in db.locations on u.location_id equals l.id into loc
                                    from l in loc.DefaultIfEmpty()
                                    select new LogViewModel
                                    {
                                        id = log.Id,
                                        user_id = log.user_id,
                                        full_name = u.full_name,
                                        action = log.action,
                                        old_data = log.old_data,
                                        request_data = log.request_data,
                                        notes = log.notes,
                                        string_created_at = log.created_at.ToString(),
                                        string_location = l.name,
                                        location_id = u.location_id,
                                        created_at = log.created_at
                                    }) ;


                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    requestsData = requestsData.Where(m => m.full_name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                        m.notes.ToLower().Contains(searchValue.ToLower()) || m.action.ToLower().Contains(searchValue.ToLower()));
                }

                if (!string.IsNullOrEmpty(user_id))
                {
                    int uId = int.Parse(user_id);
                    requestsData = requestsData.Where(s => s.user_id == uId);
                }

                if (!string.IsNullOrEmpty(location_id))
                {
                    int locID = int.Parse(location_id);
                    requestsData = requestsData.Where(s => s.location_id == locID);
                }

                if (!string.IsNullOrEmpty(from_date))
                {
                    if (Convert.ToDateTime(from_date) != DateTime.MinValue)
                    {
                        DateTime from = Convert.ToDateTime(from_date);
                        requestsData = requestsData.Where(s => s.created_at >= from);
                    }
                }

                if (!string.IsNullOrEmpty(to_date))
                {
                    if (Convert.ToDateTime(to_date) != DateTime.MinValue)
                    {
                        DateTime to = Convert.ToDateTime(to_date);
                        requestsData = requestsData.Where(s => s.created_at <= to);
                    }
                }

                //total number of rows count     
                var displayResult = requestsData.OrderByDescending(u => u.id).Skip(skip)
                        .Take(pageSize).ToList();
                var totalRecords = requestsData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            
            ViewBag.locations = db.locations.Where(l => l.parent_id == null).Select(s => new { s.id, s.name }).ToList();
            ViewBag.users = db.users.Select(s => new { s.id, s.full_name }).ToList();

            return View();
        }
    }
}