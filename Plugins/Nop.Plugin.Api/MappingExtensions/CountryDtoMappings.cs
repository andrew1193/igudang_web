using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTOs.Countries;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class CountryDtoMappings
    {
        public static CountryDto ToDto(this Country country)
        {
            return country.MapTo<Country, CountryDto>();
        }

        public static Country ToEntity(this CountryDto countryDto)
        {
            return countryDto.MapTo<CountryDto, Country>();
        }
    }
}
