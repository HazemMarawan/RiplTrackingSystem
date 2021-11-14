using RiplTrackingSystem.Auth;
using RiplTrackingSystem.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RiplTrackingSystem.Helpers;
using RiplTrackingSystem.Enums;
namespace RiplTrackingSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class HomeController : Controller
    {
        DBContext db = new DBContext();
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;
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
            ViewBag.allAssts = allAssts.Count();
            ViewBag.availabeAssets = availabeAssets.Count();
            ViewBag.availabeAssetsPercentage = Math.Ceiling(((availabeAssets.Count() / (double)allAssts.Count()) * 100)).ToString() + "%";
            ViewBag.usedAssetsPercentage = (100 - Math.Ceiling(((availabeAssets.Count() / (double)allAssts.Count()) * 100))).ToString() + "%";


            if (currentUser.location_id != null)
            {
                Location userLocation = db.locations.Find(currentUser.location_id);

                var locationAssets = (from cma in db.companyAssetsRent
                                      join location in db.locations on cma.to_location equals location.id

                                      select new
                                      {
                                          location.company_id,
                                          cma.to_location,
                                          cma.due_date
                                      });
                if (userLocation.type == (int)LocationType.Company)
                    locationAssets = locationAssets.Where(s => s.company_id == userLocation.company_id && DateTime.Now <= s.due_date);
                else
                    locationAssets = locationAssets.Where(s => s.to_location == currentUser.location_id && DateTime.Now <= s.due_date);

                ViewBag.locationAssets = locationAssets.ToList().Count();

                //int AvailabeNumberOfAssets = db.companyAssetsRent.Where(s => s.to_location == userLocation.id && s.due_date >= DateTime.Now).ToList().Count();

                int WaitingForRecieve = db.companyAssetsRent.Where(s => s.to_location == userLocation.id && s.due_date >= DateTime.Now && s.status == (int)AssetStatus.WatingForReceive).ToList().Count();
                int RecievedAssets = db.companyAssetsRent.Where(s => s.to_location == userLocation.id && s.due_date >= DateTime.Now && s.status == (int)AssetStatus.Received).ToList().Count();
                int LostAssets = db.companyAssetsRent.Where(s => s.to_location == userLocation.id && s.due_date >= DateTime.Now && s.status == (int)AssetStatus.Lost).ToList().Count();

                int TotalAssets = WaitingForRecieve + RecievedAssets + LostAssets;

                double WaitingForRecievePercentage = Math.Ceiling((WaitingForRecieve / (double)TotalAssets) * 100);

                double RecievedAssetsPercentage = Math.Ceiling((RecievedAssets / (double)TotalAssets) * 100);
                double LostAssetsPercentage = Math.Ceiling((LostAssets / (double)TotalAssets) * 100);

                ViewBag.WaitingForRecieve = WaitingForRecieve;
                ViewBag.WaitingForRecievePercentage = WaitingForRecievePercentage.ToString() + "%";

                ViewBag.RecievedAssets = RecievedAssets;
                ViewBag.RecievedAssetsPercentage = RecievedAssetsPercentage.ToString() + "%"; ;

                ViewBag.LostAssets = LostAssets;
                ViewBag.LostAssetsPercentage = LostAssetsPercentage.ToString() + "%"; ;

                ViewBag.TotalAssets = TotalAssets;
                ViewBag.LocationName = userLocation.name;
            }
            ViewBag.users = db.users.Count();
            ViewBag.assets = db.assets.Count();
            var availableRentedAssets = db.companyAssetsRent.Where(t => t.to_location == null).GroupBy(a => a.asset_id).Count();
            //var newAssets = ;
            return View();
        }

        public ActionResult usedAssetsByLocationsChart()
        {

            string cs = ConfigurationManager.ConnectionStrings["DBContextADO"].ConnectionString;

            SqlConnection sql = new SqlConnection(cs);
            sql.Open();

            SqlCommand comm = new SqlCommand("select location_id , sum(assetes_count) as assets_count from RentOrders where due_date >= getdate() group by location_id", sql);
            SqlDataReader reader = comm.ExecuteReader();
            List<int> locationsAssets = new List<int>();
            List<string> xAxis = new List<string>();

            while (reader.Read())
            {
                locationsAssets.Add(reader["assets_count"].ToString().ToInt());
                xAxis.Add(db.locations.Find(reader["location_id"].ToString().ToInt()).name);
            }

            reader.Close();

            sql.Close();


            return Json(new { locationsAssets = locationsAssets, xAxis = xAxis, message = "done" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult riplAssetsChart()
        {
            DateTime dateTime = DateTime.Now;
            var currentYear = dateTime.Year;
            var currentMonth = dateTime.Month;
            string cs = ConfigurationManager.ConnectionStrings["DBContextADO"].ConnectionString;

            SqlConnection sql = new SqlConnection(cs);
            sql.Open();

            SqlCommand comm = new SqlCommand("select YEAR(created_at) as years, MONTH(created_at) as months, sum(assetes_count) as assets_count, count(*) as orders_count from RentOrders group by YEAR(created_at) , MONTH(created_at)", sql);
            SqlDataReader reader = comm.ExecuteReader();
            List<int> orders_count = new List<int>();
            List<int> assets_count = new List<int>();
            List<string> xAxis = new List<string>();

            while (reader.Read())
            {
                assets_count.Add(reader["assets_count"].ToString().ToInt());
                orders_count.Add(reader["orders_count"].ToString().ToInt());
                xAxis.Add(reader["months"].ToString() + '/' + reader["years"].ToString());
            }

            reader.Close();

            sql.Close();

            return Json(new { orders_count = orders_count, assets_count = assets_count, xAxis = xAxis, message = "done" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult assetDistributionChart()
        {
            User user = Session["user"] as User;
            List<int> locationsAssets = new List<int>();
            List<string> xAxis = new List<string>();
            if(user.location_id!=null)
            { 
            string cs = ConfigurationManager.ConnectionStrings["DBContextADO"].ConnectionString;

            SqlConnection sql = new SqlConnection(cs);
            sql.Open();

            SqlCommand comm = new SqlCommand(@"select Locations.name,count(CompanyAssetRents.to_location) as assets_count from CompanyAssetRents inner join Locations on CompanyAssetRents.to_location = Locations.id where Locations.company_id = " + user.location_id.ToString() + " and CompanyAssetRents.due_date >= getdate() group by CompanyAssetRents.to_location, Locations.name", sql);
            SqlDataReader reader = comm.ExecuteReader();
            
            while (reader.Read())
            {
                locationsAssets.Add(reader["assets_count"].ToString().ToInt());
                xAxis.Add(reader["name"].ToString());
            }

            reader.Close();

            sql.Close();
            }

            return Json(new { locationsAssets = locationsAssets, xAxis = xAxis, message = "done" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult assetDistributionChartByStatus()
        {
            User user = Session["user"] as User;

            string cs = ConfigurationManager.ConnectionStrings["DBContextADO"].ConnectionString;

            SqlConnection sql = new SqlConnection(cs);
            sql.Open();
            List<int> waitingAssets = new List<int>();
            List<int> recievedAssets = new List<int>();
            List<int> lostAssets = new List<int>();
            List<string> xAxis = new List<string>();
            List<int> locationsIDs = new List<int>();
            if (user.location_id != null)
            {
                SqlCommand comm = new SqlCommand(@"select Locations.id,Locations.name from Locations where Locations.id in (select CompanyAssetRents.to_location from CompanyAssetRents) and Locations.company_id =" + user.location_id.ToString(), sql);
                SqlDataReader reader = comm.ExecuteReader();




                while (reader.Read())
                {
                    xAxis.Add(reader["name"].ToString());
                    locationsIDs.Add(reader["id"].ToString().ToInt());
                }

                foreach (int locID in locationsIDs)
                {
                    comm = new SqlCommand(@"select count(*) as waitingCount from CompanyAssetRents where CompanyAssetRents.to_location = " + locID.ToString() + " and due_date >= GETDATE() and CompanyAssetRents.status = " + ((int)AssetStatus.WatingForReceive).ToString(), sql);
                    reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        waitingAssets.Add(reader["waitingCount"].ToString().ToInt());
                    }

                    comm = new SqlCommand(@"select count(*) as recievedCount from CompanyAssetRents where CompanyAssetRents.to_location = " + locID.ToString() + " and due_date >= GETDATE() and CompanyAssetRents.status = " + ((int)AssetStatus.Received).ToString(), sql);
                    reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        recievedAssets.Add(reader["recievedCount"].ToString().ToInt());
                    }

                    comm = new SqlCommand(@"select count(*) as lostCount from CompanyAssetRents where CompanyAssetRents.to_location = " + locID.ToString() + " and due_date >= GETDATE() and CompanyAssetRents.status = " + ((int)AssetStatus.Lost).ToString(), sql);
                    reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        lostAssets.Add(reader["lostCount"].ToString().ToInt());
                    }


                }


                reader.Close();

                sql.Close();

            }
            return Json(new { waitingAssets = waitingAssets, recievedAssets = recievedAssets, lostAssets = lostAssets, xAxis = xAxis, message = "done" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult assetSendReciveChart()
        {
            User user = Session["user"] as User;

            string cs = ConfigurationManager.ConnectionStrings["DBContextADO"].ConnectionString;

            SqlConnection sql = new SqlConnection(cs);
            sql.Open();
            List<int> sendAssets = new List<int>();
            List<int> recivedAssets = new List<int>();
            List<int> lostAssets = new List<int>();
            List<int> locationsIDs = new List<int>();
            List<string> xAxis = new List<string>();
            
            SqlCommand comm = new SqlCommand(@"select RentOrders.location_id as id,Locations.name as name,sum(assetes_count) as count
                                                from RentOrders
                                                inner join Locations on RentOrders.location_id = Locations.id
                                                group by RentOrders.location_id,Locations.name", sql);
            SqlDataReader reader = comm.ExecuteReader();

            while (reader.Read())
            {
                xAxis.Add(reader["name"].ToString());
                sendAssets.Add(reader["count"].ToString().ToInt());
                locationsIDs.Add(reader["id"].ToString().ToInt());
            }
            bool hasValue = false;
            foreach (int locID in locationsIDs)
            {
                hasValue = false;
                comm = new SqlCommand(@"select Locations.company_id,count(*) as count
                                        from CompanyAssetRents 
                                        left join Locations on CompanyAssetRents.to_location = Locations.id
                                        where getdate() > CompanyAssetRents.due_date and company_id = "+ locID.ToString() + @" and CompanyAssetRents.status != 3
                                        group by Locations.company_id", sql);
                reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    recivedAssets.Add(reader["count"].ToString().ToInt());
                    hasValue = true;
                }

                if(hasValue == false)
                    recivedAssets.Add(0);

                hasValue = false;
                comm = new SqlCommand(@"select Locations.company_id,count(*) as count
                                        from CompanyAssetRents 
                                        left join Locations on CompanyAssetRents.to_location = Locations.id
                                        where company_id = " + locID.ToString() + @" and CompanyAssetRents.status = 3
                                        group by Locations.company_id", sql);
                reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    lostAssets.Add(reader["count"].ToString().ToInt());
                    hasValue = true;
                }

                if (hasValue == false)
                    lostAssets.Add(0);
            }


                reader.Close();
                sql.Close();

     
            return Json(new { lostAssets= lostAssets, recivedAssets = recivedAssets, sendAssets = sendAssets,xAxis = xAxis, message = "done" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}