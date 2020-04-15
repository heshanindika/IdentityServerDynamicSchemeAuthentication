// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Data.Migrations.TenantManagement;
using IdentityServerAspNetIdentity.Data.Repository;
using IdentityServerAspNetIdentity.Models;
using IdentityServerAspNetIdentity.Quickstart.Scheme;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Sustainsys.Saml2;
using Sustainsys.Saml2.AspNetCore2;
using Sustainsys.Saml2.Metadata;
using Sustainsys.Saml2.WebSso;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServerAspNetIdentity
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // configures IIS out-of-proc settings (see https://github.com/aspnet/AspNetCore/issues/14882)
            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // configures IIS in-proc settings
            services.Configure<IISServerOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // migration assembly required as DbContext's are in a different assembly
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            const string connectionString = @"Data Source=HESHAN-K;database=IdentityServer4;trusted_connection=yes;User ID=fmsi_lts;Password=fmsi1234;";

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddDbContext<TenantManagementDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddAspNetIdentity<ApplicationUser>();

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddSingleton<OAuthPostConfigureOptions<GoogleOptions, GoogleHandler>>();
            services.AddSingleton<OAuthPostConfigureOptions<FacebookOptions, FacebookHandler>>();
            services.AddSingleton<OpenIdConnectPostConfigureOptions>();
            services.AddSingleton<PostConfigureSaml2Options>();

            services.AddAuthentication();


            

            services.AddScoped<ITenantInfoRepository, TenantInfoRepository>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                InitializeDatabase(app);
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }


        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var services = new ServiceCollection();
                services.AddLogging();
                services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(@"Data Source=HESHAN-K;database=IdentityServer4;trusted_connection=yes;User ID=fmsi_lts;Password=fmsi1234;"));

                services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

                //using (var serviceProvider = services.BuildServiceProvider())
                //{
                //    using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                //    {
                        var userContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                        userContext.Database.Migrate();

                        var userMgr = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                        var alice = userMgr.FindByNameAsync("ccuser1@cc.com").Result;
                        if (alice == null)
                        {
                            alice = new ApplicationUser
                            {
                                UserName = "ccuser1@cc.com",
                                Email = "ccuser1@cc.com",
                                TenantId = 1
                            };
                            var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                            if (!result.Succeeded)
                            {
                                throw new Exception(result.Errors.First().Description);
                            }

                            result = userMgr.AddClaimsAsync(alice, new Claim[]{
                        new Claim(JwtClaimTypes.Name, "CC User1"),
                        new Claim(JwtClaimTypes.GivenName, "CC"),
                        new Claim(JwtClaimTypes.FamilyName, "User1"),
                        new Claim(JwtClaimTypes.Email, "ccuser1@cc.com"),
                        new Claim("Tenant", "1"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json)
                    }).Result;
                            if (!result.Succeeded)
                            {
                                throw new Exception(result.Errors.First().Description);
                            }
                            Log.Debug("alice created");
                        }
                        else
                        {
                            Log.Debug("alice already exists");
                        }

                        var bob = userMgr.FindByNameAsync("dimuthu@bankofzone.lk").Result;
                        if (bob == null)
                        {
                            bob = new ApplicationUser
                            {
                                UserName = "dimuthu@bankofzone.lk",
                                Email = "dimuthu@bankofzone.lk",
                                TenantId = 2
                            };
                            var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                            if (!result.Succeeded)
                            {
                                throw new Exception(result.Errors.First().Description);
                            }

                            result = userMgr.AddClaimsAsync(bob, new Claim[]{
                        new Claim(JwtClaimTypes.Name, "Dimuthu"),
                        new Claim(JwtClaimTypes.GivenName, "Dimuthu"),
                        new Claim(JwtClaimTypes.FamilyName, "Dimuthu"),
                        new Claim(JwtClaimTypes.Email, "dimuthu@bankofzone.lk"),
                        new Claim("Tenant", "2"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
                        new Claim("location", "somewhere")
                    }).Result;
                            if (!result.Succeeded)
                            {
                                throw new Exception(result.Errors.First().Description);
                            }
                            Log.Debug("bob created");
                        }
                        else
                        {
                            Log.Debug("bob already exists");
                        }
                //    }
                //}


                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                serviceScope.ServiceProvider.GetRequiredService<TenantManagementDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.Ids)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.Apis)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}