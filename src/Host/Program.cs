var builder = DistributedApplication.CreateBuilder(args);


var cache = builder.AddRedis("redis");
var postgres = builder.AddPostgres("postgres")
   .WithDataVolume(isReadOnly: false)
   .WithPgAdmin();

var db = postgres.AddDatabase("apidb");
    
builder.AddProject<Projects.Core>("coreapi")
   .WithReference(cache)
   .WithReference(db);



builder.Build().Run();