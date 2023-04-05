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
import { HttpClient } from "@angular/common/http";
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

    getJobDetails(jobId: number) : Observable<Job> {
        const endpoint = `/jobs/jobdetails/${jobId}`;
        return this.http.get(Location.joinWithSlash(this.baseUrl, endpoint))
            .pipe(
                catchError(e => throwError(() => new Error(e))),
                map((j: any) => j || {})
            );
    }
}
