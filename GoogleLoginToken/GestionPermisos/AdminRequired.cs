using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GoogleLoginToken.GestionPermisos
{
    // A handler that can determine whether a MaximumOfficeNumberRequirement is satisfied
    internal class AdminAuthorizationHandler : AuthorizationHandler<AdminRequirement>
    {
        LoginContext Context { get; set; }
        public AdminAuthorizationHandler(LoginContext context) => Context = context;
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            const string CLAIMTYPE = "email";
            const string CLAIMISSUER = "http://localhost:5000/";

            Claim claim;
            Task result=Task.CompletedTask;
            // Bail out if the office number claim isn't present
            if (context.User.HasClaim(c => c.Issuer == CLAIMISSUER && c.Type == CLAIMTYPE))
            {
                claim = context.User.FindFirst(c => c.Issuer == CLAIMISSUER && c.Type == CLAIMTYPE);
                if (!Equals(claim, default(Claim)))
                {

                    // Finally, validate that the office number from the claim is not greater
                    // than the requirement's maximum
                    if (Context.GetUserWithEmailOrDefault(claim.Value).IsAdmin)
                    {
                        // Mark the requirement as satisfied
                        context.Succeed(requirement);
                    }
                }


            }

            return result;
        }
    }

    // A custom authorization requirement which requires office number to be below a certain value
    public class AdminRequirement : IAuthorizationRequirement
    {

        public const string POLICITY= "AdminIsRequired";

    }
}
