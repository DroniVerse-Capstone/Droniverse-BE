using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyAtLeast32Characters!!")),
            ClockSkew = TimeSpan.Zero
        };
    });

// Load Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add Ocelot
builder.Services.AddOcelot(builder.Configuration);

// Add Swagger for Ocelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Enable Swagger for Ocelot UI với Security Injection
    app.UseSwaggerForOcelotUI(opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/docs";
        
        // Inject Security Definition vào tất cả Swagger documents
        opt.ReConfigureUpstreamSwaggerJson = AlterUpstreamSwaggerJson;
    });
}

app.UseHttpsRedirection();
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