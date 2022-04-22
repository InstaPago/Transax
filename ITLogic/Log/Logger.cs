using InstaTransfer.ITResources.General;
using System;
using System.Diagnostics;

namespace InstaTransfer.ITLogic.Log
{
    public class Logger
    {
        static string logName = GeneralResources.LogNameUmbrella;
        static string sourceName = GeneralResources.SourceNameUmbrella;
        

        private static EventLog log;
        public static EventLog ULog
        {
            get
            {
                if (log == null)
                {
                    if (!EventLog.SourceExists(sourceName))
                    {
                        EventLog.CreateEventSource(sourceName, logName);
                    }
                    else if (EventLog.LogNameFromSourceName(sourceName, ".") != logName)
                    {
                        EventLog.DeleteEventSource(sourceName);
                    }
                    log = new EventLog();
                    log.Source = sourceName;
                    log.Log = logName;
                }
                return log;
            }
            set { log = value; }
        }

        public static void WriteSuccessLog(string message, string source)
        {
                ULog.Source = source;
                ULog.WriteEntry(message, EventLogEntryType.Information);
        }

        public static void WriteErrorLog(Exception e, string source)
        {
                ULog.Source = source;
                ULog.WriteEntry(e.Message + e.StackTrace, EventLogEntryType.Error);
        }

        public static void WriteErrorLog(string message, string source)
        {
                ULog.Source = source;
                ULog.WriteEntry(message, EventLogEntryType.Error);
        }

        public static void WriteWarningLog(string message, string source)
        {
                ULog.Source = source;
                ULog.WriteEntry(message, EventLogEntryType.Warning);
        }
    }
}
