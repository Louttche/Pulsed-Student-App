using System;

namespace PulsedApp.Models
{
    public class Event
    {
        public string eID { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string ParticipantType { get; set; }
        public string EventDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string FullTime { get; set; }

        public Event(string title, string location, string participantType, string date, string startTime, string endTime)
        {
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