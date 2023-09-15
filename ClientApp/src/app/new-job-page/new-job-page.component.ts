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

import { Component, EventEmitter, OnDestroy, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Job } from '../models';
import { Subscription, first } from 'rxjs';
import { BackendService } from '../services/backend.service';
import { Router } from '@angular/router';
import { ToastService } from '../services/toast.service';
import { MonitorService } from '../services/monitor.service';

@Component({
  selector: 'app-new-job-page',
  templateUrl: './new-job-page.component.html',
  styleUrls: ['./new-job-page.component.scss']
})
export class NewJobPageComponent implements OnInit, OnDestroy {
  newJobForm: FormGroup;

  @Output()
  jobCreated = new EventEmitter<void>();

  private _subscription: Subscription | null = null;
  
  constructor(
    private formBuilder: FormBuilder, 
    private backendService: BackendService, 
    private toastService: ToastService,
    private monitorService: MonitorService
  ) { 
    this.newJobForm = new FormGroup([]);
  }

  ngOnInit(): void {
    this.newJobForm = this.formBuilder.group({
      jobTitle: ['', Validators.required],
      jobDescription: ['', Validators.required]
    });
  }

  ngOnDestroy(): void {
    if (this._subscription !== null) {
      this._subscription.unsubscribe();
    }
  }

  submitNewJob(): void {
    if (this.newJobForm.invalid) {
      return;
    }

    const formData = this.newJobForm.value;
    this._subscription = this.backendService.postNewJob(this._mapFormToJob(formData)).pipe(first()).subscribe({
      complete: () => {
        this.monitorService.sendSuccess({ id: 'postJob' });
        this.toastService.show(`Successfully Posted Job: ${formData.jobTitle}`, { classname: 'bg-success text-light', delay: 10000 });
        this.jobCreated.emit();
      },
      error: (_) => {
        this.monitorService.sendFailure({ id: 'postJob' });
      }
    });
  }

  private _mapFormToJob(formData: any): Job {
    return {
      name: formData.jobTitle,
      description: formData.jobDescription
    };
  }
}
