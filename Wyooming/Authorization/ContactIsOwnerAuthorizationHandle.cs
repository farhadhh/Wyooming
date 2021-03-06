﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Wyooming.Data;
using Wyooming.Models;

namespace Wyooming.Authorization
{
    public class ContactIsOwnerAuthorizationHandler
        : AuthorizationHandler<OperationAuthorizationRequirement, Contact>
    {
        UserManager<ApplicationUser> _userManager;

        public ContactIsOwnerAuthorizationHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected override Task
           HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement, Contact resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.FromResult(0);
            }

            if (requirement.Name != Constants.CreateOperationName &&
                requirement.Name != Constants.ReadOperationName && 
                requirement.Name != Constants.UpdateOperationName &&
                requirement.Name != Constants.DeleteOperationName)
            {
                return Task.FromResult(0);
            }

            if (resource.OwnerID == _userManager.GetUserId(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}
