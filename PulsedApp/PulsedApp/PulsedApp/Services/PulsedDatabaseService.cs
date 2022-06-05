using PulsedApp.Extensions;
using PulsedApp.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulsedApp.Services
{
    public class PulsedDatabaseService
    {
        public List<Event> Events { get; set; }
        //public List<string> MemberTypes { get; set; }
        public List<string> MemberTypes { get; set; }

        static string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyPulsedData.db");
        SQLiteConnection db = new SQLiteConnection(dbPath);

        public PulsedDatabaseService()
        {
            try {
                // TODO: connect to DB and add events
                // Temp: Get from json file all events
                this.Events = new List<Event>();
                //this.Events.AddRange(ParseSchedule.GetEventsFromJSON());
                (List<Event> events, List<string> memberTypes) = GetEvents();//GetEventsAsync().Result;
                
                if (events != null)
                    this.Events.AddRange(events);

                if (memberTypes != null)
                    this.MemberTypes.AddRange(memberTypes.Where(m => m.Length > 0)); // filter out blank results
            }
            catch (Exception ex) {
                Debug.WriteLine($"Couldn't get DB - {ex.ToString()}");
            }
        }

        public List<Event> GetEventsByDate(string date) // date in format DAY MONTH DATENUMBER | e.g. "THURSDAY NOVEMBER 18"
        {
            try
            {
                Debug.WriteLine($"Getting events for {date}...");
                // TODO: Get events by date
                List<Event> events = new List<Event>();
                //(e => e.EventDate.ToUpper().Trim() == date.ToUpper().Trim()));
                events.AddRange(this.Events.Where(e => e.EventDate.Equals(date)));
                return events;

            } catch (Exception ex)
            {
                Debug.WriteLine("Couldn't get events by date " + date + " - " + ex.Message);
                return null;
            }
        }

        public List<Event> GetEventsByWeek(string date) // date in format DAY MONTH DATENUMBER | e.g. "THURSDAY NOVEMBER 18"
        {
            // TODO: Get events in current week
            return (List<Event>)this.Events.Where(e => e.EventDate == date);
        }

        public async void AddEventAsync()
        {
            // TODO: Add event to 'db'
            await Task.Delay(1000);
            Debug.WriteLine($"AddEventAsync Not implemented.");
        }

        public async void UpdateEventAsync(Event e)
        {
            // TODO: Update event in 'db'
            await Task.Delay(1000);
            Debug.WriteLine($"UpdateEventAsync Not implemented.");
        }

        public async void DeleteEventAsync(Event e)
        {
            // TODO: Delete event in 'db'
            await Task.Delay(1000);
            Debug.WriteLine($"DeleteEventAsync Not implemented.");
        }

        public async Task<Event> GetEventAsync(int event_id)
        {
            // TODO: Return event from 'db'
            await Task.Delay(1000);
            Debug.WriteLine($"GetEventAsync Not implemented.");
            return null;
        }

        public async Task<(List<Event> events, List<string> participantTypes)> GetEventsAsync()
        {
            try
            {
                // TODO: Return events from 'db'
                await Task.Delay(25);
                return ParseSchedule.GetEventsFromJSON();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Couldn't get all events. - {ex.ToString()}");
                return (null, null);
            }
        }

        public (List<Event> events, List<string> participantTypes) GetEvents()
        {
            try {
                return ParseSchedule.GetEventsFromJSON();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Couldn't get all events. - {ex.ToString()}");
                return (null, null);
            }
        }
    }
}
