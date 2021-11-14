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
    [CustomAuthenticationFilter]
    public class ContactController : Controller
    {
        DBContext db = new DBContext();
        // GET: Contact
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Access Contacts", String.Empty, String.Empty, "Successfull Access Contacts");
            
            List<UserViewModel> usersContacts = new List<UserViewModel>();
            if(currentUser.location_id == null)
            {
                usersContacts = (from user in db.users
                                 join location in db.locations on user.location_id equals location.id into usPk
                                 from uPk in usPk.DefaultIfEmpty()
                                 select new UserViewModel
                                 {
                                     imagePath = user.image,
                                     full_name = user.full_name,
                                     email = user.email,
                                     stringLocation = uPk.name,
                                     phone1 = user.phone1,
                                     id = user.id
                                 }).Where(s => s.id != currentUser.id).ToList();
            }
            else
            {
                RiplTrackingSystem.Models.Location userLocation = db.locations.Find(currentUser.location_id);
                usersContacts = (from user in db.users
                                                    join location in db.locations on user.location_id equals location.id
                                                    select new UserViewModel
                                                    {
                                                        imagePath = user.image,
                                                        full_name = user.full_name,
                                                        email = user.email,
                                                        stringLocation = location.name,
                                                        phone1 = user.phone1,
                                                        company_id = location.company_id,
                                                        id = user.id
                                                    }).Where(s=>s.company_id == userLocation.company_id && s.id != currentUser.id).ToList();
            }
            return View(usersContacts);
        }
    }
}