using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.DataStructures;
using Nop.Core.Domain.Directory;

namespace Nop.Plugin.Api.Services
{
    public class StateApiService : IStateApiService
    {
        private readonly IRepository<StateProvince> _statesRepository;

        public StateApiService(IRepository<StateProvince> statesRepository)
        {
            _statesRepository = statesRepository;
        }

        public List<StateProvince> GetStates(int? countryId = null, int limit = Configurations.DefaultLimit,
                                             int page = Configurations.DefaultPageValue)
        {
            IQueryable<StateProvince> query = GetStatesQuery(countryId);

            return new ApiList<StateProvince>(query, page - 1, limit);
        }

        public StateProvince GetState(int id)
        {
            return _statesRepository.GetById(id);
        }

        private IQueryable<StateProvince> GetStatesQuery(int? countryId = null)
        {
            var query = _statesRepository.TableNoTracking;

            if (countryId != null)
            {
                query = query.Where(state => state.CountryId == countryId);
            }

            query = query.OrderBy(state => state.Name);

            return query;
        }
    }
}
