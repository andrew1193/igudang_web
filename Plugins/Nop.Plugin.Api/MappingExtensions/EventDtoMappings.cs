using Nop.Plugin.Api.AutoMapper;
using Nop.Core.Domain.Events;
using Nop.Plugin.Api.DTOs.Events;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class EventDtoMappings
    {
        public static EventDto ToDto(this Event ev)
        {
            return ev.MapTo<Event, EventDto>();
        }

        public static Event ToEntity(this EventDto eventDto)
        {
            return eventDto.MapTo<EventDto, Event>();
        }
    }
}
