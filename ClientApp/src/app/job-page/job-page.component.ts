import { Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { Applicant, Job } from '../models';
import { lastValueFrom, Subscription, switchMap } from 'rxjs';
import { BackendService } from '../services/backend.service';
import { MonthViewDay } from 'calendar-utils';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-job-page',
  templateUrl: './job-page.component.html',
  styleUrls: ['./job-page.component.scss']
})
export class JobPageComponent implements OnInit, OnDestroy {
  readonly MODE_JOB_DETAILS: number = 0;
  readonly MODE_INTERVIEW: number = 1;

  private _subscription: Subscription | null = null;

  viewDate: Date = new Date();
  mode = this.MODE_JOB_DETAILS;

  @ViewChild('confirmationModal')
  confirmationModal: any;

  @Input()
  currentJob: Job | null = null;

  @Input()
  currentApplicant: Applicant | null = null;

  constructor(private activatedRoute: ActivatedRoute, private backendService: BackendService, private modalService: NgbModal) { }

  ngOnInit(): void {
    this._subscription = 
      this.activatedRoute.paramMap.pipe(
        switchMap((params: ParamMap) => this.backendService.getJobDetails(params.get('id') ?? ''))
      ).subscribe(data => {
        this.currentJob = data;
        this.setMode(this.MODE_JOB_DETAILS);
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

  setMode(mode: number) {
    this.mode = mode;
  }

  onDayClicked(data:{ day: MonthViewDay<any>; sourceEvent: MouseEvent | KeyboardEvent;}) {
    const date = data.day.date;
    this.viewDate = date;
    this.confirmInterview();
  }

  confirmInterview() {
    this.modalService.open(this.confirmationModal, { ariaLabelledBy: 'modal-basic-title' }).result.then(async (result) => {
      if (result === 'yes') {
        //TODO: Update date
        const response = await lastValueFrom(this.backendService.updateJob(this.currentJob as Job));
        console.log(response);
      }
    });
  }
}
