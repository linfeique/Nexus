using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;

await Host.CreateDefaultBuilder(args)
    .UseOrleans(builder =>
    {
        builder.UseAdoNetClustering(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("Orleans");
            options.Invariant = builder.Configuration.GetValue<string>("STORAGE_INVARIANT");
        });

        builder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = builder.Configuration.GetValue<string>("CLUSTER_ID");
            options.ServiceId = builder.Configuration.GetValue<string>("SERVICE_ID");
        });

        builder.AddAdoNetGrainStorage(builder.Configuration.GetValue<string>("OrleansStorageDev"), options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("Orleans");
            options.Invariant = builder.Configuration.GetValue<string>("STORAGE_INVARIANT");
        });

        builder.AddActivityPropagation();
    })
    .RunConsoleAsync();