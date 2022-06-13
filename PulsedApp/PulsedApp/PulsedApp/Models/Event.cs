using SQLite;
using System;

namespace PulsedApp.Models
{
    public class Event
    {
        public static int id = 1;
        static int generateId() {
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
        public string EventDate { get; set; } // "THURSDAY NOVEMBER 18"
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string FullTime { get; set; }

        public Event() { }

        public Event(string title, string location, string participantType, string date, string startTime, string endTime)
        {
            this.eID = generateId();
            this.Title = title; // remove potential double spaces //.Replace("  ", " ")
            this.Location = location;
            this.ParticipantType = participantType;
            this.EventDate = date;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.FullTime = GetTimeToString();
        }

        public string GetTimeToString() {
            return $"{this.StartTime} - {this.EndTime}";
        }

        public DateTime GetDateTime() {
            DateTime dt;
            DateTime.TryParse(this.EventDate, out dt);
            return dt;
        }

        public string GetMonth() {
            // "THURSDAY NOVEMBER 18"
            return this.EventDate.Split(' ')[1];
        }

        public string GetDay(bool dateNr = false)
        {
            // "THURSDAY NOVEMBER 18"
            if (dateNr == true) // Get '18'
                return this.EventDate.Split(' ')[2];
            else // Get 'THURSDAY'
                return this.EventDate.Split(' ')[0];
        }
    }
}