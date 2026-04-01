using DotNetEnv;
using Droniverse.Academy.Application;
using Droniverse.Academy.Infrastructure;
using Droniverse.Identity.API;
using Droniverse.Shared;
using Droniverse.Shared.Settings;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

// Load .env
Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();


// ======================
// SERVICES
// ======================

// Controllers + Enum serialize
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// MongoDB serialize Guid -> string
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

// Application & Infrastructure
builder.Services.AddShared(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExamplesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    c.CustomSchemaIds(type => type.FullName);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header sử dụng Bearer scheme " +
        "\r\nadmin: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiI4YTY0ZDk1ZS1mMDQxLTQ5ZjctYmMxOC1hODJhZWNkODE2MTIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiYWRtaW5AZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQURNSU4iLCJqdGkiOiJkM2FlYjQ3MC04YjQxLTQ2ODMtODJjYS0zYzliODMwNjUyYzUiLCJleHAiOjE3Nzg2Mjc0OTcsImlzcyI6IkRyb25pdmVyc2UuSWRlbnRpdHkiLCJhdWQiOiJEcm9uaXZlcnNlLklkZW50aXR5In0.nZoeI8wcYQ8-eLdvdL0BxQAoxSu8fVLTVzZe5uPy6jw\r\n\r\nsystem manager: \r\neyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiIyMTUwZjdmZC05ZDE5LTQ2YjQtYTAzMS0wMjEwYmMxNjE2MGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic3lzbWFuYWdlckBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJzeXNtYW5hZ2VyQGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlNZU1RFTV9NQU5BR0VSIiwianRpIjoiZjhhZDU2NWEtZTE3Ni00MDUxLTlkY2ItMzA0ODE5YzBkNDMzIiwiZXhwIjoxNzc4NjI3NTMxLCJpc3MiOiJEcm9uaXZlcnNlLklkZW50aXR5IiwiYXVkIjoiRHJvbml2ZXJzZS5JZGVudGl0eSJ9.Nx7oz8KqrcfA0Y-J5WDrpcje7FafemofKSLJRSyXSAY\r\n\r\nclub manager:\r\neyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiJhZTZkYTdmNS0xNDczLTQ1NmYtOWU1NS03MGRmNzAyZDQ3ZWUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiY2x1Ym1hbmFnZXJAZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiY2x1Ym1hbmFnZXJAZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQ0xVQl9NQU5BR0VSIiwianRpIjoiYzUyM2E4MjktODExYS00NjNlLWJmZGMtMDZmYmZkY2IxZWJiIiwiZXhwIjoxNzc4NjI3NTU4LCJpc3MiOiJEcm9uaXZlcnNlLklkZW50aXR5IiwiYXVkIjoiRHJvbml2ZXJzZS5JZGVudGl0eSJ9.3DpUjRJ4n-ApyeD_y8BrD_nPmUMMdf5SdNBoWYovtKU\r\n\r\nclub member:\r\neyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiIzMTk3NzM0ZC1kMjVkLTQyYjEtYjk2OC04NGI2ZWU0ZDMzYzIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiY2x1Ym1lbWJlckBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJjbHVibWVtYmVyQGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNMVUJfTUVNQkVSIiwianRpIjoiOWRjYWEwZWEtYTNkNy00MmY3LTllZDYtM2M3ZTBhYzRhZmY3IiwiZXhwIjoxNzc4NjI3NTgzLCJpc3MiOiJEcm9uaXZlcnNlLklkZW50aXR5IiwiYXVkIjoiRHJvbml2ZXJzZS5JZGVudGl0eSJ9.x2_a1AB3XLZqoQTiSopuTEGEY6bePNrvAlYhARO6xS8 ",

        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.ExampleFilters();
});


// ======================
// JWT CONFIG
// ======================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt")
);

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Key)
        ),

        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Cookies["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = context.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last();
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();


// ======================
// CORS
// ======================

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


// ======================
// BUILD APP
// ======================

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DbMigration");

    try
    {
        var db = scope.ServiceProvider.GetRequiredService<MySqlDbContext>();
        db.Database.Migrate();
        logger.LogInformation("Academy DB migrated successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Academy DB migration failed.");
        throw;
    }
}

// ======================
// MIDDLEWARE
// ======================

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocExpansion(DocExpansion.None);
    });
}

// app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new IDashboardAuthorizationFilter[] { }
});
app.MapControllers();

app.Run();