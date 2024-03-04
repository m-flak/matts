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
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MaterialFileInputModule } from 'ngx-material-file-input';
import { ComponentsModule } from 'src/app/components/components.module';
import { ApplyJobPageComponent } from './apply-job-page/apply-job-page.component';
import { HomeApplicantComponent } from './home-applicant/home-applicant.component';
import { ApplicantPortalRouteModule } from './applicant-portal-route.module';
import { CommonModule } from '@angular/common';

@NgModule({
  declarations: [ApplyJobPageComponent, HomeApplicantComponent],
  imports: [
    CommonModule,
    MatButtonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatIconModule,
    MaterialFileInputModule,
    HttpClientModule,
    MatCardModule,
    // Our modules start below
    ApplicantPortalRouteModule,
    ComponentsModule,
  ],
  exports: [ApplyJobPageComponent, HomeApplicantComponent],
})
export class ApplicantPortalModule {}
