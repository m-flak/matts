import { Component, OnDestroy, OnInit } from '@angular/core';
import { BackendService } from '../services/backend.service';
import { Job } from '../models';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  private _jobSubscription: Subscription | null = null;

  jobs: Job[] = [];

  constructor(private backendService: BackendService) {
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
}
