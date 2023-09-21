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
import { NgModule } from '@angular/core';
import { ApplyJobPageComponent } from './apply-job-page/apply-job-page.component';
import { HomeApplicantComponent } from './home-applicant/home-applicant.component';
import { AuthGuard } from 'src/app/guards/auth.guard';
import { HomeGuard } from 'src/app/guards/home.guard';

export const APPLICANT_ROUTES: Routes = [
  {
    path: '',
    component: HomeApplicantComponent,
    data: { role: UserRoleConstants.USER_ROLE_APPLICANT },
    canActivate: [AuthGuard, HomeGuard],
    children: [
      {
        path: 'applyToJob/:id',
        component: ApplyJobPageComponent,
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(APPLICANT_ROUTES)],
  exports: [RouterModule],
})
export class ApplicantPortalRouteModule {}
