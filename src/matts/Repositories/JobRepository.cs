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
using matts.Daos;
using matts.Interfaces;
using matts.Models;
using matts.Models.Db;
using matts.Utils;

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

    public async Task<List<Job>> GetAllAppliedByApplicantId(string applicantId)
    {
        var jobs = await _daoJob.GetAllByRelationship(
            new DbRelationship<ApplicantDb, JobDb>(RelationshipConstants.HAS_APPLIED_TO, "r"), 
            null, 
            applicantId
        );

        return jobs.Select(j => _mapper.Map<Job>(j)).ToList();
    }

    public async Task<List<Job>> GetAllByStatus(string status)
    {
        var statusPropertyFilter = new Dictionary<string, object>();
        statusPropertyFilter.Add("status", status);

        var jobs = await _daoJob.GetAllAndFilterByProperties(statusPropertyFilter);
        return jobs.Select(j => _mapper.Map<Job>(j)).ToList();
    }

    public async Task<Job> GetJobByUuid(string uuid)
    {
        var job = await _daoJob.GetByUuid(uuid);
        var applicants = await _daoApp.GetAllByRelationship(
            new DbRelationship<ApplicantDb, JobDb>(RelationshipConstants.HAS_APPLIED_TO, "r"), 
            new DbRelationship<ApplicantDb, JobDb>(RelationshipConstants.IS_INTERVIEWING_FOR, "r2"), 
            uuid
        );

        var interviewingWiths = await _daoApp.GetPropertyFromRelated<string>(RelationshipConstants.INTERVIEWING_WITH, typeof(EmployerDb), "uuid");
        int n = interviewingWiths.Count;
        int i = 0;
        foreach (ApplicantDb applicant in applicants)
        {
            if (i >= n)
            {
                break;
            }
            if (applicant.InterviewDate != null)
            {
                applicant.InterviewingWith = interviewingWiths[i++];
            }
        }

        var returnJob = _mapper.Map<Job>(job);
        returnJob.Applicants = applicants.Select(a => _mapper.Map<Applicant>(a)).ToList();
        return returnJob;
    }

    public async Task<Job> CreateNewJob(Job job)
    {
        JobDb toCreate = _mapper.Map<JobDb>(job);
        toCreate.Status = JobConstants.STATUS_OPEN;
        return _mapper.Map<Job>( await _daoJob.CreateNew(toCreate) );
    }

    public async Task<bool> ApplyToJob(string jobUuid, string applicantUud)
    {
        JobDb job = new JobDb()
        {
            Uuid = jobUuid
        };
        ApplicantDb applicant = new ApplicantDb()
        {
            Uuid = applicantUud
        };

        var applyForRelationship = new DbRelationship(RelationshipConstants.HAS_APPLIED_TO);
        applyForRelationship.Parameters["rejected"] = false;
        return await _daoApp.CreateRelationshipBetween(applyForRelationship, applicant, job, typeof(JobDb));
    }

    public async Task<bool> RejectForJob(string jobUuid, string applicantUuid, bool isRejected)
    {
        var queryRelationship = new DbRelationship(RelationshipConstants.HAS_APPLIED_TO, "r", DbRelationship.Cardinality.BIDIRECTIONAL);
        var updateRelationship = new DbRelationship(RelationshipConstants.HAS_APPLIED_TO, "r");
        updateRelationship.Parameters["rejected"] = isRejected;

        JobDb job = new JobDb()
        {
            Uuid = jobUuid
        };
        ApplicantDb applicant = new ApplicantDb()
        {
            Uuid = applicantUuid
        };

        bool wasUpdated = false;
        bool doesExist = await _daoJob.HasRelationshipBetween(queryRelationship, job, applicant, typeof(ApplicantDb));
        if (doesExist)
        {
            wasUpdated = await _daoApp.UpdateRelationshipBetween(updateRelationship, applicant, job, typeof(JobDb));
        }

        return wasUpdated;
    }

    public async Task<Job> UpdateJob(Job job)
    {
        if (job.Uuid == null || job.Status == null)
        {
            throw new ArgumentNullException("Uuid and status should not be null here.");
        }

        var dao = (JobDao) _daoJob;
        job.Status = await dao.UpdateJobStatus(job.Uuid, job.Status);
        return job;
    }
}
