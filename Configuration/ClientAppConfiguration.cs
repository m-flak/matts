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

public sealed class ClientAppExternalApis
{
    public Uri? ResumeUploadEndpoint { get; set; }

    public string? ResumeUploadApiKey { get; set; }
}

public sealed class ClientAppBranding
{
    public Uri? NavBrandingImage { get; set; }
}

public sealed class ClientAppResumeUploader
{
    public long? MaxFileSize { get; set; }
    // Controls the uploader
    public string? AllowedFileTypes { get; set; }
    // Controls what the user sees. Purely informational.
    public string? AllowedFileExtensions { get; set; }
}

public sealed class ClientAppLinkedinOauth
{
    public string? ClientId { get; set; }
    public Uri? RedirectUri { get; set; }
    public string? Scope { get; set; }
}

public sealed class ClientAppConfiguration
{
    public ClientAppExternalApis? ExternalApis { get; set; }
    public ClientAppBranding? Branding { get; set; }
    public ClientAppResumeUploader? ResumeUploader  { get; set; }
    public ClientAppLinkedinOauth? LinkedinOauth { get; set; }
}
