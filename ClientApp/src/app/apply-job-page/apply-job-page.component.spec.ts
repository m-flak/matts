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
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';

import { ApplyJobPageComponent } from './apply-job-page.component';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { from, of } from 'rxjs';
import { Job, configurationFixure } from '../models';
import { BackendService } from '../services/backend.service';
import { AuthService, CurrentUser } from '../services/auth.service';
import { UserRoleConstants } from '../constants';
import { MatButtonModule } from '@angular/material/button';
import { ApplicantDataService } from '../services/applicant-data.service';
import { ComponentHarness, HarnessLoader, HarnessPredicate, TestElement } from '@angular/cdk/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { MatButtonHarness } from '@angular/material/button/testing';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FileInput, MaterialFileInputModule } from 'ngx-material-file-input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ConfigService } from '../services/config.service';
import {
  HttpClientModule,
  HttpEventType,
  HttpResponse,
  HttpSentEvent,
  HttpUploadProgressEvent,
} from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';

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

const progress = (l: number, t: number) =>
  ({ type: HttpEventType.UploadProgress, loaded: l, total: t }) as HttpUploadProgressEvent;
const sent = () => ({ type: HttpEventType.Sent }) as HttpSentEvent;

const FakeBackendService = {
  getJobDetails: (id: string) => of(jobData),
  uploadResume: (data: FormData) => from([sent(), progress(1, 2), progress(2, 2), new HttpResponse<any>()]),
};

const FakeAuthService = {
  currentUser: {
    userName: 'john',
    password: '',
    role: UserRoleConstants.USER_ROLE_APPLICANT,
    applicantId: 'db185379-70e8-4ec6-b5f7-370415ca3b43',
  } as CurrentUser,
};

const FakeConfigService = {
  config: {
    ...configurationFixure,
    resumeUploader: {
      maxFileSize: 1048576,
      allowedFileTypes: 'text/plain',
      allowedFileExtensions: '*.txt',
    },
  },
  loadConfig: () => configurationFixure,
};

describe('ApplyJobPageComponent', () => {
  let component: ApplyJobPageComponent;
  let fixture: ComponentFixture<ApplyJobPageComponent>;
  let applicantDataService: ApplicantDataService;

  let loader: HarnessLoader;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        BrowserAnimationsModule,
        MatButtonModule,
        FormsModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatIconModule,
        MaterialFileInputModule,
        HttpClientModule,
        HttpClientTestingModule,
      ],
      declarations: [ApplyJobPageComponent],
      providers: [
        FormBuilder,
        { provide: AuthService, useValue: FakeAuthService },
        { provide: BackendService, useValue: FakeBackendService },
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
        ApplicantDataService,
        { provide: ConfigService, useValue: FakeConfigService },
      ],
    }).compileComponents();

    applicantDataService = TestBed.inject(ApplicantDataService);

    fixture = TestBed.createComponent(ApplyJobPageComponent);
    loader = TestbedHarnessEnvironment.loader(fixture);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load requirements for the uploader', () => {
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.maxFileSize).toEqual(FakeConfigService.config.resumeUploader.maxFileSize);
    expect(component.allowedFileTypes).toEqual(FakeConfigService.config.resumeUploader.allowedFileTypes);
    expect(component.allowedFileExtensions).toEqual(FakeConfigService.config.resumeUploader.allowedFileExtensions);
  });

  it('should disallow an application if the job is applied for', fakeAsync(async () => {
    spyOn(applicantDataService, 'isJobAppliedFor').and.returnValue(true);
    component.ngOnInit();
    tick(2);
    fixture.detectChanges();

    await fixture.whenStable();

    const compiled = fixture.debugElement;
    const applyForText = compiled.nativeElement.querySelector('p.have-not-applied');
    const alreadApplyText = compiled.nativeElement.querySelector('p.have-applied');

    const button = await loader.getHarness(MatButtonHarness);
    const buttonIsDisabled = await button.isDisabled();

    expect(applicantDataService.isJobAppliedFor).toHaveBeenCalled();
    expect(applyForText).toBeNull();
    expect(alreadApplyText).not.toBeNull();
    expect(buttonIsDisabled).toBe(true);
  }));

  it('should take files in the form', fakeAsync(async () => {
    spyOn(applicantDataService, 'isJobAppliedFor').and.returnValue(false);
    component.ngOnInit();
    tick(2);
    fixture.detectChanges();

    await fixture.whenStable();
    const file = new File([''], 'resume.docx');
    component.applyToJobForm.setValue({
      resumeFile: new FileInput([file]),
    });
    fixture.detectChanges();

    const fileInputData: FileInput = component.applyToJobForm.controls.resumeFile.value;
    expect(fileInputData.files.length).toEqual(1);
    expect(fileInputData.fileNames.split(', ').length).toEqual(1);
  }));

  it('should upload the files from the form', fakeAsync(async () => {
    spyOn(applicantDataService, 'isJobAppliedFor').and.returnValue(false);
    spyOn(applicantDataService, 'applyToJob').and.returnValue(of(new HttpResponse<any>({ status: 200 })));
    spyOn(applicantDataService, 'getOpenAndAppliedJobs').and.returnValue(of([jobData]));
    component.ngOnInit();
    tick(2);
    fixture.detectChanges();

    await fixture.whenStable();
    const file = new File([''], 'resume.docx');
    component.applyToJobForm.setValue({
      resumeFile: new FileInput([file]),
    });
    fixture.detectChanges();

    const fileInputData: FileInput = component.applyToJobForm.controls.resumeFile.value;
    expect(fileInputData.files.length).toEqual(1);
    expect(fileInputData.fileNames.split(', ').length).toEqual(1);

    const button = await loader.getHarness(MatButtonHarness);
    await button.click();
    tick(6);
    fixture.detectChanges();

    await fixture.whenStable();
    expect(applicantDataService.applyToJob).toHaveBeenCalledTimes(1);
    expect(applicantDataService.getOpenAndAppliedJobs).toHaveBeenCalledTimes(1);
  }));
});
