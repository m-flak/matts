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

@Component({
  selector: 'app-job-page',
  templateUrl: './job-page.component.html',
  styleUrls: ['./job-page.component.scss']
})
export class JobPageComponent implements OnInit, OnDestroy {
  readonly MODE_JOB_DETAILS: number = 0;
  readonly MODE_INTERVIEW: number = 1;

  private _subscription: Subscription | null = null;

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
    this._subscription = 
      this.activatedRoute.paramMap.pipe(
        switchMap((params: ParamMap) => this.jobPageDataService.getJobByUuid(params.get('id') ?? ''))
      ).subscribe(async (data) => {
        this.currentJob = data;
        this.setMode(this.MODE_JOB_DETAILS);

        const interviewDates = await lastValueFrom(this.jobPageDataService.getAllInterviewDatesForJob((data.uuid as string)));
        this.events = interviewDates.map(idate => this._mapInterviewDateToCalendarEvent(idate));
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
        const response = await lastValueFrom(this.jobPageDataService.changeJobData(this.currentJob as Job));
        console.log(response);
      }
    });
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
      start: (interviewDate.date as Date),
      title: `${interviewDate.applicant?.name}: Interviewing for ${this.currentJob?.name}`,
      color: { ...color }
    };
  }
}
