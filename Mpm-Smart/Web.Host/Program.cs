using MpmSmart.Web.Host;

var builder = DistributedApplication.CreateBuilder(args);

var launchProfile = builder.ExecutionContext.IsPublishMode ? "https" : "Watch";

var username = builder.AddParameter("username");
var sqlPassword = builder.CreateStablePassword("sqlPassword");
var rabbitMqPassword = builder.CreateStablePassword("rabbitMqPassword");


// Resources

var redis = builder.AddRedis("redis")
    .WithPersistence()
    .WithDataVolume()
    .PublishAsAzureRedis();

var sql = builder.AddSqlServer("sql", password: sqlPassword)
    .WithDataVolume()
    .PublishAsAzureSqlDatabase()
    .AddDatabase("MPM-Smart");

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
    .WithReference(sql);

await builder.Build().RunAsync();