import { Component, OnDestroy, OnInit } from '@angular/core';
import { BackendService } from '../services/backend.service';
import { Job } from '../models';
import { Subscription } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  readonly ITEM_JOBLIST = 'joblist';
  readonly ITEM_POSTNEW = 'postnew';

  private _jobSubscription: Subscription | null = null;

  jobs: Job[] = [];

  currentSelectedToolbarItem = this.ITEM_JOBLIST;

  constructor(private backendService: BackendService, private router: Router, private route: ActivatedRoute) {
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
    this.router.navigate(['viewJob', `${job.uuid}`], { relativeTo: this.route });
  }

  toolbarButtonClicked(buttonId: string) {
    console.log(`${buttonId} clicked!`);
  }
}
