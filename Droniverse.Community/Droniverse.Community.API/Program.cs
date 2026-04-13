using DotNetEnv;
using Droniverse.Community.Application;
using Droniverse.Community.Infrastructure;
using Droniverse.Shared;
using Droniverse.Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Filters;
using MongoDB.Bson.Serialization.Serializers;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Droniverse.Community.Application.Jobs;
using Droniverse.Community.API.Jobs;
using Droniverse.Shared.Exceptions;


Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddShared(builder.Configuration);
//builder.Services.AddScoped<CompetitionStatusJob>();
//builder.Services.AddScoped<RoundStatusJob>();
//builder.Services.AddScoped<HotCompetitionsJob>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Convert enum sang string khi serialize/deserialize JSON
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Cho phép serialize Guid dưới dạng string trong MongoDB
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

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header sử dụng Bearer scheme.
                        
**Hướng dẫn sử dụng:**
1. Copy một trong các token mẫu bên dưới
2. Paste vào ô 'Value' (không cần thêm 'Bearer ')
3. Click 'Authorize'

---

### Token mẫu theo Role có thời hạn tới (19/6/2026):

#### CLUB_MEMBER
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiI5NGM2ODkxZi0zMTNkLTRkNGYtYTI1MC00MGI4YTQ3ZjkxMzkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidG9hbk1lbWJlckBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ0b2FuTWVtYmVyQGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNMVUJfTUVNQkVSIiwianRpIjoiNjA1YTU5OGMtMmQ5My00NDY2LThlNTEtYmJmYjhjZGRiZTk5IiwiZXhwIjoxNzgxODMwMzEwLCJpc3MiOiJEcm9uaXZlcnNlLklkZW50aXR5IiwiYXVkIjoiRHJvbml2ZXJzZS5JZGVudGl0eSJ9.VM1tbYOvNMXIDj00JXn40g4-UsVodnpCyMZzykvQsRk
```

#### CLUB_MANAGER
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiI1ZmNlOTk1MC1jOTI4LTQzMzMtYWFiZC02YjMyZTYzNWY3NjYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYmxhY2twcm9DbHViTWFuYWdlckBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJibGFja3Byb0NsdWJNYW5hZ2VyQGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNMVUJfTUFOQUdFUiIsImp0aSI6IjQxZjhlYzQ3LTI3ZmQtNGQ1OS1hNzkyLWIxODk1NDgzNzc2ZCIsImV4cCI6MTc4MTgzMDgzNCwiaXNzIjoiRHJvbml2ZXJzZS5JZGVudGl0eSIsImF1ZCI6IkRyb25pdmVyc2UuSWRlbnRpdHkifQ.pbMSMNn4zk6ZSUGkEzNtcRzcYeIHwajriuQI12iN6a8
```

#### SYSTEM_MANAGER
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiIyMTUwZjdmZC05ZDE5LTQ2YjQtYTAzMS0wMjEwYmMxNjE2MGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic3lzbWFuYWdlckBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJzeXNtYW5hZ2VyQGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlNZU1RFTV9NQU5BR0VSIiwianRpIjoiOGQ5MTJmOWUtNDVkNC00MWMwLWFmNDgtM2EzYzBlZDJhNjg3IiwiZXhwIjoxNzgxODMwNzY3LCJpc3MiOiJEcm9uaXZlcnNlLklkZW50aXR5IiwiYXVkIjoiRHJvbml2ZXJzZS5JZGVudGl0eSJ9.RKHooDfGadVlDkikUsa85EjXbH58BFuo1PiXiiqYKXs
```

#### ADMIN
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiI4YTY0ZDk1ZS1mMDQxLTQ5ZjctYmMxOC1hODJhZWNkODE2MTIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiYWRtaW5AZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQURNSU4iLCJqdGkiOiI0YzJjNWMxZC0wNGQ0LTRjZGEtOGRlYy02MTMwZDkxZGEwMWUiLCJleHAiOjE3ODE4MzA4MTIsImlzcyI6IkRyb25pdmVyc2UuSWRlbnRpdHkiLCJhdWQiOiJEcm9uaXZlcnNlLklkZW50aXR5In0.NurEt2VFtkyIkP8slJaNaPBTiswbKDKcAsm0ps5YkCw
```

---

**Lưu ý:** Token được đọc tự động từ Cookie 'AccessToken' hoặc Authorization header.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var serviceProvider = builder.Services.BuildServiceProvider();
    var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
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
    //JwtBearerEventsConfigurator.Configure(options);
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Đọc token từ cookie trước
            var accessToken = context.Request.Cookies["AccessToken"];

            // Nếu không có trong cookie, thử đọc từ header (cho mobile app)
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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ======================
// HANGFIRE
// ======================

//var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireMySqlConnection")
//    ?? throw new InvalidOperationException("Missing connection string 'HangfireMySqlConnection'.");

//builder.Services.AddHangfire(config =>
//{
//    config.UseSimpleAssemblyNameTypeSerializer();
//    config.UseRecommendedSerializerSettings();
//    config.UseStorage(new MySqlStorage(
//        hangfireConnectionString,
//        new MySqlStorageOptions
//        {
//            TablesPrefix = "Hangfire",
//            PrepareSchemaIfNecessary = true,
//            QueuePollInterval = TimeSpan.FromSeconds(15),
//            TransactionTimeout = TimeSpan.FromSeconds(30),
//            TransactionIsolationLevel = IsolationLevel.ReadCommitted,
//            JobExpirationCheckInterval = TimeSpan.FromMinutes(10),
//        }
//    ));
//});

//builder.Services.AddHangfireServer(config =>
//{
//    config.WorkerCount = 3;
//    config.Queues = ["critical", "default", "low"];
//    config.SchedulePollingInterval = TimeSpan.FromSeconds(10);
//    config.HeartbeatInterval = TimeSpan.FromSeconds(30);
//    config.ServerCheckInterval = TimeSpan.FromMinutes(1);
//    config.CancellationCheckInterval = TimeSpan.FromSeconds(15);
//});

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        //c.SwaggerEndpoint("/swagger/v1/swagger.json", "Droniverse Community API v1");
//        c.DocExpansion(DocExpansion.None); //Đóng các api lại cho gọn

//    });
//}


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocExpansion(DocExpansion.None);
    c.DocumentTitle = "Community API Docs";
    c.DisplayRequestDuration();
    c.EnableFilter();
    c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
});

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseStaticFiles();

//app.UseHttpsRedirection();

//Console.WriteLine(typeof(CompetitionStatusJob).FullName);
//Console.WriteLine(typeof(CompetitionStatusJob).Assembly.FullName);
//Console.WriteLine(AppDomain.CurrentDomain
//    .GetAssemblies()
//    .Any(a => a.GetName().Name == "Droniverse.Community.Application"));

app.UseCors();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//app.UseHangfireDashboard("/hangfire", new DashboardOptions
//{
//    Authorization = [],
//    DisplayStorageConnectionString = true,
//});

Console.Title = "Community Service";

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine($"Is Development : {app.Environment.IsDevelopment()}");
    Console.WriteLine("Background job is not running !");
    //RecurringJobScheduler.ScheduleJobs();
});

app.Lifetime.ApplicationStopping.Register(() => Console.WriteLine("App is stopping..."));

app.Lifetime.ApplicationStopped.Register(() => Console.WriteLine("App stopped."));

app.MapControllers();

app.Run();