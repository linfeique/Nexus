using Nexus.Infrastructure;
using Orleans.Configuration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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