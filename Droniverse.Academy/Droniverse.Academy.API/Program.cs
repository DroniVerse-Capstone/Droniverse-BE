using DotNetEnv;
using Droniverse.Academy.Application;
using Droniverse.Academy.Infrastructure;
using Droniverse.Identity.API;
using Droniverse.Shared.Settings;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using System.Text.Json.Serialization;
using System.Transactions;

Env.Load("../../env");
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Convert enum sang string khi serialize/deserialize JSON
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Cho phép serialize Guid dưới dạng string trong MongoDB
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    //c.ExampleFilters(); // Add data mẫu vào các API, Vd: LoginExampleProvider LoginEmailDto,...

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header sử dụng Bearer scheme. Fe quăng token dô đây nha",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, // SecuritySchemeType.ApiKey là Bearer {token}
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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
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

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocExpansion(DocExpansion.None); //Đóng các api lại cho gọn
    });
}

//app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();   
app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard("/hangfire");

app.Run();
