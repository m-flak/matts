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
import { Observable, Subject, filter, map, of, tap } from "rxjs";
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
    // Holds jobs and also does caching
    private _jobMap: Map<string, JobMapValue> = new Map();
    
    private _currentJob: Job | null = null;
    private _currentApplicant: Applicant | null = null;
    currentJobSubject: Subject<Job> = new Subject<Job>();
    currentApplicantSubject: Subject<Applicant> = new Subject<Applicant>();

    // Applicants <--> Job association map
    public jobApplicants: Map<string, Set<string>> = new Map();
    // Applicants
    public applicants: Map<string, Applicant> = new Map();

    constructor(private backendService: BackendService) {}

    _getJob(uuid: string): Job | null {
        const foundJobEntry: JobMapValue | undefined = this._jobMap.get(uuid);

        return (foundJobEntry !== undefined && foundJobEntry.isDirty === false) ? foundJobEntry.data : null;
    }

    _hasJob(uuid: string): boolean {
        return this._jobMap.has(uuid);
    }

    assignCurrentApplicant(applicantUuid: string) {
        const applicant = this.applicants.get(applicantUuid) as Applicant;
        this._currentApplicant = applicant;
        this.currentApplicantSubject.next(applicant);
    }

    updateApplicantDetails(applicant: Applicant) {
        const uuid = applicant.uuid as string;
        this.applicants.set(uuid, applicant);
        this._currentApplicant = applicant;
        this.currentApplicantSubject.next(applicant);
    }

    setCurrentJob(job: Job) {
        this._currentJob = job;

        const set = new Set<string>();
        job.applicants?.forEach(app => {
            this.applicants.set(app.uuid as string, app);
            set.add(app.uuid as string);
        });

        this.jobApplicants.set(job.uuid as string, set);
        this.currentJobSubject.next(this._currentJob);
    }

    getCurrentJob(): Job {
        const returnJob = { ...this._currentJob };
        returnJob.applicants = [];

        const set: Set<string> = this.jobApplicants.get(returnJob.uuid as string) ?? new Set<string>();
        for (const applicantUuid of set) {
            returnJob.applicants.push(this.applicants.get(applicantUuid) as Applicant);
        }

        return returnJob;
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

    getAllInterviewDatesForJob(jobUuid: string): InterviewDate[] {
        let dates: InterviewDate[] = [];

        const set: Set<string> = this.jobApplicants.get(jobUuid) ?? new Set<string>();
        for (const applicantUuid of set) {
            const applicant = this.applicants.get(applicantUuid);
            dates.push({
                applicant: { ...applicant },
                date: parseISO((applicant?.interviewDate as string))
            });
        }

        return dates;
    }

    resetService() {
        this._jobMap.clear();
        this.jobApplicants.clear();
        this.applicants.clear();
    }
}
