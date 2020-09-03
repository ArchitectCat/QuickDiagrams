using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuickDiagrams.Api.Data;
using QuickDiagrams.Api.Data.Migrations;
using QuickDiagrams.Api.Data.TypeHandlers;

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

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureDapper();

            services.AddControllers();

            services
                .AddAuthentication()
                .AddGoogle(options =>
                {
                    var googleConfig = Configuration.GetSection("Authentication:Google");
                    options.ClientId = googleConfig["ClientId"];
                    options.ClientSecret = googleConfig["ClientSecret"];
                    options.CallbackPath = "/signin-google";
                });

            RegisterServices(services);
        }
    }
}