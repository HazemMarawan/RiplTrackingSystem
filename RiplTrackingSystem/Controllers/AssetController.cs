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
    public class AssetController : Controller
    {
        DBContext db = new DBContext();
        // GET: Asset
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;

            if (!can.hasPermission("access_asset")) //hazemm
            {
                
                Helpers.Helpers.SystemLogger(currentUser.id, "Access Assets", String.Empty, String.Empty, "Tring to access Assets but has no permission");
                return RedirectToAction("Error404", "Error");
            }
            if (Request.IsAjaxRequest())
            {
                //Helpers.Helpers.SystemLogger(currentUser.id, "Access Assets", String.Empty, String.Empty, "Successfull Access to Assets Page");

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var asset_type = Request.Form.GetValues("columns[0][search][value]")[0];
                var asset_color = Request.Form.GetValues("columns[1][search][value]")[0];
                var user_id = Request.Form.GetValues("columns[2][search][value]")[0];
                var from_date = Request.Form.GetValues("columns[3][search][value]")[0];
                var to_date = Request.Form.GetValues("columns[4][search][value]")[0];
                var visibility_status = Request.Form.GetValues("columns[5][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var assetsData = db.assets.Select(s => new AssetViewModel
                {
                    id = s.id,
                    tag_id = s.tag_id,
                    type = s.type,
                    tall = s.tall,
                    color = s.color,
                    created_by = s.created_by,
                    created_at = s.created_at,
                    updated_at = s.updated_at,
                    string_created_at = s.created_at.ToString(),
                    active = s.active

                });

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    assetsData = assetsData.Where(m => m.tag_id.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                     m.type.ToLower().Contains(searchValue.ToLower()) || m.tall.ToLower().Contains(searchValue.ToLower()));
                }
                if (!string.IsNullOrEmpty(asset_type))
                {
                    assetsData = assetsData.Where(s => s.type.ToLower() == asset_type.ToLower());
                }

                if (!string.IsNullOrEmpty(user_id))
                {
                    int userID = int.Parse(user_id);
                    assetsData = assetsData.Where(s => s.created_by == userID);
                }

                if (!string.IsNullOrEmpty(asset_color))
                {
                    assetsData = assetsData.Where(s => s.color.ToLower() == asset_color.ToLower());
                }

                if (!string.IsNullOrEmpty(from_date))
                {
                    if (Convert.ToDateTime(from_date) != DateTime.MinValue)
                    {
                        DateTime from = Convert.ToDateTime(from_date);
                        assetsData = assetsData.Where(s => s.created_at >= from);
                    }
                }

                if (!string.IsNullOrEmpty(to_date))
                {
                    if (Convert.ToDateTime(to_date) != DateTime.MinValue)
                    {
                        DateTime to = Convert.ToDateTime(to_date);
                        assetsData = assetsData.Where(s => s.created_at <= to);
                    }
                }

                if(!String.IsNullOrEmpty(visibility_status))
                {
                    int visibility_status_int = int.Parse(visibility_status);
                    assetsData = assetsData.Where(s => s.active == visibility_status_int);
                }
                else
                {
                    assetsData = assetsData.Where(s => s.active == 1);
                }
               
                //total number of rows count     
                var displayResult = assetsData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = assetsData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.users = db.users.Where(l => l.location_id == null).Select(s => new { s.id, s.full_name }).ToList();

            return View();
        }

        [HttpPost]
        public JsonResult saveAsset(AssetViewModel assetVM)
        {
            User currentUser = Session["user"] as User;

            if (!can.hasPermission("edit_asset"))
            {
                if(assetVM.id == 0)
                    Helpers.Helpers.SystemLogger(currentUser.id, "Add Assets", String.Empty, JsonConvert.SerializeObject(assetVM), "Successfull Access to Assets Page");
                else
                    Helpers.Helpers.SystemLogger(currentUser.id, "Edit Assets", JsonConvert.SerializeObject(db.assets.Find(assetVM.id)), JsonConvert.SerializeObject(assetVM), "Successfull Access to Assets Page");
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }

            if (assetVM.id == 0)
            {
                Asset asset = AutoMapper.Mapper.Map<AssetViewModel, Asset>(assetVM);

                asset.status = "New";
                asset.updated_at = DateTime.Now;
                asset.created_at = DateTime.Now;
                asset.active = 1;
                db.assets.Add(asset);

                Helpers.Helpers.SystemLogger(currentUser.id, "Add Assets", String.Empty, JsonConvert.SerializeObject(assetVM), "Successfull Add Asset");

            }
            else
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Edit Assets", JsonConvert.SerializeObject(db.assets.Find(assetVM.id)), JsonConvert.SerializeObject(assetVM), "Successfull Edit Assets");

                Asset oldAsset = db.assets.Find(assetVM.id);

                oldAsset.tag_id = assetVM.tag_id;
                oldAsset.color = assetVM.color;
                oldAsset.type = assetVM.type;
                oldAsset.updated_at = DateTime.Now;

                db.Entry(oldAsset).State = System.Data.Entity.EntityState.Modified;


            }
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteAsset(int id)
        {
            User currentUser = Session["user"] as User;

            if (!can.hasPermission("delete_asset"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Delete Assets", String.Empty, JsonConvert.SerializeObject(db.assets.Find(id)), "Tring to Delete Asset but has no permission");
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }

            Asset deleteAsset = db.assets.Find(id);
            deleteAsset.active = 0;
            db.Entry(deleteAsset).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            Helpers.Helpers.SystemLogger(currentUser.id, "Delete Assets", String.Empty, JsonConvert.SerializeObject(deleteAsset), "Successfull Delete Asset");

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult restoreAsset(int id)
        {
            User currentUser = Session["user"] as User;

            //if (!can.hasPermission("restore_asset"))
            //{
            //    Helpers.Helpers.SystemLogger(currentUser.id, "Delete Assets", String.Empty, JsonConvert.SerializeObject(db.assets.Find(id)), "Tring to Delete Asset but has no permission");
            //    return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            //}

            Asset restoreAsset = db.assets.Find(id);
            restoreAsset.active = 1;
            db.Entry(restoreAsset).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            Helpers.Helpers.SystemLogger(currentUser.id, "Restore Assets", String.Empty, JsonConvert.SerializeObject(restoreAsset), "Successfull Restored Asset");

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult availabeAssets()
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("availabe_assets"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Access Assets", String.Empty, String.Empty, "Tring to access Available Assets but has no permission");
                return RedirectToAction("Error404", "Error");
            }
            //Helpers.Helpers.SystemLogger(currentUser.id, "Access Available Assets", String.Empty, String.Empty, "Successfull access to Available Assets");

            List<Asset> allAssts = db.assets.ToList();
            List<Asset> availabeAssets = new List<Asset>();
            foreach (var asset in allAssts)
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
            return View(availabeAssets);
        }

        public ActionResult ManageLost()
        {
            User currentUser = Session["user"] as User;

            if (!can.hasPermission("access_manage_lost"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Access Lost Assets", String.Empty, String.Empty, "Tring to access Lost Assets but has no permission");
                return RedirectToAction("Error404", "Error");
            }

            User user = Session["user"] as User;

            ViewBag.currentLocation = user.location_id == null ? 0 : user.location_id;

            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[0][search][value]")[0];
                var to_date = Request.Form.GetValues("columns[1][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var companyRentAssetData = (from asset in db.assets
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

                                            }).Where(s=>s.from_location == user.location_id && s.status == (int) AssetStatus.Lost);
               
                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    companyRentAssetData = companyRentAssetData.Where(m => m.tag_id.ToString().ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                     m.type.ToLower().Contains(searchValue.ToLower()));
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
    
            return View();
        }

        public void ExportAssetsSheetTempalte()
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Assets Sheet Tempalte", String.Empty, String.Empty, "Successfull Export Assets Sheet Template");

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Assets");
            Sheet.Cells["A1"].Value = "Tag ID";
            Sheet.Cells["B1"].Value = "Type";
            Sheet.Cells["C1"].Value = "Color";

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        public void ExportAssetsSheet()
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Assets Sheet", String.Empty, String.Empty, "Successfull Export Assets Sheet");

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

            List<AssetViewModel> assetsData = db.assets.Select(s => new AssetViewModel
            {
                id = s.id,
                tag_id = s.tag_id,
                type = s.type,
                tall = s.tall,
                color = s.color,
                created_at = s.created_at,
                updated_at = s.updated_at,
                string_created_at = s.created_at.ToString()

            }).ToList();

            int row = 2;
            foreach (var item in assetsData)
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
            Sheet.Cells[string.Format("B{0}", row)].Value = assetsData.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

        public void ExportLostAssetsSheet()
        {
            User user = Session["user"] as User;
            Helpers.Helpers.SystemLogger(user.id, "Export Lost Assets Sheet", String.Empty, String.Empty, "Successfull Export Lost Assets Sheet Tempalte");

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Assets");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells["A1:I1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:I1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:I1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "Id";
            Sheet.Cells["B1"].Value = "Tag ID";
            Sheet.Cells["C1"].Value = "Type";
            Sheet.Cells["D1"].Value = "Tall";
            Sheet.Cells["E1"].Value = "From";
            Sheet.Cells["F1"].Value = "Current Location";
            Sheet.Cells["G1"].Value = "Status";
            Sheet.Cells["H1"].Value = "Start Date";
            Sheet.Cells["I1"].Value = "Due Date";

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

                                        }).Where(s => s.from_location == user.location_id && s.status == (int)AssetStatus.Lost).ToList();


            int row = 2;
            foreach (var item in companyRentAssetData)
            {
                string locaitonFactories = String.Empty;
                string locaitonStores = String.Empty;
                string locaitonDistributors = String.Empty;

                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.tag_id;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.type;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.tall;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.from_location_string;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.to_location_name;
                Sheet.Cells[string.Format("G{0}", row)].Value = "Lost";
                Sheet.Cells[string.Format("H{0}", row)].Value = item.start_date.ToString();
                Sheet.Cells[string.Format("I{0}", row)].Value = item.due_date.ToString();

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
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

        public void ExportAvailableAssetsSheet()
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Available Assets Sheet", String.Empty, String.Empty, "Successfull Export Available Assets Sheet");

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Available Assets");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells["A1:D1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:D1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:D1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "Tag ID";
            Sheet.Cells["B1"].Value = "Type";
            Sheet.Cells["C1"].Value = "Color";
            Sheet.Cells["D1"].Value = "Created at";

            List<Asset> allAssts = db.assets.ToList();
            List<Asset> availabeAssets = new List<Asset>();
            foreach (var asset in allAssts)
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

            int row = 2;
            foreach (var item in availabeAssets)
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
            Sheet.Cells[string.Format("B{0}", row)].Value = availabeAssets.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        public JsonResult ImportExcel(HttpPostedFileBase file)
        {
            User user = Session["user"] as User;
            Guid guid = Guid.NewGuid();
            var InputFileName = Path.GetFileName(file.FileName);
            var ServerSavePath = Path.Combine(Server.MapPath("~/Files/Assets/") + guid.ToString() + "_Assets" + Path.GetExtension(file.FileName));
            file.SaveAs(ServerSavePath);

            //Save the uploaded Excel file.

            //Open the Excel file in Read Mode using OpenXml.
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(ServerSavePath, false))
            {
                //Read the first Sheet from Excel file.
                Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();

                //Get the Worksheet instance.
                Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;

                //Fetch all the rows present in the Worksheet.
                IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();

                //Create a new DataTable.
                DataTable dt = new DataTable();

                //Loop through the Worksheet rows.
                foreach (Row row in rows)
                {
                    //Use the first row to add columns to DataTable.
                    if (row.RowIndex.Value == 1)
                    {
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            dt.Columns.Add(GetValue(doc, cell));
                        }
                    }
                    else
                    {
                        //Add rows to DataTable.
                        dt.Rows.Add();
                        int i = 0;
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = GetValue(doc, cell);
                            i++;
                        }
                    }
                }
                List<Asset> importedAssets = new List<Asset>();
                for (int i= 0; i < dt.Rows.Count;i++)
                {
                    string TagId = dt.Rows[i][0].ToString();
                    string Type = dt.Rows[i][1].ToString();
                    string Color = dt.Rows[i][2].ToString();

                    Asset asset = new Asset();
                    asset.tag_id = TagId;
                    asset.type = Type;
                    asset.color = Color;
                    asset.created_at = DateTime.Now;
                    asset.updated_at = DateTime.Now;
                    asset.created_by = user.id;

                    importedAssets.Add(asset);
                }
                db.assets.AddRange(importedAssets);
                db.SaveChanges();

                User currentUser = Session["user"] as User;
                Helpers.Helpers.SystemLogger(currentUser.id, "Import Assets Sheet", String.Empty, JsonConvert.SerializeObject(importedAssets), "Successfull Import Assets Sheet");
            }

            return Json(new { msg = "done" }, JsonRequestBehavior.AllowGet);
        }

        private string GetValue(SpreadsheetDocument doc, Cell cell)
        {
            string value = cell.CellValue.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
            }
            return value;
        }

    }
}