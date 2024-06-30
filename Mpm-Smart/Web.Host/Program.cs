using MPM_Betting.Aspire.AppHost;
using MpmSmart.Web.Host;

var builder = DistributedApplication.CreateBuilder(args);

// Configuration

var launchProfile = builder.ExecutionContext.IsPublishMode ? "https" : "Watch";

var publishToAzure = builder.ExecutionContext.IsPublishMode && builder.Configuration["azure"] == "true";

var username = builder.AddParameter("username");
var sqlPassword = builder.CreateStablePassword("sqlPassword");
var rabbitMqPassword = builder.CreateStablePassword("rabbitMqPassword");
var neo4JPassword = builder.CreateStablePassword("neo4Password");


// Resources

var redis = builder.AddRedis("redis")
    .WithPersistence()
    .WithDataVolume();

var sqlServer = builder.AddSqlServer("sql", password: sqlPassword)
    .WithDataVolume();

var primaryDb = sqlServer.AddDatabase("PrimaryDatabase");
var tenantDb = sqlServer.AddDatabase("TenantDatabase");

var mongoDbServer = builder.AddMongoDB("mongo")
    .WithDataVolume();

var homeDataDatabase = mongoDbServer.AddDatabase("HomeDataDatabase");

var rabbitMq = builder.AddRabbitMQ("rabbitmq", userName: username, password: rabbitMqPassword)
    .WithManagementPlugin()
    .WithDataVolume();

var kafka = builder.AddKafka("kafka")
    .WithDataVolume();

var mail = builder.AddMailDev("maildev", 9324, 9325);

var neo4J  = builder.AddNeo4J("neo4j", 9326, 9327, neo4JPassword);

// Services

var api = builder.AddProject<Projects.Web_Api>("Api", "Watch");

builder.AddProject<Projects.Web_Server>("Web-Server", "Watch")
    .WithReference(api);

// Management

var dbManager = builder.AddProject<Projects.DbManager>("dbmanager")
    .WithReference(redis)
    .WithReference(primaryDb)
    .WithReference(tenantDb)
    .WithReference(homeDataDatabase);

if (builder.ExecutionContext.IsRunMode)
{
    builder.AddContainer("grafana", "grafana/grafana")
        .WithBindMount("../grafana/config", "/etc/grafana", isReadOnly: false)
        .WithBindMount("../grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: false)
        .WithHttpEndpoint(port: 3000, targetPort: 3000, isProxied: false);
    builder.AddContainer("prometheus", "prom/prometheus")
        .WithBindMount("../prometheus", "/etc/prometheus")
        .WithHttpEndpoint(port: 9090, targetPort: 9090, isProxied: false);
}

// Deployment

if (publishToAzure)
{
    redis.PublishAsAzureRedis();
    sqlServer.PublishAsAzureSqlDatabase();
}

await builder.Build().RunAsync();