using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core
{
    public partial class GlobalValue
    {
        public class DefaultCustomerRoleIds
        {
            public const int Superadmin = 1;

            public const int ForumModerator = 2;

            public const int Registered = 3;

            public const int Guest = 4;

            public const int Workshop = 5;

            public const int HwxAdmin = 8;

            public const int Distributor = 9;

            public const int Branch = 11;
        }

        public class BaiduApi
        {
            public const string secretKey = "hzg6W736P9EKd3rlbimELMRhS9Gm9290";
        }

        public class DefaultCountryIds
        {
            public const int China = 21;
        }

        public class DefaultAddress
        {
            public const int DefaultAddressId = 3913;
        }
    }
}
