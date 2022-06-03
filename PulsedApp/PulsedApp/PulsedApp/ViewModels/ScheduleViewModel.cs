using System;
using System.IO;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using PulsedApp.Models;
using System.Threading.Tasks;
using MvvmHelpers.Commands;
using MvvmHelpers;
using PulsedApp.Extensions;
using System.Reflection;
using OfficeOpenXml;
using System.Diagnostics;
using System.Collections.Generic;
using PulsedApp.Services;
using System.Globalization;
using System.Linq;

namespace PulsedApp.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        // Refresh list
        public ICommand RefreshCommand { get; }
        public ICommand SortByParticipantTypeCommand { get; }

        enum ScheduleDateSort
        {
            Today,
            Weekly,
            Monthly,
        }

        private static PulsedDatabaseService pDB;
        public ObservableRangeCollection<Event> ScheduleEvents { get; set; }
        public ObservableRangeCollection<string> EventMemberTypes { get; set; }
        //public ObservableRangeCollection<Grouping<string, Event>> eventsGroups = new ObservableRangeCollection<Grouping<string, Event>>();

        public string dateFormat = "dddd MMMM dd";

        // UI stuff
        public string lbl_debug { get; set; }

        public ScheduleViewModel()
        {
            Title = "Schedule";
            RefreshCommand = new AsyncCommand(Refresh);
            SortByParticipantTypeCommand = new AsyncCommand(SortByParticipantType);

            if (pDB == null)
            {
                //GetEventsFromXLSResource(); // TODO: Fix
                pDB = new PulsedDatabaseService();

                EventMemberTypes = new ObservableRangeCollection<string>();
                //EventMemberTypes.AddRange(pDB.MemberTypes);
                //Debug.WriteLine($"Nr of types: {EventMemberTypes.Count()}");

                ScheduleEvents = new ObservableRangeCollection<Event>();
            }

            DisplayTodayEvents();
        }

        private void DisplayEventsByDate(string date) {
            // Get date chosen by user
            // Parse date to format: "THURSDAY NOVEMBER 18" | DAYNAME MONTH DAYNUMBER

            // Get events with of that date
        }

        private void DisplayTodayEvents() {
            DateTime todayDate = DateTime.Now;
            string formattedDate = "THURSDAY NOVEMBER 18";
            lbl_debug = formattedDate.ToUpper();

            this.ScheduleEvents.AddRange(pDB.GetEventsByDate(formattedDate));
            Debug.WriteLine($"Displaying {this.ScheduleEvents.Count} events");
        }

        private void DisplayCurrentWeekEvents()
        {
            // find dates of current week
            DateTime startOfWeek = DateTime.Today.AddDays(
                (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek -
                (int)DateTime.Today.DayOfWeek);

            string week_date_range = string.Join("," + Environment.NewLine, Enumerable
                .Range(0, 7)
                .Select(i => startOfWeek
                    .AddDays(i)
                    .ToString(dateFormat)));

            // for each date of the current week
            DateTime todayDate = DateTime.Now;
            lbl_debug = todayDate.ToString(dateFormat).ToUpper();

            //string formattedDate = "THURSDAY NOVEMBER 18";
            //events.AddRange(pDB.GetEventsByDate(formattedDate));
        }

        async Task SortByParticipantType()
        {

            await Task.Delay(2000);
        }

        async Task Refresh()
        {
            IsBusy = true;
            await Task.Delay(2000);
            IsBusy = false;
        }

        private FileStream SaveStreamToFile(string fileFullPath, Stream stream)
        {
            try {
                Debug.WriteLine($"Storing stream in {fileFullPath}...");

                if (stream.Length == 0) return null;

                //Create a FileStream object to write a stream to a file
                using (FileStream fileStream = File.Create(fileFullPath, (int)stream.Length))
                {
                    // Fill the bytes[] array with the stream data
                    byte[] bytesInStream = new byte[stream.Length];
                    stream.Read(bytesInStream, 0, (int)bytesInStream.Length);

                    // Use FileStream object to write to the specified file
                    fileStream.Write(bytesInStream, 0, bytesInStream.Length);

                    return fileStream;
                }
            }
            catch (Exception e) {
                Debug.WriteLine("Couldn't save stream to file - " + e.ToString());
                return null;
            }
            finally
            {
                Debug.WriteLine($"Finished storing schedule at {fileFullPath}.");
                // DEBUG Check if file was stored:
                string[] files = Directory.GetFiles(FileSystem.CacheDirectory);
                foreach (var file in files)
                {
                    Debug.WriteLine("Found file: " + file + " in " + fileFullPath);
                }
            }
        }
    }
}