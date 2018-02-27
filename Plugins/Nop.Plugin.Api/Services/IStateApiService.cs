using System.Collections.Generic;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.Constants;

namespace Nop.Plugin.Api.Services
{
    public interface IStateApiService
    {
        List<StateProvince> GetStates(int? countryId = null, int limit = Configurations.DefaultLimit,
                                      int page = Configurations.DefaultPageValue);

        StateProvince GetState(int id);
    }
}
