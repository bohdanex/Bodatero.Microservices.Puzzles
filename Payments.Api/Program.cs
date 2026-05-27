using Carter;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Payments.Api.Extensions;
using Payments.Api.Features.Transactions.Consumers;
using Payments.Api.Infrastructure;
using Payments.Api.Infrastructure.Configurations;
using Serilog;
using SerilogTracing;

using var _ = new ActivityListenerConfiguration()
    .Instrument.AspNetCoreRequests()
    .Instrument.SqlClientCommands()
    .TraceToSharedLogger();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();
try
{
    Log.Information($"Starting the {nameof(Payments)} web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, config) =>
    {
        config.ReadFrom.Configuration(builder.Configuration);
        config.ReadFrom.Services(services);
    });

    builder.Logging.Configure(options =>
    {
        options.ActivityTrackingOptions =
            ActivityTrackingOptions.TraceId |
            ActivityTrackingOptions.SpanId |
            ActivityTrackingOptions.ParentId;
    });

    builder.Services.AddHealthChecks();
    builder.Services.AddOpenTelemetry()
        .WithMetrics(options =>
        {
            options.AddPrometheusExporter();
            options.AddMeter([
                "Microsoft.AspNetCore.Hosting",
                "Microsoft.AspNetCore.Server.Kestrel"
            ]);
            options.AddView("request-duration", new ExplicitBucketHistogramConfiguration() { Boundaries = new[] { 0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.3, 0.5, 0.7, 1, 1.5, 2, 3, 5 } });
        });


    builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));

    builder.Services.AddDbContext<PaymentsDBContext>(options =>
        options
            .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention());

    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    builder.Services.AddCarter();
    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

    builder.Services.AddMassTransit(busConfigurator =>
    {
        busConfigurator.AddConsumer<InventoryReservedConsumer>();

        busConfigurator.AddEntityFrameworkOutbox<PaymentsDBContext>((o) =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });

        busConfigurator.SetKebabCaseEndpointNameFormatter();
        busConfigurator.UsingRabbitMq((context, configurator) =>
        {
            var rbmqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

            configurator.Host(rbmqOptions.Host, rbmqOptions.Port, "/", h =>
            {
                h.Username(rbmqOptions.Username);
                h.Password(rbmqOptions.Password);
            });

            configurator.ConfigureEndpoints(context);
        });
    });

    var app = builder.Build();

    app.MapPrometheusScrapingEndpoint();
    app.MapHealthChecks("/health");
    app.UseSerilogRequestLogging();
    app.ApplyMigrations();
    app.MapCarter();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}