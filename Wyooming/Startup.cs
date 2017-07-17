using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wyooming.Data;
using Wyooming.Models;
using Wyooming.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Wyooming.Authorization;

namespace Wyooming
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("WyoomingConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var _testSecret = Configuration["MySecret"];

            services.AddMvc();


            // Authorization handlers.
            services.AddScoped<IAuthorizationHandler,
                ContactIsOwnerAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler,
                ContactManagerAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler,
                ContactAdministratorsAuthorizationHandler>();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddDbContext<WyoomingContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("WyoomingConnection")));


                    var skipSSL = Configuration.GetValue<bool>("LocalTest:skipSSL");

                   services.Configure<MvcOptions>(options =>
                    {
                        if(!skipSSL)
            {
                            options.Filters.Add(new RequireHttpsAttribute());
                        }
                    });

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseIdentity();

            var testUserPw = Configuration["MySecret"];
            if (String.IsNullOrEmpty(testUserPw))
            {
                throw new System.Exception("Use secrets manager to set SeedUserPW \n" +
                                           "dotnet user-secrets set SeedUserPW <pw>");
            }


            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            try
            {
                seedData.Initialize(app.ApplicationServices, testUserPw).Wait();
            }
            catch
            {
                throw new System.Exception("You need to update the DB "
                    + "\nPM > Update-Database " + "\n or \n" +
                      "> dotnet ef database update"
                      + "\nIf that doesn't work, comment out SeedData and register a new user");
            }
        }
    }
}
