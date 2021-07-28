using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using EventHub.Admin.Organizations;
using Microsoft.AspNetCore.Components.Web;
using Volo.Abp.Application.Dtos;

namespace EventHub.Admin.Web.Pages
{
    public partial class OrganizationManagement
    {
        private IReadOnlyList<OrganizationInListDto> OrganizationList { get; set; }
        private OrganizationListFilterDto Filter { get; set; }
        private int CurrentPage { get; set; }
        private string CurrentSorting { get; set; }
        private int TotalCount { get; set; }
        private int PageSize { get; }

        private OrganizationProfileDto Organization { get; set; }
        private string ProfileImageUrl { get; set; }
        
        private Guid EditingOrganizationId { get; set; }
        private UpdateOrganizationDto EditingOrganization { get; set; }
        private Modal EditOrganizationModal { get; set; }
        private IFileEntry FileEntry { get; set; }
        private bool IsLoadingProfileImage { get; set; }
        private string SelectedTabInEditModal { get; set; }
        
        public OrganizationManagement()
        {
            Filter = new OrganizationListFilterDto();
            PageSize = LimitedResultRequestDto.DefaultMaxResultCount;

            OrganizationList = new List<OrganizationInListDto>();
            EditingOrganization = new UpdateOrganizationDto();
            IsLoadingProfileImage = false;
            SelectedTabInEditModal = TabContentInEditModal.OrganizationProfile.ToString();
        }

        protected override async Task OnInitializedAsync()
        {
            await GetOrganizationsAsync();
        }

        private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<OrganizationInListDto> e)
        {
            CurrentSorting = e.Columns
                .Where(c => c.Direction != SortDirection.None)
                .Select(c => c.Field + (c.Direction == SortDirection.Descending ? " DESC" : ""))
                .JoinAsString(",");
            CurrentPage = e.Page - 1;
            await GetOrganizationsAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async Task GetOrganizationsAsync()
        {
            Filter.MaxResultCount = PageSize;
            Filter.SkipCount = CurrentPage * PageSize;
            Filter.Sorting = CurrentSorting;

            var result = await OrganizationAppService.GetListAsync(Filter);
            OrganizationList = result.Items;
            TotalCount = (int) result.TotalCount;
        }

        private async Task OpenEditOrganizationModal(OrganizationInListDto input)
        {
            EditingOrganizationId = input.Id;
            Organization = await OrganizationAppService.GetAsync(EditingOrganizationId);
            FillProfileImageUrl(Organization.ProfilePictureContent);
            
            EditingOrganization = ObjectMapper.Map<OrganizationProfileDto, UpdateOrganizationDto>(Organization);
            EditOrganizationModal.Show();
        }

        private async Task UpdateOrganizationAsync()
        {
            await OrganizationAppService.UpdateAsync(EditingOrganizationId, EditingOrganization);
            await GetOrganizationsAsync();
            EditOrganizationModal.Hide();
        }

        private async Task DeleteOrganizationAsync(OrganizationInListDto organization)
        {
            await OrganizationAppService.DeleteAsync(organization.Id);
            await GetOrganizationsAsync();
        }
        
        private async Task OnProfileImageFileChanged(FileChangedEventArgs e)
        {
            IsLoadingProfileImage = true;
            
            if (e.Files != null && e.Files.Any())
            { 
                FileEntry = e.Files[0];
                
                if (FileEntry != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        await FileEntry.WriteToStreamAsync(stream);

                        stream.Seek(0, SeekOrigin.Begin);
                        EditingOrganization.ProfilePictureContent = stream.ToArray();
                        FillProfileImageUrl(EditingOrganization.ProfilePictureContent);
                        await InvokeAsync(StateHasChanged);
                    }
                }
            }
        }
        
        private void FillProfileImageUrl(byte[] content)
        {
            if (content != null)
            {
                var imageBase64Data = Convert.ToBase64String(content);
                var imageDataUrl = $"data:image/png;base64,{imageBase64Data}";
                ProfileImageUrl = imageDataUrl;
            }
        }
        
        private void OnProgressedForProfileImage(FileProgressedEventArgs e)
        {
            if (e.Percentage == 100D)
            {
                IsLoadingProfileImage = false;
            }
        }

        private void OnSelectedTabChangedInEditModal(string name)
        {
            SelectedTabInEditModal = name;
        }
        
        private void OnEditModalClosing(CancelEventArgs e)
        {
            IsLoadingProfileImage = false;
            SelectedTabInEditModal = TabContentInEditModal.OrganizationProfile.ToString();
            
        }

        private async Task OnKeyPress(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                await GetOrganizationsAsync();
            }
        }
    }
    
    public enum TabContentInEditModal : byte
    {
        OrganizationProfile = 0,
        ProfileImage
    }
}