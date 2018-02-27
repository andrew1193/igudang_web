

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
namespace Nop.Web.Controllers
{
    public class LandingController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult FormSubmit1(string gender, string email)
        {
            try
            {
                string subject = "Subscribe now and get 50% discount voucher!!";
                string gen = "";
                if (gender == "M")
                {
                    gen = "Male";
                }
                else
                {
                    gen = "Female";
                }
                string content = "<p>A new member has subscribed with the following details</p></br><p>Email : " + email + "</p></br><p>Gender : " + gen + "</p>";
                var isSuccess = SendEmail(subject, content);
                if (isSuccess)
                    return Json(new { success = true, msg = "Thank you for your subscription!" });

                return Json(new { success = false, msg = "Send Email Failed!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Send Email Failed!" });
            }

        }

        [HttpPost]
        public ActionResult FormSubmit2(string gender, string email)
        {
            try
            {

                var record = new Data();
                var campain = new Campaign();
                campain.CampaignId = "4wNHL";
                record.Campaign = campain;
                record.Name = email;
                record.Email = email;
                record.DayOfCycle = "0";
                record.IpAddress = GetIPAddress();

                //custom field 
                var customField = new CustomFieldValue();
                customField.CustomFieldId = "tB4CP";
                customField.Value = new List<string>();
                customField.Value.Add(gender);


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

                client.SendAsync(request).ContinueWith(responseTask =>
                {

                });






                return Json(new { success = true, msg = "Thanks for your subscription!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Send Email Failed!" });
            }

        }
        protected string GetIPAddress()
        {
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


        [HttpPost]
        public ActionResult FormSubmit3(string name, string email, string order, string type, string msg)
        {
            try
            {
                string subject = "Get In Touch!";
                string orderNum = "";
                if (order != null)
                {
                    orderNum = order;
                }

                string content = "<b>New Message</b></br><table><tr><td>Fullname:</td><td>" + name + "</td></tr><tr><td>Email:</td><td>" + email + "</td></tr><tr><td>Order Number:</td><td>" + orderNum + "</td></tr><tr><td>Type Of Enquiry:</td><td>" + type + "</td></tr><tr><td>Message:</td><td>" + msg + "</td></tr></table>";
                var isSuccess = SendEmail(subject, content);
                if (isSuccess)
                    return Json(new { success = true, msg = "Thank you for your subscription!" });

                return Json(new { success = false, msg = "Send Email Failed!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Send Email Failed!" });
            }

        }

        private bool SendEmail(string subject, string bodyContent)
        {

            MailMessage message = new System.Net.Mail.MailMessage();
            string fromEmail = "customer@igudang.com";
            string password = "cust@101017";
            string toEmail = "customer@igudang.com";
            message.From = new MailAddress(fromEmail);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = bodyContent;
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            using (SmtpClient smtpClient = new SmtpClient("mail.igudang.com", 587))
            {
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(fromEmail, password);

                smtpClient.Send(message.From.ToString(), message.To.ToString(), message.Subject, message.Body);
               
            }



            return true;
        }




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
        public static Data FromJson(string json) => JsonConvert.DeserializeObject<Data>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Data self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}