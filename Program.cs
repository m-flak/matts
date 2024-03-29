﻿/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023  Matthew E. Kehrer <matthew@kehrer.dev>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
**/
using Azure.Identity;
using FluentValidation;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Azure;
using Neo4j.Driver;
using matts.Configuration;
using matts.Models;
using matts.Models.Db;
using matts.Interfaces;
using matts.Services;
using matts.Daos;
using matts.Repositories;
using matts.Constants;
using matts.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var credential = new DefaultAzureCredential();

// Setup Azure App Config Service

bool useAzureAppConfig = builder.Configuration.GetSection("AzurePlatform").GetValue<bool?>("UseAzureAppConfiguration") ?? false;
if (useAzureAppConfig)
{
    builder.Configuration.AddAzureAppConfiguration(
        options =>
        {
            string? connectionString = builder.Configuration.GetConnectionString("AzureAppConfiguration");
            string? primaryServiceUrl = builder.Configuration.GetSection("AzurePlatform")
                .GetSection("AzureAppConfiguration")
                .GetValue<string?>("PrimaryServiceUrl");

            if (connectionString != null)
            {
                options = options.Connect(connectionString);
            }
            else if (Uri.IsWellFormedUriString(primaryServiceUrl, UriKind.Absolute))
            {
                options = options.Connect(new Uri(primaryServiceUrl, UriKind.Absolute), credential);
            }
            else
            {
                throw new ApplicationException("MISSING REQUIRED CONFIG VALUES: Azure App Coonfiguration ConnString OR Azure App Configuration URL");
            }

            options.Select("MattsSPA:*", builder.Environment.EnvironmentName)
                .TrimKeyPrefix("MattsSPA:")
                .ConfigureKeyVault(kv => kv.SetCredential(credential));
        }
    );

    builder.Services.AddAzureAppConfiguration();
}

// Setup other Azure services
bool useAzureBlobService = builder.Configuration.GetSection("AzurePlatform").GetValue<bool?>("UseAzureBlobService") ?? false;

builder.Services.AddAzureClients(clients => 
{
    if (useAzureBlobService)
    {
        var blobConfigs = builder.Configuration.GetSection("AzurePlatform")
            .GetSection("AzureBlobConfigurations")
            .Get<AzureBlobConfiguration[]>() 
                ?? throw new ApplicationException("MISSING REQUIRED CONFIG SECTION: AzureBlobConfigurations");

        // Create blob clients and register their configs as well
        int i = 0;
        foreach (var config in blobConfigs)
        {
            if (config.ServiceName == null)
            {
                throw new ApplicationException("MISSING REQUIRED CONFIG VALUE: AzureBlobConfiguration[i]:ServiceName");
            }

            string? connectionString = builder.Configuration.GetConnectionString(config.ServiceName);
            if (connectionString != null)
            {
                clients.AddBlobServiceClient(connectionString).WithName(config.ServiceName);
            }
            else if (config.PrimaryServiceUrl != null)
            {
                clients.AddBlobServiceClient(config.PrimaryServiceUrl).WithName(config.ServiceName).WithCredential(credential);
            }
            else
            {
                throw new ApplicationException("MISSING REQUIRED CONFIG VALUES: Azure Blob ConnString OR Azure Blob URL");
            }

            builder.Services.Configure<AzureBlobConfiguration>(
                config.ServiceName,
                builder.Configuration.GetSection($"AzurePlatform:AzureBlobConfigurations:{i++}")
            );
        }
    }
});

