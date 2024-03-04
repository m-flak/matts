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

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Job } from '../../../models';
import { Subscription } from 'rxjs';
import { Router, ActivatedRoute } from '@angular/router';
import { BackendService } from '../../../services/backend.service';
import { ApplicantDataService } from '../../../services/applicant-data.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-home-applicant',
  templateUrl: './home-applicant.component.html',
  styleUrls: ['./home-applicant.component.scss'],
})
export class HomeApplicantComponent implements OnInit, OnDestroy {
  private _jobSubscription: Subscription | null = null;
  private _jobSubscription2: Subscription | null = null;

  openJobs: Job[] = [];

  showDetailCardMsg = true;
  initialActivate = (_: any) => (this.showDetailCardMsg = false);
  // when clicking the home link, reshow the msg
  deactivate = (_: any) => (this.showDetailCardMsg = true);

  constructor(
    private applicantDataService: ApplicantDataService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    // backend will use the role claim to return all open jobs
    this._jobSubscription = this.applicantDataService
      .getOpenAndAppliedJobs(this.authService.currentUser?.applicantId as string)
      .subscribe(jobs => {
        this.openJobs = jobs;
      });

    this._jobSubscription2 = this.applicantDataService.jobsListSubject.subscribe(jobs => {
      this.openJobs = jobs;
    });
  }

  ngOnDestroy(): void {
    if (this._jobSubscription !== null) {
      this._jobSubscription.unsubscribe();
    }
    if (this._jobSubscription2 !== null) {
      this._jobSubscription2.unsubscribe();
    }
  }

  onSelectJob(job: Job) {
    this.router.navigate(['applyToJob', `${job.uuid}`], { relativeTo: this.route });
  }
}
