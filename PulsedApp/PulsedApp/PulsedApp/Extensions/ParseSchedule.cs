using System;
using System.Collections.Generic;
using System.Text;
using PulsedApp.Models;
using OfficeOpenXml;
using System.Linq;

namespace PulsedApp.Extensions
{
    public static class ParseSchedule
    {
        public static List<Event> ParseXLS(string filepath)
        {
            bool is_not_first_col = false;
            ExcelWorksheet currentSheet = null;
            List<Event> events = new List<Event>();

            Dictionary<string, string> participantTypes = new Dictionary<string, string>(); // color code, type || type e.g. 'Pulsers', 'Empower All students'            

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
                                    Event e = new Event(event_title, event_location, participantType, current_date, startTimeS, endTimeS);
                                    if (e != null)
                                        events.Add(e);
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
    }
}
