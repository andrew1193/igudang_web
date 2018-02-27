using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Data;
using Nop.Plugin.Api.Models.StoresParameters;

namespace Nop.Plugin.Api.Services
{
    public class StoreApiService : IStoreApiService
    {
        private readonly IDbContext _dbContext;

        public StoreApiService(
            IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public SliderModel GetSliderById(int sliderId)
        {
            var result = _dbContext.SqlQuery<SliderModel>("SELECT * FROM [Nopcommerce].[dbo].[SS_AS_AnywhereSlider] where Id={0}", sliderId).FirstOrDefault();
            if (result == null)
                return null;
            return result;
        }

        public List<SliderImageModel> GetImagesBySliderId(int sliderId)
        {
            var results = _dbContext.SqlQuery<SliderImageModel>("SELECT * FROM [Nopcommerce].[dbo].[SS_AS_SliderImage] where SliderId={0}", sliderId).ToList();
            if (results.Count() <= 0)
                return null;
            return results;
        }
    }
}
