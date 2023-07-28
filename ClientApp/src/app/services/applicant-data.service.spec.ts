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
import { ApplicantDataService } from "./applicant-data.service";
import { BackendService } from "./backend.service";
import { Job } from "../models";
import { of } from "rxjs";

const allJobs: Job[] = [
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
const appliedJobs: Job[] = [
    {
        "id": 1,
        "uuid": "4b4d7c64-ef5d-4379-add3-a3f6adc42f01",
        "name": "Full Stack Software Developer",
        "status": "OPEN",
        "description": "John Doe Corporation is looking for a talented Full Stack Software Developer professional to work in a fast-paced, exciting environment!",
    }
];

const FakeBackendService = {
    getAllAppliedJobs: (applicantId: string) => of(appliedJobs),
    getAllJobs: () => of(allJobs)
};

describe('ApplicantDataService', () => {
    let applicantDataService: ApplicantDataService;
    let backendService: BackendService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            declarations: [],
            imports: [],
            providers: [
                { provide: BackendService, useValue: FakeBackendService },
                ApplicantDataService
            ]
        });

        applicantDataService = TestBed.inject(ApplicantDataService);
        backendService = TestBed.inject(BackendService);
    });

    afterEach(() => {
        applicantDataService.resetService();
    });

    it('should instantiate', () => {
        expect(applicantDataService).toBeTruthy();
    });

    it('should get the job list and track if jobs were applied to', (done) => {
        applicantDataService.getOpenAndAppliedJobs('applicantId').subscribe(jobs => {
            expect(jobs[0].hasAppliedTo).toBe(true);
            expect(jobs[1].hasAppliedTo).toBeUndefined();
            done();
        });
    });
});
