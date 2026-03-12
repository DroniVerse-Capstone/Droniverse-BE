using DotNetEnv;
using Droniverse.Academy.Application;
using Droniverse.Academy.Infrastructure;
using Droniverse.Identity.API;
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
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using System.Text.Json.Serialization;
using System.Transactions;

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
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header sử dụng Bearer scheme",
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