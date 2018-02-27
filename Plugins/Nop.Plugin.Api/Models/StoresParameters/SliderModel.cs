using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.StoresParameters
{
    public class SliderModel
    {
        public int Id { get; set; }

        public string SystemName { get; set; }

        public int SliderType { get; set; }

        public int LanguageId { get; set; }

        public bool LimitedToStores { get; set; }
    }
}
