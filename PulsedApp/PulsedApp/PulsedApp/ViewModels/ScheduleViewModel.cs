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
            //Today,
            Daily,
            Weekly,
            Monthly,
            //Pick_Date
        }

        public DateTime pickedDate { get; set; }

        private static PulsedDatabaseService pDB;
        private ObservableRangeCollection<string> eventMemberTypes;
        public ObservableRangeCollection<string> EventMemberTypes {
            get {
                if (eventMemberTypes == null) {
                    eventMemberTypes = new ObservableRangeCollection<string>();
                    eventMemberTypes.Add("All");
                    eventMemberTypes.AddRange(GetParticipantsOfCurrentEvents());
                }
                
                return eventMemberTypes;
            }
            set { eventMemberTypes = value; }
        }
        public List<EventsGroup> ScheduleEventsGroup;
        private ObservableRangeCollection<EventsGroup> sortedScheduleEventsGroup;
        public ObservableRangeCollection<EventsGroup> SortedScheduleEventsGroup { get => sortedScheduleEventsGroup;
            set {
                sortedScheduleEventsGroup = value;
                OnPropertyChanged();
            }
        }

        // View sorts the ScheduledEvents from this class
        public string[] ScheduleDateViews {
            get { return Enum.GetNames(typeof(scheduleDateSort)); }
        }
        private int selectedDView = 0;
        public int selectedDateView { get { return selectedDView; }
            set {
                selectedDView = value;
                SortByDateView(selectedDView);
                OnPropertyChanged();
            }
        }
        public string dateFormat = "dddd MMMM d";
        public string lbl_debug { get; set; }
        private string scroll_s;
        public string lbl_scroll { get => scroll_s; set { scroll_s = value; OnPropertyChanged(); } }

        // Type sorts what events to get from pDB class
        private int selectedPType = 0;
        public int selectedParticipantType { get => selectedPType;
            set {
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

                if (pDB == null) {
                    EventMemberTypes = new ObservableRangeCollection<string>();
                    ScheduleEventsGroup = new List<EventsGroup>();
                    SortedScheduleEventsGroup = new ObservableRangeCollection<EventsGroup>();

                    //GetEventsFromXLSResource(); // TODO: Fix
                    pDB = new PulsedDatabaseService();
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Couldn't initialize ScheduleViewModel - {ex.ToString()}");
            }
            finally {
                pickedDate = DateTime.Today; // set picked date to today
                selectedDateView = 0; // Set date view before participant sorting (gets all events)
                selectedParticipantType = 0; // filters events
            }
        }

        private void SortByParticipantType(int index)
        {
            try {
                if (index != -1 && this.EventMemberTypes.Count > index)
                {
                    string type_s = this.EventMemberTypes[index];
                    Debug.WriteLine("Showing events for " + type_s);

                    // clear and add all the events in sorted list before filtering to sort it
                    this.SortedScheduleEventsGroup.Clear();
                    int eg_i = 0;
                    foreach (EventsGroup eg in ScheduleEventsGroup) {
                        this.SortedScheduleEventsGroup.Add(new EventsGroup(eg.Title, eg.ShortName));
                        foreach (Event ev in eg) {
                            this.SortedScheduleEventsGroup[eg_i].Add(new Event(ev.Title, ev.Location,
                                ev.ParticipantType,ev.EventDate, ev.StartTime, ev.EndTime));
                        }
                        eg_i++;
                    }

                    // if sorting by 'All' dont remove any events
                    if (type_s == "All")
                        return;

                    // for every group in the eventgroup list
                    for (int i_eg = 0; i_eg < this.SortedScheduleEventsGroup.Count(); i_eg++) {
                        // for every event in the group list
                        for (int i_ev = this.SortedScheduleEventsGroup[i_eg].Count() - 1; i_ev >= 0; i_ev--) {
                            Event ev = this.SortedScheduleEventsGroup[i_eg][i_ev];
                            if (ev.ParticipantType != null && ev.ParticipantType.ToUpper().Trim() != type_s.ToUpper().Trim())
                                this.SortedScheduleEventsGroup[i_eg].RemoveAt(i_ev);
                        }
                    }
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
                        //case "Pick_Date":
                        //    // TODO: Show date picker
                        //    // TODO: Get picked date
                        //    DateTime pickedDate = DateTime.Today;
                        //    lbl_scroll = pickedDate.ToString("dddd");
                        //    ViewPickedDateEvents(pickedDate);
                        //    break;
                        case "Daily": // "Today"
                            lbl_scroll = this.pickedDate.ToString("dddd");
                            ViewDailyEvents(this.pickedDate);
                            break;
                        case "Weekly":
                            List<string> wr = new List<string>();
                            // Display current week by default
                            wr.AddRange(GetWeekRange(this.pickedDate));
                            lbl_scroll = $"{wr[0].Substring(0, 2)}-{wr.Last().Substring(0, 2)}";
                            // Show current week
                            ViewWeeksEvents(wr);
                            break;
                        case "Monthly":
                            // TODO: Show current month events
                            lbl_scroll = this.pickedDate.ToString("MMM");
                            ViewMonthlyEvents();
                            break;
                        default:
                            break;
                    }

                    // Set participant types of newly sorted events
                    this.EventMemberTypes.Clear();
                    this.EventMemberTypes.Add("All");
                    this.EventMemberTypes.AddRange(GetParticipantsOfCurrentEvents());
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Couldn't ViewPickedDateEvents - " + ex.ToString());
            }
        }
        private void ViewDailyEvents(DateTime date)
        {
            try {
                string formattedDate = date.ToString(dateFormat);//"THURSDAY NOVEMBER 18";
                lbl_debug = formattedDate;

                // Using list with grouping
                this.ScheduleEventsGroup.Clear();
                EventsGroup tempEG = new EventsGroup(formattedDate, " ");
                tempEG.AddRange(pDB.GetEventsByDate(formattedDate));
                this.ScheduleEventsGroup.Add(tempEG);

                Debug.WriteLine($"Displaying {this.ScheduleEventsGroup[0].Count} events");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Couldn't ViewTodayEvents - " + ex.ToString());
            }
        }
        private void ViewWeeksEvents(List<string> weekRange)
        {
            try {
                //this.ScheduleEvents.Clear();
                this.ScheduleEventsGroup.Clear();
                // for each date of the current week
                foreach (string day in weekRange)
                {
                    DateTime parsedDate;
                    if (DateTime.TryParse(day, out parsedDate))
                    {
                        // default format "dd-MMMM-yyyy"
                        string formattedDate = parsedDate.ToString(dateFormat).ToUpper(); //"THURSDAY NOVEMBER 18"; // pass as format "THURSDAY NOVEMBER 18"
                        //Debug.WriteLine("Parsed date to: " + );

                        // Group events by DAY
                        EventsGroup currentWeekGroup = new EventsGroup(formattedDate.Split(' ')[0], formattedDate.Split(' ')[2]);
                        
                        //this.ScheduleEvents.AddRange(pDB.GetEventsByDate(formattedDate));
                        // Add events in group
                        currentWeekGroup.AddRange(pDB.GetEventsByDate(formattedDate));
                        // Add group of events to list of groups
                        this.ScheduleEventsGroup.Add(currentWeekGroup);
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
        private List<string> GetParticipantsOfCurrentEvents()
        {
            List<string> participants = new List<string>();

            if (this.ScheduleEventsGroup != null) {
                foreach (EventsGroup eg in this.ScheduleEventsGroup)
                {
                    foreach (Event ev in eg)
                    {
                        if (!participants.Contains(ev.ParticipantType))
                        {
                            participants.Add(ev.ParticipantType);
                        }
                    }
                }
            }

            return participants;
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

            switch (selectedDateView)
            {
                case 0: // Today (Daily)
                    // Display events of next day
                    this.pickedDate = pickedDate.AddDays(-1);
                    break;
                case 1: // Weekly
                    this.pickedDate = pickedDate.AddDays(-7);
                    break;
                case 2: // Monthly
                    this.pickedDate = pickedDate.AddMonths(-1);
                    break;
                default:
                    break;
            }

            // Refresh sorting
            SortByDateView(selectedDateView);
            // Show 'All' types
            selectedParticipantType = 0;
        }
        async Task ScrollNext() {
            await Task.Delay(25);
            Debug.WriteLine("Next button clicked.");

            switch (selectedDateView)
            {
                case 0: // Today (Daily)
                    // Display events of next day
                    this.pickedDate = pickedDate.AddDays(1);
                    break;
                case 1: // Weekly
                    this.pickedDate = pickedDate.AddDays(7);
                    break;
                case 2: // Monthly
                    this.pickedDate = pickedDate.AddMonths(1);
                    break;
                default:
                    break;
            }

            // Refresh sorting
            SortByDateView(selectedDateView);
            // Show 'All' types
            selectedParticipantType = 0;
        }
    }
}