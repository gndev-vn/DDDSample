using System.Text;
using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using FluentValidation;
using IdentityAPI.Configuration;
using IdentityAPI.Domain.Identity;
using IdentityAPI.Features;
using IdentityAPI.Features.Auth.Services;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using ServiceDefaults;
using Shared.Authentication;
using Shared.Hosting;
using Shared.Middleware;
using Shared.Models;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCentralizedApiEndpoints();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddMediator(options =>
{
    options.Assemblies = [typeof(Program)];
    options.ServiceLifetime = ServiceLifetime.Scoped;
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

var configuredMongoDbSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>() ?? new MongoDbSettings();
var aspireMongoConnectionString = builder.Configuration.GetConnectionString("MongoDB");
var mongoDbSettings = new MongoDbSettings
{
    ConnectionString = aspireMongoConnectionString
                       ?? configuredMongoDbSettings.ConnectionString
                       ?? throw new InvalidOperationException("MongoDB connection string is not configured."),
    DatabaseName = configuredMongoDbSettings.DatabaseName ?? "identitydb"
};

builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = mongoDbSettings.ConnectionString;
    options.DatabaseName = mongoDbSettings.DatabaseName;
});
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<IdentitySeedOptions>(builder.Configuration.GetSection("IdentitySeed"));

var mongoDbIdentityConfig = new MongoDbIdentityConfiguration
{
    MongoDbSettings = new MongoDbSettings
    {
        ConnectionString = mongoDbSettings.ConnectionString,
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
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    }
};

builder.Services.ConfigureMongoDbIdentity<ApplicationUser, ApplicationRole, Guid>(mongoDbIdentityConfig);

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

builder.Services.AddApplicationAuthorization();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
});

builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<ILoginResponseFactory, LoginResponseFactory>();
builder.Services.AddScoped<IIdentityAccountService, IdentityAccountService>();
builder.Services.AddScoped<IdentitySeedService>();
builder.Services.Configure<GoogleSettings>(builder.Configuration.GetSection("Google"));
builder.Services.AddSingleton<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddSingleton<IValidateOptions<GoogleSettings>, GoogleSettingsValidator>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var identitySeedService = scope.ServiceProvider.GetRequiredService<IdentitySeedService>();
    await identitySeedService.SeedAsync();
}

app.UseGlobalExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();
app.MapOpenApi();
app.MapIdentityEndpoints();
app.MapDefaultEndpoints();

app.Run();
