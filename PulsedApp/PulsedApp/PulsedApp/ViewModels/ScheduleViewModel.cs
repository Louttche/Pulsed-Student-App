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
        public ICommand ScrollNextCommand { get; }
        public ICommand ScrollPreviousCommand { get; }

        public enum scheduleDateSort {
            Today,
            Weekly,
            Monthly,
            Pick_Date
        }

        private static PulsedDatabaseService pDB;
        public ObservableRangeCollection<Event> ScheduleEvents { get; set; }
        public ObservableRangeCollection<string> EventMemberTypes { get; set; }
        //public ObservableRangeCollection<Grouping<string, Event>> EventsGroup { get; set; }
        public List<EventsGroup> ScheduleEventsDayGroup { get; set; }

        // View sorts the ScheduledEvents from this class
        public string[] ScheduleDateViews
        {
            get { return Enum.GetNames(typeof(scheduleDateSort)); }
        }
        private int selectedDView;
        public int selectedDateView { get { return selectedDView; }
            set {
                selectedDView = value;
                SortByDateView(selectedDView);
                OnPropertyChanged();
            }
        }
        public string dateFormat = "dddd MMMM dd";
        public string lbl_debug { get; set; }
        private string scroll_s;
        public string lbl_scroll { get => scroll_s; set { scroll_s = value; OnPropertyChanged(); } }

        // Type sorts what events to get from pDB class
        private int selectedPType;
        public int selectedParticipantType { get => selectedPType; set {
                selectedPType = value;
                // Sort based on type
                SortByParticipantType(selectedPType);
                OnPropertyChanged();
            }
        }

        public ScheduleViewModel()
        {
            try
            {
                Title = "Schedule";
                RefreshCommand = new AsyncCommand(Refresh);
                ScrollNextCommand = new AsyncCommand(ScrollNext);
                ScrollPreviousCommand = new AsyncCommand(ScrollPrevious);

                if (pDB == null)
                {
                    ScheduleEvents = new ObservableRangeCollection<Event>();
                    EventMemberTypes = new ObservableRangeCollection<string>();
                    ScheduleEventsDayGroup = new List<EventsGroup>();

                    //GetEventsFromXLSResource(); // TODO: Fix
                    pDB = new PulsedDatabaseService();
                    EventMemberTypes.Add("All");
                    EventMemberTypes.AddRange(pDB.MemberTypes);
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Couldn't initialize ScheduleViewModel - {ex.ToString()}");
            }
            finally {
                ViewTodayEvents();
            }
        }

        private void SortByParticipantType(int index)
        {
            try {
                if (index != -1 && this.EventMemberTypes.Count > index)
                {
                    string type_s = this.EventMemberTypes[index];
                    Debug.WriteLine("Showing events for " + type_s);

                    this.ScheduleEvents.Clear();
                    this.ScheduleEvents.AddRange(pDB.Events.Where(e => e.ParticipantType.Equals(type_s)));
                }

            }
            catch (Exception ex) {
                Debug.WriteLine($"Couldn't SortByParticipantType - {ex.ToString()}");
            }
        }
        private void SortByDateView(int index)
        {
            try
            {
                if (index != -1 && this.ScheduleDateViews.Length > index)
                {
                    Debug.WriteLine("Selected " + this.ScheduleDateViews[index]);
                    switch (this.ScheduleDateViews[index])
                    {
                        case "Pick_Date":
                            // TODO: Show date picker
                            // TODO: Get picked date
                            DateTime pickedDate = DateTime.Today;
                            lbl_scroll = pickedDate.ToString("dddd");
                            ViewPickedDateEvents(pickedDate);
                            break;
                        case "Today":
                            lbl_scroll = DateTime.Today.ToString("dddd");
                            ViewTodayEvents();
                            break;
                        case "Weekly":
                            List<string> wr = new List<string>();
                            // Display current week by default
                            wr.AddRange(GetWeekRange(DateTime.Today));
                            lbl_scroll = $"{wr[0].Substring(0, 2)}-{wr.Last().Substring(0, 2)}";
                            // Show current week
                            ViewWeeksEvents(wr);
                            break;
                        case "Monthly":
                            // TODO: Show current month events
                            lbl_scroll = DateTime.Today.ToString("MMM");
                            ViewMonthlyEvents();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Couldn't SortByDateView - " + ex.ToString());
            }
        }
        private void ViewPickedDateEvents(DateTime picked_time) {
            try
            {
                // TODO: Parse date
                string formattedDate = "";
                // display events of that date
                this.ScheduleEvents.Clear();
                this.ScheduleEvents.AddRange(pDB.GetEventsByDate(formattedDate));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Couldn't ViewPickedDateEvents - " + ex.ToString());
            }
        }
        private void ViewTodayEvents()
        {
            try
            {
                // TODO: Put all Events in one group

                // TODO: Fix formattedDate
                DateTime todayDate = DateTime.Now;
                string formattedDate = "THURSDAY NOVEMBER 18";
                lbl_debug = formattedDate.ToUpper();

                this.ScheduleEvents.Clear();
                this.ScheduleEvents.AddRange(pDB.GetEventsByDate(formattedDate));
                Debug.WriteLine($"Displaying {this.ScheduleEvents.Count} events");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Couldn't ViewTodayEvents - " + ex.ToString());
            }
        }
        private void ViewWeeksEvents(List<string> weekRange)
        {
            try {
                this.ScheduleEvents.Clear();
                // for each date of the current week
                foreach (string day in weekRange)
                {
                    DateTime parsedDate;
                    if (DateTime.TryParse(day, out parsedDate))
                    {
                        // default format "dd-MMMM-yyyy"
                        string formattedDate = "THURSDAY NOVEMBER 18"; // pass as format "THURSDAY NOVEMBER 18"
                        Debug.WriteLine("Parsed date to: " + parsedDate.ToString(dateFormat));

                        // Group events by DAY
                        EventsGroup currentWeekGroup = new EventsGroup(formattedDate.Split(' ')[0], formattedDate.Split(' ')[2]);
                        
                        //this.ScheduleEvents.AddRange(pDB.GetEventsByDate(formattedDate));
                        // Add events in group
                        currentWeekGroup.AddRange(pDB.GetEventsByDate(formattedDate));
                        // Add group of events to list of groups
                        ScheduleEventsDayGroup.Add(currentWeekGroup);
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine("Couldn't show weekly events: " + ex.ToString());
            }
        }
        private void ViewMonthlyEvents()
        {
            // TODO: Group events based on week of month
            // TODO: Show current month's events
            Debug.WriteLine("'ViewMonthlyEvents' Not yet implemented.");
        }
        private List<string> GetWeekRange(DateTime current_date, string format = "dd-MMMM-yyyy")
        {
            try
            {
                DateTime startOfWeek = current_date.AddDays(
                            (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek -
                            (int)DateTime.Today.DayOfWeek);

                List<string> week_range = new List<string>();
                week_range.AddRange(Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i).ToString(format)));
                Debug.WriteLine($"Got week {week_range[0]}-{week_range.Last()}");

                return week_range;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Couldn't GetWeekRange - " + ex.ToString());
                return null;
            }
        }
        async Task Refresh()
        {
            IsBusy = true;
            await Task.Delay(2000);
            IsBusy = false;
        }
        async Task ScrollPrevious() {
            await Task.Delay(25);
            Debug.WriteLine("Previous button clicked.");
        }
        async Task ScrollNext() {
            await Task.Delay(25);
            Debug.WriteLine("Next button clicked.");
        }
    }
}