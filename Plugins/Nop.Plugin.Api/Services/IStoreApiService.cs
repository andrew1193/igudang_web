using Nop.Plugin.Api.Models.StoresParameters;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Services
{
    public interface IStoreApiService
    {
        SliderModel GetSliderById(int sliderId);

        List<SliderImageModel> GetImagesBySliderId(int sliderId);
    }
}
