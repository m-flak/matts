/* matts
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
using FluentValidation;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Neo4j.Driver;
using matts.Models;
using matts.Models.Db;
using matts.Interfaces;
using matts.Services;
using matts.Daos;
using matts.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthorization(options =>
{
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

            if ( (usersJwtIssuer != null && issuer == usersJwtIssuer) || (appJwtIssuer != null && issuer == appJwtIssuer) )
            {
                return issuer;
            }

            throw new SecurityTokenInvalidIssuerException($"Invalid Issuer: {issuer}");
        },
        IssuerSigningKeyResolver = (string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters) =>
        {
            var logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
            
            List<SecurityKey> keys = new List<SecurityKey>();
            
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
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddControllersWithViews();

// SINGLETONS
builder.Services.AddSingleton<IDriver>(implementationFactory: provider => {
    var connectionUrl = builder.Configuration["Neo4J:ConnectionURL"];
    var username = builder.Configuration["Neo4J:User"];
    var password = builder.Configuration["Neo4J:Password"];

    return GraphDatabase.Driver(connectionUrl, AuthTokens.Basic(username, password));
});


// SCOPED
builder.Services.AddScoped<IValidator<User>, UserValidator>();
builder.Services.AddScoped(typeof(IDataAccessObject<JobDb>), typeof(JobDao));
builder.Services.AddScoped(typeof(IDataAccessObject<ApplicantDb>), typeof(ApplicantDao));
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();

// TRANSIENT
builder.Services.AddTransient<IMapper, Mapper>();

var app = builder.Build();

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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
