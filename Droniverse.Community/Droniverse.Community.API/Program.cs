using DotNetEnv;
using Droniverse.Community.Application;
using Droniverse.Community.Infrastructure;
using Droniverse.Identity.API;
using Droniverse.Shared;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Transactions;

Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddShared(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Convert enum sang string khi serialize/deserialize JSON
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Cho phép serialize Guid dưới dạng string trong MongoDB
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Droniverse Community API",
        Version = "v1",
        Description = "Community microservice"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();


builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ======================
// HANGFIRE
// ======================

var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");

builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer();
    config.UseRecommendedSerializerSettings();

    config.UseStorage(new MySqlStorage(
        connectionString,
        new MySqlStorageOptions
        {
            TablesPrefix = "Hangfire",
            PrepareSchemaIfNecessary = true,
            QueuePollInterval = TimeSpan.FromSeconds(15),
            TransactionTimeout = TimeSpan.FromMinutes(1),
            TransactionIsolationLevel = IsolationLevel.ReadCommitted
        }
    ));
});

builder.Services.AddHangfireServer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Droniverse Community API v1");
        c.DocExpansion(DocExpansion.None); //Đóng các api lại cho gọn
    });
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

//app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new IDashboardAuthorizationFilter[] { }
});
app.MapControllers();

app.Run();
