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
namespace matts.Interfaces;

using System;
using Ical.Net;
using matts.Models;

public interface IJobService
{
    public Task<IEnumerable<Job>> GetJobs();

    public Task<IEnumerable<Job>> GetOpenJobs();

    public Task<IEnumerable<Job>> GetAppliedJobs(string applicantId);

    public Task<Job> GetJobDetails(string uuid);

    public Task<Job> CreateNewJob(Job newJob);

    public Task<bool> ApplyToJob(ApplyToJob application);

    public Task<bool> RejectForJob(string jobUuid, string applicantUuid, bool isRejected);
    
    public Task<Calendar?> GetICSCalendar(string juuid, string auuid, DateTime dateTime);
}
