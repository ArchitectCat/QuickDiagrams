using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuickDiagrams.Storage.Migrations;
using System.Threading;
using System.Threading.Tasks;

namespace QuickDiagrams.Api
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Run migrations
            using (var scope = host.Services.CreateScope())
            {
                var migrationRunner = scope.ServiceProvider.GetRequiredService<IDataMigrationRunner>();
                var migrations = scope.ServiceProvider.GetServices<IDataMigration>();

                await migrationRunner.RunMigrationsAsync(migrations, CancellationToken.None);
            }

            await host.RunAsync();
        }
    }
}