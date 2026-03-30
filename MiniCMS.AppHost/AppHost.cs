var builder = DistributedApplication.CreateBuilder(args);

// SQL Server container + database resource.
// IMPORTANT: use "DefaultConnection" so MiniCMS.Web can keep using
// Configuration.GetConnectionString("DefaultConnection") unchanged.
var sql = builder.AddSqlServer("sql")
                 .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("DefaultConnection");

// Existing ASP.NET backend
var web = builder.AddProject<Projects.MiniCMS_Web>("web")
                 .WithReference(db)
                 .WaitFor(db);

// Existing standalone Blazor WebAssembly frontend
builder.AddProject<Projects.MiniCMS_Client>("client")
       .WithReference(web)
       .WaitFor(web);

builder.Build().Run();