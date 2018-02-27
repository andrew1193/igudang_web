using System.Web.Mvc;
using Nop.Web.Framework.Security;
using System.Collections.Generic;
using System.Net.Http;
using System;
using Nop.Core.Domain.Messages;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Events;
using Nop.Web.Models.Events;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Customers;

namespace Nop.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        private readonly IWorkContext _workContext;
        private readonly IEventService _eventService;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;

        public HomeController(IWorkContext workContext,
            IEventService eventService,
            IStoreContext storeContext,
            ICustomerService customerService,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
            IPictureService pictureService)
        {
            _customerService = customerService;
            _storeContext = storeContext;
            _workContext = workContext;
            _eventService = eventService;
            _pictureService = pictureService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
        }

        public partial class CustomFieldValue
        {
            [JsonProperty("customFieldId")]
            public string CustomFieldId { get; set; }

            [JsonProperty("value")]
            public List<string> Value { get; set; }
        }

        public partial class Campaign
        {
            [JsonProperty("campaignId")]
            public string CampaignId { get; set; }
        }

        public partial class Data
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("dayOfCycle")]
            public string DayOfCycle { get; set; }

            [JsonProperty("campaign")]
            public Campaign Campaign { get; set; }

            [JsonProperty("customFieldValues")]
            public List<CustomFieldValue> CustomFieldValues { get; set; }

            [JsonProperty("ipAddress")]
            public string IpAddress { get; set; }
        }

        public partial class Data
        {
            public static Data FromJson(string json) => JsonConvert.DeserializeObject<Data>(json, Converter.Settings);
        }

        public static class Serialize
        {
            public static string ToJson(Data self) => JsonConvert.SerializeObject(self, Converter.Settings);
        }

        public class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
            };
        }

        protected string GetIPAddress()
        {
            //string szRemoteAddr = request.UserHostAddress;
            //string szXForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];
            //string szIP = "";

            //if (szXForwardedFor == null)
            //{
            //    szIP = szRemoteAddr;
            //}
            //else
            //{
            //    szIP = szXForwardedFor;
            //    if (szIP.IndexOf(",") > 0)
            //    {
            //        string[] arIPs = szIP.Split(',');

            //        foreach (string item in arIPs)
            //        {
            //            if (!isPrivateIP(item))
            //            {
            //                return item;
            //            }
            //        }
            //    }
            //}
            //return szIP;

            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> FormSubmit1(string gender, string email)
        {
            try
            {

                
                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(email, _storeContext.CurrentStore.Id);
                if (subscription != null)
                {
                    return Json(new { success = false, msg = "This email address is already subscribed, thank you!" });

                }
                else
                {
                    subscription = new NewsLetterSubscription
                    {
                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                        Email = email,
                        Active = false,
                        StoreId = _storeContext.CurrentStore.Id,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);


                    var lastIp = _workContext.CurrentCustomer.LastIpAddress;
                    var record = new Data();
                    var campain = new Campaign();
                    campain.CampaignId = "4wNHL";
                    record.Campaign = campain;
                    record.Name = email;
                    record.Email = email;
                    record.DayOfCycle = "0";
                    record.IpAddress = lastIp;

                    //custom field 
                    var customField = new CustomFieldValue();
                    customField.CustomFieldId = "tB4CP";
                    customField.Value = new List<string>();

                    string strGender = "";
                    if (gender == "M")
                    {
                        strGender = "Male";
                    }
                    else if (gender == "F")
                    {
                        strGender = "Female";
                    }
                    customField.Value.Add(strGender);


                    record.CustomFieldValues = new List<CustomFieldValue>();
                    record.CustomFieldValues.Add(customField);
                    var parameter = Serialize.ToJson(record);

                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("https://api.getresponse.com/v3/");
                    client.DefaultRequestHeaders.Add("X-Auth-Token", "api-key dfc0b5abd68fed94fa69a30d618dcd1b");
                    client.DefaultRequestHeaders
                          .Accept
                          .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "contacts");
                    request.Content = new StringContent(parameter,
                                                        Encoding.UTF8,
                                                        "application/json");//CONTENT-TYPE header

                    string msg = "";
                    await client.SendAsync(request).ContinueWith(responseTask =>
                    {
                        msg = responseTask.Result.ToString();
                        msg += responseTask.Status.ToString();
                    });






                    return Json(new { success = true, msg = "Thanks for your subscription!" });
                }

             
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Send Email Failed!" });
            }



            //var record = new Data();


            //var campain = new Campaign();
            //campain.CampaignId = "4wNHL";
            //record.Campaign = campain;
            //record.Name = email;
            //record.Email = email;
            //record.DayOfCycle = "0";
            //record.IpAddress = GetIPAddress();

            ////custom field 
            //var customField = new CustomFieldValue();
            //customField.CustomFieldId = "tB4CP";
            //customField.Value = new List<string>();
            //customField.Value.Add(gender);


            //record.CustomFieldValues = new List<CustomFieldValue>();
            //record.CustomFieldValues.Add(customField);
            //var parameter = Serialize.ToJson(record);

            //HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri("https://api.getresponse.com/v3/");
            //client.DefaultRequestHeaders.Add("X-Auth-Token", "api-key dfc0b5abd68fed94fa69a30d618dcd1b");
            //client.DefaultRequestHeaders
            //      .Accept
            //      .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "contacts");
            //request.Content = new StringContent(parameter,
            //                                    Encoding.UTF8,
            //                                    "application/json");//CONTENT-TYPE header

            //string msg = "";
            //await client.SendAsync(request).ContinueWith(responseTask =>
            //{
            //    msg = responseTask.Result.ToString();
            //});

            //return Redirect("/Home/Index");
        }


        [ChildActionOnly]
        public virtual ActionResult HomepageEventBanner()
        {
            var currentEvent = _eventService.GetCurrentEvent();
            var model = new EventModel();
            model.Id = currentEvent.Id;
            model.Name = currentEvent.Name;
            model.Description = currentEvent.Description;
            model.AvailableStartDateTimeUtc = currentEvent.StartedOnUtc;
            model.AvailableEndDateTimeUtc = currentEvent.EndedOnUtc;
            model.BackgroundPictureId = currentEvent.BackgroundPictureId;
            model.Published = currentEvent.Published;
            if (currentEvent.BackgroundPictureId > 0)
            {
                var picture = _pictureService.GetPictureById(currentEvent.BackgroundPictureId);
                model.BackgroundPictureUrl = _pictureService.GetPictureUrl(picture);
            }

            return PartialView(model);
        }
    }
}
