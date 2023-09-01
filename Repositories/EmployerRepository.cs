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

public class EmployerRepository : IEmployerRepository
{
    private readonly IDataAccessObject<EmployerDb> _daoEmp;
    private readonly IMapper _mapper;

    public EmployerRepository(IDataAccessObject<EmployerDb> daoEmp, IMapper mapper)
    {
        _daoEmp = daoEmp;
        _mapper = mapper;
    }

    public async Task<bool> CreateOrRemoveInterviewingWith(bool remove, string? interviewerUuid, string intervieweeUuid)
    {
        if (interviewerUuid == null && !remove)
        {
            throw new ArgumentNullException(nameof(interviewerUuid), "A Uuid MUST be provided when `remove` is false.");
        }

        var queryRelationship = new DbRelationship(RelationshipConstants.INTERVIEWING_WITH, "r", DbRelationship.Cardinality.UNIDIRECTIONAL);
        var createRelationship = new DbRelationship(RelationshipConstants.INTERVIEWING_WITH, "r");

        ApplicantDb interviewee = new()
        {
            Uuid = intervieweeUuid
        };

        // Remove using applicant uuid
        if (interviewerUuid == null)
        {
            return await _daoEmp.DeleteRelationshipBetween(createRelationship, null, interviewee, typeof(ApplicantDb));
        }

        EmployerDb employer = new()
        {
            Uuid = interviewerUuid
        };

        bool hasRelationship = await _daoEmp.HasRelationshipBetween(queryRelationship, employer, interviewee, typeof(ApplicantDb));
        if (!remove)
        {
            bool createdOrExisted = hasRelationship;
            if (!hasRelationship)
            {
                createdOrExisted = await _daoEmp.CreateRelationshipBetween(createRelationship, employer, interviewee, typeof(ApplicantDb));
            }
            return createdOrExisted;
        }
        else
        {
            bool removedOrDeleted = !hasRelationship;
            if (!removedOrDeleted)
            {
                removedOrDeleted = await _daoEmp.DeleteRelationshipBetween(createRelationship, employer, null, typeof(ApplicantDb));
            }
            return removedOrDeleted;
        }
    }

    public async Task<Employer> GetEmployerInterviewingWith(string applicantInterviewingWith)
    {
        var results = await _daoEmp.GetAllByRelationship(
            new DbRelationship<EmployerDb, ApplicantDb>(RelationshipConstants.INTERVIEWING_WITH, "r"),
            null, 
            applicantInterviewingWith
        );

        var employer = results.First();
        return _mapper.Map<Employer>(employer);
    }
}
