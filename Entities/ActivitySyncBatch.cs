﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaitTime.Entities
{
    public class ActivitySyncBatch
    {
        public ActivitySyncBatch()
        {
            Locations = new HashSet<ActivitySyncBatchLocation>();
        }

        public Guid ActivitySyncBatchID { get; set; }
        public Guid ActivityID { get; set; }
        public int BatchNbr { get; set; }

        public Activity Activity { get; set; }
        public ICollection<ActivitySyncBatchLocation> Locations { get; set; }
    }
}
