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
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomeApplicantComponent } from './home-applicant.component';
import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { BackendService } from '../../../services/backend.service';
import { JWT_OPTIONS, JwtHelperService } from '@auth0/angular-jwt';
import { jwtOptionsFactory } from '../../../app.module';
import { Job } from '../../../models';
import { ApplicantPortalModule } from '../applicant-portal.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

const allJobs: Job[] = [
  {
    'id': 1,
    'uuid': '4b4d7c64-ef5d-4379-add3-a3f6adc42f01',
    'name': 'Full Stack Software Developer',
    'status': 'OPEN',
    'description':
      'John Doe Corporation is looking for a talented Full Stack Software Developer professional to work in a fast-paced, exciting environment!',
  },
  {
    'id': 2,
    'uuid': 'eab3e2e8-f5a1-41c1-aa1d-1ad7eb6f3a96',
    'name': 'Junior HR',
    'status': 'OPEN',
    'description':
      'John Doe Corporation is looking for a talented Junior HR professional to work in a fast-paced, exciting environment!',
  },
];
const appliedJobs: Job[] = [
  {
    'id': 1,
    'uuid': '4b4d7c64-ef5d-4379-add3-a3f6adc42f01',
    'name': 'Full Stack Software Developer',
    'status': 'OPEN',
    'description':
      'John Doe Corporation is looking for a talented Full Stack Software Developer professional to work in a fast-paced, exciting environment!',
  },
];

const FakeBackendService = {
  getAllAppliedJobs: (applicantId: string) => of(appliedJobs),
  getAllJobs: () => of(allJobs),
};

describe('HomeApplicantComponent', () => {
  let component: HomeApplicantComponent;
  let fixture: ComponentFixture<HomeApplicantComponent>;
  let service: BackendService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        BrowserAnimationsModule,
        HttpClientModule,
        HttpClientTestingModule,
        ApplicantPortalModule,
        RouterTestingModule.withRoutes([]),
      ],
      declarations: [HomeApplicantComponent],
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
        {
          provide: JWT_OPTIONS,
          useValue: jwtOptionsFactory('https://localhost/'),
        },
        JwtHelperService,
      ],
    }).compileComponents();

    service = TestBed.inject(BackendService);

    fixture = TestBed.createComponent(HomeApplicantComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
