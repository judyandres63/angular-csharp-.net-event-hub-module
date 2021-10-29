﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Payment.Admin.Payments;
using Volo.Abp.Application.Dtos;
using Payment.PaymentRequests;

namespace Payment.Admin.Pages.Payment.Requests
{
    public partial class Index
    {
        [Inject] 
        protected IPaymentRequestAdminAppService PaymentRequestAdminAppService { get; set; }
        
        protected IReadOnlyList<PaymentRequestWithDetailsDto> PaymentRequests { get; set; }

        protected PaymentRequestGetListInput GetListInput { get; set; }

        protected virtual int PageSize { get; } = LimitedResultRequestDto.DefaultMaxResultCount;

        protected int CurrentPage { get; set; } = 1;

        protected string CurrentSorting { get; set; }

        protected int? TotalCount { get; set; }

        public Index()
        {
            GetListInput = new();
        }
        
        protected override async Task OnInitializedAsync()
        {
            await GetPaymentRequestsAsync();
            await InvokeAsync(StateHasChanged);
        }

        protected virtual async Task GetPaymentRequestsAsync()
        {
            try
            {
                GetListInput.Sorting = CurrentSorting;
                GetListInput.SkipCount = (CurrentPage - 1) * PageSize;
                GetListInput.MaxResultCount = PageSize;

                var result = await PaymentRequestAdminAppService.GetListAsync(GetListInput);
                PaymentRequests = result.Items.As<IReadOnlyList<PaymentRequestWithDetailsDto>>();
                TotalCount = (int?) result.TotalCount;
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
            }
        }

        protected virtual async Task OnDataGridReadAsync(DataGridReadDataEventArgs<PaymentRequestWithDetailsDto> e)
        {
            CurrentSorting = e.Columns
                .Where(c => c.SortDirection != SortDirection.None)
                .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
                .JoinAsString(",");

            CurrentPage = e.Page;

            await GetPaymentRequestsAsync();
            await InvokeAsync(StateHasChanged);
        }
        
        private async Task OnKeyPress(KeyboardEventArgs e)
        {
            if (e.Code is "Enter" or "NumpadEnter")
            {
                await GetPaymentRequestsAsync();
            }
        }
        
        private async Task OnSelectedDateChangedForMinCreationTime(DateTime? minCreationTime)
        {
            GetListInput.MinCreationTime = minCreationTime;
            await GetPaymentRequestsAsync();
        }

        private async Task OnSelectedDateChangedForMaxCreationTime(DateTime? maxCreationTime)
        {
            GetListInput.MaxCreationTime = maxCreationTime;
            await GetPaymentRequestsAsync();
        }
        
        private async Task OnPaymentRequestStateChanged(PaymentRequestState? status)
        {
            Console.WriteLine("Status changed!");
            Console.WriteLine(status);
            
            GetListInput.Status = status;
            await GetPaymentRequestsAsync();
        }
    }
}