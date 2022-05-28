using PulsedApp.Extensions;
using PulsedApp.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PulsedApp.Services
{
    public class PulsedDatabaseService
    {
        public List<Event> Events { get; set; }
        static string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyPulsedData.db");
        SQLiteConnection db = new SQLiteConnection(dbPath);

        public PulsedDatabaseService()
        {
            // Temp: Get from json file all events
            this.Events = new List<Event>();
            this.Events.AddRange(ParseSchedule.GetEventsFromJSON());

            // TODO: connect to DB and add events
        }

        public List<Event> GetEventsByDate()
        {
            List<Event> events = new List<Event>();

            // TODO: Get events by date

            return events;
        }

        public async void AddEventAsync()
        {

        }

        public async void UpdateEventAsync()
        {

        }

        public async void DeleteEventAsync()
        {

        }

        public async void GetEventAsync()
        {

        }

        public async void GetEventsAsync()
        {

        }
    }
}
