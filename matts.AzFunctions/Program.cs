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
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

string? storageConnString = null;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
        var config = builder.Build();

        storageConnString = config.GetValue<string>("AzureWebJobsStorage");
    })
    .ConfigureServices(s =>
    {
        s.AddAzureClients(c =>
        {
            if (storageConnString == null)
            {
                throw new ApplicationException("Connection string for Blob Storage missing from \"AzureWebJobsStorage\"!");
            }

            c.AddBlobServiceClient(storageConnString);
        });
    })
    .Build();

host.Run();
