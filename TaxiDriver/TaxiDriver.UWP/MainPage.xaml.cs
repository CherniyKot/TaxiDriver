using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Timers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TaxiDriver.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            Xamarin.FormsMaps.Init("IL4KJLeeFpfhl9mqdvw8~QuoEqNN-cICujvx87S4zcg~AgVzx_pxd7tdbOkilsKFwk5B-2iNInOs1OC1HXhm810TqteSR1TzPN87K6czdFAL");
            MapService.ServiceToken = "IL4KJLeeFpfhl9mqdvw8~QuoEqNN-cICujvx87S4zcg~AgVzx_pxd7tdbOkilsKFwk5B-2iNInOs1OC1HXhm810TqteSR1TzPN87K6czdFAL";

            SendLocation(null, null);
            Timer timer = new Timer(10000);
            timer.Elapsed += SendLocation;
            timer.Start();

            LoadApplication(new TaxiDriver.App());
        }

        private void SendLocation(object sender, ElapsedEventArgs e)
        {
#warning todo
            int id = 2;
            double Longitude = 59.945580; 
            double Latitude = 30.373148;
            Server.SendLocation(id, Latitude, Longitude);
        }
    }
}
