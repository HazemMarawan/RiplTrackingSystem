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
    public class TaskController : Controller
    {
        DBContext db = new DBContext();
        // GET: Task
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;

            UserTaskViewModel userTasks = new UserTaskViewModel();


            userTasks.allTasks = (from task in db.tasks
                                  join user_task in db.userTasks on task.id equals user_task.task_id
                                  join user in db.users on user_task.user_id equals user.id
                                  select new TaskViewModel
                                  {
                                      id = task.id,
                                      user_task_id = user_task.id,
                                      title = task.title,
                                      description = task.description,
                                      stringCreatedAt = task.created_at.ToString(),
                                      status = user_task.status,
                                      created_at = task.created_at,
                                      created_by = task.created_by,
                                      stringCreatedToBy = user.full_name,
                                     active = task.active
                                  }).Where(s => s.created_by == currentUser.id && s.active == 1).OrderByDescending(s => s.created_at).ToList();

            userTasks.myTasks = (from task in db.tasks
                             join user in db.users on task.created_by equals user.id
                             join user_task in db.userTasks on task.id equals user_task.task_id
                             select new TaskViewModel
                             {
                                 id = task.id,
                                 user_task_id = user_task.id,
                                 title = task.title,
                                 description = task.description,
                                 stringCreatedAt = task.created_at.ToString(),
                                 user_id = user_task.user_id,
                                 status = user_task.status,
                                 created_at = task.created_at,
                                 created_by = task.created_by,
                                 stringCreatedToBy = user.full_name,
                                 active = task.active
                             }).Where(s => s.user_id == currentUser.id && s.active == 1).OrderByDescending(s => s.created_at).ToList();
            
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
                ViewBag.users = companyUsers.Select(s => new { s.id, s.full_name }).ToList();
            }
            else
            {
                ViewBag.users = db.users.Where(s => s.location_id == null && s.id != currentUser.id).Select(s => new { s.id, s.full_name }).ToList();
            }
            
            return View(userTasks);
        }

        [HttpPost]
        public JsonResult saveTask(TaskViewModel taskVM)
        {
            User currentUser = Session["user"] as User;
            Task task = AutoMapper.Mapper.Map<TaskViewModel, Task>(taskVM);

            task.created_by = currentUser.id;

            task.updated_at = DateTime.Now;
            task.created_at = DateTime.Now;

            db.tasks.Add(task);
            db.SaveChanges();

            foreach (int user_id in taskVM.user_ids)
            {
                UserTask userTask = new UserTask();
                userTask.task_id = task.id;
                userTask.user_id = user_id;
                userTask.status = (int)UserTaskStatus.WatingForAction;

                db.userTasks.Add(userTask);
                db.SaveChanges();
            }
            Helpers.Helpers.SystemLogger(currentUser.id, "Add Task", String.Empty, JsonConvert.SerializeObject(taskVM), "Successfull Add Task");

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult followTask(int id)
        {
            UserTask userTask = db.userTasks.Where(s => s.id == id).FirstOrDefault();
            userTask.status = (int)UserTaskStatus.Follow;

            db.Entry(userTask).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Follow Task", String.Empty, JsonConvert.SerializeObject(db.tasks.Find(id)), "Successfull Follow Task");

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult doneTask(int id)
        {
            UserTask userTask = db.userTasks.Where(s => s.id == id).FirstOrDefault();
            userTask.status = (int)UserTaskStatus.Done;

            db.Entry(userTask).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Done Task", String.Empty, JsonConvert.SerializeObject(db.tasks.Find(id)), "Successfull Done Task");

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult deleteTask(int id)
        {
            Task task = db.tasks.Where(s => s.id == id).FirstOrDefault();
            //db.userTasks.Where(s => s.task_id == task.id).ToList().ForEach(s=>db.userTasks.Remove(s));
            //db.SaveChanges();

            task.active = 0;
            db.Entry(task).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            User currentUser = Session["user"] as User;
            Helpers.Helpers.SystemLogger(currentUser.id, "Delete Task", String.Empty, JsonConvert.SerializeObject(db.tasks.Find(id)), "Successfull Delete Task");

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}