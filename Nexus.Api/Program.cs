using Nexus.Infrastructure;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orleans.Configuration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
});

Console.WriteLine(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("nexus-api", serviceNamespace: "nexus-group"))
    .WithMetrics(metrics =>
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddNpgsqlInstrumentation()
            .AddMeter("Microsoft.Orleans"))
    .WithTracing(tracing =>
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddNpgsql()
            .AddSource("Microsoft.Orleans.Runtime")
            .AddSource("Microsoft.Orleans.Application"))
    .UseOtlpExporter();

builder.Host.UseOrleansClient((context, clientBuilder) =>
{
    clientBuilder.UseAdoNetClustering(options =>
    {
        options.ConnectionString = context.Configuration.GetConnectionString("DefaultConnection");
        options.Invariant = SiloConstants.StorageInvariant;
    });

    clientBuilder.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = SiloConstants.ClusterId;
        options.ServiceId = SiloConstants.ServiceId;
    });

    clientBuilder.AddActivityPropagation();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();