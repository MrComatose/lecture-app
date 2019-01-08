using System;
using Microsoft.AspNetCore.Authentication;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KovalukApp.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace KovalukApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("Config.json")
                .Build();
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RazorViewEngineOptions>(opt => { opt.ViewLocationExpanders.Add(new EmailTemplateExpender()); });

            services.AddDbContext<ApplicationContext>(options =>
                 options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<IUserRepository, UserDataRepository>();
            services.AddTransient<IGroupsRepository, EFGroupRepository>();
            services.AddTransient<IDocStorage, DocPagesRepository>();
            services.AddTransient<IFileStorage, EFFileStorage>();
            services.AddTransient<INewsStorage, NewsRepository>();
            services.AddTransient<AppMailSender>();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddScoped<RazorViewToStringRenderer>();
            services.AddIdentity<User, IdentityRole>()
                  .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationContext>();
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.ReturnUrlParameter = "returnUrl";
                options.LogoutPath = "/Account/LogOut";
                options.AccessDeniedPath = "/";


            });
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;
                
                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters += "йцукенгшщзхїєждлорпавіфячсмитьбю" + "йцукенгшщзхїєждлорпавіфячсмитьбю".ToUpper();
                // User settings
                options.User.RequireUniqueEmail = true;

            });
            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

            services.AddMvc()
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider service)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            app.UseStaticFiles();
            app.UseAuthentication();
            var supportedCultures = new[]{
                 new CultureInfo("en-US"),
                 new CultureInfo("uk"),
                   new CultureInfo("ru-RU"),
                };
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("uk"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures,
                
            };

            var cookieProvider = localizationOptions.RequestCultureProviders
    .OfType<CookieRequestCultureProvider>()
    .First();
            // Set the new cookie name
            cookieProvider.CookieName = "UserCulture";
            app.UseRequestLocalization(localizationOptions);
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                  name: "areas",
                  template: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );

                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id}",
                    defaults: new { controller = "Home", action = "Main", id = 0 }
                    );

            });
            UserManager<User> appManager = service.GetService<UserManager<User>>();
            RoleManager<IdentityRole> appRoles = service.GetService<RoleManager<IdentityRole>>();
            ApplicationContext.CreateTeacherAccount(appManager, appRoles, Configuration).Wait();
        }
    }
}
