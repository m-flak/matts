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

    public async Task<DateTime?> ScheduleInterview(Applicant scheduleFor, string jobUuid, DateTime? when)
    {
        if (when == null)
        {
            // TODO: Remove relationship
            return null;
        }

        var queryRelationship = new DbRelationship(RelationshipConstants.IS_INTERVIEWING_FOR, "r", DbRelationship.Cardinality.BIDIRECTIONAL);
        var updateRelationship = new DbRelationship(RelationshipConstants.IS_INTERVIEWING_FOR, "r");
        updateRelationship.Parameters["interviewDate"] = when;

        JobDb job = new JobDb()
        {
            Uuid = jobUuid
        };
        ApplicantDb applicant = _mapper.Map<ApplicantDb>(scheduleFor);

        bool hasRelationship = await _daoApp.HasRelationshipBetween(queryRelationship, applicant, job, typeof(JobDb));
        if (hasRelationship)
        {
            await _daoApp.UpdateRelationshipBetween(updateRelationship, applicant, job, typeof(JobDb));
        }
        else
        {
            await _daoApp.CreateRelationshipBetween(updateRelationship, applicant, job, typeof(JobDb));
        }
        
        return when;
    }
}