using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PulsedApp.Models
{
    public class EventsGroup : ObservableRangeCollection<Event>
    {
        public string Title { get; set; }
        public string ShortName { get; set; }

        public EventsGroup(string title, string shortName) {
            Title = title;
            ShortName = shortName;
        }

        //public static IList<EventsGroup> All { private set; get; }

        //static EventsGroup()
        //{
        //    ObservableRangeCollection<EventsGroup> Groups = new ObservableRangeCollection<EventsGroup>();
        //    All = Groups; //set the publicly accessible list
        //}
    }
}
