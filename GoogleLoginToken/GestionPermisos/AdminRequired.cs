using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GoogleLoginToken.GestionPermisos
{
    // A handler that can determine whether a MaximumOfficeNumberRequirement is satisfied
    public class AdminAuthorizationHandler : AuthorizationHandler<AdminRequirement>
    {
        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            const string CLAIMTYPE = nameof(UserInfo.Permisos);

            System.Text.Json.JsonElement permisos;

            Claim claim = context.User.FindFirst(c => c.Type == CLAIMTYPE);
            // Bail out if the office number claim isn't present

            if (!Equals(claim, default(Claim)))
            {
                permisos = System.Text.Json.JsonDocument.Parse(claim.Value).RootElement;
                // Finally, validate that the office number from the claim is not greater
                // than the requirement's maximum
                if (permisos.EnumerateArray().Any(p => Equals(p.GetString(), Permiso.ADMIN)))
                {
                    // Mark the requirement as satisfied
                    context.Succeed(requirement);
                }
            }

        }
    }

    // A custom authorization requirement which requires office number to be below a certain value
    public class AdminRequirement : IAuthorizationRequirement
    {

        public const string POLICITY = "AdminIsRequired";

    }
}
