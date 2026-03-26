using System.Reflection;
using System.Text;
using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using FluentValidation;
using Microsoft.Extensions.Options;
using IdentityAPI.Domain.Identity;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Shared.Middleware;
using Shared.Models;
using Shared.Validation;
using ITokenBlacklistService = Shared.Services.ITokenBlacklistService;
using TokenBlacklistService = Shared.Services.TokenBlacklistService;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    var restfulHttpPort = builder.Configuration["Hosting:Restful:Http"] ?? throw new InvalidOperationException();
    // var restfulHttpsPort = builder.Configuration["Hosting:Restful:Https"] ?? throw new InvalidOperationException();
    var grpcHttpPort = builder.Configuration["Hosting:Grpc:Http"] ?? throw new InvalidOperationException();
    // var grpcHttpsPort = builder.Configuration["Hosting:Grpc:Https"] ?? throw new InvalidOperationException();

    // Setup a HTTP/1.1 endpoint for REST API
    options.ListenAnyIP(int.Parse(restfulHttpPort), o => o.Protocols = HttpProtocols.Http1);
    /*options.ListenAnyIP(int.Parse(restfulHttpsPort), o =>
    {
        o.Protocols = HttpProtocols.Http1;
        o.UseHttps();
    });*/
    // Setup a HTTP/2 endpoint for gRPC
    options.ListenAnyIP(int.Parse(grpcHttpPort), o => o.Protocols = HttpProtocols.Http2);
    /*options.ListenLocalhost(int.Parse(grpcHttpsPort), o =>
    {
        o.Protocols = HttpProtocols.Http2;
        o.UseHttps();
    });*/
});

// Add services to the container.
builder.Services.AddScoped<RequestValidationActionFilter>();
builder.Services.AddControllers(options => options.Filters.AddService<RequestValidationActionFilter>());
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Mediator
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// MongoDB Configuration
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Identity Configuration
var mongoDbSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>();
var mongoDbIdentityConfig = new MongoDbIdentityConfiguration
{
    MongoDbSettings = new MongoDbSettings
    {
        ConnectionString = mongoDbSettings!.ConnectionString,
        DatabaseName = mongoDbSettings.DatabaseName
    },
    IdentityOptionsAction = options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.RequireUniqueEmail = true;
        
        // Disable default sign-in scheme (we use JWT)
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    }
};

builder.Services.ConfigureMongoDbIdentity<ApplicationUser, ApplicationRole, Guid>(mongoDbIdentityConfig);

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
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
        ValidIssuer = jwtSettings!.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization();

// Redis for token blacklist
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
});

// Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.Configure<GoogleSettings>(builder.Configuration.GetSection("Google"));
builder.Services.AddSingleton<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddSingleton<IValidateOptions<GoogleSettings>, GoogleSettingsValidator>();

var app = builder.Build();

// Global exception handler
app.UseGlobalExceptionHandler();

// Configure the HTTP request pipeline.
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();
app.MapOpenApi();
app.MapControllers();

app.Run();
