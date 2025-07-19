using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;

namespace XtremeIdiots.Portal.Web.Helpers
{
    /// <summary>
    /// Tag helper for conditionally rendering content based on authorization policies
    /// </summary>
    [HtmlTargetElement(Attributes = "policy")]
    public class PolicyTagHelper : TagHelper
    {
        private readonly IAuthorizationService _authService;
        private readonly ClaimsPrincipal _principal;

        public PolicyTagHelper(IAuthorizationService authService, IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            // HttpContext should not be null in normal request flow
            _principal = httpContextAccessor.HttpContext?.User ?? throw new InvalidOperationException("HttpContext is not available");
        }

        public required string Policy { get; set; }

        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!(await _authService.AuthorizeAsync(_principal, Policy)).Succeeded)
                output.SuppressOutput();
        }
    }
}