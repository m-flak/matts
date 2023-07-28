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

import { Injectable } from "@angular/core";
import { BackendService } from "./backend.service";
import { Observable, Subject, map, switchMap, tap } from "rxjs";
import { ApplyToJob, Job } from "../models";
import { HttpResponse } from "@angular/common/http";

@Injectable({
    providedIn: 'root'
})
export class ApplicantDataService {
    // job uuid, boolean
    private _appliedJobMap: Map<string, boolean> = new Map();
    jobsListSubject: Subject<Job[]> = new Subject<Job[]>();

    constructor(private backendService: BackendService) {}

    updateJobArray(jobArray: Job[]): Job[] {
        return jobArray.map(job => {
            const appliedTo = this._appliedJobMap.get(job?.uuid as string);
            if (appliedTo !== undefined) {
                job.hasAppliedTo = appliedTo;
            }
            return job;
        });
    }
    
    resetService(): void {
        this._appliedJobMap.clear();
    }

    isJobAppliedFor(job: Job): boolean {
        return (this._appliedJobMap.get(job?.uuid as string) !== undefined);
    }

    getOpenAndAppliedJobs(applicantId: string): Observable<Job[]> {
        return this.backendService.getAllAppliedJobs(applicantId).pipe(
            tap(jobs => jobs.forEach(j => this._appliedJobMap.set(j?.uuid as string, true))),
            switchMap(() => this.backendService.getAllJobs()),
            map(jobs => this.updateJobArray(jobs))
        );
    }

    applyToJob(applicantId: string, jobId: string): Observable<HttpResponse<any>> {
        const application: ApplyToJob = {
            jobUuid: jobId,
            applicantUuid: applicantId
        };

        return this.backendService.applyToJob(application).pipe(
            tap(response => {
                if (response.status === 200) {
                    this._appliedJobMap.set(jobId, true);
                }
            })
        );
    }
}