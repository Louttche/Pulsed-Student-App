using SQLite;
using System;

namespace PulsedApp.Models
{
    public class Event
    {
        public static int id = 1;
        static int generateId()
        {
            return id++;
        }

        [PrimaryKey, AutoIncrement]
        public int eID { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        // TODO: locationID to refer to object
        //[Indexed]
        //public int locationID { get; set; }
        public string ParticipantType { get; set; }
        public string EventDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string FullTime { get; set; }

        public Event() { }

        public Event(string title, string location, string participantType, string date, string startTime, string endTime)
        {
            this.eID = generateId();
            this.Title = title;
            this.Location = location;
            this.ParticipantType = participantType;
            this.EventDate = date;
            this.StartTime = startTime;
            this.EndTime = endTime;
            FullTime = $"{startTime} - {endTime}";
        }
    }
}