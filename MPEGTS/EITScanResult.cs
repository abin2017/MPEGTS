using System;
using System.Collections.Generic;
using System.Text;

namespace MPEGTS
{
    public class EITScanResult
    {
        public bool OK { get; set; }

        /// <summary>
        /// MapPID -> current event
        /// </summary>
        public Dictionary<int, EventItem> CurrentEvents { get; set; } = new Dictionary<int, EventItem>();

        /// <summary>
        /// MapPID -> List of events
        /// </summary>
        public Dictionary<int, List<EventItem>> ScheduledEvents { get; set; } = new Dictionary<int, List<EventItem>>();
    }
}
