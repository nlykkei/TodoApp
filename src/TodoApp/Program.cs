using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Serilog;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.HttpOverrides;
using System.Reflection;
using Microsoft.OpenApi.Models;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure logging
if (builder.Environment.IsDevelopment())
{
    builder.Host.UseSerilog(
    (context, loggerConfiguration) => loggerConfiguration
        .MinimumLevel.Debug()
        .WriteTo.Console());
}
else
{
    builder.Host.UseSerilog(
    (context, loggerConfiguration) => loggerConfiguration
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File("logs/todoapp.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.ApplicationInsights(new TelemetryConfiguration
        {
            InstrumentationKey = builder.Configuration["ApplicationInsightsInstrumentationKey"]
        }, TelemetryConverter.Traces));
}

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddXmlDataContractSerializerFormatters();

builder.Services.AddProblemDetails();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

// Configure authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();

// builder.Services.AddAuthorization(options =>
// {
//     options.AddPolicy("MustBeFromAntwerp", policy =>
//     {
//         policy.RequireAuthenticatedUser();
//         policy.RequireClaim("city", "Antwerp");
//     });
// });

// Configure OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(action =>
{
    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    var tenantId = builder.Configuration.GetValue<string>("AzureAd:TenantId");
    var clientId = builder.Configuration.GetValue<string>("AzureAd:ClientId");

    action.IncludeXmlComments(xmlCommentsFullPath);

    action.AddSecurityDefinition("TodoAppAuth", new()
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { $"api://{clientId}/user_impersonation", "Access the application as the signed-in user" },
                }
            }
        },
        Scheme = "Bearer",
    });

    action.AddSecurityRequirement(new()
    {
        {
            new ()
            {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "TodoAppAuth"
                }
            },
            new List<string>()
        }
    });
    // action.InferSecuritySchemes();
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
    | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-8.0
    app.UseExceptionHandler();
}

app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

#pragma warning disable ASP0014
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// app.MapControllers();

app.Run();
