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
import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { Applicant, Job } from '../models';
import { catchError, filter, first, lastValueFrom, mergeMap, of, Subscription, switchMap, takeWhile, tap } from 'rxjs';
import { MonthViewDay, EventColor } from 'calendar-utils';
import { CalendarEvent } from 'angular-calendar';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { InterviewDate, JobPageDataService } from '../services/job-page-data.service';
import { formatISO, isPast, parse, parseISO, set } from 'date-fns';
import { ChangeCommandData, JobPageChanges } from './job-page-changes';
import { JobConstants } from '../constants';
import { ToastService } from '../services/toast.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-job-page',
  templateUrl: './job-page.component.html',
  styleUrls: ['./job-page.component.scss']
})
export class JobPageComponent implements OnInit, OnDestroy {
  readonly MODE_JOB_DETAILS: number = 0;
  readonly MODE_INTERVIEW: number = 1;

  readonly _jobStatusConstants = JobConstants;

  private _subscription: Subscription | null = null;
  private _subscription2: Subscription | null = null;
  private _subscription3: Subscription | null = null;

  activeEmployerUuid = '';
  changeQueue: JobPageChanges[] = [];
  changesMadeToJob = false;
  events: CalendarEvent[] = [];
  viewDate: Date = new Date();
  mode = this.MODE_JOB_DETAILS;

  @ViewChild('confirmationModal')
  confirmationModal: any;
  // used in the modal
  chosenInterviewTimeString = '';

  @Input()
  currentJob: Job | null = null;

  @Input()
  currentApplicant: Applicant | null = null;

  constructor(private activatedRoute: ActivatedRoute, private jobPageDataService: JobPageDataService, private modalService: NgbModal, private toastService: ToastService, private authService: AuthService) { }

  ngOnInit(): void {
    this.activeEmployerUuid = this.authService.currentUser?.employerId as string;

    this._subscription3 =
      this.jobPageDataService.currentApplicantSubject.subscribe(applicant => {
        this.currentApplicant = applicant;
      });
    
    this._subscription2 =
      this.jobPageDataService.currentJobSubject.subscribe(job => {
        this.currentJob = job;
      });
    
    this._subscription = 
      this.activatedRoute.paramMap.pipe(
        switchMap((params: ParamMap) => this.jobPageDataService.getJobByUuid(params.get('id') ?? ''))
      ).subscribe(async (data) => {
        this.changesMadeToJob = false;
        this.currentApplicant = null;
        this.jobPageDataService.setCurrentJob(data);
        this.setMode(this.MODE_JOB_DETAILS);

        const interviewDates = this.jobPageDataService.getAllInterviewDatesForJob(data.uuid as string);
        this.events = interviewDates.map(idate => this._mapInterviewDateToCalendarEvent(idate));
      });
  }

  ngOnDestroy(): void {
    if (this._subscription !== null) {
      this._subscription.unsubscribe();
    }
    if (this._subscription2 !== null) {
      this._subscription2.unsubscribe();
    }
    if (this._subscription3 !== null) {
      this._subscription3.unsubscribe();
    }
  }

  onPickApplicant(applicantUuid: string) {
    this.jobPageDataService.assignCurrentApplicant(applicantUuid);
  }

  clearCurrentApplicant() {
    this.currentApplicant = null;
  }

  setMode(mode: number) {
    this.mode = mode;
  }

  onDayClicked(data:{ day: MonthViewDay<any>; sourceEvent: MouseEvent | KeyboardEvent;}) {
    const date = data.day.date;

    if (isPast(date)) {
      this.toastService.show('Please pick either the current day or a day in the future.', { classname: 'bg-warning text-dark', delay: 5000, ariaLive: 'assertive' });
      //do nothing
      return;
    }

    this.viewDate = date;
    this.confirmInterview();
  }

  async onDownloadICS() {
    this.toastService.show('Download started. If nothing happens, try disabling your pop-up blocker.', { classname: 'bg-info text-white', delay: 5000 });
    
    try {
      const interviewDate = parseISO(this.currentApplicant?.interviewDate as string);
      const icsFile = await lastValueFrom(this.jobPageDataService.downloadIcsFile(this.currentJob?.uuid as string, this.currentApplicant?.uuid as string, interviewDate));
      window.open(URL.createObjectURL(icsFile), '_blank');
    }
    catch (e) {
      if (e instanceof Error) {
        const err: Error = e;
        console.error(e?.message);
      }
      this.toastService.show('An error ocurred, or the system might be unavailable. Please try again.', { classname: 'bg-danger text-light', delay: 15000, ariaLive: 'assertive' });
    }
  }

  setCurrentJobStatus(status: string) {
    if (this.currentJob !== null) {
      this.currentJob.status = status;
      this.jobPageDataService.setCurrentJob(this.currentJob);
      this.changesMadeToJob = true;
    }
  }

