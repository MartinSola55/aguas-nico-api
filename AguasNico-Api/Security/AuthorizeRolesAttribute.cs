using Microsoft.AspNetCore.Authorization;

namespace AguasNico_Api.Security;

public class AuthorizeRolesAttribute(params string[] roles) : IAuthorizationRequirement
{
    public string Roles { get; } = string.Join(",", roles);
}
