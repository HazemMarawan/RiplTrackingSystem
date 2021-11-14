using RiplTrackingSystem.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RiplTrackingSystem.Models;
using RiplTrackingSystem.ViewModel;
using RiplTrackingSystem.Enums;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace RiplTrackingSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class TransactionController : Controller
    {
        DBContext db = new DBContext();
        // GET: Transaction
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;
            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var user_id = Request.Form.GetValues("columns[0][search][value]")[0];
                var from_location_id = Request.Form.GetValues("columns[1][search][value]")[0];
                var to_location_id = Request.Form.GetValues("columns[2][search][value]")[0];
                var from_date = Request.Form.GetValues("columns[3][search][value]")[0];
                var to_date = Request.Form.GetValues("columns[4][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var transactions = (from transaction in db.transcactions
                                            join user in db.users on transaction.created_by equals user.id
                                            join from_location in db.locations on transaction.from_location equals from_location.id
                                            join to_location in db.locations on transaction.to_location equals to_location.id
                                            join recieveUser in db.users on transaction.recieved_by equals recieveUser.id into recievedByUser
                                            from recievedByUserData in recievedByUser.DefaultIfEmpty()

                                            select new TranscationViewModel
                                            {
                                                id = transaction.id,
                                                from_location = transaction.from_location,
                                                stringFromLocation = from_location.name,
                                                stringToLocation = to_location.name,
                                                to_location = transaction.to_location,
                                                created_by = transaction.created_by,
                                                created_at = transaction.created_at,
                                                working_date = transaction.working_date,
                                                stringWorking_date = transaction.working_date.ToString(),
                                                status = transaction.status,
                                                notes = transaction.notes,
                                                stringUser = user.full_name,
                                                stringRecievedBy = (recievedByUserData != null) ? recievedByUserData.full_name: null,
                                                assetes_count = transaction.assetes_count,
                                                stringCreated_at= transaction.created_at.ToString(),
                                                attachments = db.transactionFiles.Where(t=>t.transcaction_id == transaction.id).Select(t=>new TransactionFileViewModel { 
                                                    path = t.path
                                                }).ToList()

                                            });

                if(currentUser.location_id != null)
                {
                    transactions = transactions.Where(t => t.from_location == currentUser.location_id || t.to_location == currentUser.location_id);
                }
                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    transactions = transactions.Where(m => m.stringFromLocation.ToString().ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                     m.stringToLocation.ToLower().Contains(searchValue.ToLower()) || m.stringToLocation.ToLower().Contains(searchValue.ToLower()));
                }

                if (!string.IsNullOrEmpty(user_id))
                {
                    int fUId = int.Parse(user_id);
                    transactions = transactions.Where(s => s.created_by == fUId);
                }

                if (!string.IsNullOrEmpty(from_location_id))
                {
                    int FLocID = int.Parse(from_location_id);
                    transactions = transactions.Where(s => s.from_location == FLocID);
                }

                if (!string.IsNullOrEmpty(to_location_id))
                {
                    int TLocID = int.Parse(to_location_id);
                    transactions = transactions.Where(s => s.to_location == TLocID);
                }

                if (!string.IsNullOrEmpty(from_date))
                {
                    if (Convert.ToDateTime(from_date) != DateTime.MinValue)
                    {
                        DateTime from = Convert.ToDateTime(from_date);
                        transactions = transactions.Where(s => s.created_at >= from);
                    }
                }

                if (!string.IsNullOrEmpty(to_date))
                {
                    if (Convert.ToDateTime(to_date) != DateTime.MinValue)
                    {
                        DateTime to = Convert.ToDateTime(to_date);
                        transactions = transactions.Where(s => s.created_at <= to);
                    }
                }

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

            ViewBag.locations = db.locations.Select(s => new { s.id, s.name }).ToList();
            ViewBag.users = db.users.Select(s => new { s.id, s.full_name }).ToList();

            return View();
        }

        public void ExportTransactionsSheet()
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Export Transactions Sheet", String.Empty, String.Empty , "Successfull Export Transactions Sheet");

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Transcation");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells["A1:F1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:F1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:F1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "Id";
            Sheet.Cells["B1"].Value = "From";
            Sheet.Cells["C1"].Value = "To";
            Sheet.Cells["D1"].Value = "Number Of Assets";
            Sheet.Cells["E1"].Value = "Notes";
            Sheet.Cells["F1"].Value = "Created At";

            List<TranscationViewModel> transactions = (from transaction in db.transcactions
                                join user in db.users on transaction.created_by equals user.id
                                join from_location in db.locations on transaction.from_location equals from_location.id
                                join to_location in db.locations on transaction.to_location equals to_location.id
                                join recieveUser in db.users on transaction.recieved_by equals recieveUser.id into recievedByUser
                                from recievedByUserData in recievedByUser.DefaultIfEmpty()

                                select new TranscationViewModel
                                {
                                    id = transaction.id,
                                    from_location = transaction.from_location,
                                    stringFromLocation = from_location.name,
                                    stringToLocation = to_location.name,
                                    to_location = transaction.to_location,
                                    status = transaction.status,
                                    notes = transaction.notes,
                                    stringUser = user.full_name,
                                    stringRecievedBy = (recievedByUserData != null) ? recievedByUserData.full_name : null,
                                    assetes_count = transaction.assetes_count,
                                    stringCreated_at = transaction.created_at.ToString()

                                }).Where(t => t.from_location == currentUser.location_id || t.to_location == currentUser.location_id).ToList();


            int row = 2;
            foreach (var item in transactions)
            {
                string locaitonFactories = String.Empty;
                string locaitonStores = String.Empty;
                string locaitonDistributors = String.Empty;

                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.stringFromLocation+" By: "+item.stringUser;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.stringToLocation + " By: " + item.stringRecievedBy;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.assetes_count;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.notes;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.stringCreated_at;


                row++;
            }

            row++;
            colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
            text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Font.Color.SetColor(text);

            Sheet.Cells[string.Format("A{0}", row)].Value = "Total";
            Sheet.Cells[string.Format("B{0}", row)].Value = transactions.Count();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
    }
}