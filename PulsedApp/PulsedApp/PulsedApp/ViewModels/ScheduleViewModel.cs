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

namespace PulsedApp.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        string xls_filename = "PulsedSchedule.xlsx";
        string cacheFile = "";

        private static PulsedDatabaseService pDB;

        public ObservableRangeCollection<Event> events { get; set; }

        public ObservableRangeCollection<Grouping<string, Event>> eventsGroups = new ObservableRangeCollection<Grouping<string, Event>>();

        // UI stuff
        public string lbl_debug { get; set; }

        public ScheduleViewModel()
        {
            Title = "Schedule";
            pDB = new PulsedDatabaseService();

            events = new ObservableRangeCollection<Event>();
            
            //GetEventsFromXLSResource(); // TODO: Fix
            RefreshCommand = new AsyncCommand(Refresh);
            //GetEventsFromDBCommand = new AsyncCommand(GetEventsFromDB);
        }

        // Refresh list
        public ICommand RefreshCommand { get; }

        //public ICommand GetEventsFromDBCommand { get; }

        async Task Refresh()
        {
            IsBusy = true;
            await Task.Delay(2000);
            IsBusy = false;
        }

        // TODO: Get from DB, not JSON
        //async void GetEventsFromJSON()
        //{
        //    await Task.Delay(1000);
        //    // TODO: Get current day's events

        //    // Temp code
        //    var asm = IntrospectionExtensions.GetTypeInfo(typeof(ScheduleViewModel)).Assembly;

        //    // Set cache path + filename to save to after
        //    cacheFile = Path.Combine(FileSystem.CacheDirectory, json_filename);
        //    if (File.Exists(cacheFile)) { File.Delete(cacheFile); }

        //    // Find schedule in embedded resources
        //    foreach (var res in asm.GetManifestResourceNames())
        //    {
        //        if (res.Contains(json_filename))
        //        {
        //            Debug.WriteLine("found resource: " + res);
        //            Stream resStream = asm.GetManifestResourceStream(res);

        //            // read the json stream
        //            string json_str = "";
        //            using (var reader = new StreamReader(resStream))
        //            {
        //                json_str = reader.ReadToEnd();
        //                //Debug.WriteLine($"json string from resource: {json_str}");

        //                List<Event> eee = ParseSchedule.ParseJSON(json_str);
        //                if (eee != null)
        //                {
        //                    Debug.WriteLine($"Parsed {eee.Count} events.");
        //                    events.Clear();
        //                    events.AddRange(eee);
        //                }

        //                break;
        //            }
        //        }
        //    }
        //}

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