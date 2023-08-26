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
using Microsoft.Extensions.Configuration;
using matts.Interfaces;
using matts.Models;
using matts.Constants;
using Ical.Net;
using System;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using matts.Models.Db;

public partial class JobService : IJobService
{
    private static List<Job>? _jobsDummyData = null;
    private readonly ILogger<JobService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IJobRepository _repository;
    private readonly IApplicantRepository _appRepository;
    private readonly IEmployerRepository _empRepository;

    private bool _useDummyData;

    public JobService(ILogger<JobService> logger, IConfiguration configuration, IJobRepository repository, IApplicantRepository appRepository, IEmployerRepository empRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _repository = repository;
        _appRepository = appRepository;
        _empRepository = empRepository;

        if(_jobsDummyData == null)
        {
            _jobsDummyData = CreateJobs();
        }
        
        ConfigureService();
    }

    public async Task<IEnumerable<Job>> GetJobs()
    {
        if (_useDummyData)
        {
            return _jobsDummyData;
        }

        return await _repository.GetAll();
    }

    public async Task<IEnumerable<Job>> GetOpenJobs()
    {
        if (_useDummyData)
        {
            return _jobsDummyData.Where(j => j.Status == JobConstants.STATUS_OPEN);
        }

        return await _repository.GetAllByStatus(JobConstants.STATUS_OPEN);
    }

    public async Task<IEnumerable<Job>> GetAppliedJobs(string applicantId)
    {
        if (_useDummyData)
        {
            return _jobsDummyData.Where(j => j.Status == JobConstants.STATUS_OPEN).Take(1);
        }

        return await _repository.GetAllAppliedByApplicantId(applicantId);
    }

    public async Task<Job> GetJobDetails(string uuid)
    {
        if (_useDummyData)
        { 
            Job job = _jobsDummyData
                .Where(j => j.Uuid == uuid)
                .First();

            return job;
        }

        return await _repository.GetJobByUuid(uuid);
    }

    public async Task<Job> CreateNewJob(Job newJob)
    {
        if (_useDummyData)
        {
            return newJob;
        }

        return await _repository.CreateNewJob(newJob);
    }

    public async Task<Job> UpdateJob(Job job)
    {
        if (_useDummyData)
        {
            return job;
        }

        if (job.Uuid == null)
        {
            throw new ArgumentNullException(nameof(job.Uuid), "Job.Uuid is null!");
        }
        
        foreach (Applicant applicant in job.Applicants ?? Enumerable.Empty<Applicant>())
        {
            if (applicant.Uuid == null)
            {
                throw new ArgumentNullException(nameof(applicant.Uuid), "Applicant.Uuid is null!");
            }

            // Will remove the interviewing relationship if the date is null
            applicant.InterviewDate = await _appRepository.ScheduleInterview(applicant, job.Uuid, applicant.InterviewDate);
            var interviewingWith = applicant.InterviewingWith;
            if (interviewingWith != null)
            {
                if ( !await _appRepository.CreateOrRemoveInterviewingWith(false, applicant, interviewingWith) )
                {
                    _logger.LogError(
                        "Unable to create {Relationship} between APPLICANT {AppUuid} and EMPLOYER {EmpUuid}.",
                        RelationshipConstants.INTERVIEWING_WITH,
                        applicant.Uuid,
                        interviewingWith
                    );
                }

                if ( !await _empRepository.CreateOrRemoveInterviewingWith(false, interviewingWith, applicant.Uuid) )
                {
                    _logger.LogError(
                        "Unable to create {Relationship} between EMPLOYER {EmpUuid} and APPLICANT {AppUuid}.",
                        RelationshipConstants.INTERVIEWING_WITH,
                        interviewingWith,
                        applicant.Uuid
                    );
                }
            }
            else
            {
                if ( !await _appRepository.CreateOrRemoveInterviewingWith(true, applicant, interviewingWith) )
                {
                    _logger.LogError(
                        "Unable to remove {Relationship} between APPLICANT {AppUuid} and EMPLOYER {EmpUuid}.",
                        RelationshipConstants.INTERVIEWING_WITH,
                        applicant.Uuid,
                        interviewingWith
                    );
                }

                if ( !await _empRepository.CreateOrRemoveInterviewingWith(true, interviewingWith, applicant.Uuid) )
                {
                    _logger.LogError(
                        "Unable to remove {Relationship} between EMPLOYER {EmpUuid} and APPLICANT {AppUuid}.",
                        RelationshipConstants.INTERVIEWING_WITH,
                        interviewingWith,
                        applicant.Uuid
                    );
                }
            }
        }

        return await _repository.UpdateJob(job);
    }

    public async Task<bool> ApplyToJob(ApplyToJob application)
    {
        if (_useDummyData)
        {
            return true;
        }

        if (application.JobUuid == null || application.ApplicantUuid == null)
        {
            return false;
        }

        return await _repository.ApplyToJob(application.JobUuid, application.ApplicantUuid);
    }

    public async Task<bool> RejectForJob(string jobUuid, string applicantUuid, bool isRejected)
    {
        if (_useDummyData)
        {
            return true;
        }

        return await _repository.RejectForJob(jobUuid, applicantUuid, isRejected);
    }

    public async Task<Calendar?> GetICSCalendar(string juuid, string auuid, DateTime dateTime)
    {
        if (_useDummyData) 
        {
            Job job = _jobsDummyData
                .Where(j => j.Uuid == juuid)
                .First();
            
            Applicant applicant = job.Applicants
                .Where(a => a.Uuid == auuid)
                .First();

            var employer = new Employer()
            {
                Name = "Dummy Employer",
                Email = "employer@gmail.com"
            };

            return CreateCalendar(job, applicant, employer, dateTime);
        }

        try
        {
            var theJob = await _repository.GetJobByUuid(juuid);
            var theApplicant = await _appRepository.GetApplicantByUuid(auuid);
            var theEmployer = await _empRepository.GetEmployerInterviewingWith(auuid);
            return CreateCalendar(theJob, theApplicant, theEmployer, dateTime);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to generate the ICS File Payload.");
            return null;
        }
    }

    private void ConfigureService()
    {
        var useDummyData = _configuration.GetValue<bool>("DummyData:JobService", false);
        _useDummyData = useDummyData;
    }

    private Calendar CreateCalendar(Job job, Applicant applicant, Employer employer, DateTime interviewDate)
    {
        var interviewer = new Attendee()
        {
            CommonName = employer.Name,
            Role = "Interviewer",
            Rsvp = true,
            Value = new Uri($"mailto:{employer.Email}")
        };
        var organizer = new Organizer()
        {
            CommonName = employer.Name,
            Value = new Uri($"mailto:{employer.Email}"),
            SentBy = new Uri($"mailto:{employer.Email}")
        };
        var interviewee = new Attendee()
        {
            CommonName = applicant.Name,
            Role = "Interviewee",
            Rsvp = true,
            Value = new Uri($"mailto:{applicant.Email}")
        };

        var interview = new CalendarEvent()
        {
            Description = $"{job.Name} Interview: {applicant.Name}",
            Summary = $"{job.Name} Interview: {applicant.Name}",
            Organizer = organizer,
            Start = new CalDateTime(interviewDate),
            End = new CalDateTime(interviewDate.AddHours(1))
        };
        interview.Attendees = new List<Attendee>()
        {
            interviewer,
            interviewee
        };

        var calendar = new Calendar();
        calendar.Events.Add(interview);
        return calendar;
    }
}
