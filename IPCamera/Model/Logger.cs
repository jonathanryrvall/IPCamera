using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace IPCamera.Model
{
    /// <summary>
    /// Logs stuff
    /// </summary>
    public class Logger
    {
        private ConcurrentQueue<LogEntry> entries;
        private Timer timer;


        /// <summary>
        /// A log entry
        /// </summary>
        public class LogEntry
        {
            public DateTime Timestamp;
            public string Message;

            public LogEntry(string message)
            {
                Message = message;
                Timestamp = DateTime.Now;
            }

            public override string ToString()
            {
                return Timestamp.ToString("HH:mm:ss") + " " + Message;
            }
        }
        
        /// <summary>
        /// Initializes a new instance of <see cref="Logger"/>
        /// </summary>
        public Logger()
        {
            entries = new ConcurrentQueue<LogEntry>();
            timer = new Timer();
            timer.Interval = 800;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        /// <summary>
        /// Save timer tick, pause timer and save, resume when done
        /// </summary>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();

            if (entries.Count > 0)
            {
                WriteToFile();
            }

            timer.Start();
        }

        /// <summary>
        /// Write queued log entries to file!
        /// </summary>
        private void WriteToFile()
        {
            List<string> lines = new List<string>();


            while(entries.TryDequeue(out LogEntry entry))
            {
                lines.Add(entry.ToString());
            }

            File.AppendAllLines(CurrentLogFileName(), lines);
        }

        /// <summary>
        /// Add a new log entry
        /// </summary>
        public void NewLog(string message)
        {
            entries.Enqueue(new LogEntry(message));
        }

        /// <summary>
        /// Returns the current file name
        /// </summary>
        private string CurrentLogFileName()
        {
            return FilePaths.LogsPath() + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        }
    }
}
