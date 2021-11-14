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
namespace RiplTrackingSystem.Controllers
{
    //[CustomAuthenticationFilter]
    public class ReportController : Controller
    {
        DBContext db = new DBContext();
        // GET: Report
        public ActionResult Orders()
        {
            ViewBag.users = db.users.Where(l => l.location_id == null).Select(s => new { s.id, s.full_name }).ToList();
            ViewBag.locations = db.locations.Where(l => l.parent_id == null).Select(s => new { s.id, s.name }).ToList();
            return View();
        }

        public void OrdersReport(ReportViewModel report)
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Orders Report", String.Empty, String.Empty, "Successfull Export Orders Report");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Orders With Assets");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells["A1:H1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:H1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "ID";
            Sheet.Cells["B1"].Value = "Location";
            Sheet.Cells["C1"].Value = "Assets Count";
            Sheet.Cells["D1"].Value = "Assets";
            Sheet.Cells["E1"].Value = "Created By";
            Sheet.Cells["F1"].Value = "Start Date";
            Sheet.Cells["G1"].Value = "End Date";
            Sheet.Cells["H1"].Value = "Created at";
            var ordersQuery = (from rentOrder in db.rentOrders
                                               join user in db.users on rentOrder.created_by equals user.id
                                               join loc in db.locations on rentOrder.location_id equals loc.id
                                               //join comAssetRent in db.companyAssetsRent on asset.id equals comAssetRent.asset_id
                                               //group loc by comAssetRent.location_id into locID
                                               select new RentOrderViewModel
                                               {
                                                   id = rentOrder.id,
                                                   assets_count = rentOrder.assetes_count,
                                                   created_by = rentOrder.created_by,
                                                   location_name = loc.name,
                                                   location_type = loc.type,
                                                   notes = rentOrder.notes,
                                                   stringCreatedBy = user.full_name,
                                                   created_at = rentOrder.created_at,
                                                   start_date = rentOrder.start_date,
                                                   due_date = rentOrder.due_date,
                                                   location_id = rentOrder.location_id,
                                                   assets_ids = db.companyAssetsRent.Where(car => car.rent_order_id == rentOrder.id).Select(car => car.asset_id).ToList(),
                                                   assetsStatusAndLocation = (from asset in db.assets
                                                                              join comAssetRent in db.companyAssetsRent on asset.id equals comAssetRent.asset_id
                                                                              join to_loc in db.locations on comAssetRent.to_location equals to_loc.id into to
                                                                              from tLoc in to.DefaultIfEmpty()
                                                                              select new AssetViewModel
                                                                              {
                                                                                  tag_id = asset.tag_id,
                                                                                  status = comAssetRent.status,
                                                                                  rent_order_id = comAssetRent.rent_order_id,
                                                                                  finalDestination = String.IsNullOrEmpty(tLoc.name) ? "Ripl" : tLoc.name,
                                                                              }).Where(r => r.rent_order_id == rentOrder.id).ToList()

                                               }).Where(loc => loc.location_type == (int)(LocationType.Company));
            if (report.location_id != null)
            {
                ordersQuery = ordersQuery.Where(s => s.location_id == report.location_id);
            }

            if (report.user_id != null)
            {
                ordersQuery = ordersQuery.Where(s => s.created_by == report.user_id);
            }

            if (report.from_date != DateTime.MinValue)
            {
                ordersQuery = ordersQuery.Where(s => s.created_at >= report.from_date);
            }

            if (report.to_date != DateTime.MinValue)
            {
                ordersQuery = ordersQuery.Where(s => s.created_at <= report.to_date);
            }

            if (!string.IsNullOrEmpty(report.text_search))
            {
                ordersQuery = ordersQuery.Where(m => m.assets_count.ToString().ToLower().Contains(report.text_search.ToLower()) || m.id.ToString().ToLower().Contains(report.text_search.ToLower()) ||
                 m.location_name.ToLower().Contains(report.text_search.ToLower()));
            }

            List<RentOrderViewModel> orders = ordersQuery.ToList();
            int row = 2;
            foreach (var item in orders)
            {
                string orderAssets = String.Empty;
                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.location_name;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.assets_count;
                if (item.assetsStatusAndLocation != null)
                {
                    foreach (var asset in item.assetsStatusAndLocation)
                    {
                        orderAssets += "(" + asset.tag_id + "-" + (AssetStatus)asset.status + "),";
                    }
                    orderAssets = orderAssets.Remove(orderAssets.Length - 1);
                }
                Sheet.Cells[string.Format("D{0}", row)].Value = orderAssets;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.stringCreatedBy;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.start_date.ToString();
                Sheet.Cells[string.Format("G{0}", row)].Value = item.due_date.ToString();
                Sheet.Cells[string.Format("H{0}", row)].Value = item.created_at.ToString();
                row++;
            }

            colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
            text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Font.Color.SetColor(text);

            Sheet.Cells[string.Format("A{0}", row)].Value = "Total";
            Sheet.Cells[string.Format("B{0}", row)].Value = orders.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

        public ActionResult Assets()
        {
            ViewBag.users = db.users.Where(l => l.location_id == null).Select(s => new { s.id, s.full_name }).ToList();
            return View();
        }
        public void AssetsReport(ReportViewModel report)
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Assets Report", String.Empty, String.Empty, "Successfull Export Assets Report");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Assets");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells["A1:D1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:D1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:D1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "Tag ID";
            Sheet.Cells["B1"].Value = "Type";
            Sheet.Cells["C1"].Value = "Color";
            Sheet.Cells["D1"].Value = "Created at";

            var assetsQuery = db.assets.Select(s => new AssetViewModel
            {
                id = s.id,
                tag_id = s.tag_id,
                type = s.type,
                tall = s.tall,
                color = s.color,
                created_at = s.created_at,
                updated_at = s.updated_at,
                created_by = s.created_by,
                string_created_at = s.created_at.ToString()

            });

            if (report.asset_type != null)
            {
                assetsQuery = assetsQuery.Where(s => s.type.ToLower() == report.asset_type.ToLower());
            }

            if (report.user_id != null)
            {
                assetsQuery = assetsQuery.Where(s => s.created_by == report.user_id);
            }

            if (report.asset_color != null)
            {
                assetsQuery = assetsQuery.Where(s => s.color.ToLower() == report.asset_color.ToLower());
            }

            if (report.from_date != DateTime.MinValue)
            {
                assetsQuery = assetsQuery.Where(s => s.created_at >= report.from_date);
            }

            if (report.to_date != DateTime.MinValue)
            {
                assetsQuery = assetsQuery.Where(s => s.created_at <= report.to_date);
            }

            if (!string.IsNullOrEmpty(report.text_search))
            {
                assetsQuery = assetsQuery.Where(m => m.tag_id.ToLower().Contains(report.text_search.ToLower()) || m.id.ToString().ToLower().Contains(report.text_search.ToLower()) ||
                 m.type.ToLower().Contains(report.text_search.ToLower()) || m.tall.ToLower().Contains(report.text_search.ToLower()));
            }

            List<AssetViewModel> assets = assetsQuery.ToList();

            List<AssetViewModel> finalAssets = new List<AssetViewModel>();
            if(report.asset_status == 1)
            {
                finalAssets = assets;
            }
            else
            {
                List<AssetViewModel> availabeAssets = new List<AssetViewModel>();
                foreach (var asset in assets)
                {
                    List<CompanyAssetRent> companyAssetRents = db.companyAssetsRent.Where(s => s.asset_id == asset.id).ToList();
                    bool isAvailable = true;
                    foreach (var companyAssetRent in companyAssetRents)
                    {
                        if (DateTime.Now <= companyAssetRent.due_date)
                        {
                            isAvailable = false;
                            break;
                        }
                    }

                    if (isAvailable == true)
                        availabeAssets.Add(asset);
                }

                finalAssets = availabeAssets;
            }

            int row = 2;
            foreach (var item in finalAssets)
            {
                string locaitonFactories = String.Empty;
                string locaitonStores = String.Empty;
                string locaitonDistributors = String.Empty;

                Sheet.Cells[string.Format("A{0}", row)].Value = item.tag_id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.type;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.color;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.created_at.ToString();
                row++;
            }

            row++;
            colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
            text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Font.Color.SetColor(text);

            Sheet.Cells[string.Format("A{0}", row)].Value = "Total";
            Sheet.Cells[string.Format("B{0}", row)].Value = assets.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

        public ActionResult AssetTracking()
        {
            ViewBag.from_locations = db.locations.Select(s => new { s.id, s.name }).ToList();
            ViewBag.to_locations = db.locations.Select(s => new { s.id, s.name }).ToList();
            return View();
        }

        public void AssetsTrackingReport(ReportViewModel report)
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Asset Tracking Report", String.Empty, String.Empty, "Successfull Export Asset Tracking Report");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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

            var assetsQuery = (from asset in db.assets
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
                                   created_at = comAssetRent.created_at,
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

                               });
            if (report.tag_id != null)
            {
                assetsQuery = assetsQuery.Where(s => s.tag_id == report.tag_id);
            }

