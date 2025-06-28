using AdventureWorks.WebApi.Dependency;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.CoreBuilder(configuration);

var app = builder.Build();
app.MapServices(configuration);

await app.RunAsync();
