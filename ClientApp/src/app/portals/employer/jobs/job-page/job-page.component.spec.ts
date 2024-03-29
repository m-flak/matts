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
import { ComponentFixture, TestBed, fakeAsync, tick, waitForAsync } from '@angular/core/testing';
import { compareAsc, parseISO } from 'date-fns';
import { JobPageComponent } from './job-page.component';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { of } from 'rxjs';
import { BackendService } from '../../../../services/backend.service';
import { HttpResponse } from '@angular/common/http';
import { Applicant, Job } from '../../../../models';
import { JobPageDataService } from '../../../../services/job-page-data.service';
import { HarnessLoader } from '@angular/cdk/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { ApplicantsPickerHarness } from '../../../../components/applicants-picker/applicants-picker.harness';
import { JWT_OPTIONS, JwtHelperService } from '@auth0/angular-jwt';
import { jwtOptionsFactory } from '../../../../app.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { EmployerPortalModule } from '../../employer-portal.module';
import { EmployerJobsModule } from '../employer-jobs.module';

const jobData: Job = {
  'id': 1,
  'uuid': '54991ebe-ba9e-440b-a202-247f0c33574f',
  'name': 'Full Stack Software Developer',
  'status': 'OPEN',
  'applicants': [
    {
      'id': 1,
      'uuid': 'db185379-70e8-4ec6-b5f7-370415ca3b43',
      'name': 'John Doe',
      'applicantPhoto': null,
      'interviewDate': '2023-04-11T16:10:30.0273024Z',
    },
    {
      'id': 2,
      'uuid': 'cb9057c9-b504-4add-a5b8-0113ef08e9e4',
      'name': 'Jane Doe',
      'applicantPhoto': null,
      'interviewDate': '2023-04-11T16:10:30.0273449Z',
    },
    {
      'id': 3,
      'uuid': 'ef4e92bc-8027-4fd2-9a13-450a1cfb8697',
      'name': 'John Public',
      'applicantPhoto': null,
      'interviewDate': '2023-04-11T16:10:30.0273453Z',
    },
    {
      'id': 4,
      'uuid': '3b785136-cafb-48ed-b58f-1b2150f74bf6',
      'name': 'Lee Cardholder',
      'applicantPhoto': null,
      'interviewDate': '2023-04-11T16:10:30.0273464Z',
    },
  ],
};

const FakeBackendService = {
  getJobDetails: (id: string) => of(jobData),
  updateJob: (job: Job) => of(new HttpResponse<any>()),
};

describe('JobPageComponent', () => {
  let component: JobPageComponent;
  let fixture: ComponentFixture<JobPageComponent>;
  let pageService: JobPageDataService;
  let loader: HarnessLoader;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BrowserAnimationsModule, EmployerPortalModule, EmployerJobsModule],
      declarations: [JobPageComponent],
      providers: [
        {
          provide: 'BASE_URL',
          useValue: 'https://localhost/',
        },
        {
          provide: 'WS_BASE_URL',
          useValue: 'ws://localhost/',
        },
        { provide: BackendService, useValue: FakeBackendService },
        JobPageDataService,
        {
          provide: ActivatedRoute,
          useValue: {
            'paramMap': of(
              (() => {
                let m = new Map();
                m.set('id', '54991ebe-ba9e-440b-a202-247f0c33574f');
                return m as unknown as ParamMap;
              })(),
            ),
          },
        },
        {
          provide: JWT_OPTIONS,
          useValue: jwtOptionsFactory('https://localhost/'),
        },
        JwtHelperService,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(JobPageComponent);
    pageService = TestBed.inject(JobPageDataService);
    loader = TestbedHarnessEnvironment.loader(fixture);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load the job details', fakeAsync(() => {
    tick(2);
    expect(component.currentJob).not.toBeNull();
    expect(component.currentJob?.id).toEqual(jobData.id);
    expect(component.currentJob?.uuid).toEqual(jobData.uuid);
    expect(component.currentJob?.name).toEqual(jobData.name);
    expect(component.currentJob?.status).toEqual(jobData.status);
  }));

  it("should update the calendar events array with the candidates' interview dates", fakeAsync(() => {
    tick(3);
    fixture.detectChanges();

    expect(component.events.length).toEqual(jobData.applicants?.length as number);

    const zip = (a: any, b: any) => a.map((k: any, i: any) => [k, b[i]]);
    zip(jobData.applicants, component.events).forEach(([expected, event]: any[]) => {
      const expectedDate = parseISO(expected.interviewDate);
      const actualDate = event.start;

      expect(compareAsc(expectedDate, actualDate)).toEqual(0);
    });
  }));

  it('should have the applicants in the picker', waitForAsync(() => {
    fixture.detectChanges();

    fixture.whenStable().then(async () => {
      const applicantsPicker = await loader.getHarness(ApplicantsPickerHarness);
      const applicants = await applicantsPicker.getApplicants();
      expect(applicants.length).toEqual(jobData.applicants?.length as number);
    });
  }));

  it('should be able to reject applicants and display the rejection', waitForAsync(() => {
    fixture.detectChanges();

    fixture
      .whenStable()
      .then(() => component.rejectApplicant((jobData.applicants as Applicant[])[0]))
      .then(async () => {
        fixture.detectChanges();
        const applicantsPicker = await loader.getHarness(ApplicantsPickerHarness);
        const rejectApplicants = await applicantsPicker.getApplicants({ isRejected: true });
        expect(rejectApplicants.length).toEqual(1);
      });
  }));

  it('should be able to reject applicants and then unreject applicants', waitForAsync(() => {
    fixture.detectChanges();

    fixture
      .whenStable()
      .then(() => component.rejectApplicant((jobData.applicants as Applicant[])[0]))
      .then(async () => {
        fixture.detectChanges();
        const applicantsPicker = await loader.getHarness(ApplicantsPickerHarness);
        const rejectApplicants = await applicantsPicker.getApplicants({ isRejected: true });
        expect(rejectApplicants.length).toEqual(1);
      })
      .then(() => component.unrejectApplicant((jobData.applicants as Applicant[])[0]))
      .then(async () => {
        fixture.detectChanges();
        const applicantsPicker = await loader.getHarness(ApplicantsPickerHarness);
        const rejectApplicants = await applicantsPicker.getApplicants({ isRejected: true });
        expect(rejectApplicants.length).toEqual(0);
      });
  }));

  it('should call the persistChanges calls in the correct order', waitForAsync(() => {
    spyOn(pageService, 'changeJobData').and.callFake(job => of(new HttpResponse<any>()));
    spyOn(pageService, 'rejectApplicantFromJob').and.callFake((job, app) => of(new HttpResponse<any>()));

    fixture.detectChanges();

    fixture
      .whenStable()
      .then(() => component.rejectApplicant((jobData.applicants as Applicant[])[0]))
      .then(async () => {
        fixture.detectChanges();
        const wait4me = await component.persistChanges();
        expect(pageService.changeJobData).toHaveBeenCalledBefore(pageService.rejectApplicantFromJob);
      });
  }));

  it('should be able to download an ICS for an Applicant with an interview', waitForAsync(() => {
    spyOn(pageService, 'downloadIcsFile').and.callFake((job, app, date) =>
      of(new Blob(['dummy data'], { type: 'text/calendar' })),
    );
    spyOn(window, 'open').and.stub();

    fixture.detectChanges();

    fixture
      .whenStable()
      .then(() => component.onDownloadICS())
      .then(() => {
        fixture.detectChanges();
        expect(pageService.downloadIcsFile).toHaveBeenCalled();
        expect(window.open).toHaveBeenCalled();
      });
  }));
});
