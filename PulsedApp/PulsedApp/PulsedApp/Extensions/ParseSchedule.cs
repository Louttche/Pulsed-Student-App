using System;
using System.Collections.Generic;
using System.Text;
using PulsedApp.Models;
using OfficeOpenXml;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using MvvmHelpers;
using System.Threading.Tasks;
using System.Reflection;
using Xamarin.Essentials;
using System.Globalization;

namespace PulsedApp.Extensions
{
    public static class ParseSchedule
    {
        static string json_filename = "PulsedSchedule.json";//"PulsedSchedule_1.json";

        public static (List<Event> parsedEvents, List<string> participantTypes) GetEventsFromJSON()
        {
            try
            {
                List<Event> events = new List<Event>();
                List<string> pt = new List<string>();

                var asm = IntrospectionExtensions.GetTypeInfo(typeof(ParseSchedule)).Assembly;

                // Set cache path + filename to save to after
                string cacheFile = Path.Combine(FileSystem.CacheDirectory, json_filename);
                if (File.Exists(cacheFile)) { File.Delete(cacheFile); }

                // Find schedule in embedded resources
                foreach (var res in asm.GetManifestResourceNames())
                {
                    if (res.Contains(json_filename))
                    {
                        Debug.WriteLine("found resource: " + res);
                        Stream resStream = asm.GetManifestResourceStream(res);

                        // read the json stream
                        string json_str = "";
                        using (var reader = new StreamReader(resStream))
                        {
                            json_str = reader.ReadToEnd();
                            //Debug.WriteLine($"json string from resource: {json_str}");

                            List<Event> temp_events = ParseSchedule.ParseJSON(json_str);
                            if (temp_events != null) {
                                Debug.WriteLine($"Parsed {temp_events.Count} events.");
                                pt.AddRange(GetParticipantTypes(temp_events));
                                events.Clear();
                                events.AddRange(temp_events);
                            }

                            break;
                        }
                    }
                }

                return (events, pt);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Couldn't parse events from JSON. - {ex.ToString()}");
                return (null, null);
            }
        }

        private static List<string> GetParticipantTypes(List<Event> events)
        {
            List<string> result = new List<string>();
            foreach (Event e in events) {
                if (!result.Any(type => type == e.ParticipantType)) {
                    result.Add(e.ParticipantType);
                }
            }

            return result;
        }

        // Get events by parsing the xls file from embedded resources
        //private void GetEventsFromXLSResource()
        //{
        //    string resourcePrefix = "";
        //    // Check which platform to specify path
        //    if (Device.RuntimePlatform == Device.iOS)
        //        resourcePrefix = "PulsedApp.iOS.";
        //    else if (Device.RuntimePlatform == Device.Android)
        //        resourcePrefix = "PulsedApp.Droid.";

        //    // copy file from embedded resources to cachedirectory of device:
        //    var asm = IntrospectionExtensions.GetTypeInfo(typeof(ScheduleViewModel)).Assembly;
        //    Stream resStream = null;
        //    // Set cache path + filename to save to after
        //    cacheFile = Path.Combine(FileSystem.CacheDirectory, xls_filename);
        //    if (File.Exists(cacheFile)) { File.Delete(cacheFile); }

        //    // Find schedule in embedded resources
        //    foreach (var res in asm.GetManifestResourceNames())
        //    {
        //        if (res.Contains(xls_filename))
        //        {
        //            Debug.WriteLine("found resource: " + res);
        //            resStream = asm.GetManifestResourceStream(res);

        //            //FileStream filestream = SaveStreamToFile(cacheFile, resStream);

        //            //using (ExcelPackage package = new ExcelPackage(resStream))
        //            //{
        //            //    Debug.WriteLine($"viewing xlsx stream");
        //            //    package.SaveAsync(cacheFile);
        //            //    //if (package != null) {
        //            //    //    if (package.Workbook != null)
        //            //    //        Debug.WriteLine("workbook exists.");
        //            //    //    else
        //            //    //        Debug.WriteLine("workbook does NOT exist.");
        //            //    //}
        //            //    //events.AddRange(ParseSchedule.ParseXLS_FromPath(fso.Name));
        //            //    //Debug.WriteLine($"Events added: {events.Count}");
        //            //}

