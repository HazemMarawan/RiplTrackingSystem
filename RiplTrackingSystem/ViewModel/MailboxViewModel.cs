using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class MailboxViewModel
    {
        public List<EmailViewModel> inboxMails { get; set; }
        public List<EmailViewModel> sendMails { get; set; }
    }
}