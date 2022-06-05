using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PulsedApp.ViewModels;

namespace PulsedApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SchedulePage : ContentPage
    {
        //ScheduleViewModel svm = new ScheduleViewModel();
        public SchedulePage()
        {
            InitializeComponent();
            //Debug.WriteLine($"wowowo {svm.lbl_debug}");
        }
    }
}