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
import { JobPageDataService } from "./job-page-data-svc";
import { of } from "rxjs";

const job: Job = {
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
    getJobDetails: (id: string) => of(job)
};

describe('JobPageDataService', () => {
    let jobPageDataService: JobPageDataService;

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
    });

    it('should instantiate', () => {
        expect(jobPageDataService).toBeTruthy();
    });
});