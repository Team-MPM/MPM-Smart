using Confluent.Kafka;
using DataModel.Primary;
using DbManager;
using HealthChecks.Kafka;
using Microsoft.EntityFrameworkCore;

var assemblyName = typeof(Program).Assembly.GetName().Name;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<PrimaryDbContext>("PrimaryDatabase", null,
    optionsBuilder =>
    {
        optionsBuilder.UseSqlServer(sqlBuilder =>
        {
            sqlBuilder.MigrationsAssembly(assemblyName);
            sqlBuilder.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

        optionsBuilder.EnableDetailedErrors();
    });

builder.AddMongoDBClient("HomeDataDatabase");

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(DbInitializer.ActivitySourceName));

builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DbInitializer>());

builder.Services.AddHealthChecks()
    .AddCheck<DbInitializerHealthCheck>("DbInitializer")
    .AddSqlServer(builder.Configuration.GetConnectionString("PrimaryDatabase")!)
    .AddRedis(builder.Configuration.GetConnectionString("redis")!)
    .AddAzureBlobStorage(builder.Configuration.GetConnectionString("BlobConnection")!)
    .AddAzureQueueStorage(builder.Configuration.GetConnectionString("QueueConnection")!)
    .AddRabbitMQ(builder.Configuration.GetConnectionString("RabbitMQ")!, null, "RabbitMQ")
    .AddMongoDb(builder.Configuration.GetConnectionString("HomeDataDatabase")!)
    .AddKafka(new KafkaHealthCheckOptions
    {
        Configuration = new ProducerConfig
        {
            BootstrapServers = builder.Configuration.GetConnectionString("kafka")!
        }
    });

var app = builder.Build();

app.UseRouting();
app.MapDefaultEndpoints();

await app.RunAsync();
