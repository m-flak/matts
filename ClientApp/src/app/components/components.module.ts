import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatListModule } from '@angular/material/list';
import { ApplicantsPickerComponent } from './applicants-picker/applicants-picker.component';
import { ApplicantComponent } from './applicants-picker/applicant/applicant.component';
import { JobListComponent } from './job-list/job-list.component';
import { HttpClientModule } from '@angular/common/http';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { NgbToastModule } from '@ng-bootstrap/ng-bootstrap';
import { MatButtonModule } from '@angular/material/button';
import { EmployerToolbarComponent } from '../components/employer-toolbar/employer-toolbar.component';
import { ToastsContainerComponent } from './toasts-container/toasts-container.component';
import { MatIconLinkDirective } from './mat-icon-link/mat-icon-link.directive';
import { MonitoredMatButtonDirective } from './monitored-component/monitored-mat-button.directive';
import { BrandingContainerComponent } from '../components/branding-container/branding-container.component';
import { BrandingNoneDefaultDirective, BrandingWithBrandDirective } from './branding-container/branding.directives';

@NgModule({
  declarations: [
    ApplicantsPickerComponent,
    ApplicantComponent,
    JobListComponent,
    EmployerToolbarComponent,
    ToastsContainerComponent,
    MatIconLinkDirective,
    MonitoredMatButtonDirective,
    BrandingContainerComponent,
    BrandingWithBrandDirective,
    BrandingNoneDefaultDirective,
  ],
  imports: [CommonModule, MatListModule, HttpClientModule, MatIconModule, NgbToastModule, MatButtonModule],
  exports: [
    ApplicantsPickerComponent,
    JobListComponent,
    EmployerToolbarComponent,
    ToastsContainerComponent,
    MatIconLinkDirective,
    MonitoredMatButtonDirective,
    BrandingContainerComponent,
    BrandingWithBrandDirective,
    BrandingNoneDefaultDirective,
  ],
})
export class ComponentsModule {
  constructor(iconRegistry: MatIconRegistry, sanitizer: DomSanitizer) {
    iconRegistry.addSvgIcon(
      'briefcase_list',
      sanitizer.bypassSecurityTrustResourceUrl('../../assets/briefcase-list.svg'),
    );
    iconRegistry.addSvgIcon(
      'briefcase_new',
      sanitizer.bypassSecurityTrustResourceUrl('../../assets/briefcase-new.svg'),
    );
  }
}
