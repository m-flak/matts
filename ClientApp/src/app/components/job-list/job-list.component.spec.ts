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
import {HarnessLoader} from '@angular/cdk/testing';
import {TestbedHarnessEnvironment} from '@angular/cdk/testing/testbed';
import {MatListModule} from '@angular/material/list';
import {MatListHarness}  from '@angular/material/list/testing';

import { JobListComponent } from './job-list.component';
import { JobConstants } from 'src/app/constants';

const jobs = [
  {
      id: 1,
      name: 'Tester',
      status: JobConstants.STATUS_OPEN,
      applicants: [],
      applicantCount: 0
  }
];

describe('JobListComponent', () => {
  let component: JobListComponent;
  let fixture: ComponentFixture<JobListComponent>;

  let loader: HarnessLoader;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ MatListModule ],
      declarations: [ JobListComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobListComponent);
    loader = TestbedHarnessEnvironment.loader(fixture);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Population', () => {
    afterEach(() => {
      component.jobs = [];
      fixture.detectChanges();
    });

    it('should display the list with the jobs', async () => {
      component.jobs = jobs;
      fixture.detectChanges();

      await fixture.whenStable();
      const list = await loader.getHarness(MatListHarness);
      const listItems = await list.getItems();

      expect(listItems.length).toEqual(1);
      
      const allText = await listItems[0].getText();
      expect(allText.includes(jobs[0].name)).toBe(true);
      expect(allText.includes(jobs[0].status)).toBe(true);
      expect(allText.includes(`${jobs[0].applicants.length}`)).toBe(true);
    });
  });
});
