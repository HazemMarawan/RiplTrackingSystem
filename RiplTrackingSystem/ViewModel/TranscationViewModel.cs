using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class TranscationViewModel
    {
        public int id { get; set; }
        public int assetes_count { get; set; }
        public int status { get; set; }
        public int created_by { get; set; }
        public string stringUser { get; set; }
        public string stringRecievedBy { get; set; }
        public string notes { get; set; }
        public int? from_location { get; set; }
        public int? to_location { get; set; }
        public int received_assets { get; set; }
        public string stringFromLocation { get; set; }
        public string stringToLocation { get; set; }
        public string stringCreated_at { get; set; }
        public string stringWorking_date { get; set; }
        public List<AssetViewModel> assetsStatusAndLocation { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? working_date { get; set; }
        public List<TransactionFileViewModel> attachments { get; set; }
    }
}