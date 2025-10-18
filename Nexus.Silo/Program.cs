using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Nexus.Infrastructure;
using Orleans.Configuration;

await Host.CreateDefaultBuilder(args)
    .UseOrleans(builder =>
    {
        var configuration = builder.Configuration;
        
        const string databaseConnectionString = 
            "Host=localhost;Port=5433;Username=postgres;Password=0202;Database=NexusOrleans";
        
        builder.UseAdoNetClustering(options =>
        {
            options.ConnectionString = databaseConnectionString;
            options.Invariant = SiloConstants.StorageInvariant;
        });

        builder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = SiloConstants.ClusterId;
            options.ServiceId = SiloConstants.ServiceId;
        });

        builder.AddAdoNetGrainStorage(SiloConstants.StorageName, options =>
        {
            options.ConnectionString = databaseConnectionString;
            options.Invariant = SiloConstants.StorageInvariant;
        });
    })
    .RunConsoleAsync();