using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace TaxiDriver
{
    public static class Server
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<OrderStatus> GetOrder()
        {
            WebResponse response;
            try
            {
# warning TODO int id = int.Parse(await SecureStorage.GetAsync("UserId"));
                int id = 2;
                var request = WebRequest.Create($"https://localhost:44368/Api/DriverOrder?driverId={id}");
                response = request.GetResponse();
            }
            catch
            {
                return null;
            }
            OrderStatus result;
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                string r = reader.ReadLine();
                try
                {
                    result = JsonConvert.DeserializeObject<OrderStatus>(r);
                }
                catch
                {
                    return null;
                }
            }
            return result;
        }

        public static void SendLocation(int driverId, double latitude, double longitude)
        {
            var values = new Dictionary<string, string>
            {
                { "driverId", driverId.ToString() },
                { "latitude", latitude.ToString().Replace('.',',') },
                { "longitude", longitude.ToString().Replace('.',',') }
            };

            var content = new FormUrlEncodedContent(values);
            client.PostAsync("https://localhost:44368/Api/UpdateLocation", content);
        }

        public static void SetOrderStatus(int driverId, int orderStatus)
        {
            var values = new Dictionary<string, string>
            {
                { "driverId", driverId.ToString() },
                { "orderStatus", orderStatus.ToString() }
            };

            var content = new FormUrlEncodedContent(values);
            client.PostAsync("https://localhost:44368/Api/SetOrderStatus", content);
        }
    }

    public class OrderStatus
    {
        public int Status { get; set; }
        public double longitudeFrom { get; set; }
        public double latitudeFrom { get; set; }
        public double longitudeTo { get; set; }
        public double latitudeTo { get; set; }
        public double? longitudeDriver { get; set; }
        public double? latitudeDriver { get; set; }
    }


}

