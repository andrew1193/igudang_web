using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.DataStructures;
using Nop.Core;
using Nop.Core.Domain.Directory;

namespace Nop.Plugin.Api.Services
{
    public class CountryApiService : ICountryApiService
    {
        private readonly IRepository<Country> _countriesRepository;
        private readonly IStoreContext _storeContext;

        public CountryApiService(IRepository<Country> countriesRepository, IStoreContext storeContext)
        {
            _countriesRepository = countriesRepository;
            _storeContext = storeContext;
        }

        public List<Country> GetCountries(DateTime? createdAtMin = null, DateTime? createdAtMax = null,
                                                           DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, int limit = Configurations.DefaultLimit,
                                                           int page = Configurations.DefaultPageValue)
        {
            IQueryable<Country> query = GetCountriesQuery();

            return new ApiList<Country>(query, page - 1, limit);
        }

        public Country GetCountry(int id)
        {
            return _countriesRepository.GetById(id);
        }

        private IQueryable<Country> GetCountriesQuery()
        {
            var query = _countriesRepository.TableNoTracking.Where(x => x.Published == true);


            query = query.OrderBy(country => country.Id);

            return query;
        }
    }
}
