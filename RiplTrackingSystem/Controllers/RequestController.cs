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
    public class RequestController : Controller
    {
        DBContext db = new DBContext();
        ILog logger = log4net.LogManager.GetLogger(typeof(RequestController));
        // GET: Request
        public ActionResult Index()
        {
            if (!can.hasPermission("access_request"))
                return RedirectToAction("Error404", "Error");

            User currrentUser = Session["user"] as User;
            try
            {
                if (Request.IsAjaxRequest())
                {
                    var draw = Request.Form.GetValues("draw").FirstOrDefault();
                    var start = Request.Form.GetValues("start").FirstOrDefault();
                    var length = Request.Form.GetValues("length").FirstOrDefault();
                    var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                    var request_type = Request.Form.GetValues("columns[0][search][value]")[0];
                    var location_id = Request.Form.GetValues("columns[1][search][value]")[0];
                    var from_date = Request.Form.GetValues("columns[2][search][value]")[0];
                    var to_date = Request.Form.GetValues("columns[3][search][value]")[0];
                    int pageSize = length != null ? Convert.ToInt32(length) : 0;
                    int skip = start != null ? Convert.ToInt32(start) : 0;

                    // Getting all data    
                    var requestsData = (from request in db.requests
                                        join location in db.locations on request.location_id equals location.id
                                        join user in db.users on request.created_by equals user.id
                                        select new RequestViewModel
                                        {
                                            id = request.id,
                                            stringLocation = location.name,
                                            created_at =request.created_at,
                                            created_by = request.created_by,
                                            number_of_assets = request.number_of_assets,
                                            location_id = request.location_id,
                                            type = request.type,
                                            notes = request.notes,
                                            stringUser = user.full_name,
                                            stringType = request.type == 1 ? "New" : "Return",
                                            stringCreated_at = request.created_at.ToString()

                                        });

                    if (currrentUser.location_id != null)
                    {
                        Location userLocation = db.locations.Find(currrentUser.location_id);
                        if (userLocation.type == (int)LocationType.Company)
                            requestsData = requestsData.Where(m => m.location_id == userLocation.id);
                        else
                            requestsData = requestsData.Where(m => m.location_id == -1);
                    }

                    //Search    
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        requestsData = requestsData.Where(m => m.stringLocation.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                         m.notes.ToLower().Contains(searchValue.ToLower()) || m.stringUser.ToLower().Contains(searchValue.ToLower()));
                    }

                    if (!string.IsNullOrEmpty(request_type))
                    {
                        int reqType = int.Parse(request_type);
                        requestsData = requestsData.Where(s => s.type == reqType);
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
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "| ID: " + currrentUser.id + " Name: " + currrentUser.full_name);
            }
            ViewBag.locations = db.locations.Where(l => l.parent_id == null).Select(s => new { s.id, s.name }).ToList();

            return View();
        }

        public JsonResult save(RequestViewModel requestViewModel)
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Add Request", String.Empty, JsonConvert.SerializeObject(requestViewModel), "Successfull Add Request");

            User user = Session["user"] as User;
            Request request = AutoMapper.Mapper.Map<RequestViewModel, Request>(requestViewModel);
            request.created_at = DateTime.Now;
            request.updated_at = DateTime.Now;
            request.created_by = user.id;
            request.location_id = user.location_id;
            db.requests.Add(request);
            db.SaveChanges();

            return Json(new { msg = "done" }, JsonRequestBehavior.AllowGet);
        }

        public void ExportRequestsSheet()
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Request Sheet", String.Empty, String.Empty, "Successfull Export Request Sheet");

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Assets");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells["A1:G1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:G1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:G1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "Id";
            Sheet.Cells["B1"].Value = "Location";
            Sheet.Cells["C1"].Value = "Number Of Assets";
            Sheet.Cells["D1"].Value = "Notes";
            Sheet.Cells["E1"].Value = "Created By";
            Sheet.Cells["F1"].Value = "Type";
            Sheet.Cells["G1"].Value = "Created At";

            List<RequestViewModel> requestsData = (from request in db.requests
                                join location in db.locations on request.location_id equals location.id
                                join user in db.users on request.created_by equals user.id
                                select new RequestViewModel
                                {
                                    id = request.id,
                                    stringLocation = location.name,

                                    number_of_assets = request.number_of_assets,
                                    location_id = request.location_id,
                                    notes = request.notes,
                                    stringUser = user.full_name,
                                    stringType = request.type == 1 ? "New" : "Return",
                                    stringCreated_at = request.created_at.ToString()


                                }).ToList();

            int row = 2;
            foreach (var item in requestsData)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.stringLocation;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.number_of_assets;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.notes;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.stringUser;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.stringType;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.created_at.ToString();
                row++;
            }

            row++;
            colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells[string.Format("A{0},G{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[string.Format("A{0},G{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
            text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells[string.Format("A{0},G{1}", row, row)].Style.Font.Color.SetColor(text);

            Sheet.Cells[string.Format("A{0}", row)].Value = "Total";
            Sheet.Cells[string.Format("B{0}", row)].Value = requestsData.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
    }
}