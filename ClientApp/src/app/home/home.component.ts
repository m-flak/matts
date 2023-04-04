import { Component, OnDestroy, OnInit } from '@angular/core';
import { BackendService } from '../services/backend.service';
import { Job } from '../models';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { JobPageRouteData } from '../job-page/job-page.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  private _jobSubscription: Subscription | null = null;

  jobs: Job[] = [];

  constructor(private backendService: BackendService, private router: Router) {
  }

  ngOnDestroy(): void {
    if (this._jobSubscription !== null) {
      this._jobSubscription.unsubscribe();
    }
  }

  ngOnInit(): void {
    this._jobSubscription = this.backendService.getAllJobs().subscribe(theJobs => {
      this.jobs = theJobs;
    });
  }

  onSelectJob(job: Job) {
    console.error(job);
    this.router.navigate(['viewJob', `${job.id}`], { state: ({ currentJob: job } as JobPageRouteData ) });
  }
}
