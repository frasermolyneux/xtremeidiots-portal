using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;

namespace XtremeIdiots.Portal.Web.Helpers;

[HtmlTargetElement(Attributes = "policy")]
public class PolicyTagHelper(IAuthorizationService authService, IHttpContextAccessor httpContextAccessor) : TagHelper
{
    private readonly IAuthorizationService authService = authService;
    private readonly ClaimsPrincipal principal = httpContextAccessor.HttpContext?.User ?? throw new InvalidOperationException("HttpContext is not available");

    public required string Policy { get; set; }

    public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (!(await authService.AuthorizeAsync(principal, Policy)).Succeeded)
            output.SuppressOutput();
    }
}