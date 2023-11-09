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

import { Location } from '@angular/common';
import { HttpClient, HttpErrorResponse, HttpEvent, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, catchError, throwError, map, tap } from 'rxjs';
import { ApplyToJob, Job } from '../models';
import { getDate } from 'date-fns';
import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root',
})
export class BackendService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private configService: ConfigService,
  ) {}

  getAllJobs(): Observable<Job[]> {
    const endpoint = '/api/v1/jobs/getjobs';
    return this.http.get(Location.joinWithSlash(this.baseUrl, endpoint)).pipe(
      catchError((e: HttpErrorResponse) => throwError(() => new Error(e?.error))),
      map((r: any) => r || []),
    );
  }

  getAllAppliedJobs(applicantId: string): Observable<Job[]> {
    const endpoint = '/api/v1/jobs/getappliedjobs';
    return this.http
      .get(Location.joinWithSlash(this.baseUrl, endpoint), { params: new HttpParams().set('applicantId', applicantId) })
      .pipe(
        catchError((e: HttpErrorResponse) => throwError(() => new Error(e?.error))),
        map((r: any) => r || []),
      );
  }

  getJobDetails(jobUuid: string): Observable<Job> {
    const endpoint = `/api/v1/jobs/jobdetails/${jobUuid}`;
    return this.http.get(Location.joinWithSlash(this.baseUrl, endpoint)).pipe(
      catchError((e: HttpErrorResponse) => throwError(() => new Error(e?.error))),
      map((j: any) => j || {}),
    );
  }

  applyToJob(applyToJob: ApplyToJob): Observable<HttpResponse<any>> {
    const endpoint = `/api/v1/jobs/applytojob`;

    const httpHeaders = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    return this.http
      .post(Location.joinWithSlash(this.baseUrl, endpoint), applyToJob, { headers: httpHeaders, observe: 'response' })
      .pipe(catchError((e: HttpErrorResponse) => throwError(() => new Error(e?.error))));
  }

  postNewJob(job: Job): Observable<HttpResponse<any>> {
    const endpoint = `/api/v1/jobs/postnewjob`;

    const httpHeaders = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    return this.http
      .post(Location.joinWithSlash(this.baseUrl, endpoint), job, { headers: httpHeaders, observe: 'response' })
      .pipe(catchError((e: HttpErrorResponse) => throwError(() => new Error(e?.error))));
  }

  updateJob(job: Job): Observable<HttpResponse<any>> {
    const endpoint = `/api/v1/jobs/updatejob`;

    const httpHeaders = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    return this.http
      .patch(Location.joinWithSlash(this.baseUrl, endpoint), job, { headers: httpHeaders, observe: 'response' })
      .pipe(catchError((e: HttpErrorResponse) => throwError(() => new Error(e?.error))));
  }

  rejectForJob(jobUuid: string, applicantUuid: string, isRejected: boolean): Observable<HttpResponse<any>> {
    const endpoint = `/api/v1/jobs/reject/${jobUuid}/${applicantUuid}`;

    return this.http
      .post(Location.joinWithSlash(this.baseUrl, endpoint), null, {
        observe: 'response',
        params: new HttpParams().set('rejected', isRejected),
      })
      .pipe(catchError((e: HttpErrorResponse) => throwError(() => new Error(e?.error))));
  }

  downloadIcs(jobUuid: string, applicantUuid: string, interviewDate: Date): Observable<Blob> {
    const endpoint = `/api/v1/jobs/ics/${jobUuid}/${applicantUuid}`;
    const params = new HttpParams({
      fromObject: {
        y: interviewDate.getFullYear(),
        m: interviewDate.getMonth() + 1,
        d: getDate(interviewDate),
        h: interviewDate.getHours(),
        mm: interviewDate.getMinutes(),
      },
    });

    return this.http
      .get(Location.joinWithSlash(this.baseUrl, endpoint), {
        params: params,
        responseType: 'blob',
        observe: 'response',
      })
      .pipe(
        catchError((e: HttpErrorResponse) => throwError(() => new Error(e?.error))),
        tap(response => {
          if (response.body === null) {
            throwError(() => new Error('Download ICS Error: Received No Content from the Backend'));
          }
        }),
        map(response => response.body as Blob),
      );
  }

  uploadResume(formData: FormData): Observable<HttpEvent<any>> {
    const endpoint = this.configService.config.externalApis.resumeUploadEndpoint;
    const apiKey = this.configService.config.externalApis.resumeUploadApiKey;

    const params = new HttpParams({
      fromObject: {
        code: apiKey,
      },
    });

    return this.http.post(endpoint, formData, { params: params, reportProgress: true, observe: 'events' });
  }
}
