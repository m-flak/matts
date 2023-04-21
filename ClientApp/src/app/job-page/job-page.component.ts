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
import { Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { Applicant, Job } from '../models';
import { lastValueFrom, Subscription, switchMap } from 'rxjs';
import { MonthViewDay, EventColor } from 'calendar-utils';
import { CalendarEvent } from 'angular-calendar';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { InterviewDate, JobPageDataService } from '../services/job-page-data.service';
import { formatISO } from 'date-fns';
import { ChangeCommandData, JobPageChanges } from './job-page-changes';

@Component({
  selector: 'app-job-page',
  templateUrl: './job-page.component.html',
  styleUrls: ['./job-page.component.scss']
})
export class JobPageComponent implements OnInit, OnDestroy {
  readonly MODE_JOB_DETAILS: number = 0;
  readonly MODE_INTERVIEW: number = 1;

  private _subscription: Subscription | null = null;
  private _subscription2: Subscription | null = null;
  private _subscription3: Subscription | null = null;

  changeQueue: JobPageChanges[] = [];
  changesMadeToJob = false;
  events: CalendarEvent[] = [];
  viewDate: Date = new Date();
  mode = this.MODE_JOB_DETAILS;

  @ViewChild('confirmationModal')
  confirmationModal: any;

  @Input()
  currentJob: Job | null = null;

  @Input()
  currentApplicant: Applicant | null = null;

  constructor(private activatedRoute: ActivatedRoute, private jobPageDataService: JobPageDataService, private modalService: NgbModal) { }

  ngOnInit(): void {
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

  setMode(mode: number) {
    this.mode = mode;
  }

  onDayClicked(data:{ day: MonthViewDay<any>; sourceEvent: MouseEvent | KeyboardEvent;}) {
    const date = data.day.date;
    this.viewDate = date;
    this.confirmInterview();
  }

  async persistChanges() {
    // There only needs to be one call to change job data
    this.changeQueue.push(
      new JobPageChanges('A', async (serviceInstance: JobPageDataService, commandData: ChangeCommandData) => { 
          const response = await lastValueFrom(serviceInstance.changeJobData(commandData));
          console.log(response);
          return response.status === 200;
        },
        this.jobPageDataService.getCurrentJob()
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
      this.changesMadeToJob = false;
    }
    else {
      //TODO: Handle an error
      console.error('YIKES! An error occurred!!');
    }
  }

  confirmInterview() {
    this.modalService.open(this.confirmationModal, { ariaLabelledBy: 'modal-basic-title' }).result.then(async (result) => {
      if (result === 'yes') {
        const applicant = this.currentApplicant as Applicant;
        //viewDate is the new interview date
        applicant.interviewDate = formatISO(this.viewDate);
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
    });
  }

  cancelInterview(applicant: Applicant) {
    applicant.interviewDate = undefined;
    this.jobPageDataService.updateApplicantDetails(applicant);
    this.changesMadeToJob = true;
    this.events = this.events.filter(e => e.id !== applicant.uuid);
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