        //            //using (FileStream fso = File.OpenWrite(cacheFile))
        //            //{
        //            //    //Debug.WriteLine($"{fso.Name}");
        //            //    resStream.CopyTo(fso);
        //            //}

        //            break;
        //        }
        //    }
        //    // TODO: get xls contents properly to parse it
        //    //SaveStreamToFile(cacheFile, fileStream);  //<--here is where to save to local storage (bytes)

        //    // If the file exists
        //    if (File.Exists(cacheFile))
        //    {
        //        lbl_debug = File.Exists(cacheFile) ? $"Schedule found at {cacheFile}." : $"Schedule not found at {cacheFile}.";
        //    }
        //}

        public static (List<Event> events, Dictionary<string, string>  memberTypes) ParseXLS_FromPath(string filepath)
        {
            bool is_not_first_col = false;
            ExcelWorksheet currentSheet = null;
            List<Event> events = new List<Event>();

            Dictionary<string, string> participantTypes = new Dictionary<string, string>(); // color code, type || type e.g. 'Pulsers', 'Empower All students'

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try
            {
                using (ExcelPackage package = new ExcelPackage(filepath))
                {
                    int timeRow = 2; // row where the time slots are located
                    int locationCol = 1; // col where locations are located

                    // Get current month
                    DateTime dt = DateTime.Now;
                    string currentMonth = dt.ToString("MMMMM");

                    // Get date of monday of the week
                    var monday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
                    string currentMonday = monday.Day.ToString();

                    // Check which worksheet refers to current week
                    foreach (ExcelWorksheet sheet in package.Workbook.Worksheets)
                    {
                        if (sheet.Name.Contains("Mrch"))
                        { // March is written as 'Mrch' in the sheets
                            if (sheet.Name.Contains("Mrch") && sheet.Name.Contains(currentMonday))
                                currentSheet = sheet;
                        }
                        else
                        {
                            if (sheet.Name.Contains(currentMonth) && sheet.Name.Contains(currentMonday))
                                currentSheet = sheet;
                        }
                    }

                    if (currentSheet != null)
                    {
                        Console.WriteLine("Using sheet: " + currentSheet.Name);

                        // Iterate over all cells of the sheet
                        int nrOfRows = currentSheet.Dimension.End.Row;
                        int nrOfCols = currentSheet.Dimension.End.Column;

                        string current_date = "";

                        for (int row = 1; row <= nrOfRows; row++)
                        {
                            for (int col = 1; col <= nrOfCols; col++)
                            {
                                ExcelRange cell_range = currentSheet.Cells[row, col]; // current cell

                                // Get the numeric characters of cell range for filtering rows
                                string cell_number = string.Empty; // indicates cell row nr
                                for (int i = 0; i < $"{cell_range}".Length; i++)
                                {
                                    if (Char.IsDigit($"{cell_range}"[i]))
                                        cell_number += $"{cell_range}"[i];
                                    if (cell_number.Length > 0)
                                        cell_number = $"{int.Parse(cell_number)}";
                                }

                                //Make all text fit the cells
                                string[] merged_cells = cell_range.GetMergedRangeAddress().Split(':');
                                string end_range = "";
                                if (merged_cells.Count() > 1)
                                {
                                    end_range = merged_cells[1];

                                    //Console.WriteLine($"{end_range}: {}");  
                                }

                                is_not_first_col = (($"{cell_range}".Count(char.IsLetter) > 1) || !($"{cell_range}"[0] == 'A'));

                                // Get color of cell
                                string ColorValue = "#000000";
                                if (cell_range.Style.Fill.BackgroundColor.Rgb != null)
                                    ColorValue = cell_range.Style.Fill.BackgroundColor.Rgb;

                                // Check if row is 'starting' a new day schedule (By cell color and if text starts with day name)
                                List<string> day_names = new List<string>{
                                "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY" };

                                if (day_names.Any(d => $"{cell_range.Value}".Contains(d)) && (ColorValue == "FF666666"))
                                {
                                    // Get date from cell if done with top info
                                    current_date = $"{cell_range.Value}";

                                    continue; // skip to next row
                                }

                                if (current_date.Length > 0)
                                { // Check IF we're 'under' a date so we can GET EVENT from cell
                                  // Get event TITLE
                                    string event_title = $"{cell_range.Value}";

                                    // If cell has event (check from title)
                                    if (event_title.Length > 0 && is_not_first_col)
                                    {
                                        // get event LOCATION
                                        string event_location = $"{currentSheet.Cells[row, locationCol].Value}";
                                        // get event START TIME and END TIME
                                        DateTime startTime = DateTime.Now;
                                        DateTime endTime = DateTime.Now;
                                        if (end_range.Length > 0)
                                        {
                                            startTime = (DateTime)currentSheet.Cells[timeRow, col].Value;
                                            if (currentSheet.Cells[timeRow, currentSheet.Cells[end_range].End.Column + 1].Value != null)
                                                endTime = (DateTime)currentSheet.Cells[timeRow, currentSheet.Cells[end_range].End.Column + 1].Value;
                                        }
                                        else
                                        { //single cell event (1 time slot only)
                                            startTime = (DateTime)currentSheet.Cells[timeRow, col - 1].Value;
                                            endTime = (DateTime)currentSheet.Cells[timeRow, col].Value;
                                        }
                                        // format how the time will be displayed
                                        string startTimeS = startTime.ToString("hh':'mm':'tt");
                                        string endTimeS = endTime.ToString("hh':'mm':'tt");

                                        // Get event PARTICIPANT/MEMBER TYPE (e.g. Pulsers, Embrace All students)
                                        string participantType = participantTypes.First(pt => pt.Key == ColorValue).Value;

                                        // TODO: Create event and store it (TODO: in DB).
                                        //Event e = new Event(event_title, event_location, participantType, current_date, startTimeS, endTimeS);
                                        //if (e != null)
                                        //    events.Add(e);
                                    }

                                    // ELSE we either need to get a date now or get top sheet info (member types)
                                }
                                else
                                {
                                    // We're still at the top of the file so GET PARTICIPANT TYPES
                                    if (is_not_first_col && (cell_number[0] == '1') && ($"{cell_range.Value}".Length > 0) && (cell_number.Length == 1))
                                    {
                                        if (ColorValue != null)
                                            participantTypes[ColorValue] = $"{cell_range.Value}"; // Add to list
                                    }
                                }
                            }
                        }

                        Console.WriteLine($"nr of participant types: {participantTypes.Count()}\nnr of events: {events.Count()}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Couldn't parse {filepath} - {e.ToString()}");
            }
            
            return (events, participantTypes);
        }

        private static List<Event> ParseJSON(string json)
        {
            Console.WriteLine($"parsing json file...");

            List<Event> events = new List<Event>();
            // better to store events as groups based on their date, so that sorting/filtering is faster
            //ObservableRangeCollection<Grouping<string, Event>> date_events_dict = new ObservableRangeCollection<Grouping<string, Event>>();

            Dictionary<string, List<Event>> date_events = JsonConvert.DeserializeObject<Dictionary<string, List<Event>>>(json);

            // for each key in json (event date)
            foreach (KeyValuePair<string, List<Event>> date_event_pair in date_events)
            {
                string current_date = date_event_pair.Key; // shows week dates in month --> "7-11Feb"

                foreach (Event ev in date_event_pair.Value)
                {
                    // get event properties
                    string event_title = ev.Title;
                    string event_location = ev.Location;
                    string event_date = ev.EventDate;
                    string participantType = ev.ParticipantType;
                    string startTimeS = ev.StartTime;
                    string endTimeS = ev.EndTime;

                    //create event object
                    Event e = new Event(event_title, event_location, participantType, event_date, startTimeS, endTimeS);
                    // add event object to list
                    events.Add(e);
                }
            }

            return events;
        }
        private static string GetMergedRangeAddress(this ExcelRange @this)
        {
            if (@this.Merge)
            {
                var idx = @this.Worksheet.GetMergeCellId(@this.Start.Row, @this.Start.Column);
                return @this.Worksheet.MergedCells[idx - 1]; //the array is 0-indexed but the mergeId is 1-indexed...
            }
            else
            {
                return @this.Address;
            }
        }

        private static FileStream SaveStreamToFile(string fileFullPath, Stream stream)
        {
            try
            {
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
            catch (Exception e)
            {
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
