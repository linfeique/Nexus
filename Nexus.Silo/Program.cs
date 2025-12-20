using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;

await Host.CreateDefaultBuilder(args)
    .UseOrleans((context, builder) =>
    {
        var configuration = context.Configuration;
        
        builder.UseAdoNetClustering(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("Nexus");
            options.Invariant = configuration.GetValue<string>("OrleansConfig:StorageInvariant");
        });

        builder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = configuration.GetValue<string>("OrleansConfig:ClusterId");
            options.ServiceId = configuration.GetValue<string>("OrleansConfig:ServiceId");
        });

        builder.AddAdoNetGrainStorage(configuration.GetValue<string>("OrleansConfig:StorageName"), options =>
        {
            options.ConnectionString = configuration.GetConnectionString("Nexus");
            options.Invariant = configuration.GetValue<string>("OrleansConfig:StorageInvariant");
        });

        builder.AddActivityPropagation();
    })
    .RunConsoleAsync();