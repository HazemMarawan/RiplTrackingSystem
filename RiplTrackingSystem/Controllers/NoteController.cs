using RiplTrackingSystem.Auth;
using RiplTrackingSystem.Models;
using RiplTrackingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RiplTrackingSystem.Enums;
using System.IO;
using System.Data;
using Newtonsoft.Json;

namespace RiplTrackingSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class NoteController : Controller
    {
        DBContext db = new DBContext();
        // GET: Note
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;
            List<NoteViewModel> notes = db.notes.Where(s => s.created_by == currentUser.id && s.active == 1).Select(s => new NoteViewModel
            {
                id = s.id,
                title = s.title,
                description = s.description,
                isFavourite = s.isFavourite,
                stringCreatedAt = s.created_at.ToString(),
                created_by = s.created_by,
                created_at = s.created_at
            }).OrderByDescending(s=>s.created_at).ToList();
            return View(notes);
        }

        public JsonResult saveNote(NoteViewModel noteVM)
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Add Note", String.Empty, JsonConvert.SerializeObject(noteVM), "Successfull Add Note");

            Note note = AutoMapper.Mapper.Map<NoteViewModel, Note>(noteVM);

            note.created_by = currentUser.id;
            note.active = 1;
            note.updated_at = DateTime.Now;
            note.created_at = DateTime.Now;

            db.notes.Add(note);
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult setFavouriteNote(int id)
        {
            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Add Note To Favourite", String.Empty, JsonConvert.SerializeObject(db.notes.Find(id)), "Successfull Add Note To Favourite");

            Note note = db.notes.Find(id);
            note.isFavourite = !note.isFavourite;
            db.Entry(note).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult deleteNote(int id)
        {
            Note note = db.notes.Find(id);
            //db.notes.Remove(note);
            note.active = 0;
            db.Entry(note).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}