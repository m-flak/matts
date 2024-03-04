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

import { RouterModule, Routes } from '@angular/router';
import { UserRoleConstants } from 'src/app/constants';
import { JobListComponent, JobListComponentResolver } from './job-list/job-list.component';
import { JobPageComponent } from './job-page/job-page.component';
import { NgModule } from '@angular/core';
import { AuthGuard } from 'src/app/guards/auth.guard';
import { NewJobPageComponent } from './new-job-page/new-job-page.component';

export const EMPLOYER_JOBS_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'list',
    pathMatch: 'full',
  },
  {
    path: 'list',
    component: JobListComponent,
    resolve: { jobList: JobListComponentResolver },
    data: { role: UserRoleConstants.USER_ROLE_EMPLOYER },
    canActivate: [AuthGuard],
    children: [
      {
        path: 'viewJob/:id',
        component: JobPageComponent,
      },
    ],
  },
  {
    path: 'postNew',
    component: NewJobPageComponent,
    data: { role: UserRoleConstants.USER_ROLE_EMPLOYER },
    canActivate: [AuthGuard],
    children: [],
  },
];

@NgModule({
  imports: [RouterModule.forChild(EMPLOYER_JOBS_ROUTES)],
  exports: [RouterModule],
})
export class EmployerJobsRouteModule {}
