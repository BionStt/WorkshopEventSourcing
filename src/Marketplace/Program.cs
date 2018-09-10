﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using static System.Environment;
using static System.Reflection.Assembly;

namespace Marketplace
{
    public static class Program
    {
        static Program() =>
            CurrentDirectory = Path.GetDirectoryName(GetEntryAssembly().Location);

        static async Task<int> Main(string[] args)
        {
            await Console.Error.WriteLineAsync(
                $"Application starting ({GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"})...");

            try
            {
                var configuration = BuildConfiguration(args);

                await Console.Error.WriteLineAsync("Configuration built successfully.");

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

                await ConfigureWebHost(configuration).Build().RunAsync();

                return 0;
            }
            catch (Exception ex)
            {
                if (!Log.IsEnabled(LogEventLevel.Fatal))
                    await Console.Error.WriteLineAsync($"Terminated unexpectedly! {ex.Message}");
                else
                    Log.Fatal(ex, "Terminated unexpectedly!");

                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IConfiguration BuildConfiguration(string[] args)
            => new ConfigurationBuilder()
                .SetBasePath(CurrentDirectory)
                .AddJsonFile("appsettings.json", false, false)
                .AddJsonFile($"appsettings.{GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

        public static IWebHostBuilder ConfigureWebHost(IConfiguration configuration)
            => new WebHostBuilder()
                .UseStartup<Startup>()
                .UseConfiguration(configuration)
                .ConfigureServices(services => services.AddSingleton(configuration))
                .UseContentRoot(CurrentDirectory)
                .UseKestrel();
    }
}
