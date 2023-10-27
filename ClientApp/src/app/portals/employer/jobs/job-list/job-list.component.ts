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
import { Component, Injectable, OnDestroy, OnInit } from '@angular/core';
import { BackendService } from '../../../../services/backend.service';
import { Job } from '../../../../models';
import { catchError, Observable, of, Subscription, throwError } from 'rxjs';
import {
  ActivatedRoute,
  ActivatedRouteSnapshot,
  NavigationBehaviorOptions,
  Resolve,
  Router,
  RouterStateSnapshot,
  UrlSegment,
} from '@angular/router';
import { ToastService } from '../../../../services/toast.service';
import { JobPageDataService } from '../../../../services/job-page-data.service';

@Injectable({
  providedIn: 'root',
})
export class JobListComponentResolver implements Resolve<Observable<Job[]>> {
  constructor(
    private backendService: BackendService,
    private toastService: ToastService,
  ) {}

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Job[]> {
    return this.backendService.getAllJobs().pipe(
      catchError((e: Error) => {
        console.error(e?.message);
        this.toastService.show('An error ocurred, or the system might be unavailable. Please try again.', {
          classname: 'bg-danger text-light',
          delay: 15000,
          ariaLive: 'assertive',
        });
        return of([] as Job[]);
      }),
    );
  }
}

@Component({
  selector: 'app-jobs-joblist',
  templateUrl: './job-list.component.html',
  styleUrls: ['./job-list.component.scss'],
})
export class JobListComponent implements OnInit, OnDestroy {
  private _jobSubscription: Subscription | null = null;
  private _jobSubscription2: Subscription | null = null;

  jobs: Job[] = [];

  showDetailCardMsg = true;

  initialActivate = (_: any) => (this.showDetailCardMsg = false);
  // when clicking the home link, reshow the msg
  deactivate = (_: any) => (this.showDetailCardMsg = true);

  constructor(
    private backendService: BackendService,
    private router: Router,
    private route: ActivatedRoute,
    private jobPageDataService: JobPageDataService,
  ) {}

  ngOnDestroy(): void {
    if (this._jobSubscription !== null) {
      this._jobSubscription.unsubscribe();
    }
    if (this._jobSubscription2 !== null) {
      this._jobSubscription2.unsubscribe();
    }
  }

  ngOnInit(): void {
    this.jobs = [...this.route.snapshot.data.jobList];
    this._jobSubscription2 = this.jobPageDataService.jobUpdatedSubject.subscribe(() => {
      this.onJobUpdated();
    });
  }

  onSelectJob(job: Job) {
    this.router.navigate(['viewJob', `${job.uuid}`], { relativeTo: this.route });
  }

  onJobUpdated(): void {
    if (this._jobSubscription !== null && !this._jobSubscription?.closed) {
      this._jobSubscription.unsubscribe();
    }

    this._jobSubscription = this.backendService.getAllJobs().subscribe(theJobs => {
      this.jobs = theJobs;
    });
  }
}
