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
using Mapster;
using MapsterMapper;
using matts.Constants;
using matts.Interfaces;
using matts.Models;
using matts.Models.Db;

namespace matts.Repositories;

public class JobRepository : IJobRepository
{
    private readonly IDataAccessObject<JobDb> _daoJob;
    private readonly IDataAccessObject<ApplicantDb> _daoApp;
    private readonly IMapper _mapper;

    public JobRepository(IDataAccessObject<JobDb> daoJob, IDataAccessObject<ApplicantDb> daoApp, IMapper mapper)
    {
        _daoJob = daoJob;
        _daoApp = daoApp;
        _mapper = mapper;
    }

    public async Task<List<Job>> GetAll()
    {
        var jobs = await _daoJob.GetAll();
        return jobs.Select(j => _mapper.Map<Job>(j)).ToList();
    }

    public async Task<List<Job>> GetAllByStatus(string status)
    {
        var statusPropertyFilter = new Dictionary<string, string>();
        statusPropertyFilter.Add("status", status);

        var jobs = await _daoJob.GetAllAndFilterByProperties(statusPropertyFilter);
        return jobs.Select(j => _mapper.Map<Job>(j)).ToList();
    }

    public async Task<Job> GetJobByUuid(string uuid)
    {
        var job = await _daoJob.GetByUuid(uuid);
        var applicants = await _daoApp.GetAllByRelationship(RelationshipConstants.HAS_APPLIED_TO, RelationshipConstants.IS_INTERVIEWING_FOR, uuid);

        var returnJob = _mapper.Map<Job>(job);
        returnJob.Applicants = applicants.Select(a => _mapper.Map<Applicant>(a)).ToList();
        return returnJob;
    }
}
