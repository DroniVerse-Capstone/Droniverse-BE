using DotNetEnv;
using Droniverse.Identity.API.Swagger;
using Droniverse.Identity.Application;
using Droniverse.Identity.Application.RabbitMQ;
using Droniverse.Identity.Infrastructure;
using Droniverse.Shared;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Settings;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using System.Text.Json.Serialization;
using System.Transactions;

// Load .env for JWT, Cloudinary, PayOS settings
Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);

// If Development: reload appsettings.Development.json to override RabbitMQ settings locally
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);
}

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Convert enum sang string khi serialize/deserialize JSON
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddShared(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
// chỉ cần 1 dòng này là các ExampleProvider trong assembly sẽ đc apply vào swagger
builder.Services.AddSwaggerExamplesFromAssemblyOf<LoginExampleProvider>();

builder.Services.AddSwaggerGen(c =>
{
    c.ExampleFilters(); // Add data mẫu vào các API, Vd: LoginExampleProvider LoginEmailDto,...

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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); //cho phép gửi cookie
    });
});


// ======================
// HANGFIRE
// ======================

var connectionString = builder.Configuration.GetConnectionString("IdentityConnection");

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

//builder.Services.AddHangfireServer();

var app = builder.Build();

Console.Title = "Identity Service";

// Start RabbitMQ OrderNotificationConsumer
var orderNotificationConsumer = app.Services.GetRequiredService<OrderNotificationConsumer>();
orderNotificationConsumer.Consume();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseStaticFiles(); // sử dụng static files

//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.DocExpansion(DocExpansion.None); //Đóng các api lại cho gọn
//        c.InjectJavascript("/swagger-custom.js"); // nhúm static file vào swagger cho ô Authorize
//    });
//}

app.UseStaticFiles(); // sử dụng static files

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocExpansion(DocExpansion.None); //Đóng các api lại cho gọn
    c.DocumentTitle = "Identity API Docs";
    c.DisplayRequestDuration();
    c.EnableFilter();
    c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
});

// Inject swagger-custom.js into Swagger UI
var swaggerCustomFile = Path.Combine(app.Environment.WebRootPath, "swagger-custom.js");
var swaggerCustomVersion = File.Exists(swaggerCustomFile)
    ? File.GetLastWriteTimeUtc(swaggerCustomFile).Ticks.ToString()
    : DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        var originalBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await next();

        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

        if (context.Response.ContentType?.Contains("text/html") == true)
        {
            responseBody = responseBody.Replace(
                "</body>",
                $"<script src=\"/swagger-custom.js?v={swaggerCustomVersion}\"></script></body>"
            );
        }

        var modifiedBody = Encoding.UTF8.GetBytes(responseBody);
        context.Response.Body = originalBody;
        context.Response.ContentLength = modifiedBody.Length;
        await context.Response.Body.WriteAsync(modifiedBody);
    }
    else
    {
        await next();
    }
});

//app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseCors();

app.UseRouting();
app.UseAuthentication();
//app.UseHangfireDashboard("/hangfire", new DashboardOptions
//{
//    Authorization = new IDashboardAuthorizationFilter[] { }
//});
app.UseAuthorization();


app.MapControllers();

app.Run();
