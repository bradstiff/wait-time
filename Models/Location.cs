﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaitTime.Models
{
    public class Location
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Accuracy { get; set; }
        public float Altitude { get; set; }
        public float? AltitudeAccuracy { get; set; }
        public float Bearing { get; set; }
        public float? BearingAccuracy { get; set; }
        public float Speed { get; set; }
        public float? SpeedAccuracy { get; set; }
        public float Timestamp { get; set; }
    }
}
