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
import { TestBed } from "@angular/core/testing";
import { Applicant, Job } from "../models";
import { BackendService } from "./backend.service";
import { JobPageDataService } from "./job-page-data.service";
import { concat, merge, of, switchMap, tap } from "rxjs";
import { sha1 } from 'object-hash';
import { JobConstants } from "../constants";
import { HttpResponse } from "@angular/common/http";

const jobData: Job = {
    "id": 1,
    "uuid": "54991ebe-ba9e-440b-a202-247f0c33574f",
    "name": "Full Stack Software Developer",
    "status": "OPEN",
    "applicants": [
        {
            "id": 1,
            "uuid": "db185379-70e8-4ec6-b5f7-370415ca3b43",
            "name": "John Doe",
            "applicantPhoto": null,
            "interviewDate": "2023-04-11T16:10:30.0273024Z"
        },
        {
            "id": 2,
            "uuid": "cb9057c9-b504-4add-a5b8-0113ef08e9e4",
            "name": "Jane Doe",
            "applicantPhoto": null,
            "interviewDate": "2023-04-11T16:10:30.0273449Z"
        },
        {
            "id": 3,
            "uuid": "ef4e92bc-8027-4fd2-9a13-450a1cfb8697",
            "name": "John Public",
            "applicantPhoto": null,
            "interviewDate": "2023-04-11T16:10:30.0273453Z"
        },
        {
            "id": 4,
            "uuid": "3b785136-cafb-48ed-b58f-1b2150f74bf6",
            "name": "Lee Cardholder",
            "applicantPhoto": null,
            "interviewDate": "2023-04-11T16:10:30.0273464Z"
        }
    ]
};

const FakeBackendService = {
    getJobDetails: (id: string) => of(jobData),
    updateJob: (job: Job) => of(new HttpResponse<any>())
};

describe('JobPageDataService', () => {
    let jobPageDataService: JobPageDataService;
    let backendService: BackendService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            declarations: [],
            imports: [],
            providers: [
                { provide: BackendService, useValue: FakeBackendService },
                JobPageDataService
            ]
        });

        jobPageDataService = TestBed.inject(JobPageDataService);
        backendService = TestBed.inject(BackendService);
    });

    afterEach(() => {
        jobPageDataService.resetService();
    });

    it('should instantiate', () => {
        expect(jobPageDataService).toBeTruthy();
    });

    it('should store the job in the map', (done) => {
        jobPageDataService.getJobByUuid('54991ebe-ba9e-440b-a202-247f0c33574f').subscribe(job => {
            const hashReturned = sha1(job);
            const hashMap = sha1(jobPageDataService._getJob('54991ebe-ba9e-440b-a202-247f0c33574f'));
            const hashExpected = sha1(jobData);

            expect(hashReturned).toEqual(hashExpected);
            expect(hashMap).toEqual(hashExpected);
            done();
        });
    });

    it('should mark a changed job as dirty', (done) => {
        const changedJob = { ...jobData };
        changedJob.status = JobConstants.STATUS_CLOSED;

        spyOn(backendService, 'updateJob').and.callThrough();

        jobPageDataService.changeJobData(changedJob).subscribe(() => {
            expect(backendService.updateJob).toHaveBeenCalled();
            expect(jobPageDataService._getJob('54991ebe-ba9e-440b-a202-247f0c33574f')).toBeNull();
            expect(jobPageDataService._hasJob('54991ebe-ba9e-440b-a202-247f0c33574f')).toBe(true);
            done();
        });
    });

    it('should get the interview dates associated with a job', (done) => {
        spyOn(backendService, 'getJobDetails').and.callThrough();

        jobPageDataService.getJobByUuid('54991ebe-ba9e-440b-a202-247f0c33574f').subscribe(job => {
            jobPageDataService.setCurrentJob(job);
            expect(backendService.getJobDetails).toHaveBeenCalled();

            const dates = jobPageDataService.getAllInterviewDatesForJob(job.uuid as string);
            expect(dates.length).toEqual(4);
            dates.forEach(interviewDate => {
                expect(interviewDate.applicant).toBeTruthy();

                // For some reason, we'll occasionally get NaN. :/
                if (!isNaN((interviewDate.date as Date).getMonth()) && !isNaN((interviewDate.date as Date).getDate())) {
                    expect((interviewDate.date as Date).getMonth()+1).toEqual(4);
                    expect((interviewDate.date as Date).getDate()).toEqual(11);
                }
                else {
                    console.error('Got NaN... :(');
                }
            });
            done();
        });

    });

    it('should have all applicant uuids per job uuid', (done) => {
        spyOn(backendService, 'getJobDetails').and.callThrough();

        jobPageDataService.getJobByUuid('54991ebe-ba9e-440b-a202-247f0c33574f').subscribe(job => {
            jobPageDataService.setCurrentJob(job);
            const applicants = jobPageDataService.jobApplicants.get('54991ebe-ba9e-440b-a202-247f0c33574f');

            expect(applicants).toBeDefined();
            expect(applicants?.size).toEqual(4);

            expect(jobPageDataService.applicants.size).toEqual(4);
            done();
        });
    });

    it('should have the mutated applicants in the patch call', (done) => {
        spyOn(backendService, 'getJobDetails').and.callThrough();
        spyOn(backendService, 'updateJob').and.callFake(job => {
            const applicant = job.applicants?.filter(a => a.uuid === 'db185379-70e8-4ec6-b5f7-370415ca3b43').pop();
            expect(applicant?.interviewDate).toBeUndefined();
            return of(new HttpResponse<any>());
        });

        const a = jobPageDataService.getJobByUuid('54991ebe-ba9e-440b-a202-247f0c33574f').pipe(
            tap(job => jobPageDataService.setCurrentJob(job)),
            tap(() => {
                const applicant = jobPageDataService.applicants.get('db185379-70e8-4ec6-b5f7-370415ca3b43');
                (applicant as Applicant).interviewDate = undefined;
                jobPageDataService.applicants.set('db185379-70e8-4ec6-b5f7-370415ca3b43', applicant as Applicant);
            }),
            switchMap(() => jobPageDataService.changeJobData(jobPageDataService.getCurrentJob()))
        ).subscribe(() => {
            expect(backendService.updateJob).toHaveBeenCalled();
            done();
        });
    });
});