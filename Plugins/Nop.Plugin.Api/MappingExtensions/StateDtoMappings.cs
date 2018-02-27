using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTOs.States;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class StateDtoMappings
    {
        public static StateDto ToDto(this StateProvince state)
        {
            return state.MapTo<StateProvince, StateDto>();
        }

        public static StateProvince ToEntity(this StateDto stateDto)
        {
            return stateDto.MapTo<StateDto, StateProvince>();
        }
    }
}