import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {MatListModule} from '@angular/material/list';
import { ApplicantsPickerComponent } from './applicants-picker/applicants-picker.component';
import { ApplicantComponent } from './applicants-picker/applicant/applicant.component';
import { JobListComponent } from './job-list/job-list.component';



@NgModule({
  declarations: [
    ApplicantsPickerComponent,
    ApplicantComponent,
    JobListComponent
  ],
  imports: [
    CommonModule,
    MatListModule
  ],
  exports: [
    ApplicantsPickerComponent,
    JobListComponent
  ]
})
export class ComponentsModule { }
