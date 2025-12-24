using System.Reflection;
using System.Text;
using DotPulsar;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nexus.Api.Abstractions;
using Nexus.Api.Abstractions.UseCases;
using Nexus.Api.Auth;
using Nexus.Api.Auth.Actor;
using Nexus.Api.Extensions;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orleans.Configuration;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.Pulsar;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddLogging();

builder.Host.UseOrleansClient((context, clientBuilder) =>
{
    clientBuilder.UseAdoNetClustering(options =>
    {
        options.ConnectionString = context.Configuration.GetConnectionString("Nexus");
        options.Invariant = context.Configuration.GetValue<string>("OrleansConfig:StorageInvariant");
    });
    
    clientBuilder.Configure<ClusterOptions>(options => 
    {
        options.ClusterId = context.Configuration.GetValue<string>("OrleansConfig:ClusterId");
        options.ServiceId = context.Configuration.GetValue<string>("OrleansConfig:ServiceId");
    });

    clientBuilder.AddActivityPropagation();
});

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Auth"));
});

builder.Services
    .AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.UseWolverine(options =>
{
    options.UsePulsar(p =>
    {
        var pulsarUri = new Uri(builder.Configuration.GetConnectionString("Pulsar") 
                                ?? throw new InvalidOperationException());
        p.ServiceUrl(pulsarUri);
    });
    
    options.AddSubscribeOrderMessages();
    options.AddPublishOrderMessages();
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters.ValidIssuer = builder.Configuration["JwtConfig:Issuer"];
        options.TokenValidationParameters.ValidAudience = builder.Configuration["JwtConfig:Audience"];
        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:SecretKey"]!));
        
        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidateLifetime = true;
        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
    });

builder.Services.AddAuthorization();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.SlidingExpiration = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IActor, Actor>();

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
});

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

var handlers = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(d => d is { IsClass: true, IsAbstract: false } 
                && d.GetInterfaces().Any(i => i.IsGenericType && 
                                              i.GetGenericTypeDefinition() == typeof(IUseCaseHandler<,>)));

foreach (var implType in handlers)
{
    builder.Services.AddScoped(implType);

    var serviceTypes = implType.GetInterfaces()
        .Where(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IUseCaseHandler<,>))
        .ToArray();

    foreach (var serviceType in serviceTypes)
        builder.Services.AddScoped(serviceType, implType);
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    
    await app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();