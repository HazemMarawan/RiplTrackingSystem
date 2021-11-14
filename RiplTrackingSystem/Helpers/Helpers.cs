using RiplTrackingSystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RiplTrackingSystem.Models;
namespace RiplTrackingSystem.Helpers
{
    public static class Helpers
    {
        public static DBContext db = new DBContext();
        public static int ToInt(this string str)
        {
            return Convert.ToInt32(str);
        }
        public static int ToDate(this string str)
        {
            return Convert.ToInt32(str);
        }
        public static string getAssetStatus(int value)
        {
            var enumDisplayStatus = (AssetStatus)value;
            string stringValue = enumDisplayStatus.ToString();
            return stringValue;
        }

        public static void SystemLogger(int? user_id,string action,string old_data,string request_data,string notes)
        {
            Log log = new Log();
            log.user_id = user_id;
            log.action = action;
            log.old_data = old_data;
            log.request_data = request_data;
            log.notes = notes;
            log.created_at = DateTime.Now;

            db.logs.Add(log);
            db.SaveChanges();
        }
    }

}