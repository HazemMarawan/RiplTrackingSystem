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
using Newtonsoft.Json;

namespace RiplTrackingSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class SendAssetController : Controller
    {
        DBContext db = new DBContext();
        // GET: SendAsset
        public ActionResult Index()
        {
            User user = Session["user"] as User;
            if (!can.hasPermission("access_ripl_order"))
            {
                Helpers.Helpers.SystemLogger(user.id, "Access Orders", String.Empty, String.Empty, "Trining to Access Orders but has no permission");

                return RedirectToAction("Error404", "Error");
            }

            
            ViewBag.currentLocation = user.location_id;
            ViewBag.isRipl = user.location_id == null?true:false;
            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var location_id_search = Request.Form.GetValues("columns[0][search][value]")[0];
                var user_id = Request.Form.GetValues("columns[1][search][value]")[0];
                var from_date = Request.Form.GetValues("columns[2][search][value]")[0];
                var to_date = Request.Form.GetValues("columns[3][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var companyRentAssetData = (from rentOrder in db.rentOrders
                                            join loc in db.locations on rentOrder.location_id equals loc.id
                                            //join comAssetRent in db.companyAssetsRent on asset.id equals comAssetRent.asset_id
                                            //group loc by comAssetRent.location_id into locID
                                            select new RentOrderViewModel
                                            {
                                                id = rentOrder.id,
                                                assets_count = rentOrder.assetes_count,
                                                location_name = loc.name,
                                                location_type = loc.type,
                                                notes = rentOrder.notes,
                                                start_date = rentOrder.start_date,
                                                due_date = rentOrder.due_date,
                                                created_by = rentOrder.created_by,
                                                location_id = rentOrder.location_id,
                                                created_at = rentOrder.created_at,
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

                if (user.location_id != null)
                {
                    RiplTrackingSystem.Models.Location userLocation = db.locations.Find(user.location_id);
                    if (userLocation.type == (int)(LocationType.Company))
                        companyRentAssetData = companyRentAssetData.Where(m => m.location_id == userLocation.id);
                    else
                        companyRentAssetData = companyRentAssetData.Where(m => m.location_id == -1);
                }
                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    companyRentAssetData = companyRentAssetData.Where(m => m.assets_count.ToString().ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                     m.location_name.ToLower().Contains(searchValue.ToLower()));
                }
                
                if (!string.IsNullOrEmpty(location_id_search))
                {
                    int locID = int.Parse(location_id_search);
                    companyRentAssetData = companyRentAssetData.Where(s => s.location_id == locID);
                }

                if (!string.IsNullOrEmpty(user_id))
                {
                    int userID = int.Parse(user_id);
                    companyRentAssetData = companyRentAssetData.Where(s => s.created_by == userID);
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
            ViewBag.assets = availabeAssets.Select(s => new { s.id, s.tag_id }).ToList();

            ViewBag.users = db.users.Where(l => l.location_id == null).Select(s => new { s.id, s.full_name }).ToList();
            ViewBag.locations = db.locations.Where(l => l.parent_id == null).Select(s => new { s.id, s.name }).ToList();

            return View();
        }

        [HttpPost]
        public JsonResult saveSendAsset(RentOrderViewModel rentOrderVM)
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("edit_ripl_order"))
            {
                if(rentOrderVM.id == 0)
                    Helpers.Helpers.SystemLogger(currentUser.id, "Add Order", String.Empty, JsonConvert.SerializeObject(rentOrderVM), "Trining to Add Order but has no permission");
                else
                    Helpers.Helpers.SystemLogger(currentUser.id, "Edit Order", JsonConvert.SerializeObject(db.rentOrders.Find(rentOrderVM.id)), JsonConvert.SerializeObject(rentOrderVM), "Trining to Add Order but has no permission");

                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }

            if (rentOrderVM.id == 0)
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Add Order", String.Empty, JsonConvert.SerializeObject(rentOrderVM), "Successfull Add Order");

                RentOrder rentOrder = AutoMapper.Mapper.Map<RentOrderViewModel, RentOrder>(rentOrderVM);
                rentOrder.assetes_count = rentOrderVM.assets_ids.Count();
                rentOrder.created_at = DateTime.Now;
                rentOrder.updated_at = DateTime.Now;
                //user.created_by = Session["id"].ToString().ToInt();
                //user.type = (int)UserTypes.Staff;

                db.rentOrders.Add(rentOrder);
                db.SaveChanges();

                foreach (int assetID in rentOrderVM.assets_ids)
                {
                    CompanyAssetRent companyAssetRent = new CompanyAssetRent();
                    companyAssetRent.rent_order_id = rentOrder.id;
                    companyAssetRent.asset_id = assetID;
                    companyAssetRent.status = (int)AssetStatus.Received;
                    companyAssetRent.to_location = rentOrderVM.location_id;
                    companyAssetRent.start_date = rentOrder.start_date;
                    companyAssetRent.due_date = rentOrder.due_date;
                    companyAssetRent.created_at = DateTime.Now;
                    companyAssetRent.updated_at = DateTime.Now;
                    db.companyAssetsRent.Add(companyAssetRent);
                    db.SaveChanges();

                }

            }
            else
            {

                RentOrder oldRentOrder = db.rentOrders.Find(rentOrderVM.id);
                Helpers.Helpers.SystemLogger(currentUser.id, "Edit Order", JsonConvert.SerializeObject(oldRentOrder), JsonConvert.SerializeObject(rentOrderVM), "Successfull Edit Order");

                oldRentOrder.assetes_count = rentOrderVM.assets_ids.Count();
                oldRentOrder.start_date = rentOrderVM.start_date;
                oldRentOrder.due_date = rentOrderVM.due_date;
                oldRentOrder.updated_at = DateTime.Now;
                db.Entry(oldRentOrder).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                db.companyAssetsRent.Where(car => car.rent_order_id == oldRentOrder.id).ToList().ForEach(car => db.companyAssetsRent.Remove(car));
                db.SaveChanges();

                foreach (int assetID in rentOrderVM.assets_ids)
                {
                    CompanyAssetRent companyAssetRent = new CompanyAssetRent();
                    companyAssetRent.rent_order_id = oldRentOrder.id;
                    companyAssetRent.asset_id = assetID;
                    companyAssetRent.status = (int)AssetStatus.WatingForReceive;
                    companyAssetRent.start_date = oldRentOrder.start_date;
                    companyAssetRent.due_date = oldRentOrder.due_date;
                    companyAssetRent.created_at = DateTime.Now;
                    companyAssetRent.updated_at = DateTime.Now;
                    db.companyAssetsRent.Add(companyAssetRent);
                    db.SaveChanges();

                }
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }
        public void OrdersReport()
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Orders Report", String.Empty,String.Empty, "Successfull Export Orders Report");

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Orders");
            Sheet.Cells["A1"].Value = "ID";
            Sheet.Cells["B1"].Value = "Location";
            Sheet.Cells["C1"].Value = "Assets Count";
            Sheet.Cells["D1"].Value = "Created By";
            Sheet.Cells["E1"].Value = "Start Date";
            Sheet.Cells["F1"].Value = "End Date";
            Sheet.Cells["G1"].Value = "Created at";
            List< RentOrderViewModel> orders = (from rentOrder in db.rentOrders
                                                join user in db.users on rentOrder.created_by equals user.id
                                                join loc in db.locations on rentOrder.location_id equals loc.id
                                        //join comAssetRent in db.companyAssetsRent on asset.id equals comAssetRent.asset_id
                                        //group loc by comAssetRent.location_id into locID
                                        select new RentOrderViewModel
                                        {
                                            id = rentOrder.id,
                                            assets_count = rentOrder.assetes_count,
                                            location_name = loc.name,
                                            location_type = loc.type,
                                            notes = rentOrder.notes,
                                            stringCreatedBy = user.full_name,
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

                                        }).Where(loc => loc.location_type == (int)(LocationType.Company)).ToList();

            int row = 2;
            foreach (var item in orders)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.location_name;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.assets_count;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.stringCreatedBy;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.start_date.ToString();
                Sheet.Cells[string.Format("F{0}", row)].Value = item.due_date.ToString();
                Sheet.Cells[string.Format("G{0}", row)].Value = item.created_at.ToString();
                row++;
            }
            Sheet.Cells[string.Format("A{0}", row)].Value = "Total";
            Sheet.Cells[string.Format("B{0}", row)].Value = orders.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        public void OrdersWithAssetsReport()
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Orders With Assets Report", String.Empty, String.Empty, "Successfull Export Orders With Assets Report");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Orders With Assets");
            Sheet.Cells["A1"].Value = "ID";
            Sheet.Cells["B1"].Value = "Location";
            Sheet.Cells["C1"].Value = "Assets Count";
            Sheet.Cells["D1"].Value = "Assets";
            Sheet.Cells["E1"].Value = "Created By";
            Sheet.Cells["F1"].Value = "Start Date";
            Sheet.Cells["G1"].Value = "End Date";
            Sheet.Cells["H1"].Value = "Created at";
            List<RentOrderViewModel> orders = (from rentOrder in db.rentOrders
                                               join user in db.users on rentOrder.created_by equals user.id
                                               join loc in db.locations on rentOrder.location_id equals loc.id
                                               //join comAssetRent in db.companyAssetsRent on asset.id equals comAssetRent.asset_id
                                               //group loc by comAssetRent.location_id into locID
                                               select new RentOrderViewModel
                                               {
                                                   id = rentOrder.id,
                                                   assets_count = rentOrder.assetes_count,
                                                   
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

                                               }).Where(loc => loc.location_type == (int)(LocationType.Company)).ToList();

            int row = 2;
            foreach (var item in orders)
            {
                string orderAssets = String.Empty;
                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.location_name;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.assets_count;
                if(item.assetsStatusAndLocation != null)
                {
                    foreach(var asset in item.assetsStatusAndLocation)
                    {
                        orderAssets +="("+ asset.tag_id+"-"+(AssetStatus)asset.status + "),";
                    }
                    orderAssets = orderAssets.Remove(orderAssets.Length -1);
                }
                Sheet.Cells[string.Format("D{0}", row)].Value = orderAssets;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.stringCreatedBy;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.start_date.ToString();
                Sheet.Cells[string.Format("G{0}", row)].Value = item.due_date.ToString();
                Sheet.Cells[string.Format("H{0}", row)].Value = item.created_at.ToString();
                row++;
            }
            Sheet.Cells[string.Format("A{0}", row)].Value = "Total";
            Sheet.Cells[string.Format("B{0}", row)].Value = orders.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

    }
}