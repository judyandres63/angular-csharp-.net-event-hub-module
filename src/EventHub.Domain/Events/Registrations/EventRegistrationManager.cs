﻿using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Timing;

namespace EventHub.Events.Registrations
{
    public class EventRegistrationManager : IDomainService
    {
        private readonly IEventRegistrationRepository _eventRegistrationRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IClock _clock;

        public EventRegistrationManager(
            IEventRegistrationRepository eventRegistrationRepository,
            IGuidGenerator guidGenerator,
            IClock clock)
        {
            _eventRegistrationRepository = eventRegistrationRepository;
            _guidGenerator = guidGenerator;
            _clock = clock;
        }

        public async Task RegisterAsync(
            Event @event,
            IdentityUser user)
        {
            if (IsPastEvent(@event))
            {
                throw new BusinessException(EventHubErrorCodes.CantRegisterOrUnregisterForAPastEvent);
            }

            if (await IsRegisteredAsync(@event, user))
            {
                return;
            }
            
            var registrationCount = await _eventRegistrationRepository.GetCountAsync(@event.Id);

            if (@event.Capacity != null &&  registrationCount >= @event.Capacity)
            {
                throw new BusinessException(EventHubErrorCodes.CapacityOfEventFull)
                    .WithData("EventTitle", @event.Title);
            }
                
            await _eventRegistrationRepository.InsertAsync(
                new EventRegistration(
                    _guidGenerator.Create(),
                    @event.Id,
                    user.Id
                )
            );
        }

        public async Task UnregisterAsync(
            Event @event,
            IdentityUser user)
        {
            if (IsPastEvent(@event))
            {
                throw new BusinessException(EventHubErrorCodes.CantRegisterOrUnregisterForAPastEvent);
            }

            await _eventRegistrationRepository.DeleteAsync(
                x => x.EventId == @event.Id && x.UserId == user.Id
            );
        }

        public async Task<bool> IsRegisteredAsync(
            Event @event,
            IdentityUser user)
        {
            return await _eventRegistrationRepository
                .ExistsAsync(@event.Id, user.Id);
        }

        public bool IsPastEvent(Event @event)
        {
            return _clock.Now > @event.EndTime;
        }
    }
}
