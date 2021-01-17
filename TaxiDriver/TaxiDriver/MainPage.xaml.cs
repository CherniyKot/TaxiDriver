using BingMapsRESTToolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TaxiDriver
{
    public partial class MainPage : ContentPage
    {
        Timer RP;
        OrderStatus order;
        public MainPage()
        {
            InitializeComponent();
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;

            CheckOrderStatus(null, null);

            RP = new Timer(5000);
            RP.Elapsed += CheckOrderStatus;
            RP.Start();
        }
        public async Task<double> DrawPolyline(Polyline polyline, SimpleWaypoint from, SimpleWaypoint to)
        {
            polyline.Geopath.Clear();
            RouteRequest request = new RouteRequest()
            {
                RouteOptions = new RouteOptions()
                {
                    Avoid = new List<AvoidType>()
                    {
                        AvoidType.MinimizeTolls
                    },
                    TravelMode = TravelModeType.Driving,
                    DistanceUnits = DistanceUnitType.Kilometers,
                    RouteAttributes = new List<RouteAttributeType>()
                    {
                        RouteAttributeType.RoutePath
                    },
                    Optimize = RouteOptimizationType.TimeWithTraffic
                },
                Waypoints = new List<SimpleWaypoint>() { from, to },
                BingMapsKey = "IL4KJLeeFpfhl9mqdvw8~QuoEqNN-cICujvx87S4zcg~AgVzx_pxd7tdbOkilsKFwk5B-2iNInOs1OC1HXhm810TqteSR1TzPN87K6czdFAL"
            };
            var response = await request.Execute();
            if (response.StatusCode == 200)
                foreach (var pos in response.ResourceSets.First().Resources.OfType<Route>().First().RoutePath.Line.Coordinates.Select(e => new Position(e[0], e[1])).ToList())
                {
                    polyline.Geopath.Add(pos);
                }
            else DisplayAlert("Routing error", string.Concat(response.ErrorDetails ?? new[] { "Unknown error occured", "" }), "Ok");

            return response.ResourceSets.First().Resources.OfType<Route>().First().TravelDistance;
        }

        public async void CheckOrderStatus(object sender, ElapsedEventArgs e)
        {
            RP?.Stop();
            order = await Server.GetOrder();
            if (order == null || order.Status == 0)
            {
                Dispatcher.BeginInvokeOnMainThread(() =>
                {
                    polylineDriver.Geopath.Clear();
                    polylineOrder.Geopath.Clear();
                    CostLayout.IsVisible = false;
                });
                map.Pins.Clear();
                RP?.Start();
                return;
            }
            if (order.Status != 1)
            {
                if (map.Pins.Where(p => p.Label == "Taxi").Count() == 0)
                    map.Pins.Add(new Pin()
                    {
                        Label = "Taxi",
                        Position = new Position(order.latitudeDriver.Value, order.longitudeDriver.Value),
                        Type = PinType.SavedPin
                    });
                else
                    map.Pins.Where(p => p.Label == "Taxi").FirstOrDefault().Position = new Position(order.latitudeDriver.Value, order.longitudeDriver.Value);
            }

            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                CostLayout.IsVisible = true;
                Cost.Text = order.Cost.ToString();
            });
            if (order.Status == 2)
            {
                if (map.Pins.Any(p => p.Label == "Destination")) map.Pins.Remove(map.Pins.Where(p => p.Label == "Destination").First());
                if (map.Pins.Where(p => p.Type == PinType.Place).Count() == 0) map.Pins.Add(new Pin()
                {
                    Label = "Location",
                    Position = new Position(order.latitudeFrom, order.longitudeFrom),
                    Type = PinType.Place
                });
                Dispatcher.BeginInvokeOnMainThread(async () =>
                {
                    await DrawPolyline(polylineDriver, new SimpleWaypoint(order.latitudeDriver.Value, order.longitudeDriver.Value), new SimpleWaypoint(order.latitudeFrom, order.longitudeFrom));
                    polylineOrder.Geopath.Clear();
                });
            }
            else if (order.Status == 3)
            {
                if (map.Pins.Any(p => p.Label == "Location")) map.Pins.Remove(map.Pins.Where(p => p.Label == "Location").First());
                if (map.Pins.Where(p => p.Type == PinType.Place).Count() == 0) map.Pins.Add(new Pin()
                {
                    Label = "Destination",
                    Position = new Position(order.latitudeTo, order.longitudeTo),
                    Type = PinType.Place
                });
                Dispatcher.BeginInvokeOnMainThread(async () =>
                {
                    await DrawPolyline(polylineOrder, new SimpleWaypoint(order.latitudeDriver.Value, order.longitudeDriver.Value), new SimpleWaypoint(order.latitudeTo, order.longitudeTo));
                    polylineDriver.Geopath.Clear();
                });
            }
            RP?.Start();
        }

        private void StartDrive(object sender, EventArgs e)
        {
#warning TODO
            int id = 2;
            if (order.Status == 2) Server.SetOrderStatus(id, 3);
        }

        private void EndDrive(object sender, EventArgs e)
        {
#warning TODO
            int id = 2;
            if (order.Status == 3) Server.SetOrderStatus(id, 4);
#warning TODO if (order.Status == 2) Server.SetOrderStatus(id, 5);
            
        }
    }
}
