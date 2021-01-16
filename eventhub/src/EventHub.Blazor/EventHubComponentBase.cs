﻿using EventHub.Localization;
using Volo.Abp.AspNetCore.Components;

namespace EventHub.Blazor
{
    public abstract class EventHubComponentBase : AbpComponentBase
    {
        protected EventHubComponentBase()
        {
            LocalizationResource = typeof(EventHubResource);
        }
    }
}
