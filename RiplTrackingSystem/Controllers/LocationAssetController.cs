using RiplTrackingSystem.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RiplTrackingSystem.Models;
using RiplTrackingSystem.ViewModel;
using RiplTrackingSystem.Enums;
using RiplTrackingSystem.Helpers;
using Newtonsoft.Json;
using System.IO;

namespace RiplTrackingSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class LocationAssetController : Controller
    {
        DBContext db = new DBContext();
        // GET: LocationAsset
        public ActionResult recieve()
        {
            if (!can.hasPermission("receive_assets"))
                return RedirectToAction("Error404", "Error");

            User currentUser = Session["user"] as User;
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

                List<int?> transactionsIDs = db.companyAssetsRent.Where(c => c.transaction_id != null).Select(cmp => cmp.transaction_id).ToList();
                // Getting all data    
                var transactions = (from transaction in db.transcactions
                                    join user in db.users on transaction.created_by equals user.id
                                    join from_loc in db.locations on transaction.from_location equals from_loc.id
                                    join to_loc in db.locations on transaction.to_location equals to_loc.id

                                    select new TranscationViewModel
                                    {
                                        id = transaction.id,
                                        assetes_count = transaction.assetes_count,
                                        to_location = transaction.to_location,
                                        stringUser = user.full_name,
                                        notes = transaction.notes,
                                        stringFromLocation = from_loc.name,
                                        stringToLocation = to_loc.name,
                                        created_at = transaction.created_at,
                                        stringCreated_at = transaction.created_at.ToString(),
                                        status = transaction.status,
                                        assetsStatusAndLocation = (from cmp in db.companyAssetsRent
                                                                   join asset in db.assets on cmp.asset_id equals asset.id
                                                                   select new AssetViewModel
                                                                   {
                                                                       id = asset.id,
                                                                       tag_id = asset.tag_id,
                                                                       status = cmp.status,
                                                                       transcation_id = cmp.transaction_id

                                                                   }).Where(s => s.transcation_id == transaction.id).ToList(),
                                        received_assets = db.companyAssetsRent.Where(s => s.transaction_id == transaction.id && s.status == (int)AssetStatus.Received).ToList().Count
                                    }).Where(t => t.to_location == currentUser.location_id && t.status == (int)AssetStatus.WatingForReceive).Where(s => transactionsIDs.Contains(s.id));

                //total number of rows count     
                var displayResult = transactions.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = transactions.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            return View("recieve");
        }

        public ActionResult send()
        {
            User currentUser = Session["user"] as User;
            if (!can.hasPermission("send_assets"))
            {
                Helpers.Helpers.SystemLogger(currentUser.id, "Send Assets", String.Empty, String.Empty, "Tring to access Send Assets but has no permission");
                return RedirectToAction("Error404", "Error");
            }
            User user = Session["user"] as User;
            List<AssetViewModel> availableAssets = new List<AssetViewModel>();

            ViewBag.canSendPluck = false;
            if (user.location_id != null)
            {
                int? userLocationID = db.locations.Find(user.location_id).company_id;
                if (userLocationID > 0)
                {
                    Location currentUserCompany = db.locations.Find(userLocationID);
                    if (currentUserCompany.can_send_pluck == 1)
                    {
                        ViewBag.canSendPluck = true;
                    }
                }
            }
            else
            {
                ViewBag.canSendPluck = true;
            }

            if (user.location_id != null)
            {
                availableAssets = (from cmp in db.companyAssetsRent
                                   join asset in db.assets on cmp.asset_id equals asset.id
                                   select new AssetViewModel
                                   {
                                       id = cmp.id,
                                       tag_id = asset.tag_id,
                                       type = asset.type,
                                       transcation_id = cmp.transaction_id,
                                       tall = asset.tall,
                                       status = cmp.status,
                                       to_location = cmp.to_location,
                                       due_date = cmp.due_date

                                   }).Where(t => t.to_location == user.location_id && t.status == (int)AssetStatus.Received && DateTime.Now <= t.due_date).ToList();
            }
            else
            {
                List<Asset> allAssts = db.assets.ToList();

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
                    {
                        AssetViewModel assetViewModel = new AssetViewModel();
                        assetViewModel.id = asset.id;
                        assetViewModel.tag_id = asset.tag_id;
                        assetViewModel.type = asset.type;
                        assetViewModel.tall = asset.tall;

                        availableAssets.Add(assetViewModel);
                    }
                }
            }
            Location userLocation = db.locations.Find(user.location_id);
            if(user.location_id != null)
                ViewBag.locations = db.locations.Where(s => s.company_id == userLocation.company_id && s.id != user.location_id).Select(s => new { s.id, s.name }).ToList();
            else
                ViewBag.locations = db.locations.Where(s => s.type == (int)LocationType.Company).Select(s => new { s.id, s.name }).ToList();

            ViewBag.isRIPL = user.location_id == null ? true : false;
            return View(availableAssets);

        }
        public JsonResult checkRecieveAsset(string tag_id)
        {
            User user = Session["user"] as User;
            Asset asset = db.assets.Where(s => s.tag_id == tag_id).FirstOrDefault();
            if (asset != null)
            {
                CompanyAssetRent companyAssetRent = db.companyAssetsRent.Where(s => s.asset_id == asset.id && s.to_location == user.location_id && s.status == (int)AssetStatus.WatingForReceive && DateTime.Now <= s.due_date).FirstOrDefault();

                if (companyAssetRent != null)
                {
                    return Json(new { asset = asset, companyAssetRent = companyAssetRent, status = "success" }, JsonRequestBehavior.AllowGet);

                }
            }
            return Json(new { status = "error" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult checkSendAsset(string tag_id)
        {
            User user = Session["user"] as User;
            if (user.location_id != null)
            {
                Asset asset = db.assets.Where(s => s.tag_id == tag_id).FirstOrDefault();
                if (asset != null)
                {
                    CompanyAssetRent companyAssetRent = db.companyAssetsRent.Where(s => s.asset_id == asset.id && s.to_location == user.location_id && s.status == (int)AssetStatus.Received && DateTime.Now <= s.due_date).FirstOrDefault();

                    if (companyAssetRent != null)
                    {
                        return Json(new { asset = asset, companyAssetRent = companyAssetRent, status = "success" }, JsonRequestBehavior.AllowGet);

                    }
                }
            }
            else
            {
                return Json(new { status = "success" }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { status = "error" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult sendAssets(SendAssetViewModel sendAssetViewModel)
        {
            User user = Session["user"] as User;


            string[] companyAssetRents = sendAssetViewModel.assets.Split(',');

            if (user.location_id != null)
            {
                Transcaction transcaction = new Transcaction();
                transcaction.assetes_count = companyAssetRents.Length;
                transcaction.created_by = user.id;
                transcaction.notes = sendAssetViewModel.notes;
                transcaction.from_location = user.location_id;
                transcaction.to_location = sendAssetViewModel.location_id;
                transcaction.created_at = DateTime.Now;
                transcaction.updated_at = DateTime.Now;
                transcaction.status = (int)AssetStatus.WatingForReceive;
                transcaction.working_date = sendAssetViewModel.working_date;
                db.transcactions.Add(transcaction);
                db.SaveChanges();
                
                Helpers.Helpers.SystemLogger(user.id, "Send Assets", String.Empty, JsonConvert.SerializeObject(transcaction), "Successfull Send Assets");

                if (sendAssetViewModel.attachments != null)
                {
                    foreach(HttpPostedFileBase file in sendAssetViewModel.attachments)
                    {
                        Guid guid = Guid.NewGuid();
                        var InputFileName = Path.GetFileName(file.FileName);
                        var ServerSavePath = Path.Combine(Server.MapPath("~/Files/Transcations/") + guid.ToString() + "_TF" + Path.GetExtension(file.FileName));
                        file.SaveAs(ServerSavePath);

                        TransactionFile transactionFile = new TransactionFile();
                        transactionFile.path = "/Files/Transcations/" + guid.ToString() + "_TF" + Path.GetExtension(file.FileName);
                        transactionFile.transcaction_id = transcaction.id;

                        db.transactionFiles.Add(transactionFile);
                        db.SaveChanges();
                    }
                }

                CompanyAssetRent companyAssetRent = new CompanyAssetRent();
                foreach (string s in companyAssetRents)
                {
                    companyAssetRent = db.companyAssetsRent.Find(int.Parse(s));
                    CompanyAssetRentHistory companyAssetRentHistory = AutoMapper.Mapper.Map<CompanyAssetRent, CompanyAssetRentHistory>(companyAssetRent);
                    companyAssetRentHistory.id = 0;

                    db.companyAssetRentHistories.Add(companyAssetRentHistory);
                    db.SaveChanges();

                    companyAssetRent.to_location = sendAssetViewModel.location_id;
                    companyAssetRent.from_location = user.location_id;
                    companyAssetRent.status = (int)AssetStatus.WatingForReceive;
                    companyAssetRent.transaction_id = transcaction.id;
                    companyAssetRent.working_date = sendAssetViewModel.working_date;
                    db.Entry(companyAssetRent).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                }

                RentOrder rentOrder = db.rentOrders.Where(s => s.id == companyAssetRent.rent_order_id).FirstOrDefault();
                rentOrder.notes = sendAssetViewModel.notes;

                db.Entry(rentOrder).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                RentOrder rentOrder = new RentOrder();
                rentOrder.assetes_count = companyAssetRents.Count();
                rentOrder.created_at = DateTime.Now;
                rentOrder.updated_at = DateTime.Now;
                rentOrder.created_by = user.id;
                rentOrder.location_id = sendAssetViewModel.location_id;
                rentOrder.due_date = sendAssetViewModel.due_date;
                //user.created_by = Session["id"].ToString().ToInt();
                //user.type = (int)UserTypes.Staff;

                db.rentOrders.Add(rentOrder);
                db.SaveChanges();

                Helpers.Helpers.SystemLogger(user.id, "Send Assets", String.Empty, JsonConvert.SerializeObject(rentOrder), "Successfull Send Assets");

                foreach (string assetID in companyAssetRents)
                {
                    CompanyAssetRent companyAssetRent = new CompanyAssetRent();
                    companyAssetRent.rent_order_id = rentOrder.id;
                    companyAssetRent.asset_id = assetID.ToInt();
                    companyAssetRent.status = (int)AssetStatus.Received;
                    companyAssetRent.to_location = sendAssetViewModel.location_id;
                    companyAssetRent.start_date = rentOrder.start_date;
                    companyAssetRent.due_date = rentOrder.due_date;
                    companyAssetRent.created_at = DateTime.Now;
                    companyAssetRent.updated_at = DateTime.Now;
                    db.companyAssetsRent.Add(companyAssetRent);
                    db.SaveChanges();

                }
            }
            return Json(new { status = "done" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult recieveAssets(SendAssetViewModel sendAssetViewModel)
        {
            User user = Session["user"] as User;
            Helpers.Helpers.SystemLogger(user.id, "Recieve Assets", String.Empty, JsonConvert.SerializeObject(sendAssetViewModel), "Successfull Recieve Assets");

            string[] companyAssetRents = sendAssetViewModel.assets.Split(',');

            List<CompanyAssetRent> lostAssets = db.companyAssetsRent.Where(s => s.transaction_id == sendAssetViewModel.transaction_id).ToList();
            foreach (CompanyAssetRent cm in lostAssets)
            {
                cm.status = (int)AssetStatus.Lost;
                db.Entry(cm).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            CompanyAssetRent companyAssetRent = new CompanyAssetRent();
            foreach (string s in companyAssetRents)
            {
                companyAssetRent = db.companyAssetsRent.Find(int.Parse(s));
                if (companyAssetRent != null)
                {
                    companyAssetRent.status = (int)AssetStatus.Received;

                    db.Entry(companyAssetRent).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

            }

            Transcaction transcaction = db.transcactions.Find(sendAssetViewModel.transaction_id);
            transcaction.status = (int)AssetStatus.Received;
            transcaction.recieved_by = user.id;
            db.Entry(transcaction).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            RentOrder rentOrder = db.rentOrders.Where(s => s.id == companyAssetRent.rent_order_id).FirstOrDefault();
            rentOrder.notes = sendAssetViewModel.notes;

            db.Entry(rentOrder).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return Json(new { status = "done" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult openTransaction(int id)
        {
            List<AssetViewModel> transactionAssets = (from cmp in db.companyAssetsRent
                                                      join asset in db.assets on cmp.asset_id equals asset.id
                                                      join transcation in db.transcactions on cmp.transaction_id equals transcation.id
                                                      select new AssetViewModel
                                                      {
                                                          id = cmp.id,
                                                          tag_id = asset.tag_id,
                                                          type = asset.type,
                                                          transcation_id = cmp.transaction_id,
                                                          tall = asset.tall,
                                                          status = cmp.status,
                                                          transcaionStatus = transcation.status

                                                      }).Where(t => t.transcation_id == id).ToList();
            ViewBag.transcactionID = id;
            return View(transactionAssets);
        }
    }


}