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

import { HttpResponse } from '@angular/common/http';
import { lastValueFrom, of } from 'rxjs';
import { Job } from '../../../models';
import { JobPageDataService } from '../../../services/job-page-data.service';
import { ChangeCommandData, JobPageChanges } from './job-page-changes';

const FakeJobPageDataService = {
  changeJobData: (job: Job) => of(new HttpResponse<any>({ status: 200, statusText: 'OK' })),
};

describe('JobPageChanges Implementations', () => {
  const service = FakeJobPageDataService as JobPageDataService;

  beforeEach(() => {
    spyOn(service, 'changeJobData').and.callThrough();
  });

  it('Runs the Change Command', done => {
    const job: Job = {};
    const command = new JobPageChanges(
      '',
      async (serviceInstance: JobPageDataService, commandData: ChangeCommandData) => {
        const res = await lastValueFrom(service.changeJobData(commandData as Job));
        return res.status === 200;
      },
      job,
    );

    const res = command.invokeCommand(service).then(rez => {
      expect(service.changeJobData).toHaveBeenCalled();
      expect(rez).toBe(true);
      done();
    });
  });

  it('Runs commands in a certain order', done => {
    const job: Job = {};
    const expectedResults = [true, true, false, false];

    let commands = [
      new JobPageChanges(
        'B',
        async (serviceInstance: JobPageDataService, commandData: ChangeCommandData) => {
          return false;
        },
        job,
      ),
      new JobPageChanges(
        'A',
        async (serviceInstance: JobPageDataService, commandData: ChangeCommandData) => {
          return true;
        },
        job,
      ),
      new JobPageChanges(
        'B',
        async (serviceInstance: JobPageDataService, commandData: ChangeCommandData) => {
          return false;
        },
        job,
      ),
      new JobPageChanges(
        'A',
        async (serviceInstance: JobPageDataService, commandData: ChangeCommandData) => {
          return true;
        },
        job,
      ),
    ];
    commands = commands.sort((a, b) => {
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

    Promise.all(commands.map(c => c.invokeCommand(service))).then(actualResults => {
      expect(actualResults).toEqual(expectedResults);
      done();
    });
  });
});
