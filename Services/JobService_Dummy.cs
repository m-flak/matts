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
namespace matts.Services;

using System.Collections.Generic;
using matts.Constants;
using matts.Interfaces;
using matts.Models;

public partial class JobService : IJobService 
{
    private static long JOB_COUNTER = 0;
    private static long APPLICANT_COUNTER = 0;

    private Job CreateJob(string name, string status, List<Applicant>? applicants)
    {
        return new Job
        {
            Id = ++JOB_COUNTER,
            Uuid = System.Guid.NewGuid().ToString(),
            Name = name,
            Status = status,
            Description = $"John Doe Corporation is looking for a talented {name} professional to work in a fast-paced, exciting environment!",
            Applicants = applicants,
            ApplicantCount = applicants?.Count ?? 0
        };
    }

    private Applicant CreateApplicant(string name, Applicant.ProfileImage? profilePic, bool hasInterview=true)
    {
        return new Applicant
        {
            Id = ++APPLICANT_COUNTER,
            Uuid = System.Guid.NewGuid().ToString(),
            Name = name,
            ApplicantPhoto = profilePic,
            Email = $"{name.ToLower().Replace(' ', '.')}@gmail.com",
            PhoneNumber = "615-555-0123",
            InterviewDate = (hasInterview) ? DateTime.UtcNow : null,
            InterviewingWith = (hasInterview) ? System.Guid.NewGuid().ToString() : null
        };
    }

    private List<Job> CreateJobs()
    {
        var applicants = new List<Applicant>
        {
            CreateApplicant("John Doe", null, false),
            CreateApplicant("Jane Doe", null),
            CreateApplicant("John Public", null),
            CreateApplicant("Lee Cardholder", null)
        };

        return new List<Job>
        {
            CreateJob("Full Stack Software Developer", JobConstants.STATUS_OPEN, applicants),
            CreateJob("Junior HR", JobConstants.STATUS_OPEN, applicants),
            CreateJob("Senior HR", JobConstants.STATUS_FILLED, applicants),
            CreateJob("Sanitation Engineer", JobConstants.STATUS_FILLED, null),
            CreateJob("Executive Senior Associate President", JobConstants.STATUS_CLOSED, null),
            CreateJob("CEO", JobConstants.STATUS_CLOSED, null)
        };
    }
}