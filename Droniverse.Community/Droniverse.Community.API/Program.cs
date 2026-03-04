using DotNetEnv;
using Droniverse.Community.Application;
using Droniverse.Community.Infrastructure;
using Droniverse.Identity.API;
using Microsoft.OpenApi.Models;
using Droniverse.Shared;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text.Json.Serialization;

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
app.UseAuthorization();
app.MapControllers();

app.Run();
