using MpmSmart.Web.Host;

var builder = DistributedApplication.CreateBuilder(args);

// Configuration

var launchProfile = builder.ExecutionContext.IsPublishMode ? "https" : "Watch";

var publishToAzure = builder.ExecutionContext.IsPublishMode && builder.Configuration["azure"] == "true";

var username = builder.AddParameter("username");
var sqlPassword = builder.CreateStablePassword("sqlPassword");
var rabbitMqPassword = builder.CreateStablePassword("rabbitMqPassword");


// Resources

var redis = builder.AddRedis("redis")
    .WithPersistence()
    .WithDataVolume();

var sqlServer = builder.AddSqlServer("sql", password: sqlPassword)
    .WithDataVolume();

var sqlDb = sqlServer.AddDatabase("MPM-Smart");

var rabbitMq = builder.AddRabbitMQ("rabbitmq", userName: username, password: rabbitMqPassword)
    .WithManagementPlugin()
    .WithDataVolume();

var kafka = builder.AddKafka("kafka")
    .WithDataVolume();

var api = builder.AddProject<Projects.Web_Api>("Api", "Watch");

// Projects

builder.AddProject<Projects.Web_Server>("Web-Server", "Watch")
    .WithReference(api);

var dbManager = builder.AddProject<Projects.DbManager>("dbmanager")
    .WithReference(redis)
    .WithReference(sqlDb);

if (publishToAzure)
{
    redis.PublishAsAzureRedis();
    sqlServer.PublishAsAzureSqlDatabase();
}

await builder.Build().RunAsync();