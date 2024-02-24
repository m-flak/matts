/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023-2024  Matthew E. Kehrer <matthew@kehrer.dev>
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
using System.Reflection;
using Json.Schema;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace matts.AzFunctions;
public class Startup
{
    public string? StorageConnectionString { get; set; } = null;

    public void ConfigureAppConfiguration(HostBuilderContext _, IConfigurationBuilder builder)
    {
        builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
        var config = builder.Build();

        this.StorageConnectionString = config.GetValue<string>("AzureWebJobsStorage");
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAzureClients(c =>
        {
            if (StorageConnectionString == null)
            {
                throw new ApplicationException("Connection string for Blob Storage missing from \"AzureWebJobsStorage\"!");
            }

            c.AddBlobServiceClient(StorageConnectionString);
            c.AddTableServiceClient(StorageConnectionString);
            c.AddQueueServiceClient(StorageConnectionString)
                .ConfigureOptions(qc => qc.MessageEncoding = Azure.Storage.Queues.QueueMessageEncoding.Base64);
        });

        services.AddSingleton<SchemaRegistry>(implementationFactory: _ =>
        {
            var registry = SchemaRegistry.Global;
            foreach (var file in Directory.GetFiles("schemas", "*.json"))
            {
                var schema = JsonSchema.FromFile(file);
                registry.Register(schema);
            }
            return registry;
        });

        services.AddSingleton<Func<string>>(implementationFactory: _ =>
        {
            return () =>
            {
                string path = Assembly.GetExecutingAssembly().Location;
                int iDir = 0;
                for (int i = 0; i < path.Length; ++i)
                {
                    if (path[i] == '/' || path[i] == '\\')
                    {
                        iDir = i;
                    }
                }
                return Path.Join(path[..iDir], "schemas");
            };
        });
    }
}
