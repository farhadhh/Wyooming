﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wyooming.Models;

namespace Wyooming.Data
{
    public static class seedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {
            using (var context = new WyoomingContext(
                serviceProvider.GetRequiredService<DbContextOptions<WyoomingContext>>()))
            {
                var uid = await CreateTestUser(serviceProvider, testUserPw);
                SeedDB(context, uid);
            }


        }

        private static async Task<string> CreateTestUser(IServiceProvider serviceProvider, string testUserPw)
        {
            if (String.IsNullOrEmpty(testUserPw))
            {
                return "";
            }

            const string SeedUserName = "test@example.com";

            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByNameAsync(SeedUserName);
            if (user == null)
            {
                user = new ApplicationUser { UserName = SeedUserName };
                await userManager.CreateAsync(user, testUserPw);
            }

            return user.Id;
        }

        public static void SeedDB(WyoomingContext context, string adminID)
        {
            if (context.Contact.Any())
            {
                return;
            }

            context.Contact.AddRange(
                new Contact
                {
                    Name = "Debra Garcia",
                    Address = "1234 Main St",
                    City = "Redmond",
                    State = "WA",
                    Zip = "10999",
                    Email = "debra@example.com",
                    Status = ContactStatus.Approved,
                    OwnerID = adminID
                },
                new Contact
                {
                    Name = "Thorsten Weinrich",
                    Address = "5678 1st Ave W",
                    City = "Redmond",
                    State = "WA",
                    Zip = "10999",
                    Email = "thorsten@example.com",
                    Status = ContactStatus.Approved,
                    OwnerID = adminID
                },
             new Contact
             {
                 Name = "Yuhong Li",
                 Address = "9012 State st",
                 City = "Redmond",
                 State = "WA",
                 Zip = "10999",
                 Email = "yuhong@example.com",
                 Status = ContactStatus.Approved,
                 OwnerID = adminID
             },
             new Contact
             {
                 Name = "Jon Orton",
                 Address = "3456 Maple St",
                 City = "Redmond",
                 State = "WA",
                 Zip = "10999",
                 Email = "jon@example.com",
                 Status = ContactStatus.Approved,
                 OwnerID = adminID
             },
             new Contact
             {
                 Name = "Diliana Alexieva-Bosseva",
                 Address = "7890 2nd Ave E",
                 City = "Redmond",
                 State = "WA",
                 Zip = "10999",
                 Email = "diliana@example.com",
                 Status = ContactStatus.Approved,
                 OwnerID = adminID
             }
             );
            context.SaveChanges();
        }
    }
}
