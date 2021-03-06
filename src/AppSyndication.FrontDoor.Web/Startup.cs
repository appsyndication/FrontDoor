﻿using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using AppSyndication.BackendModel.Data;
using AppSyndication.BackendModel.IndexedData;
using AppSyndication.FrontDoor.Web.Models;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSyndication.FrontDoor.Web
{
    public class Startup
    {
        public Startup()
        {
            this.Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = this.Configuration["Connection:StorageConnectionString"];

            services.Configure<OpenIdConnectConfig>(this.Configuration.GetSection("OpenIdConnect"));

            services.AddAuthentication(sharedOptions =>
                sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddTagStorage(connectionString)
                .AddTagIndex();

            services.ConfigureRouting(options =>
            {
                options.AppendTrailingSlash = true;
                options.LowercaseUrls = true;
            });

            services.ConfigureRouting(options =>
            {
                options.LowercaseUrls = true;
                options.AppendTrailingSlash = true;
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<OpenIdConnectConfig> oidcConfig)
        {
            var sourceSwitch = new SourceSwitch("AppSyndicationFrontDoorWebTraceSource") { Level = SourceLevels.Warning };
            loggerFactory.AddTraceSource(sourceSwitch, new AzureApplicationLogTraceListener());
            loggerFactory.AddTraceSource(sourceSwitch, new EventLogTraceListener("Application"));

#if DEBUG
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
#endif

            if (env.IsDevelopment())
            {
                //app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseIISPlatformHandler();

            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthenticate = true;
            });

            app.UseOpenIdConnectAuthentication(options =>
            {
                options.AutomaticChallenge = true;

                options.Authority = oidcConfig.Value.Authority;
                options.ClientId = oidcConfig.Value.ClientId;
                options.ClientSecret = oidcConfig.Value.ClientSecret;
                options.CallbackPath = "/account/signin-oidc";

                options.ResponseType = OpenIdConnectResponseTypes.Code;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.Scope.Add("email");
                options.Scope.Add("roles");
                options.Scope.Add("channels");
                options.TokenValidationParameters.NameClaimType = "sub";

#if DEBUG
                options.RequireHttpsMetadata = false;
#endif
                //options.PostLogoutRedirectUri = "https://www.appsyndication.com/";
            });

            app.UseStaticFiles();

            app.UseMvc(
            //    routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //}
            );
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
