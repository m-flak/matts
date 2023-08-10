import { Component, Injectable, OnDestroy, OnInit } from '@angular/core';
import { BackendService } from '../services/backend.service';
import { Job } from '../models';
import { catchError, Observable, of, Subscription, throwError } from 'rxjs';
import { ActivatedRoute, ActivatedRouteSnapshot, NavigationBehaviorOptions, Resolve, Router, RouterStateSnapshot, UrlSegment } from '@angular/router';
import { ToastService } from '../services/toast.service';
import { JobPageDataService } from '../services/job-page-data.service';

@Injectable({
  providedIn: 'root'
})
export class HomeComponentResolver implements Resolve<Observable<Job[]>> {
  constructor(private backendService: BackendService, private toastService: ToastService) {}

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Job[]> {
    return this.backendService.getAllJobs().pipe(
      catchError((e: Error) => {
        console.error(e?.message);
        this.toastService.show('An error ocurred, or the system might be unavailable. Please try again.', { classname: 'bg-danger text-light', delay: 15000, ariaLive: 'assertive' });
        return of([] as Job[]);
      })
    )
  }
}

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  readonly ITEM_JOBLIST = 'joblist';
  readonly ITEM_POSTNEW = 'postnew';

  private _jobSubscription: Subscription | null = null;
  private _jobSubscription2: Subscription | null = null;

  jobs: Job[] = [];

  currentSelectedToolbarItem = this.ITEM_JOBLIST;

  constructor(private backendService: BackendService, private router: Router, private route: ActivatedRoute, private jobPageDataService: JobPageDataService) {
  }

  ngOnDestroy(): void {
    if (this._jobSubscription !== null) {
      this._jobSubscription.unsubscribe();
    }
    if (this._jobSubscription2 !== null) {
      this._jobSubscription2.unsubscribe();
    }
  }

  ngOnInit(): void {
    this.jobs = [ ...this.route.snapshot.data.jobList ];
    this._jobSubscription2 = this.jobPageDataService.jobUpdatedSubject.subscribe(() => {
      this.onJobUpdated();
    });
  }

  onSelectJob(job: Job) {
    this.router.navigate(['viewJob', `${job.uuid}`], { relativeTo: this.route });
  }

  onJobCreated(): void {
    if (this.router.url.includes('/viewJob/')) {
      this.router.navigateByUrl('/', { skipLocationChange: true })
        .then(() => this.router.navigate(['employer'], { relativeTo: this.route.root, }));
    }
    else {
      this.currentSelectedToolbarItem = this.ITEM_JOBLIST;
    }
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
