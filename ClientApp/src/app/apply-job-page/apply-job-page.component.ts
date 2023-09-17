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
import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { BackendService } from '../services/backend.service';
import { AuthService } from '../services/auth.service';
import { Subscription, filter, lastValueFrom, map, switchMap, take, tap } from 'rxjs';
import { Job } from '../models';
import { ApplicantDataService } from '../services/applicant-data.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FileInput, FileValidator } from 'ngx-material-file-input';
import { ConfigService } from '../services/config.service';
import { HttpEventType } from '@angular/common/http';

@Component({
  selector: 'app-apply-job-page',
  templateUrl: './apply-job-page.component.html',
  styleUrls: ['./apply-job-page.component.scss'],
})
export class ApplyJobPageComponent implements OnInit, OnDestroy {
  private _subscription: Subscription | null = null;
  private _subscription2: Subscription | null = null;

  @Input()
  currentJob: Job | null = null;

  applyToJobForm: FormGroup;

  uploadStarted = false;
  uploadingMsg = 'Uploading...';

  maxFileSize!: number;
  allowedFileTypes!: string;
  allowedFileExtensions!: string;

  constructor(
    private formBuilder: FormBuilder,
    private activatedRoute: ActivatedRoute,
    private applicantDataService: ApplicantDataService,
    private backendService: BackendService,
    private authService: AuthService,
    private configService: ConfigService,
  ) {
    this.applyToJobForm = new FormGroup([]);
  }

  ngOnInit(): void {
    this._subscription = this.activatedRoute.paramMap
      .pipe(
        switchMap((params: ParamMap) => this.backendService.getJobDetails(params.get('id') ?? '')),
        map(job => {
          const appliedFor = this.applicantDataService.isJobAppliedFor(job);
          if (appliedFor) {
            return { ...job, hasAppliedTo: appliedFor };
          }
          return job;
        }),
      )
      .subscribe(async data => {
        this.currentJob = data;
      });

    this.maxFileSize = this.configService.config.resumeUploader.maxFileSize as number;
    this.allowedFileTypes = this.configService.config.resumeUploader.allowedFileTypes as string;
    this.allowedFileExtensions = this.configService.config.resumeUploader.allowedFileExtensions as string;

    this.applyToJobForm = this.formBuilder.group({
      resumeFile: [undefined, [Validators.required, FileValidator.maxContentSize(this.maxFileSize)]],
    });
  }

  ngOnDestroy(): void {
    if (this._subscription !== null) {
      this._subscription.unsubscribe();
    }
    if (this._subscription2 !== null) {
      this._subscription2.unsubscribe();
    }
  }

  applyToJob(): void {
    if (this.applyToJobForm.invalid) {
      return;
    }

    const fileControlData: FileInput = this.applyToJobForm.controls.resumeFile.value;
    const uploadData = new FormData();
    uploadData.append('file', fileControlData.files[0]);
    uploadData.append('fileName', fileControlData.fileNames.split(', ')[0]);
    uploadData.append('jobUuid', this.currentJob?.uuid as string);
    uploadData.append('applicantUuid', this.authService.currentUser?.applicantId as string);

    this.uploadStarted = true;
    this._subscription2 = this.backendService
      .uploadResume(uploadData)
      .pipe(
        tap(response => {
          if (response.type === HttpEventType.UploadProgress) {
            const percentDone = Math.round((100 * response.loaded) / (response.total ?? response.loaded));
            this.uploadingMsg = `Uploading ${percentDone}%...`;
          }
        }),
        filter(response => response.type === HttpEventType.Response),
        take(1),
        switchMap(() =>
          this.applicantDataService.applyToJob(
            this.authService.currentUser?.applicantId as string,
            this.currentJob?.uuid as string,
          ),
        ),
      )
      .subscribe(async response => {
        this.uploadStarted = false;
        this.uploadingMsg = 'Uploading...';
        if (response.status === 200) {
          const jobs = await lastValueFrom(
            this.applicantDataService.getOpenAndAppliedJobs(this.authService.currentUser?.applicantId as string),
          );
          this.applicantDataService.jobsListSubject.next(jobs);
          this.currentJob = { ...this.currentJob, hasAppliedTo: true };
        }
      });
  }
}
