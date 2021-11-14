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
    public class MailController : Controller
    {
        DBContext db = new DBContext();
        // GET: Mail
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;
            if (currentUser.location_id != null)
            {
                int? userCompanyId = db.locations.Find(currentUser.location_id).company_id;
                List<UserViewModel> companyUsers = (from user in db.users
                                                    join location in db.locations on user.location_id equals location.id
                                                    select new UserViewModel
                                                    {
                                                        id = user.id,
                                                        full_name = user.full_name,
                                                        company_id = location.company_id
                                                    }).Where(s => s.company_id == userCompanyId && s.id != currentUser.id).ToList();
                ViewBag.toUsers = companyUsers.Select(s => new { s.id, s.full_name }).ToList();
            }
            else
            {
                //List<UserViewModel> companyUsers = (from user in db.users
                //                                    join location in db.locations on user.location_id equals location.id
                //                                    select new UserViewModel
                //                                    {
                //                                        id = user.id,
                //                                        full_name = user.full_name,
                //                                        company_id = location.company_id,
                //                                        parent_id = location.parent_id
                //                                    }).Where(s => s.parent_id == null).ToList();
                //ViewBag.toUsers = companyUsers.Select(s => new { s.id, s.full_name }).ToList();
                ViewBag.toUsers = db.users.Where(s=>s.location_id == null && s.id != currentUser.id).Select(s => new { s.id, s.full_name }).ToList();
            }

            List<EmailViewModel> inboxMails = (from user in db.users
                                               join email in db.emails on user.id equals email.from_user
                                               select new EmailViewModel
                                               {
                                                   id = email.id,
                                                   subject = email.subject,
                                                   body = email.body,
                                                   stringCreatedAt = email.created_at.ToString(),
                                                   created_at = email.created_at,
                                                   stringFromUser = user.full_name,
                                                   to_user = email.to_user,
                                                   userImage = user.image,
                                                   emailAttachments = db.emailAttachments.Where(e=>e.email_id == email.id).Select(e=>new EmailAttachmentViewModel
                                                   {
                                                       attachmentPath = e.attachmentPath
                                                   }).ToList()
                                               }).Where(s => s.to_user == currentUser.id).OrderByDescending(s => s.created_at).ToList();

            List<EmailViewModel> sendMails = (from user in db.users
                                               join email in db.emails on user.id equals email.to_user
                                               select new EmailViewModel
                                               {
                                                   id = user.id,
                                                   subject = email.subject,
                                                   body = email.body,
                                                   stringCreatedAt = email.created_at.ToString(),
                                                   created_at = email.created_at,
                                                   stringToUser = user.full_name,
                                                   to_user = email.to_user,
                                                   userImage = user.image,
                                                   from_user = email.from_user,
                                                   emailAttachments = db.emailAttachments.Where(e => e.email_id == email.id).Select(e => new EmailAttachmentViewModel
                                                   {
                                                       attachmentPath = e.attachmentPath
                                                   }).ToList()
                                               }).Where(s => s.from_user == currentUser.id).OrderByDescending(s=>s.created_at).ToList();
            MailboxViewModel mailboxViewModel = new MailboxViewModel();
            mailboxViewModel.inboxMails = inboxMails;
            mailboxViewModel.sendMails = sendMails;

            ViewBag.currentUserName = currentUser.full_name;

            return View(mailboxViewModel);
        }

        [HttpPost]
        public JsonResult sendMail(EmailViewModel emailVM)
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Send Email", String.Empty, JsonConvert.SerializeObject(emailVM), "Successfull Send Email");

            Email email = AutoMapper.Mapper.Map<EmailViewModel, Email>(emailVM);

            email.from_user = currentUser.id;
            email.active = 1;
            email.updated_at = DateTime.Now;
            email.created_at = DateTime.Now;

            db.emails.Add(email);
            db.SaveChanges();

            if(emailVM.attachments.Count() != 0)
            { 
                foreach(var file in emailVM.attachments)
                {
                    Guid guid = Guid.NewGuid();
                    var InputFileName = Path.GetFileName(file.FileName);
                    var ServerSavePath = Path.Combine(Server.MapPath("~/Email/Attachments/") + guid.ToString() + "attachment" + Path.GetExtension(file.FileName));
                    file.SaveAs(ServerSavePath);

                    EmailAttachment emailAttachment = new EmailAttachment();
                    emailAttachment.attachmentPath = "/Email/Attachments/" + guid.ToString() + "attachment" + Path.GetExtension(file.FileName);
                    emailAttachment.email_id = email.id;

                    db.emailAttachments.Add(emailAttachment);
                    db.SaveChanges();
                }
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }
    }
}