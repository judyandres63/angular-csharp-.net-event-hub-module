﻿namespace EventHub
{
    public static class EventHubErrorCodes
    {
        public const string OrganizationNameAlreadyExists = "EventHub:OrganizationNameAlreadyExists";
        public const string NotAuthorizedToCreateEventInThisOrganization = "EventHub:NotAuthorizedToCreateEventInThisOrganization";
        public const string EventEndTimeCantBeEarlierThanStartTime = "EventHub:EventEndTimeCantBeEarlierThanStartTime";
        public const string CantRegisterOrUnregisterForAPastEvent = "EventHub:CantRegisterOrUnregisterForAPastEvent";
        public const string NotAuthorizedToUpdateOrganizationProfile = "EventHub:NotAuthorizedToUpdateOrganizationProfile";
        public const string CapacityOfEventFull = "EventHub:CapacityOfEventFull";
    }
}
