using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class UserTaskViewModel
    {
        public List<TaskViewModel> allTasks { get; set; }
        public List<TaskViewModel> myTasks { get; set; }

    }
}