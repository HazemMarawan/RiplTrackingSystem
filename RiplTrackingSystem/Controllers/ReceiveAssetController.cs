using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RiplTrackingSystem.Auth;
using RiplTrackingSystem.Enums;
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
    public class ReceiveAssetController : Controller
    {
        DBContext db = new DBContext(); 
        // GET: RecieveAsset
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("access_manage_assets"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Access Track Assets", String.Empty, String.Empty , "Tring to Access Track Assets but has no permission");

                return RedirectToAction("Error404", "Error");
            }

            ViewBag.can_receive = can.hasPermission("receive_assets") ? 1 : 0;
            ViewBag.can_send = can.hasPermission("send_assets") ? 1 : 0;

            User user = Session["user"] as User;

            ViewBag.currentLocation = user.location_id == null ? 0 : user.location_id;

            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var tag_id = Request.Form.GetValues("columns[0][search][value]")[0];
                var asset_type = Request.Form.GetValues("columns[1][search][value]")[0];
                var asset_color = Request.Form.GetValues("columns[2][search][value]")[0];
                var from_location_search = Request.Form.GetValues("columns[3][search][value]")[0];
                var to_location = Request.Form.GetValues("columns[4][search][value]")[0];
                var asset_status = Request.Form.GetValues("columns[5][search][value]")[0];
                var from_date = Request.Form.GetValues("columns[6][search][value]")[0];
                var to_date = Request.Form.GetValues("columns[7][search][value]")[0];

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;            

                // Getting all data    
                var companyRentAssetData = (from asset in db.assets
                                            join comAssetRent in db.companyAssetsRent on asset.id equals comAssetRent.asset_id
                                            join location in db.locations on comAssetRent.from_location equals location.id into from_location
                                            from fl in from_location.DefaultIfEmpty()
                                            join tLocation in db.locations on comAssetRent.to_location equals tLocation.id into tLoc
                                            from tolocation in tLoc.DefaultIfEmpty()
                                            join rentOrder in db.rentOrders on  comAssetRent.rent_order_id  equals rentOrder.id
                                            select new AssetViewModel
                                            {
                                                id = comAssetRent.id,
                                                tag_id = asset.tag_id,
                                                type = asset.type,
                                                tall = asset.tall,
                                                color = asset.color,
                                                status = comAssetRent.status,
                                                start_date = comAssetRent.start_date,
                                                created_at = comAssetRent.created_at,
                                                from_location = comAssetRent.from_location,
                                                from_location_string = String.IsNullOrEmpty(fl.name)?"Ripl": fl.name,
                                                due_date = comAssetRent.due_date,
                                                to_location = tolocation.id == null ? 0 : tolocation.id,
                                                to_location_name = String.IsNullOrEmpty(tolocation.name) ? "Ripl" : tolocation.name,
                                                parent_id = fl.parent_id,
                                                company_id = fl.company_id,
                                                to_location_company_id = tolocation.company_id

                                            });
                if (user.location_id != null) { 
                    bool is_company = db.locations.Find(user.location_id).type == 1 ? true : false;

                    if (is_company)
                    {
                        companyRentAssetData = companyRentAssetData.Where(loc => loc.company_id == user.location_id || loc.to_location == user.location_id);
                    }
                    else
                    {
                        companyRentAssetData = companyRentAssetData.Where(loc => loc.to_location == user.location_id);
                    }
                }

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    companyRentAssetData = companyRentAssetData.Where(m => m.tag_id.ToString().ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                     m.type.ToLower().Contains(searchValue.ToLower()));
                }

                if (!string.IsNullOrEmpty(tag_id))
                {
                    
                    companyRentAssetData = companyRentAssetData.Where(s => s.tag_id == tag_id);
                }

                if (!string.IsNullOrEmpty(asset_type))
                {
                    companyRentAssetData = companyRentAssetData.Where(s => s.type.ToLower() == asset_type.ToLower());
                }

                if (!string.IsNullOrEmpty(asset_color))
                {
                    companyRentAssetData = companyRentAssetData.Where(s => s.color.ToLower() == asset_color.ToLower());
                }

                if (!string.IsNullOrEmpty(from_location_search))
                {
                    int fromLoc = int.Parse(from_location_search);
                    companyRentAssetData = companyRentAssetData.Where(s => s.from_location == fromLoc);
                }

                if (!string.IsNullOrEmpty(to_location))
                {
                    int toLoc = int.Parse(to_location);
                    companyRentAssetData = companyRentAssetData.Where(s => s.to_location == toLoc);
                }

                if (!string.IsNullOrEmpty(asset_status))
                {
                    int status = int.Parse(asset_status);
                    companyRentAssetData = companyRentAssetData.Where(s => s.status == status);
                }

                if (!string.IsNullOrEmpty(from_date))
                {
                    if (Convert.ToDateTime(from_date) != DateTime.MinValue)
                    {
                        DateTime from = Convert.ToDateTime(from_date);
                        companyRentAssetData = companyRentAssetData.Where(s => s.created_at >= from);
                    }
                }

                if (!string.IsNullOrEmpty(to_date))
                {
                    if (Convert.ToDateTime(to_date) != DateTime.MinValue)
                    {
                        DateTime to = Convert.ToDateTime(to_date);
                        companyRentAssetData = companyRentAssetData.Where(s => s.created_at <= to);
                    }
                }

                //total number of rows count     
                var displayResult = companyRentAssetData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = companyRentAssetData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            if (user.location_id != null)
            {
                Location userLocation = db.locations.Find(user.location_id);
                ViewBag.locations = db.locations.Where(s => s.company_id == userLocation.company_id && s.id != user.location_id).Select(s => new { s.id, s.name }).ToList();
            }else
                ViewBag.locations = db.locations.Where(s => s.type == (int)LocationType.Company).Select(s => new { s.id, s.name }).ToList();
            ViewBag.from_locations = db.locations.Select(s => new { s.id, s.name }).ToList();
            ViewBag.to_locations = db.locations.Select(s => new { s.id, s.name }).ToList();

            return View();
        }

        [HttpGet]
        public JsonResult receiveAsset(int? id)
        {
            User currentUser = Session["user"] as User;

            if (!can.hasPermission("receive_assets"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Recieve Asset", String.Empty, JsonConvert.SerializeObject(db.companyAssetsRent.Find(id)), "Tring to Recieve Asset but has no permission");

                return Json(new
                {
                    msg = "error",
                    icon = "error",
                    title = "Error!",
                    text = "Process Is Not Completed!"
                }, JsonRequestBehavior.AllowGet);
            }

            CompanyAssetRent companyAssetRent = db.companyAssetsRent.Find(id);
            companyAssetRent.status = (int)AssetStatus.Received;

            db.Entry(companyAssetRent).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            Helpers.Helpers.SystemLogger(currentUser.id, "Recieve Asset", String.Empty, JsonConvert.SerializeObject(companyAssetRent), "Successfull Recieve Asset");

            return Json(new
            {
                msg = "done",
                icon = "success",
                title = "Received",
                text = "Asset Received Successfully."
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult rejectAsset(int? id)
        {
            User currentUser = Session["user"] as User;

            if (!can.hasPermission("receive_assets"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Reject Asset", String.Empty, JsonConvert.SerializeObject(db.companyAssetsRent.Find(id)), "Tring to Reject Asset but has no permission");

                return Json(new
                {
                    msg = "error",
                    icon = "error",
                    title = "Error!",
                    text = "Process Is Not Completed!"
                }, JsonRequestBehavior.AllowGet);
            }

            CompanyAssetRent companyAssetRent = db.companyAssetsRent.Find(id);
            companyAssetRent.status = (int)AssetStatus.Lost;

            db.Entry(companyAssetRent).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            Helpers.Helpers.SystemLogger(currentUser.id, "Reject Asset", String.Empty, JsonConvert.SerializeObject(companyAssetRent), "Successfull Reject Asset");

            return Json(new
            {
                msg = "Oh!",
                icon = "info",
                title = "Lost!",
                text = "Asset Lost Please Try To Get It."
            }, JsonRequestBehavior.AllowGet);
        }
     
        public JsonResult sendAsset(SendAssetViewModel sendAssetViewModel)
        {
            User currentUser = Session["user"] as User;

            if (!can.hasPermission("send_assets"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Send Asset", String.Empty, JsonConvert.SerializeObject(db.companyAssetsRent.Find(sendAssetViewModel.id)), "Tring to Send Asset but has no permission");

                return Json(new
                {
                    msg = "error",
                    icon = "error",
                    title = "Error!",
                    text = "Process Is Not Completed!"
                }, JsonRequestBehavior.AllowGet);
            }

            User user = Session["user"] as User;
          

            CompanyAssetRent companyAssetRent = db.companyAssetsRent.Find(sendAssetViewModel.id);

            companyAssetRent.status = (int)AssetStatus.WatingForReceive;
            companyAssetRent.from_location = user.location_id;

            if (sendAssetViewModel.location_id == -1)
                companyAssetRent.to_location = null;
            else
                companyAssetRent.to_location = sendAssetViewModel.location_id;

            db.Entry(companyAssetRent).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            Helpers.Helpers.SystemLogger(currentUser.id, "Send Asset", String.Empty, JsonConvert.SerializeObject(companyAssetRent), "Successfull Send Asset");

            return Json(new { msg = "done",
                icon = "success",
                title = "Sent",
                text = "Asset Sent Successfully."
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult rejectAsset(SendAssetViewModel sendAssetViewModel)
        {
            User currentUser = Session["user"] as User;

            if (!can.hasPermission("receive_assets"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Reject Asset", String.Empty, JsonConvert.SerializeObject(db.companyAssetsRent.Find(sendAssetViewModel.id)), "Tring to Reject Asset but has no permission");

                return Json(new
                {
                    msg = "error",
                    icon = "error",
                    title = "Error!",
                    text = "Process Is Not Completed!"
                }, JsonRequestBehavior.AllowGet);
            }

            CompanyAssetRent companyAssetRent = db.companyAssetsRent.Find(sendAssetViewModel.id);
            companyAssetRent.status = (int)AssetStatus.Lost;

            db.Entry(companyAssetRent).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            Helpers.Helpers.SystemLogger(currentUser.id, "Reject Asset", String.Empty, JsonConvert.SerializeObject(companyAssetRent), "Reject Send Asset");

            return Json(new { msg = "done",
                icon = "success",
                title = "Rejected!",
                text = "Asset Rejected And Marked As Lost!"
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult restoreAsset(int? id)
        {
            User currentUser = Session["user"] as User;

            CompanyAssetRent companyAssetRent = db.companyAssetsRent.Find(id);

            CompanyAssetRentHistory companyAssetRentHistory = db.companyAssetRentHistories.Where(s => s.to_location == companyAssetRent.from_location && s.asset_id == companyAssetRent.asset_id).OrderByDescending(s => s.created_at).FirstOrDefault();
            companyAssetRent.from_location = companyAssetRentHistory.from_location;
            companyAssetRent.to_location = companyAssetRentHistory.to_location;
            companyAssetRent.status = (int)AssetStatus.Received;
            db.Entry(companyAssetRent).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            Helpers.Helpers.SystemLogger(currentUser.id, "Restore Asset", String.Empty, JsonConvert.SerializeObject(companyAssetRent), "Successfull Restore Asset");


            return Json(new
            {
                msg = "Success",
                icon = "success",
                title = "Success",
                text = "Asset has been restored"
            }, JsonRequestBehavior.AllowGet);
        }

        public void ExportTrackingAssetsSheet()
        {
            User currentUser = Session["user"] as User;

            Helpers.Helpers.SystemLogger(currentUser.id, "Export Tracking Assets Sheet", String.Empty, String.Empty , "Successfull Export Tracking Assets Sheet");

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Available Assets");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells["A1:I1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:I1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:I1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "Tag ID";
            Sheet.Cells["B1"].Value = "Type";
            Sheet.Cells["C1"].Value = "Color";
            Sheet.Cells["D1"].Value = "From";
            Sheet.Cells["E1"].Value = "To";
            Sheet.Cells["F1"].Value = "Status";
            Sheet.Cells["G1"].Value = "Start Date";
            Sheet.Cells["H1"].Value = "Due Date";
            Sheet.Cells["I1"].Value = "Sent at";

            List<AssetViewModel> companyRentAssetData = (from asset in db.assets
                                        join comAssetRent in db.companyAssetsRent on asset.id equals comAssetRent.asset_id
                                        join location in db.locations on comAssetRent.from_location equals location.id into from_location
                                        from fl in from_location.DefaultIfEmpty()
                                        join tLocation in db.locations on comAssetRent.to_location equals tLocation.id into tLoc
                                        from tolocation in tLoc.DefaultIfEmpty()
                                        join rentOrder in db.rentOrders on comAssetRent.rent_order_id equals rentOrder.id
                                        select new AssetViewModel
                                        {
                                            id = comAssetRent.id,
                                            tag_id = asset.tag_id,
                                            type = asset.type,
                                            tall = asset.tall,
                                            status = comAssetRent.status,
                                            start_date = comAssetRent.start_date,
                                            from_location = comAssetRent.from_location,
                                            from_location_string = String.IsNullOrEmpty(fl.name) ? "Ripl" : fl.name,
                                            due_date = comAssetRent.due_date,
                                            to_location = tolocation.id == null ? 0 : tolocation.id,
                                            to_location_name = String.IsNullOrEmpty(tolocation.name) ? "Ripl" : tolocation.name,
                                            parent_id = fl.parent_id,
                                            company_id = fl.company_id,
                                            to_location_company_id = tolocation.company_id

                                        }).ToList();

            int row = 2;
            foreach (var item in companyRentAssetData)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.tag_id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.type;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.color;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.from_location_string;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.to_location_name;
                Sheet.Cells[string.Format("F{0}", row)].Value = (item.status==(int)AssetStatus.WatingForReceive)? "Wating For Receive ": (item.status == (int)AssetStatus.Received)? "Received":"Lost";
                Sheet.Cells[string.Format("G{0}", row)].Value = item.start_date.ToString();
                Sheet.Cells[string.Format("H{0}", row)].Value = item.due_date.ToString();
                Sheet.Cells[string.Format("I{0}", row)].Value = item.created_at.ToString();

                row++;
            }

            row++;
            colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
            text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Font.Color.SetColor(text);

            Sheet.Cells[string.Format("A{0}", row)].Value = "Total";
            Sheet.Cells[string.Format("B{0}", row)].Value = companyRentAssetData.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + DateTime.Now.ToString()+ "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
    }
}