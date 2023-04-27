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

import { Location } from "@angular/common";
import { HttpClient, HttpHeaders, HttpResponse } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { Observable, catchError, throwError, map } from "rxjs";
import { Job } from "../models";

@Injectable({
    providedIn: 'root'
})
export class BackendService {
    constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {}

    getAllJobs() : Observable<Job[]> {
        const endpoint = '/jobs/getjobs';
        return this.http.get(Location.joinWithSlash(this.baseUrl, endpoint))
            .pipe(
                catchError(e => throwError(() => new Error(e))),
                map((r: any) => r || [])
            );
    }

    getJobDetails(jobUuid: string) : Observable<Job> {
        const endpoint = `/jobs/jobdetails/${jobUuid}`;
        return this.http.get(Location.joinWithSlash(this.baseUrl, endpoint))
            .pipe(
                catchError(e => throwError(() => new Error(e))),
                map((j: any) => j || {})
            );
    }

    updateJob(job: Job) : Observable<HttpResponse<any>> {
        const endpoint = `/jobs/updatejob`;

        const httpHeaders = new HttpHeaders({
            'Content-Type' : 'application/json'
        });

        return this.http.patch(Location.joinWithSlash(this.baseUrl, endpoint), job, { headers: httpHeaders, observe: "response" })
            .pipe(
                catchError(e => throwError(() => new Error(e)))
            );
    }

    rejectForJob(jobUuid: string, applicantUuid: string) : Observable<HttpResponse<any>> {
        const endpoint = `/jobs/reject/${jobUuid}/${applicantUuid}`;

        return this.http.post(Location.joinWithSlash(this.baseUrl, endpoint), null, { observe: "response" })
            .pipe(
                catchError(e => throwError(() => new Error(e)))
            );
    }
}
