using Newtonsoft.Json;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.ExternalApi
{
    public partial class NinjaVanApiService : INinjaVanApiService
    {
        //public const string apiUrl = "https://api-sandbox.ninjavan.co/sg/";
        public const string apiUrl = "https://api.ninjavan.co/my/";
        public const string client_id = "42ee3b648c594343aea4a34993b1fbe4";
        public const string client_secret = "634d424743bc414f976a7e5c786e276b";
        public const string access_token = "Bearer Pl2aXY5xUIs49GMCGL2nEWTDd96VFXyUORUmr1oO";

        public class OAuthData
        {
            public string access_token { get; set; }
            public string expires { get; set; }
            public string token_type { get; set; }
            public string expires_in { get; set; }
        }

        public class OrderCreateData
        {
            public string id { get; set; }
            public string status { get; set; }
            public string message { get; set; }
            public string order_ref_no { get; set; }
            public string tracking_id { get; set; }

            //status 401 unauthorized
            public string code { get; set; }
            public List<string> messages { get; set; }
            public string application { get; set; }
            public string description { get; set; }
            public data data { get; set; }
        }

        public class data
        {
            public string message { get; set; }
        }

        public class TrackOrderData
        {
            public string trackingId { get; set; }
            public string status { get; set; }
            public string updatedAt { get; set; }
        }

        public OAuthData GetAccessToken()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(apiUrl + "oauth/access_token?grant_type=client_credentials");
                var postData = "client_id=" + client_id;
                postData += "&client_secret=" + client_secret;
                var data = Encoding.ASCII.GetBytes(postData);
                //string json = JsonConvert.SerializeObject(new
                //{
                //    client_id = client_id,
                //    client_secret = client_secret
                //});

                //var data = Encoding.ASCII.GetBytes(json);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var resposnseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var c = JsonConvert.DeserializeObject<OAuthData>(resposnseData);
                return c;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        //public OrderCreateData CreateOrder()
        //{
        //    var request = (HttpWebRequest)WebRequest.Create("https://api-sandbox.ninjavan.co/sg/order-create/3.0/orders");

        //    string json = JsonConvert.SerializeObject(new
        //    {
        //        from_postcode="159363",
        //     from_address1="30 Jalan Kilang Barat",
        //     from_address2="Ninja Van HQ",
        //     from_locality="Bukit Merah",
        //     from_city="SG",
        //     from_country="SG",
        //     from_email="jane.doe@gmail.com",
        //     from_name="Han Solo Exports",
        //     from_contact="91234567",
        //     to_postcode="318993",
        //     to_address1="998 Toa Payoh North",
        //     to_address2="#01-10",
        //     to_locality="Toa Payoh",
        //     to_city="SG",
        //     to_country="SG",
        //     to_email="john.doe@gmail.com",
        //     to_name="James T Kirk",
        //     to_contact="98765432",
        //     delivery_date="2014-12-02",
        //     pickup_date="2014-12-01",
        //     pickup_weekend=true,
        //     delivery_weekend=true,
        //     staging=false,
        //     pickup_timewindow_id=1,
        //     delivery_timewindow_id=2,
        //     max_delivery_days=1,
        //     cod_goods=35.50,
        //     pickup_instruction="Warehouse alarm bell does not work, please call",
        //     delivery_instruction="My doorbell is broken.Please knock the door",
        //     requested_tracking_id="24168",
        //     order_ref_no="8375",
        //     type="NORMAL",
        //     parcel_size= 1,
        //     parcel_volume= 4000,
        //     parcel_weight= 1.2
        //    });
        //    json = "[" + json + "]";

        //    ASCIIEncoding encoder = new ASCIIEncoding();
        //    byte[] data = encoder.GetBytes(json);

        //    request.Method = "POST";
        //    request.ContentType = "application/json";
        //    request.ContentLength = data.Length;
        //    request.Expect = "application/json";
        //    request.Headers["Authorization"] = access_token;

        //    request.GetRequestStream().Write(data, 0, data.Length);

        //    try
        //    {
        //        using (var response = (HttpWebResponse)request.GetResponse())
        //        {
        //            string s = response.ToString();
        //            StreamReader reader = new StreamReader(response.GetResponseStream());
        //            String jsonresponse = "";
        //            String temp = null;
        //            while ((temp = reader.ReadLine()) != null)
        //            {
        //                jsonresponse += temp;
        //            }
        //            var c = JsonConvert.DeserializeObject<OrderCreateData>(jsonresponse.Substring(1, jsonresponse.Length - 2));
        //            return c;
        //        }
        //    }
        //    catch (WebException e)
        //    {
        //        using (WebResponse response = e.Response)
        //        {
        //            HttpWebResponse httpResponse = (HttpWebResponse)response;
        //            Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
        //            using (Stream data2 = response.GetResponseStream())
        //            using (var reader = new StreamReader(data2))
        //            {
        //                string text = reader.ReadToEnd();
        //                var obj = JsonConvert.DeserializeObject<OrderCreateData>(text);
        //                return obj;
        //            }
        //        }
        //    }

        //}





        public OrderCreateData CreateOrder(Order order, Warehouse warehouse,
            Address warehouseAddress, string TrackingNumber, DateTime NinjaVanPickupDate,
            DateTime NinjaVanDeliveryDate, int? ParcelSize, int? ParcelVolume, decimal? ParcelWeight, OAuthData oauthData)
        {
            var request = (HttpWebRequest)WebRequest.Create(apiUrl + "order-create/3.0/orders");

            string json = JsonConvert.SerializeObject(new
            {
                //Warehouse Address
                //from_postcode = warehouseAddress.ZipPostalCode,
                //from_address1 = warehouseAddress.Address1,
                //from_address2 = warehouseAddress.Address2,
                //from_locality = warehouseAddress.StateProvince.Name,
                //from_city = warehouseAddress.City,
                //from_country = warehouseAddress.Country.Name,
                //from_email = warehouseAddress.Email,
                //from_name = warehouse.Name,
                //from_contact = warehouseAddress.PhoneNumber,

                from_postcode = 43300,
                from_address1 = "B1-02-21, Kantan Court, Jalan 4/2, Taman Bukit Serdang",
                from_address2 = "",
                from_locality = "Selangor",
                from_city = "Seri Kembangan",
                from_country = "Malaysia",
                from_email = "andrew@gmail.com",
                from_name = "Andrew Vendor",
                from_contact = "0163040544",

                //Customer Address
                to_postcode = order.Customer.ShippingAddress.ZipPostalCode,
                to_address1 = order.Customer.ShippingAddress.Address1,
                to_address2 = order.Customer.ShippingAddress.Address2,
                to_locality = "",
                to_city = order.Customer.ShippingAddress.City,
                to_country = "Malaysia",
                to_email = order.Customer.ShippingAddress.Email,
                to_name = order.Customer.GetFullName(),
                to_contact = order.Customer.ShippingAddress.PhoneNumber,

                delivery_date = NinjaVanDeliveryDate.ToString("yyyy-MM-dd"),
                pickup_date = NinjaVanPickupDate.ToString("yyyy-MM-dd"),
                pickup_weekend = true,
                delivery_weekend = true,
                staging = false,
                pickup_timewindow_id = 1,
                delivery_timewindow_id = 2,
                max_delivery_days = 1,
                //Key may be omitted if no COD to be collected
                //cod_goods = 35.50,
                //pickup_instruction = "Warehouse alarm bell does not work, please call",
                //delivery_instruction = "My doorbell is broken.Please knock the door",

                //If left null, Ninja Van will generate a tracking number.
                requested_tracking_id = TrackingNumber,
                order_ref_no = order.Id,
                type = "NORMAL",
                parcel_size = ParcelSize,
                parcel_volume = ParcelVolume,
                parcel_weight = ParcelWeight
            });

            json = "[" + json + "]";

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] data = encoder.GetBytes(json);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            request.Expect = "application/json";
            request.Headers["Authorization"] = "Bearer " + oauthData.access_token;
            //request.Headers["Authorization"] = "Bearer GrFu57BZGPh9nBJ4NgVqV3RHxfLDpTRiivP8ygaG";

            request.GetRequestStream().Write(data, 0, data.Length);

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    string s = response.ToString();
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    String jsonresponse = "";
                    String temp = null;
                    while ((temp = reader.ReadLine()) != null)
                    {
                        jsonresponse += temp;
                    }
                    var c = JsonConvert.DeserializeObject<OrderCreateData>(jsonresponse.Substring(1, jsonresponse.Length - 2));
                    return c;
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data2 = response.GetResponseStream())
                    using (var reader = new StreamReader(data2))
                    {
                        string text = reader.ReadToEnd();
                        var obj = JsonConvert.DeserializeObject<OrderCreateData>(text);
                        return obj;
                    }
                }
            }

        }


        public OrderCreateData CreateOrderWithSubShipperId(Order order, Warehouse warehouse, Address warehouseAddress, string TrackingNumber, DateTime NinjaVanPickupDate, DateTime NinjaVanDeliveryDate, int? ParcelSize, int? ParcelVolume, decimal? ParcelWeight, OAuthData oauthData)
        {
            throw new NotImplementedException();
        }

        public List<TrackOrderData> TrackOrder(List<string> trackingIds)
        {
            var oauthData = GetAccessToken();

            var request = (HttpWebRequest)WebRequest.Create(apiUrl + "2.0/track");

            string json = JsonConvert.SerializeObject(new
            {
                trackingIds = trackingIds
            });

            //json = "[" + json + "]";

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] data = encoder.GetBytes(json);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            request.Expect = "application/json";
            request.Headers["Authorization"] = "Bearer " + oauthData.access_token;

            request.GetRequestStream().Write(data, 0, data.Length);

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    string s = response.ToString();
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    String jsonresponse = "";
                    String temp = null;
                    while ((temp = reader.ReadLine()) != null)
                    {
                        jsonresponse += temp;
                    }
                    //var c = JsonConvert.DeserializeObject<List<TrackOrderData>>(jsonresponse.Substring(1, jsonresponse.Length - 2));
                    var c = JsonConvert.DeserializeObject<List<TrackOrderData>>(jsonresponse);
                    return c;
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data2 = response.GetResponseStream())
                    using (var reader = new StreamReader(data2))
                    {
                        string text = reader.ReadToEnd();
                        var obj = JsonConvert.DeserializeObject<List<TrackOrderData>>(text);
                        return obj;
                    }
                }
            }

        }


    }
}
