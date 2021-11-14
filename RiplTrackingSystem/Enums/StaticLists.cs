using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Enums
{
    public enum Gender
    {
        Male = 1,
        Female = 2,
    }

    public enum LocationType
    {
        Company = 1,
        Factory = 2,
        Store = 3,
        Distributor = 4
    }
    public enum AssetStatus
    {
        WatingForReceive = 1,
        Received = 2,
        Lost = 3
    }
    public enum TransactionStatus
    {
        WatingForReceive = 1,
        Received = 2,
    }

    public enum UserTaskStatus
    {
        WatingForAction = 1,
        Follow = 2,
        Done = 3
    }
    public enum LogAtion
    {
        Insert = 1,
        Update = 2,
        Delete = 3,
        Retrieve = 4,
    }

}