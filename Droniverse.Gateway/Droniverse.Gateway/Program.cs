using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;
using DotNetEnv;
using Microsoft.Extensions.Options;
using Droniverse.Shared.Settings;
using Droniverse.Shared;
using System.IO;

Env.Load("../../.env");
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddShared(builder.Configuration);

// JWT Authentication
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
            // Đọc token từ cookie
            var accessToken = context.Request.Cookies["AccessToken"];

            // Nếu không có trong cookie, thử từ header
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

// Load Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add Ocelot
builder.Services.AddOcelot(builder.Configuration);

// Add Swagger for Ocelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);
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

var app = builder.Build();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
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
}

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
    opt.ReConfigureUpstreamSwaggerJson = AlterUpstreamSwaggerJson;
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.UseOcelot();

app.Run();

//Hàm để thêm Security Definition vào Swagger JSON
static string AlterUpstreamSwaggerJson(HttpContext context, string swaggerJson)
{
    var swagger = JObject.Parse(swaggerJson);

    // Thêm Security Schemes
    if (swagger["components"] == null)
    {
        swagger["components"] = new JObject();
    }

    swagger["components"]!["securitySchemes"] = new JObject
    {
        ["Bearer"] = new JObject
        {
            ["type"] = "http",
            ["scheme"] = "bearer",
            ["bearerFormat"] = "JWT",
            ["description"] = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
        }
    };

    // Thêm Security Requirement cho tất cả endpoints
    swagger["security"] = new JArray
    {
        new JObject
        {
            ["Bearer"] = new JArray()
        }
    };

    // Thêm Security cho từng path
    if (swagger["paths"] != null)
    {
        foreach (var path in swagger["paths"]!.Children<JProperty>())
        {
            foreach (var method in path.Value.Children<JProperty>())
            {
                // Nếu endpoint chưa có security, thêm vào
                if (method.Value["security"] == null)
                {
                    method.Value["security"] = new JArray
                    {
                        new JObject
                        {
                            ["Bearer"] = new JArray()
                        }
                    };
                }
            }
        }
    }

    return swagger.ToString(Newtonsoft.Json.Formatting.Indented);
}