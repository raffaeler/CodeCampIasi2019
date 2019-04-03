using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationLibrary
{
    public class ListHandler : AuthorizationHandler<ListRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ListRequirement requirement)
        {
            var claims = context.User.Claims
                .Where(c => c.Type.StartsWith("Article"))
                .ToList();

            // claim.Value contains zero or more "LCRUD" chars
            var claim = claims.FirstOrDefault();

            if (claim != null && claim.Value.Contains('L'))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;

        }
    }
}
