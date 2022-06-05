using System;
using System.Collections.Generic;
using System.Text;

namespace PulsedApp.Models
{
    public class EventsGroup : List<Event>
    {
        public string Title { get; set; }
        public string ShortName { get; set; } //will be used for jump lists
        public string Subtitle { get; set; }
        public EventsGroup(string title, string shortName) {
            Title = title;
            ShortName = shortName;
        }

        public static IList<EventsGroup> All { private set; get; }

        static EventsGroup()
        {
            List<EventsGroup> Groups = new List<EventsGroup>();
            All = Groups; //set the publicly accessible list
        }
    }
}
