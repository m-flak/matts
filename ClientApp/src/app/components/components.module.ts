import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApplicantsPickerComponent } from './applicants-picker/applicants-picker.component';
import { ApplicantComponent } from './applicants-picker/applicant/applicant.component';



@NgModule({
  declarations: [
    ApplicantsPickerComponent,
    ApplicantComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    ApplicantsPickerComponent
  ]
})
export class ComponentsModule { }
