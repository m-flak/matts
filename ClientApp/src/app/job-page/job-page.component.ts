import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { Applicant, Job } from '../models';
import { Subscription, switchMap } from 'rxjs';
import { BackendService } from '../services/backend.service';

@Component({
  selector: 'app-job-page',
  templateUrl: './job-page.component.html',
  styleUrls: ['./job-page.component.scss']
})
export class JobPageComponent implements OnInit, OnDestroy {
  private _subscription: Subscription | null = null;

  @Input()
  currentJob: Job | null = null;

  @Input()
  currentApplicant: Applicant | null = null;

  constructor(private activatedRoute: ActivatedRoute, private backendService: BackendService) { }

  ngOnInit(): void {
    this._subscription = 
      this.activatedRoute.paramMap.pipe(
        switchMap((params: ParamMap) => this.backendService.getJobDetails(Number(params.get('id'))))
      ).subscribe(data => {
        this.currentJob = data;
      });
  }

  ngOnDestroy(): void {
    if (this._subscription !== null) {
      this._subscription.unsubscribe();
    }
  }

  onPickApplicant(applicant: Applicant) {
    this.currentApplicant = applicant;
  }
}
