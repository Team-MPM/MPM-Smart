using MPM_Betting.Aspire.AppHost;
using MpmSmart.Web.Host;

var builder = DistributedApplication.CreateBuilder(args);

// Configuration

var launchProfile = builder.ExecutionContext.IsPublishMode ? "" : "Watch";

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
//var tenantDb = sqlServer.AddDatabase("TenantDatabase");

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

var storage = builder.AddAzureStorage("Storage");

var blobs = storage.AddBlobs("BlobConnection");
var queues = storage.AddQueues("QueueConnection");
var tables = storage.AddTables("TablesConnection");

// Services

var authService = builder.AddProject<Projects.AuthService>("AuthService", launchProfile)
    .WithReference(redis)
    .WithReference(sqlServer)
    .WithReference(rabbitMq)
    .WithReference(kafka)
    .WithReference(neo4J);

var notificationService = builder.AddProject<Projects.NotificationService>("NotificationService", launchProfile)
    .WithReference(redis)
    .WithReference(sqlServer)
    .WithReference(rabbitMq)
    .WithReference(kafka)
    .WithReference(neo4J);

var networkService = builder.AddProject<Projects.NetworkService>("NetworkService", launchProfile)
    .WithReference(redis)
    .WithReference(sqlServer)
    .WithReference(rabbitMq)
    .WithReference(kafka)
    .WithReference(neo4J)
    .WithReference(authService)
    .WithReference(notificationService);

var commandService = builder.AddProject<Projects.CommandService>("CommandService", launchProfile)
    .WithReference(redis)
    .WithReference(sqlServer)
    .WithReference(rabbitMq)
    .WithReference(kafka)
    .WithReference(neo4J)
    .WithReference(networkService)
    .WithReference(authService)
    .WithReference(notificationService);

var homeDataService = builder.AddProject<Projects.HomeDataService>("HomeDataService", launchProfile)
    .WithReference(redis)
    .WithReference(sqlServer)
    .WithReference(rabbitMq)
    .WithReference(kafka)
    .WithReference(neo4J)
    .WithReference(homeDataDatabase)
    .WithReference(networkService)
    .WithReference(authService)
    .WithReference(notificationService);

var routineService = builder.AddProject<Projects.RoutineService>("RoutineService", launchProfile)
    .WithReference(redis)
    .WithReference(sqlServer)
    .WithReference(rabbitMq)
    .WithReference(kafka)
    .WithReference(neo4J)
    .WithReference(networkService)
    .WithReference(authService)
    .WithReference(commandService)
    .WithReference(homeDataService)
    .WithReference(notificationService);

var dataGateway = builder.AddProject<Projects.Web_DataGateway>("DataGateway", launchProfile)
    .WithReference(redis)
    .WithReference(sqlServer)
    .WithReference(rabbitMq)
    .WithReference(kafka)
    .WithReference(neo4J)
    .WithReference(homeDataService)
    .WithReference(authService);

var api = builder.AddProject<Projects.Web_Api>("Api", launchProfile)
    .WithReference(redis)
    .WithReference(sqlServer)
    .WithReference(rabbitMq)
    .WithReference(kafka)
    .WithReference(neo4J)
    .WithReference(homeDataService)
    .WithReference(authService)
    .WithReference(dataGateway)
    .WithReference(notificationService)
    .WithReference(routineService)
    .WithReference(blobs)
    .WithReference(queues)
    .WithReference(tables);

builder.AddProject<Projects.Web_Server>("Web-Server", launchProfile)
    .WithReference(api);

// Management

var dbManager = builder.AddProject<Projects.DbManager>("dbmanager", launchProfile)
    .WithReference(redis)
    .WithReference(primaryDb)
    //.WithReference(tenantDb)
    .WithReference(homeDataDatabase)
    .WithReference(blobs)
    .WithReference(queues)
    .WithReference(tables);

var adminDashboard = builder.AddProject<Projects.AdminDashboard>("AdminDashboard", launchProfile)
    .WithReference(redis)
    .WithReference(networkService)
    .WithReference(authService)
    .WithReference(notificationService)
    .WithReference(routineService)
    .WithReference(dataGateway)
    .WithReference(api)
    .WithReference(dbManager);

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
else
{
    storage.RunAsEmulator(resourceBuilder =>
    {
        resourceBuilder.WithBlobPort(4100);
        resourceBuilder.WithQueuePort(4101);
        resourceBuilder.WithTablePort(4102);
    });
}

await builder.Build().RunAsync();