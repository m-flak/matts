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
using Neo4j.Driver;
using matts.Models;
using matts.Interfaces;
using matts.Services;
using matts.Daos;
using matts.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

// SINGLETONS
builder.Services.AddSingleton<IDriver>(implementationFactory: provider => {
    var connectionUrl = builder.Configuration["Neo4J:ConnectionURL"];
    var username = builder.Configuration["Neo4J:User"];
    var password = builder.Configuration["Neo4J:Password"];

    return GraphDatabase.Driver(connectionUrl, AuthTokens.Basic(username, password));
});

builder.Services.AddSingleton<IJobService, JobService>();

// SCOPED
builder.Services.AddScoped(typeof(IDataAccessObject<Job>), typeof(JobDao));
builder.Services.AddScoped<IJobRepository, JobRepository>();

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


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
