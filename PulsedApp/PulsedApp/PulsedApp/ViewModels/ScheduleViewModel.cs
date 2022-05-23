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

namespace PulsedApp.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        string xls_path = Path.Combine(FileSystem.AppDataDirectory, "PulsedSchedule.xlsx");
        public ObservableRangeCollection<Event> events { get; set; }

        public ObservableRangeCollection<Grouping<string, Event>> eventsGroups = new ObservableRangeCollection<Grouping<string, Event>>();

        // UI stuff
        public string lbl_debug { get; set; }

        public ScheduleViewModel()
        {
            Title = "Schedule";
            lbl_debug = "DEBUG";

            //temp
            events = new ObservableRangeCollection<Event>();
            events.Add(new Event("event 1", "you mama's house", "Embrace All students", "04/07/2010", "8:00am", "10:00am"));
            events.Add(new Event("event 2", "you dada's house", "Empower All students", "04/07/2010", "8:00am", "10:00am"));
            events.Add(new Event("event 3", "you sister's house", "Pulsers", "04/07/2010", "8:00am", "10:00am"));

            GetEventsCommand = new AsyncCommand(GetEvents);
            RefreshCommand = new AsyncCommand(Refresh);
        }

        public ICommand GetEventsCommand { get; }
        public ICommand RefreshCommand { get; }

        async Task Refresh()
        {
            IsBusy = true;
            await Task.Delay(2000);
            IsBusy = false;
        }

        async Task GetEvents() {
            // TODO (much later): Get events from DB

            // DEBUG
            lbl_debug = xls_path;

            // temp solution: parse local excel schedule file
            //events.AddRange(ParseSchedule.ParseXLS(xls_path));
        }
    }
}