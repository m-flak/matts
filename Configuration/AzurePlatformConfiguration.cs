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
namespace matts.Configuration;

public sealed class AzurePlatformConfiguration
{
    public bool? UseAzureAppConfiguration { get; set; }
    public AzureAppConfiguration? AzureAppConfiguration { get; set; }
    public bool? UseAzureBlobService { get; set; }
    public AzureBlobConfiguration[]? AzureBlobConfigurations { get; set; }
}

public sealed class AzureAppConfiguration
{
    // Used with Managed Identity
    public Uri? PrimaryServiceUrl { get; set; }
}

public sealed class AzureBlobConfiguration
{
    // Name used to identify the config and the client
    public string? ServiceName { get; set; }
    public Uri? PrimaryServiceUrl { get; set; }
    public string? ContainerName { get; set; }
}
