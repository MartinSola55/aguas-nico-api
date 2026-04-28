using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AguasNico_Api.Security;

public class RolesHandler(IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<AuthorizeRolesAttribute>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeRolesAttribute requirement)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var role = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(role) || !requirement.Roles.Split(',').Contains(role))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