            if (report.asset_type != null)
            {
                assetsQuery = assetsQuery.Where(s => s.type.ToLower() == report.asset_type.ToLower());
            }

            if (report.asset_color != null)
            {
                assetsQuery = assetsQuery.Where(s => s.color.ToLower() == report.asset_color.ToLower());
            }

            if (report.from_location != null)
            {
                assetsQuery = assetsQuery.Where(s => s.from_location == report.from_location);
            }

            if (report.to_location != null)
            {
                assetsQuery = assetsQuery.Where(s => s.to_location == report.to_location);
            }

            if (report.to_location != null)
            {
                assetsQuery = assetsQuery.Where(s => s.to_location == report.to_location);
            }

            if (report.asset_status != null)
            {
                assetsQuery = assetsQuery.Where(s => s.status == report.asset_status);
            }

            if (report.from_date != DateTime.MinValue)
            {
                assetsQuery = assetsQuery.Where(s => s.created_at >= report.from_date);
            }

            if (report.to_date != DateTime.MinValue)
            {
                assetsQuery = assetsQuery.Where(s => s.created_at <= report.to_date);
            }

            if (!string.IsNullOrEmpty(report.text_search))
            {
                assetsQuery = assetsQuery.Where(m => m.tag_id.ToLower().Contains(report.text_search.ToLower()) || m.id.ToString().ToLower().Contains(report.text_search.ToLower()) ||
                 m.type.ToLower().Contains(report.text_search.ToLower()) || m.tall.ToLower().Contains(report.text_search.ToLower()));
            }

