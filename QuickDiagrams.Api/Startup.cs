using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuickDiagrams.Api.Data;
using QuickDiagrams.Api.Data.Migrations;
using QuickDiagrams.Api.Data.TypeHandlers;
using QuickDiagrams.Api.Identity;
using QuickDiagrams.Api.Identity.Models;

namespace QuickDiagrams.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private void ConfigureDapper()
        {
            SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.AddTypeHandler(new TimeSpanTypeHandler());
        }

        private void RegisterMigrations(IServiceCollection services)
        {
            services.AddSingleton<IDataMigration, InitialSeed>();
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IDatabaseConnectionFactory, SqliteDatabaseConnectionFactory>();
            services.AddSingleton<IDataMigrationRunner, SqliteDataMigrationRunner>();

            RegisterMigrations(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseHttpsRedirection()
                .UseAuthentication();

            app
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureDapper();

            services
                .AddAuthentication()
                .AddGoogle(options =>
                {
                    var googleConfig = Configuration.GetSection("Authentication:Google");
                    options.ClientId = googleConfig["ClientId"];
                    options.ClientSecret = googleConfig["ClientSecret"];
                    options.CallbackPath = "/signin-google";
                });

            services.AddSingleton<IUserStore<ApplicationUser>, UserStore>();
            services.AddSingleton<IRoleStore<ApplicationRole>, RoleStore>();

            services
                .AddIdentity<ApplicationUser, ApplicationRole>()
                .AddDefaultTokenProviders();

            services.AddControllers();

            RegisterServices(services);
        }
    }
}