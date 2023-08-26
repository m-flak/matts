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
using matts.Utils;

namespace matts.Repositories;

public class ApplicantRepository : IApplicantRepository
{
    private readonly IDataAccessObject<ApplicantDb> _daoApp;
    private readonly IMapper _mapper;

    public ApplicantRepository(IDataAccessObject<ApplicantDb> daoApp, IMapper mapper)
    {
        _daoApp = daoApp;
        _mapper = mapper;
    }

    public async Task<Applicant> GetApplicantByUuid(string uuid)
    {
        var app = await _daoApp.GetByUuid(uuid);
        return _mapper.Map<Applicant>(app);
    }

    public async Task<DateTime?> ScheduleInterview(Applicant scheduleFor, string jobUuid, DateTime? when)
    {
        var queryRelationship = new DbRelationship(RelationshipConstants.IS_INTERVIEWING_FOR, "r", DbRelationship.Cardinality.UNIDIRECTIONAL);
        var updateRelationship = new DbRelationship(RelationshipConstants.IS_INTERVIEWING_FOR, "r");

        JobDb job = new()
        {
            Uuid = jobUuid
        };
        ApplicantDb applicant = _mapper.Map<ApplicantDb>(scheduleFor);

        // The delete will delete if it exists or do nothing
        if (when == null)
        {
            await _daoApp.DeleteRelationshipBetween(updateRelationship, applicant, job, typeof(JobDb));
            return when;
        }

        bool hasRelationship = await _daoApp.HasRelationshipBetween(queryRelationship, applicant, job, typeof(JobDb));
        if (hasRelationship)
        {
            updateRelationship.Parameters["interviewDate"] = ((DateTime) when).ToUniversalTime().ToString("O");
            await _daoApp.UpdateRelationshipBetween(updateRelationship, applicant, job, typeof(JobDb));
        }
        else
        {
            updateRelationship.Parameters["interviewDate"] = ((DateTime)when).ToUniversalTime().ToString("O");
            await _daoApp.CreateRelationshipBetween(updateRelationship, applicant, job, typeof(JobDb));
        }
        
        return when;
    }

    public async Task<bool> CreateOrRemoveInterviewingWith(bool remove, Applicant interviewee, string? interviewerUuid)
    {
        if (interviewerUuid == null && !remove)
        {
            throw new ArgumentNullException(nameof(interviewerUuid), "A Uuid MUST be provided when `remove` is false.");
        }

        var queryRelationship = new DbRelationship(RelationshipConstants.INTERVIEWING_WITH, "r", DbRelationship.Cardinality.UNIDIRECTIONAL);
        var createRelationship = new DbRelationship(RelationshipConstants.INTERVIEWING_WITH, "r");

        EmployerDb interviewer = new()
        {
            Uuid = interviewerUuid
        };
        ApplicantDb applicant = _mapper.Map<ApplicantDb>(interviewee);

        bool hasRelationship = false; 
        if (interviewerUuid != null)
        {
            hasRelationship = await _daoApp.HasRelationshipBetween(queryRelationship, applicant, interviewer, typeof(EmployerDb));
        }
        
        if (!remove)
        {
            bool createdOrExisted = hasRelationship;
            if (!hasRelationship)
            {
                createdOrExisted = await _daoApp.CreateRelationshipBetween(createRelationship, applicant, interviewer, typeof(EmployerDb));
            }
            return createdOrExisted;
        }
        else
        {
            return await _daoApp.DeleteRelationshipBetween(createRelationship, applicant, null, typeof(EmployerDb));
        }
    }
}
