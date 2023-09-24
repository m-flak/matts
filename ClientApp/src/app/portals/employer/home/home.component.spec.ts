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
import { HomeComponent } from './home.component';
import { BackendService } from '../../../services/backend.service';
import { of } from 'rxjs';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientModule } from '@angular/common/http';
import { JobConstants } from '../../../constants';
import { ActivatedRoute } from '@angular/router';
import { EmployerPortalModule } from '../employer-portal.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

const jobs = [
  {
    id: 1,
    name: 'Tester',
    status: JobConstants.STATUS_OPEN,
    applicants: [],
  },
];

const FakeBackendService = {
  getAllJobs: () => of(jobs),
};

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let service: BackendService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        BrowserAnimationsModule,
        HttpClientModule,
        HttpClientTestingModule,
        EmployerPortalModule,
        RouterTestingModule.withRoutes([]),
      ],
      declarations: [HomeComponent],
      providers: [
        //{ provide: 'BASE_URL', useValue: '' },
        { provide: BackendService, useValue: FakeBackendService },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { data: { jobList: jobs } } },
        },
      ],
    }).compileComponents();

    service = TestBed.inject(BackendService);

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});