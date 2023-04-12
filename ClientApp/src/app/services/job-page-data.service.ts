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
import { Applicant, Job } from "../models";
import { Observable, filter, map, of, tap } from "rxjs";
import { parseISO } from "date-fns";

export type JobMapValue = {data: Job, isDirty: boolean};

export interface InterviewDate {
    applicant?: Applicant;
    date?: Date;
}

@Injectable({
    providedIn: 'root'
})
export class JobPageDataService {
    private _jobMap: Map<string, JobMapValue> = new Map();

    constructor(private backendService: BackendService) {}

    _getJob(uuid: string): Job | null {
        const foundJobEntry: JobMapValue | undefined = this._jobMap.get(uuid);

        return (foundJobEntry !== undefined && foundJobEntry.isDirty === false) ? foundJobEntry.data : null;
    }

    _hasJob(uuid: string): boolean {
        return this._jobMap.has(uuid);
    }

    changeJobData(job: Job): Observable<any> {
        return this.backendService.updateJob(job).pipe(
            tap(() => this._jobMap.set(job.uuid as string, { ...(this._jobMap.get(job.uuid as string) as JobMapValue), isDirty: true}))
        );
    }

    getJobByUuid(jobUuid: string): Observable<Job> {
        const foundJob = this._getJob(jobUuid);
        
        if (foundJob !== null) {
            return of(foundJob);
        }

        return this.backendService.getJobDetails(jobUuid).pipe(
            tap(job => this._jobMap.set(jobUuid, { data: job, isDirty: false }))
        );
    }

    getAllInterviewDatesForJob(jobUuid: string): Observable<InterviewDate[]> {
        return this.getJobByUuid(jobUuid).pipe(
            filter(job => job.applicants !== undefined),
            map(job => (job.applicants as Applicant[]).map(applicant => {
                return {
                    applicant: { ...applicant },
                    date: parseISO((applicant.interviewDate as string))
                };
            }))
        );
    }

    resetService() {
        this._jobMap.clear();
    }
}