// Setup the remaining services

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Employers", policy =>
                  policy.RequireRole(UserRoleConstants.USER_ROLE_EMPLOYER));

    options.AddPolicy("LoggedInUsers", policy =>
                  policy.RequireAuthenticatedUser());
});
builder.Services.AddAuthentication("Bearer").AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerValidator = (string issuer, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
        {
            var usersJwtIssuer = builder.Configuration["Authentication:Schemes:Bearer:ValidIssuer"];
            var appJwtIssuer = builder.Configuration["Jwt:Issuer"];

            if ((usersJwtIssuer != null && issuer == usersJwtIssuer) || (appJwtIssuer != null && issuer == appJwtIssuer))
            {
                return issuer;
            }

            throw new SecurityTokenInvalidIssuerException($"Invalid Issuer: {issuer}");
        },
        IssuerSigningKeyResolver = (string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters) =>
        {
            var logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

            List<SecurityKey> keys = new();

            if (builder.Environment.IsDevelopment())
            {
                // Add key for dotnet-user-jwts
                var userJwtKey = builder.Configuration["Authentication:Schemes:Bearer:SigningKeys:0:Value"];
                if (userJwtKey != null)
                {
                    keys.Add(new SymmetricSecurityKey(Convert.FromBase64String(userJwtKey)));
                }
                else
                {
                    logger.LogError("The dotnet-user-jwts Jwt Signing Key is missing from Config!");
                }
            }

            // Add key for the app
            var appJwtKey = builder.Configuration["Jwt:SigningKey"];
            if (appJwtKey != null)
            {
                keys.Add(new SymmetricSecurityKey(Convert.FromBase64String(appJwtKey)));
            }
            else
            {
                logger.LogError("The application Jwt Signing Key is missing from Config!");
            }

            return keys;
        },
        ValidAudiences = builder.Configuration.GetSection("Authentication:Schemes:Bearer:ValidAudiences").Get<string[]>(),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

// CONFIGURATION FOR IOPTIONS
builder.Services.Configure<AzurePlatformConfiguration>(builder.Configuration.GetSection("AzurePlatform"));
builder.Services.Configure<Neo4JConfiguration>(builder.Configuration.GetSection("Neo4J"));
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<ClientAppConfiguration>(builder.Configuration.GetSection("ClientApp"));

// CONFIGURATION FOR IOPTIONSMONITOR / IOPTIONSSNAPSHOT
{
    // Oauth Config
    var oauthConfigs = builder.Configuration.GetSection("Oauth")
        .GetSection("OauthConfigurations")
        .Get<OauthConfig[]>()
            ?? throw new ApplicationException("MISSING REQUIRED CONFIG SECTION: OauthConfigurations");

    int i = 0;
    foreach (var config in oauthConfigs)
    {
        if (config.GetType().GetProperties().Any(info => info.GetValue(config) is null))
        {
            throw new ApplicationException($"MISSING REQUIRED CONFIG VALUE WITHIN: OauthConfigurations[{i}]");
        }

        builder.Services.Configure<OauthConfig>(
                config.ServiceName,
                builder.Configuration.GetSection($"Oauth:OauthConfigurations:{i++}")
            );
    }
}

// CSRF/XSRF Protection Setup
builder.Services.AddAntiforgery(options =>
{
    options.Cookie = new CookieBuilder
    {
        Name = "ASP-XSRF-TOKEN",
        IsEssential = true,
        SecurePolicy = CookieSecurePolicy.Always
    };
    options.HeaderName = "X-Xsrf-Token";
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// SINGLETONS
/*** Neo4J ***/
builder.Services.AddSingleton<IDriver>(implementationFactory: provider => 
{
    var settings = (provider.GetRequiredService<IOptions<Neo4JConfiguration>>()?.Value) 
        ?? throw new ApplicationException("MISSING REQUIRED CONFIG VALUES: Neo4J Driver");

    return GraphDatabase.Driver(settings.ConnectionURL, AuthTokens.Basic(settings.User, settings.Password));
});
/*** CSP Policy ***/
builder.Services.AddSingleton<CSP>(implementationFactory: provider =>
{
    var policy = CSP.DefaultPolicy.Clone();

    policy.ConnectSrc = $"{CSP.Self} matthews-ats-funcs-staging.azurewebsites.net";
    // The hash is the sha256 hash of the inline script within ClientApp/src/index.html
    policy.ScriptSrc = $"{policy.ScriptSrc} 'sha256-Vywrtc+OVj0nO9NJ0FmjzNfROZD38W0ll9q3KcU4Huk='";
    policy.Sandbox = $"{policy.Sandbox} allow-downloads allow-popups allow-popups-to-escape-sandbox";
    policy.BaseUri = string.Empty;

    return policy;
});
/*** LinkedIn OAuth Service ***/
builder.Services.AddSingleton<ILinkedinOAuthService, LinkedinOAuthService>();

// SCOPED
builder.Services.AddScoped<IValidator<User>, UserValidator>();
builder.Services.AddScoped<IValidator<UserRegistration>, UserRegistrationValidator>();
builder.Services.AddScoped<IValidator<ApplyToJob>, ApplyToJobValidator>();
builder.Services.AddScoped<IValidator<Job>, JobValidator>();
builder.Services.AddScoped(typeof(IDataAccessObject<JobDb>), typeof(JobDao));
builder.Services.AddScoped(typeof(IDataAccessObject<ApplicantDb>), typeof(ApplicantDao));
builder.Services.AddScoped(typeof(IDataAccessObject<User>), typeof(UserDao));
builder.Services.AddScoped(typeof(IDataAccessObject<EmployerDb>), typeof(EmployerDao));
builder.Services.AddScoped<IApplicantRepository, ApplicantRepository>();
builder.Services.AddScoped<IEmployerRepository, EmployerRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IUserService, UserService>();

// TRANSIENT
builder.Services.AddTransient<IMapper, Mapper>();

// HTTP CLIENTS
builder.Services.AddHttpClient("linkedin_client");

var app = builder.Build();
if (useAzureAppConfig)
{
    app.UseAzureAppConfiguration();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<AntiforgeryMiddleware>();  // MUST go after auth middleware
app.UseAuthorization();
app.UseMiddleware<ContentSecurityPolicyMiddleware>();
app.UseWebSockets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
