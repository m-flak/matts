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

    it('should get job details from backend', (done) => {
        const job = {
            id: 1,
            name: 'Tester',
            status: JobConstants.STATUS_OPEN,
            applicants: []
        };

        backendService.getJobDetails(1).subscribe(jobb => {
            expect(jobb.id).toEqual(1);
            expect(jobb.name).toEqual('Tester');
            expect(jobb.status).toEqual(JobConstants.STATUS_OPEN);
            done();
        });

        const request = httpMock.expectOne('/jobs/jobdetails/1');
        request.flush(job);

        httpMock.verify();
    });
});
