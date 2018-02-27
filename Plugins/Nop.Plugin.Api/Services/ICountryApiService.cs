using System;
using System.Collections.Generic;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.Constants;

namespace Nop.Plugin.Api.Services
{
    public interface ICountryApiService
    {
        List<Country> GetCountries(DateTime? createdAtMin = null, DateTime? createdAtMax = null,
                                                    DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, int limit = Configurations.DefaultLimit,
                                                    int page = Configurations.DefaultPageValue);

        Country GetCountry(int id);
    }
}
