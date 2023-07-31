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
import { TestBed} from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClientModule} from "@angular/common/http";
import { BackendService } from './backend.service';
import { JobConstants } from '../constants';
import { ApplyToJob, Job } from '../models';

describe('BackendService', () => {
    let httpMock: HttpTestingController;
    let backendService: BackendService;

    beforeEach(() => {
      TestBed.configureTestingModule({
        declarations: [],
        imports: [
            HttpClientModule,
            HttpClientTestingModule
        ],
        providers: [
            { provide: 'BASE_URL', useValue: '' },
            BackendService
        ]
      });

      backendService = TestBed.inject(BackendService);
      httpMock = TestBed.inject(HttpTestingController);
    });

    it('should get jobs from backend', (done) => {
        const jobs = [
            {
                id: 1,
                name: 'Tester',
                status: JobConstants.STATUS_OPEN,
                applicants: []
            }
        ];

        backendService.getAllJobs().subscribe(jobbies => {
            expect(jobbies.length).toEqual(1);
            expect(jobbies[0].id).toEqual(1);
            expect(jobbies[0].name).toEqual('Tester');
            expect(jobbies[0].status).toEqual(JobConstants.STATUS_OPEN);
            done();
        });

        const request = httpMock.expectOne('/jobs/getjobs');
        request.flush(jobs);

        httpMock.verify();
    });

    it('should get applied jobs from backend', (done) => {
        const appliedJobs: Job[] = [
            {
                "id": 1,
                "uuid": "4b4d7c64-ef5d-4379-add3-a3f6adc42f01",
                "name": "Full Stack Software Developer",
                "status": "OPEN",
                "description": "John Doe Corporation is looking for a talented Full Stack Software Developer professional to work in a fast-paced, exciting environment!",
            },
            {
                "id": 2,
                "uuid": "eab3e2e8-f5a1-41c1-aa1d-1ad7eb6f3a96",
                "name": "Junior HR",
                "status": "OPEN",
                "description": "John Doe Corporation is looking for a talented Junior HR professional to work in a fast-paced, exciting environment!",
            }
        ];

        backendService.getAllAppliedJobs('applicantId').subscribe(jobs=> {
            expect(jobs.length).toEqual(2);
            done();
        });

        const request = httpMock.expectOne('/jobs/getappliedjobs?applicantId=applicantId');
        request.flush(appliedJobs);

        httpMock.verify();
    });

    it('should get job details from backend', (done) => {
        const job = {
            id: 1,
            uuid: '3ebb58d3-a24c-499b-8c65-75636e7b57de',
            name: 'Tester',
            status: JobConstants.STATUS_OPEN,
            applicants: []
        };

        backendService.getJobDetails(job.uuid).subscribe(jobb => {
            expect(jobb.id).toEqual(1);
            expect(jobb.name).toEqual('Tester');
            expect(jobb.status).toEqual(JobConstants.STATUS_OPEN);
            done();
        });

        const request = httpMock.expectOne('/jobs/jobdetails/3ebb58d3-a24c-499b-8c65-75636e7b57de');
        request.flush(job);

        httpMock.verify();
    });

    it('should apply to a job using backend', (done) => {
        const application: ApplyToJob = {
            jobUuid: '3ebb58d3-a24c-499b-8c65-75636e7b57de',
            applicantUuid: '3b785136-cafb-48ed-b58f-1b2150f74bf6'
        };

        backendService.applyToJob(application).subscribe(response => {
            expect(response.status).toEqual(200);
            done();
        });

        const request = httpMock.expectOne('/jobs/applytojob');
        request.flush(null, { status: 200, statusText: 'OK' });

        httpMock.verify();
    });

    it('should update job details using backend', (done) => {
        const job = {
            id: 1,
            name: 'Tester',
            status: JobConstants.STATUS_OPEN,
            applicants: []
        };

        backendService.updateJob(job).subscribe(response => {
            expect(response.status).toEqual(200);
            done();
        });

        const request = httpMock.expectOne('/jobs/updatejob');
        request.flush(null, { status: 200, statusText: 'OK' });

        httpMock.verify();
    });

    it('should post a new job using backend', (done) => {
        const job = {
            name: 'Tester',
            description: 'A cool testing job!'
        };

        backendService.postNewJob(job).subscribe(response => {
            expect(response.status).toEqual(200);
            done();
        });

        const request = httpMock.expectOne('/jobs/postnewjob');
        request.flush(null, { status: 200, statusText: 'OK' });

        httpMock.verify();
    });

    it('should reject an applicant from a job using backend', (done) => {
        const jobUuid = '3ebb58d3-a24c-499b-8c65-75636e7b57de';
        const applicantUuid = '3b785136-cafb-48ed-b58f-1b2150f74bf6';

        backendService.rejectForJob(jobUuid, applicantUuid).subscribe(response => {
            expect(response.status).toEqual(200);
            done();
        });

        const request = httpMock.expectOne(`/jobs/reject/${jobUuid}/${applicantUuid}`);
        request.flush(null, { status: 200, statusText: 'OK' });

        httpMock.verify();
    });
});
