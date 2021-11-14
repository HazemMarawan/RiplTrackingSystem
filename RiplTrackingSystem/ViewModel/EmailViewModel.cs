using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class EmailViewModel
    {
        public int id { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public int? from_user { get; set; }
        public int? to_user { get; set; }
        public string stringFromUser { get; set; }
        public string stringToUser { get; set; }
        public string stringCreatedAt { get; set; }
        public string userImage { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public List<HttpPostedFileBase> attachments { get; set; }
        public List<EmailAttachmentViewModel> emailAttachments { get; set; }

    }
}