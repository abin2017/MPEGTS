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

        private int AddEventItem(Dictionary<int, List<EventItem>> scheduledEvents, EventItem item, int key)
        {
            if (!scheduledEvents.ContainsKey(key))
            {
                scheduledEvents[key] = new List<EventItem>();
            }

            foreach (var addedItem in scheduledEvents[key])
            {
                if (addedItem.EventId == item.EventId)
                {
                    return 0;
                }
            }

            scheduledEvents[key].Add(item);

            return 1;
        }

        /// <summary>
        /// Scanning events for actual TS
        /// </summary>
        /// <param name="packets"></param>
        public EITScanResult Scan(List<MPEGTransportStreamPacket> packets, bool loadOutdatedCurrentEvents = false)
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
                                    if (loadOutdatedCurrentEvents ||
                                        (item.StartTime < DateTime.Now &&
                                         item.FinishTime > DateTime.Now)
                                        )
                                    {
                                        res.CurrentEvents[programNumberToMapPID[eit.ServiceId]] = item;

                                        //if (!programNumberToMapPID.ContainsValue(eit.ServiceId))
                                        //{
                                        //    res.CurrentEvents[eit.ServiceId] = item;
                                        //}

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

                                    scheduledEventsCountFound += AddEventItem(res.ScheduledEvents, item, programMapPID);

                                    //// polish EPG has as key serviceId
                                    //if (!programNumberToMapPID.ContainsValue(eit.ServiceId))
                                    //{
                                    //    AddEventItem(res.ScheduledEvents, item, eit.ServiceId);
                                    //}

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Bad data EIT data

                            if (ex is NotSupportedException)
                            {
                                if (ex.Message.StartsWith("No data is available for encoding "))
                                {
                                    res.NotSupportedEncodingFound = true;
                                }
                            }
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

                if (e is NotSupportedException)
                {
                    if (e.Message.StartsWith("No data is available for encoding "))
                    {
                        res.NotSupportedEncodingFound = true;
                    }
                }
            }

            return res;
        }

    }
}
