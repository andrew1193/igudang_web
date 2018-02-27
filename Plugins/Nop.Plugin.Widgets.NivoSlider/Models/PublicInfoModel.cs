using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Widgets.NivoSlider.Models
{
    public class PublicInfoModel : BaseNopModel
    {
        public string Picture1Url { get; set; }
        public string Text1 { get; set; }
        public string Link1 { get; set; }

        public string Picture2Url { get; set; }
        public string Text2 { get; set; }
        public string Link2 { get; set; }

        public string Picture3Url { get; set; }
        public string Text3 { get; set; }
        public string Link3 { get; set; }

        public string Picture4Url { get; set; }
        public string Text4 { get; set; }
        public string Link4 { get; set; }

        public string Picture5Url { get; set; }
        public string Text5 { get; set; }
        public string Link5 { get; set; }


        public string RightPicture1Url { get; set; }
        public string RightText1 { get; set; }
        public string RightLink1 { get; set; }


        public string RightPicture2Url { get; set; }
        public string RightText2 { get; set; }
        public string RightLink2 { get; set; }

        public int EventDay { get; set; } = 0;
        public int EventMonth { get; set; } = 0;
        public int EventYear { get; set; } = 0;
        public int EventHour { get; set; } = 0;
        public int EventMinute { get; set; } = 0;
        public int EventSec { get; set; } = 0;
    }
}