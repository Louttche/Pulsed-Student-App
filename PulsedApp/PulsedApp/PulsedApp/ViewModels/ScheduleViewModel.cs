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
//using Excel = Microsoft.Office.Interop.Excel;

namespace PulsedApp.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        string xls_filename = "PulsedSchedule.xlsx";
        string xls_path = ""; //Path.Combine(FileSystem.AppDataDirectory, "PulsedSchedule.xlsx");
        public ObservableRangeCollection<Event> events { get; set; }

        public ObservableRangeCollection<Grouping<string, Event>> eventsGroups = new ObservableRangeCollection<Grouping<string, Event>>();

        // UI stuff
        public string lbl_debug { get; set; }

        public ScheduleViewModel()
        {
            Title = "Schedule";

            string resourcePrefix = "";
            // Check which platform to specify path
            if (Device.RuntimePlatform == Device.iOS)
                resourcePrefix = "PulsedApp.iOS.";
            else if (Device.RuntimePlatform == Device.Android)
                resourcePrefix = "PulsedApp.Droid.";

            // copy file from embedded resources to cachedirectory of device:
            var asm = IntrospectionExtensions.GetTypeInfo(typeof(ScheduleViewModel)).Assembly;
            Stream fileStream = null;
            // Set cache path + filename to save to after
            var cacheFile = Path.Combine(FileSystem.CacheDirectory, xls_filename);
            if (File.Exists(cacheFile)) { File.Delete(cacheFile); }

            // Find schedule in embedded resources
            foreach (var res in asm.GetManifestResourceNames()) {
                if (res.Contains(xls_filename)) {
                    Debug.WriteLine("found resource: " + res);
                    fileStream = asm.GetManifestResourceStream(res);
                }
            }
            SaveStreamToFile(cacheFile, fileStream);  //<--here is where to save to local storage

            lbl_debug = File.Exists(cacheFile) ? $"Schedule found at {cacheFile}." : $"Schedule not found at {cacheFile}.";

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

        public void SaveStreamToFile(string fileFullPath, Stream stream)
        {
            try {
                System.Diagnostics.Debug.WriteLine("Storing stream...");

                if (stream.Length == 0) return;

                // Create a FileStream object to write a stream to a file
                using (FileStream fileStream = File.Create(fileFullPath, (int)stream.Length))
                {
                    // Fill the bytes[] array with the stream data
                    byte[] bytesInStream = new byte[stream.Length];
                    stream.Read(bytesInStream, 0, (int)bytesInStream.Length);

                    // Use FileStream object to write to the specified file
                    fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                }
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine("Couldn't save stream to file - " + e.ToString());
            }
            finally
            {
                Debug.WriteLine($"Finished storing schedule at {fileFullPath}.");
                // DEBUG Check if file was stored:
                //string[] files = Directory.GetFiles(FileSystem.CacheDirectory);
                //foreach (var file in files)
                //{
                //    Debug.WriteLine(file);
                //}
            }
        }

        async Task GetEvents() {
            // TODO (much later): Get events from DB

            // temp solution: parse local excel schedule file
            //events.AddRange(ParseSchedule.ParseXLS(xls_path));
        }
    }
}