﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaitTime.Models
{
    public class ActivitySyncRequestModel
    {
        public string Source { get; set; }
        public List<ActivitySyncBatchModel> Batches { get; set; }
    }

    public class ActivitySyncBatchModel
    {
        public Guid ActivitySyncBatchID { get; set; }
        public Guid ActivityID { get; set; }
        public int BatchNbr { get; set; }
        [JsonProperty("locations")]
        public float[,] LocationsArray { get; set; }
    }
}
