﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class EmailAttachmentViewModel
    {
        public int id { get; set; }
        public string attachmentPath { get; set; }
        public int? email_id { get; set; }
    }
}