            List<AssetViewModel> assets = assetsQuery.ToList();

            int row = 2;
            foreach (var item in assets)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.tag_id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.type;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.color;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.from_location_string;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.to_location_name;
                Sheet.Cells[string.Format("F{0}", row)].Value = (item.status == (int)AssetStatus.WatingForReceive) ? "Wating For Receive " : (item.status == (int)AssetStatus.Received) ? "Received" : "Lost";
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
            Sheet.Cells[string.Format("B{0}", row)].Value = assets.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + DateTime.Now.ToString() + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

        public ActionResult Requests()
        {
            ViewBag.locations = db.locations.Where(l => l.parent_id == null).Select(s => new { s.id, s.name }).ToList();

            return View();
        }

        public void RequestsReport(ReportViewModel report)
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Request Report", String.Empty, String.Empty, "Successfull Export Request Report");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Requests");

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

            var requestsQuery = (from request in db.requests
                                 join location in db.locations on request.location_id equals location.id
                                 join user in db.users on request.created_by equals user.id
                                 select new RequestViewModel
                                 {
                                     id = request.id,
                                     type = request.type,
                                     stringLocation = location.name,
                                     number_of_assets = request.number_of_assets,
                                     location_id = request.location_id,
                                     notes = request.notes,
                                     stringUser = user.full_name,
                                     stringType = request.type == 1 ? "New" : "Return",
                                     created_at = request.created_at,
                                     stringCreated_at = request.created_at.ToString()


                                 });

            if (report.request_type != null)
            {
                requestsQuery = requestsQuery.Where(s => s.type == report.request_type);
            }

            if (report.location_id != null)
            {
                requestsQuery = requestsQuery.Where(s => s.location_id == report.location_id);
            }

            if (report.from_date != DateTime.MinValue)
            {
                requestsQuery = requestsQuery.Where(s => s.created_at >= report.from_date);
            }

            if (report.to_date != DateTime.MinValue)
            {
                requestsQuery = requestsQuery.Where(s => s.created_at <= report.to_date);
            }

            if (!string.IsNullOrEmpty(report.text_search))
            {
                requestsQuery = requestsQuery.Where(m => m.stringLocation.ToLower().Contains(report.text_search.ToLower()) || m.id.ToString().ToLower().Contains(report.text_search.ToLower()) ||
                 m.notes.ToLower().Contains(report.text_search.ToLower()) || m.stringUser.ToLower().Contains(report.text_search.ToLower()));
            }

            List<RequestViewModel> requests = requestsQuery.ToList();

            int row = 2;
            foreach (var item in requests)
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
            Sheet.Cells[string.Format("B{0}", row)].Value = requests.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

        public ActionResult Locations()
        {
            ViewBag.locations = db.locations.Where(l => l.parent_id == null).Select(s => new { s.id, s.name }).ToList();

            return View();
        }

        public void LocationsReport(ReportViewModel report)
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Locations Report", String.Empty, String.Empty, "Successfull Export Locations Report");

            if (!string.IsNullOrWhiteSpace(report.location_id_search))
            {
                report.location_id = int.Parse(report.location_id_search);
            }
            if (!string.IsNullOrEmpty(report.date_form_search))
            {
                report.from_date = Convert.ToDateTime(report.date_form_search);
            }
            if (!string.IsNullOrEmpty(report.date_to_search))
            {
                report.to_date = Convert.ToDateTime(report.date_to_search);
            }

            if (report.location_report_type == 2)
            {
                ExcelPackage Ep = new ExcelPackage();

                ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Orders");
                Sheet.Cells["A1"].Value = "Type";
                Sheet.Cells["B1"].Value = "Name";
                Sheet.Cells["C1"].Value = "Phone";
                Sheet.Cells["D1"].Value = "Assets Count";
                Sheet.Cells["E1"].Value = "Available Assets Count";
                Sheet.Cells["F1"].Value = "Lost Assets";
                Sheet.Cells["G1"].Value = "Created at";

                System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
                Sheet.Cells["A1:G1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells["A1:G1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
                System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
                Sheet.Cells["A1:G1"].Style.Font.Color.SetColor(text);

                var locationsData = (from location in db.locations

                                     select new CompanyDetail
                                     {
                                         id = location.id,
                                         name = location.name,
                                         description = location.description,
                                         address = location.address,
                                         created_at = location.created_at,
                                         phone = location.phone,
                                         parent_id = location.parent_id,
                                         stringCreatedAt = location.created_at.ToString(),
                                         factories = db.locations.Where(s => s.company_id == location.id && s.type == (int)LocationType.Factory).
                                         Select(fac => new locationViewModel
                                         {
                                             id = fac.id,
                                             name = fac.name,
                                             created_at = fac.created_at,
                                             actual_number_of_assets = db.companyAssetsRent.Where(sn => sn.to_location ==fac.id && sn.status == (int)AssetStatus.Received && sn.due_date >= DateTime.Now).ToList().Count(),
                                             number_of_assets = db.companyAssetsRent.Where(sn => sn.to_location == fac.id && sn.due_date >= DateTime.Now).ToList().Count(),
                                             lost_number_of_assets = db.companyAssetsRent.Where(sn => sn.to_location == fac.id && sn.status == (int)AssetStatus.Lost && sn.due_date >= DateTime.Now).ToList().Count(),
                                             phone = fac.phone,
                                             stringCreatedAt = fac.created_at.ToString(),
                                         }).ToList(),
                                         type = location.type,
                                         displayedType = (location.type == 1) ? "Company" : (location.type == 2) ? "Factory" : (location.type == 3) ? "Store" : "Distributor",
                                         displayedParent = (location.parent_id != null) ? db.locations.Where(l => l.id == location.parent_id).FirstOrDefault().name : "",
                                         actual_number_of_assets = db.companyAssetsRent.Where(s => s.to_location == location.id && s.status == (int)AssetStatus.Received && s.due_date >= DateTime.Now).ToList().Count(),
                                         number_of_assets = db.rentOrders.Where(s => s.location_id == location.id && s.due_date >= DateTime.Now).ToList().Count() != 0 ? db.rentOrders.Where(s => s.location_id == location.id && s.due_date >= DateTime.Now).Select(s => s.assetes_count).Sum() : 0,
                                         lost_number_of_assets = db.companyAssetsRent.Where(sn => sn.to_location == location.id && sn.status == (int)AssetStatus.Lost && sn.due_date >= DateTime.Now).ToList().Count(),

                                     }).Where(l => l.type == (int)LocationType.Company);

                if (report.location_id != null)
                {
                    locationsData = locationsData.Where(s => s.id == report.location_id);
                }

                if (report.from_date != DateTime.MinValue)
                {
                    locationsData = locationsData.Where(s => s.created_at >= report.from_date);
                }

                if (report.to_date != DateTime.MinValue)
                {
                    locationsData = locationsData.Where(s => s.created_at <= report.to_date);
                }

                if (!string.IsNullOrEmpty(report.text_search))
                {
                    locationsData = locationsData.Where(m => m.name.ToLower().Contains(report.text_search.ToLower()) || m.id.ToString().ToLower().Contains(report.text_search.ToLower()) ||
                     m.description.ToLower().Contains(report.text_search.ToLower()) || m.description.ToLower().Contains(report.text_search.ToLower()) || m.address.ToLower().Contains(report.text_search.ToLower()) || m.phone.ToLower().Contains(report.text_search.ToLower()));
                }

                int row = 2;
                foreach (var item in locationsData)
                {
                    colFromHex = System.Drawing.ColorTranslator.FromHtml("#d4d3e3");
                    Sheet.Cells[string.Format("A{0}:G{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("A{0}:G{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    Sheet.Cells[string.Format("A{0}", row)].Value = "Company";
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.name;
                    Sheet.Cells[string.Format("C{0}", row)].Value = item.phone;
                    Sheet.Cells[string.Format("D{0}", row)].Value = item.number_of_assets;
                    Sheet.Cells[string.Format("E{0}", row)].Value = item.actual_number_of_assets;
                    Sheet.Cells[string.Format("F{0}", row)].Value = item.lost_number_of_assets;
                    Sheet.Cells[string.Format("G{0}", row)].Value = item.stringCreatedAt;

                    row++;

                    if (item.factories.Count() != 0)
                    {
                        foreach (var factory in item.factories)
                        {
                            colFromHex = System.Drawing.ColorTranslator.FromHtml("#a7a7b5");
                            Sheet.Cells[string.Format("A{0}:G{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            Sheet.Cells[string.Format("A{0}:G{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);

                            Sheet.Cells[string.Format("A{0}", row)].Value = "Factory";

                            Sheet.Cells[string.Format("B{0}", row)].Value = factory.name;
                            Sheet.Cells[string.Format("C{0}", row)].Value = factory.phone;
                            Sheet.Cells[string.Format("D{0}", row)].Value = factory.number_of_assets;
                            Sheet.Cells[string.Format("E{0}", row)].Value = factory.actual_number_of_assets;
                            Sheet.Cells[string.Format("F{0}", row)].Value = factory.lost_number_of_assets;
                            Sheet.Cells[string.Format("G{0}", row)].Value = factory.stringCreatedAt; 
                            row++;


                            List<locationViewModel> factoryStores = db.locations.Where(s => s.parent_id == factory.id && s.type == (int)LocationType.Store).Select(s => new locationViewModel
                            {
                                id = s.id,
                                name = s.name,
                                created_at = s.created_at,
                                actual_number_of_assets = db.companyAssetsRent.Where(sv => sv.to_location == s.id && sv.status == (int)AssetStatus.Received && sv.due_date >= DateTime.Now).ToList().Count(),
                                number_of_assets = db.companyAssetsRent.Where(sn => sn.to_location == s.id && sn.due_date >= DateTime.Now).ToList().Count(),
                                lost_number_of_assets = db.companyAssetsRent.Where(sn => sn.to_location == s.id && sn.status == (int)AssetStatus.Lost && sn.due_date >= DateTime.Now).ToList().Count(),                                phone = s.phone,
                                stringCreatedAt = s.created_at.ToString(),
                            }).ToList();

                            if (factoryStores.Count() != 0)
                            {

                                foreach (locationViewModel factoryStore in factoryStores)
                                {
                                    colFromHex = System.Drawing.ColorTranslator.FromHtml("#868691");
                                    Sheet.Cells[string.Format("A{0}:G{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    Sheet.Cells[string.Format("A{0}:G{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);


                                    Sheet.Cells[string.Format("A{0}", row)].Value = "Store";
                                    Sheet.Cells[string.Format("B{0}", row)].Value = factoryStore.name;
                                    Sheet.Cells[string.Format("C{0}", row)].Value = factoryStore.phone;
                                    Sheet.Cells[string.Format("D{0}", row)].Value = factoryStore.number_of_assets;
                                    Sheet.Cells[string.Format("E{0}", row)].Value = factoryStore.actual_number_of_assets;
                                    Sheet.Cells[string.Format("F{0}", row)].Value = factoryStore.lost_number_of_assets;
                                    Sheet.Cells[string.Format("G{0}", row)].Value = factoryStore.stringCreatedAt; 
                                    row++;
                                    List<locationViewModel> storeDistributors = db.locations.Where(s => s.parent_id == factoryStore.id && s.type == (int)LocationType.Distributor).Select(s => new locationViewModel
                                    {
                                        id = s.id,
                                        name = s.name,
                                        created_at = s.created_at,
                                        actual_number_of_assets = db.companyAssetsRent.Where(sv => sv.to_location == s.id && sv.status == (int)AssetStatus.Received && sv.due_date >= DateTime.Now).ToList().Count(),
                                        number_of_assets = db.companyAssetsRent.Where(sn => sn.to_location == s.id && sn.due_date >= DateTime.Now).ToList().Count(),
                                        lost_number_of_assets = db.companyAssetsRent.Where(sn => sn.to_location == s.id && sn.status == (int)AssetStatus.Lost && sn.due_date >= DateTime.Now).ToList().Count(),
                                        phone = s.phone,
                                        stringCreatedAt = s.created_at.ToString(),
                                    }).ToList();

                                    if (storeDistributors.Count() != 0)
                                    {
                                        colFromHex = System.Drawing.ColorTranslator.FromHtml("#676773");
                                        Sheet.Cells[string.Format("A{0}:G{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        Sheet.Cells[string.Format("A{0}:G{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);

                                        Sheet.Cells[string.Format("A{0}", row)].Value = "Distributor";

                                        foreach (locationViewModel storeDistributor in storeDistributors)
                                        {
                                            Sheet.Cells[string.Format("B{0}", row)].Value = storeDistributor.name;
                                            Sheet.Cells[string.Format("C{0}", row)].Value = storeDistributor.phone;
                                            Sheet.Cells[string.Format("D{0}", row)].Value = storeDistributor.number_of_assets;
                                            Sheet.Cells[string.Format("E{0}", row)].Value = storeDistributor.actual_number_of_assets;
                                            Sheet.Cells[string.Format("F{0}", row)].Value = storeDistributor.lost_number_of_assets;
                                            Sheet.Cells[string.Format("G{0}", row)].Value = storeDistributor.stringCreatedAt; 
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
                    Sheet.Cells[string.Format("A{0}:F{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("A{0}:F{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);

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
            else
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage Ep = new ExcelPackage();
                ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Locations");

                System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
                Sheet.Cells["A1:I1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells["A1:I1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
                System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
                Sheet.Cells["A1:I1"].Style.Font.Color.SetColor(text);

                Sheet.Cells["A1"].Value = "ID";
                Sheet.Cells["B1"].Value = "Name";
                Sheet.Cells["C1"].Value = "Phone";
                Sheet.Cells["D1"].Value = "Assets Count";
                Sheet.Cells["E1"].Value = "Available Assets Count";
                Sheet.Cells["F1"].Value = "Factories";
                Sheet.Cells["G1"].Value = "Stores";
                Sheet.Cells["H1"].Value = "Distributors";
                Sheet.Cells["I1"].Value = "Created at";

                var locationsQuery = (from location in db.locations
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
                                          created_at = location.created_at,
                                          stores = db.locations.Where(s => s.company_id == location.id && s.type == (int)LocationType.Store).Select(s => new locationViewModel
                                          {
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
                                          number_of_assets = db.rentOrders.Where(s => s.location_id == location.id && s.due_date >= DateTime.Now).ToList().Count() != 0 ? db.rentOrders.Where(s => s.location_id == location.id && s.due_date >= DateTime.Now).Select(s => s.assetes_count).Sum() : 0,
                                          actual_number_of_assets = db.companyAssetsRent.Where(s => s.to_location == location.id && s.status == (int)AssetStatus.Received && s.due_date >= DateTime.Now).ToList().Count(),

                                      }).Where(l => l.type == (int)LocationType.Company);


                if (report.location_id != null)
                {
                    locationsQuery = locationsQuery.Where(s => s.id == report.location_id);
                }

                if (report.from_date != DateTime.MinValue)
                {
                    locationsQuery = locationsQuery.Where(s => s.created_at >= report.from_date);
                }

                if (report.to_date != DateTime.MinValue)
                {
                    locationsQuery = locationsQuery.Where(s => s.created_at <= report.to_date);
                }

                if (!string.IsNullOrEmpty(report.text_search))
                {
                    locationsQuery = locationsQuery.Where(m => m.name.ToLower().Contains(report.text_search.ToLower()) || m.id.ToString().ToLower().Contains(report.text_search.ToLower()) ||
                     m.description.ToLower().Contains(report.text_search.ToLower()) || m.description.ToLower().Contains(report.text_search.ToLower()) || m.address.ToLower().Contains(report.text_search.ToLower()) || m.phone.ToLower().Contains(report.text_search.ToLower()));
                }

                List<CompanyDetail> locations = locationsQuery.ToList();

                int row = 2;
                foreach (var item in locations)
                {
                    string locaitonFactories = String.Empty;
                    string locaitonStores = String.Empty;
                    string locaitonDistributors = String.Empty;

                    Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.name;
                    Sheet.Cells[string.Format("C{0}", row)].Value = item.phone;
                    Sheet.Cells[string.Format("D{0}", row)].Value = item.number_of_assets;
                    Sheet.Cells[string.Format("E{0}", row)].Value = item.actual_number_of_assets;

                    if (item.factories.Count() != 0)
                    {
                        foreach (var factory in item.factories)
                        {
                            locaitonFactories += factory.name + ",";
                        }
                        locaitonFactories = locaitonFactories.Remove(locaitonFactories.Length - 1);
                    }
                    Sheet.Cells[string.Format("F{0}", row)].Value = locaitonFactories;

                    if (item.stores.Count() != 0)
                    {
                        foreach (var store in item.stores)
                        {
                            locaitonStores += store.name + ",";
                        }
                        locaitonStores = locaitonStores.Remove(locaitonStores.Length - 1);
                    }
                    Sheet.Cells[string.Format("G{0}", row)].Value = locaitonStores;

                    if (item.distributors.Count() != 0)
                    {
                        foreach (var distributor in item.distributors)
                        {
                            locaitonDistributors += distributor.name + ",";
                        }
                        locaitonDistributors = locaitonDistributors.Remove(locaitonDistributors.Length - 1);
                    }
                    Sheet.Cells[string.Format("H{0}", row)].Value = locaitonDistributors;
                    Sheet.Cells[string.Format("I{0}", row)].Value = item.stringCreatedAt;

                    row++;
                }

                row++;
                colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
                Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
                text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
                Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Font.Color.SetColor(text);

                Sheet.Cells[string.Format("A{0}", row)].Value = "Total";
                Sheet.Cells[string.Format("B{0}", row)].Value = locations.Count();

                Sheet.Cells["A:AZ"].AutoFitColumns();
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
                Response.BinaryWrite(Ep.GetAsByteArray());
                Response.End();
            }
            
        }
    }
}