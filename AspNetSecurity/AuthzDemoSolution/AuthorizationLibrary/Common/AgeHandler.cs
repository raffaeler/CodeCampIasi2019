using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthorizationLibrary.Common;
using Microsoft.AspNetCore.Authorization;
using ModelLibrary;

namespace AuthorizationLibrary
{
    public class AgeHandler : AuthorizationHandler<AgeRequirement, Article>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            AgeRequirement requirement, Article resource)
        {
            var userMaturity = MaturityHelper.GetMaturity(context.User);

            if (userMaturity >= resource.Maturity)
            {
                context.Succeed(requirement);
            }

            // not allowed because it requires
            // more seniority
            return Task.CompletedTask;
        }

    }
}
