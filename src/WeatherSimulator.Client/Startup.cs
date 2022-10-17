using WeatherSimulator.Client.Configurations;
using WeatherSimulator.Client.LimiterInfrastructure;
using WeatherSimulator.Client.Mappers;
using WeatherSimulator.Client.Repositories;
using WeatherSimulator.Client.Services;
using WeatherSimulator.Core.Abstractions.Repositories;
using WeatherSimulator.Core.Models;
using WeatherSimulator.Proto;

namespace WeatherSimulator.Client;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }
    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddGrpc();
        serviceCollection.Configure<SubscriptionConfiguration>(Configuration.GetSection("Subscription"));
        serviceCollection.Configure<WeatherClientConfiguration>(Configuration.GetSection("WeatherClientConfiguration"));
        
        serviceCollection.AddGrpcClient<WeatherSimulatorService.WeatherSimulatorServiceClient>(
            o =>
            {
                var serverUri = Configuration.GetValue<string>("Subscription:ServerUri");
                o.Address = new Uri(serverUri);
            });
        serviceCollection.AddHostedService<WeatherBackgroundService>();
        serviceCollection.AddHostedService<LimitersRenewBackgroundService>();
        serviceCollection.AddSingleton<ISensorListManager, SensorListManager>();
        serviceCollection.AddSingleton(typeof(IRepository<SensorMeasure>), (_) => 
            new InMemoryRepository<SensorMeasure>());
        serviceCollection.AddScoped<ISensorDataService, SensorDataService>();
        serviceCollection.AddSingleton<ISensorMeasureMapper, SensorMeasureMapper>();
        serviceCollection.AddControllers();

        serviceCollection.AddSingleton<IRateLimiterManager, RateLimiterManager>();
        serviceCollection.AddScoped<RateLimiterAttribute>();
    }

    public void Configure(IApplicationBuilder builder, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            builder.UseDeveloperExceptionPage();
        }

        builder.UseRouting();
        builder.UseEndpoints(x => x.MapControllers());
    }
}