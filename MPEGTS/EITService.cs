using LoggerService;
using MPEGTS;
using System;
using System.Collections.Generic;
using System.Text;

namespace MPEGTS
{

    public class EITService
    {
        private ILoggingService _log;

        public EITService(ILoggingService loggingService)
        {
            _log = loggingService;
        }

        /// <summary>
        /// Scanning actual and scheduled events for actual TS
        /// </summary>
        /// <param name="packets"></param>
        public EITScanResult Scan(List<MPEGTransportStreamPacket> packets)
        {
            _log.Debug($"Scanning EIT from packets");

            var res = new EITScanResult()
            {
                OK = true
            };

            try
            {
                var programNumberToMapPID = new Dictionary<int, int>();

                var psiTable = DVBTTable.CreateFromPackets<PSITable>(packets, 0);
                if (psiTable != null && psiTable.ProgramAssociations != null)
                {
                    foreach (var kvp in psiTable.ProgramAssociations)
                    {
                        _log.Debug($"Associate  program number {kvp.ProgramNumber} to PID {kvp.ProgramMapPID}");
                        programNumberToMapPID[kvp.ProgramNumber] = kvp.ProgramMapPID;
                    }

                    var eitData = MPEGTransportStreamPacket.GetAllPacketsPayloadBytesByPID(packets, 18);

                    _log.Debug($"EIT packets count: {eitData.Count}");

                   // var eventIDs = new Dictionary<int, Dictionary<int, EventItem>>(); // ServiceID -> (event id -> event item )

                    var currentEventsCountFound = 0;
                    var scheduledEventsCountFound = 0;

                    foreach (var kvp in eitData)
                    {
                        try
                        {
                            var eit = DVBTTable.Create<EITTable>(kvp.Value);

                            if (eit == null || !eit.CRCIsValid())
                                continue;

                            if (eit.ID == 78) // actual TS, present/following event information = table_id = 0x4E;
                            {
                                foreach (var item in eit.EventItems)
                                {
                                    if (item.StartTime < DateTime.Now &&
                                        item.FinishTime > DateTime.Now)
                                    {
                                        // reading only the actual event
                                        // there can be event that start in future!

                                        res.CurrentEvents[programNumberToMapPID[eit.ServiceId]] = item;

                                        currentEventsCountFound++;

                                        break;
                                    }
                                }
                            }
                            else
                            if (eit.ID >= 80 && eit.ID <= 95) // actual TS, event schedule information = table_id = 0x50 to 0x5F;
                            {
                                foreach (var item in eit.EventItems)
                                {
                                    if (!programNumberToMapPID.ContainsKey(eit.ServiceId))
                                    {
                                        continue; // unknown channel?
                                    }

                                    var programMapPID = programNumberToMapPID[eit.ServiceId];

                                    if (!res.ScheduledEvents.ContainsKey(programMapPID))
                                    {
                                        res.ScheduledEvents[programMapPID] = new List<EventItem>();
                                    }

                                    foreach (var addedItem in res.ScheduledEvents[programMapPID])
                                    {
                                        if (addedItem.EventId == addedItem.EventId)
                                        {
                                            continue; // already added
                                        }
                                    }

                                    res.ScheduledEvents[programMapPID].Add(item);

                                    scheduledEventsCountFound++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Bad data EIT data
                        }
                    }

                    _log.Debug($"Scheduled Events found: {scheduledEventsCountFound}");
                    _log.Debug($"Current Events found: {currentEventsCountFound}");

                    // sorting:

                    foreach (var kvp in res.ScheduledEvents)
                    {
                        res.ScheduledEvents[kvp.Key].Sort();
                    }
                }
                else
                {
                    _log.Debug($"No PSI table found");
                    res.OK = false;
                }

            }
            catch (Exception e)
            {
                _log.Error(e);
                res.OK = false;
            }

            return res;
        }

    }
}