  async persistChanges() {
    // There only needs to be one call to change job data
    this.changeQueue.push(
      new JobPageChanges('A', async (serviceInstance: JobPageDataService, commandData: ChangeCommandData) => { 
          const response = await lastValueFrom(serviceInstance.changeJobData(commandData));
          return response.status === 200;
        },
        this.jobPageDataService.getCurrentJob(true)
      )
    );
    
    const changes = [ ...this.changeQueue ];
    this.changeQueue = [];

    changes.sort((a,b) => {
      const tagA = a.sortTag.toUpperCase();
      const tagB = b.sortTag.toUpperCase();
      if (tagA < tagB) {
        return -1;
      }
      if (tagA > tagB) {
        return 1;
      }
      return 0;
    });
    
    const responses = await Promise.all(changes.map(c => c.invokeCommand(this.jobPageDataService)));
    if (responses[0] === true) {
      const updatedJob: Job | null = await lastValueFrom(
        this.jobPageDataService.getJobByUuid(this.currentJob?.uuid as string).pipe(
          catchError(e => of(null)),
          mergeMap((j_n: Job | null) => of(j_n).pipe(
              takeWhile(j => j !== null),
              tap(job => {
                this.toastService.show(`Successfully Updated Job: ${job?.name}`, { classname: 'bg-success text-light', delay: 10000 });
                this.jobPageDataService.jobUpdatedSubject.next();
              })
          )),
          first()
        )
      );

      if (updatedJob === null) {
        this.toastService.show('An error ocurred, or the system might be unavailable. Please try again.', { classname: 'bg-danger text-light', delay: 15000, ariaLive: 'assertive' });
        return;
      }

      this.changesMadeToJob = false;
      this.jobPageDataService.setCurrentJob(updatedJob);

      const interviewDates = this.jobPageDataService.getAllInterviewDatesForJob(updatedJob.uuid as string);
      this.events = interviewDates.map(idate => this._mapInterviewDateToCalendarEvent(idate));
    }
    else {
      this.toastService.show('An error ocurred, or the system might be unavailable. Please try again.', { classname: 'bg-danger text-light', delay: 15000, ariaLive: 'assertive' });
    }
  }

  confirmInterview() {
    this.modalService.open(this.confirmationModal, { ariaLabelledBy: 'modal-basic-title' }).result.then(async (result) => {
      if (result === 'yes') {
        const applicant = this.currentApplicant as Applicant;
        // Combine viewDate and the time from the picker for the interview date.
        const time = parse(this.chosenInterviewTimeString, 'hh:mm a', new Date());
        let interviewDate = new Date(this.viewDate);
        interviewDate = set(interviewDate, { hours: time.getHours(), minutes: time.getMinutes() });
        applicant.interviewDate = formatISO(interviewDate);

        applicant.interviewingWith = this.authService.currentUser?.employerId as string;

        this.jobPageDataService.updateApplicantDetails(applicant);
        this.changesMadeToJob = true;

        const evIndex = this.events.findIndex(e => e.id === applicant.uuid);
        if (evIndex !== -1) {
          let event = this.events[evIndex];
          event.start = new Date(this.viewDate);
          this.events[evIndex] = event;
        }
        
        this.setMode(this.MODE_JOB_DETAILS);
      }

      // always clear the time, in case its set
      this.chosenInterviewTimeString = '';
    });
  }

  cancelInterview(applicant: Applicant) {
    applicant.interviewDate = undefined;
    applicant.interviewingWith = undefined;
    this.jobPageDataService.updateApplicantDetails(applicant);
    this.changesMadeToJob = true;
    this.events = this.events.filter(e => e.id !== applicant.uuid);
  }

  rejectApplicant(applicant: Applicant) {
    applicant.rejected = true;
    this.jobPageDataService.updateApplicantDetails(applicant);
    this.changesMadeToJob = true;

    this.changeQueue = this.changeQueue.filter(chg => chg.sortTag !== `C${applicant.uuid}`);
    this.changeQueue.push(
      new JobPageChanges(`B${this.currentApplicant?.uuid}`, async (serviceInstance: JobPageDataService, commandData: ChangeCommandData) => { 
          const [job, applicant] = commandData;
          const response = await lastValueFrom(serviceInstance.rejectApplicantFromJob(job.uuid, applicant.uuid, true));
          return response.status === 200;
        },
        [this.currentJob, this.currentApplicant]
      )
    );
  }

  unrejectApplicant(applicant: Applicant) {
    applicant.rejected = false;
    this.jobPageDataService.updateApplicantDetails(applicant);
    this.changesMadeToJob = true;

    this.changeQueue = this.changeQueue.filter(chg => chg.sortTag !== `B${applicant.uuid}`);
    this.changeQueue.push(
      new JobPageChanges(`C${this.currentApplicant?.uuid}`, async (serviceInstance: JobPageDataService, commandData: ChangeCommandData) => { 
          const [job, applicant] = commandData;
          const response = await lastValueFrom(serviceInstance.rejectApplicantFromJob(job.uuid, applicant.uuid, false));
          return response.status === 200;
        },
        [this.currentJob, this.currentApplicant]
      )
    );
  }

  _hasInterview(applicant: Applicant): boolean {
    return (applicant.interviewDate !== undefined && applicant.interviewDate !== null);
  }

  private _mapInterviewDateToCalendarEvent(interviewDate: InterviewDate): CalendarEvent {
    const color: EventColor = {
      primary: '#ad2121',
      secondary: '#FAE3E3',
    };

    return {
      id: `${interviewDate.applicant?.uuid}`,
      start: (interviewDate.date as Date),
      title: `${interviewDate.applicant?.name}: Interviewing for ${this.currentJob?.name}`,
      color: { ...color }
    };
  }
}
